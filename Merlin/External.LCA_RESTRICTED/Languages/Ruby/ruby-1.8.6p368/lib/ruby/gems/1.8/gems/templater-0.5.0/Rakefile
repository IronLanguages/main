require 'rubygems'
require 'rake/gempackagetask'
require 'rubygems/specification'
require 'rake/rdoctask'
require 'date'
require 'spec/rake/spectask'
require File.join(File.dirname(__FILE__), 'lib', 'templater')

PLUGIN = "templater"
NAME = "templater"
AUTHOR = "Jonas Nicklas, Michael Klishin"
EMAIL = "jonas.nicklas@gmail.com, michael.s.klishin@gmail.com"
HOMEPAGE = "http://templater.rubyforge.org/"
SUMMARY = "File generation system"


# Used by release task
RUBY_FORGE_PROJECT  = "templater"
GEM_NAME            = NAME
PROJECT_URL         = HOMEPAGE
PROJECT_SUMMARY     = SUMMARY
PROJECT_DESCRIPTION = SUMMARY

PKG_BUILD    = ENV['PKG_BUILD'] ? '.' + ENV['PKG_BUILD'] : ''
GEM_VERSION  = Templater::VERSION + PKG_BUILD
RELEASE_NAME = "REL #{GEM_VERSION}"

require "extlib/tasks/release"

#
# ==== Gemspec and installation
#

spec = Gem::Specification.new do |s|
  s.name = NAME
  s.version = Templater::VERSION
  s.platform = Gem::Platform::RUBY
  s.has_rdoc = true
  s.extra_rdoc_files = ["README", "LICENSE", 'ROADMAP']
  s.summary = SUMMARY
  s.description = s.summary
  s.author = AUTHOR
  s.email = EMAIL
  s.homepage = HOMEPAGE
  s.require_path = 'lib'
  s.autorequire = PLUGIN
  s.files = %w(LICENSE README Rakefile ROADMAP) + Dir.glob("{lib,spec}/**/*")
  
  s.add_dependency "highline", ">= 1.4.0"
  s.add_dependency "diff-lcs", ">= 1.1.2"
  s.add_dependency "extlib", ">= 0.9.5"
end

Rake::GemPackageTask.new(spec) do |pkg|
  pkg.gem_spec = spec
end

desc "removes any generated content"
task :clean do
  FileUtils.rm_rf "clobber/*"
  FileUtils.rm_rf "pkg/*"
end

desc "install the plugin locally"
task :install => [:clean, :package] do
  sh %{sudo gem install pkg/#{NAME}-#{Templater::VERSION} --no-update-sources}
end

desc "create a gemspec file"
task :make_spec do
  File.open("#{GEM}.gemspec", "w") do |file|
    file.puts spec.to_ruby
  end
end

namespace :jruby do

  desc "Run :package and install the resulting .gem with jruby"
  task :install => :package do
    sh %{#{SUDO} jruby -S gem install pkg/#{NAME}-#{Templater::VERSION}.gem --no-rdoc --no-ri}
  end
  
end

#
# ==== RDoc
#

desc 'Generate documentation for Templater.'
Rake::RDocTask.new(:doc) do |rdoc|
  rdoc.rdoc_dir = 'doc'
  rdoc.title    = 'Templater'
  rdoc.options << '--line-numbers' << '--inline-source'
  rdoc.rdoc_files.include('README')
  rdoc.rdoc_files.include('LICENSE')
  rdoc.rdoc_files.include('lib/**/*.rb')
end

#
# ==== RCov
#

desc "Run coverage suite"
task :rcov do
  require 'fileutils'
  FileUtils.rm_rf("coverage") if File.directory?("coverage")
  FileUtils.mkdir("coverage")
  path = File.expand_path(Dir.pwd)
  files = Dir["spec/**/*_spec.rb"]
  files.each do |spec|
    puts "Getting coverage for #{File.expand_path(spec)}"
    command = %{rcov #{File.expand_path(spec)} --aggregate #{path}/coverage/data.data}
    command += " --no-html" unless spec == files.last
    `#{command} 2>&1`
  end
end

file_list = FileList['spec/**/*_spec.rb']

desc "Run all examples"
Spec::Rake::SpecTask.new('spec') do |t|
  t.spec_files = file_list
end

namespace :spec do
  desc "Run all examples with RCov"
  Spec::Rake::SpecTask.new('rcov') do |t|
    t.spec_files = file_list
    t.rcov = true
    t.rcov_dir = "doc/coverage"
    t.rcov_opts = ['--exclude', 'spec']
  end
  
  desc "Generate an html report"
  Spec::Rake::SpecTask.new('report') do |t|
    t.spec_files = file_list
    t.spec_opts = ["--format", "html:doc/reports/specs.html"]
    t.fail_on_error = false
  end
end

desc 'Default: run unit tests.'
task :default => 'spec'
