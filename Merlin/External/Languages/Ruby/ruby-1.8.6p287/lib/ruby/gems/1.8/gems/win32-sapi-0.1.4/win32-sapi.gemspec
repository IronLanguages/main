require "rubygems"

spec = Gem::Specification.new do |gem|
   gem.name        = "win32-sapi"
   gem.version     = "0.1.4"
   gem.author      = "Daniel J. Berger"
   gem.email       = "djberg96@gmail.com"
   gem.homepage    = "http://www.rubyforge.org/projects/win32utils"
   gem.platform    = Gem::Platform::RUBY
   gem.summary     = "An interface to the MS SAPI (Sound API) library."
   gem.description = "An interface to the MS SAPI (Sound API) library."
   gem.test_file   = "test/tc_sapi5.rb"
   gem.has_rdoc    = true
   gem.files = Dir['lib/win32/*.rb'] + Dir['[A-Z]*']  + Dir['test/*']
   gem.files.reject! { |fn| fn.include? "CVS" }
   gem.require_path = "lib"
   gem.extra_rdoc_files = ['README', 'CHANGES', 'MANIFEST']
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end