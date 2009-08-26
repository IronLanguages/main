#!/usr/local/bin/ruby -w

raise LoadError, "ParseTree doesn't work with ruby #{RUBY_VERSION}" if
  RUBY_VERSION >= "1.9"
raise LoadError, "ParseTree isn't needed with rubinius" if
  defined? RUBY_ENGINE and RUBY_ENGINE == "rbx"

require 'rubygems'
require 'inline'
require 'unified_ruby'

class Module
  def modules
    ancestors[1..-1]
  end
end

class Class
  def modules
    a = self.ancestors
    a[1..a.index(superclass)-1]
  end
end

##
# ParseTree is a RubyInline-style extension that accesses and
# traverses the internal parse tree created by ruby.
#
#   class Example
#     def blah
#       return 1 + 1
#     end
#   end
#
#   ParseTree.new.parse_tree(Example)
#   => [[:class, :Example, :Object,
#          [:defn,
#            "blah",
#            [:scope,
#              [:block,
#                [:args],
#                [:return, [:call, [:lit, 1], "+", [:array, [:lit, 1]]]]]]]]]

class RawParseTree

  VERSION = '3.0.4'

  ##
  # Front end translation method.

  def self.translate(klass_or_str, method=nil)
    pt = self.new(false)
    case klass_or_str
    when String then
      sexp = pt.parse_tree_for_string(klass_or_str).first
      if method then
        # class, scope, block, *methods
        sexp.last.last[1..-1].find do |defn|
          defn[1] == method
        end
      else
        sexp
      end
    else
      unless method.nil? then
        if method.to_s =~ /^self\./ then
          method = method.to_s[5..-1].intern
          pt.parse_tree_for_method(klass_or_str, method, true)
        else
          pt.parse_tree_for_method(klass_or_str, method)
        end
      else
        pt.parse_tree(klass_or_str).first
      end
    end
  end

  ##
  # Initializes a ParseTree instance. Includes newline nodes if
  # +include_newlines+ which defaults to +$DEBUG+.

  def initialize(include_newlines=$DEBUG)
    @include_newlines = include_newlines
  end

  ##
  # Main driver for ParseTree. Returns an array of arrays containing
  # the parse tree for +klasses+.
  #
  # Structure:
  #
  #   [[:class, classname, superclassname, [:defn :method1, ...], ...], ...]
  #
  # NOTE: v1.0 - v1.1 had the signature (klass, meth=nil). This wasn't
  # used much at all and since parse_tree_for_method already existed,
  # it was deemed more useful to expand this method to do multiple
  # classes.

  def parse_tree(*klasses)
    result = []
    klasses.each do |klass|
      klassname = klass.name rescue '' # HACK klass.name should never be nil
                                   # Tempfile's DelegateClass(File) seems to
                                   # cause this
      klassname = "UnnamedClass_#{klass.object_id}" if klassname.empty?
      klassname = klassname.to_sym

      code = if Class === klass then
               sc = klass.superclass
               sc_name = ((sc.nil? or sc.name.empty?) ? "nil" : sc.name).intern
               [:class, klassname, [:const, sc_name]]
             else
               [:module, klassname]
             end

      method_names = []
      method_names += klass.instance_methods false
      method_names += klass.private_instance_methods false
      # protected methods are included in instance_methods, go figure!

      method_names.sort.each do |m|
        r = parse_tree_for_method(klass, m.to_sym)
        code << r
      end

      klass.modules.each do |mod| # TODO: add a test for this damnit
        mod.instance_methods.each do |m|
          r = parse_tree_for_method(mod, m.to_sym)
          code << r
        end
      end

      klass.singleton_methods(false).sort.each do |m|
        code << parse_tree_for_method(klass, m.to_sym, true)
      end

      result << code
    end
    return result
  end

  ##
  # Returns the parse tree for just one +method+ of a class +klass+.
  #
  # Format:
  #
  #   [:defn, :name, :body]

  def parse_tree_for_method(klass, method, is_cls_meth=false, verbose = true)
    $stderr.puts "** parse_tree_for_method(#{klass}, #{method}):" if $DEBUG
    old_verbose, $VERBOSE = $VERBOSE, verbose
    r = parse_tree_for_meth(klass, method.to_sym, is_cls_meth)
    r
  ensure
    $VERBOSE = old_verbose
  end

  ##
  # Returns the parse tree for a string +source+.
  #
  # Format:
  #
  #   [[sexps] ... ]

  def parse_tree_for_string(source,
                            filename = '(string)', line = 1, verbose = true)
    old_verbose, $VERBOSE = $VERBOSE, verbose
    return parse_tree_for_str0(source, filename, line)
  ensure
    $VERBOSE = old_verbose
  end

  def parse_tree_for_str0(*__1args2__) # :nodoc:
    parse_tree_for_str(*__1args2__)    # just helps clean up the binding
  end

  if RUBY_VERSION < "1.8.4" then
    inline do |builder|
      builder.add_type_converter("bool", '', '')
      builder.c_singleton "
        bool has_alloca() {
          (void)self;
          #ifdef C_ALLOCA
            return Qtrue;
          #else
            return Qfalse;
          #endif
          }"
    end
  else
    def self.has_alloca
      true
    end
  end


  NODE_NAMES = [
                #  00
                :method, :fbody, :cfunc, :scope, :block,
                :if, :case, :when, :opt_n, :while,
                #  10
                :until, :iter, :for, :break, :next,
                :redo, :retry, :begin, :rescue, :resbody,
                #  20
                :ensure, :and, :or, :not, :masgn,
                :lasgn, :dasgn, :dasgn_curr, :gasgn, :iasgn,
                #  30
                :cdecl, :cvasgn, :cvdecl, :op_asgn1, :op_asgn2,
                :op_asgn_and, :op_asgn_or, :call, :fcall, :vcall,
                #  40
                :super, :zsuper, :array, :zarray, :hash,
                :return, :yield, :lvar, :dvar, :gvar,
                #  50
                :ivar, :const, :cvar, :nth_ref, :back_ref,
                :match, :match2, :match3, :lit, :str,
                #  60
                :dstr, :xstr, :dxstr, :evstr, :dregx,
                :dregx_once, :args, :argscat, :argspush, :splat,
                #  70
                :to_ary, :svalue, :block_arg, :block_pass, :defn,
                :defs, :alias, :valias, :undef, :class,
                #  80
                :module, :sclass, :colon2, :colon3, :cref,
                :dot2, :dot3, :flip2, :flip3, :attrset,
                #  90
                :self, :nil, :true, :false, :defined,
                #  95
                :newline, :postexe, :alloca, :dmethod, :bmethod,
                # 100
                :memo, :ifunc, :dsym, :attrasgn,
                :last
               ]

  if RUBY_VERSION < "1.8.4" then
    NODE_NAMES.delete :alloca unless has_alloca
  end

  if RUBY_VERSION > "1.9" then
    NODE_NAMES.insert NODE_NAMES.index(:hash), :values
    NODE_NAMES.insert NODE_NAMES.index(:defined), :errinfo
    NODE_NAMES.insert NODE_NAMES.index(:last), :prelude, :lambda
    NODE_NAMES.delete :dmethod
    NODE_NAMES[128] = NODE_NAMES.delete :newline
  end

  ############################################################
  # END of rdoc methods
  ############################################################

  inline do |builder|
    builder.add_type_converter("bool", '', '')
    builder.add_type_converter("ID *", '', '')
    builder.add_type_converter("NODE *", '(NODE *)', '(VALUE)')
    builder.include '"intern.h"'
    builder.include '"version.h"'
    builder.include '"rubysig.h"'
    builder.include '"node.h"'
    builder.include '"st.h"'
    builder.include '"env.h"'

    builder.prefix '#define _sym(s) ID2SYM(rb_intern((s)))'

    if RUBY_VERSION < "1.8.6" then
      builder.prefix '#define RARRAY_PTR(s) (RARRAY(s)->ptr)'
      builder.prefix '#define RARRAY_LEN(s) (RARRAY(s)->len)'
    end

    if ENV['ANAL'] or ENV['DOMAIN'] =~ /zenspider/ then
      builder.add_compile_flags "-Wall"
      builder.add_compile_flags "-W"
      builder.add_compile_flags "-Wpointer-arith"
      builder.add_compile_flags "-Wcast-qual"
      builder.add_compile_flags "-Wcast-align"
      builder.add_compile_flags "-Wwrite-strings"
      builder.add_compile_flags "-Wmissing-noreturn"
      builder.add_compile_flags "-Wno-long-long"

      # NOTE: this flag doesn't work w/ gcc 2.95.x - the FreeBSD default
      builder.add_compile_flags "-Wno-strict-aliasing"

      # ruby.h screws these up hardcore:
      # builder.add_compile_flags "-Wundef"
      # builder.add_compile_flags "-Wconversion"
      # builder.add_compile_flags "-Wstrict-prototypes"
      # builder.add_compile_flags "-Wmissing-prototypes"
      # builder.add_compile_flags "-Wsign-compare"
    end

    # NOTE: If you get weird compiler errors like:
    #    dereferencing type-punned pointer will break strict-aliasing rules
    # PLEASE do one of the following:
    # 1) Get me a login on your box so I can repro this and get it fixed.
    # 2) Fix it and send me the patch
    # 3) (quick, but dirty and bad), comment out the following line:
    builder.add_compile_flags "-Werror" unless RUBY_PLATFORM =~ /mswin/

    builder.prefix %{
        #define nd_3rd   u3.node
        static unsigned case_level = 0;
        static unsigned when_level = 0;
        static unsigned inside_case_args = 0;
        static int masgn_level = 0;
    }

    builder.prefix %{
      static VALUE wrap_into_node(const char * name, VALUE val) {
        VALUE n = rb_ary_new();
        rb_ary_push(n, _sym(name));
        if (val) rb_ary_push(n, val);
        return n;
      }
    }

    builder.prefix %{
        struct METHOD {
          VALUE klass, rklass;
          VALUE recv;
          ID id, oid;
#if RUBY_VERSION_CODE > 182
          int safe_level;
#endif
          NODE *body;
        };

        struct BLOCK {
          NODE *var;
          NODE *body;
          VALUE self;
          struct FRAME frame;
          struct SCOPE *scope;
          VALUE klass;
          NODE *cref;
          int iter;
          int vmode;
          int flags;
          int uniq;
          struct RVarmap *dyna_vars;
          VALUE orig_thread;
          VALUE wrapper;
          VALUE block_obj;
          struct BLOCK *outer;
          struct BLOCK *prev;
        };
    } unless RUBY_VERSION >= "1.9" # we got matz to add this to env.h

  ##
  # add_to_parse_tree(self, ary, node, local_variables)

  builder.prefix %Q@
void add_to_parse_tree(VALUE self, VALUE ary, NODE * n, ID * locals) {
  NODE * volatile node = n;
  VALUE current;
  VALUE node_name;
  static VALUE node_names = Qnil;

  if (NIL_P(node_names)) {
    node_names = rb_const_get_at(rb_path2class("RawParseTree"),rb_intern("NODE_NAMES"));
  }

  if (!node) return;

again:

  if (node) {
    node_name = rb_ary_entry(node_names, nd_type(node));
    if (RTEST(ruby_debug)) {
      fprintf(stderr, "%15s: %s%s%s\\n",
        rb_id2name(SYM2ID(node_name)),
        (RNODE(node)->u1.node != NULL ? "u1 " : "   "),
        (RNODE(node)->u2.node != NULL ? "u2 " : "   "),
        (RNODE(node)->u3.node != NULL ? "u3 " : "   "));
    }
  } else {
    node_name = _sym("ICKY");
  }

  current = rb_ary_new();
  rb_ary_push(ary, current);
  rb_ary_push(current, node_name);

  switch (nd_type(node)) {

    case NODE_BLOCK:
      {
        while (node) {
          add_to_parse_tree(self, current, node->nd_head, locals);
          node = node->nd_next;
        }
        if (!masgn_level && RARRAY_LEN(current) == 2) {
          rb_ary_pop(ary);
          rb_ary_push(ary, rb_ary_pop(current));
          return;
        }
      }
      break;

    case NODE_FBODY:
    case NODE_DEFINED:
      add_to_parse_tree(self, current, node->nd_head, locals);
      break;

    case NODE_COLON2:
      add_to_parse_tree(self, current, node->nd_head, locals);
      rb_ary_push(current, ID2SYM(node->nd_mid));
      break;

    case NODE_MATCH2:
    case NODE_MATCH3:
      add_to_parse_tree(self, current, node->nd_recv, locals);
      add_to_parse_tree(self, current, node->nd_value, locals);
      break;

    case NODE_BEGIN:
    case NODE_OPT_N:
    case NODE_NOT:
      add_to_parse_tree(self, current, node->nd_body, locals);
      break;

    case NODE_IF:
      add_to_parse_tree(self, current, node->nd_cond, locals);
      if (node->nd_body) {
        add_to_parse_tree(self, current, node->nd_body, locals);
      } else {
        rb_ary_push(current, Qnil);
      }
      if (node->nd_else) {
        add_to_parse_tree(self, current, node->nd_else, locals);
      } else {
        rb_ary_push(current, Qnil);
      }
      break;

  case NODE_CASE:
    case_level++;
    if (node->nd_head != NULL) {
      add_to_parse_tree(self, current, node->nd_head, locals); /* expr */
    } else {
      rb_ary_push(current, Qnil);
    }
    node = node->nd_body;
    while (node) {
      add_to_parse_tree(self, current, node, locals);
      if (nd_type(node) == NODE_WHEN) {                 /* when */
        node = node->nd_next;
      } else {
        break;                                          /* else */
      }
      if (! node) {
        rb_ary_push(current, Qnil);                     /* no else */
      }
    }
    case_level--;
    break;

  case NODE_WHEN:
    when_level++;
    if (!inside_case_args && case_level < when_level) { /* when without case, ie, no expr in case */
      if (when_level > 0) when_level--;
      rb_ary_pop(ary); /* reset what current is pointing at */
      node = NEW_CASE(0, node);
      goto again;
    }
    inside_case_args++;
    add_to_parse_tree(self, current, node->nd_head, locals); /* args */
    inside_case_args--;

    if (node->nd_body) {
      add_to_parse_tree(self, current, node->nd_body, locals); /* body */
    } else {
      rb_ary_push(current, Qnil);
    }

    if (when_level > 0) when_level--;
    break;

  case NODE_WHILE:
  case NODE_UNTIL:
    add_to_parse_tree(self, current,  node->nd_cond, locals);
    if (node->nd_body) {
      add_to_parse_tree(self, current,  node->nd_body, locals);
    } else {
      rb_ary_push(current, Qnil);
    }
    rb_ary_push(current, node->nd_3rd == 0 ? Qfalse : Qtrue);
    break;

  case NODE_BLOCK_PASS:
    add_to_parse_tree(self, current, node->nd_body, locals);
    add_to_parse_tree(self, current, node->nd_iter, locals);
    break;

  case NODE_ITER:
  case NODE_FOR:
    add_to_parse_tree(self, current, node->nd_iter, locals);
    masgn_level++;
    if (node->nd_var != (NODE *)1
        && node->nd_var != (NODE *)2
        && node->nd_var != NULL) {
      add_to_parse_tree(self, current, node->nd_var, locals);
    } else {
      if (node->nd_var == NULL) {
        // e.g. proc {}
        rb_ary_push(current, Qnil);
      } else {
        // e.g. proc {||}
        rb_ary_push(current, INT2FIX(0));
      }
    }
    masgn_level--;
    add_to_parse_tree(self, current, node->nd_body, locals);
    break;

  case NODE_BREAK:
  case NODE_NEXT:
    if (node->nd_stts)
      add_to_parse_tree(self, current, node->nd_stts, locals);

    break;

  case NODE_YIELD:
    if (node->nd_stts)
      add_to_parse_tree(self, current, node->nd_stts, locals);

    // if node is newline, it is aref_args w/ a splat, eg: yield([*[1]])
    if (node->nd_stts && nd_type(node->nd_stts) == NODE_NEWLINE)
      rb_ary_push(current, Qtrue);

    // array is an array, not list of args
    if (node->nd_stts
        && (nd_type(node->nd_stts) == NODE_ARRAY
            || nd_type(node->nd_stts) == NODE_ZARRAY)
        && !node->nd_state)
      rb_ary_push(current, Qtrue);

    break;

  case NODE_RESCUE:
    add_to_parse_tree(self, current, node->nd_1st, locals);
    add_to_parse_tree(self, current, node->nd_2nd, locals);
    add_to_parse_tree(self, current, node->nd_3rd, locals);
    break;

  /*
  // rescue body:
  // begin stmt rescue exception => var; stmt; [rescue e2 => v2; s2;]* end
  // stmt rescue stmt
  // a = b rescue c
  */

  case NODE_RESBODY:
      if (node->nd_3rd) {
        add_to_parse_tree(self, current, node->nd_3rd, locals);
      } else {
        rb_ary_push(current, Qnil);
      }
      add_to_parse_tree(self, current, node->nd_2nd, locals);
      add_to_parse_tree(self, current, node->nd_1st, locals);
    break;

  case NODE_ENSURE:
    add_to_parse_tree(self, current, node->nd_head, locals);
    if (node->nd_ensr) {
      add_to_parse_tree(self, current, node->nd_ensr, locals);
    }
    break;

  case NODE_AND:
  case NODE_OR:
    add_to_parse_tree(self, current, node->nd_1st, locals);
    add_to_parse_tree(self, current, node->nd_2nd, locals);
    break;

  case NODE_FLIP2:
  case NODE_FLIP3:
    if (nd_type(node->nd_beg) == NODE_LIT) {
      /*
       new somewhere between 1.8.6 p287 to p368 and 1.8.7 p72 to p160.
       [:flip2, [:call, [:lit, 1], :==, [:array, [:gvar, :$.]]],
      */
      VALUE result = rb_ary_new3(1, _sym("call"));
      add_to_parse_tree(self, result, node->nd_beg, locals);
      rb_ary_push(result, _sym("=="));
      rb_ary_push(result, rb_ary_new3(2, _sym("array"),
                                      rb_ary_new3(2, _sym("gvar"),
                                                     _sym("$."))));
      rb_ary_push(current, result);
    } else {
      add_to_parse_tree(self, current, node->nd_beg, locals);
    }


    // add_to_parse_tree(self, current, node->nd_beg, locals);
    add_to_parse_tree(self, current, node->nd_end, locals);
    break;

  case NODE_DOT2:
  case NODE_DOT3:
    add_to_parse_tree(self, current, node->nd_beg, locals);
    add_to_parse_tree(self, current, node->nd_end, locals);
    break;

  case NODE_RETURN:
    if (node->nd_stts)
      add_to_parse_tree(self, current, node->nd_stts, locals);
    break;

  case NODE_ARGSCAT:
  case NODE_ARGSPUSH:
    add_to_parse_tree(self, current, node->nd_head, locals);
    add_to_parse_tree(self, current, node->nd_body, locals);
    break;

  case NODE_CALL:
  case NODE_FCALL:
  case NODE_VCALL:
    if (nd_type(node) != NODE_FCALL)
      add_to_parse_tree(self, current, node->nd_recv, locals);
    rb_ary_push(current, ID2SYM(node->nd_mid));
    if (node->nd_args || nd_type(node) != NODE_FCALL)
      add_to_parse_tree(self, current, node->nd_args, locals);
    break;

  case NODE_SUPER:
    add_to_parse_tree(self, current, node->nd_args, locals);
    break;

  case NODE_BMETHOD:
    {
      struct BLOCK *data;
      Data_Get_Struct(node->nd_cval, struct BLOCK, data);
      if (data->var == 0 || data->var == (NODE *)1 || data->var == (NODE *)2) {
        rb_ary_push(current, Qnil);
      } else {
        masgn_level++;
        add_to_parse_tree(self, current, data->var, locals);
        masgn_level--;
      }
      add_to_parse_tree(self, current, data->body, locals);
    }
    break;

#if RUBY_VERSION_CODE < 190
  case NODE_DMETHOD:
    {
      struct METHOD *data;
      Data_Get_Struct(node->nd_cval, struct METHOD, data);
      rb_ary_push(current, ID2SYM(data->id));
      add_to_parse_tree(self, current, data->body, locals);
      break;
    }
#endif

  case NODE_METHOD:
    add_to_parse_tree(self, current, node->nd_3rd, locals);
    break;

  case NODE_SCOPE:
    add_to_parse_tree(self, current, node->nd_next, node->nd_tbl);
    break;

  case NODE_OP_ASGN1:
    add_to_parse_tree(self, current, node->nd_recv, locals);
#if RUBY_VERSION_CODE < 185
    add_to_parse_tree(self, current, node->nd_args->nd_next, locals);
    rb_ary_pop(rb_ary_entry(current, -1)); /* no idea why I need this */
#else
    add_to_parse_tree(self, current, node->nd_args->nd_2nd, locals);
#endif
    switch (node->nd_mid) {
    case 0:
      rb_ary_push(current, _sym("||"));
      break;
    case 1:
      rb_ary_push(current, _sym("&&"));
      break;
    default:
      rb_ary_push(current, ID2SYM(node->nd_mid));
      break;
    }
    add_to_parse_tree(self, current, node->nd_args->nd_head, locals);
    break;

  case NODE_OP_ASGN2:
    add_to_parse_tree(self, current, node->nd_recv, locals);
    rb_ary_push(current, ID2SYM(node->nd_next->nd_aid));

    switch (node->nd_next->nd_mid) {
    case 0:
      rb_ary_push(current, _sym("||"));
      break;
    case 1:
      rb_ary_push(current, _sym("&&"));
      break;
    default:
      rb_ary_push(current, ID2SYM(node->nd_next->nd_mid));
      break;
    }

    add_to_parse_tree(self, current, node->nd_value, locals);
    break;

  case NODE_OP_ASGN_AND:
  case NODE_OP_ASGN_OR:
    add_to_parse_tree(self, current, node->nd_head, locals);
    add_to_parse_tree(self, current, node->nd_value, locals);
    break;

  case NODE_MASGN:
    masgn_level++;
    if (node->nd_head) {
      add_to_parse_tree(self, current, node->nd_head, locals);
    } else {
      rb_ary_push(current, Qnil);
    }
    if (node->nd_args) {
      if (node->nd_args != (NODE *)-1) {
        add_to_parse_tree(self, current, node->nd_args, locals);
      } else {
        rb_ary_push(current, wrap_into_node("splat", 0));
      }
    } else {
      rb_ary_push(current, Qnil);
    }
    if (node->nd_value) {
      add_to_parse_tree(self, current, node->nd_value, locals);
    } else {
      rb_ary_push(current, Qnil);
    }
    masgn_level--;
    break;

  case NODE_LASGN:
  case NODE_IASGN:
  case NODE_DASGN:
  case NODE_CVASGN:
  case NODE_CVDECL:
  case NODE_GASGN:
    rb_ary_push(current, ID2SYM(node->nd_vid));
    add_to_parse_tree(self, current, node->nd_value, locals);
    break;

  case NODE_CDECL:
    if (node->nd_vid) {
      rb_ary_push(current, ID2SYM(node->nd_vid));
    } else {
      add_to_parse_tree(self, current, node->nd_else, locals);
    }

    add_to_parse_tree(self, current, node->nd_value, locals);
    break;

  case NODE_DASGN_CURR:
    rb_ary_push(current, ID2SYM(node->nd_vid));
    if (node->nd_value) {
      add_to_parse_tree(self, current, node->nd_value, locals);
      if (!masgn_level && RARRAY_LEN(current) == 2) {
        rb_ary_pop(ary);
        return;
      }
    } else {
      if (!masgn_level) {
        rb_ary_pop(ary);
        return;
      }
    }
    break;

  case NODE_VALIAS:           /* u1 u2 (alias $global $global2) */
#if RUBY_VERSION_CODE < 185
    rb_ary_push(current, ID2SYM(node->u2.id));
    rb_ary_push(current, ID2SYM(node->u1.id));
#else
    rb_ary_push(current, ID2SYM(node->u1.id));
    rb_ary_push(current, ID2SYM(node->u2.id));
#endif
    break;
  case NODE_ALIAS:            /* u1 u2 (alias :blah :blah2) */
#if RUBY_VERSION_CODE < 185
    rb_ary_push(current, wrap_into_node("lit", ID2SYM(node->u2.id)));
    rb_ary_push(current, wrap_into_node("lit", ID2SYM(node->u1.id)));
#else
    add_to_parse_tree(self, current, node->nd_1st, locals);
    add_to_parse_tree(self, current, node->nd_2nd, locals);
#endif
    break;

  case NODE_UNDEF:            /* u2    (undef name, ...) */
#if RUBY_VERSION_CODE < 185
    rb_ary_push(current, wrap_into_node("lit", ID2SYM(node->u2.id)));
#else
    add_to_parse_tree(self, current, node->nd_value, locals);
#endif
    break;

  case NODE_COLON3:           /* u2    (::OUTER_CONST) */
    rb_ary_push(current, ID2SYM(node->u2.id));
    break;

  case NODE_HASH:
    {
      NODE *list;

      list = node->nd_head;
      while (list) {
        add_to_parse_tree(self, current, list->nd_head, locals);
        list = list->nd_next;
        if (list == 0)
          rb_bug("odd number list for Hash");
        add_to_parse_tree(self, current, list->nd_head, locals);
        list = list->nd_next;
      }
    }
    break;

  case NODE_ARRAY:
      while (node) {
        add_to_parse_tree(self, current, node->nd_head, locals);
        node = node->nd_next;
      }
    break;

  case NODE_DSTR:
  case NODE_DSYM:
  case NODE_DXSTR:
  case NODE_DREGX:
  case NODE_DREGX_ONCE:
    {
      NODE *list = node->nd_next;
      rb_ary_push(current, rb_str_new3(node->nd_lit));
      while (list) {
        if (list->nd_head) {
          switch (nd_type(list->nd_head)) {
          case NODE_STR:
            add_to_parse_tree(self, current, list->nd_head, locals);
            break;
          case NODE_EVSTR:
            add_to_parse_tree(self, current, list->nd_head, locals);
            break;
          default:
            add_to_parse_tree(self, current, list->nd_head, locals);
            break;
          }
        }
        list = list->nd_next;
      }
      switch (nd_type(node)) {
      case NODE_DREGX:
      case NODE_DREGX_ONCE:
        if (node->nd_cflag) {
          rb_ary_push(current, INT2FIX(node->nd_cflag));
        }
      }
    }
    break;

  case NODE_DEFN:
  case NODE_DEFS:
    if (node->nd_defn) {
      if (nd_type(node) == NODE_DEFS)
        add_to_parse_tree(self, current, node->nd_recv, locals);
      rb_ary_push(current, ID2SYM(node->nd_mid));
      add_to_parse_tree(self, current, node->nd_defn, locals);
    }
    break;

  case NODE_CLASS:
  case NODE_MODULE:
    if (nd_type(node->nd_cpath) == NODE_COLON2 && ! node->nd_cpath->nd_vid) {
      rb_ary_push(current, ID2SYM((ID)node->nd_cpath->nd_mid));
    } else {
      add_to_parse_tree(self, current, node->nd_cpath, locals);
    }

    if (nd_type(node) == NODE_CLASS) {
      if (node->nd_super) {
        add_to_parse_tree(self, current, node->nd_super, locals);
      } else {
        rb_ary_push(current, Qnil);
      }
    }
    add_to_parse_tree(self, current, node->nd_body, locals);
    break;

  case NODE_SCLASS:
    add_to_parse_tree(self, current, node->nd_recv, locals);
    add_to_parse_tree(self, current, node->nd_body, locals);
    break;

  case NODE_ARGS: {
    NODE *optnode;
    int i = 0, max_args = node->nd_cnt;

    /* push regular argument names */
    for (; i < max_args; i++) {
      rb_ary_push(current, ID2SYM(locals[i + 3]));
    }

    /* look for optional arguments */
    masgn_level++;
    optnode = node->nd_opt;
    while (optnode) {
      rb_ary_push(current, ID2SYM(locals[i + 3]));
      i++;
      optnode = optnode->nd_next;
    }

    /* look for vargs */
#if RUBY_VERSION_CODE > 184
    if (node->nd_rest) {
      VALUE sym = rb_str_new2("*");
      if (locals[i + 3]) {
        rb_str_concat(sym, rb_str_new2(rb_id2name(locals[i + 3])));
      }
      sym = rb_str_intern(sym);
      rb_ary_push(current, sym);
    }
#else
    {
      long arg_count = (long)node->nd_rest;
      if (arg_count > 0) {
        /* *arg name */
        VALUE sym = rb_str_new2("*");
        if (locals[i + 3]) {
          rb_str_concat(sym, rb_str_new2(rb_id2name(locals[i + 3])));
        }
        sym = rb_str_intern(sym);
        rb_ary_push(current, sym);
      } else if (arg_count == 0) {
        /* nothing to do in this case, empty list */
      } else if (arg_count == -1) {
        /* nothing to do in this case, handled above */
      } else if (arg_count == -2) {
        /* nothing to do in this case, no name == no use */
        rb_ary_push(current, rb_str_intern(rb_str_new2("*")));
      } else {
        rb_raise(rb_eArgError,
                 "not a clue what this arg value is: %ld", arg_count);
      }
    }
#endif

    optnode = node->nd_opt;
    if (optnode) {
      add_to_parse_tree(self, current, node->nd_opt, locals);
    }
    masgn_level--;
  }  break;

  case NODE_LVAR:
  case NODE_DVAR:
  case NODE_IVAR:
  case NODE_CVAR:
  case NODE_GVAR:
  case NODE_CONST:
  case NODE_ATTRSET:
    rb_ary_push(current, ID2SYM(node->nd_vid));
    break;

  case NODE_XSTR:             /* u1    (%x{ls}) */
  case NODE_STR:              /* u1 */
  case NODE_LIT:
    rb_ary_push(current, node->nd_lit);
    break;

  case NODE_MATCH:            /* u1 -> [:lit, u1] */
    rb_ary_push(current, wrap_into_node("lit", node->nd_lit));
    break;

  case NODE_NEWLINE:
    rb_ary_push(current, INT2FIX(nd_line(node)));
    rb_ary_push(current, rb_str_new2(node->nd_file));
    if (! RTEST(rb_iv_get(self, "\@include_newlines"))) {
      rb_ary_pop(ary); /* nuke it */
      node = node->nd_next;
      goto again;
    } else {
      add_to_parse_tree(self, current, node->nd_next, locals);
    }
    break;

  case NODE_NTH_REF:          /* u2 u3 ($1) - u3 is local_cnt('~') ignorable? */
    rb_ary_push(current, INT2FIX(node->nd_nth));
    break;

  case NODE_BACK_REF:         /* u2 u3 ($& etc) */
    {
    char c = node->nd_nth;
    rb_ary_push(current, rb_str_intern(rb_str_new(&c, 1)));
    }
    break;

  case NODE_BLOCK_ARG:        /* u1 u3 (def x(&b) */
    rb_ary_push(current, ID2SYM(node->u1.id));
    break;

  /* these nodes are empty and do not require extra work: */
  case NODE_RETRY:
  case NODE_FALSE:
  case NODE_NIL:
  case NODE_SELF:
  case NODE_TRUE:
  case NODE_ZARRAY:
  case NODE_ZSUPER:
  case NODE_REDO:
    break;

  case NODE_SPLAT:
  case NODE_TO_ARY:
  case NODE_SVALUE:             /* a = b, c */
    add_to_parse_tree(self, current, node->nd_head, locals);
    break;

  case NODE_ATTRASGN:           /* literal.meth = y u1 u2 u3 */
    /* node id node */
    if (node->nd_1st == RNODE(1)) {
      add_to_parse_tree(self, current, NEW_SELF(), locals);
    } else {
      add_to_parse_tree(self, current, node->nd_1st, locals);
    }
    rb_ary_push(current, ID2SYM(node->u2.id));
    add_to_parse_tree(self, current, node->nd_3rd, locals);
    break;

  case NODE_EVSTR:
    add_to_parse_tree(self, current, node->nd_2nd, locals);
    break;

  case NODE_POSTEXE:            /* END { ... } */
    /* Nothing to do here... we are in an iter block */
    break;

  case NODE_IFUNC:
  case NODE_CFUNC:
    rb_ary_push(current, INT2NUM((long)node->nd_cfnc));
    rb_ary_push(current, INT2NUM(node->nd_argc));
    break;

#if RUBY_VERSION_CODE >= 190
  case NODE_ERRINFO:
  case NODE_VALUES:
  case NODE_PRELUDE:
  case NODE_LAMBDA:
    puts("no worky in 1.9 yet");
    break;
#endif

  /* Nodes we found but have yet to decypher */
  /* I think these are all runtime only... not positive but... */
  case NODE_MEMO:               /* enum.c zip */
  case NODE_CREF:
  /* #defines: */
  /* case NODE_LMASK: */
  /* case NODE_LSHIFT: */
  default:
    rb_warn("Unhandled node #%d type '%s'", nd_type(node), rb_id2name(SYM2ID(rb_ary_entry(node_names, nd_type(node)))));
    if (RNODE(node)->u1.node != NULL) rb_warning("unhandled u1 value");
    if (RNODE(node)->u2.node != NULL) rb_warning("unhandled u2 value");
    if (RNODE(node)->u3.node != NULL) rb_warning("unhandled u3 value");
    if (RTEST(ruby_debug)) fprintf(stderr, "u1 = %p u2 = %p u3 = %p\\n", (void*)node->nd_1st, (void*)node->nd_2nd, (void*)node->nd_3rd);
    rb_ary_push(current, INT2FIX(-99));
    rb_ary_push(current, INT2FIX(nd_type(node)));
    break;
  }
}
@ # end of add_to_parse_tree block

    builder.c %Q{
  static VALUE parse_tree_for_proc(VALUE proc) {
    VALUE result = rb_ary_new();
    struct BLOCK *data;
    Data_Get_Struct(proc, struct BLOCK, data);

    rb_ary_push(result, _sym("iter"));
    rb_ary_push(result, rb_ary_new3(4, _sym("call"), Qnil, _sym("proc"),
                                    rb_ary_new3(1, _sym("arglist"))));
    masgn_level++;
    if (data->var) {
      add_to_parse_tree(self, result, data->var, NULL);
    } else {
      rb_ary_push(result, Qnil);
    }
    add_to_parse_tree(self, result, data->body, NULL);
    masgn_level--;

    return result;
  }
}

    builder.c %Q{
static VALUE parse_tree_for_meth(VALUE klass, VALUE method, VALUE is_cls_meth) {
  VALUE n;
  NODE *node = NULL;
  ID id;
  VALUE result = rb_ary_new();
  VALUE version = rb_const_get_at(rb_cObject,rb_intern("RUBY_VERSION"));

  (void) self; /* quell warnings */

  if (strcmp(StringValuePtr(version), #{RUBY_VERSION.inspect})) {
    rb_fatal("bad version, %s != #{RUBY_VERSION}\\n", StringValuePtr(version));
  }

  id = rb_to_id(method);
  if (RTEST(is_cls_meth)) { /* singleton method */
    klass = CLASS_OF(klass);
  }
  if (st_lookup(RCLASS(klass)->m_tbl, id, &n)) {
    node = (NODE*)n;
    rb_ary_push(result, _sym(is_cls_meth ? "defs": "defn"));
    if (is_cls_meth) {
      rb_ary_push(result, rb_ary_new3(1, _sym("self")));
    }
    rb_ary_push(result, ID2SYM(id));
    add_to_parse_tree(self, result, node->nd_body, NULL);
  } else {
    rb_ary_push(result, Qnil);
  }

  return result;
}
}

    extern_mode = RUBY_PLATFORM =~ /mswin/ ? 'RUBY_EXTERN' : 'extern'
    builder.prefix " #{extern_mode} NODE *ruby_eval_tree_begin; " \
      if RUBY_VERSION < '1.9.0'

    # FIXME: ruby_in_eval is not properly exported across platforms
    # http://blade.nagaokaut.ac.jp/cgi-bin/scat.rb/ruby/ruby-core/13558
    builder.c %Q{
static VALUE parse_tree_for_str(VALUE source, VALUE filename, VALUE line) {
  VALUE tmp;
  VALUE result = rb_ary_new();
  NODE *node = NULL;
  int critical;

  tmp = rb_check_string_type(filename);
  if (NIL_P(tmp)) {
    filename = rb_str_new2("(string)");
  }

  if (NIL_P(line)) {
    line = LONG2FIX(1);
  }

  ruby_nerrs = 0;
  StringValue(source);
  critical = rb_thread_critical;
  rb_thread_critical = Qtrue;
  ruby_in_eval++;
  node = rb_compile_string(StringValuePtr(filename), source, NUM2INT(line));
  ruby_in_eval--;
  rb_thread_critical = critical;

  if (ruby_nerrs > 0) {
    ruby_nerrs = 0;
#if RUBY_VERSION_CODE < 190
    ruby_eval_tree_begin = 0;
#endif
    rb_exc_raise(ruby_errinfo);
  }

  add_to_parse_tree(self, result, node, NULL);

  return result;
}
}

  end # inline call
end # RawParseTree class

class ParseTree < RawParseTree
  ##
  # Initializes a ParseTree instance. Includes newline nodes if
  # +include_newlines+ which defaults to +$DEBUG+.

  def initialize(include_newlines=$DEBUG)
    super
    @unifier = Unifier.new
  end

  ##
  # Main driver for ParseTree. Returns a Sexp instance containing the
  # AST representing the input given. This is a UnifiedRuby sexp, not
  # a raw sexp from ruby. If you want raw, use the old
  # parse_tree_for_xxx methods... Please tell me if/why you want raw,
  # I'd like to know so I can justify keeping the code around.

  def process(input, verbose = nil, file = "(string)", line = -1)
    case input
    when Array then
      @unifier.process(input)
    when String then
      pt = self.parse_tree_for_string(input, file, line, verbose).first
      @unifier.process(pt)
    else
      raise ArgumentError, "Unknown input type #{input.inspect}"
    end
  end
end
