# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-open3}
  s.version = "0.2.9"
  s.platform = %q{x86-mswin32-60}

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Park Heesob", "Daniel Berger"]
  s.date = %q{2009-03-07}
  s.description = %q{Provides an Open3.popen3 implementation for MS Windows}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST", "ext/win32/open3.c"]
  s.files = ["lib/win32/open3.so", "test/test_win32_open3.rb", "README", "CHANGES", "MANIFEST", "ext/win32/open3.c"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.2")
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Provides an Open3.popen3 implementation for MS Windows}
  s.test_files = ["test/test_win32_open3.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
