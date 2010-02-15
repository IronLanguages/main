# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ironruby-sqlserver}
  s.version = "0.1.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Jimmy Schementi"]
  s.date = %q{2010-01-14}
  s.description = %q{Wires up ActiveRecord to use SQLServer through IronRuby.}
  s.email = ["jschementi@gmail.com"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = ["History.txt", "Manifest.txt", "README.txt", "Rakefile", "lib/activerecord-sqlserver-adapter/adonet_patch.rb", "lib/ironruby_sqlserver.rb"]
  s.homepage = %q{http://github.com/jschementi/ironruby-sqlserver}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{ironruby-sqlserver}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Wires up ActiveRecord to use SQLServer through IronRuby.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<ironruby-dbi>, [">= 0"])
      s.add_runtime_dependency(%q<activerecord>, [">= 0"])
      s.add_runtime_dependency(%q<activerecord-sqlserver-adapter>, [">= 0"])
      s.add_development_dependency(%q<rubyforge>, [">= 2.0.3"])
      s.add_development_dependency(%q<gemcutter>, [">= 0.3.0"])
      s.add_development_dependency(%q<hoe>, [">= 2.5.0"])
    else
      s.add_dependency(%q<ironruby-dbi>, [">= 0"])
      s.add_dependency(%q<activerecord>, [">= 0"])
      s.add_dependency(%q<activerecord-sqlserver-adapter>, [">= 0"])
      s.add_dependency(%q<rubyforge>, [">= 2.0.3"])
      s.add_dependency(%q<gemcutter>, [">= 0.3.0"])
      s.add_dependency(%q<hoe>, [">= 2.5.0"])
    end
  else
    s.add_dependency(%q<ironruby-dbi>, [">= 0"])
    s.add_dependency(%q<activerecord>, [">= 0"])
    s.add_dependency(%q<activerecord-sqlserver-adapter>, [">= 0"])
    s.add_dependency(%q<rubyforge>, [">= 2.0.3"])
    s.add_dependency(%q<gemcutter>, [">= 0.3.0"])
    s.add_dependency(%q<hoe>, [">= 2.5.0"])
  end
end
