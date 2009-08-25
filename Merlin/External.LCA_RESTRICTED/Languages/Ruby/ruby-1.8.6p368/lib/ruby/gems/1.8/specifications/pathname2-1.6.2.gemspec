# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{pathname2}
  s.version = "1.6.2"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2009-08-03}
  s.description = %q{      The pathname2 library provides an implementation of the Pathname
      class different from the one that ships as part of the Ruby standard
      library. It is a subclass of String, though several methods have been
      overridden to better fit a path context. In addition, it supports file
      URL's as paths, provides additional methods for Windows paths, and
      handles UNC paths on Windows properly. See the README file for more
      details.
}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["benchmarks/bench_pathname.rb", "benchmarks/bench_plus.rb", "examples/example_pathname.rb", "lib/pathname2.rb", "test/test_pathname.rb", "test/test_pathname_windows.rb", "README", "CHANGES", "MANIFEST"]
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.licenses = ["Artistic 2.0"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{shards}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{An alternate implementation of the Pathname class}
  s.test_files = ["test/test_pathname.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<facade>, [">= 1.0.4"])
      s.add_development_dependency(%q<test-unit>, [">= 2.0.3"])
    else
      s.add_dependency(%q<facade>, [">= 1.0.4"])
      s.add_dependency(%q<test-unit>, [">= 2.0.3"])
    end
  else
    s.add_dependency(%q<facade>, [">= 1.0.4"])
    s.add_dependency(%q<test-unit>, [">= 2.0.3"])
  end
end
