require "rubygems"

spec = Gem::Specification.new do |gem|
	gem.name         = "win32-clipboard"
	gem.version      = "0.4.4"
	gem.authors      = ["Daniel J. Berger", "Park Heesob"]
	gem.email        = "djberg96@gmail.com"
	gem.homepage     = "http://www.rubyforge.org/projects/win32utils"
	gem.platform     = Gem::Platform::RUBY
	gem.summary      = "A library for interacting with the Windows clipboard"
	gem.description  = "A library for interacting with the Windows clipboard"
	gem.test_file    = "test/test_clipboard.rb"
	gem.has_rdoc     = true
	gem.extra_rdoc_files  = ['CHANGES', 'README', 'MANIFEST']
	gem.rubyforge_project = "win32utils"
	gem.add_dependency("windows-pr", ">= 0.8.1")
	
	files = Dir["doc/*"] + Dir["examples/*"] + Dir["lib/win32/*"]
	files += Dir["test/*"] + Dir["[A-Z]*"]
	files.delete_if{ |item| item.include?("CVS") }
	gem.files = files
end

if $0 == __FILE__
   Gem.manage_gems
   Gem::Builder.new(spec).build
end
