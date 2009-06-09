# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-assets}
  s.version = "1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ezra Zygmuntowicz"]
  s.date = %q{2009-02-16}
  s.description = %q{Merb plugin that provides the helpers for assets and asset bundling}
  s.email = %q{ez@engineyard.com}
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "lib/merb-assets.rb", "lib/merb-assets", "lib/merb-assets/assets_mixin.rb", "lib/merb-assets/assets.rb", "spec/merb-assets_spec.rb", "spec/spec_helper.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Merb plugin that provides the helpers for assets and asset bundling}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.1"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.1"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.1"])
  end
end
