require 'rake'
require 'rake/testtask'
require 'rbconfig'
include Config

desc 'Install the facade library (non-gem)'
task :install do
   sitelibdir = CONFIG["sitelibdir"]
   file = "lib/facade.rb"
   FileUtils.cp(file, sitelibdir, :verbose => true)
end

desc 'Install the facade library as a gem'
task :install_gem do
   ruby 'facade.gemspec'
   file = Dir["*.gem"].first
   sh "gem install #{file}"
end

Rake::TestTask.new do |t|
   t.libs << 'test'
   t.verbose = true
   t.warning = true
end
