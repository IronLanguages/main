require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "win32-eventlog"
   gem.version     = "0.5.0"
   gem.authors     = ["Daniel J. Berger", "Park Heesob"]
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "Interface for the MS Windows Event Log."
   gem.description = "Interface for the MS Windows Event Log."
   gem.test_files  = Dir["test/*.rb"]
   gem.has_rdoc    = true
   gem.rubyforge_project = 'win32utils'
   gem.files       = Dir["lib/win32/*.rb"] + Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ["README", "CHANGES", "MANIFEST", "doc/tutorial.txt"]
   gem.add_dependency("windows-pr", ">= 0.9.3")
   gem.add_dependency("ptools", ">= 1.1.6")
   gem.add_dependency("test-unit", ">= 2.0.0")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
