# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-exceptions}
  s.version = "1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Andy Kent"]
  s.date = %q{2009-02-16}
  s.description = %q{Email and web hook exceptions for Merb.}
  s.email = %q{andy@new-bamboo.co.uk}
  s.extra_rdoc_files = ["README.markdown", "LICENSE"]
  s.files = ["LICENSE", "README.markdown", "Rakefile", "lib/merb-exceptions.rb", "lib/merb-exceptions", "lib/merb-exceptions/exceptions_helper.rb", "lib/merb-exceptions/templates", "lib/merb-exceptions/templates/email.erb", "lib/merb-exceptions/default_exception_extensions.rb", "lib/merb-exceptions/notification.rb", "spec/default_exception_extensions_spec.rb", "spec/exceptions_helper_spec.rb", "spec/notification_spec.rb", "spec/spec_helper.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Email and web hook exceptions for Merb.}

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
