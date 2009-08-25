module Merb
  module Slices
    
    VERSION = "0.9.8"
    
    class << self
      
      # Retrieve a slice module by name 
      #
      # @param <#to_s> The slice module to check for.
      # @return <Module> The slice module itself.
      def [](module_name)
        Object.full_const_get(module_name.to_s) if exists?(module_name)
      end
      
      # Helper method to transform a slice filename to a module Symbol
      def filename2module(slice_file)
        File.basename(slice_file, '.rb').gsub('-', '_').camel_case.to_sym
      end
    
      # Register a Slice by its gem/lib path for loading at startup
      #
      # This is referenced from gems/<slice-gem-x.x.x>/lib/<slice-gem>.rb
      # Which gets loaded for any gem. The name of the file is used
      # to extract the Slice module name.
      #
      # @param slice_file<String> The path of the gem 'init file'
      # @param force<Boolean> Whether to overwrite currently registered slice or not.
      #
      # @return <Module> The Slice module that has been setup.
      #
      # @example Merb::Slices::register(__FILE__)
      # @example Merb::Slices::register('/path/to/my-slice/lib/my-slice.rb')
      def register(slice_file, force = true)
        # do what filename2module does, but with intermediate variables
        identifier  = File.basename(slice_file, '.rb')
        underscored = identifier.gsub('-', '_')
        module_name = underscored.camel_case
        slice_path  = File.expand_path(File.dirname(slice_file) + '/..')
        # check if slice_path exists instead of just the module name - more flexible
        if !self.paths.include?(slice_path) || force
          Merb.logger.verbose!("Registered slice '#{module_name}' located at #{slice_path}") if force
          self.files[module_name] = slice_file
          self.paths[module_name] = slice_path
          slice_mod = setup_module(module_name)
          slice_mod.identifier = identifier
          slice_mod.identifier_sym = underscored.to_sym
          slice_mod.root = slice_path
          slice_mod.file = slice_file
          slice_mod.registered
          slice_mod
        else
          Merb.logger.info!("Already registered slice '#{module_name}' located at #{slice_path}")
          Object.full_const_get(module_name)
        end
      end
      
      # Look for any slices in Merb.root / 'slices' (the default) or if given, 
      # Merb::Plugins.config[:merb_slices][:search_path] (String/Array)
      def register_slices_from_search_path!
        slice_files_from_search_path.each do |slice_file|
          absolute_path = File.expand_path(slice_file)
          Merb.logger.info!("Found slice '#{File.basename(absolute_path, '.rb')}' in search path at #{absolute_path.relative_path_from(Merb.root)}")
          Merb::Slices::Loader.load_classes(absolute_path)
        end
      end
      
      # Unregister a Slice at runtime
      #
      # This clears the slice module from ObjectSpace and reloads the router.
      # Since the router doesn't add routes for any disabled slices this will
      # correctly reflect the app's routing state.
      #
      # @param slice_module<#to_s> The Slice module to unregister.
      def unregister(slice_module)
        if (slice = self[slice_module]) && self.paths.delete(module_name = slice.name)
          slice.loadable_files.each { |file| Merb::Slices::Loader.remove_classes_in_file file }
          Object.send(:remove_const, module_name)
          unless Object.const_defined?(module_name)
            Merb.logger.info!("Unregistered slice #{module_name}")
            Merb::Slices::Loader.reload_router!
          end
        end
      end
      
      # Activate a Slice module at runtime
      #
      # Looks for previously registered slices; then searches :search_path for matches.
      #
      # @param slice_module<#to_s> Usually a string of version of the slice module name.
      def activate(slice_module)  
        unless slice_file = self.files[slice_module.to_s]
          module_name_underscored = slice_module.to_s.snake_case.escape_regexp
          module_name_dasherized  = module_name_underscored.tr('_', '-').escape_regexp
          regexp = Regexp.new(/\/(#{module_name_underscored}|#{module_name_dasherized})\/lib\/(#{module_name_underscored}|#{module_name_dasherized})\.rb$/)
          slice_file = slice_files_from_search_path.find { |path| path.match(regexp) } # from search path(s)
        end
        activate_by_file(slice_file) if slice_file
      rescue => e
        Merb.logger.error!("Failed to activate slice #{slice_module} (#{e.message})")
      end
      
      # Register a Slice by its gem/lib init file path and activate it at runtime
      #
      # Normally slices are loaded using BootLoaders on application startup.
      # This method gives you the possibility to add slices at runtime, all
      # without restarting your app. Together with #deactivate it allows
      # you to enable/disable slices at any time. The router is reloaded to
      # incorporate any changes. Disabled slices will be skipped when 
      # routes are regenerated.
      #
      # @param slice_file<String> The path of the gem 'init file'
      #
      # @example Merb::Slices.activate_by_file('/path/to/gems/slice-name/lib/slice-name.rb')
      def activate_by_file(slice_file)
        Merb::Slices::Loader.load_classes(slice_file)
        slice = register(slice_file, false) # just to get module by slice_file
        slice.load_slice # load the slice
        Merb::Slices::Loader.reload_router!
        slice.init     if slice.respond_to?(:init)
        slice.activate if slice.respond_to?(:activate) && slice.routed?
        slice
      rescue
        Merb::Slices::Loader.reload_router!
      end
      alias :register_and_load :activate_by_file
      
      # Deactivate a Slice module at runtime
      #
      # @param slice_module<#to_s> The Slice module to unregister.
      def deactivate(slice_module)
        if slice = self[slice_module]
          slice.deactivate if slice.respond_to?(:deactivate) && slice.routed?
          unregister(slice)
        end
      end
      
      # Deactivate a Slice module at runtime by specifying its slice file
      #
      # @param slice_file<String> The Slice location of the slice init file to unregister.
      def deactivate_by_file(slice_file)
        if slice = self.slices.find { |s| s.file == slice_file }
          deactivate(slice.name)
        end
      end
      
      # Reload a Slice at runtime
      #
      # @param slice_module<#to_s> The Slice module to reload.
      def reload(slice_module)
        if slice = self[slice_module]
          deactivate slice.name
          activate_by_file slice.file
        end
      end
      
      # Reload a Slice at runtime by specifying its slice file
      #
      # @param slice_file<String> The Slice location of the slice init file to reload.
      def reload_by_file(slice_file)
        if slice = self.slices.find { |s| s.file == slice_file }
          reload(slice.name)
        end
      end
      
      # Watch all specified search paths to dynamically load/unload slices at runtime
      #
      # If a valid slice is found it's automatically registered and activated;
      # once a slice is removed (or renamed to not match the convention), it
      # will be unregistered and deactivated. Runs in a Thread.
      #
      # @example Merb::BootLoader.after_app_loads { Merb::Slices.start_dynamic_loader! }
      # 
      # @param interval<Numeric> 
      #   The interval in seconds of checking the search path(s) for changes.
      def start_dynamic_loader!(interval = nil)
        DynamicLoader.start(interval)
      end
      
      # Stop watching search paths to dynamically load/unload slices at runtime
      def stop_dynamic_loader!
        DynamicLoader.stop
      end
      
      # @return <Hash[Hash]> 
      #   A Hash mapping between slice identifiers and non-prefixed named routes.
      def named_routes
        @named_routes ||= {}
      end
      
      # @return <Hash>
      #   The configuration loaded from Merb.root / "config/slices.yml" or, if
      #   the load fails, an empty hash.
      def config
        @config ||= begin
          empty_hash = Hash.new { |h,k| h[k] = {} }
          if File.exists?(Merb.root / "config" / "slices.yml")
            require "yaml"
            YAML.load(File.read(Merb.root / "config" / "slices.yml")) || empty_hash
          else
            empty_hash
          end
        end
      end
    
      # All registered Slice modules
      #
      # @return <Array[Module]> A sorted array of all slice modules.
      def slices
        slice_names.map do |name|
          Object.full_const_get(name) rescue nil
        end.compact
      end
    
      # All registered Slice module names
      #
      # @return <Array[String]> A sorted array of all slice module names.
      def slice_names
        self.paths.keys.sort
      end
    
      # Check whether a Slice exists
      # 
      # @param <#to_s> The slice module to check for.
      def exists?(module_name)
        const_name = module_name.to_s.camel_case
        slice_names.include?(const_name) && Object.const_defined?(const_name)
      end
    
      # A lookup for finding a Slice module's path
      #
      # @return <Hash> A Hash mapping module names to root paths.
      # @note Whenever a slice is deactivated, its path is removed from the lookup.
      def paths
        @paths ||= {}
      end
      
      # A lookup for finding a Slice module's slice file path
      #
      # @return <Hash> A Hash mapping module names to slice files.
      # @note This is unaffected by deactivating a slice; used to reload slices by name.
      def files
        @files ||= {}
      end
  
      # Iterate over all registered slices
      #
      # By default iterates alphabetically over all registered modules. 
      # If Merb::Plugins.config[:merb_slices][:queue] is set, only the
      # defined modules are loaded in the given order. This can be
      # used to selectively load slices, and also maintain load-order
      # for slices that depend on eachother.
      #
      # @yield Iterate over known slices and pass in the slice module.
      # @yieldparam module<Module> The Slice module.
      def each_slice(&block)
        loadable_slices = Merb::Plugins.config[:merb_slices].key?(:queue) ? Merb::Plugins.config[:merb_slices][:queue] : slice_names
        loadable_slices.each do |module_name|
          if mod = self[module_name]
            block.call(mod)
          end
        end
      end
      
      # Slice file locations from all search paths; this default to host-app/slices.
      #
      # Look for any slices in those default locations or if given, 
      # Merb::Plugins.config[:merb_slices][:search_path] (String/Array).
      # Specify files, glob patterns or paths containing slices.
      def slice_files_from_search_path
        search_paths = Array(Merb::Plugins.config[:merb_slices][:search_path] || [Merb.root / "slices"])
        search_paths.inject([]) do |files, path|
          # handle both Pathname and String
          path = path.to_s
          if File.file?(path) && File.extname(path) == ".rb"
            files << path
          elsif path.include?("*")
            files += glob_search_path(path)
          elsif File.directory?(path)
            files += glob_search_path(path / "**/lib/*.rb")
          end
          files
        end
      end
    
      private
    
      # Prepare a module to be a proper Slice module
      #
      # @param module_name<#to_s> The name of the module to prepare
      #
      # @return <Module> The module that has been setup
      def setup_module(module_name)
        Object.make_module(module_name)
        slice_mod = Object.full_const_get(module_name)
        slice_mod.extend(ModuleMixin)
        slice_mod
      end
      
      # Glob slice files
      #
      # @param glob_pattern<String> A glob path with pattern
      # @return <Array> Valid slice file paths.
      def glob_search_path(glob_pattern)
        # handle both Pathname and String
        glob_pattern = glob_pattern.to_s
        Dir[glob_pattern].inject([]) do |files, libfile|
          basename = File.basename(libfile, '.rb')
          files << libfile if File.basename(File.dirname(File.dirname(libfile))) == basename
          files
        end
      end
      
    end
    
    class DynamicLoader
      
      cattr_accessor :lookup
      
      def self.start(interval = nil)
        self.lookup ||= Set.new(Merb::Slices.slice_files_from_search_path)
        @thread = self.every(interval || Merb::Plugins.config[:merb_slices][:autoload_interval] || 1.0) do
          current_files = Set.new(Merb::Slices.slice_files_from_search_path)
          (current_files - self.lookup).each { |f| Merb::Slices.activate_by_file(f) }
          (self.lookup - current_files).each { |f| Merb::Slices.deactivate_by_file(f) }
          self.lookup = current_files
        end
      end
      
      def self.stop
        @thread.exit if @thread.is_a?(Thread)
      end
      
      private
      
      def self.every(seconds, &block)
        Thread.new do
          loop do
            sleep(seconds)
            block.call
          end
          Thread.exit
        end
      end
      
    end
    
  end
end
