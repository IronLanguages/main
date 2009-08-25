# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-more}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 1.3.0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Engine Yard"]
  s.date = %q{2009-06-30}
  s.description = %q{(merb - merb-core) == merb-more.  The Full Stack. Take what you need; leave what you don't.}
  s.email = %q{merb@engineyard.com}
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "lib/merb-more.rb"]
  s.homepage = %q{http://www.merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb-more}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{(merb - merb-core) == merb-more.  The Full Stack. Take what you need; leave what you don't.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_runtime_dependency(%q<merb-action-args>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-assets>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-slices>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-auth>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-cache>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-exceptions>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-gen>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-haml>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-helpers>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-mailer>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<merb-param-protection>, ["= 1.0.12"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_dependency(%q<merb-action-args>, ["= 1.0.12"])
      s.add_dependency(%q<merb-assets>, ["= 1.0.12"])
      s.add_dependency(%q<merb-slices>, ["= 1.0.12"])
      s.add_dependency(%q<merb-auth>, ["= 1.0.12"])
      s.add_dependency(%q<merb-cache>, ["= 1.0.12"])
      s.add_dependency(%q<merb-exceptions>, ["= 1.0.12"])
      s.add_dependency(%q<merb-gen>, ["= 1.0.12"])
      s.add_dependency(%q<merb-haml>, ["= 1.0.12"])
      s.add_dependency(%q<merb-helpers>, ["= 1.0.12"])
      s.add_dependency(%q<merb-mailer>, ["= 1.0.12"])
      s.add_dependency(%q<merb-param-protection>, ["= 1.0.12"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.0.12"])
    s.add_dependency(%q<merb-action-args>, ["= 1.0.12"])
    s.add_dependency(%q<merb-assets>, ["= 1.0.12"])
    s.add_dependency(%q<merb-slices>, ["= 1.0.12"])
    s.add_dependency(%q<merb-auth>, ["= 1.0.12"])
    s.add_dependency(%q<merb-cache>, ["= 1.0.12"])
    s.add_dependency(%q<merb-exceptions>, ["= 1.0.12"])
    s.add_dependency(%q<merb-gen>, ["= 1.0.12"])
    s.add_dependency(%q<merb-haml>, ["= 1.0.12"])
    s.add_dependency(%q<merb-helpers>, ["= 1.0.12"])
    s.add_dependency(%q<merb-mailer>, ["= 1.0.12"])
    s.add_dependency(%q<merb-param-protection>, ["= 1.0.12"])
  end
end
