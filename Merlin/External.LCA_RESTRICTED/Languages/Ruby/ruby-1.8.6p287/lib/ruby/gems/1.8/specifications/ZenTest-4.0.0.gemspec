# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ZenTest}
  s.version = "4.0.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis", "Eric Hodel"]
  s.date = %q{2009-03-02}
  s.description = %q{ZenTest provides 4 different tools and 1 library: zentest, unit_diff, autotest, multiruby, and Test::Rails.  ZenTest scans your target and unit-test code and writes your missing code based on simple naming rules, enabling XP at a much quicker pace. ZenTest only works with Ruby and Test::Unit.  unit_diff is a command-line filter to diff expected results from actual results and allow you to quickly see exactly what is wrong.  autotest is a continous testing facility meant to be used during development. As soon as you save a file, autotest will run the corresponding dependent tests.  multiruby runs anything you want on multiple versions of ruby. Great for compatibility checking! Use multiruby_setup to manage your installed versions.  Test::Rails helps you build industrial-strength Rails code.}
  s.email = ["ryand-ruby@zenspider.com", "drbrain@segment7.net"]
  s.executables = ["autotest", "multigem", "multiruby", "multiruby_setup", "unit_diff", "zentest"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt", "articles/how_to_use_zentest.txt", "example.txt"]
  s.files = [".autotest", "History.txt", "Manifest.txt", "README.txt", "Rakefile", "articles/Article.css", "articles/getting_started_with_autotest.html", "articles/how_to_use_zentest.txt", "bin/autotest", "bin/multigem", "bin/multiruby", "bin/multiruby_setup", "bin/unit_diff", "bin/zentest", "example.txt", "example1.rb", "example2.rb", "example_dot_autotest.rb", "lib/autotest.rb", "lib/autotest/autoupdate.rb", "lib/autotest/camping.rb", "lib/autotest/cctray.rb", "lib/autotest/discover.rb", "lib/autotest/emacs.rb", "lib/autotest/email_notify.rb", "lib/autotest/fixtures.rb", "lib/autotest/growl.rb", "lib/autotest/heckle.rb", "lib/autotest/html_report.rb", "lib/autotest/jabber_notify.rb", "lib/autotest/kdenotify.rb", "lib/autotest/menu.rb", "lib/autotest/migrate.rb", "lib/autotest/notify.rb", "lib/autotest/once.rb", "lib/autotest/pretty.rb", "lib/autotest/rails.rb", "lib/autotest/rcov.rb", "lib/autotest/redgreen.rb", "lib/autotest/restart.rb", "lib/autotest/shame.rb", "lib/autotest/snarl.rb", "lib/autotest/timestamp.rb", "lib/focus.rb", "lib/functional_test_matrix.rb", "lib/multiruby.rb", "lib/unit_diff.rb", "lib/zentest.rb", "lib/zentest_mapping.rb", "test/test_autotest.rb", "test/test_focus.rb", "test/test_unit_diff.rb", "test/test_zentest.rb", "test/test_zentest_mapping.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.zenspider.com/ZSS/Products/ZenTest/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{zentest}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{ZenTest provides 4 different tools and 1 library: zentest, unit_diff, autotest, multiruby, and Test::Rails}
  s.test_files = ["test/test_autotest.rb", "test/test_focus.rb", "test/test_unit_diff.rb", "test/test_zentest.rb", "test/test_zentest_mapping.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<hoe>, [">= 1.9.0"])
    else
      s.add_dependency(%q<hoe>, [">= 1.9.0"])
    end
  else
    s.add_dependency(%q<hoe>, [">= 1.9.0"])
  end
end
