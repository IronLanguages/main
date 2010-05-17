#!/usr/bin/env ruby
require 'rubygems'
require 'thor'
require 'fileutils'
require 'yaml'

# Important - don't change this line or its position
MERB_THOR_VERSION = '0.0.52'

##############################################################################

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
    Dir.chdir(source_dir) do
      opts = args.last.is_a?(Hash) ? args.pop : {}
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

if File.directory?(gems_dir = File.join(Dir.pwd, 'gems')) ||
   File.directory?(gems_dir = File.join(File.dirname(__FILE__), '..', 'gems'))
  $BUNDLE = true; Gem.clear_paths; Gem.path.unshift(gems_dir)
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

##############################################################################

class SourceManager
  
  include ColorfulMessages
  
  attr_accessor :source_dir
  
  def initialize(source_dir)
    self.source_dir = source_dir
  end
  
  def clone(name, url)
    FileUtils.cd(source_dir) do
      raise "destination directory already exists" if File.directory?(name)
      system("git clone --depth 1 #{url} #{name}")
    end
  rescue => e
    error "Unable to clone #{name} repository (#{e.message})"
  end
  
  def update(name, url)
    if File.directory?(repository_dir = File.join(source_dir, name))
      FileUtils.cd(repository_dir) do
        repos = existing_repos(name)
        fork_name = url[/.com\/+?(.+)\/.+\.git/u, 1]
        if url == repos["origin"]
          # Pull from the original repository - no branching needed
          info "Pulling from origin: #{url}"
          system "git fetch; git checkout master; git rebase origin/master"
        elsif repos.values.include?(url) && fork_name
          # Update and switch to a remote branch for a particular github fork
          info "Switching to remote branch: #{fork_name}"
          system "git checkout -b #{fork_name} #{fork_name}/master"   
          system "git rebase #{fork_name}/master"
        elsif fork_name
          # Create a new remote branch for a particular github fork
          info "Adding a new remote branch: #{fork_name}"
          system "git remote add -f #{fork_name} #{url}"
          system "git checkout -b #{fork_name} #{fork_name}/master"
        else
          warning "No valid repository found for: #{name}"
        end
      end
      return true
    else
      warning "No valid repository found at: #{repository_dir}"
    end
  rescue => e
    error "Unable to update #{name} repository (#{e.message})"
    return false
  end
  
  def existing_repos(name)
    repos = []
    FileUtils.cd(File.join(source_dir, name)) do
      repos = %x[git remote -v].split("\n").map { |branch| branch.split(/\s+/) }
    end
    Hash[*repos.flatten]
  end
  
end

##############################################################################

module MerbThorHelper
  
  attr_accessor :include_dependencies, :force_gem_dir
  
  def self.included(base)
    base.send(:include, ColorfulMessages)
    base.extend ColorfulMessages
  end
  
  def source_manager
    @_source_manager ||= SourceManager.new(source_dir)
  end
  
  def extract_repositories(names)
    repos = []
    names.each do |name|
      if repo_url = Merb::Source.repo(name, options[:sources])
        # A repository entry for this dependency exists
        repo = [name, repo_url]
        repos << repo unless repos.include?(repo) 
      elsif (repo_name = Merb::Stack.lookup_repository_name(name)) &&
        (repo_url = Merb::Source.repo(repo_name, options[:sources]))
        # A parent repository entry for this dependency exists
        repo = [repo_name, repo_url]
        unless repos.include?(repo)
          puts "Found #{repo_name}/#{name} at #{repo_url}"
          repos << repo 
        end
      end
    end
    repos
  end
  
  def update_dependency_repositories(dependencies)
    repos = extract_repositories(dependencies.map { |d| d.name })
    update_repositories(repos)
  end
  
  def update_repositories(repos)
    repos.each do |(name, url)|
      if File.directory?(repository_dir = File.join(source_dir, name))
        message "Updating or branching #{name}..."
        source_manager.update(name, url)
      else
        message "Cloning #{name} repository from #{url}..."
        source_manager.clone(name, url)
      end
    end
  end
  
  def install_dependency(dependency, opts = {})
    opts[:version] ||= dependency.version_requirements.to_s
    Merb::Gem.install(dependency.name, default_install_options.merge(opts))
  end

  def install_dependency_from_source(dependency, opts = {})
    matches = Dir[File.join(source_dir, "**", dependency.name, "{Rakefile,Thorfile}")]
    matches.reject! { |m| File.basename(m) == 'Thorfile' }
    if matches.length == 1 && matches[0]
      if File.directory?(gem_src_dir = File.dirname(matches[0]))
        begin
          Merb::Gem.install_gem_from_source(gem_src_dir, default_install_options.merge(opts))
          puts "Installed #{dependency.name}"
          return true
        rescue => e
          warning "Unable to install #{dependency.name} from source (#{e.message})"
        end
      else
        msg = "Unknown directory: #{gem_src_dir}"
        warning "Unable to install #{dependency.name} from source (#{msg})"
      end
    elsif matches.length > 1
      error "Ambigous source(s) for dependency: #{dependency.name}"
      matches.each { |m| puts "- #{m}" }
    end
    return false
  end
  
  def clobber_dependencies!
    if options[:force] && gem_dir && File.directory?(gem_dir)
      # Remove all existing local gems by clearing the gems directory
      if dry_run?
        note 'Clearing existing local gems...'
      else
        message 'Clearing existing local gems...'
        FileUtils.rm_rf(gem_dir) && FileUtils.mkdir_p(default_gem_dir)
      end
    elsif !local.empty? 
      # Uninstall all local versions of the gems to install
      if dry_run?
        note 'Uninstalling existing local gems:'
        local.each { |gemspec| note "Uninstalled #{gemspec.name}" }
      else
        message 'Uninstalling existing local gems:' if local.size > 1
        local.each do |gemspec|
          Merb::Gem.uninstall(gemspec.name, default_uninstall_options)
        end
      end
    end
  end
    
  def display_gemspecs(gemspecs)
    if gemspecs.empty?
      puts "- none"
    else
      gemspecs.each do |spec| 
        if hint = Dir[File.join(spec.full_gem_path, '*.strategy')][0]
          strategy = File.basename(hint, '.strategy')
          puts "- #{spec.full_name} (#{strategy})"
        else
          puts "~ #{spec.full_name}" # unknown strategy
        end
      end
    end
  end
  
  def display_dependencies(dependencies)
    if dependencies.empty?
      puts "- none"
    else
      dependencies.each { |d| puts "- #{d.name} (#{d.version_requirements})" }
    end
  end
  
  def default_install_options
    { :install_dir => gem_dir, :ignore_dependencies => ignore_dependencies? }
  end
  
  def default_uninstall_options
    { :install_dir => gem_dir, :ignore => true, :all => true, :executables => true }
  end
  
  def dry_run?
    options[:"dry-run"]
  end
  
  def ignore_dependencies?
    options[:"ignore-dependencies"] || !include_dependencies?
  end
  
  def include_dependencies?
    options[:"include-dependencies"] || self.include_dependencies
  end
  
  # The current working directory, or Merb app root (--merb-root option).
  def working_dir
    @_working_dir ||= File.expand_path(options['merb-root'] || Dir.pwd)
  end
  
  # We should have a ./src dir for local and system-wide management.
  def source_dir
    @_source_dir  ||= File.join(working_dir, 'src')
    create_if_missing(@_source_dir)
    @_source_dir
  end
    
  # If a local ./gems dir is found, return it.
  def gem_dir
    return force_gem_dir if force_gem_dir
    if File.directory?(dir = default_gem_dir)
      dir
    end
  end
  
  def default_gem_dir
    File.join(working_dir, 'gems')
  end
  
  # If we're in a Merb app, we can have a ./bin directory;
  # create it if it's not there.
  def bin_dir
    @_bin_dir ||= begin
      if gem_dir
        dir = File.join(working_dir, 'bin')
        create_if_missing(dir)
        dir
      end
    end
  end
  
  # Helper to create dir unless it exists.
  def create_if_missing(path)
    FileUtils.mkdir(path) unless File.exists?(path)
  end

  def ensure_bin_wrapper_for(*gems)
    Merb::Gem.ensure_bin_wrapper_for(gem_dir, bin_dir, *gems)
  end
  
  def sudo
    ENV['THOR_SUDO'] ||= "sudo"
    sudo = Gem.win_platform? ? "" : ENV['THOR_SUDO']
  end
    
  def local_gemspecs(directory = gem_dir)
    if File.directory?(specs_dir = File.join(directory, 'specifications'))
      Dir[File.join(specs_dir, '*.gemspec')].map do |gemspec_path|
        gemspec = Gem::Specification.load(gemspec_path)
        gemspec.loaded_from = gemspec_path
        gemspec
      end
    else
      []
    end
  end
  
end

##############################################################################

$SILENT = true # don't output all the mess some rake package tasks spit out

module Merb
    
  class Dependencies < Thor
    
    # The Dependencies tasks will install dependencies based on actual application
    # dependencies. For this, the application is queried for any dependencies.
    # All operations will be performed within this context.
    
    attr_accessor :system, :local, :missing
    
    include MerbThorHelper
    
    global_method_options = {
      "--merb-root"            => :optional,  # the directory to operate on
      "--include-dependencies" => :boolean,   # gather sub-dependencies
      "--stack"                => :boolean,   # gather only stack dependencies
      "--no-stack"             => :boolean,   # gather only non-stack dependencies
      "--config"               => :boolean,   # gather dependencies from yaml config
      "--config-file"          => :optional,  # gather from the specified yaml config
      "--version"              => :optional   # gather specific version of framework
    }
    
    method_options global_method_options
    def initialize(*args); super; end
    
    # List application dependencies.
    #
    # By default all dependencies are listed, partitioned into system, local and
    # currently missing dependencies. The first argument allows you to filter
    # on any of the partitionings. A second argument can be used to filter on
    # a set of known components, like all merb-more gems for example.
    # 
    # Examples:
    #
    # merb:dependencies:list                                    # list all dependencies - the default
    # merb:dependencies:list local                              # list only local gems
    # merb:dependencies:list all merb-more                      # list only merb-more related dependencies
    # merb:dependencies:list --stack                            # list framework dependencies
    # merb:dependencies:list --no-stack                         # list 3rd party dependencies
    # merb:dependencies:list --config                           # list dependencies from the default config
    # merb:dependencies:list --config-file file.yml             # list from the specified config file
       
    desc 'list [all|local|system|missing] [comp]', 'Show application dependencies'
    def list(filter = 'all', comp = nil)
      deps = comp ? Merb::Stack.select_component_dependencies(dependencies, comp) : dependencies
      self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      case filter
      when 'all'
        message 'Installed system gem dependencies:' 
        display_gemspecs(system)
        message 'Installed local gem dependencies:'
        display_gemspecs(local)
        unless missing.empty?
          error 'Missing gem dependencies:'
          display_dependencies(missing)
        end
      when 'system'
        message 'Installed system gem dependencies:'
        display_gemspecs(system)
      when 'local'
        message 'Installed local gem dependencies:'
        display_gemspecs(local)
      when 'missing'
        error 'Missing gem dependencies:'
        display_dependencies(missing)
      else
        warning "Invalid listing filter '#{filter}'"
      end
      if missing.size > 0
        info "Some dependencies are currently missing!"
      elsif local.size == deps.size
        info "All dependencies have been bundled with the application."
      elsif local.size > system.size
        info "Most dependencies have been bundled with the application."
      elsif system.size > 0 && local.size > 0
        info "Some dependencies have been bundled with the application."  
      elsif local.empty? && system.size == deps.size
        info "All dependencies are available on the system."
      end
    end
    
    # Install application dependencies.
    #
    # By default all required dependencies are installed. The first argument 
    # specifies which strategy to use: stable or edge. A second argument can be 
    # used to filter on a set of known components.
    #
    # Existing dependencies will be clobbered; when :force => true then all gems
    # will be cleared first, otherwise only existing local dependencies of the
    # gems to be installed will be removed.
    # 
    # Examples:
    #
    # merb:dependencies:install                                 # install all dependencies using stable strategy
    # merb:dependencies:install stable --version 0.9.8          # install a specific version of the framework
    # merb:dependencies:install stable missing                  # install currently missing gems locally
    # merb:dependencies:install stable merb-more                # install only merb-more related dependencies
    # merb:dependencies:install stable --stack                  # install framework dependencies
    # merb:dependencies:install stable --no-stack               # install 3rd party dependencies
    # merb:dependencies:install stable --config                 # read dependencies from the default config
    # merb:dependencies:install stable --config-file file.yml   # read from the specified config file
    #
    # In addition to the options above, edge install uses the following: 
    #
    # merb:dependencies:install edge                            # install all dependencies using edge strategy
    # merb:dependencies:install edge --sources file.yml         # install edge from the specified git sources config
    
    desc 'install [stable|edge] [comp]', 'Install application dependencies'
    method_options "--sources" => :optional, # only for edge strategy
                   "--local"   => :boolean,  # force local install
                   "--dry-run" => :boolean, 
                   "--force"   => :boolean                   
    def install(strategy = 'stable', comp = nil)
      if strategy?(strategy)
        # Force local dependencies by creating ./gems before proceeding
        create_if_missing(default_gem_dir) if options[:local]
        
        where = gem_dir ? 'locally' : 'system-wide'
        
        # When comp == 'missing' then filter on missing dependencies
        if only_missing = comp == 'missing'
          message "Preparing to install missing gems #{where} using #{strategy} strategy..."
          comp = nil
        else
          message "Preparing to install #{where} using #{strategy} strategy..."
        end
        
        # If comp given, filter on known stack components
        deps = comp ? Merb::Stack.select_component_dependencies(dependencies, comp) : dependencies
        self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
        
        # Only install currently missing gems (for comp == missing)
        if only_missing
          deps.reject! { |dep| not missing.include?(dep) }
        end
        
        if deps.empty?
          warning "No dependencies to install..."
        else
          puts "#{deps.length} dependencies to install..."
          install_dependencies(strategy, deps)
        end
        
        # Show current dependency info now that we're done
        puts # Seperate output
        list('local', comp)
      else
        warning "Invalid install strategy '#{strategy}'"
        puts
        message "Please choose one of the following installation strategies: stable or edge:"
        puts "$ thor merb:dependencies:install stable"
        puts "$ thor merb:dependencies:install edge"
      end      
    end
    
    # Uninstall application dependencies.
    #
    # By default all required dependencies are installed. An optional argument 
    # can be used to filter on a set of known components.
    #
    # Existing dependencies will be clobbered; when :force => true then all gems
    # will be cleared, otherwise only existing local dependencies of the
    # matching component set will be removed.
    #
    # Examples:
    #
    # merb:dependencies:uninstall                               # uninstall all dependencies - the default
    # merb:dependencies:uninstall merb-more                     # uninstall merb-more related gems locally
    # merb:dependencies:uninstall --config                      # read dependencies from the default config
    
    desc 'uninstall [comp]', 'Uninstall application dependencies'
    method_options "--dry-run" => :boolean, "--force" => :boolean
    def uninstall(comp = nil)
      # If comp given, filter on known stack components
      deps = comp ? Merb::Stack.select_component_dependencies(dependencies, comp) : dependencies
      self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      # Clobber existing local dependencies - based on self.local
      clobber_dependencies!
    end
    
    # Recreate binary gems on the current platform.
    #
    # This task should be executed as part of a deployment setup, where the 
    # deployment system runs this after the app has been installed.
    # Usually triggered by Capistrano, God...
    #
    # It will regenerate gems from the bundled gems cache for any gem that has 
    # C extensions - which need to be recompiled for the target deployment platform.
    #
    # Note: gems/cache should be in your SCM for this to work correctly.
    
    desc 'redeploy', 'Recreate any binary gems on the target platform'
    method_options "--dry-run" => :boolean
    def redeploy
      require 'tempfile' # for Dir::tmpdir access
      if gem_dir && File.directory?(cache_dir = File.join(gem_dir, 'cache'))
        local_gemspecs.each do |gemspec|
          unless gemspec.extensions.empty?
            if File.exists?(gem_file = File.join(cache_dir, "#{gemspec.full_name}.gem"))
              gem_file_copy = File.join(Dir::tmpdir, File.basename(gem_file))
              if dry_run?
                note "Recreating #{gemspec.full_name}"
              else
                message "Recreating #{gemspec.full_name}"
                # Copy the gem to a temporary file, because otherwise RubyGems/FileUtils
                # will complain about copying identical files (same source/destination).
                FileUtils.cp(gem_file, gem_file_copy)
                Merb::Gem.install(gem_file_copy, :install_dir => gem_dir)
                File.delete(gem_file_copy)
              end
            end
          end
        end
      else
        error "No application local gems directory found"
      end
    end
    
    # Create a dependencies configuration file.
    #
    # A configuration yaml file will be created from the extracted application
    # dependencies. The format of the configuration is as follows:
    #
    # --- 
    # - merb-core (= 0.9.8, runtime)
    # - merb-slices (= 0.9.8, runtime)
    # 
    # This format is exactly the same as Gem::Dependency#to_s returns.
    #
    # Examples:
    #
    # merb:dependencies:configure --force                       # overwrite the default config file
    # merb:dependencies:configure --version 0.9.8               # configure specific framework version
    # merb:dependencies:configure --config-file file.yml        # write to the specified config file 
    
    desc 'configure [comp]', 'Create a dependencies config file'
    method_options "--dry-run" => :boolean, "--force" => :boolean
    def configure(comp = nil)
      # If comp given, filter on known stack components
      deps = comp ? Merb::Stack.select_component_dependencies(dependencies, comp) : dependencies
      config = YAML.dump(deps.map { |d| d.to_s })
      puts "#{config}\n"
      if File.exists?(config_file) && !options[:force]
        error "File already exists! Use --force to overwrite."
      else
        if dry_run?
          note "Written #{config_file}"
        else
          FileUtils.mkdir_p(config_dir) unless File.directory?(config_dir)
          File.open(config_file, 'w') { |f| f.write config }
          success "Written #{config_file}"
        end
      end
    rescue  
      error "Failed to write to #{config_file}"
    end 
    
    ### Helper Methods
    
    def strategy?(strategy)
      if self.respond_to?(method = :"#{strategy}_strategy", true)
        method
      end
    end
    
    def install_dependencies(strategy, deps)
      if method = strategy?(strategy)
        # Clobber existing local dependencies
        clobber_dependencies!
        
        # Run the chosen strategy - collect files installed from stable gems
        installed_from_stable = send(method, deps).map { |d| d.name }

        # Sleep a bit otherwise the following steps won't see the new files
        sleep(deps.length) if deps.length > 0
      
        # Leave a file to denote the strategy that has been used for this dependency
        self.local.each do |spec|
          next unless File.directory?(spec.full_gem_path)
          unless installed_from_stable.include?(spec.name)
            FileUtils.touch(File.join(spec.full_gem_path, "#{strategy}.strategy"))
          else
            FileUtils.touch(File.join(spec.full_gem_path, "stable.strategy"))
          end           
        end
        
        # Add local binaries for the installed framework dependencies
        comps = Merb::Stack.all_components & deps.map { |d| d.name }
        comps << { :no_minigems => 'merb-gen' }
        ensure_bin_wrapper_for(*comps)          
        return true
      end
      false
    end
    
    def dependencies
      if use_config?
        # Use preconfigured dependencies from yaml file
        deps = config_dependencies
      else
        # Extract dependencies from the current application
        deps = Merb::Stack.core_dependencies(gem_dir, ignore_dependencies?)
        deps += Merb::Dependencies.extract_dependencies(working_dir)
      end
      
      stack_components = Merb::Stack.components

      if options[:stack]
        # Limit to stack components only
        deps.reject! { |dep| not stack_components.include?(dep.name) }
      elsif options[:"no-stack"]
        # Limit to non-stack components
        deps.reject! { |dep| stack_components.include?(dep.name) }
      end
      
      if options[:version]
        version_req = ::Gem::Requirement.create("= #{options[:version]}")
      elsif core = deps.find { |d| d.name == 'merb-core' }
        version_req = core.version_requirements
      end
      
      if version_req
        # Handle specific version requirement for framework components
        framework_components = Merb::Stack.framework_components
        deps.each do |dep|
          if framework_components.include?(dep.name)
            dep.version_requirements = version_req
          end
        end
      end
            
      deps
    end
    
    def config_dependencies
      if File.exists?(config_file)
        self.class.parse_dependencies_yaml(File.read(config_file))
      else
        []
      end
    end
    
    def use_config?
      options[:config] || options[:"config-file"]
    end
    
    def config_file
      @config_file ||= begin
        options[:"config-file"] || File.join(working_dir, 'config', 'dependencies.yml')
      end
    end
    
    def config_dir
      File.dirname(config_file)
    end
    
    ### Strategy handlers
    
    private
    
    def stable_strategy(deps)
      installed_from_rubygems = []
      if core = deps.find { |d| d.name == 'merb-core' }
        if dry_run?
          note "Installing #{core.name}..."
        else
          if install_dependency(core)
            installed_from_rubygems << core
          else
            msg = "Try specifying a lower version of merb-core with --version"
            if version_no = core.version_requirements.to_s[/([\.\d]+)$/, 1]
              num = "%03d" % (version_no.gsub('.', '').to_i - 1)
              puts "The required version (#{version_no}) probably isn't available as a stable rubygem yet."
              info "#{msg} #{num.split(//).join('.')}"
            else
              puts "The required version probably isn't available as a stable rubygem yet."
              info msg
            end           
          end
        end
      end
      
      deps.each do |dependency|
        next if dependency.name == 'merb-core'
        if dry_run?
          note "Installing #{dependency.name}..."
        else
          install_dependency(dependency)
          installed_from_rubygems << dependency
        end        
      end
      installed_from_rubygems
    end
    
    def edge_strategy(deps)
      installed_from_rubygems = []
      
      # Selectively update repositories for the matching dependencies
      update_dependency_repositories(deps) unless dry_run?
      
      # Skip gem dependencies to prevent them from being installed from stable;
      # however, core dependencies will be retrieved from source when available
      install_opts = { :ignore_dependencies => true }
      if core = deps.find { |d| d.name == 'merb-core' }
        if dry_run?
          note "Installing #{core.name}..."
        else
          if install_dependency_from_source(core, install_opts)
          elsif install_dependency(core, install_opts)
            info "Installed #{core.name} from rubygems..."
            installed_from_rubygems << core
          end
        end
      end
      
      deps.each do |dependency|
        next if dependency.name == 'merb-core'
        if dry_run?
          note "Installing #{dependency.name}..."
        else
          if install_dependency_from_source(dependency, install_opts)
          elsif install_dependency(dependency, install_opts)
            info "Installed #{dependency.name} from rubygems..."
            installed_from_rubygems << dependency
          end
        end        
      end
      
      installed_from_rubygems
    end
    
    ### Class Methods
    
    public
    
    def self.list(filter = 'all', comp = nil, options = {})
      instance = Merb::Dependencies.new
      instance.options = options
      instance.list(filter, comp)
    end
    
    # Extract application dependencies by querying the app directly.
    def self.extract_dependencies(merb_root, env = 'production')
      require 'merb-core'
      if !@_merb_loaded || Merb.root != merb_root
        Merb.start_environment(
          :testing => true, 
          :adapter => 'runner', 
          :environment => env, 
          :merb_root => merb_root
        )
        @_merb_loaded = true
      end
      Merb::BootLoader::Dependencies.dependencies
    rescue => e
      error "Couldn't extract dependencies from application!"
      error e.message
      puts  "Make sure you're executing the task from your app (--merb-root), or"
      puts  "specify a config option (--config or --config-file=YAML_FILE)"
      return []
    end
        
    # Parse the basic YAML config data, and process Gem::Dependency output.
    # Formatting example: merb_helpers (>= 0.9.8, runtime)
    def self.parse_dependencies_yaml(yaml)
      dependencies = []
      entries = YAML.load(yaml) rescue []
      entries.each do |entry|
        if matches = entry.match(/^(\S+) \(([^,]+)?, ([^\)]+)\)/)
          name, version_req, type = matches.captures
          dependencies << ::Gem::Dependency.new(name, version_req, type.to_sym)
        else
          error "Invalid entry: #{entry}"
        end
      end
      dependencies
    end
    
  end  
  
  class Stack < Thor
    
    # The Stack tasks will install dependencies based on known sets of gems,
    # regardless of actual application dependency settings.
    
    DM_STACK = %w[
      extlib
      dm-core
      dm-aggregates
      dm-migrations
      dm-timestamps
      dm-types
      dm-validations
    ]
    
    MERB_STACK = %w[      
      extlib
      merb-core
      merb-action-args
      merb-assets
      merb-cache
      merb-helpers
      merb-mailer
      merb-slices
      merb-auth
    ] + DM_STACK
    
    MERB_BASICS = %w[      
      extlib
      merb-core
      merb-action-args
      merb-assets
      merb-cache
      merb-helpers
      merb-mailer
      merb-slices
    ]
    
    # The following sets are meant for repository lookup; unlike the sets above
    # these correspond to specific git repository items.
    
    MERB_MORE = %w[
      merb-action-args
      merb-assets
      merb-auth
      merb-auth-core
      merb-auth-more 
      merb-auth-slice-password
      merb-cache
      merb-exceptions
      merb-gen
      merb-haml
      merb-helpers
      merb-mailer
      merb-param-protection
      merb-slices
      merb_datamapper
    ]
    
    MERB_PLUGINS = %w[
      merb_activerecord
      merb_builder
      merb_jquery
      merb_laszlo
      merb_parts
      merb_screw_unit
      merb_sequel
      merb_stories
      merb_test_unit
    ]
    
    DM_MORE = %w[
      dm-adjust
      dm-aggregates
      dm-ar-finders
      dm-cli
      dm-constraints
      dm-is-example
      dm-is-list
      dm-is-nested_set
      dm-is-remixable
      dm-is-searchable
      dm-is-state_machine
      dm-is-tree
      dm-is-versioned
      dm-migrations
      dm-observer
      dm-querizer
      dm-serializer
      dm-shorthand
      dm-sweatshop
      dm-tags
      dm-timestamps
      dm-types
      dm-validations
      
      dm-couchdb-adapter
      dm-ferret-adapter
      dm-rest-adapter
    ]
    
    attr_accessor :system, :local, :missing
    
    include MerbThorHelper
    
    global_method_options = {
      "--merb-root"            => :optional,  # the directory to operate on
      "--include-dependencies" => :boolean,   # gather sub-dependencies
      "--version"              => :optional   # gather specific version of framework    
    }
    
    method_options global_method_options
    def initialize(*args); super; end
    
    # List components and their dependencies.
    #
    # Examples:
    # 
    # merb:stack:list                                           # list all standard stack components
    # merb:stack:list all                                       # list all component sets
    # merb:stack:list merb-more                                 # list all dependencies of merb-more
  
    desc 'list [all|comp]', 'List available components (optionally filtered, defaults to merb stack)'
    def list(comp = 'stack')
      if comp == 'all'
        Merb::Stack.component_sets.keys.sort.each do |comp|
          unless (components = Merb::Stack.component_sets[comp]).empty?
            message "Dependencies for '#{comp}' set:"
            components.each { |c| puts "- #{c}" }
          end
        end
      else
        message "Dependencies for '#{comp}' set:"
        Merb::Stack.components(comp).each { |c| puts "- #{c}" }
      end      
    end
    
    # Install stack components or individual gems - from stable rubygems by default.
    #
    # See also: Merb::Dependencies#install and Merb::Dependencies#install_dependencies
    #
    # Examples:
    #
    # merb:stack:install                                        # install the default merb stack
    # merb:stack:install basics                                 # install a basic set of dependencies
    # merb:stack:install merb-core                              # install merb-core from stable
    # merb:stack:install merb-more --edge                       # install merb-core from edge
    # merb:stack:install merb-core thor merb-slices             # install the specified gems                  
      
    desc 'install [COMP, ...]', 'Install stack components'
    method_options  "--edge"      => :boolean,
                    "--sources"   => :optional,
                    "--force"     => :boolean,
                    "--dry-run"   => :boolean,
                    "--strategy"  => :optional
    def install(*comps)
      mngr = self.dependency_manager
      deps = gather_dependencies(comps)
      mngr.system, mngr.local, mngr.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      mngr.install_dependencies(strategy, deps)
    end
        
    # Uninstall stack components or individual gems.
    #
    # See also: Merb::Dependencies#uninstall
    #
    # Examples:
    #
    # merb:stack:uninstall                                      # uninstall the default merb stack
    # merb:stack:uninstall merb-more                            # uninstall merb-more
    # merb:stack:uninstall merb-core thor merb-slices           # uninstall the specified gems
    
    desc 'uninstall [COMP, ...]', 'Uninstall stack components'
    method_options "--dry-run" => :boolean, "--force" => :boolean
    def uninstall(*comps)
      deps = gather_dependencies(comps)
      self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      # Clobber existing local dependencies - based on self.local
      clobber_dependencies!
    end
    
    # Install or uninstall minigems from the system.
    #
    # Due to the specific nature of MiniGems it can only be installed system-wide.
    #
    # Examples:
    #
    # merb:stack:minigems install                               # install minigems
    # merb:stack:minigems uninstall                             # uninstall minigems
    
    desc 'minigems (install|uninstall)', 'Install or uninstall minigems (needs sudo privileges)'
    def minigems(action)
      case action
      when 'install'
        Kernel.system "#{sudo} thor merb:stack:install_minigems"
      when 'uninstall'
        Kernel.system "#{sudo} thor merb:stack:uninstall_minigems"
      else
        error "Invalid command: merb:stack:minigems #{action}"
      end
    end    
    
    # hidden minigems install task
    def install_minigems
      message "Installing MiniGems"
      mngr = self.dependency_manager
      deps = gather_dependencies('minigems')
      mngr.system, mngr.local, mngr.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      mngr.force_gem_dir = ::Gem.dir
      mngr.install_dependencies(strategy, deps)
      Kernel.system "#{sudo} minigem install"
    end
    
    # hidden minigems uninstall task
    def uninstall_minigems
      message "Uninstalling MiniGems"
      Kernel.system "#{sudo} minigem uninstall"
      deps = gather_dependencies('minigems')
      self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      # Clobber existing local dependencies - based on self.local
      clobber_dependencies!      
    end
    
    protected
    
    def gather_dependencies(comps = [])
      if comps.empty?
        gems = MERB_STACK
      else
        gems = comps.map { |c| Merb::Stack.components(c) }.flatten
      end
      
      version_req = if options[:version]
        ::Gem::Requirement.create(options[:version])
      end
      
      framework_components = Merb::Stack.framework_components
      
      gems.map do |gem|
        if version_req && framework_components.include?(gem)
          ::Gem::Dependency.new(gem, version_req)
        else
          ::Gem::Dependency.new(gem, ::Gem::Requirement.default)
        end
      end
    end
    
    def strategy
      options[:strategy] || (options[:edge] ? 'edge' : 'stable')
    end
    
    def dependency_manager
      @_dependency_manager ||= begin
        instance = Merb::Dependencies.new
        instance.options = options
        instance
      end
    end
    
    public
    
    def self.repository_sets
      @_repository_sets ||= begin
        # the component itself as a fallback
        comps = Hash.new { |(hsh,c)| [c] }
        
        # git repository based component sets
        comps["merb"]         = ["merb-core"] + MERB_MORE
        comps["merb-more"]    = MERB_MORE.sort
        comps["merb-plugins"] = MERB_PLUGINS.sort
        comps["dm-more"]      = DM_MORE.sort
        
        comps
      end     
    end
    
    def self.component_sets
      @_component_sets ||= begin
        # the component itself as a fallback
        comps = Hash.new { |(hsh,c)| [c] }
        comps.update(repository_sets)
        
        # specific set of dependencies
        comps["stack"]        = MERB_STACK.sort
        comps["basics"]       = MERB_BASICS.sort
        
        # orm dependencies
        comps["datamapper"]   = DM_STACK.sort
        comps["sequel"]       = ["merb_sequel", "sequel"]
        comps["activerecord"] = ["merb_activerecord", "activerecord"]
        
        comps
      end
    end
    
    def self.framework_components
      %w[merb-core merb-more merb-plugins].inject([]) do |all, comp| 
        all + components(comp)
      end
    end
    
    def self.components(comp = nil)
      if comp
        component_sets[comp]
      else
        comps = %w[merb-core merb-more merb-plugins dm-core dm-more]
        comps.inject([]) do |all, grp|
          all + (component_sets[grp] || [])
        end
      end
    end
    
    def self.select_component_dependencies(dependencies, comp = nil)
      comps = components(comp) || []
      dependencies.select { |dep| comps.include?(dep.name) }
    end
    
    def self.base_components
      %w[thor rake]
    end
    
    def self.all_components
      base_components + framework_components
    end
    
    # Find the latest merb-core and gather its dependencies.
    # We check for 0.9.8 as a minimum release version.
    def self.core_dependencies(gem_dir = nil, ignore_deps = false)
      @_core_dependencies ||= begin
        if gem_dir # add local gems to index
          ::Gem.clear_paths; ::Gem.path.unshift(gem_dir)
        end
        deps = []
        merb_core = ::Gem::Dependency.new('merb-core', '>= 0.9.8')
        if gemspec = ::Gem.source_index.search(merb_core).last
          deps << ::Gem::Dependency.new('merb-core', gemspec.version)
          if ignore_deps 
            deps += gemspec.dependencies.select do |d| 
              base_components.include?(d.name)
            end
          else
            deps += gemspec.dependencies
          end
        end
        ::Gem.clear_paths if gem_dir # reset
        deps
      end
    end
    
    def self.lookup_repository_name(item)
      set_name = nil
      # The merb repo contains -more as well, so it needs special attention
      return 'merb' if self.repository_sets['merb'].include?(item)
      
      # Proceed with finding the item in a known component set
      self.repository_sets.find do |set, items| 
        next if set == 'merb'
        items.include?(item) ? (set_name = set) : nil
      end
      set_name
    end
    
  end
  
  class Tasks < Thor
    
    include MerbThorHelper
    
    # Show merb.thor version information
    #
    # merb:tasks:version                                        # show the current version info
    # merb:tasks:version --info                                 # show extended version info
    
    desc 'version', 'Show verion info'
    method_options "--info" => :boolean
    def version
      message "Currently installed merb.thor version: #{MERB_THOR_VERSION}"
      if options[:version]
        self.options = { :"dry-run" => true }
        self.update # run update task with dry-run enabled
      end
    end
    
    # Update merb.thor tasks from remotely available version
    #
    # merb:tasks:update                                        # update merb.thor
    # merb:tasks:update --force                                # force-update merb.thor
    # merb:tasks:update --dry-run                              # show version info only
    
    desc 'update [URL]', 'Fetch the latest merb.thor and install it locally'
    method_options "--dry-run" => :boolean, "--force" => :boolean
    def update(url = 'http://merbivore.com/merb.thor')
      require 'open-uri'
      require 'rubygems/version'
      remote_file = open(url)
      code = remote_file.read
      
      # Extract version information from the source code
      if version = code[/^MERB_THOR_VERSION\s?=\s?('|")([\.\d]+)('|")/,2]
        # borrow version comparison from rubygems' Version class
        current_version = ::Gem::Version.new(MERB_THOR_VERSION)
        remote_version  = ::Gem::Version.new(version)
        
        if current_version >= remote_version
          puts "currently installed: #{current_version}"
          if current_version != remote_version
            puts "available version:   #{remote_version}"
          end
          info "No update of merb.thor necessary#{options[:force] ? ' (forced)' : ''}"
          proceed = options[:force]
        elsif current_version < remote_version
          puts "currently installed: #{current_version}"
          puts "available version:   #{remote_version}"
          proceed = true
        end
          
        if proceed && !dry_run?
          File.open(File.join(__FILE__), 'w') do |f|
            f.write(code)
          end
          success "Installed the latest merb.thor (v#{version})"
        end
      else
        raise "invalid source-code data"
      end      
    rescue OpenURI::HTTPError
      error "Error opening #{url}"
    rescue => e
      error "An error occurred (#{e.message})"
    end
    
  end
  
  #### MORE LOW-LEVEL TASKS ####
  
  class Gem < Thor
    
    group 'core'
    
    include MerbThorHelper
    extend GemManagement
    
    attr_accessor :system, :local, :missing
    
    global_method_options = {
      "--merb-root"            => :optional,  # the directory to operate on
      "--version"              => :optional,  # gather specific version of gem
      "--ignore-dependencies"  => :boolean    # don't install sub-dependencies
    }
    
    method_options global_method_options
    def initialize(*args); super; end
    
    # List gems that match the specified criteria.
    #
    # By default all local gems are listed. When the first argument is 'all' the
    # list is partitioned into system an local gems; specify 'system' to show
    # only system gems. A second argument can be used to filter on a set of known
    # components, like all merb-more gems for example.
    # 
    # Examples:
    #
    # merb:gem:list                                    # list all local gems - the default
    # merb:gem:list all                                # list system and local gems
    # merb:gem:list system                             # list only system gems
    # merb:gem:list all merb-more                      # list only merb-more related gems
    # merb:gem:list --version 0.9.8                    # list gems that match the version    
       
    desc 'list [all|local|system] [comp]', 'Show installed gems'
    def list(filter = 'local', comp = nil)
      deps = comp ? Merb::Stack.select_component_dependencies(dependencies, comp) : dependencies
      self.system, self.local, self.missing = Merb::Gem.partition_dependencies(deps, gem_dir)
      case filter
      when 'all'
        message 'Installed system gems:'
        display_gemspecs(system)
        message 'Installed local gems:'
        display_gemspecs(local)
      when 'system'
        message 'Installed system gems:'
        display_gemspecs(system)
      when 'local'
        message 'Installed local gems:'
        display_gemspecs(local)
      else
        warning "Invalid listing filter '#{filter}'"
      end
    end
    
    # Install the specified gems.
    #
    # All arguments should be names of gems to install.
    #
    # When :force => true then any existing versions of the gems to be installed
    # will be uninstalled first. It's important to note that so-called meta-gems
    # or gems that exactly match a set of Merb::Stack.components will have their
    # sub-gems uninstalled too. For example, uninstalling merb-more will install
    # all contained gems: merb-action-args, merb-assets, merb-gen, ...
    # 
    # Examples:
    #
    # merb:gem:install merb-core merb-slices          # install all specified gems
    # merb:gem:install merb-core --version 0.9.8      # install a specific version of a gem
    # merb:gem:install merb-core --force              # uninstall then subsequently install the gem
    # merb:gem:install merb-core --cache              # try to install locally from system gems
    # merb:gem:install merb-core --binaries           # also install adapted bin wrapper
     
    desc 'install GEM_NAME [GEM_NAME, ...]', 'Install a gem from rubygems'
    method_options "--cache"     => :boolean,
                   "--binaries"  => :boolean,
                   "--dry-run"   => :boolean,
                   "--force"     => :boolean
    def install(*names)
      self.include_dependencies = true # deal with dependencies by default
      opts = { :version => options[:version], :cache => options[:cache] }
      current_gem = nil
      
      # uninstall existing gems of the ones we're going to install
      uninstall(*names) if options[:force]
      
      names.each do |gem_name|
        current_gem = gem_name      
        if dry_run?
          note "Installing #{current_gem}..."
        else
          message "Installing #{current_gem}..."
          self.class.install(gem_name, default_install_options.merge(opts))
          ensure_bin_wrapper_for(gem_name) if options[:binaries]
        end
      end
    rescue => e
      error "Failed to install #{current_gem ? current_gem : 'gem'} (#{e.message})"
    end
    
    # Uninstall the specified gems.
    #
    # By default all specified gems are uninstalled. It's important to note that 
    # so-called meta-gems or gems that match a set of Merb::Stack.components will 
    # have their sub-gems uninstalled too. For example, uninstalling merb-more 
    # will install all contained gems: merb-action-args, merb-assets, ...
    #
    # Existing dependencies will be clobbered; when :force => true then all gems
    # will be cleared, otherwise only existing local dependencies of the
    # matching component set will be removed.
    #
    # Examples:
    #
    # merb:gem:uninstall merb-core merb-slices        # uninstall all specified gems
    # merb:gem:uninstall merb-core --version 0.9.8    # uninstall a specific version of a gem
    
    desc 'uninstall GEM_NAME [GEM_NAME, ...]', 'Unstall a gem'
    method_options "--dry-run" => :boolean
    def uninstall(*names)
      self.include_dependencies = true # deal with dependencies by default
      opts = { :version => options[:version] }
      current_gem = nil
      if dry_run?
        note "Uninstalling any existing gems of: #{names.join(', ')}"
      else
        message "Uninstalling any existing gems of: #{names.join(', ')}"
        names.each do |gem_name|
          current_gem = gem_name
          Merb::Gem.uninstall(gem_name, default_uninstall_options) rescue nil
          # if this gem is a meta-gem or a component set name, remove sub-gems
          (Merb::Stack.components(gem_name) || []).each do |comp|
            Merb::Gem.uninstall(comp, default_uninstall_options) rescue nil
          end
        end
      end 
    rescue => e
      error "Failed to uninstall #{current_gem ? current_gem : 'gem'} (#{e.message})"
    end
    
    private
    
    # Return dependencies for all installed gems; both system-wide and locally;
    # optionally filters on :version requirement.
    def dependencies
      version_req = if options[:version]
        ::Gem::Requirement.create(options[:version])
      else
        ::Gem::Requirement.default
      end
      if gem_dir
        ::Gem.clear_paths; ::Gem.path.unshift(gem_dir)
        ::Gem.source_index.refresh!
      end
      deps = []
      ::Gem.source_index.each do |fullname, gemspec| 
        if version_req.satisfied_by?(gemspec.version)
          deps << ::Gem::Dependency.new(gemspec.name, "= #{gemspec.version}")
        end
      end
      ::Gem.clear_paths if gem_dir
      deps.sort
    end
    
    public
    
    # Install gem with some default options.
    def self.install(name, options = {})
      defaults = {}
      defaults[:cache] = false unless opts[:install_dir]
      install_gem(name, defaults.merge(options))
    end
    
    # Uninstall gem with some default options.
    def self.uninstall(name, options = {})
      defaults = { :ignore => true, :executables => true }
      uninstall_gem(name, defaults.merge(options))
    end
    
  end
  
  class Source < Thor
    
    group 'core'
        
    include MerbThorHelper
    extend GemManagement
    
    attr_accessor :system, :local, :missing
    
    global_method_options = {
      "--merb-root"            => :optional,  # the directory to operate on
      "--include-dependencies" => :boolean,   # gather sub-dependencies
      "--sources"              => :optional   # a yml config to grab sources from
    }
    
    method_options global_method_options
    def initialize(*args); super; end
        
    # List source repositories, of either local or known sources.
    #
    # Examples:
    #
    # merb:source:list                                   # list all local sources
    # merb:source:list available                         # list all known sources
    
    desc 'list [local|available]', 'Show git source repositories'
    def list(mode = 'local')
      if mode == 'available'
        message 'Available source repositories:'
        repos = self.class.repos(options[:sources])
        repos.keys.sort.each { |name| puts "- #{name}: #{repos[name]}" }
      elsif mode == 'local'
        message 'Current source repositories:'
        Dir[File.join(source_dir, '*')].each do |src|
          next unless File.directory?(src)
          src_name = File.basename(src)
          unless (repos = source_manager.existing_repos(src_name)).empty?
            puts "#{src_name}"
            repos.keys.sort.each { |b| puts "- #{b}: #{repos[b]}" }
          end
        end
      else
        error "Unknown listing: #{mode}"
      end
    end

    # Install the specified gems.
    #
    # All arguments should be names of gems to install.
    #
    # When :force => true then any existing versions of the gems to be installed
    # will be uninstalled first. It's important to note that so-called meta-gems
    # or gems that exactly match a set of Merb::Stack.components will have their
    # sub-gems uninstalled too. For example, uninstalling merb-more will install
    # all contained gems: merb-action-args, merb-assets, merb-gen, ...
    # 
    # Examples:
    #
    # merb:source:install merb-core merb-slices          # install all specified gems
    # merb:source:install merb-core --force              # uninstall then subsequently install the gem
    # merb:source:install merb-core --wipe               # clear repo then install the gem
    # merb:source:install merb-core --binaries           # also install adapted bin wrapper

    desc 'install GEM_NAME [GEM_NAME, ...]', 'Install a gem from git source/edge'
    method_options "--binaries"  => :boolean,
                   "--dry-run"   => :boolean,
                   "--force"     => :boolean,
                   "--wipe"      => :boolean
    def install(*names)
      # uninstall existing gems of the ones we're going to install
      uninstall(*names) if options[:force] || options[:wipe]
      
      # We want dependencies instead of just names
      deps = names.map { |n| ::Gem::Dependency.new(n, ::Gem::Requirement.default) }
      
      # Selectively update repositories for the matching dependencies
      update_dependency_repositories(deps) unless dry_run?
      
      current_gem = nil
      deps.each do |dependency|
        current_gem = dependency.name      
        if dry_run?
          note "Installing #{current_gem} from source..."
        else
          message "Installing #{current_gem} from source..."
          if install_dependency_from_source(dependency)
            ensure_bin_wrapper_for(dependency.name) if options[:binaries]
          end
        end
      end
    rescue => e
      error "Failed to install #{current_gem ? current_gem : 'gem'} (#{e.message})"
    end
    
    # Uninstall the specified gems.
    #
    # By default all specified gems are uninstalled. It's important to note that 
    # so-called meta-gems or gems that match a set of Merb::Stack.components will 
    # have their sub-gems uninstalled too. For example, uninstalling merb-more 
    # will install all contained gems: merb-action-args, merb-assets, ...
    #
    # Existing dependencies will be clobbered; when :force => true then all gems
    # will be cleared, otherwise only existing local dependencies of the
    # matching component set will be removed. Additionally when :wipe => true, 
    # the matching git repositories will be removed from the source directory.
    #
    # Examples:
    #
    # merb:source:uninstall merb-core merb-slices       # uninstall all specified gems
    # merb:source:uninstall merb-core --wipe            # force-uninstall a gem and clear repo
    
    desc 'uninstall GEM_NAME [GEM_NAME, ...]', 'Unstall a gem (specify --force to remove the repo)'
    method_options "--version" => :optional, "--dry-run" => :boolean, "--wipe" => :boolean
    def uninstall(*names)
      # Remove the repos that contain the gem
      if options[:wipe] 
        extract_repositories(names).each do |(name, url)|
          if File.directory?(src = File.join(source_dir, name))
            if dry_run?
              note "Removing #{src}..."
            else
              info "Removing #{src}..."
              FileUtils.rm_rf(src)
            end
          end
        end
      end
      
      # Use the Merb::Gem#uninstall task to handle this
      gem_tasks = Merb::Gem.new
      gem_tasks.options = options
      gem_tasks.uninstall(*names)
    end
    
    # Update the specified source repositories.
    #
    # The arguments can be actual repository names (from Merb::Source.repos)
    # or names of known merb stack gems. If the repo doesn't exist already,
    # it will be created and cloned.
    #
    # merb:source:pull merb-core                         # update source of specified gem
    # merb:source:pull merb-slices                       # implicitly updates merb-more
    
    desc 'pull REPO_NAME [GEM_NAME, ...]', 'Update git source repository from edge'
    def pull(*names)
      repos = extract_repositories(names)
      update_repositories(repos)
      unless repos.empty?
        message "Updated the following repositories:"
        repos.each { |name, url| puts "- #{name}: #{url}" }
      else
        warning "No repositories found to update!"
      end
    end    
    
    # Clone a git repository into ./src. 
    
    # The repository can be a direct git url or a known -named- repository.
    #
    # Examples:
    #
    # merb:source:clone merb-core 
    # merb:source:clone dm-core awesome-repo
    # merb:source:clone dm-core --sources ./path/to/sources.yml
    # merb:source:clone git://github.com/sam/dm-core.git
    
    desc 'clone (REPO_NAME|URL) [DIR_NAME]', 'Clone git source repository by name or url'
    def clone(repository, name = nil)
      if repository =~ /^git:\/\//
        repository_url  = repository
        repository_name = File.basename(repository_url, '.git')
      elsif url = Merb::Source.repo(repository, options[:sources])
        repository_url = url
        repository_name = repository
      end
      source_manager.clone(name || repository_name, repository_url)
    end
    
    # Git repository sources - pass source_config option to load a yaml 
    # configuration file - defaults to ./config/git-sources.yml and
    # ~/.merb/git-sources.yml - which you need to create yourself. 
    #
    # Example of contents:
    #
    # merb-core: git://github.com/myfork/merb-core.git
    # merb-more: git://github.com/myfork/merb-more.git
    
    def self.repos(source_config = nil)
      source_config ||= begin
        local_config = File.join(Dir.pwd, 'config', 'git-sources.yml')
        user_config  = File.join(ENV["HOME"] || ENV["APPDATA"], '.merb', 'git-sources.yml')
        File.exists?(local_config) ? local_config : user_config
      end
      if source_config && File.exists?(source_config)
        default_repos.merge(YAML.load(File.read(source_config)))
      else
        default_repos
      end
    end
    
    def self.repo(name, source_config = nil)
      self.repos(source_config)[name]
    end
    
    # Default Git repositories
    def self.default_repos
      @_default_repos ||= { 
        'merb'          => "git://github.com/wycats/merb.git",
        'merb-plugins'  => "git://github.com/wycats/merb-plugins.git",
        'extlib'        => "git://github.com/sam/extlib.git",
        'dm-core'       => "git://github.com/sam/dm-core.git",
        'dm-more'       => "git://github.com/sam/dm-more.git",
        'sequel'        => "git://github.com/wayneeseguin/sequel.git",
        'do'            => "git://github.com/sam/do.git",
        'thor'          => "git://github.com/wycats/thor.git",
        'rake'          => "git://github.com/jimweirich/rake.git"
      }
    end
       
  end
  
end