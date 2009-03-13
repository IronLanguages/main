# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-more}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Sam Smoot"]
  s.date = %q{2009-03-10}
  s.description = %q{Faster, Better, Simpler.}
  s.email = ["ssmoot [a] gmail [d] com"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = [".gitignore", "History.txt", "MIT-LICENSE", "Manifest.txt", "README.textile", "README.txt", "Rakefile", "TODO", "lib/dm-more.rb", "lib/dm-more/version.rb", "tasks/hoe.rb"]
  s.homepage = %q{http://github.com/sam/dm-more/tree/master}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An Object/Relational Mapper for Ruby}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-adjust>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-serializer>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-validations>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-types>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-couchdb-adapter>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-ferret-adapter>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-rest-adapter>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-aggregates>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-ar-finders>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-cli>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-constraints>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-list>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-nested_set>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-remixable>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-searchable>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-state_machine>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-tree>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-versioned>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-is-viewable>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-migrations>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-observer>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-querizer>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-shorthand>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-sweatshop>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-tags>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<dm-timestamps>, ["~> 0.9.11"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-adjust>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-serializer>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-validations>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-types>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-couchdb-adapter>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-ferret-adapter>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-rest-adapter>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-aggregates>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-ar-finders>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-cli>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-constraints>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-list>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-nested_set>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-remixable>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-searchable>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-state_machine>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-tree>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-versioned>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-is-viewable>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-migrations>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-observer>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-querizer>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-shorthand>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-sweatshop>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-tags>, ["~> 0.9.11"])
      s.add_dependency(%q<dm-timestamps>, ["~> 0.9.11"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-adjust>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-serializer>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-validations>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-types>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-couchdb-adapter>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-ferret-adapter>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-rest-adapter>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-aggregates>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-ar-finders>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-cli>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-constraints>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-list>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-nested_set>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-remixable>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-searchable>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-state_machine>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-tree>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-versioned>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-is-viewable>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-migrations>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-observer>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-querizer>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-shorthand>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-sweatshop>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-tags>, ["~> 0.9.11"])
    s.add_dependency(%q<dm-timestamps>, ["~> 0.9.11"])
  end
end
