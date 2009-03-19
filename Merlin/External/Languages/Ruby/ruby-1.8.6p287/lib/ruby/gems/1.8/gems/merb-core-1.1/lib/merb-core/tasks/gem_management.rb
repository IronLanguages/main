require 'rubygems'

module ColorfulMessages
  
  # red
  def error(*messages)
    puts messages.map { |msg| "\033[1;31m#{msg}\033[0m" }
  end
  
  # yellow
  def warning(*messages)
    puts messages.map { |msg| "\033[1;33m#{msg}\033[0m" }
  end
  
  # green
  def success(*messages)
    puts messages.map { |msg| "\033[1;32m#{msg}\033[0m" }
  end
  
  alias_method :message, :success
  
  # magenta
  def note(*messages)
    puts messages.map { |msg| "\033[1;35m#{msg}\033[0m" }
  end
  
  # blue
  def info(*messages)
    puts messages.map { |msg| "\033[1;34m#{msg}\033[0m" }
  end
  
end

##############################################################################

require 'rubygems/dependency_installer'
require 'rubygems/uninstaller'
require 'rubygems/dependency'

module GemManagement
  
  include ColorfulMessages
    
  # Install a gem - looks remotely and local gem cache;
  # won't process rdoc or ri options.
  def install_gem(gem, options = {})
    refresh = options.delete(:refresh) || []
    from_cache = (options.key?(:cache) && options.delete(:cache))
    if from_cache
      install_gem_from_cache(gem, options)
    else
      version = options.delete(:version)
      Gem.configuration.update_sources = false

      # Limit source index to install dir
      update_source_index(options[:install_dir]) if options[:install_dir]

      installer = Gem::DependencyInstaller.new(options.merge(:user_install => false))
      
      # Force-refresh certain gems by excluding them from the current index
      if !options[:ignore_dependencies] && refresh.respond_to?(:include?) && !refresh.empty?
        source_index = installer.instance_variable_get(:@source_index)
        source_index.gems.each do |name, spec| 
          source_index.gems.delete(name) if refresh.include?(spec.name)
        end
      end
      
      exception = nil
      begin
        installer.install gem, version
      rescue Gem::InstallError => e
        exception = e
      rescue Gem::GemNotFoundException => e
        if from_cache && gem_file = find_gem_in_cache(gem, version)
          puts "Located #{gem} in gem cache..."
          installer.install gem_file
        else
          exception = e
        end
      rescue => e
        exception = e
      end
      if installer.installed_gems.empty? && exception
        error "Failed to install gem '#{gem} (#{version || 'any version'})' (#{exception.message})"
      end
      ensure_bin_wrapper_for_installed_gems(installer.installed_gems, options)
      installer.installed_gems.each do |spec|
        success "Successfully installed #{spec.full_name}"
      end
      return !installer.installed_gems.empty?
    end
  end

  # Install a gem - looks in the system's gem cache instead of remotely;
  # won't process rdoc or ri options.
  def install_gem_from_cache(gem, options = {})
    version = options.delete(:version)
    Gem.configuration.update_sources = false
    installer = Gem::DependencyInstaller.new(options.merge(:user_install => false))
    exception = nil
    begin
      if gem_file = find_gem_in_cache(gem, version)
        puts "Located #{gem} in gem cache..."
        installer.install gem_file
      else
        raise Gem::InstallError, "Unknown gem #{gem}"
      end
    rescue Gem::InstallError => e
      exception = e
    end
    if installer.installed_gems.empty? && exception
      error "Failed to install gem '#{gem}' (#{e.message})"
    end
    ensure_bin_wrapper_for_installed_gems(installer.installed_gems, options)
    installer.installed_gems.each do |spec|
      success "Successfully installed #{spec.full_name}"
    end
  end

  # Install a gem from source - builds and packages it first then installs.
  # 
  # Examples:
  # install_gem_from_source(source_dir, :install_dir => ...)
  # install_gem_from_source(source_dir, gem_name)
  # install_gem_from_source(source_dir, :skip => [...])
  def install_gem_from_source(source_dir, *args)
    installed_gems = []
    opts = args.last.is_a?(Hash) ? args.pop : {}
    Dir.chdir(source_dir) do      
      gem_name     = args[0] || File.basename(source_dir)
      gem_pkg_dir  = File.join(source_dir, 'pkg')
      gem_pkg_glob = File.join(gem_pkg_dir, "#{gem_name}-*.gem")
      skip_gems    = opts.delete(:skip) || []

      # Cleanup what's already there
      clobber(source_dir)
      FileUtils.mkdir_p(gem_pkg_dir) unless File.directory?(gem_pkg_dir)

      # Recursively process all gem packages within the source dir
      skip_gems << gem_name
      packages = package_all(source_dir, skip_gems)
      
      if packages.length == 1
        # The are no subpackages for the main package
        refresh = [gem_name]
      else
        # Gather all packages into the top-level pkg directory
        packages.each do |pkg|
          FileUtils.copy_entry(pkg, File.join(gem_pkg_dir, File.basename(pkg)))
        end
        
        # Finally package the main gem - without clobbering the already copied pkgs
        package(source_dir, false)
        
        # Gather subgems to refresh during installation of the main gem
        refresh = packages.map do |pkg|
          File.basename(pkg, '.gem')[/^(.*?)-([\d\.]+)$/, 1] rescue nil
        end.compact
        
        # Install subgems explicitly even if ignore_dependencies is set
        if opts[:ignore_dependencies]
          refresh.each do |name| 
            gem_pkg = Dir[File.join(gem_pkg_dir, "#{name}-*.gem")][0]
            install_pkg(gem_pkg, opts)
          end
        end
      end
      
      ensure_bin_wrapper_for(opts[:install_dir], opts[:bin_dir], *installed_gems)
      
      # Finally install the main gem
      if install_pkg(Dir[gem_pkg_glob][0], opts.merge(:refresh => refresh))
        installed_gems = refresh
      else
        installed_gems = []
      end
    end
    installed_gems
  end
  
  def install_pkg(gem_pkg, opts = {})
    if (gem_pkg && File.exists?(gem_pkg))
      # Needs to be executed from the directory that contains all packages
      Dir.chdir(File.dirname(gem_pkg)) { install_gem(gem_pkg, opts) }
    else
      false
    end
  end
  
  # Uninstall a gem.
  def uninstall_gem(gem, options = {})
    if options[:version] && !options[:version].is_a?(Gem::Requirement)
      options[:version] = Gem::Requirement.new ["= #{options[:version]}"]
    end
    update_source_index(options[:install_dir]) if options[:install_dir]
    Gem::Uninstaller.new(gem, options).uninstall rescue nil
  end

  def clobber(source_dir)
    Dir.chdir(source_dir) do 
      system "#{Gem.ruby} -S rake -s clobber" unless File.exists?('Thorfile')
    end
  end

  def package(source_dir, clobber = true)
    Dir.chdir(source_dir) do 
      if File.exists?('Thorfile')
        thor ":package"
      elsif File.exists?('Rakefile')
        rake "clobber" if clobber
        rake "package"
      end
    end
    Dir[File.join(source_dir, 'pkg/*.gem')]
  end

  def package_all(source_dir, skip = [], packages = [])
    if Dir[File.join(source_dir, '{Rakefile,Thorfile}')][0]
      name = File.basename(source_dir)
      Dir[File.join(source_dir, '*', '{Rakefile,Thorfile}')].each do |taskfile|
        package_all(File.dirname(taskfile), skip, packages)
      end
      packages.push(*package(source_dir)) unless skip.include?(name)
    end
    packages.uniq
  end
  
  def rake(cmd)
    cmd << " >/dev/null" if $SILENT && !Gem.win_platform?
    system "#{Gem.ruby} -S #{which('rake')} -s #{cmd} >/dev/null"
  end
  
  def thor(cmd)
    cmd << " >/dev/null" if $SILENT && !Gem.win_platform?
    system "#{Gem.ruby} -S #{which('thor')} #{cmd}"
  end

  # Use the local bin/* executables if available.
  def which(executable)
    if File.executable?(exec = File.join(Dir.pwd, 'bin', executable))
      exec
    else
      executable
    end
  end
  
  # Partition gems into system, local and missing gems
  def partition_dependencies(dependencies, gem_dir)
    system_specs, local_specs, missing_deps = [], [], []
    if gem_dir && File.directory?(gem_dir)
      gem_dir = File.expand_path(gem_dir)
      ::Gem.clear_paths; ::Gem.path.unshift(gem_dir)
      ::Gem.source_index.refresh!
      dependencies.each do |dep|
        gemspecs = ::Gem.source_index.search(dep)
        local = gemspecs.reverse.find { |s| s.loaded_from.index(gem_dir) == 0 }
        if local
          local_specs << local
        elsif gemspecs.last
          system_specs << gemspecs.last
        else
          missing_deps << dep
        end
      end
      ::Gem.clear_paths
    else
      dependencies.each do |dep|
        gemspecs = ::Gem.source_index.search(dep)
        if gemspecs.last
          system_specs << gemspecs.last
        else
          missing_deps << dep
        end
      end
    end
    [system_specs, local_specs, missing_deps]
  end
  
  # Create a modified executable wrapper in the specified bin directory.
  def ensure_bin_wrapper_for(gem_dir, bin_dir, *gems)
    options = gems.last.is_a?(Hash) ? gems.last : {}
    options[:no_minigems] ||= []
    if bin_dir && File.directory?(bin_dir)
      gems.each do |gem|
        if gemspec_path = Dir[File.join(gem_dir, 'specifications', "#{gem}-*.gemspec")].last
          spec = Gem::Specification.load(gemspec_path)
          enable_minigems = !options[:no_minigems].include?(spec.name)
          spec.executables.each do |exec|
            executable = File.join(bin_dir, exec)
            message "Writing executable wrapper #{executable}"
            File.open(executable, 'w', 0755) do |f|
              f.write(executable_wrapper(spec, exec, enable_minigems))
            end
          end
        end
      end
    end
  end
  
  def ensure_bin_wrapper_for_installed_gems(gemspecs, options)
    if options[:install_dir] && options[:bin_dir]
      gems = gemspecs.map { |spec| spec.name }
      ensure_bin_wrapper_for(options[:install_dir], options[:bin_dir], *gems)
    end
  end
  
  private

  def executable_wrapper(spec, bin_file_name, minigems = true)
    requirements = ['minigems', 'rubygems']
    requirements.reverse! unless minigems
    try_req, then_req = requirements
    <<-TEXT
#!/usr/bin/env ruby
#
# This file was generated by Merb's GemManagement
#
# The application '#{spec.name}' is installed as part of a gem, and
# this file is here to facilitate running it.

begin 
  require '#{try_req}'
rescue LoadError 
  require '#{then_req}'
end

# use gems dir if ../gems exists - eg. only for ./bin/#{bin_file_name}
if File.directory?(gems_dir = File.join(File.dirname(__FILE__), '..', 'gems'))
  $BUNDLE = true; Gem.clear_paths; Gem.path.replace([gems_dir])
  ENV["PATH"] = "\#{File.dirname(__FILE__)}:\#{gems_dir}/bin:\#{ENV["PATH"]}"
  if (local_gem = Dir[File.join(gems_dir, "specifications", "#{spec.name}-*.gemspec")].last)
    version = File.basename(local_gem)[/-([\\.\\d]+)\\.gemspec$/, 1]
  end
end

version ||= "#{Gem::Requirement.default}"

if ARGV.first =~ /^_(.*)_$/ and Gem::Version.correct? $1 then
  version = $1
  ARGV.shift
end

gem '#{spec.name}', version
load '#{bin_file_name}'
TEXT
  end

  def find_gem_in_cache(gem, version)
    spec = if version
      version = Gem::Requirement.new ["= #{version}"] unless version.is_a?(Gem::Requirement)
      Gem.source_index.find_name(gem, version).first
    else
      Gem.source_index.find_name(gem).sort_by { |g| g.version }.last
    end
    if spec && File.exists?(gem_file = "#{spec.installation_path}/cache/#{spec.full_name}.gem")
      gem_file
    end
  end

  def update_source_index(dir)
    Gem.source_index.load_gems_in(File.join(dir, 'specifications'))
  end
    
end
