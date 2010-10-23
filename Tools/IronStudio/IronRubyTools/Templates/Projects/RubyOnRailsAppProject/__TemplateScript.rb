app_name = "$safeprojectname$"

puts "Creating Rails app #{app_name} in #{Dir.pwd} ..."

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

def create_database server, db
  puts "Creating database #{db} on #{server} ..."

  require 'System.Data'
  sql_client = System::Data::SqlClient
  
  connection = sql_client::SqlConnection.new("Data Source=#{server};Integrated Security=True")
  command = sql_client::SqlCommand.new("CREATE DATABASE [#{db}]", connection)
  connection.open
  command.execute_non_query

  puts 'Database created.'
rescue Exception => e
  puts "Error: #{e.message}"
ensure
  connection.close unless connection.nil?
end

# generate application files:

rails_version = "3.0.1"
sqlserver_adapter_version = "3.0.2"
gems_installed = false

begin
  gem 'rails', rails_version
rescue Gem::LoadError
  raise if gems_installed
  
  # install gems:
  gem_home = GemCmd.new.install(["rails", rails_version], ["activerecord-sqlserver-adapter", sqlserver_adapter_version])
  exit 1 if gem_home.nil?
  ENV['GEM_PATH'] = gem_home if gem_home != ENV['GEM_PATH'] 
  
  gems_installed = true
  retry
end

# create rails app:
ARGV.clear
ARGV << "new"
ARGV << app_name
require "rails/cli"

# create database:
create_database "$machinename$\\SQLEXPRESS", "$safeprojectname$"
