# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-types}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Sam Smoot"]
  s.date = %q{2009-03-29}
  s.description = %q{DataMapper plugin providing extra data types}
  s.email = ["ssmoot [a] gmail [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/dm-types.rb", "lib/dm-types/bcrypt_hash.rb", "lib/dm-types/csv.rb", "lib/dm-types/enum.rb", "lib/dm-types/epoch_time.rb", "lib/dm-types/file_path.rb", "lib/dm-types/flag.rb", "lib/dm-types/ip_address.rb", "lib/dm-types/json.rb", "lib/dm-types/regexp.rb", "lib/dm-types/serial.rb", "lib/dm-types/slug.rb", "lib/dm-types/uri.rb", "lib/dm-types/uuid.rb", "lib/dm-types/version.rb", "lib/dm-types/yaml.rb", "spec/integration/bcrypt_hash_spec.rb", "spec/integration/enum_spec.rb", "spec/integration/file_path_spec.rb", "spec/integration/flag_spec.rb", "spec/integration/ip_address_spec.rb", "spec/integration/json_spec.rb", "spec/integration/slug_spec.rb", "spec/integration/uri_spec.rb", "spec/integration/uuid_spec.rb", "spec/integration/yaml_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "spec/unit/bcrypt_hash_spec.rb", "spec/unit/csv_spec.rb", "spec/unit/enum_spec.rb", "spec/unit/epoch_time_spec.rb", "spec/unit/file_path_spec.rb", "spec/unit/flag_spec.rb", "spec/unit/ip_address_spec.rb", "spec/unit/json_spec.rb", "spec/unit/regexp_spec.rb", "spec/unit/uri_spec.rb", "spec/unit/yaml_spec.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-types}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{DataMapper plugin providing extra data types}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["= 0.9.11"])
      s.add_runtime_dependency(%q<addressable>, ["~> 2.0.2"])
    else
      s.add_dependency(%q<dm-core>, ["= 0.9.11"])
      s.add_dependency(%q<addressable>, ["~> 2.0.2"])
    end
  else
    s.add_dependency(%q<dm-core>, ["= 0.9.11"])
    s.add_dependency(%q<addressable>, ["~> 2.0.2"])
  end
end
