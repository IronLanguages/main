Gem::Specification.new do |s|
  s.platform    = "universal-dotnet"
  s.name        = '$safeprojectname$'
  s.version     = '0.1'
  s.summary     = "Your Gem's summary"
  s.description = "Your Gem's description"
  s.required_ruby_version = '>= 1.9.1'

  s.author            = 'Your Name'
  s.email             = 'your@email'
  s.homepage          = 'http://yourhomepage'
  
  s.rubyforge_project = '$safeprojectname$'

  s.files        = ['CHANGELOG', 'README', 'LICENSE'] + Dir['lib/**/*']
  s.require_path = 'lib'
  s.requirements << 'none'

  s.has_rdoc = true

  # Add other gem dependencies
  # s.add_dependency('name', '= 1.0.0')
end

