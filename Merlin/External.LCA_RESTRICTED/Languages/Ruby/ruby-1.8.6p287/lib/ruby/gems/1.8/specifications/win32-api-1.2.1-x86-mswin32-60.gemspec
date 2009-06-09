# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-api}
  s.version = "1.2.1"
  s.platform = %q{x86-mswin32-60}

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger", "Park Heesob"]
  s.date = %q{2008-11-14}
  s.description = %q{A superior replacement for Win32API}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST", "ext/win32/api.c"]
  s.files = ["lib/win32/api.so", "test/test_win32_api.rb", "test/test_win32_api_callback.rb", "test/test_win32_api_function.rb", "README", "CHANGES", "MANIFEST", "ext/win32/api.c"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.2")
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{A superior replacement for Win32API}
  s.test_files = ["test/test_win32_api.rb", "test/test_win32_api_callback.rb", "test/test_win32_api_function.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
