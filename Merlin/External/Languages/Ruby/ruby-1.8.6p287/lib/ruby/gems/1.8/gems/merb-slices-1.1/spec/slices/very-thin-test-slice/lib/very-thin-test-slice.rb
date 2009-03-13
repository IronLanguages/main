if defined?(Merb::Plugins)

  $:.unshift File.dirname(__FILE__)

  dependency 'merb-slices', :immediate => true
  Merb::Plugins.add_rakefiles "very-thin-test-slice/merbtasks", "very-thin-test-slice/slicetasks"

  # Register the Slice for the current host application
  Merb::Slices::register(__FILE__)
  
  # Slice configuration - set this in a before_app_loads callback.
  Merb::Slices::config[:very_thin_test_slice][:foo] ||= :bar
  
  # All Slice code is expected to be namespaced inside a module
  module VeryThinTestSlice
    
    # Slice metadata
    self.description = "VeryThinTestSlice is a very thin Merb slice!"
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
    
    # Deactivation hook - triggered by Merb::Slices.deactivate(VeryThinTestSlice)
    def self.deactivate
    end
    
    # Setup routes inside the host application
    #
    # @param scope<Merb::Router::Behaviour>
    #  Routes will be added within this scope (namespace). In fact, any 
    #  router behaviour is a valid namespace, so you can attach
    #  routes at any level of your router setup.
    #
    # @note prefix your named routes with :very_thin_test_slice_
    #   to avoid potential conflicts with global named routes.
    def self.setup_router(scope)
      # enable slice-level default routes by default
      scope.default_routes
    end
    
    # This sets up a very thin slice's structure.
    def self.setup_default_structure!
      self.push_app_path(:root, Merb.root / 'slices' / self.identifier, nil)
      
      self.push_path(:stub, root_path('stubs'), nil)
      self.push_app_path(:stub, app_dir_for(:root), nil)
      
      self.push_path(:application, root, 'application.rb')
      self.push_app_path(:application, app_dir_for(:root), 'application.rb')
            
      self.push_path(:public, root_path('public'), nil)
      self.push_app_path(:public, Merb.root / 'public' / 'slices' / self.identifier, nil)
      
      public_components.each do |component|
        self.push_path(component, dir_for(:public) / "#{component}s", nil)
        self.push_app_path(component, app_dir_for(:public) / "#{component}s", nil)
      end
    end
    
  end
  
  # Setup the slice layout for VeryThinTestSlice
  #
  # Use VeryThinTestSlice.push_path and VeryThinTestSlice.push_app_path
  # to set paths to very-thin-test-slice-level and app-level paths. Example:
  #
  # VeryThinTestSlice.push_path(:application, VeryThinTestSlice.root)
  # VeryThinTestSlice.push_app_path(:application, Merb.root / 'slices' / 'very-thin-test-slice')
  # ...
  #
  # Any component path that hasn't been set will default to VeryThinTestSlice.root
  #
  # For a very thin slice we just add application.rb and :public locations.
  VeryThinTestSlice.setup_default_structure!
  
  # Add dependencies for other VeryThinTestSlice classes below. Example:
  # dependency "very-thin-test-slice/other"
  
end