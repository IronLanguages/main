module ThinTestSlice
  
  # All Slice code is expected to be namespaced inside this module.
  
  class Application < Merb::Controller
    
    controller_for_slice
    
    private
    
    # Construct a path relative to the public directory
    def public_path_for(type, *segments)
      ::ThinTestSlice.public_path_for(type, *segments)
    end
    
    # Construct an app-level path.
    def app_path_for(type, *segments)
      ::ThinTestSlice.app_path_for(type, *segments)
    end
    
    # Construct a slice-level path
    def slice_path_for(type, *segments)
      ::ThinTestSlice.slice_path_for(type, *segments)
    end
    
  end
  
  class Main < Application
    
    def index
      render "#{slice.description} (v. #{slice.version})"
    end
    
  end
  
end