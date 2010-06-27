module Merb::InlineTemplates
end

module Merb::Template
  
  EXTENSIONS            = {} unless defined?(EXTENSIONS)
  METHOD_LIST           = {} unless defined?(METHOD_LIST)
  SUPPORTED_LOCALS_LIST = Hash.new([].freeze) unless defined?(SUPPORTED_LOCALS_LIST)
  MTIMES                = {} unless defined?(MTIMES)
  
  class << self
    # Get the template's method name from a full path. This replaces
    # non-alphanumeric characters with __ and "." with "_"
    #
    # Collisions are potentially possible with something like:
    # ~foo.bar and __foo.bar or !foo.bar.
    #
    # ==== Parameters
    # path<String>:: A full path to convert to a valid Ruby method name
    #
    # ==== Returns
    # String:: The template name.
    #
    #
    # We might want to replace this with something that varies the
    # character replaced based on the non-alphanumeric character
    # to avoid edge-case collisions.
    #
    # :api: private
    def template_name(path)
      path = File.expand_path(path)      
      path.gsub(/[^\.a-zA-Z0-9]/, "__").gsub(/\./, "_")
    end

    chainable do
      # For a given path, get an IO object that responds to #path.
      #
      # This is so that plugins can override this if they provide
      # mechanisms for specifying templates that are not just simple
      # files. The plugin is responsible for ensuring that the fake
      # path provided will work with #template_for, and thus the
      # RenderMixin in general.
      #
      # ==== Parameters
      # path<String>:: A full path to find a template for. This is the
      #   path that the RenderMixin assumes it should find the template
      #   in.
      # 
      # ==== Returns
      # IO#path:: An IO object that responds to path (File or VirtualFile).
      #
      # :api: plugin
      # @overridable
      def load_template_io(path)
        file = Dir["#{path}.{#{template_extensions.join(',')}}"].first
        File.open(file, "r") if file
      end
    end

    # Get the name of the template method for a particular path.
    #
    # ==== Parameters
    # path<String>:: A full path to find a template method for.
    # template_stack<Array>:: The template stack. Not used.
    # locals<Array[Symbol]>:: The names of local variables
    #
    # ==== Returns
    # <String>:: name of the method that inlines the template.
    #
    # :api: private
    def template_for(path, template_stack = [], locals=[])
      path = File.expand_path(path)
      
      if needs_compilation?(path, locals)
        template_io = load_template_io(path)
        METHOD_LIST[path] = inline_template(template_io, locals) if template_io
      end
      
      METHOD_LIST[path]
    end
    
    # Decide if a template needs to be re/compiled.
    #
    # ==== Parameters
    # path<String>:: The full path of the template to check support for.
    # locals<Array[Symbol]>:: The list of locals that need to be supported
    #
    # ==== Returns
    # Boolean:: Whether or not the template for the provided path needs to be recompiled
    #
    # :api: private
    def needs_compilation?(path, locals)
      return true if Merb::Config[:reload_templates] || !METHOD_LIST[path]
      
      current_locals = SUPPORTED_LOCALS_LIST[path]
      current_locals != locals &&
        !(locals - current_locals).empty?
    end
    
    # Get all known template extensions
    #
    # ==== Returns
    #   Array:: Extension strings.
    #
    # :api: plugin
    def template_extensions
      EXTENSIONS.keys
    end
    
    # Takes a template at a particular path and inlines it into a module and
    # adds it to the METHOD_LIST table to speed lookup later.
    # 
    # ==== Parameters
    # io<#path>::
    #   An IO that responds to #path (File or VirtualFile)
    # locals<Array[Symbol]>::
    #   A list of local names that should be assigned in the template method
    #   from the arguments hash. Defaults to [].
    # mod<Module>::
    #   The module to put the compiled method into. Defaults to
    #   Merb::InlineTemplates
    #
    # ==== Returns
    # Symbol:: The name of the method that the template was compiled into.
    #
    # ==== Notes
    # Even though this method supports inlining into any module, the method
    # must be available to instances of AbstractController that will use it.
    #
    # :api: private
    def inline_template(io, locals=[], mod = Merb::InlineTemplates)
      full_file_path = File.expand_path(io.path)
      engine_neutral_path = full_file_path.gsub(/\.[^\.]*$/, "")
      
      local_list = (SUPPORTED_LOCALS_LIST[engine_neutral_path] |= locals)
      ret = METHOD_LIST[engine_neutral_path] =
        engine_for(full_file_path).compile_template(io, 
        template_name(full_file_path), local_list, mod)
        
      io.close
      ret
    end
    
    # Finds the engine for a particular path.
    # 
    # ==== Parameters
    # path<String>:: The path of the file to find an engine for.
    #
    # ==== Returns
    # Class:: The engine.
    #
    # :api: private
    def engine_for(path)
      path = File.expand_path(path)      
      EXTENSIONS[path.match(/\.([^\.]*)$/)[1]]
    end
    
    # Registers the extensions that will trigger a particular templating
    # engine.
    # 
    # ==== Parameters
    # engine<Class>:: The class of the engine that is being registered
    # extensions<Array[String]>:: 
    #   The list of extensions that will be registered with this templating
    #   language
    #
    # ==== Raises
    # ArgumentError:: engine does not have a compile_template method.
    #
    # ==== Returns
    # nil
    #
    # ==== Example
    #   Merb::Template.register_extensions(Merb::Template::Erubis, ["erb"])
    #
    # :api: plugin
    def register_extensions(engine, extensions) 
      raise ArgumentError, "The class you are registering does not have a compile_template method" unless
        engine.respond_to?(:compile_template)
      extensions.each{|ext| EXTENSIONS[ext] = engine }
      Merb::AbstractController.class_eval <<-HERE
        include #{engine}::Mixin
      HERE
    end
  end
  
  require 'erubis'

  class Erubis    
    # ==== Parameters
    # io<#path>:: An IO containing the full path of the template.
    # name<String>:: The name of the method that will be created.
    # locals<Array[Symbol]>:: A list of locals to assign from the args passed into the compiled template.
    # mod<Module>:: The module that the compiled method will be placed into.
    #
    # :api: private
    def self.compile_template(io, name, locals, mod)
      template = ::Erubis::BlockAwareEruby.new(io.read)
      _old_verbose, $VERBOSE = $VERBOSE, nil
      assigns = locals.inject([]) do |assigns, local|
        assigns << "#{local} = _locals[#{local.inspect}]"
      end.join(";")
      
      code = "def #{name}(_locals={}); #{assigns}; #{template.src}; end"
      mod.module_eval code, File.expand_path(io.path)
      $VERBOSE = _old_verbose
      
      name
    end

    module Mixin
      
      # ==== Parameters
      # *args:: Arguments to pass to the block.
      # &block:: The template block to call.
      #
      # ==== Returns
      # String:: The output of the block.
      #
      # ==== Examples
      # Capture being used in a .html.erb page:
      # 
      #   <% @foo = capture do %>
      #     <p>Some Foo content!</p> 
      #   <% end %>
      #
      # :api: private
      def capture_erb(*args, &block)
        _old_buf, @_erb_buf = @_erb_buf, ""
        block.call(*args)
        ret = @_erb_buf
        @_erb_buf = _old_buf
        ret
      end

      # :api: private
      def concat_erb(string, binding)
        @_erb_buf << string
      end
            
    end
  
    Merb::Template.register_extensions(self, %w[erb])    
  end
  
end

module Erubis
  module BlockAwareEnhancer
    # :api: private
    def add_preamble(src)
      src << "_old_buf, @_erb_buf = @_erb_buf, ''; "
      src << "@_engine = 'erb'; "
    end

    # :api: private
    def add_postamble(src)
      src << "\n" unless src[-1] == ?\n      
      src << "_ret = @_erb_buf; @_erb_buf = _old_buf; _ret.to_s;\n"
    end

    # :api: private
    def add_text(src, text)
      src << " @_erb_buf.concat('" << escape_text(text) << "'); "
    end

    # :api: private
    def add_expr_escaped(src, code)
      src << ' @_erb_buf.concat(' << escaped_expr(code) << ');'
    end
    
    # :api: private
    def add_stmt2(src, code, tailch)
      src << code
      src << " ).to_s; " if tailch == "="
      src << ';' unless code[-1] == ?\n
    end
    
    # :api: private
    def add_expr_literal(src, code)
      if code =~ /(do|\{)(\s*\|[^|]*\|)?\s*\Z/
        src << ' @_erb_buf.concat( ' << code << "; "
      else
        src << ' @_erb_buf.concat((' << code << ').to_s);'
      end
    end
  end

  class BlockAwareEruby < Eruby
    include BlockAwareEnhancer
  end
  
  # module RubyEvaluator
  # 
  #   # DOC
  #   def def_method(object, method_name, filename=nil)
  #     m = object.is_a?(Module) ? :module_eval : :instance_eval
  #     setup = "@_engine = 'erb'"
  #     object.__send__(m, "def #{method_name}(locals={}); #{setup}; #{@src}; end", filename || @filename || '(erubis)')
  #   end
  #  
  # end
end
