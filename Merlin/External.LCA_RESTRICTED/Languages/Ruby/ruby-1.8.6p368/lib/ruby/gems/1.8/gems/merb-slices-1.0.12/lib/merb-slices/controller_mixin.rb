module Merb
  module Slices
    
    module Support
      
      # This module should be explicitly included into a controller,
      # for example in Application (the main controller of your app).
      # It contains optional methods for slice/plugin developers.
      
      # Generate a slice url - takes the slice's :path_prefix into account.
      #
      # @param slice_name<Symbol> 
      #   The name of the slice - in identifier_sym format (underscored).
      # @param *args<Array[Symbol,Hash]> 
      #   There are several possibilities regarding arguments:
      #   - when passing a Hash only, the :default route of the current 
      #     slice will be used
      #   - when a Symbol is passed, it's used as the route name
      #   - a Hash with additional params can optionally be passed
      # 
      # @return <String> A uri based on the requested slice.
      #
      # @example slice_url(:awesome, :format => 'html')
      # @example slice_url(:forum, :posts, :format => 'xml')          
      def slice_url(slice_name, *args)
        opts = args.last.is_a?(Hash) ? args.pop : {}
        route_name = args[0].is_a?(Symbol) ? args.shift : :default
        
        routes = Merb::Slices.named_routes[slice_name]
        unless routes && route = routes[route_name]
          raise Merb::Router::GenerationError, "Named route not found: #{route_name}"
        end
        
        args.push(opts)
        route.generate(args, params)
      end
      
    end
    
    module ControllerMixin
      
      def self.included(klass)
        klass.extend ClassMethods
      end
      
      module ClassMethods
        
        # Setup a controller to reference a slice and its template roots
        #
        # This method is available to any class inheriting from Merb::AbstractController;
        # it enabled correct location of templates, as well as access to the slice module.
        #
        # @param slice_module<#to_s> The slice module to use; defaults to current module.
        # @param options<Hash> 
        #   Optional parameters to set which component path is used (defaults to :view) and
        #   the :path option lets you specify a subdirectory of that component path.
        #   When :layout is set, then this is used instead of the config's :layout setting.
        #
        # @example controller_for_slice # uses current module
        # @example controller_for_slice SliceMod # defaults to :view templates and no subdirectory
        # @example controller_for_slice :templates_for => :mailer, :path => 'views' # for Merb::Mailer
        # @example controller_for_slice SliceMod, :templates_for => :mailer, :path => 'views' # for Merb::Mailer
        def controller_for_slice(slice_module = nil, options = {})
          options, slice_module = slice_module.merge(options), nil if slice_module.is_a?(Hash)
          slice_module ||= self.name.split('::').first
          options[:templates_for] = :view unless options.key?(:templates_for)
          if slice_mod = Merb::Slices[slice_module.to_s]
            # Include the instance methods
            unless self.kind_of?(Merb::Slices::ControllerMixin::MixinMethods)
              self.send(:extend, Merb::Slices::ControllerMixin::MixinMethods)
            end
            # Reference this controller's slice module
            self.class_inheritable_accessor :slice, :instance_writer => false
            self.slice = slice_mod
            # Setup template roots
            if options[:templates_for]
              self._template_root  = join_template_path(slice_mod.dir_for(options[:templates_for]), options[:path])
              self._template_roots = []
              # app-level app/views directory for shared and fallback views, layouts and partials
              self._template_roots << [join_template_path(Merb.dir_for(options[:templates_for]), options[:path]), :_template_location] if Merb.dir_for(options[:templates_for])
              # slice-level app/views for the standard supplied views
              self._template_roots << [self._template_root, :_slice_template_location] 
              # app-level slices/<slice>/app/views for specific overrides
              self._template_roots << [join_template_path(slice_mod.app_dir_for(options[:templates_for]), options[:path]), :_slice_template_location]
              # additional template roots for specific overrides (optional)
              self._template_roots += Array(options[:template_roots]) if options[:template_roots]
            end
            # Set the layout for this slice controller
            layout_for_slice(options[:layout])
          end
        end
        
        private
        
        def join_template_path(*segments)
          File.join(*segments.compact)
        end
        
      end
      
      module MixinMethods
        
        def self.extended(klass)
          klass.send(:include, InstanceMethods)
          klass.hide_action :slice if klass.respond_to?(:hide_action)
        end
        
        # Use the slice's layout - defaults to underscored identifier.
        #
        # This is set for generated stubs that support layouts.
        #
        # @param layout<#to_s> The layout name to use.
        def layout_for_slice(layout = nil)
          layout(layout || self.slice.config[:layout]) if layout || self.slice.config.key?(:layout)
        end
        
        module InstanceMethods
      
          # Reference this controller's slice module directly.
          #
          # @return <Module> A slice module.
          def slice; self.class.slice; end
          
          # Generate a url - takes the slice's :path_prefix into account.
          #
          # @param *args<Array[Symbol,Hash]> 
          #   There are several possibilities regarding arguments:
          #   - when passing a Hash only, the :default route of the current 
          #     slice will be used
          #   - when a single Symbol is passed, it's used as the route name,
          #     while the slice itself will be the current one
          #   - when two Symbols are passed, the first will be the slice name,
          #     the second will be the route name
          #   - a Hash with additional params can optionally be passed
          # 
          # @return <String> A uri based on the requested slice.
          #
          # @example slice_url(:controller => 'foo', :action => 'bar')
          # @example slice_url(:awesome, :format => 'html')
          # @example slice_url(:forum, :posts, :format => 'xml')          
          def slice_url(*args)
            opts = args.last.is_a?(Hash) ? args.pop : {}
            slice_name, route_name = if args[0].is_a?(Symbol) && args[1].is_a?(Symbol)
              [args.shift, args.shift] # other slice identifier, route name
            elsif args[0].is_a?(Symbol)
              [slice.identifier_sym, args.shift] # self, route name
            else
              [slice.identifier_sym, :default] # self, default route
            end
            
            routes = Merb::Slices.named_routes[slice_name]
            unless routes && route = routes[route_name]
              raise Merb::Router::GenerationError, "Named route not found: #{route_name}"
            end
            
            args.push(opts)
            route.generate(args, params)
          end

          private
  
          # This is called after the controller is instantiated to figure out where to
          # for templates under the _template_root. This helps the controllers
          # of a slice to locate templates without looking in a subdirectory with
          # the name of the module. Instead it will just be app/views/controller/*
          #
          # @param context<#to_str> The controller context (the action or template name).
          # @param type<#to_str> The content type. Defaults to nil.
          # @param controller<#to_str> The name of the controller. Defaults to controller_name.
          #
          # @return <String> 
          #   Indicating where to look for the template for the current controller,
          #   context, and content-type.
          def _slice_template_location(context, type = nil, controller = controller_name)
            if controller && controller.include?('/')
              # skip first segment if given (which is the module name)
              segments = controller.split('/')
              "#{segments[1,segments.length-1].join('/')}/#{context}.#{type}"
            else
              # default template location logic
              _template_location(context, type, controller)
            end
          end
          
        end
      
      end

    end
    
  end
end