## THESE ARE CRUCIAL
require File.join(File.dirname(__FILE__), "merb-core/lib/merb-core/version.rb")

require "rake/clean"
require "rake/gempackagetask"
require File.join(File.dirname(__FILE__), 'merb-core/lib/merb-core/tasks/merb_rake_helper')
require 'fileutils'
include FileUtils

merb_more_gem_paths = %w[
  merb-action-args 
  merb-assets 
  merb-slices
  merb-auth
  merb-cache 
  merb-exceptions
  merb-gen 
  merb-haml
  merb-helpers 
  merb-mailer 
  merb-param-protection
]

merb_release = {
  "merb-auth" => 
    [
      "merb-auth",
      "merb-auth-core",
      "merb-auth-more",
      "merb-auth-slice-password"
    ],
  "merb" =>
    [
      "merb-action-args",
      "merb-assets",
      "merb-slices",
      "merb-cache",
      "merb-core",
      "merb-exceptions",
      "merb-gen",
      "merb-haml",
      "merb-helpers",
      "merb-mailer",
      "merb-param-protection",
      "merb_datamapper",
      "merb",
      "merb-more"
    ]
}

merb_gem_paths = %w[merb merb-core merb_datamapper] + merb_more_gem_paths

merb_gems = merb_gem_paths.map { |p| File.basename(p) }
merb_more_gems = merb_more_gem_paths.map { |p| File.basename(p) }

merb_more_spec = Gem::Specification.new do |s|
  s.rubyforge_project = 'merb-more'
  s.name         = "merb-more"
  s.version      = Merb::VERSION
  s.platform     = Gem::Platform::RUBY
  s.author       = "Engine Yard"
  s.email        = "merb@engineyard.com"
  s.homepage     = "http://www.merbivore.com"
  s.summary      = "(merb - merb-core) == merb-more.  The Full Stack. Take what you need; leave what you don't."
  s.description  = s.summary
  s.files        = %w( LICENSE README Rakefile TODO lib/merb-more.rb )
  s.required_rubygems_version = ">= 1.3.0"
  s.add_dependency "merb-core", ">= #{Merb::VERSION}"
  merb_more_gems.each do |gem|
    s.add_dependency gem, "= #{Merb::VERSION}"
  end
end

CLEAN.include ["**/.*.sw?", "pkg", "lib/*.bundle", "*.gem", "doc/rdoc", ".config", "coverage", "cache", "lib/merb-more.rb", "gems/*"]

Rake::GemPackageTask.new(merb_more_spec) do |package|
  package.gem_spec = merb_more_spec
end

namespace :install do
  
  desc "Install core gem"
  task :core => :clean do
    Merb::RakeHelper.install('merb-core', :version => Merb::VERSION)
  end
      
  desc "Install all merb-more gems"
  task :more => :clean do
    merb_more_gems.each do |gem|
      Merb::RakeHelper.install(gem, :version => Merb::VERSION)
    end
    Merb::RakeHelper.install("merb", :version => Merb::VERSION)
  end
  
end

namespace :uninstall do
  
  desc "Uninstall core gem"
  task :core do
    Merb::RakeHelper.uninstall('merb-core', :version => Merb::VERSION)
  end
      
  desc "Uninstall all merb-more gems"
  task :more do
    merb_more_gems.each do |gem|
      Merb::RakeHelper.uninstall(gem, :version => Merb::VERSION)
    end
  end
  
end

desc "Install all gems"
task :install do
  merb_gems.each do |gem|
    Merb::RakeHelper.install(gem, :version => Merb::VERSION)
  end
  puts %x{sudo gem install pkg/merb-more-#{Merb::VERSION}.gem}
end

desc "Uninstall all gems"
task :uninstall => ['uninstall:core', 'uninstall:more']

desc "Build the merb-more gems"
task :build_gems do
  merb_gem_paths.each do |dir|
    Dir.chdir(dir) { sh "#{Gem.ruby} -S rake package" }
  end
end

desc "Clobber the merb-more sub-gems"
task :clobber_gems do
  merb_gem_paths.each do |dir|
    Dir.chdir(dir) { sh "#{Gem.ruby} -S rake clobber" }
  end
end

task :package => ["lib/merb-more.rb", :build_gems]
desc "Create merb-more.rb"
task "lib/merb-more.rb" do
  mkdir_p "lib"
  File.open("lib/merb-more.rb","w+") do |file|
    file.puts "### AUTOMATICALLY GENERATED. DO NOT EDIT!"
    merb_more_gems.each do |gem|
      next if gem == "merb-gen"
      file.puts "require \"#{gem}\""
    end
  end
end

task :package do
  mkdir_p "gems"
  Dir["**/pkg/*.gem"].each do |file|
    FileUtils.cp(file, "gems")
  end
end

# This task is only for releasing edge gems on edge.merbivore.com

task :release_edge => :package do
  FileUtils.cp(Dir["gems/*.gem"], "../gems")
  Dir.chdir("..") do
    `gem generate_index`
  end
end

RUBY_FORGE_PROJECT = "merb"

GROUP_NAME    = "merb"
PKG_BUILD     = ENV['PKG_BUILD'] ? '.' + ENV['PKG_BUILD'] : ''
PKG_VERSION   = Merb::VERSION + PKG_BUILD

RELEASE_NAME  = "REL #{PKG_VERSION}"

namespace :release do
  desc "Publish Merb More release files to RubyForge."
  task :merb_more => [ :package ] do
    require 'rubyforge'
    require 'rake/contrib/rubyforgepublisher'

    packages = %w( gem tgz zip ).collect{ |ext| "pkg/merb-more-#{PKG_VERSION}.#{ext}" }

    begin
      sh %{rubyforge login}
      sh %{rubyforge add_release #{RUBY_FORGE_PROJECT} merb-more #{Merb::VERSION} #{packages.join(' ')}}
      sh %{rubyforge add_file #{RUBY_FORGE_PROJECT} merb-more #{Merb::VERSION} #{packages.join(' ')}}
    rescue Exception => e
      puts
      puts "Release failed: #{e.message}"
      puts
      puts "Set PKG_BUILD environment variable if you do a subrelease (0.9.4.2008_08_02 when version is 0.9.4)"
    end
  end

  desc "Publish Merb More gem to RubyForge, one by one."
  task :merb_more_gems => [ :build_gems ] do
    merb_more_gems.each do |gem|
      Dir.chdir(gem){ sh "#{Gem.ruby} -S rake release" }
    end
  end

  desc "Publish Merb release files to RubyForge."
  task :merb => [ :package ] do
    require 'rubyforge'
    
    r = RubyForge.new
    r.configure
    
    puts "\nLogging in...\n\n"
    r.login
    
    merb_release.each do |project, packages|
      packages.each do |package|
        begin
          puts "Adding #{project}: #{package}"
          file = "gems/#{package}-#{PKG_VERSION}.gem"
          r.add_release project, package, Merb::VERSION, file
          r.add_file    project, package, Merb::VERSION, file
        rescue Exception => e
          if e.message =~ /You have already released this version/
            puts "You already released #{project}: #{package}. Continuing\n\n"
          else
            raise e
          end
        end
      end
    end

  end
end

desc "Run spec examples for Merb More gems, one by one."
task :spec do
  merb_gem_paths.each do |gem|
    Dir.chdir(gem) { sh "#{Gem.ruby} -S rake spec" }
  end
end

desc 'Default: run spec examples for all the gems.'
task :default => 'spec'

