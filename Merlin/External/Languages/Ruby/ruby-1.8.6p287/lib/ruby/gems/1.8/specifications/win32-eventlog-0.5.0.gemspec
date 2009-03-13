# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-eventlog}
  s.version = "0.5.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger", "Park Heesob"]
  s.date = %q{2008-09-12}
  s.description = %q{Interface for the MS Windows Event Log.}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST", "doc/tutorial.txt"]
  s.files = ["lib/win32/eventlog.rb", "lib/win32/mc.rb", "test/foo.mc", "test/test_eventlog.rb", "test/test_mc.rb", "CHANGES", "doc", "examples", "lib", "MANIFEST", "misc", "Rakefile", "README", "test", "win32-eventlog-0.5.0.gem", "win32-eventlog.gemspec", "doc/tutorial.txt"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Interface for the MS Windows Event Log.}
  s.test_files = ["test/test_eventlog.rb", "test/test_mc.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-pr>, [">= 0.9.3"])
      s.add_runtime_dependency(%q<ptools>, [">= 1.1.6"])
      s.add_runtime_dependency(%q<test-unit>, [">= 2.0.0"])
    else
      s.add_dependency(%q<windows-pr>, [">= 0.9.3"])
      s.add_dependency(%q<ptools>, [">= 1.1.6"])
      s.add_dependency(%q<test-unit>, [">= 2.0.0"])
    end
  else
    s.add_dependency(%q<windows-pr>, [">= 0.9.3"])
    s.add_dependency(%q<ptools>, [">= 1.1.6"])
    s.add_dependency(%q<test-unit>, [">= 2.0.0"])
  end
end
