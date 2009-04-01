# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-file-stat}
  s.version = "1.3.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger", "Park Heesob"]
  s.date = %q{2008-08-09}
  s.description = %q{A File::Stat class tailored to MS Windows}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES"]
  s.files = ["lib/win32/file/stat.rb", "CHANGES", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-file-stat.gemspec", "test/sometestfile.exe", "test/sometestfile.txt", "test/test_file_stat.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{Win32Utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{A File::Stat class tailored to MS Windows}
  s.test_files = ["test/test_file_stat.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-pr>, [">= 0.9.1"])
    else
      s.add_dependency(%q<windows-pr>, [">= 0.9.1"])
    end
  else
    s.add_dependency(%q<windows-pr>, [">= 0.9.1"])
  end
end
