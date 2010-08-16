# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{flexmock}
  s.version = "0.8.7"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Jim Weirich"]
  s.date = %q{2010-07-19}
  s.description = %q{
      FlexMock is a extremely simple mock object class compatible
      with the Test::Unit framework.  Although the FlexMock's
      interface is simple, it is very flexible.
    }
  s.email = %q{jim@weirichhouse.org}
  s.extra_rdoc_files = ["README.rdoc", "CHANGES", "doc/GoogleExample.rdoc", "doc/releases/flexmock-0.4.0.rdoc", "doc/releases/flexmock-0.4.1.rdoc", "doc/releases/flexmock-0.4.2.rdoc", "doc/releases/flexmock-0.4.3.rdoc", "doc/releases/flexmock-0.5.0.rdoc", "doc/releases/flexmock-0.5.1.rdoc", "doc/releases/flexmock-0.6.0.rdoc", "doc/releases/flexmock-0.6.1.rdoc", "doc/releases/flexmock-0.6.2.rdoc", "doc/releases/flexmock-0.6.3.rdoc", "doc/releases/flexmock-0.6.4.rdoc", "doc/releases/flexmock-0.7.0.rdoc", "doc/releases/flexmock-0.7.1.rdoc", "doc/releases/flexmock-0.8.0.rdoc", "doc/releases/flexmock-0.8.2.rdoc", "doc/releases/flexmock-0.8.3.rdoc", "doc/releases/flexmock-0.8.4.rdoc", "doc/releases/flexmock-0.8.5.rdoc"]
  s.files = ["CHANGES", "Rakefile", "README.rdoc", "TAGS", "lib/flexmock/argument_matchers.rb", "lib/flexmock/argument_types.rb", "lib/flexmock/base.rb", "lib/flexmock/composite.rb", "lib/flexmock/core.rb", "lib/flexmock/core_class_methods.rb", "lib/flexmock/default_framework_adapter.rb", "lib/flexmock/deprecated_methods.rb", "lib/flexmock/errors.rb", "lib/flexmock/expectation.rb", "lib/flexmock/expectation_director.rb", "lib/flexmock/mock_container.rb", "lib/flexmock/noop.rb", "lib/flexmock/ordering.rb", "lib/flexmock/partial_mock.rb", "lib/flexmock/rails/view_mocking.rb", "lib/flexmock/rails.rb", "lib/flexmock/recorder.rb", "lib/flexmock/rspec.rb", "lib/flexmock/test_unit.rb", "lib/flexmock/test_unit_integration.rb", "lib/flexmock/undefined.rb", "lib/flexmock/validators.rb", "lib/flexmock.rb", "test/asserts.rb", "test/redirect_error.rb", "test/rspec_integration/integration_spec.rb", "test/test_aliasing.rb", "test/test_container_methods.rb", "test/test_default_framework_adapter.rb", "test/test_demeter_mocking.rb", "test/test_deprecated_methods.rb", "test/test_examples_from_readme.rb", "test/test_extended_should_receive.rb", "test/test_flexmodel.rb", "test/test_naming.rb", "test/test_new_instances.rb", "test/test_partial_mock.rb", "test/test_rails_view_stub.rb", "test/test_record_mode.rb", "test/test_samples.rb", "test/test_should_ignore_missing.rb", "test/test_should_receive.rb", "test/test_tu_integration.rb", "test/test_undefined.rb", "test/test_unit_integration/test_auto_test_unit.rb", "flexmock.blurb", "install.rb", "doc/GoogleExample.rdoc", "doc/releases/flexmock-0.4.0.rdoc", "doc/releases/flexmock-0.4.1.rdoc", "doc/releases/flexmock-0.4.2.rdoc", "doc/releases/flexmock-0.4.3.rdoc", "doc/releases/flexmock-0.5.0.rdoc", "doc/releases/flexmock-0.5.1.rdoc", "doc/releases/flexmock-0.6.0.rdoc", "doc/releases/flexmock-0.6.1.rdoc", "doc/releases/flexmock-0.6.2.rdoc", "doc/releases/flexmock-0.6.3.rdoc", "doc/releases/flexmock-0.6.4.rdoc", "doc/releases/flexmock-0.7.0.rdoc", "doc/releases/flexmock-0.7.1.rdoc", "doc/releases/flexmock-0.8.0.rdoc", "doc/releases/flexmock-0.8.2.rdoc", "doc/releases/flexmock-0.8.3.rdoc", "doc/releases/flexmock-0.8.4.rdoc", "doc/releases/flexmock-0.8.5.rdoc"]
  s.homepage = %q{http://flexmock.rubyforge.org}
  s.rdoc_options = ["--title", "Flex Mock", "--main", "README", "--line-numbers"]
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{Simple and Flexible Mock Objects for Testing}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
