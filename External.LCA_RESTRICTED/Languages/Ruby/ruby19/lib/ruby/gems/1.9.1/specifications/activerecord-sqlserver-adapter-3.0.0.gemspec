# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{activerecord-sqlserver-adapter}
  s.version = "3.0.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ken Collins", "Murray Steele", "Shawn Balestracci", "Joe Rafaniello", "Tom Ward"]
  s.date = %q{2010-08-28}
  s.description = %q{SQL Server 2005 and 2008 Adapter For ActiveRecord}
  s.email = %q{ken@metaskills.net}
  s.extra_rdoc_files = ["README.rdoc"]
  s.files = ["CHANGELOG", "MIT-LICENSE", "README.rdoc", "lib/active_record/connection_adapters/sqlserver/core_ext/active_record.rb", "lib/active_record/connection_adapters/sqlserver/core_ext/odbc.rb", "lib/active_record/connection_adapters/sqlserver/database_limits.rb", "lib/active_record/connection_adapters/sqlserver/database_statements.rb", "lib/active_record/connection_adapters/sqlserver/errors.rb", "lib/active_record/connection_adapters/sqlserver/query_cache.rb", "lib/active_record/connection_adapters/sqlserver/quoting.rb", "lib/active_record/connection_adapters/sqlserver/schema_statements.rb", "lib/active_record/connection_adapters/sqlserver_adapter.rb", "lib/activerecord-sqlserver-adapter.rb", "lib/arel/engines/sql/compilers/sqlserver_compiler.rb"]
  s.homepage = %q{http://github.com/rails-sqlserver/activerecord-sqlserver-adapter}
  s.rdoc_options = ["--main", "README.rdoc"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{activerecord-sqlserver-adapter}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{SQL Server 2005 and 2008 Adapter For ActiveRecord.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activerecord>, ["~> 3.0"])
    else
      s.add_dependency(%q<activerecord>, ["~> 3.0"])
    end
  else
    s.add_dependency(%q<activerecord>, ["~> 3.0"])
  end
end
