# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{mailfactory}
  s.version = "1.4.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["David Powers"]
  s.date = %q{2008-08-06}
  s.description = %q{MailFactory is s simple module for producing RFC compliant mail that can include multiple attachments, multiple body parts, and arbitrary headers}
  s.email = %q{david@grayskies.net}
  s.files = ["./lib/mailfactory.rb", "./tests/test_mailfactory.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://mailfactory.rubyforge.org}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{mailfactory}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{MailFactory is a pure-ruby MIME mail generator}
  s.test_files = ["./tests/test_mailfactory.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<mime-types>, [">= 1.13.1"])
    else
      s.add_dependency(%q<mime-types>, [">= 1.13.1"])
    end
  else
    s.add_dependency(%q<mime-types>, [">= 1.13.1"])
  end
end
