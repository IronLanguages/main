# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{activerecord-sqlserver-adapter}
  s.version = "2.3"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ken Collins", "Murray Steele", "Shawn Balestracci", "Joe Rafaniello", "Tom Ward"]
  s.date = %q{2009-09-09}
  s.description = %q{SQL Server 2000, 2005 and 2008 Adapter For Rails.}
  s.email = %q{ken@metaskills.net}
  s.extra_rdoc_files = ["README.rdoc"]
  s.files = ["CHANGELOG", "MIT-LICENSE", "Rakefile", "README.rdoc", "RUNNING_UNIT_TESTS", "autotest/discover.rb", "autotest/railssqlserver.rb", "autotest/sqlserver.rb", "lib/active_record/connection_adapters/sqlserver_adapter.rb", "lib/active_record/connection_adapters/sqlserver_adapter/core_ext/active_record.rb", "lib/active_record/connection_adapters/sqlserver_adapter/core_ext/dbi.rb", "test/cases/aaaa_create_tables_test_sqlserver.rb", "test/cases/adapter_test_sqlserver.rb", "test/cases/attribute_methods_test_sqlserver.rb", "test/cases/basics_test_sqlserver.rb", "test/cases/calculations_test_sqlserver.rb", "test/cases/column_test_sqlserver.rb", "test/cases/connection_test_sqlserver.rb", "test/cases/eager_association_test_sqlserver.rb", "test/cases/execute_procedure_test_sqlserver.rb", "test/cases/inheritance_test_sqlserver.rb", "test/cases/method_scoping_test_sqlserver.rb", "test/cases/migration_test_sqlserver.rb", "test/cases/named_scope_test_sqlserver.rb", "test/cases/offset_and_limit_test_sqlserver.rb", "test/cases/pessimistic_locking_test_sqlserver.rb", "test/cases/query_cache_test_sqlserver.rb", "test/cases/schema_dumper_test_sqlserver.rb", "test/cases/specific_schema_test_sqlserver.rb", "test/cases/sqlserver_helper.rb", "test/cases/table_name_test_sqlserver.rb", "test/cases/transaction_test_sqlserver.rb", "test/cases/unicode_test_sqlserver.rb", "test/cases/validations_test_sqlserver.rb", "test/connections/native_sqlserver/connection.rb", "test/connections/native_sqlserver_odbc/connection.rb", "test/migrations/transaction_table/1_table_will_never_be_created.rb", "test/schema/sqlserver_specific_schema.rb"]
  s.homepage = %q{http://github.com/rails-sqlserver}
  s.rdoc_options = ["--line-numbers", "--inline-source", "--main", "README.rdoc"]
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{SQL Server 2000, 2005 and 2008 Adapter For Rails.}
  s.test_files = ["test/cases/aaaa_create_tables_test_sqlserver.rb", "test/cases/adapter_test_sqlserver.rb", "test/cases/attribute_methods_test_sqlserver.rb", "test/cases/basics_test_sqlserver.rb", "test/cases/calculations_test_sqlserver.rb", "test/cases/column_test_sqlserver.rb", "test/cases/connection_test_sqlserver.rb", "test/cases/eager_association_test_sqlserver.rb", "test/cases/execute_procedure_test_sqlserver.rb", "test/cases/inheritance_test_sqlserver.rb", "test/cases/method_scoping_test_sqlserver.rb", "test/cases/migration_test_sqlserver.rb", "test/cases/named_scope_test_sqlserver.rb", "test/cases/offset_and_limit_test_sqlserver.rb", "test/cases/pessimistic_locking_test_sqlserver.rb", "test/cases/query_cache_test_sqlserver.rb", "test/cases/schema_dumper_test_sqlserver.rb", "test/cases/specific_schema_test_sqlserver.rb", "test/cases/sqlserver_helper.rb", "test/cases/table_name_test_sqlserver.rb", "test/cases/transaction_test_sqlserver.rb", "test/cases/unicode_test_sqlserver.rb", "test/cases/validations_test_sqlserver.rb", "test/connections/native_sqlserver/connection.rb", "test/connections/native_sqlserver_odbc/connection.rb", "test/migrations/transaction_table/1_table_will_never_be_created.rb", "test/schema/sqlserver_specific_schema.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
