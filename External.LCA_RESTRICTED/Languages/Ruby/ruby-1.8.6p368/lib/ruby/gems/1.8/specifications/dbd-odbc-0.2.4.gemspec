# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dbd-odbc}
  s.version = "0.2.4"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Erik Hollensbe", "Christopher Maujean"]
  s.cert_chain = nil
  s.date = %q{2008-09-27}
  s.description = %q{ODBC DBI DBD}
  s.email = %q{ruby-dbi-users@rubyforge.org}
  s.extra_rdoc_files = ["README", "LICENSE", "ChangeLog"]
  s.files = ["test/dbd/general/test_database.rb", "test/dbd/general/test_statement.rb", "test/dbd/general/test_types.rb", "test/dbd/odbc/base.rb", "test/dbd/odbc/down.sql", "test/dbd/odbc/test_statement.rb", "test/dbd/odbc/test_transactions.rb", "test/dbd/odbc/up.sql", "test/dbd/odbc/test_ping.rb", "lib/dbd/ODBC.rb", "lib/dbd/odbc/database.rb", "lib/dbd/odbc/driver.rb", "lib/dbd/odbc/statement.rb", "test/DBD_TESTS", "README", "LICENSE", "ChangeLog", "test/ts_dbd.rb"]
  s.homepage = %q{http://www.rubyforge.org/projects/ruby-dbi}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.0")
  s.rubyforge_project = %q{ruby-dbi}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{ODBC DBI DBD}
  s.test_files = ["test/ts_dbd.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dbi>, [">= 0.4.0"])
    else
      s.add_dependency(%q<dbi>, [">= 0.4.0"])
    end
  else
    s.add_dependency(%q<dbi>, [">= 0.4.0"])
  end
end
