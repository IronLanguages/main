# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-cache}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ben Burkert"]
  s.date = %q{2009-06-30}
  s.description = %q{Merb plugin that provides caching (page, action, fragment, object)}
  s.email = %q{ben@benburkert.com}
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "lib/merb-cache/cache.rb", "lib/merb-cache/cache_request.rb", "lib/merb-cache/core_ext/enumerable.rb", "lib/merb-cache/core_ext/hash.rb", "lib/merb-cache/merb_ext/controller.rb", "lib/merb-cache/stores/fundamental/abstract_store.rb", "lib/merb-cache/stores/fundamental/file_store.rb", "lib/merb-cache/stores/fundamental/memcached_store.rb", "lib/merb-cache/stores/strategy/abstract_strategy_store.rb", "lib/merb-cache/stores/strategy/action_store.rb", "lib/merb-cache/stores/strategy/adhoc_store.rb", "lib/merb-cache/stores/strategy/gzip_store.rb", "lib/merb-cache/stores/strategy/page_store.rb", "lib/merb-cache/stores/strategy/sha1_store.rb", "lib/merb-cache.rb", "spec/merb-cache/cache_request_spec.rb", "spec/merb-cache/cache_spec.rb", "spec/merb-cache/core_ext/enumerable_spec.rb", "spec/merb-cache/core_ext/hash_spec.rb", "spec/merb-cache/merb_ext/controller_spec.rb", "spec/merb-cache/stores/fundamental/abstract_store_spec.rb", "spec/merb-cache/stores/fundamental/file_store_spec.rb", "spec/merb-cache/stores/fundamental/memcached_store_spec.rb", "spec/merb-cache/stores/strategy/abstract_strategy_store_spec.rb", "spec/merb-cache/stores/strategy/action_store_spec.rb", "spec/merb-cache/stores/strategy/adhoc_store_spec.rb", "spec/merb-cache/stores/strategy/gzip_store_spec.rb", "spec/merb-cache/stores/strategy/page_store_spec.rb", "spec/merb-cache/stores/strategy/sha1_store_spec.rb", "spec/spec_helper.rb"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb plugin that provides caching (page, action, fragment, object)}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.0.12"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.0.12"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.0.12"])
  end
end
