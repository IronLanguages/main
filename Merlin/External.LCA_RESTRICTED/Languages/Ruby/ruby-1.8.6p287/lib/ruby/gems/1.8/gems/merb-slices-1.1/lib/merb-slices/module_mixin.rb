module Merb
  module Slices
    module ModuleMixin
      
      # See bin/slice for this - used by ModuleMixin#push_app_path
      $SLICE_MODULE ||= false
      
      def self.extended(slice_module)
        slice_module.meta_class.module_eval do
          attr_accessor :identifier, :identifier_sym, :root, :file
          attr_accessor :description, :version, :author
        end
      end
    
      # Stub that gets triggered when a slice has been registered.
      #
      # @note This is rarely needed but still provided for edge cases.
      def registered; end
    
      # Stub classes loaded hook - runs before LoadClasses BootLoader
      # right after a slice's classes have been loaded internally.
      def loaded; end
    
      # Stub initialization hook - runs before AfterAppLoads BootLoader.
      def init; end
    
      # Stub activation hook - runs after AfterAppLoads BootLoader.
      def activate; end
    
      # Stub deactivation method - not triggered automatically.
      def deactivate; end
    
      # Stub to setup routes inside the host application.
      def setup_router(scope); end
      
      # Check if there have been any routes setup.
      def routed?
        self.named_routes && !self.named_routes.empty?
      end
      
      # Whether we're in an application or running from the slice dir itself.
      def standalone?
        Merb.root == self.root
      end
      
      # Return a value suitable for routes/urls.
      def to_param
        self.identifier
      end

      # @param <Symbol> The configuration key.
      # @return <Object> The configuration value.
      def [](key)
        self.config[key]
      end
      
      # @param <Symbol> The configuration key.
      # @param <Object> The configuration value.
      def []=(key, value)
        self.config[key] = value
      end
      
      # @return <Hash> The configuration for this slice.
      def config
        Merb::Slices::config[self.identifier_sym] ||= {}
      end
      
      # @return <Hash> The named routes for this slice.
      def named_routes
        Merb::Slices.named_routes[self.identifier_sym] ||= {}
      end
      
      # Load slice and it's classes located in the slice-level load paths.
      # 
      # Assigns collected_slice_paths and collected_app_paths, then loads
      # the collected_slice_paths and triggers the #loaded hook method.
      def load_slice
        # load application.rb (or similar) for thin slices
        Merb::Slices::Loader.load_file self.dir_for(:application) if File.file?(self.dir_for(:application))
        # assign all relevant paths for slice-level and app-level
        self.collect_load_paths
        # load all slice-level classes from paths
        Merb::Slices::Loader.load_classes self.collected_slice_paths
        # call hook if available
        self.loaded if self.respond_to?(:loaded)
        Merb.logger.info!("Loaded slice '#{self}' ...")
      rescue => e
        Merb.logger.warn!("Failed loading #{self} (#{e.message})")
      end
      
      # The slice-level load paths that have been used when the slice was loaded.
      # 
      # This may be a subset of app_paths, which includes any path to look for.
      #
      # @return <Array[String]> load paths (with glob pattern)
      def collected_slice_paths
        @collected_slice_paths ||= []
      end
      
      # The app-level load paths that have been used when the slice was loaded.
      # 
      # This may be a subset of app_paths, which includes any path to look for.
      #
      # @return <Array[String]> Application load paths (with glob pattern)
      def collected_app_paths
        @collected_app_paths ||= []
      end
    
      # The slice-level load paths to use when loading the slice.
      #
      # @return <Hash> The load paths which make up the slice-level structure.
      def slice_paths
        @slice_paths ||= Hash.new { [self.root] }
      end
    
      # The app-level load paths to use when loading the slice.
      #
      # @return <Hash> The load paths which make up the app-level structure.
      def app_paths
        @app_paths ||= Hash.new { [Merb.root] }
      end
    
      # @param *path<#to_s>
      #   The relative path (or list of path components) to a directory under the
      #   root of the application.
      #
      # @return <String> The full path including the root.
      def root_path(*path) File.join(self.root, *path) end
    
      # Retrieve the absolute path to a slice-level directory.
      #
      # @param type<Symbol> The type of path to retrieve directory for, e.g. :view.
      #
      # @return <String> The absolute path for the requested type.
      def dir_for(type) self.slice_paths[type].first end
    
      # @param type<Symbol> The type of path to retrieve glob for, e.g. :view.
      #
      # @return <String> The pattern with which to match files within the type directory.
      def glob_for(type) self.slice_paths[type][1] end

      # Retrieve the absolute path to a app-level directory. 
      #
      # @param type<Symbol> The type of path to retrieve directory for, e.g. :view.
      #
      # @return <String> The directory for the requested type.
      def app_dir_for(type) self.app_paths[type].first end
    
      # @param type<Symbol> The type of path to retrieve glob for, e.g. :view.
      #
      # @return <String> The pattern with which to match files within the type directory.
      def app_glob_for(type) self.app_paths[type][1] end
    
      # Retrieve the relative path to a public directory.
      #
      # @param type<Symbol> The type of path to retrieve directory for, e.g. :view.
      #
      # @return <String> The relative path to the public directory for the requested type.
      def public_dir_for(type)
        dir = type.is_a?(Symbol) ? self.app_dir_for(type) : self.app_dir_for(:public) / type
        dir = dir.relative_path_from(Merb.dir_for(:public)) rescue '.'
        dir == '.' ? '/' : "/#{dir}"
      end
      
      # Construct a path relative to the public directory
      # 
      # @param <Symbol> The type of component.
      # @param *segments<Array[#to_s]> Path segments to append.
      #
      # @return <String> 
      #  A path relative to the public directory, with added segments.
      def public_path_for(type, *segments)
        File.join(self.public_dir_for(type), *segments)
      end
      
      # Construct an app-level path.
      # 
      # @param <Symbol> The type of component.
      # @param *segments<Array[#to_s]> Path segments to append.
      #
      # @return <String> 
      #  A path within the host application, with added segments.
      def app_path_for(type, *segments)
        prefix = type.is_a?(Symbol) ? self.app_dir_for(type) : self.app_dir_for(:root) / type
        File.join(prefix, *segments)
      end
      
      # Construct a slice-level path.
      # 
      # @param <Symbol> The type of component.
      # @param *segments<Array[#to_s]> Path segments to append.
      #
      # @return <String> 
      #  A path within the slice source (Gem), with added segments.
      def slice_path_for(type, *segments)
        prefix = type.is_a?(Symbol) ? self.dir_for(type) : self.dir_for(:root) / type
        File.join(prefix, *segments)
      end
    
      # This is the core mechanism for setting up your slice-level layout.
      #
      # @param type<Symbol> The type of path being registered (i.e. :view)
      # @param path<String> The full path
      # @param file_glob<String>
      #   A glob that will be used to autoload files under the path. Defaults to "**/*.rb".
      def push_path(type, path, file_glob = "**/*.rb")
        enforce!(type => Symbol)
        slice_paths[type] = [path, file_glob]
      end
    
      # Removes given types of application components
      # from slice-level load path this slice uses for autoloading.
      #
      # @param *args<Array[Symbol]> Components names, for instance, :views, :models
      def remove_paths(*args)
        args.each { |arg| self.slice_paths.delete(arg) }
      end
    
      # This is the core mechanism for setting up your app-level layout.
      #
      # @param type<Symbol> The type of path being registered (i.e. :view)
      # @param path<String> The full path
      # @param file_glob<String>
      #   A glob that will be used to autoload files under the path. Defaults to "**/*.rb".
      #
      # @note The :public path is adapted when the slice is run from bin/slice.
      def push_app_path(type, path, file_glob = "**/*.rb")
        enforce!(type => Symbol)
        if type == :public && standalone? && $SLICE_MODULE
          path.gsub!(/\/slices\/#{self.identifier}$/, '')
        end
        app_paths[type] = [path, file_glob]
      end
    
      # Removes given types of application components
      # from app-level load path this slice uses for autoloading.
      #
      # @param *args<Array[Symbol]> Components names, for instance, :views, :models
      def remove_app_paths(*args)
        args.each { |arg| self.app_paths.delete(arg) }
      end
      
      # Return all *.rb files from valid component paths
      #
      # @return <Array> Full paths to loadable ruby files.
      def loadable_files
        app_components.inject([]) do |paths, type|
          paths += Dir[dir_for(type) / '**/*.rb'] if slice_paths.key?(type)
          paths += Dir[app_dir_for(type) / '**/*.rb'] if app_paths.key?(type)
          paths
        end        
      end
      
      # Return all application path component types
      #
      # @return <Array[Symbol]> Component types.
      def app_components
        [:view, :model, :controller, :helper, :mailer, :part]
      end
      
      # Return all public path component types
      #
      # @return <Array[Symbol]> Component types.
      def public_components
        [:stylesheet, :javascript, :image]
      end
    
      # Return all path component types to mirror
      #
      # If config option :mirror is set return a subset, otherwise return all types.
      #
      # @return <Array[Symbol]> Component types.
      def mirrored_components
        all = slice_paths.keys
        config[:mirror].is_a?(Array) ? config[:mirror] & all : all
      end
      
      # Return all application path component types to mirror
      #
      # @return <Array[Symbol]> Component types.
      def mirrored_app_components
        mirrored_components & app_components
      end
      
      # Return all public path component types to mirror
      #
      # @return <Array[Symbol]> Component types.
      def mirrored_public_components
        mirrored_components & public_components
      end
      
      # Return all slice files mapped from the source to their relative path
      #
      # @param type<Symbol> Which type to use; defaults to :root (all)
      # @return <Array[Array]> An array of arrays [abs. source, relative dest.]
      def manifest(type = :root)
        files = if type == :root
          Dir.glob(self.root / "**/*")
        elsif slice_paths.key?(type)
          glob = ((type == :view) ? view_templates_glob : glob_for(type) || "**/*")
          Dir.glob(dir_for(type) / glob)
        else 
          []
        end
        files.map { |source| [source, source.relative_path_from(root)] }
      end
      
      # Clone all files from the slice to their app-level location; this will
      # also copy /lib, causing merb-slices to pick up the slice there.
      # 
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      def clone_slice!
        app_slice_root = app_dir_for(:root)
        copied, duplicated = [], []
        manifest.each do |source, relative_path|
          mirror_file(source, app_slice_root / relative_path, copied, duplicated)
        end
        [copied, duplicated]
      end
      
      # Unpack a subset of files from the slice to their app-level location; 
      # this will also copy /lib, causing merb-slices to pick up the slice there.
      # 
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      #
      # @note Files for the :stub component type are skipped.
      def unpack_slice!
        app_slice_root = app_dir_for(:root)
        copied, duplicated = mirror_public!
        manifest.each do |source, relative_path|
          next unless unpack_file?(relative_path)
          mirror_file(source, app_slice_root / relative_path, copied, duplicated)
        end
        [copied, duplicated]
      end
      
      # Copies all files from mirrored_components to their app-level location
      #
      # This includes application and public components. 
      # 
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      def mirror_all!
        mirror_files_for mirrored_components + mirrored_public_components
      end
      
      # Copies all files from the (optional) stubs directory to their app-level location
      #
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      def mirror_stubs!
        mirror_files_for :stub
      end
      
      # Copies all application files from mirrored_components to their app-level location
      #
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      def mirror_app!
        components = mirrored_app_components
        components << :application if application_file?
        mirror_files_for components
      end
      
      # Copies all application files from mirrored_components to their app-level location
      #
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      def mirror_public!
        mirror_files_for mirrored_public_components
      end
      
      # Copy files from specified component path types to their app-level location
      #
      # App-level overrides are preserved by creating duplicates before writing gem-level files.
      # Because of their _override postfix they will load after their original implementation.
      # In the case of views, this won't work, but the user override is preserved nonetheless.
      # 
      # @return <Array[Array]> 
      #   Array of two arrays, one for all copied files, the other for overrides 
      #   that may have been preserved to resolve collisions.
      #
      # @note Only explicitly defined component paths will be taken into account to avoid
      #   cluttering the app's Merb.root by mistake - since undefined paths default to that.
      def mirror_files_for(*types)
        seen, copied, duplicated = [], [], [] # keep track of files we copied
        types.flatten.each do |type|
          if app_paths.key?(type) && (source_path = dir_for(type)) && (destination_path = app_dir_for(type))
            manifest(type).each do |source, relative_path| # this relative path is not what we need here
              next if seen.include?(source)
              mirror_file(source, destination_path / source.relative_path_from(source_path), copied, duplicated)
              seen << source
            end
          end
        end
        [copied, duplicated]
      end
      
      # This sets up the default slice-level and app-level structure.
      # 
      # You can create your own structure by implementing setup_structure and
      # using the push_path and push_app_paths. By default this setup matches
      # what the merb-gen slice generator creates.
      def setup_default_structure!
        self.push_app_path(:root, Merb.root / 'slices' / self.identifier, nil)
        
        self.push_path(:stub, root_path('stubs'), nil)
        self.push_app_path(:stub, app_dir_for(:root), nil)
        
        self.push_path(:application, root_path('app'), nil)
        self.push_app_path(:application, app_dir_for(:root) / 'app', nil)
      
        app_components.each do |component|
          self.push_path(component, dir_for(:application) / "#{component}s")
          self.push_app_path(component, app_dir_for(:application) / "#{component}s")
        end
      
        self.push_path(:public, root_path('public'), nil)
        self.push_app_path(:public,  Merb.dir_for(:public) / 'slices' / self.identifier, nil)
      
        public_components.each do |component|
          self.push_path(component, dir_for(:public) / "#{component}s", nil)
          self.push_app_path(component, app_dir_for(:public) / "#{component}s", nil)
        end
      end   
      
      protected
      
      # Collect slice-level and app-level load paths to load from.
      #
      # @param modify_load_path<Boolean> 
      #   Whether to add certain paths to $LOAD_PATH; defaults to true.
      # @param push_merb_path<Boolean> 
      #   Whether to add app-level paths using Merb.push_path; defaults to true.
      def collect_load_paths(modify_load_path = true, push_merb_path = true)
        self.collected_slice_paths.clear; self.collected_app_paths.clear
        Merb.push_path(:"#{self.name.snake_case}_file", File.dirname(self.file), File.basename(self.file))
        self.collected_app_paths << self.file
        self.slice_paths.each do |component, path|
          if File.directory?(component_path = path.first)
            $LOAD_PATH.unshift(component_path) if modify_load_path && component.in?(:model, :controller, :lib) && !$LOAD_PATH.include?(component_path)
            # slice-level component load path - will be preceded by application/app/component - loaded next by Setup.load_classes
            self.collected_slice_paths << path.first / path.last if path.last
            # app-level component load path (override) path - loaded by BootLoader::LoadClasses
            if (app_glob = self.app_glob_for(component)) && File.directory?(app_component_dir = self.app_dir_for(component))
              self.collected_app_paths << app_component_dir / app_glob
              Merb.push_path(:"#{self.name.snake_case}_#{component}", app_component_dir, app_glob) if push_merb_path
            end
          end
        end
      end
      
      # Helper method to copy a source file to destination while resolving any conflicts.
      #
      # @param source<String> The source path.
      # @param dest<String> The destination path.
      # @param copied<Array> Keep track of all copied files - relative paths.
      # @param duplicated<Array> Keep track of all duplicated files - relative paths.
      # @param postfix<String> The postfix to use for resolving conflicting filenames.
      def mirror_file(source, dest, copied = [], duplicated = [], postfix = '_override')
        base, rest = split_name(source)
        dst_dir = File.dirname(dest)
        dup_path = dst_dir / "#{base}#{postfix}.#{rest}"           
        if File.file?(source)
          FileUtils.mkdir_p(dst_dir) unless File.directory?(dst_dir)
          if File.exists?(dest) && !File.exists?(dup_path) && !FileUtils.identical?(source, dest)
            # copy app-level override to *_override.ext
            FileUtils.copy_entry(dest, dup_path, false, false, true)
            duplicated << dup_path.relative_path_from(Merb.root)
          end
          # copy gem-level original to location
          if !File.exists?(dest) || (File.exists?(dest) && !FileUtils.identical?(source, dest))
            FileUtils.copy_entry(source, dest, false, false, true) 
            copied << dest.relative_path_from(Merb.root)
          end
        end
      end
      
      # Predicate method to check if a file should be taken into account when unpacking files
      #
      # By default any public component paths and stubs are skipped; additionally you can set
      # the :skip_files in the slice's config for other relative paths to skip.
      #
      # @param file<String> The relative path to test.
      # @return <TrueClass,FalseClass> True if the file may be mirrored.
      def unpack_file?(file)
        @mirror_exceptions_regexp ||= begin
          skip_paths = (mirrored_public_components + [:stub]).map { |type| dir_for(type).relative_path_from(self.root) }
          skip_paths += config[:skip_files] if config[:skip_files].is_a?(Array)
          Regexp.new("^(#{skip_paths.join('|')})")
        end
        not file.match(@mirror_exceptions_regexp)
      end
      
      # Predicate method to check if the :application component is a file
      def application_file?
        File.file?(dir_for(:application) / glob_for(:application))
      end
      
      # Glob pattern matching all valid template extensions
      def view_templates_glob
        "**/*.{#{Merb::Template.template_extensions.join(',')}}"
      end
      
      # Split a file name so a postfix can be inserted
      #
      # @return <Array[String]> 
      #   The first element will be the name up to the first dot, the second will be the rest.
      def split_name(name)
        file_name = File.basename(name)
        mres = /^([^\/\.]+)\.(.+)$/i.match(file_name)
        mres.nil? ? [file_name, ''] : [mres[1], mres[2]]
      end
      
    end
  end
end