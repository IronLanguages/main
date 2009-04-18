# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{minitest}
  s.version = "1.3.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis"]
  s.date = %q{2009-01-20}
  s.description = %q{minitest/unit is a small and fast replacement for ruby's huge and slow test/unit. This is meant to be clean and easy to use both as a regular test writer and for language implementors that need a minimal set of methods to bootstrap a working unit test suite.  mini/spec is a functionally complete spec engine.  mini/mock, by Steven Baker, is a beautifully tiny mock object framework.  (This package was called miniunit once upon a time)}
  s.email = ["ryand-ruby@zenspider.com"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = [".autotest", "History.txt", "Manifest.txt", "README.txt", "Rakefile", "lib/minitest/autorun.rb", "lib/minitest/mock.rb", "lib/minitest/spec.rb", "lib/minitest/unit.rb", "test/test_mini_mock.rb", "test/test_mini_spec.rb", "test/test_mini_test.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://rubyforge.org/projects/bfts}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{bfts}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{minitest/unit is a small and fast replacement for ruby's huge and slow test/unit}
  s.test_files = ["test/test_mini_mock.rb", "test/test_mini_spec.rb", "test/test_mini_test.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<hoe>, [">= 1.8.2"])
    else
      s.add_dependency(%q<hoe>, [">= 1.8.2"])
    end
  else
    s.add_dependency(%q<hoe>, [">= 1.8.2"])
  end
end
