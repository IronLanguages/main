# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{activemodel}
  s.version = "3.0.pre"

  s.required_rubygems_version = Gem::Requirement.new("> 1.3.1") if s.respond_to? :required_rubygems_version=
  s.authors = ["David Heinemeier Hansson"]
  s.date = %q{2009-12-08}
  s.description = %q{Extracts common modeling concerns from ActiveRecord to share between similar frameworks like ActiveResource.}
  s.email = %q{david@loudthinking.com}
  s.files = ["CHANGELOG", "MIT-LICENSE", "README", "lib/active_model/attribute_methods.rb", "lib/active_model/conversion.rb", "lib/active_model/deprecated_error_methods.rb", "lib/active_model/dirty.rb", "lib/active_model/errors.rb", "lib/active_model/lint.rb", "lib/active_model/locale/en.yml", "lib/active_model/naming.rb", "lib/active_model/observing.rb", "lib/active_model/serialization.rb", "lib/active_model/serializers/json.rb", "lib/active_model/serializers/xml.rb", "lib/active_model/state_machine/event.rb", "lib/active_model/state_machine/machine.rb", "lib/active_model/state_machine/state.rb", "lib/active_model/state_machine/state_transition.rb", "lib/active_model/state_machine.rb", "lib/active_model/test_case.rb", "lib/active_model/translation.rb", "lib/active_model/validations/acceptance.rb", "lib/active_model/validations/confirmation.rb", "lib/active_model/validations/exclusion.rb", "lib/active_model/validations/format.rb", "lib/active_model/validations/inclusion.rb", "lib/active_model/validations/length.rb", "lib/active_model/validations/numericality.rb", "lib/active_model/validations/presence.rb", "lib/active_model/validations/with.rb", "lib/active_model/validations.rb", "lib/active_model/validations_repair_helper.rb", "lib/active_model/validator.rb", "lib/active_model/version.rb", "lib/active_model.rb"]
  s.homepage = %q{http://www.rubyonrails.org}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{activemodel}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{A toolkit for building other modeling frameworks like ActiveRecord}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, ["= 3.0.pre"])
    else
      s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
    end
  else
    s.add_dependency(%q<activesupport>, ["= 3.0.pre"])
  end
end
