require 'rubygems'

spec = Gem::Specification.new do |gem|
   gem.name      = 'facade'
   gem.version   = '1.0.4'
   gem.author    = 'Daniel J. Berger'
   gem.license   = 'Artistic 2.0'
   gem.email     = 'djberg96@gmail.com'
   gem.homepage  = 'http://www.rubyforge.org/projects/shards'
   gem.platform  = Gem::Platform::RUBY
   gem.summary   = 'An easy way to implement the facade pattern in your class'
   gem.test_file = 'test/test_facade.rb'
   gem.has_rdoc  = true
   gem.files     = Dir['**/*'].reject{ |f| f.include?('CVS') }

   gem.rubyforge_project = 'shards'
   gem.extra_rdoc_files  = ['README', 'CHANGES', 'MANIFEST']

   gem.description = <<-EOF
      The facade library allows you to mixin singleton methods from classes
      or modules as instance methods of the extending class.
   EOF
end

Gem::Builder.new(spec).build
