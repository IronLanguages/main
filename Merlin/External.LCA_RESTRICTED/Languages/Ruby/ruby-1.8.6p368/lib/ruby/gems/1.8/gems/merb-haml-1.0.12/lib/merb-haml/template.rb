module Merb::Template

  class Haml

    # Defines a method for calling a specific HAML template.
    #
    # ==== Parameters
    # path<String>:: Path to the template file.
    # name<~to_s>:: The name of the template method.
    # locals<Array[Symbol]>:: A list of locals to assign from the args passed into the compiled template.
    # mod<Class, Module>::
    #   The class or module wherein this method should be defined.
    def self.compile_template(io, name, locals, mod)
      path = File.expand_path(io.path)
      config = (Merb::Plugins.config[:haml] || {}).inject({}) do |c, (k, v)|
        c[k.to_sym] = v
        c
      end.merge :filename => path
      template = ::Haml::Engine.new(io.read, config)
      template.def_method(mod, name, *locals)
      name
    end
  
    module Mixin
      # ==== Parameters
      # string<String>:: The string to add to the HAML buffer.
      # binding<Binding>::
      #   Not used by HAML, but is necessary to conform to the concat_*
      #   interface.
      def concat_haml(string, binding)
        haml_buffer.buffer << string
      end
      
    end
    Merb::Template.register_extensions(self, %w[haml])  
  end
end

module Haml
  class Engine

    # ==== Parameters
    # object<Class, Module>::
    #   The class or module wherein this method should be defined.
    # name<~to_s>:: The name of the template method.
    # *local_names:: Local names to define in the HAML template.
    def def_method(object, name, *local_names)
      method = object.is_a?(Module) ? :module_eval : :instance_eval

      setup = "@_engine = 'haml'"

      object.send(method, "def #{name}(_haml_locals = {}); #{setup}; #{precompiled_with_ambles(local_names)}; end",
                  @options[:filename], 0)
    end
 
  end
end
