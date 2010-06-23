Gem::Specification.new do |s|
  s.name              = 'ironruby-rack'
  s.version           = '0.0.9'
  s.platform          = 'universal-dotnet'
  s.summary           = 'ASP.NET-enabled webserver interface for IronRuby'

  s.description       = <<-EOF
IronRuby-Rack provides a Rack hander for ASP.NET-enabled webservers,
including Information Internet Services (IIS), ASP.NET web development
server (WebDev.WebServer.exe), Cassini, and any .NET application
embedding ASP.NET using System.Web.Hosting.
EOF

  s.files             = Dir['{bin/*,lib/**/*}'] +
                        %w(LICENSE.html ironruby-rack.gemspec README.markdown)
  s.bindir            = 'bin'
  s.executables       << 'rack2aspnet' << 'deploy2iis' 
  s.require_path      = 'lib'
  s.has_rdoc          = true
  s.extra_rdoc_files  = %w(README.markdown LICENSE.html)
  s.test_files        = Dir['test/{test,spec}_*.rb']

  s.author            = 'Jimmy Schementi'
  s.email             = 'jimmy@schementi.com'
  s.homepage          = 'http://ironruby.net'
  s.rubyforge_project = 'ironruby'

  s.add_dependency('rack', '>= 1.0.0')

  s.add_development_dependency 'test-spec'
end