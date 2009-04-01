# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-dir}
  s.version = "0.3.2"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.cert_chain = nil
  s.date = %q{2007-07-25}
  s.description = %q{Extra constants and methods for the Dir class on Windows.}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/win32/dir.rb", "test/CVS", "test/tc_dir.rb", "CHANGES", "CVS", "examples", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-dir.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Extra constants and methods for the Dir class on Windows.}
  s.test_files = ["test/tc_dir.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-pr>, [">= 0.5.1"])
    else
      s.add_dependency(%q<windows-pr>, [">= 0.5.1"])
    end
  else
    s.add_dependency(%q<windows-pr>, [">= 0.5.1"])
  end
end
