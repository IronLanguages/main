require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "win32-file"
   gem.version     = "0.5.5"
   gem.author      = "Daniel J. Berger"
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "Extra or redefined methods for the File class on Windows."
   gem.description = "Extra or redefined methods for the File class on Windows."
   gem.test_files  = Dir["test/tc*"]
   gem.has_rdoc    = true
   gem.files       = Dir["lib/win32/*.rb"] + Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ["README", "CHANGES"]
   gem.add_dependency("win32-file-stat", ">= 1.2.0")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
