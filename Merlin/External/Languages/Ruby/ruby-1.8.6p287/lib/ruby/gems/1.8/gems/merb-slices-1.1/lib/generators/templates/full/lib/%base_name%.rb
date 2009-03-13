if defined?(Merb::Plugins)

  $:.unshift File.dirname(__FILE__)

  dependency 'merb-slices', :immediate => true
  Merb::Plugins.add_rakefiles "<%= base_name %>/merbtasks", "<%= base_name %>/slicetasks", "<%= base_name %>/spectasks"

  # Register the Slice for the current host application
  Merb::Slices::register(__FILE__)
  
  # Slice configuration - set this in a before_app_loads callback.
  # By default a Slice uses its own layout, so you can swicht to 
  # the main application layout or no layout at all if needed.
  # 
  # Configuration options:
  # :layout - the layout to use; defaults to :<%= base_name %>
  # :mirror - which path component types to use on copy operations; defaults to all
  Merb::Slices::config[:<%= symbol_name %>][:layout] ||= :<%= symbol_name %>
  
  # All Slice code is expected to be namespaced inside a module
  module <%= module_name %>
    
    # Slice metadata
    self.description = "<%= module_name %> is a chunky Merb slice!"
    self.version = "0.0.1"
    self.author = "Engine Yard"
    
    # Stub classes loaded hook - runs before LoadClasses BootLoader
    # right after a slice's classes have been loaded internally.
    def self.loaded
    end
    
    # Initialization hook - runs before AfterAppLoads BootLoader
    def self.init
    end
    
    # Activation hook - runs after AfterAppLoads BootLoader
    def self.activate
    end
    
    # Deactivation hook - triggered by Merb::Slices.deactivate(<%= module_name %>)
    def self.deactivate
    end
    
    # Setup routes inside the host application
    #
    # @param scope<Merb::Router::Behaviour>
    #  Routes will be added within this scope (namespace). In fact, any 
    #  router behaviour is a valid namespace, so you can attach
    #  routes at any level of your router setup.
    #
    # @note prefix your named routes with :<%= symbol_name %>_
    #   to avoid potential conflicts with global named routes.
    def self.setup_router(scope)
      # example of a named route
      scope.match('/index(.:format)').to(:controller => 'main', :action => 'index').name(:index)
      # the slice is mounted at /<%= base_name %> - note that it comes before default_routes
      scope.match('/').to(:controller => 'main', :action => 'index').name(:home)
      # enable slice-level default routes by default
      scope.default_routes
    end
    
  end
  
  # Setup the slice layout for <%= module_name %>
  #
  # Use <%= module_name %>.push_path and <%= module_name %>.push_app_path
  # to set paths to <%= base_name %>-level and app-level paths. Example:
  #
  # <%= module_name %>.push_path(:application, <%= module_name %>.root)
  # <%= module_name %>.push_app_path(:application, Merb.root / 'slices' / '<%= base_name %>')
  # ...
  #
  # Any component path that hasn't been set will default to <%= module_name %>.root
  #
  # Or just call setup_default_structure! to setup a basic Merb MVC structure.
  <%= module_name %>.setup_default_structure!
  
  # Add dependencies for other <%= module_name %> classes below. Example:
  # dependency "<%= base_name %>/other"
  
end