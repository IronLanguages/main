# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-clipboard}
  s.version = "0.4.4"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger", "Park Heesob"]
  s.date = %q{2008-08-26}
  s.description = %q{A library for interacting with the Windows clipboard}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["CHANGES", "README", "MANIFEST"]
  s.files = ["examples/clipboard_test.rb", "lib/win32/clipboard.rb", "test/test_clipboard.rb", "CHANGES", "examples", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-clipboard.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{A library for interacting with the Windows clipboard}
  s.test_files = ["test/test_clipboard.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-pr>, [">= 0.8.1"])
    else
      s.add_dependency(%q<windows-pr>, [">= 0.8.1"])
    end
  else
    s.add_dependency(%q<windows-pr>, [">= 0.8.1"])
  end
end
