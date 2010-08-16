# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{test-spec}
  s.version = "0.10.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Christian Neukirchen"]
  s.date = %q{2009-01-31}
  s.default_executable = %q{specrb}
  s.description = %q{test/spec layers an RSpec-inspired interface on top of Test::Unit, so you can mix TDD and BDD (Behavior-Driven Development).  test/spec is a clean-room implementation that maps most kinds of Test::Unit assertions to a `should'-like syntax.}
  s.email = %q{chneukirchen@gmail.com}
  s.executables = ["specrb"]
  s.extra_rdoc_files = ["README", "SPECS", "ROADMAP"]
  s.files = ["bin/specrb", "examples/stack.rb", "examples/stack_spec.rb", "lib/test/spec/dox.rb", "lib/test/spec/rdox.rb", "lib/test/spec/should-output.rb", "lib/test/spec/version.rb", "lib/test/spec.rb", "Rakefile", "README", "ROADMAP", "test/spec_dox.rb", "test/spec_flexmock.rb", "test/spec_mocha.rb", "test/spec_nestedcontexts.rb", "test/spec_new_style.rb", "test/spec_should-output.rb", "test/spec_testspec.rb", "test/spec_testspec_order.rb", "test/test_testunit.rb", "TODO", "SPECS"]
  s.homepage = %q{http://test-spec.rubyforge.org}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{test-spec}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{a Behaviour Driven Development interface for Test::Unit}
  s.test_files = ["test/test_testunit.rb", "test/spec_dox.rb", "test/spec_flexmock.rb", "test/spec_mocha.rb", "test/spec_nestedcontexts.rb", "test/spec_new_style.rb", "test/spec_should-output.rb", "test/spec_testspec.rb", "test/spec_testspec_order.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
