# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{actionpack}
  s.version = "3.0.pre"

  s.required_rubygems_version = Gem::Requirement.new("> 1.3.1") if s.respond_to? :required_rubygems_version=
  s.authors = ["David Heinemeier Hansson"]
  s.autorequire = %q{action_controller}
  s.date = %q{2009-12-08}
  s.description = %q{Eases web-request routing, handling, and response as a half-way front, half-way page controller. Implemented with specific emphasis on enabling easy unit/integration testing that doesn't require a browser.}
  s.email = %q{david@loudthinking.com}
  s.files = ["CHANGELOG", "README", "MIT-LICENSE", "lib/abstract_controller/base.rb", "lib/abstract_controller/callbacks.rb", "lib/abstract_controller/exceptions.rb", "lib/abstract_controller/helpers.rb", "lib/abstract_controller/layouts.rb", "lib/abstract_controller/localized_cache.rb", "lib/abstract_controller/logger.rb", "lib/abstract_controller/rendering_controller.rb", "lib/abstract_controller.rb", "lib/action_controller/base.rb", "lib/action_controller/caching/actions.rb", "lib/action_controller/caching/fragments.rb", "lib/action_controller/caching/pages.rb", "lib/action_controller/caching/sweeping.rb", "lib/action_controller/caching.rb", "lib/action_controller/deprecated/integration_test.rb", "lib/action_controller/deprecated/performance_test.rb", "lib/action_controller/deprecated.rb", "lib/action_controller/dispatch/dispatcher.rb", "lib/action_controller/metal/benchmarking.rb", "lib/action_controller/metal/compatibility.rb", "lib/action_controller/metal/conditional_get.rb", "lib/action_controller/metal/configuration.rb", "lib/action_controller/metal/cookies.rb", "lib/action_controller/metal/exceptions.rb", "lib/action_controller/metal/filter_parameter_logging.rb", "lib/action_controller/metal/flash.rb", "lib/action_controller/metal/head.rb", "lib/action_controller/metal/helpers.rb", "lib/action_controller/metal/hide_actions.rb", "lib/action_controller/metal/http_authentication.rb", "lib/action_controller/metal/layouts.rb", "lib/action_controller/metal/mime_responds.rb", "lib/action_controller/metal/rack_convenience.rb", "lib/action_controller/metal/redirector.rb", "lib/action_controller/metal/rendering_controller.rb", "lib/action_controller/metal/render_options.rb", "lib/action_controller/metal/request_forgery_protection.rb", "lib/action_controller/metal/rescue.rb", "lib/action_controller/metal/responder.rb", "lib/action_controller/metal/session.rb", "lib/action_controller/metal/session_management.rb", "lib/action_controller/metal/streaming.rb", "lib/action_controller/metal/testing.rb", "lib/action_controller/metal/url_for.rb", "lib/action_controller/metal/verification.rb", "lib/action_controller/metal.rb", "lib/action_controller/middleware.rb", "lib/action_controller/notifications.rb", "lib/action_controller/polymorphic_routes.rb", "lib/action_controller/record_identifier.rb", "lib/action_controller/testing/process.rb", "lib/action_controller/test_case.rb", "lib/action_controller/translation.rb", "lib/action_controller/url_rewriter.rb", "lib/action_controller/vendor/html-scanner/html/document.rb", "lib/action_controller/vendor/html-scanner/html/node.rb", "lib/action_controller/vendor/html-scanner/html/sanitizer.rb", "lib/action_controller/vendor/html-scanner/html/selector.rb", "lib/action_controller/vendor/html-scanner/html/tokenizer.rb", "lib/action_controller/vendor/html-scanner/html/version.rb", "lib/action_controller/vendor/html-scanner.rb", "lib/action_controller.rb", "lib/action_dispatch/http/headers.rb", "lib/action_dispatch/http/mime_type.rb", "lib/action_dispatch/http/mime_types.rb", "lib/action_dispatch/http/request.rb", "lib/action_dispatch/http/response.rb", "lib/action_dispatch/http/status_codes.rb", "lib/action_dispatch/http/utils.rb", "lib/action_dispatch/middleware/callbacks.rb", "lib/action_dispatch/middleware/params_parser.rb", "lib/action_dispatch/middleware/rescue.rb", "lib/action_dispatch/middleware/session/abstract_store.rb", "lib/action_dispatch/middleware/session/cookie_store.rb", "lib/action_dispatch/middleware/session/mem_cache_store.rb", "lib/action_dispatch/middleware/show_exceptions.rb", "lib/action_dispatch/middleware/stack.rb", "lib/action_dispatch/middleware/static.rb", "lib/action_dispatch/middleware/string_coercion.rb", "lib/action_dispatch/middleware/templates/rescues/diagnostics.erb", "lib/action_dispatch/middleware/templates/rescues/layout.erb", "lib/action_dispatch/middleware/templates/rescues/missing_template.erb", "lib/action_dispatch/middleware/templates/rescues/routing_error.erb", "lib/action_dispatch/middleware/templates/rescues/template_error.erb", "lib/action_dispatch/middleware/templates/rescues/unknown_action.erb", "lib/action_dispatch/middleware/templates/rescues/_request_and_response.erb", "lib/action_dispatch/middleware/templates/rescues/_trace.erb", "lib/action_dispatch/routing/deprecated_mapper.rb", "lib/action_dispatch/routing/mapper.rb", "lib/action_dispatch/routing/route.rb", "lib/action_dispatch/routing/route_set.rb", "lib/action_dispatch/routing.rb", "lib/action_dispatch/testing/assertions/dom.rb", "lib/action_dispatch/testing/assertions/model.rb", "lib/action_dispatch/testing/assertions/response.rb", "lib/action_dispatch/testing/assertions/routing.rb", "lib/action_dispatch/testing/assertions/selector.rb", "lib/action_dispatch/testing/assertions/tag.rb", "lib/action_dispatch/testing/assertions.rb", "lib/action_dispatch/testing/integration.rb", "lib/action_dispatch/testing/performance_test.rb", "lib/action_dispatch/testing/test_request.rb", "lib/action_dispatch/testing/test_response.rb", "lib/action_dispatch/test_case.rb", "lib/action_dispatch.rb", "lib/action_pack/version.rb", "lib/action_pack.rb", "lib/action_view/base.rb", "lib/action_view/context.rb", "lib/action_view/erb/util.rb", "lib/action_view/helpers/active_model_helper.rb", "lib/action_view/helpers/ajax_helper.rb", "lib/action_view/helpers/asset_tag_helper.rb", "lib/action_view/helpers/atom_feed_helper.rb", "lib/action_view/helpers/cache_helper.rb", "lib/action_view/helpers/capture_helper.rb", "lib/action_view/helpers/date_helper.rb", "lib/action_view/helpers/debug_helper.rb", "lib/action_view/helpers/form_helper.rb", "lib/action_view/helpers/form_options_helper.rb", "lib/action_view/helpers/form_tag_helper.rb", "lib/action_view/helpers/javascript_helper.rb", "lib/action_view/helpers/number_helper.rb", "lib/action_view/helpers/prototype_helper.rb", "lib/action_view/helpers/raw_output_helper.rb", "lib/action_view/helpers/record_identification_helper.rb", "lib/action_view/helpers/record_tag_helper.rb", "lib/action_view/helpers/sanitize_helper.rb", "lib/action_view/helpers/scriptaculous_helper.rb", "lib/action_view/helpers/tag_helper.rb", "lib/action_view/helpers/text_helper.rb", "lib/action_view/helpers/translation_helper.rb", "lib/action_view/helpers/url_helper.rb", "lib/action_view/helpers.rb", "lib/action_view/locale/en.yml", "lib/action_view/paths.rb", "lib/action_view/render/partials.rb", "lib/action_view/render/rendering.rb", "lib/action_view/safe_buffer.rb", "lib/action_view/template/error.rb", "lib/action_view/template/handler.rb", "lib/action_view/template/handlers/builder.rb", "lib/action_view/template/handlers/erb.rb", "lib/action_view/template/handlers/rjs.rb", "lib/action_view/template/handlers.rb", "lib/action_view/template/resolver.rb", "lib/action_view/template/text.rb", "lib/action_view/template.rb", "lib/action_view/test_case.rb", "lib/action_view.rb"]
  s.homepage = %q{http://www.rubyonrails.org}
  s.require_paths = ["lib"]
  s.requirements = ["none"]
  s.rubyforge_project = %q{actionpack}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Web-flow and rendering framework putting the VC in MVC.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<activemodel>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<rack>, ["~> 1.0.1"])
      s.add_runtime_dependency(%q<rack-test>, ["~> 0.5.0"])
      s.add_runtime_dependency(%q<rack-mount>, ["~> 0.3.2"])
      s.add_runtime_dependency(%q<erubis>, ["~> 2.6.5"])
    else
      s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_dependency(%q<activemodel>, ["= 3.0.pre"])
      s.add_dependency(%q<rack>, ["~> 1.0.1"])
      s.add_dependency(%q<rack-test>, ["~> 0.5.0"])
      s.add_dependency(%q<rack-mount>, ["~> 0.3.2"])
      s.add_dependency(%q<erubis>, ["~> 2.6.5"])
    end
  else
    s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
    s.add_dependency(%q<activemodel>, ["= 3.0.pre"])
    s.add_dependency(%q<rack>, ["~> 1.0.1"])
    s.add_dependency(%q<rack-test>, ["~> 0.5.0"])
    s.add_dependency(%q<rack-mount>, ["~> 0.3.2"])
    s.add_dependency(%q<erubis>, ["~> 2.6.5"])
  end
end
