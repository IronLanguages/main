# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-core}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Dan Kubb"]
  s.date = %q{2009-03-29}
  s.description = %q{Faster, Better, Simpler.}
  s.email = ["dan.kubb@gmail.com"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = [".autotest", ".gitignore", "CONTRIBUTING", "FAQ", "History.txt", "MIT-LICENSE", "Manifest.txt", "QUICKLINKS", "README.txt", "Rakefile", "SPECS", "TODO", "dm-core.gemspec", "lib/dm-core.rb", "lib/dm-core/adapters.rb", "lib/dm-core/adapters/abstract_adapter.rb", "lib/dm-core/adapters/data_objects_adapter.rb", "lib/dm-core/adapters/in_memory_adapter.rb", "lib/dm-core/adapters/mysql_adapter.rb", "lib/dm-core/adapters/postgres_adapter.rb", "lib/dm-core/adapters/sqlite3_adapter.rb", "lib/dm-core/associations.rb", "lib/dm-core/associations/many_to_many.rb", "lib/dm-core/associations/many_to_one.rb", "lib/dm-core/associations/one_to_many.rb", "lib/dm-core/associations/one_to_one.rb", "lib/dm-core/associations/relationship.rb", "lib/dm-core/associations/relationship_chain.rb", "lib/dm-core/auto_migrations.rb", "lib/dm-core/collection.rb", "lib/dm-core/dependency_queue.rb", "lib/dm-core/hook.rb", "lib/dm-core/identity_map.rb", "lib/dm-core/is.rb", "lib/dm-core/logger.rb", "lib/dm-core/migrations/destructive_migrations.rb", "lib/dm-core/migrator.rb", "lib/dm-core/model.rb", "lib/dm-core/naming_conventions.rb", "lib/dm-core/property.rb", "lib/dm-core/property_set.rb", "lib/dm-core/query.rb", "lib/dm-core/repository.rb", "lib/dm-core/resource.rb", "lib/dm-core/scope.rb", "lib/dm-core/support.rb", "lib/dm-core/support/array.rb", "lib/dm-core/support/assertions.rb", "lib/dm-core/support/errors.rb", "lib/dm-core/support/kernel.rb", "lib/dm-core/support/symbol.rb", "lib/dm-core/transaction.rb", "lib/dm-core/type.rb", "lib/dm-core/type_map.rb", "lib/dm-core/types.rb", "lib/dm-core/types/boolean.rb", "lib/dm-core/types/discriminator.rb", "lib/dm-core/types/object.rb", "lib/dm-core/types/paranoid_boolean.rb", "lib/dm-core/types/paranoid_datetime.rb", "lib/dm-core/types/serial.rb", "lib/dm-core/types/text.rb", "lib/dm-core/version.rb", "script/all", "script/performance.rb", "script/profile.rb", "spec/integration/association_spec.rb", "spec/integration/association_through_spec.rb", "spec/integration/associations/many_to_many_spec.rb", "spec/integration/associations/many_to_one_spec.rb", "spec/integration/associations/one_to_many_spec.rb", "spec/integration/auto_migrations_spec.rb", "spec/integration/collection_spec.rb", "spec/integration/data_objects_adapter_spec.rb", "spec/integration/dependency_queue_spec.rb", "spec/integration/model_spec.rb", "spec/integration/mysql_adapter_spec.rb", "spec/integration/postgres_adapter_spec.rb", "spec/integration/property_spec.rb", "spec/integration/query_spec.rb", "spec/integration/repository_spec.rb", "spec/integration/resource_spec.rb", "spec/integration/sqlite3_adapter_spec.rb", "spec/integration/sti_spec.rb", "spec/integration/strategic_eager_loading_spec.rb", "spec/integration/transaction_spec.rb", "spec/integration/type_spec.rb", "spec/lib/logging_helper.rb", "spec/lib/mock_adapter.rb", "spec/lib/model_loader.rb", "spec/lib/publicize_methods.rb", "spec/models/content.rb", "spec/models/vehicles.rb", "spec/models/zoo.rb", "spec/spec.opts", "spec/spec_helper.rb", "spec/unit/adapters/abstract_adapter_spec.rb", "spec/unit/adapters/adapter_shared_spec.rb", "spec/unit/adapters/data_objects_adapter_spec.rb", "spec/unit/adapters/in_memory_adapter_spec.rb", "spec/unit/adapters/postgres_adapter_spec.rb", "spec/unit/associations/many_to_many_spec.rb", "spec/unit/associations/many_to_one_spec.rb", "spec/unit/associations/one_to_many_spec.rb", "spec/unit/associations/one_to_one_spec.rb", "spec/unit/associations/relationship_spec.rb", "spec/unit/associations_spec.rb", "spec/unit/auto_migrations_spec.rb", "spec/unit/collection_spec.rb", "spec/unit/data_mapper_spec.rb", "spec/unit/identity_map_spec.rb", "spec/unit/is_spec.rb", "spec/unit/migrator_spec.rb", "spec/unit/model_spec.rb", "spec/unit/naming_conventions_spec.rb", "spec/unit/property_set_spec.rb", "spec/unit/property_spec.rb", "spec/unit/query_spec.rb", "spec/unit/repository_spec.rb", "spec/unit/resource_spec.rb", "spec/unit/scope_spec.rb", "spec/unit/transaction_spec.rb", "spec/unit/type_map_spec.rb", "spec/unit/type_spec.rb", "tasks/ci.rb", "tasks/dm.rb", "tasks/doc.rb", "tasks/gemspec.rb", "tasks/hoe.rb", "tasks/install.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://datamapper.org}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An Object/Relational Mapper for Ruby}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<data_objects>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<extlib>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<addressable>, ["~> 2.0.2"])
    else
      s.add_dependency(%q<data_objects>, ["~> 0.9.11"])
      s.add_dependency(%q<extlib>, ["~> 0.9.11"])
      s.add_dependency(%q<addressable>, ["~> 2.0.2"])
    end
  else
    s.add_dependency(%q<data_objects>, ["~> 0.9.11"])
    s.add_dependency(%q<extlib>, ["~> 0.9.11"])
    s.add_dependency(%q<addressable>, ["~> 2.0.2"])
  end
end
