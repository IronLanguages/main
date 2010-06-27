$TESTING = true

$0 = __FILE__ if $0 =~ /-e|\(eval\)/

require 'inline'
require 'tempfile'
require 'tmpdir'
require 'test/unit'
require 'fileutils' unless defined?(::FileUtils)

File.umask(0)

require 'pathname'

$expand_paths     = Pathname.new(__FILE__).absolute?
$inline_path      = './lib/inline.rb'
$test_inline_path = './test/test_inline.rb'

if $expand_paths then
  $inline_path      = File.expand_path $inline_path
  $test_inline_path = File.expand_path $test_inline_path
end

class InlineTestCase < Test::Unit::TestCase
  def setup
    super
    @rootdir = File.join(Dir.tmpdir, "test_inline.#{$$}")
    Dir.mkdir @rootdir, 0700 unless test ?d, @rootdir
    ENV['INLINEDIR'] = @rootdir
  end

  def teardown
    unless $DEBUG then
      FileUtils.rm_rf @rootdir
      ENV.delete 'INLINEDIR'
    end
  end

  def test_stupid
    #shuts test unit up
  end
end

class TestDir < InlineTestCase
  def setup
    super
    @count = 1
  end

  def util_assert_secure(perms, should_pass)
    path = File.join(@rootdir, @count.to_s)
    @count += 1
    Dir.mkdir path, perms unless perms.nil?
    if should_pass then
      Dir.assert_secure path
    else
      assert_raises(SecurityError) do
        Dir.assert_secure path
      end
    end
  end

  def test_assert_secure
    # existing/good
    util_assert_secure 0700, true
    # existing/bad
    util_assert_secure 0707, false
    util_assert_secure 0770, false
    util_assert_secure 0777, false
    # missing
    util_assert_secure nil, true
  end
end unless Inline::WINDOZE

class TestInline < InlineTestCase

  def test_rootdir
    assert_equal(@rootdir, Inline.rootdir)
  end

  def test_directory
    inlinedir = File.join(@rootdir, ".ruby_inline")
    assert_equal(inlinedir, Inline.directory)
  end

end

class TestInline
class TestC < InlineTestCase

  def setup
    super
    @builder = Inline::C.new(self.class)
  end


  # quick hack to make tests more readable,
  # does nothing I wouldn't otherwise do...
  def inline(lang=:C)
    self.class.inline(lang, true) do |builder|
      yield(builder)
    end
  end

  def test_initialize
    x = Inline::C.new(self.class)
    assert_equal TestInline::TestC, x.mod
    assert_equal [], x.src
    assert_equal({}, x.sig)
    assert_equal [], x.flags
    assert_equal [], x.libs
  end

  def test_accessor
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.accessor 'method_name', 'int'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-READER
# line N "#{$inline_path}"
static VALUE method_name(VALUE self) {

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  return (INT2FIX(pointer->method_name));
}
    READER

    expected << <<-WRITER
# line N "#{$inline_path}"
static VALUE method_name_equals(VALUE self, VALUE _value) {
  VALUE value = (_value);

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  pointer->method_name = FIX2INT(value);

  return (value);
}
    WRITER

    assert_equal expected, source
  end

  def test_accessor_member_name
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.accessor 'method_name', 'int', 'member_name'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-READER
# line N "#{$inline_path}"
static VALUE method_name(VALUE self) {

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  return (INT2FIX(pointer->member_name));
}
    READER

    expected << <<-WRITER
# line N "#{$inline_path}"
static VALUE method_name_equals(VALUE self, VALUE _value) {
  VALUE value = (_value);

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  pointer->member_name = FIX2INT(value);

  return (value);
}
    WRITER

    assert_equal expected, source
  end

  def test_accessor_no_struct_name
    builder = Inline::C.new self.class

    e = assert_raises RuntimeError do
      builder.accessor 'method_name', 'int'
    end

    assert_equal "struct name not set for reader method_name int", e.message
  end

  def test_add_type_converter
    builder = Inline::C.new self.class

    builder.add_type_converter 'my_type', 'ruby_type2my_type',
                               'my_type2ruby_type'

    assert_equal 'my_type2ruby_type', builder.c2ruby('my_type')
    assert_equal 'ruby_type2my_type', builder.ruby2c('my_type')
  end

  def test_alias_type_converter
    builder = Inline::C.new self.class

    builder.alias_type_converter 'long long', 'int64_t'

    assert_equal 'LL2NUM', builder.c2ruby('int64_t')
    assert_equal 'NUM2LL', builder.ruby2c('int64_t')
  end

  def test_reader
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.reader 'method_name', 'int'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-READER
# line N "#{$inline_path}"
static VALUE method_name(VALUE self) {

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  return (INT2FIX(pointer->method_name));
}
    READER

    assert_equal expected, source
  end

  def test_reader_member_name
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.reader 'method_name', 'int', 'member_name'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-READER
# line N "#{$inline_path}"
static VALUE method_name(VALUE self) {

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  return (INT2FIX(pointer->member_name));
}
    READER

    assert_equal expected, source
  end

  def test_reader_no_struct_name
    builder = Inline::C.new self.class

    e = assert_raises RuntimeError do
      builder.reader 'method_name', 'int'
    end

    assert_equal "struct name not set for reader method_name int", e.message
  end

  def test_remove_type_converter
    builder = Inline::C.new self.class

    builder.remove_type_converter 'long'

    assert_raises ArgumentError do
      builder.c2ruby 'long'
    end
  end

  def test_writer
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.writer 'method_name', 'int'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-WRITER
# line N "#{$inline_path}"
static VALUE method_name_equals(VALUE self, VALUE _value) {
  VALUE value = (_value);

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  pointer->method_name = FIX2INT(value);

  return (value);
}
    WRITER

    assert_equal expected, source
  end

  def test_writer_member_name
    builder = Inline::C.new self.class

    builder.struct_name = 'MyStruct'
    builder.writer 'method_name', 'int', 'member_name'

    source = util_strip_lines builder.src

    expected = []
    expected << <<-WRITER
# line N "#{$inline_path}"
static VALUE method_name_equals(VALUE self, VALUE _value) {
  VALUE value = (_value);

  MyStruct *pointer;

  Data_Get_Struct(self, MyStruct, pointer);

  pointer->member_name = FIX2INT(value);

  return (value);
}
    WRITER

    assert_equal expected, source
  end

  def test_writer_no_struct_name
    builder = Inline::C.new self.class

    e = assert_raises RuntimeError do
      builder.writer 'method_name', 'int'
    end

    assert_equal "struct name not set for writer method_name int", e.message
  end

  def test_ruby2c
    x = Inline::C.new(self.class)
    assert_equal 'NUM2CHR',        x.ruby2c("char")
    assert_equal 'StringValuePtr', x.ruby2c("char *")

    assert_equal "FI\X2INT",       x.ruby2c("int")
    assert_equal 'NUM2UINT',       x.ruby2c("unsigned")
    assert_equal 'NUM2UINT',       x.ruby2c("unsigned int")

    assert_equal 'NUM2LONG',       x.ruby2c("long")
    assert_equal 'NUM2ULONG',      x.ruby2c("unsigned long")

    assert_equal 'NUM2LL',         x.ruby2c("long long")
    assert_equal 'NUM2ULL',        x.ruby2c("unsigned long long")

    assert_equal 'NUM2DBL',        x.ruby2c("double")

    assert_equal 'NUM2OFFT',       x.ruby2c("off_t")

    assert_equal '',               x.ruby2c("VALUE")

    assert_raises ArgumentError do
      x.ruby2c('blah')
    end
  end

  def test_c2ruby
    x = Inline::C.new(self.class)
    assert_equal 'CHR2FIX',      x.c2ruby("char")

    assert_equal 'rb_str_new2',  x.c2ruby("char *")

    assert_equal 'INT2FIX',      x.c2ruby("int")
    assert_equal 'UINT2NUM',     x.c2ruby("unsigned int")
    assert_equal 'UINT2NUM',     x.c2ruby("unsigned")

    assert_equal 'LONG2NUM',     x.c2ruby("long")
    assert_equal 'ULONG2NUM',    x.c2ruby("unsigned long")

    assert_equal 'LL2NUM',       x.c2ruby("long long")
    assert_equal 'ULL2NUM',      x.c2ruby("unsigned long long")

    assert_equal 'rb_float_new', x.c2ruby("double")

    assert_equal 'OFFT2NUM',     x.c2ruby("off_t")

    assert_equal '',             x.c2ruby("VALUE")

    assert_raises ArgumentError do
      x.c2ruby('blah')
    end
  end

  def util_module_name(*signatures)
    md5 = Digest::MD5.new

    signatures.each do |signature|
      @builder.sig[signature] = [nil, 0]
      md5 << signature.to_s
    end

    assert_equal("Inline_TestInline__TestC_#{md5.to_s[0,4]}",
                 @builder.module_name)
  end

  def test_module_name_0_methods
    util_module_name
  end

  def test_module_name_1_method
    util_module_name :something1
  end

  def test_module_name_2_methods
    util_module_name :something2, :something3
  end

  def test_module_name_2_other_methods
    util_module_name :something4, :something5
  end

  def util_parse_signature(src, expected, t=nil, a=nil, b=nil)
    result = nil

    @builder.add_type_converter t, a, b unless t.nil?
    result = @builder.parse_signature(src)

    assert_equal(expected, result)
  end

  def test_parse_signature
    src = "// stupid cpp comment
    #include \"header.h\"
    /* stupid c comment */
    int
    add(int x, int y) {
      int result = x+y;
      return result;
    }
    "

    expected = {
      'name' => 'add',
      'return' => 'int',
      'arity' => 2,
      'args' => [
    ['x', 'int'],
    ['y', 'int']
      ]
    }

    util_parse_signature(src, expected)
  end

  def test_parse_signature_custom

    src = "// stupid cpp comment
    #include \"header.h\"
    /* stupid c comment */
    int
    add(fooby x, int y) {
      int result = x+y;
      return result;
    }
    "

    expected = {
      'name' => 'add',
      'return' => 'int',
      'arity' => 2,
      'args' => [
    [ 'x', 'fooby' ],
    ['y', 'int']
      ]
    }

    util_parse_signature(src, expected,
             "fooby", "r2c_fooby", "c2r_fooby")
  end

  def test_parse_signature_register

    src = "// stupid cpp comment
    #include \"header.h\"
    /* stupid c comment */
    int
    add(register int x, int y) {
      int result = x+y;
      return result;
    }
    "

    expected = {
      'name' => 'add',
      'return' => 'int',
      'arity' => 2,
      'args' => [
    [ 'x', 'register int' ],
    ['y', 'int']
      ]
    }


    util_parse_signature(src, expected,
             "register int", "FI\X2INT", "INT2FI\X")
  end

  def util_generate(src, expected, expand_types=true)
    result = @builder.generate src, expand_types
    result = util_strip_lines result
    result.gsub!(/\# line \d+/, '# line N')
    expected = "# line N \"#{$0}\"\n" + expected
    assert_equal(expected, result)
  end

  def util_generate_raw(src, expected)
    util_generate(src, expected, false)
  end

  def util_strip_lines(src)
    case src
    when String then
      src.gsub(/\# line \d+/, '# line N')
    when Array then
      src.map do |chunk|
        util_strip_lines chunk
      end
    end
  end

  # Ruby Arity Rules, from the mouth of Matz:
  # -2 = ruby array argv
  # -1 = c array argv
  #  0 = self
  #  1 = self, value
  #  2 = self, value, value
  # ...
  # 16 = self, value * 15

  def test_generate_raw_arity_0
    src = "VALUE y(VALUE self) {blah;}"

    expected = "static VALUE y(VALUE self) {blah;}"

    util_generate_raw(src, expected)
  end

  def test_generate_arity_0
    src = "int y() { do_something; return 42; }"

    expected = "static VALUE y(VALUE self) {\n do_something; return INT2FIX(42); }"

    util_generate(src, expected)
  end

  def test_generate_arity_0_no_return
    src = "void y() { do_something; }"

    expected = "static VALUE y(VALUE self) {\n do_something;\nreturn Qnil;\n}"

    util_generate(src, expected)
  end

  def test_generate_arity_0_void_return
    src = "void y(void) {go_do_something_external;}"

    expected = "static VALUE y(VALUE self) {
go_do_something_external;\nreturn Qnil;\n}"

    util_generate(src, expected)
  end

  def test_generate_arity_0_int_return
    src = "int x() {return 42}"

    expected = "static VALUE x(VALUE self) {
return INT2FIX(42)}"

    util_generate(src, expected)
  end

  def test_generate_raw_arity_1
    src = "VALUE y(VALUE self, VALUE obj) {blah;}"

    expected = "static VALUE y(VALUE self, VALUE obj) {blah;}"

    util_generate_raw(src, expected)
  end

  def test_generate_arity_1
    src = "int y(int x) {blah; return x+1;}"

    expected = "static VALUE y(VALUE self, VALUE _x) {\n  int x = FIX2INT(_x);\nblah; return INT2FIX(x+1);}"

    util_generate(src, expected)
  end

  def test_generate_arity_1_no_return
    src = "void y(int x) {blah;}"

    expected = "static VALUE y(VALUE self, VALUE _x) {\n  int x = FIX2INT(_x);\nblah;\nreturn Qnil;\n}"

    util_generate(src, expected)
  end

  def test_generate_raw_arity_2
    src = "VALUE func(VALUE self, VALUE obj1, VALUE obj2) {blah;}"

    expected = "static VALUE func(VALUE self, VALUE obj1, VALUE obj2) {blah;}"

    util_generate_raw(src, expected)
  end

  def test_generate_arity_2
    src = "int func(int x, int y) {blah; return x+y;}"

    expected = "static VALUE func(VALUE self, VALUE _x, VALUE _y) {\n  int x = FIX2INT(_x);\n  int y = FIX2INT(_y);\nblah; return INT2FIX(x+y);}"

    util_generate(src, expected)
  end

  def test_generate_raw_arity_3
    src = "VALUE func(VALUE self, VALUE obj1, VALUE obj2, VALUE obj3) {blah;}"

    expected = "static VALUE func(VALUE self, VALUE obj1, VALUE obj2, VALUE obj3) {blah;}"

    util_generate_raw(src, expected)
  end

  def test_generate_arity_too_many
    src = "int too_many(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int aA, int aB, int aC, int aD, int aE, int aF, int ugh) {
  int q = v + w;
  return q+x+y+z;
}"

    assert_raises ArgumentError do
      @builder.generate src, true
    end
  end

  def test_generate_comments
    src = "// stupid cpp comment
    /* stupid c comment */
    int
    add(int x, int y) { // add two numbers
      return x+y;
    }
    "

    expected = "static VALUE add(VALUE self, VALUE _x, VALUE _y) {
  int x = FIX2INT(_x);
  int y = FIX2INT(_y);

      return INT2FIX(x+y);
    }
    "

    util_generate(src, expected)
  end

  def test_generate_local_header
    src = "// stupid cpp comment
#include \"header\"
/* stupid c comment */
int
add(int x, int y) { // add two numbers
  return x+y;
}
"
    # FIX: should be 2 spaces before the return. Can't find problem.
    expected = "#include \"header\"
static VALUE add(VALUE self, VALUE _x, VALUE _y) {
  int x = FIX2INT(_x);
  int y = FIX2INT(_y);

  return INT2FIX(x+y);
}
"
    util_generate(src, expected)
  end

  def test_generate_map_name
    src = "VALUE y_equals(VALUE self) {blah;}"

    expected = "static VALUE y_equals(VALUE self) {blah;}"

    util_generate_raw(src, expected)

    assert_equal [-1, nil, 'y='], @builder.sig['y_equals']
  end

  def test_generate_system_header
    src = "// stupid cpp comment
#include <header>
/* stupid c comment */
int
add(int x, int y) { // add two numbers
  return x+y;
}
"
    expected = "#include <header>
static VALUE add(VALUE self, VALUE _x, VALUE _y) {
  int x = FIX2INT(_x);
  int y = FIX2INT(_y);

  return INT2FIX(x+y);
}
"
    util_generate(src, expected)
  end

  def test_generate_wonky_return
    src = "unsigned\nlong z(void) {return 42}"

    expected = "static VALUE z(VALUE self) {
return ULONG2NUM(42)}"

    util_generate(src, expected)
  end

  def test_generate_compact
    src = "int add(int x, int y) {return x+y}"

    expected = "static VALUE add(VALUE self, VALUE _x, VALUE _y) {
  int x = FIX2INT(_x);
  int y = FIX2INT(_y);
return INT2FIX(x+y)}"

    util_generate(src, expected)
  end

  def test_generate_char_star_normalize
    src = "char\n\*\n  blah(  char*s) {puts(s); return s}"

    expected = "static VALUE blah(VALUE self, VALUE _s) {
  char * s = StringValuePtr(_s);
puts(s); return rb_str_new2(s)}"

    util_generate(src, expected)
  end

  def test_generate_ext
    @builder.c_singleton "VALUE allocate() { return Qnil; }"

    @builder.c "VALUE my_method() { return Qnil; }"

    windoze = "\n  __declspec(dllexport)" if Inline::WINDOZE

    expected = <<-EXT
#include "ruby.h"

# line N "#{$test_inline_path}"
static VALUE allocate(VALUE self) {
 return (Qnil); }

# line N "#{$test_inline_path}"
static VALUE my_method(VALUE self) {
 return (Qnil); }


#ifdef __cplusplus
extern \"C\" {
#endif#{windoze}
  void Init_Inline_TestInline__TestC_eba5() {
    VALUE c = rb_cObject;
    c = rb_const_get(c, rb_intern("TestInline"));
    c = rb_const_get(c, rb_intern("TestC"));

    rb_define_alloc_func(c, (VALUE(*)(VALUE))allocate);
    rb_define_method(c, "my_method", (VALUE(*)(ANYARGS))my_method, 0);

  }
#ifdef __cplusplus
}
#endif
    EXT

    assert_equal expected, util_strip_lines(@builder.generate_ext)
  end

  def test_generate_ext_bad_allocate
    @builder.c_singleton "VALUE allocate(VALUE bad) { return Qnil; }"

    e = assert_raise RuntimeError do
      @builder.generate_ext
    end

    assert_equal 'TestInline::TestC::allocate must have an arity of zero',
                 e.message
  end

  def test_c
    result = @builder.c "int add(int a, int b) { return a + b; }"

    expected = "# line N \"#{$0}\"\nstatic VALUE add(VALUE self, VALUE _a, VALUE _b) {\n  int a = FIX2INT(_a);\n  int b = FIX2INT(_b);\n return INT2FIX(a + b); }"

    result.gsub!(/\# line \d+/, '# line N')

    assert_equal expected, result
    assert_equal [expected], @builder.src
  end

  def test_c_raw
    src = "static VALUE answer_raw(int argc, VALUE *argv, VALUE self) { return INT2NUM(42); }"
    result = @builder.c_raw src.dup

    result.gsub!(/\# line \d+/, '# line N')
    expected = "# line N \"#{$0}\"\n" + src

    assert_equal expected, result
    assert_equal [expected], @builder.src
  end

  def test_map_c_const
    @builder.map_c_const :C_NAME => :int

    expected = [
      "    rb_define_const(c, \"C_NAME\", INT2FIX(C_NAME));"
    ]

    assert_equal expected, @builder.init_extra
  end

  def test_map_c_const_named
    @builder.map_c_const :C_NAME => [:int, :RUBY_NAME]

    expected = [
      "    rb_define_const(c, \"RUBY_NAME\", INT2FIX(C_NAME));"
    ]

    assert_equal expected, @builder.init_extra
  end

  # TODO: fix ?
  def util_simple_code(klassname, c_src)
    result = "
      require 'inline'

      class #{klassname}
        inline do |builder|
          builder.c <<-EOC
            #{c_src}
          EOC
        end
      end"
    result
  end

  def util_test_build(src)
    tempfile = Tempfile.new("util_test_build")
    tempfile.write src
    tempfile.close
    rb_file = tempfile.path + ".rb"
    File.rename tempfile.path, rb_file
    begin
      Kernel.module_eval { require rb_file }
      yield if block_given?
    ensure
      File.unlink rb_file
    end
  end

  def test_build_good
    code = util_simple_code(:DumbTest1, "long dumbpi() { return 314; }")
    util_test_build(code) do
      result = DumbTest1.new.dumbpi
      assert_equal(314, result)
    end
  end

  def test_build_bad
    code = util_simple_code(:DumbTest2, "void should_puke() { 1+1  2+2 }")
    assert_raises(CompilationError) do
      util_test_build(code) do
        flunk
      end
    end
  end

  def util_strip_comments(input)
    expect = 'line 1

#if 0
  line 2
#endif
  line 3

'

    assert_equal expect, @builder.strip_comments(input)
  end

  def test_strip_comments_cpp
    input = 'line 1

#if 0
  line 2
#endif
  // 1 comment
  // 2 comment
  line 3               // trailing comment

'
    util_strip_comments(input)
  end

  def test_strip_comments_c
    input = 'line 1

#if 0
  line 2
#endif
  /*
   * 1 comment
   * 2 comment
   */
  line 3               /* trailing comment */

'
    util_strip_comments(input)
  end

  def test_load
    # totally tested by test_build
  end

end # class TestC
end # class TestInline

module Foo
  class Bar
    # inline stuff will go here...
  end
end

$test_module_code = <<-EOR
module Foo
  class Bar
    inline do |builder|
      builder.c <<-EOC
        static int forty_two_instance() { return 42; }
      EOC
      builder.c_singleton <<-EOC
        static int twenty_four_class() { return 24; }
      EOC
    end
  end
end
EOR

$test_module_code2 = <<-EOR
require 'inline'

# Demonstrates native functions in nested classes and
# extending a class more than once from different ruby
# source files
module Foo
  class Bar
    inline do |builder|
      builder.c <<-EOC
        int twelve_instance() { return 12; }
      EOC
      builder.c_singleton <<-EOC
        int twelve_class() { return 12; }
      EOC
    end
  end
end
EOR

class TestModule < InlineTestCase
  def test_nested
    Object.class_eval $test_module_code
    fb = Foo::Bar.new
    assert_equal(42, fb.forty_two_instance)
    assert_equal(24, Foo::Bar.twenty_four_class)

    tempfile = Tempfile.new("test_inline_nested")
    tempfile.write($test_module_code2)
    tempfile.flush
    tempfile.rewind

    FileUtils.cp tempfile.path, "#{tempfile.path}.rb"

    require "#{tempfile.path}.rb"
    assert_equal(12, fb.twelve_instance)
    assert_equal(12, Foo::Bar.twelve_class)

    FileUtils.rm "#{tempfile.path}.rb"
  end

  def test_argument_check_good
    util_arity_check
    fb = Foo::Bar.new
    assert_equal 13, fb.arity6(1, 2, 3, 4, 5, "blah")
  end

  def test_argument_check_fewer
    util_arity_check
    fb = Foo::Bar.new

    assert_raises ArgumentError do
      fb.arity6(1, 2, 3)
    end
  end

  def test_argument_check_more
    util_arity_check
    fb = Foo::Bar.new
    assert_raises ArgumentError do
      fb.arity6(1, 2, 3, 4, 5, "blah", :extra)
    end
  end

  def test_inline
    self.class.inline(:C) do |builder|
      builder.c "int add(int a, int b) { return a + b; }"
    end
    assert(test(?d, Inline.directory),
       "inline dir should have been created")
    matches = Dir[File.join(Inline.directory, "Inline_TestModule_*.c")]
    assert_equal(1, matches.length, "Source should have been created")
    library_file = matches.first.gsub(/\.c$/) { "." + Config::CONFIG["DLEXT"] }
    assert(test(?f, library_file),
       "Library file should have been created")
  end

  def util_arity_check
    methods = Foo::Bar.public_instance_methods.map { |s| s.to_s }

    unless methods.include? "arity6" then
      Foo::Bar.inline do |builder|
        builder.include "<string.h>"
        builder.c "int arity6(int u, int v, int w, int x, int y, char * z) { return x + y + strlen(z); }"
      end
    end
  end
end
