require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "windows-api"
   gem.version     = "0.2.4"
   gem.author      = "Daniel J. Berger"
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.rubyforge_project = "win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "An easier way to create methods using Win32API"
   gem.description = "An easier way to create methods using Win32API"
   gem.test_file   = "test/test_windows_api.rb"
   gem.has_rdoc    = true
   gem.files       = Dir["lib/windows/*.rb"] + Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
   gem.add_dependency("win32-api", ">= 1.0.5")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
