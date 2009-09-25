# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dbi}
  s.version = "0.4.1"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Erik Hollensbe", "Christopher Maujean"]
  s.cert_chain = nil
  s.date = %q{2008-11-28}
  s.default_executable = %q{dbi}
  s.description = %q{A vendor independent interface for accessing databases, similar to Perl's DBI}
  s.email = %q{ruby-dbi-users@rubyforge.org}
  s.executables = ["dbi"]
  s.extra_rdoc_files = ["README", "LICENSE", "ChangeLog"]
  s.files = ["examples/test1.pl", "examples/test1.rb", "examples/xmltest.rb", "bin/dbi", "build/Rakefile.dbi.rb", "lib/dbi.rb", "lib/dbi/row.rb", "lib/dbi/columninfo.rb", "lib/dbi/sql/preparedstatement.rb", "lib/dbi/typeutil.rb", "lib/dbi/utils/time.rb", "lib/dbi/utils/xmlformatter.rb", "lib/dbi/utils/timestamp.rb", "lib/dbi/utils/date.rb", "lib/dbi/utils/tableformatter.rb", "lib/dbi/binary.rb", "lib/dbi/base_classes/database.rb", "lib/dbi/base_classes/driver.rb", "lib/dbi/base_classes/statement.rb", "lib/dbi/sql_type_constants.rb", "lib/dbi/handles.rb", "lib/dbi/exceptions.rb", "lib/dbi/types.rb", "lib/dbi/trace.rb", "lib/dbi/base_classes.rb", "lib/dbi/utils.rb", "lib/dbi/sql.rb", "lib/dbi/handles/database.rb", "lib/dbi/handles/driver.rb", "lib/dbi/handles/statement.rb", "test/ts_dbi.rb", "test/dbi/tc_time.rb", "test/dbi/tc_row.rb", "test/dbi/tc_dbi.rb", "test/dbi/tc_statementhandle.rb", "test/dbi/tc_sqlbind.rb", "test/dbi/tc_columninfo.rb", "test/dbi/trace.rb", "test/dbi/tc_timestamp.rb", "test/dbi/tc_types.rb", "test/dbi/tc_date.rb", "README", "LICENSE", "ChangeLog"]
  s.homepage = %q{http://www.rubyforge.org/projects/ruby-dbi}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.0")
  s.rubyforge_project = %q{ruby-dbi}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{A vendor independent interface for accessing databases, similar to Perl's DBI}
  s.test_files = ["test/ts_dbi.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<deprecated>, [">= 2.0.0"])
    else
      s.add_dependency(%q<deprecated>, [">= 2.0.0"])
    end
  else
    s.add_dependency(%q<deprecated>, [">= 2.0.0"])
  end
end
