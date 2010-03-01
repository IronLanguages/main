require 'rubygems'

spec = Gem::Specification.new do |gem|
   gem.name       = 'win32-api'
   gem.version    = '1.4.5'
   gem.authors    = ['Daniel J. Berger', 'Park Heesob']
   gem.license    = 'Artistic 2.0'
   gem.email      = 'djberg96@gmail.com'
   gem.homepage   = 'http://www.rubyforge.org/projects/win32utils'
   gem.platform   = Gem::Platform::RUBY
   gem.summary    = 'A superior replacement for Win32API'
   gem.has_rdoc   = true
   gem.test_files = Dir['test/test*']
   gem.extensions = ['ext/extconf.rb']
   gem.files      = Dir['**/*'].reject{ |f| f.include?('CVS') || f.include?('lib') }
   
   gem.rubyforge_project = 'win32utils'
   gem.required_ruby_version = '>= 1.8.2'
   gem.extra_rdoc_files = ['README', 'CHANGES', 'MANIFEST', 'ext/win32/api.c']
   
   gem.add_development_dependency('test-unit', '>= 2.0.2')
   
   gem.description = <<-EOF
      The Win32::API library is meant as a replacement for the Win32API
      library that ships as part of the standard library. It contains several
      advantages over Win32API, including callback support, raw function
      pointers, an additional string type, and more.
   EOF
end

Gem::Builder.new(spec).build
