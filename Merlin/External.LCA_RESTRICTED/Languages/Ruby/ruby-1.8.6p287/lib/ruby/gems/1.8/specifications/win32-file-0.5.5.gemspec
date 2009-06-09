# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-file}
  s.version = "0.5.5"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2007-11-22}
  s.description = %q{Extra or redefined methods for the File class on Windows.}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES"]
  s.files = ["lib/win32/file.rb", "test/CVS", "test/sometestfile.txt", "test/tc_file_attributes.rb", "test/tc_file_constants.rb", "test/tc_file_encryption.rb", "test/tc_file_path.rb", "test/tc_file_security.rb", "test/tc_file_stat.rb", "CHANGES", "CVS", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-file.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Extra or redefined methods for the File class on Windows.}
  s.test_files = ["test/tc_file_attributes.rb", "test/tc_file_constants.rb", "test/tc_file_encryption.rb", "test/tc_file_path.rb", "test/tc_file_security.rb", "test/tc_file_stat.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<win32-file-stat>, [">= 1.2.0"])
    else
      s.add_dependency(%q<win32-file-stat>, [">= 1.2.0"])
    end
  else
    s.add_dependency(%q<win32-file-stat>, [">= 1.2.0"])
  end
end
