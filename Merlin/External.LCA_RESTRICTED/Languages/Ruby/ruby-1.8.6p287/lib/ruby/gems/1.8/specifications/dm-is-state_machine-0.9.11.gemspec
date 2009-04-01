# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-is-state_machine}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["David James"]
  s.date = %q{2009-03-10}
  s.description = %q{DataMapper plugin for creating state machines}
  s.email = ["djwonk [a] collectiveinsight [d] net"]
  s.extra_rdoc_files = ["README.txt", "README.markdown", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.markdown", "README.txt", "Rakefile", "TODO", "lib/dm-is-state_machine.rb", "lib/dm-is-state_machine/is/data/event.rb", "lib/dm-is-state_machine/is/data/machine.rb", "lib/dm-is-state_machine/is/data/state.rb", "lib/dm-is-state_machine/is/dsl/event_dsl.rb", "lib/dm-is-state_machine/is/dsl/state_dsl.rb", "lib/dm-is-state_machine/is/state_machine.rb", "lib/dm-is-state_machine/is/version.rb", "spec/examples/invalid_events.rb", "spec/examples/invalid_states.rb", "spec/examples/invalid_transitions_1.rb", "spec/examples/invalid_transitions_2.rb", "spec/examples/slot_machine.rb", "spec/examples/traffic_light.rb", "spec/integration/invalid_events_spec.rb", "spec/integration/invalid_states_spec.rb", "spec/integration/invalid_transitions_spec.rb", "spec/integration/slot_machine_spec.rb", "spec/integration/traffic_light_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "spec/unit/data/event_spec.rb", "spec/unit/data/machine_spec.rb", "spec/unit/data/state_spec.rb", "spec/unit/dsl/event_dsl_spec.rb", "spec/unit/dsl/state_dsl_spec.rb", "spec/unit/state_machine_spec.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-is-state_machine}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{DataMapper plugin for creating state machines}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.11"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
  end
end
