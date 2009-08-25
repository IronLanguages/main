require 'merb-slices/module'

if defined?(Merb::Plugins)
  
  Merb::Plugins.add_rakefiles "merb-slices/merbtasks"
  
  Merb::Plugins.config[:merb_slices] ||= {}
  
  require "merb-slices/module_mixin"
  require "merb-slices/controller_mixin"
  require "merb-slices/router_ext"
  
  # Enable slice behaviour for any class inheriting from AbstractController.
  # To use this call controller_for_slice in your controller class.
  Merb::AbstractController.send(:include, Merb::Slices::ControllerMixin)
  
  # Load Slice classes before the app's classes are loaded.
  #
  # This allows the application to override/merge any slice-level classes.
  class Merb::Slices::Loader < Merb::BootLoader

    before LoadClasses

    class << self

      # Gather all slices from search path and gems and load their classes.
      def run
        Merb::Slices.register_slices_from_search_path! if auto_register?
        Merb::Slices.each_slice { |slice| slice.load_slice }
      end
      
      # Load a single file and its requirements.
      #
      # @param file<String> The file to load.
      def load_file(file)
        Merb::BootLoader::LoadClasses.load_file file
      end
      
      # Remove a single file and the classes loaded by it from ObjectSpace.
      #
      # @param file<String> The file to load.
      def remove_classes_in_file(file)
        Merb::BootLoader::LoadClasses.remove_classes_in_file file
      end
        
      # Load classes from given paths - using path/glob pattern.
      #
      # @param *paths <Array> Array of paths to load classes from - may contain glob pattern
      def load_classes(*paths)
        Merb::BootLoader::LoadClasses.load_classes paths
      end
    
      # Reload the router - takes all_slices into account to load slices at runtime.
      def reload_router!
        Merb::BootLoader::Router.reload!
      end
      
      # Slice-level paths for all loaded slices.
      #
      # @return <Array[String]> Any slice-level paths that have been loaded.
      def slice_paths
        paths = []
        Merb::Slices.each_slice { |slice| paths += slice.collected_slice_paths }
        paths
      end
      
      # App-level paths for all loaded slices.
      #
      # @return <Array[String]> Any app-level paths that have been loaded.
      def app_paths
        paths = []
        Merb::Slices.each_slice { |slice| paths += slice.collected_app_paths }
        paths
      end
      
      private
      
      # Whether slices from search paths should be registered automatically.
      # Defaults to true if not explicitly set.
      def auto_register?
        Merb::Plugins.config[:merb_slices][:auto_register] || !Merb::Plugins.config[:merb_slices].key?(:auto_register)
      end
      
    end

  end
  
  # Call initialization method for each registered Slice.
  #
  # This is done just before the app's after_load_callbacks are run.
  # The application has been practically loaded completely, letting
  # the callbacks work with what has been loaded.
  class Merb::Slices::Initialize < Merb::BootLoader
  
    before AfterAppLoads
  
    def self.run
      Merb::Slices.each_slice do |slice|
        Merb.logger.verbose!("Initializing slice '#{slice}' ...") 
        slice.init if slice.respond_to?(:init)
      end
    end
  
  end
  
  # Call activation method for each registered Slice.
  #
  # This is done right after the app's after_load_callbacks are run.
  # Any settings can be taken into account in the activation step.
  #
  # @note Activation will only take place if the slice has been routed;
  #   this means you need have at least one slice route setup.
  class Merb::Slices::Activate < Merb::BootLoader
  
    after AfterAppLoads
  
    def self.run
      Merb::Slices.each_slice do |slice|
        Merb.logger.info!("Activating slice '#{slice}' ...")
        slice.activate if slice.respond_to?(:activate) && slice.routed?
      end
    end
  
  end
  
end