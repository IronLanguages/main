# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-core}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 1.3.0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ezra Zygmuntowicz"]
  s.date = %q{2009-06-30}
  s.default_executable = %q{merb}
  s.description = %q{Merb. Pocket rocket web framework.}
  s.email = %q{ez@engineyard.com}
  s.executables = ["merb"]
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "CHANGELOG", "PUBLIC_CHANGELOG", "CONTRIBUTORS", "bin/merb", "bin/merb-specs", "lib/merb-core/autoload.rb", "lib/merb-core/bootloader.rb", "lib/merb-core/config.rb", "lib/merb-core/constants.rb", "lib/merb-core/controller/abstract_controller.rb", "lib/merb-core/controller/exceptions.rb", "lib/merb-core/controller/merb_controller.rb", "lib/merb-core/controller/mime.rb", "lib/merb-core/controller/mixins/authentication.rb", "lib/merb-core/controller/mixins/conditional_get.rb", "lib/merb-core/controller/mixins/controller.rb", "lib/merb-core/controller/mixins/render.rb", "lib/merb-core/controller/mixins/responder.rb", "lib/merb-core/controller/template.rb", "lib/merb-core/core_ext/class.rb", "lib/merb-core/core_ext/hash.rb", "lib/merb-core/core_ext/json_pure_fix.rb", "lib/merb-core/core_ext/kernel.rb", "lib/merb-core/core_ext.rb", "lib/merb-core/dispatch/cookies.rb", "lib/merb-core/dispatch/default_exception/default_exception.rb", "lib/merb-core/dispatch/default_exception/views/_css.html.erb", "lib/merb-core/dispatch/default_exception/views/_javascript.html.erb", "lib/merb-core/dispatch/default_exception/views/index.html.erb", "lib/merb-core/dispatch/dispatcher.rb", "lib/merb-core/dispatch/request.rb", "lib/merb-core/dispatch/request_parsers.rb", "lib/merb-core/dispatch/router/behavior.rb", "lib/merb-core/dispatch/router/cached_proc.rb", "lib/merb-core/dispatch/router/resources.rb", "lib/merb-core/dispatch/router/route.rb", "lib/merb-core/dispatch/router.rb", "lib/merb-core/dispatch/session/container.rb", "lib/merb-core/dispatch/session/cookie.rb", "lib/merb-core/dispatch/session/memcached.rb", "lib/merb-core/dispatch/session/memory.rb", "lib/merb-core/dispatch/session/store_container.rb", "lib/merb-core/dispatch/session.rb", "lib/merb-core/dispatch/worker.rb", "lib/merb-core/gem_ext/erubis.rb", "lib/merb-core/logger.rb", "lib/merb-core/plugins.rb", "lib/merb-core/rack/adapter/abstract.rb", "lib/merb-core/rack/adapter/ebb.rb", "lib/merb-core/rack/adapter/evented_mongrel.rb", "lib/merb-core/rack/adapter/fcgi.rb", "lib/merb-core/rack/adapter/irb.rb", "lib/merb-core/rack/adapter/mongrel.rb", "lib/merb-core/rack/adapter/runner.rb", "lib/merb-core/rack/adapter/swiftiplied_mongrel.rb", "lib/merb-core/rack/adapter/thin.rb", "lib/merb-core/rack/adapter/thin_turbo.rb", "lib/merb-core/rack/adapter/webrick.rb", "lib/merb-core/rack/adapter.rb", "lib/merb-core/rack/application.rb", "lib/merb-core/rack/handler/mongrel.rb", "lib/merb-core/rack/helpers.rb", "lib/merb-core/rack/middleware/conditional_get.rb", "lib/merb-core/rack/middleware/content_length.rb", "lib/merb-core/rack/middleware/path_prefix.rb", "lib/merb-core/rack/middleware/profiler.rb", "lib/merb-core/rack/middleware/static.rb", "lib/merb-core/rack/middleware/tracer.rb", "lib/merb-core/rack/middleware.rb", "lib/merb-core/rack/stream_wrapper.rb", "lib/merb-core/rack.rb", "lib/merb-core/server.rb", "lib/merb-core/tasks/audit.rake", "lib/merb-core/tasks/gem_management.rb", "lib/merb-core/tasks/merb.rb", "lib/merb-core/tasks/merb_rake_helper.rb", "lib/merb-core/tasks/stats.rake", "lib/merb-core/test/helpers/controller_helper.rb", "lib/merb-core/test/helpers/cookie_jar.rb", "lib/merb-core/test/helpers/mock_request_helper.rb", "lib/merb-core/test/helpers/multipart_request_helper.rb", "lib/merb-core/test/helpers/request_helper.rb", "lib/merb-core/test/helpers/route_helper.rb", "lib/merb-core/test/helpers.rb", "lib/merb-core/test/matchers/controller_matchers.rb", "lib/merb-core/test/matchers/request_matchers.rb", "lib/merb-core/test/matchers/route_matchers.rb", "lib/merb-core/test/matchers.rb", "lib/merb-core/test/run_spec.rb", "lib/merb-core/test/run_specs.rb", "lib/merb-core/test/tasks/spectasks.rb", "lib/merb-core/test/test_ext/hpricot.rb", "lib/merb-core/test/test_ext/object.rb", "lib/merb-core/test/test_ext/rspec.rb", "lib/merb-core/test/test_ext/string.rb", "lib/merb-core/test.rb", "lib/merb-core/two-oh.rb", "lib/merb-core/version.rb", "lib/merb-core.rb"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.6")
  s.requirements = ["install the json gem to get faster json parsing"]
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb. Pocket rocket web framework.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<extlib>, [">= 0.9.8"])
      s.add_runtime_dependency(%q<erubis>, [">= 2.6.2"])
      s.add_runtime_dependency(%q<rake>, [">= 0"])
      s.add_runtime_dependency(%q<json_pure>, [">= 0"])
      s.add_runtime_dependency(%q<rspec>, [">= 0"])
      s.add_runtime_dependency(%q<rack>, [">= 0"])
      s.add_runtime_dependency(%q<mime-types>, [">= 0"])
      s.add_runtime_dependency(%q<thor>, [">= 0.9.9"])
      s.add_development_dependency(%q<webrat>, [">= 0.3.1"])
    else
      s.add_dependency(%q<extlib>, [">= 0.9.8"])
      s.add_dependency(%q<erubis>, [">= 2.6.2"])
      s.add_dependency(%q<rake>, [">= 0"])
      s.add_dependency(%q<json_pure>, [">= 0"])
      s.add_dependency(%q<rspec>, [">= 0"])
      s.add_dependency(%q<rack>, [">= 0"])
      s.add_dependency(%q<mime-types>, [">= 0"])
      s.add_dependency(%q<thor>, [">= 0.9.9"])
      s.add_dependency(%q<webrat>, [">= 0.3.1"])
    end
  else
    s.add_dependency(%q<extlib>, [">= 0.9.8"])
    s.add_dependency(%q<erubis>, [">= 2.6.2"])
    s.add_dependency(%q<rake>, [">= 0"])
    s.add_dependency(%q<json_pure>, [">= 0"])
    s.add_dependency(%q<rspec>, [">= 0"])
    s.add_dependency(%q<rack>, [">= 0"])
    s.add_dependency(%q<mime-types>, [">= 0"])
    s.add_dependency(%q<thor>, [">= 0.9.9"])
    s.add_dependency(%q<webrat>, [">= 0.3.1"])
  end
end
