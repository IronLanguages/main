# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{test-unit}
  s.version = "2.1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Kouhei Sutou", "Ryan Davis"]
  s.date = %q{2010-07-28}
  s.default_executable = %q{testrb}
  s.description = %q{Test::Unit 2.x - Improved version of Test::Unit bundled in
Ruby 1.8.x.

Ruby 1.9.x bundles minitest not Test::Unit. Test::Unit
bundled in Ruby 1.8.x had not been improved but unbundled
Test::Unit (Test::Unit 2.x) will be improved actively.}
  s.email = ["kou@cozmixng.org", "ryand-ruby@zenspider.com"]
  s.executables = ["testrb"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = ["COPYING", "GPL", "History.txt", "Manifest.txt", "PSFL", "README.txt", "Rakefile", "TODO", "bin/testrb", "html/bar.png", "html/bar.svg", "html/developer.png", "html/developer.svg", "html/famfamfam-logo.png", "html/favicon.ico", "html/favicon.png", "html/favicon.svg", "html/heading-mark.png", "html/heading-mark.svg", "html/index.html", "html/index.html.ja", "html/install.png", "html/install.svg", "html/jp.png", "html/kinotan-failure.png", "html/kinotan-pass.png", "html/logo.png", "html/logo.svg", "html/reference.png", "html/rubyforge.png", "html/tango-logo.png", "html/test-unit.css", "html/tutorial.png", "html/tutorial.svg", "html/us.png", "images/color-diff.png", "lib/test/unit.rb", "lib/test/unit/assertionfailederror.rb", "lib/test/unit/assertions.rb", "lib/test/unit/attribute.rb", "lib/test/unit/autorunner.rb", "lib/test/unit/collector.rb", "lib/test/unit/collector/descendant.rb", "lib/test/unit/collector/dir.rb", "lib/test/unit/collector/load.rb", "lib/test/unit/collector/objectspace.rb", "lib/test/unit/color-scheme.rb", "lib/test/unit/color.rb", "lib/test/unit/diff.rb", "lib/test/unit/error.rb", "lib/test/unit/exceptionhandler.rb", "lib/test/unit/failure.rb", "lib/test/unit/fixture.rb", "lib/test/unit/notification.rb", "lib/test/unit/omission.rb", "lib/test/unit/pending.rb", "lib/test/unit/priority.rb", "lib/test/unit/runner/console.rb", "lib/test/unit/runner/emacs.rb", "lib/test/unit/runner/tap.rb", "lib/test/unit/testcase.rb", "lib/test/unit/testresult.rb", "lib/test/unit/testsuite.rb", "lib/test/unit/ui/console/outputlevel.rb", "lib/test/unit/ui/console/testrunner.rb", "lib/test/unit/ui/emacs/testrunner.rb", "lib/test/unit/ui/tap/testrunner.rb", "lib/test/unit/ui/testrunner.rb", "lib/test/unit/ui/testrunnermediator.rb", "lib/test/unit/ui/testrunnerutilities.rb", "lib/test/unit/util/backtracefilter.rb", "lib/test/unit/util/method-owner-finder.rb", "lib/test/unit/util/observable.rb", "lib/test/unit/util/output.rb", "lib/test/unit/util/procwrapper.rb", "lib/test/unit/version.rb", "sample/adder.rb", "sample/subtracter.rb", "sample/test_adder.rb", "sample/test_subtracter.rb", "sample/test_user.rb", "test/collector/test-descendant.rb", "test/collector/test-load.rb", "test/collector/test_dir.rb", "test/collector/test_objectspace.rb", "test/run-test.rb", "test/test-attribute.rb", "test/test-color-scheme.rb", "test/test-color.rb", "test/test-diff.rb", "test/test-emacs-runner.rb", "test/test-fixture.rb", "test/test-notification.rb", "test/test-omission.rb", "test/test-pending.rb", "test/test-priority.rb", "test/test-testcase.rb", "test/test_assertions.rb", "test/test_error.rb", "test/test_failure.rb", "test/test_testresult.rb", "test/test_testsuite.rb", "test/testunit-test-util.rb", "test/ui/test_tap.rb", "test/ui/test_testrunmediator.rb", "test/util/test-method-owner-finder.rb", "test/util/test-output.rb", "test/util/test_backtracefilter.rb", "test/util/test_observable.rb", "test/util/test_procwrapper.rb"]
  s.homepage = %q{http://rubyforge.org/projects/test-unit/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{test-unit}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{Test::Unit 2.x - Improved version of Test::Unit bundled in Ruby 1.8.x}
  s.test_files = ["test/ui/test_testrunmediator.rb", "test/ui/test_tap.rb", "test/test_testresult.rb", "test/test_failure.rb", "test/test_testsuite.rb", "test/collector/test_objectspace.rb", "test/collector/test_dir.rb", "test/util/test_backtracefilter.rb", "test/util/test_observable.rb", "test/util/test_procwrapper.rb", "test/test_error.rb", "test/test_assertions.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<rubyforge>, [">= 2.0.4"])
      s.add_development_dependency(%q<hoe>, [">= 2.6.1"])
    else
      s.add_dependency(%q<rubyforge>, [">= 2.0.4"])
      s.add_dependency(%q<hoe>, [">= 2.6.1"])
    end
  else
    s.add_dependency(%q<rubyforge>, [">= 2.0.4"])
    s.add_dependency(%q<hoe>, [">= 2.6.1"])
  end
end
