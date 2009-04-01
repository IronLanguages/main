# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{pathname2}
  s.version = "1.6.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2008-11-07}
  s.description = %q{An alternate implementation of the Pathname class}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/pathname2.rb", "CHANGES", "MANIFEST", "Rakefile", "README", "test/test_pathname.rb", "test/test_pathname_windows.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{shards}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An alternate implementation of the Pathname class}
  s.test_files = ["test/test_pathname.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<facade>, [">= 1.0.0"])
    else
      s.add_dependency(%q<facade>, [">= 1.0.0"])
    end
  else
    s.add_dependency(%q<facade>, [">= 1.0.0"])
  end
end
