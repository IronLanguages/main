# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ironruby-dbi}
  s.version = "0.1.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ivan Porto Carrero"]
  s.date = %q{2010-01-14}
  s.description = %q{ADO.NET compatible DBD extension to ruby-dbo}
  s.email = ["ivan@flanders.co.nz"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = ["History.txt", "Manifest.txt", "README.txt", "Rakefile", "lib/dbd/mssql.rb", "lib/dbd/mssql/database.rb", "lib/dbd/mssql/driver.rb", "lib/dbd/mssql/statement.rb", "lib/dbd/mssql/types.rb", "lib/dbi.rb", "lib/dbi/base_classes.rb", "lib/dbi/base_classes/database.rb", "lib/dbi/base_classes/driver.rb", "lib/dbi/base_classes/statement.rb", "lib/dbi/binary.rb", "lib/dbi/columninfo.rb", "lib/dbi/exceptions.rb", "lib/dbi/handles.rb", "lib/dbi/handles/database.rb", "lib/dbi/handles/driver.rb", "lib/dbi/handles/statement.rb", "lib/dbi/row.rb", "lib/dbi/sql.rb", "lib/dbi/sql/preparedstatement.rb", "lib/dbi/sql_type_constants.rb", "lib/dbi/trace.rb", "lib/dbi/types.rb", "lib/dbi/typeutil.rb", "lib/dbi/utils.rb", "lib/dbi/utils/date.rb", "lib/dbi/utils/tableformatter.rb", "lib/dbi/utils/time.rb", "lib/dbi/utils/timestamp.rb", "lib/dbi/utils/xmlformatter.rb", "test/dbd/general/test_database.rb", "test/dbd/general/test_statement.rb", "test/dbd/general/test_types.rb"]
  s.homepage = %q{http://github.com/casualjim/ironruby-dbi}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{ironruby-dbi}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{ADO.NET compatible DBD extension to ruby-dbo}
  s.test_files = ["test/dbd/general/test_database.rb", "test/dbd/general/test_statement.rb", "test/dbd/general/test_types.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<deprecated>, [">= 2.0.1"])
      s.add_development_dependency(%q<rubyforge>, [">= 2.0.3"])
      s.add_development_dependency(%q<gemcutter>, [">= 0.3.0"])
      s.add_development_dependency(%q<hoe>, [">= 2.5.0"])
    else
      s.add_dependency(%q<deprecated>, [">= 2.0.1"])
      s.add_dependency(%q<rubyforge>, [">= 2.0.3"])
      s.add_dependency(%q<gemcutter>, [">= 0.3.0"])
      s.add_dependency(%q<hoe>, [">= 2.5.0"])
    end
  else
    s.add_dependency(%q<deprecated>, [">= 2.0.1"])
    s.add_dependency(%q<rubyforge>, [">= 2.0.3"])
    s.add_dependency(%q<gemcutter>, [">= 0.3.0"])
    s.add_dependency(%q<hoe>, [">= 2.5.0"])
  end
end
