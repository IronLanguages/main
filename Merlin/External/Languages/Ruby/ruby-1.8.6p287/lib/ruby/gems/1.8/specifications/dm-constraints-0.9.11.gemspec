# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-constraints}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Dirkjan Bussink"]
  s.date = %q{2009-03-10}
  s.description = %q{DataMapper plugin constraining relationships}
  s.email = ["d.bussink [a] gmail [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/dm-constraints.rb", "lib/dm-constraints/data_objects_adapter.rb", "lib/dm-constraints/delete_constraint.rb", "lib/dm-constraints/mysql_adapter.rb", "lib/dm-constraints/postgres_adapter.rb", "lib/dm-constraints/version.rb", "spec/integration/constraints_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-constraints}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{DataMapper plugin constraining relationships}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.11"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
  end
end
