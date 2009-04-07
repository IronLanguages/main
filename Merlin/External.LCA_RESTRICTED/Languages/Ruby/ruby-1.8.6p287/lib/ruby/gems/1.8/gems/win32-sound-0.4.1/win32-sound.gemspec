require "rubygems"

spec = Gem::Specification.new do |gem|
	gem.name         = "win32-sound"
	gem.version      = "0.4.1"
	gem.author       = "Daniel J. Berger"
	gem.email        = "djberg96@gmail.com"
	gem.homepage     = "http://www.rubyforge.org/projects/win32utils"
	gem.platform     = Gem::Platform::RUBY
	gem.summary      = "A library for playing with sound on MS Windows."
	gem.description  = "A library for playing with sound on MS Windows."
	gem.test_file    = "test/tc_sound.rb"
	gem.has_rdoc     = true
	gem.require_path = "lib"
	gem.extra_rdoc_files  = ['CHANGES', 'README', 'MANIFEST']
	gem.rubyforge_project = "win32utils"
	
	files = Dir["doc/*"] + Dir["examples/*"] + Dir["lib/win32/*"]
	files += Dir["test/*"] + Dir["[A-Z]*"]
	files.delete_if{ |item| item.include?("CVS") }
	gem.files = files
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
