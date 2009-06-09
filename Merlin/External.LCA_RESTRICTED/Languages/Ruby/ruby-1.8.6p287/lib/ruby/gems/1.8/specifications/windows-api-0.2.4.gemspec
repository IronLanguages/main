# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{windows-api}
  s.version = "0.2.4"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2008-07-19}
  s.description = %q{An easier way to create methods using Win32API}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/windows/api.rb", "test/CVS", "test/test_windows_api.rb", "CHANGES", "CVS", "lib", "MANIFEST", "Rakefile", "README", "test", "windows-api.gemspec", "windows-api.gemspec~"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An easier way to create methods using Win32API}
  s.test_files = ["test/test_windows_api.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<win32-api>, [">= 1.0.5"])
    else
      s.add_dependency(%q<win32-api>, [">= 1.0.5"])
    end
  else
    s.add_dependency(%q<win32-api>, [">= 1.0.5"])
  end
end
