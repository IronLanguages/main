# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ZenTest}
  s.version = "4.1.4"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis", "Eric Hodel"]
  s.date = %q{2009-08-07}
  s.description = %q{ZenTest provides 4 different tools: zentest, unit_diff, autotest, and
multiruby.

ZenTest scans your target and unit-test code and writes your missing
code based on simple naming rules, enabling XP at a much quicker
pace. ZenTest only works with Ruby and Test::Unit. Nobody uses this
tool anymore but it is the package namesake, so it stays.

unit_diff is a command-line filter to diff expected results from
actual results and allow you to quickly see exactly what is wrong.

autotest is a continous testing facility meant to be used during
development. As soon as you save a file, autotest will run the
corresponding dependent tests.

multiruby runs anything you want on multiple versions of ruby. Great
for compatibility checking! Use multiruby_setup to manage your
installed versions.}
  s.email = ["ryand-ruby@zenspider.com", "drbrain@segment7.net"]
  s.executables = ["autotest", "multigem", "multiruby", "multiruby_setup", "unit_diff", "zentest"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt", "articles/how_to_use_zentest.txt", "example.txt"]
  s.files = [".autotest", "History.txt", "Manifest.txt", "README.txt", "Rakefile", "articles/Article.css", "articles/getting_started_with_autotest.html", "articles/how_to_use_zentest.txt", "bin/autotest", "bin/multigem", "bin/multiruby", "bin/multiruby_setup", "bin/unit_diff", "bin/zentest", "example.txt", "example1.rb", "example2.rb", "example_dot_autotest.rb", "lib/autotest.rb", "lib/autotest/autoupdate.rb", "lib/autotest/once.rb", "lib/autotest/rcov.rb", "lib/autotest/restart.rb", "lib/autotest/timestamp.rb", "lib/focus.rb", "lib/functional_test_matrix.rb", "lib/multiruby.rb", "lib/unit_diff.rb", "lib/zentest.rb", "lib/zentest_mapping.rb", "test/test_autotest.rb", "test/test_focus.rb", "test/test_unit_diff.rb", "test/test_zentest.rb", "test/test_zentest_mapping.rb"]
  s.homepage = %q{http://www.zenspider.com/ZSS/Products/ZenTest/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{zentest}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{ZenTest provides 4 different tools: zentest, unit_diff, autotest, and multiruby}
  s.test_files = ["test/test_autotest.rb", "test/test_focus.rb", "test/test_unit_diff.rb", "test/test_zentest.rb", "test/test_zentest_mapping.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<hoe>, [">= 2.3.3"])
    else
      s.add_dependency(%q<hoe>, [">= 2.3.3"])
    end
  else
    s.add_dependency(%q<hoe>, [">= 2.3.3"])
  end
end
