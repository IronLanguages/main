# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{rails}
  s.version = "3.0.pre"

  s.required_rubygems_version = Gem::Requirement.new("> 1.3.1") if s.respond_to? :required_rubygems_version=
  s.authors = ["David Heinemeier Hansson"]
  s.date = %q{2009-12-08}
  s.description = %q{    Rails is a framework for building web-application using CGI, FCGI, mod_ruby, or WEBrick
    on top of either MySQL, PostgreSQL, SQLite, DB2, SQL Server, or Oracle with eRuby- or Builder-based templates.
}
  s.email = %q{david@loudthinking.com}
  s.homepage = %q{http://www.rubyonrails.org}
  s.rdoc_options = ["--exclude", "."]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{rails}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Web-application framework with template engine, control-flow layer, and ORM.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<actionpack>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<activerecord>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<activeresource>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<actionmailer>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<railties>, ["= 3.0.pre"])
    else
      s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_dependency(%q<actionpack>, ["= 3.0.pre"])
      s.add_dependency(%q<activerecord>, ["= 3.0.pre"])
      s.add_dependency(%q<activeresource>, ["= 3.0.pre"])
      s.add_dependency(%q<actionmailer>, ["= 3.0.pre"])
      s.add_dependency(%q<railties>, ["= 3.0.pre"])
    end
  else
    s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
    s.add_dependency(%q<actionpack>, ["= 3.0.pre"])
    s.add_dependency(%q<activerecord>, ["= 3.0.pre"])
    s.add_dependency(%q<activeresource>, ["= 3.0.pre"])
    s.add_dependency(%q<actionmailer>, ["= 3.0.pre"])
    s.add_dependency(%q<railties>, ["= 3.0.pre"])
  end
end
