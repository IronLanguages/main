puts "Creating Gem $safeprojectname$ in #{Dir.pwd} ..."

require 'rubygems'
require 'rubygems/commands/install_command'

class GemCmd < Gem::Commands::InstallCommand
  def initialize
    super
    options[:user_install] = true
    options[:cache_dir] = Gem.user_dir
    options[:generate_rdoc] = false
    options[:generate_ri] = false
  end
  
  def install *gems
    puts "Installing missing gems: #{gems.map { |name, version| if version.nil? then name else "#{name} (#{version})" end }.join(", ")}"
    gem_home = nil
    all_installed_gems = []

    gems.each do |gem_name, gem_version|
      begin

        inst = Gem::DependencyInstaller.new options
        inst.install gem_name, gem_version || options[:version]
    
        inst.installed_gems.each do |spec|          
          gem_home ||= File.expand_path(File.join(File.dirname(spec.loaded_from), '..'))                    
          all_installed_gems << spec
          puts "Successfully installed #{spec.full_name}"
        end

      rescue Gem::InstallError => e
        STDERR.puts "Error installing #{gem_name}:\n\t#{e.message}"
      rescue Gem::GemNotFoundException => e
        STDERR.puts e.message
      rescue Exception => e
        STDERR.puts e.message
      end
    end
    
    gems = all_installed_gems.length == 1 ? 'gem' : 'gems'
    puts "#{all_installed_gems.length} #{gems} installed"
        
    gem_home
  end
end

rspec_version = "1.3.0"
gems_installed = false

begin
  gem 'rspec', rspec_version
rescue Gem::LoadError
  raise if gems_installed
  
  # install gems:
  gem_home = GemCmd.new.install(["rspec", rspec_version])
  exit 1 if gem_home.nil?
  ENV['GEM_PATH'] = gem_home if gem_home != ENV['GEM_PATH'] 
  
  gems_installed = true
  retry
end
