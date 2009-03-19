# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-process}
  s.version = "0.5.9"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel Berger", "Park Heesob"]
  s.date = %q{2008-06-14}
  s.description = %q{Adds create, fork, wait, wait2, waitpid, and a special kill method}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/win32/process.rb", "test/CVS", "test/tc_process.rb", "CHANGES", "CVS", "examples", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-process.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Adds create, fork, wait, wait2, waitpid, and a special kill method}
  s.test_files = ["test/tc_process.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-pr>, [">= 0.8.6"])
    else
      s.add_dependency(%q<windows-pr>, [">= 0.8.6"])
    end
  else
    s.add_dependency(%q<windows-pr>, [">= 0.8.6"])
  end
end
