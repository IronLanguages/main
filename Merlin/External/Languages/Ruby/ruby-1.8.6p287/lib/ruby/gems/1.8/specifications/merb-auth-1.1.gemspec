# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-auth}
  s.version = "1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel Neighman"]
  s.date = %q{2009-02-23}
  s.description = %q{merb-auth.  The official authentication plugin for merb.  Setup for the default stack}
  s.email = %q{has.sox@gmail.com}
  s.files = ["LICENSE", "README.textile", "Rakefile", "TODO", "lib/merb-auth.rb"]
  s.homepage = %q{http://www.merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb-auth}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{merb-auth.  The official authentication plugin for merb.  Setup for the default stack}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, ["~> 1.1"])
      s.add_runtime_dependency(%q<merb-auth-core>, ["~> 1.1"])
      s.add_runtime_dependency(%q<merb-auth-more>, ["~> 1.1"])
      s.add_runtime_dependency(%q<merb-auth-slice-password>, ["~> 1.1"])
    else
      s.add_dependency(%q<merb-core>, ["~> 1.1"])
      s.add_dependency(%q<merb-auth-core>, ["~> 1.1"])
      s.add_dependency(%q<merb-auth-more>, ["~> 1.1"])
      s.add_dependency(%q<merb-auth-slice-password>, ["~> 1.1"])
    end
  else
    s.add_dependency(%q<merb-core>, ["~> 1.1"])
    s.add_dependency(%q<merb-auth-core>, ["~> 1.1"])
    s.add_dependency(%q<merb-auth-more>, ["~> 1.1"])
    s.add_dependency(%q<merb-auth-slice-password>, ["~> 1.1"])
  end
end
