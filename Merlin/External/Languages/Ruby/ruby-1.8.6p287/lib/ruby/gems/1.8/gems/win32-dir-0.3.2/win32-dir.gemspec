require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "win32-dir"
   gem.version     = "0.3.2"
   gem.author      = "Daniel J. Berger"
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "Extra constants and methods for the Dir class on Windows."
   gem.description = "Extra constants and methods for the Dir class on Windows."
   gem.test_file   = "test/tc_dir.rb"
   gem.has_rdoc    = true
   gem.files       = Dir["lib/win32/*.rb"] + Dir["test/*"] + Dir["[A-Z]*"]
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ['README', 'CHANGES', 'MANIFEST']
   gem.add_dependency("windows-pr", ">= 0.5.1")
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
