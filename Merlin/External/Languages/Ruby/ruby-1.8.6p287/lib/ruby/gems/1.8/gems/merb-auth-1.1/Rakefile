require File.expand_path(File.join(File.dirname(__FILE__), "..", "rake_helpers"))
require 'fileutils'
include FileUtils
require 'rake/clean'

RUBY_FORGE_PROJECT  = "merb-auth"
PROJECT_URL         = "http://merbivore.com"
PROJECT_SUMMARY     = "merb-auth.  The official authentication plugin for merb.  Setup for the default stack"
PROJECT_DESCRIPTION = PROJECT_SUMMARY

GEM_AUTHOR = "Daniel Neighman"
GEM_EMAIL  = "has.sox@gmail.com"

GEM_NAME    = "merb-auth"
PKG_BUILD   = ENV['PKG_BUILD'] ? '.' + ENV['PKG_BUILD'] : ''
GEM_VERSION = Merb::VERSION + PKG_BUILD

RELEASE_NAME    = "REL #{GEM_VERSION}"

gems = %w[
  merb-auth-core merb-auth-more merb-auth-slice-password
]

desc "Publish Merb More gem to RubyForge, one by one."
task :release do
  packages = %w( gem tgz zip ).collect{ |ext| "pkg/#{GEM_NAME}-#{GEM_VERSION}.#{ext}" }

  begin
    sh %{rubyforge login}
    sh %{rubyforge add_release #{RUBY_FORGE_PROJECT} #{GEM_NAME} #{GEM_VERSION} #{packages.join(' ')}}
    sh %{rubyforge add_file #{RUBY_FORGE_PROJECT} #{GEM_NAME} #{GEM_VERSION} #{packages.join(' ')}}
  rescue Exception => e
    puts
    puts "Release failed: #{e.message}"
    puts
    puts "Set PKG_BUILD environment variable if you do a subrelease (0.9.4.2008_08_02 when version is 0.9.4)"
  end
  
  %w(merb-auth-core merb-auth-more merb-auth-slice-password).each do |gem|
    Dir.chdir(gem){ sh "#{Gem.ruby} -S rake release" }
  end
end

merb_auth_spec = Gem::Specification.new do |s|
  s.rubyforge_project = RUBY_FORGE_PROJECT
  s.name         = GEM_NAME
  s.version      = GEM_VERSION
  s.platform     = Gem::Platform::RUBY
  s.author       = GEM_AUTHOR
  s.email        = GEM_EMAIL
  s.homepage     = "http://www.merbivore.com"
  s.summary      = PROJECT_SUMMARY
  s.description  = PROJECT_SUMMARY
  s.files = %w(LICENSE README.textile Rakefile TODO) + Dir.glob("{lib,spec}/**/*")
  s.add_dependency "merb-core", "~> #{GEM_VERSION}"
  gems.each do |gem|
    s.add_dependency gem, "~> #{GEM_VERSION}"
  end
end

CLEAN.include ["**/.*.sw?", "pkg", "lib/*.bundle", "*.gem", "doc/rdoc", ".config", "coverage", "cache"]

Rake::GemPackageTask.new(merb_auth_spec) do |package|
  package.gem_spec = merb_auth_spec
end

task :package => ["lib/merb-auth.rb", :build_children]
desc "Create merb-auth.rb"
task "lib/merb-auth.rb" do
  mkdir_p "lib"
  File.open("lib/merb-auth.rb","w+") do |file|
    file.puts "### AUTOMATICALLY GENERATED. DO NOT EDIT!"
    gems.each do |gem|
      next if gem == "merb-gen"
      file.puts "require '#{gem}'"
    end
  end
end

task :build_children do
  %w(merb-auth-core merb-auth-more merb-auth-slice-password).each do |dir|
    Dir.chdir(dir) { sh "#{Gem.ruby} -S rake package" }
  end  
end

desc "install the plugin as a gem"
task :install do
  Merb::RakeHelper.install(GEM_NAME, :version => GEM_VERSION)
end

desc "Uninstall the gem"
task :uninstall do
  Merb::RakeHelper.uninstall(GEM_NAME, :version => GEM_VERSION)
end

desc "Create a gemspec file"
task :gemspec do
  File.open("#{GEM_NAME}.gemspec", "w") do |file|
    file.puts spec.to_ruby
  end
end

desc "Run all specs"
task :spec do
  gems.each do |gem|
    Dir.chdir(gem) { sh "#{Gem.ruby} -S rake spec" }
  end
end
