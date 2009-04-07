# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{mongrel}
  s.version = "1.1.5"
  s.platform = %q{x86-mswin32-60}

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Zed A. Shaw"]
  s.date = %q{2008-05-21}
  s.default_executable = %q{mongrel_rails}
  s.description = %q{A small fast HTTP library and server that runs Rails, Camping, Nitro and Iowa apps.}
  s.email = %q{}
  s.executables = ["mongrel_rails"]
  s.files = ["bin/mongrel_rails", "CHANGELOG", "COPYING", "examples/builder.rb", "examples/camping/blog.rb", "examples/camping/README", "examples/camping/tepee.rb", "examples/httpd.conf", "examples/mime.yaml", "examples/mongrel.conf", "examples/mongrel_simple_ctrl.rb", "examples/mongrel_simple_service.rb", "examples/monitrc", "examples/random_thrash.rb", "examples/simpletest.rb", "examples/webrick_compare.rb", "ext/http11/ext_help.h", "ext/http11/extconf.rb", "ext/http11/http11.c", "ext/http11/http11_parser.c", "ext/http11/http11_parser.h", "ext/http11/http11_parser.java.rl", "ext/http11/http11_parser.rl", "ext/http11/http11_parser_common.rl", "ext/http11_java/Http11Service.java", "ext/http11_java/org/jruby/mongrel/Http11.java", "ext/http11_java/org/jruby/mongrel/Http11Parser.java", "lib/mongrel/camping.rb", "lib/mongrel/cgi.rb", "lib/mongrel/command.rb", "lib/mongrel/configurator.rb", "lib/mongrel/const.rb", "lib/mongrel/debug.rb", "lib/mongrel/gems.rb", "lib/mongrel/handlers.rb", "lib/mongrel/header_out.rb", "lib/mongrel/http_request.rb", "lib/mongrel/http_response.rb", "lib/mongrel/init.rb", "lib/mongrel/mime_types.yml", "lib/mongrel/rails.rb", "lib/mongrel/stats.rb", "lib/mongrel/tcphack.rb", "lib/mongrel/uri_classifier.rb", "lib/mongrel.rb", "LICENSE", "Manifest", "mongrel-public_cert.pem", "mongrel.gemspec", "README", "setup.rb", "test/mime.yaml", "test/mongrel.conf", "test/test_cgi_wrapper.rb", "test/test_command.rb", "test/test_conditional.rb", "test/test_configurator.rb", "test/test_debug.rb", "test/test_handlers.rb", "test/test_http11.rb", "test/test_redirect_handler.rb", "test/test_request_progress.rb", "test/test_response.rb", "test/test_stats.rb", "test/test_uriclassifier.rb", "test/test_ws.rb", "test/testhelp.rb", "TODO", "tools/trickletest.rb", "lib/http11.so"]
  s.has_rdoc = true
  s.homepage = %q{http://mongrel.rubyforge.org}
  s.require_paths = ["lib", "ext"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.4")
  s.rubyforge_project = %q{mongrel}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{A small fast HTTP library and server that runs Rails, Camping, Nitro and Iowa apps.}
  s.test_files = ["test/test_cgi_wrapper.rb", "test/test_command.rb", "test/test_conditional.rb", "test/test_configurator.rb", "test/test_debug.rb", "test/test_handlers.rb", "test/test_http11.rb", "test/test_redirect_handler.rb", "test/test_request_progress.rb", "test/test_response.rb", "test/test_stats.rb", "test/test_uriclassifier.rb", "test/test_ws.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<gem_plugin>, [">= 0.2.3"])
      s.add_runtime_dependency(%q<cgi_multipart_eof_fix>, [">= 2.4"])
    else
      s.add_dependency(%q<gem_plugin>, [">= 0.2.3"])
      s.add_dependency(%q<cgi_multipart_eof_fix>, [">= 2.4"])
    end
  else
    s.add_dependency(%q<gem_plugin>, [">= 0.2.3"])
    s.add_dependency(%q<cgi_multipart_eof_fix>, [">= 2.4"])
  end
end
