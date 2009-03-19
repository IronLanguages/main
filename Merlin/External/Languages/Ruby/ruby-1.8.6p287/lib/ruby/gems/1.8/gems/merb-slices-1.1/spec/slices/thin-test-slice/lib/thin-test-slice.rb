if defined?(Merb::Plugins)

  $:.unshift File.dirname(__FILE__)

  dependency 'merb-slices', :immediate => true
  Merb::Plugins.add_rakefiles "thin-test-slice/merbtasks", "thin-test-slice/slicetasks"

  # Register the Slice for the current host application
  Merb::Slices::register(__FILE__)
  
  # Slice configuration - set this in a before_app_loads callback.
  # By default a Slice uses its own layout.
  Merb::Slices::config[:thin_test_slice][:layout] ||= :thin_test_slice
  
  # All Slice code is expected to be namespaced inside a module
  module ThinTestSlice
    
    # Slice metadata
    self.description = "ThinTestSlice is a thin Merb slice!"
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
    
    # Deactivation hook - triggered by Merb::Slices.deactivate(ThinTestSlice)
    def self.deactivate
    end
    
    # Setup routes inside the host application
    #
    # @param scope<Merb::Router::Behaviour>
    #  Routes will be added within this scope (namespace). In fact, any 
    #  router behaviour is a valid namespace, so you can attach
    #  routes at any level of your router setup.
    #
    # @note prefix your named routes with :thin_test_slice_
    #   to avoid potential conflicts with global named routes.
    def self.setup_router(scope)
      # enable slice-level default routes by default
      scope.default_routes
    end
    
    # This sets up a thin slice's structure.
    def self.setup_default_structure!
      self.push_app_path(:root, Merb.root / 'slices' / self.identifier, nil)
      
      self.push_path(:stub, root_path('stubs'), nil)
      self.push_app_path(:stub, app_dir_for(:root), nil)
      
      self.push_path(:application, root, 'application.rb')
      self.push_app_path(:application, app_dir_for(:root), 'application.rb')
      
      self.push_path(:view, dir_for(:application) / "views")
      self.push_app_path(:view, app_dir_for(:application) / "views")
      
      self.push_path(:public, root_path('public'), nil)
      self.push_app_path(:public, Merb.root / 'public' / 'slices' / self.identifier, nil)
      
      public_components.each do |component|
        self.push_path(component, dir_for(:public) / "#{component}s", nil)
        self.push_app_path(component, app_dir_for(:public) / "#{component}s", nil)
      end
    end
    
  end
  
  # Setup the slice layout for ThinTestSlice
  #
  # Use ThinTestSlice.push_path and ThinTestSlice.push_app_path
  # to set paths to thin-test-slice-level and app-level paths. Example:
  #
  # ThinTestSlice.push_path(:application, ThinTestSlice.root)
  # ThinTestSlice.push_app_path(:application, Merb.root / 'slices' / 'thin-test-slice')
  # ...
  #
  # Any component path that hasn't been set will default to ThinTestSlice.root
  #
  # For a thin slice we just add application.rb, :view and :public locations.
  ThinTestSlice.setup_default_structure!
  
  # Add dependencies for other ThinTestSlice classes below. Example:
  # dependency "thin-test-slice/other"
  
end