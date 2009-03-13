require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "windows-pr"
   gem.version     = "0.9.3"
   gem.authors     = ["Daniel J. Berger", "Park Heesob"]
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.rubyforge_project = "win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "Windows functions and constants bundled via Win32::API"
   gem.description = "Windows functions and constants bundled via Win32::API"
   gem.test_files  = Dir["test/tc*"]
   gem.has_rdoc    = true
   gem.files       = Dir["doc/*.txt"] + Dir["lib/windows/**/*.rb"]
   gem.files += Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ["MANIFEST", "README", "CHANGES"]
   gem.add_dependency("windows-api", ">= 0.2.4")
   gem.add_dependency("win32-api", ">= 1.2.0")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
