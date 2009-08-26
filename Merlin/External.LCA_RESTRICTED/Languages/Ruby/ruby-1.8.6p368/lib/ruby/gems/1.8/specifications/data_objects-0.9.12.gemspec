# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{data_objects}
  s.version = "0.9.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Dirkjan Bussink"]
  s.date = %q{2009-05-16}
  s.description = %q{Provide a standard and simplified API for communicating with RDBMS from Ruby}
  s.email = %q{d.bussink@gmail.com}
  s.files = ["lib/data_objects/command.rb", "lib/data_objects/connection.rb", "lib/data_objects/logger.rb", "lib/data_objects/quoting.rb", "lib/data_objects/reader.rb", "lib/data_objects/result.rb", "lib/data_objects/spec/command_spec.rb", "lib/data_objects/spec/connection_spec.rb", "lib/data_objects/spec/encoding_spec.rb", "lib/data_objects/spec/quoting_spec.rb", "lib/data_objects/spec/reader_spec.rb", "lib/data_objects/spec/result_spec.rb", "lib/data_objects/spec/typecast/array_spec.rb", "lib/data_objects/spec/typecast/bigdecimal_spec.rb", "lib/data_objects/spec/typecast/boolean_spec.rb", "lib/data_objects/spec/typecast/byte_array_spec.rb", "lib/data_objects/spec/typecast/class_spec.rb", "lib/data_objects/spec/typecast/date_spec.rb", "lib/data_objects/spec/typecast/datetime_spec.rb", "lib/data_objects/spec/typecast/float_spec.rb", "lib/data_objects/spec/typecast/integer_spec.rb", "lib/data_objects/spec/typecast/ipaddr_spec.rb", "lib/data_objects/spec/typecast/nil_spec.rb", "lib/data_objects/spec/typecast/range_spec.rb", "lib/data_objects/spec/typecast/string_spec.rb", "lib/data_objects/spec/typecast/time_spec.rb", "lib/data_objects/transaction.rb", "lib/data_objects/uri.rb", "lib/data_objects/version.rb", "lib/data_objects.rb", "spec/command_spec.rb", "spec/connection_spec.rb", "spec/do_mock.rb", "spec/lib/pending_helpers.rb", "spec/lib/rspec_immediate_feedback_formatter.rb", "spec/reader_spec.rb", "spec/result_spec.rb", "spec/spec_helper.rb", "spec/transaction_spec.rb", "spec/uri_spec.rb", "tasks/gem.rake", "tasks/install.rake", "tasks/release.rake", "tasks/spec.rake", "LICENSE", "Rakefile", "History.txt", "Manifest.txt", "README.txt"]
  s.homepage = %q{http://github.com/datamapper/do}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{dorb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{DataObjects basic API and shared driver specifications}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<addressable>, ["~> 2.0.0"])
      s.add_runtime_dependency(%q<extlib>, ["~> 0.9.12"])
      s.add_development_dependency(%q<rspec>, ["~> 1.2.0"])
    else
      s.add_dependency(%q<addressable>, ["~> 2.0.0"])
      s.add_dependency(%q<extlib>, ["~> 0.9.12"])
      s.add_dependency(%q<rspec>, ["~> 1.2.0"])
    end
  else
    s.add_dependency(%q<addressable>, ["~> 2.0.0"])
    s.add_dependency(%q<extlib>, ["~> 0.9.12"])
    s.add_dependency(%q<rspec>, ["~> 1.2.0"])
  end
end
