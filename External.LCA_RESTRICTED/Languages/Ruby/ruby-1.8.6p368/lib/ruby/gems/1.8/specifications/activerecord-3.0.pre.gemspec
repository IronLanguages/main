# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{activerecord}
  s.version = "3.0.pre"

  s.required_rubygems_version = Gem::Requirement.new("> 1.3.1") if s.respond_to? :required_rubygems_version=
  s.authors = ["David Heinemeier Hansson"]
  s.autorequire = %q{active_record}
  s.date = %q{2009-12-08}
  s.description = %q{Implements the ActiveRecord pattern (Fowler, PoEAA) for ORM. It ties database tables and classes together for business objects, like Customer or Subscription, that can find, save, and destroy themselves without resorting to manual SQL.}
  s.email = %q{david@loudthinking.com}
  s.extra_rdoc_files = ["README"]
  s.files = ["CHANGELOG", "README", "examples/associations.png", "examples/performance.rb", "examples/simple.rb", "lib/active_record/aggregations.rb", "lib/active_record/associations/association_collection.rb", "lib/active_record/associations/association_proxy.rb", "lib/active_record/associations/belongs_to_association.rb", "lib/active_record/associations/belongs_to_polymorphic_association.rb", "lib/active_record/associations/has_and_belongs_to_many_association.rb", "lib/active_record/associations/has_many_association.rb", "lib/active_record/associations/has_many_through_association.rb", "lib/active_record/associations/has_one_association.rb", "lib/active_record/associations/has_one_through_association.rb", "lib/active_record/associations/through_association_scope.rb", "lib/active_record/associations.rb", "lib/active_record/association_preload.rb", "lib/active_record/attributes/aliasing.rb", "lib/active_record/attributes/store.rb", "lib/active_record/attributes/typecasting.rb", "lib/active_record/attributes.rb", "lib/active_record/attribute_methods/before_type_cast.rb", "lib/active_record/attribute_methods/dirty.rb", "lib/active_record/attribute_methods/primary_key.rb", "lib/active_record/attribute_methods/query.rb", "lib/active_record/attribute_methods/read.rb", "lib/active_record/attribute_methods/time_zone_conversion.rb", "lib/active_record/attribute_methods/write.rb", "lib/active_record/attribute_methods.rb", "lib/active_record/autosave_association.rb", "lib/active_record/base.rb", "lib/active_record/batches.rb", "lib/active_record/calculations.rb", "lib/active_record/callbacks.rb", "lib/active_record/connection_adapters/abstract/connection_pool.rb", "lib/active_record/connection_adapters/abstract/connection_specification.rb", "lib/active_record/connection_adapters/abstract/database_statements.rb", "lib/active_record/connection_adapters/abstract/query_cache.rb", "lib/active_record/connection_adapters/abstract/quoting.rb", "lib/active_record/connection_adapters/abstract/schema_definitions.rb", "lib/active_record/connection_adapters/abstract/schema_statements.rb", "lib/active_record/connection_adapters/abstract_adapter.rb", "lib/active_record/connection_adapters/mysql_adapter.rb", "lib/active_record/connection_adapters/postgresql_adapter.rb", "lib/active_record/connection_adapters/sqlite3_adapter.rb", "lib/active_record/connection_adapters/sqlite_adapter.rb", "lib/active_record/dynamic_finder_match.rb", "lib/active_record/dynamic_scope_match.rb", "lib/active_record/fixtures.rb", "lib/active_record/locale/en.yml", "lib/active_record/locking/optimistic.rb", "lib/active_record/locking/pessimistic.rb", "lib/active_record/migration.rb", "lib/active_record/named_scope.rb", "lib/active_record/nested_attributes.rb", "lib/active_record/notifications.rb", "lib/active_record/observer.rb", "lib/active_record/query_cache.rb", "lib/active_record/reflection.rb", "lib/active_record/relation.rb", "lib/active_record/schema.rb", "lib/active_record/schema_dumper.rb", "lib/active_record/serialization.rb", "lib/active_record/serializers/xml_serializer.rb", "lib/active_record/session_store.rb", "lib/active_record/state_machine.rb", "lib/active_record/test_case.rb", "lib/active_record/timestamp.rb", "lib/active_record/transactions.rb", "lib/active_record/types/number.rb", "lib/active_record/types/object.rb", "lib/active_record/types/serialize.rb", "lib/active_record/types/time_with_zone.rb", "lib/active_record/types/unknown.rb", "lib/active_record/types.rb", "lib/active_record/validations/associated.rb", "lib/active_record/validations/uniqueness.rb", "lib/active_record/validations.rb", "lib/active_record/version.rb", "lib/active_record.rb"]
  s.homepage = %q{http://www.rubyonrails.org}
  s.rdoc_options = ["--main", "README"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{activerecord}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Implements the ActiveRecord pattern for ORM.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<activemodel>, ["= 3.0.pre"])
      s.add_runtime_dependency(%q<arel>, ["= 0.2.pre"])
    else
      s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
      s.add_dependency(%q<activemodel>, ["= 3.0.pre"])
      s.add_dependency(%q<arel>, ["= 0.2.pre"])
    end
  else
    s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
    s.add_dependency(%q<activemodel>, ["= 3.0.pre"])
    s.add_dependency(%q<arel>, ["= 0.2.pre"])
  end
end
