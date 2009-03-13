require "rubygems"

spec = Gem::Specification.new do |gem|
   desc = "Adds create, fork, wait, wait2, waitpid, and a special kill method"
   gem.name        = "win32-process"
   gem.version     = "0.5.9"
   gem.authors     = ['Daniel Berger', 'Park Heesob']
   gem.email       = "djberg96@gmail.com"
   gem.rubyforge_project = 'win32utils'
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = desc
   gem.description = desc
   gem.test_file   = "test/tc_process.rb"
   gem.has_rdoc    = true
   gem.files       = Dir["lib/win32/*.rb"] + Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
   gem.add_dependency("windows-pr", ">= 0.8.6")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
