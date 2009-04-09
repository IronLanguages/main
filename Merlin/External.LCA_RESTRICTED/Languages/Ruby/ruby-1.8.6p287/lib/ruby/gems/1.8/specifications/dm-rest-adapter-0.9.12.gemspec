# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-rest-adapter}
  s.version = "0.9.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Scott Burton @ Joyent Inc"]
  s.date = %q{2009-03-10}
  s.description = %q{REST Adapter for DataMapper}
  s.email = ["scott.burton [a] joyent [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.markdown", "README.txt", "Rakefile", "TODO", "config/database.rb.example", "dm-rest-adapter.gemspec", "lib/rest_adapter.rb", "lib/rest_adapter/adapter.rb", "lib/rest_adapter/connection.rb", "lib/rest_adapter/exceptions.rb", "lib/rest_adapter/formats.rb", "lib/rest_adapter/version.rb", "spec/connection_spec.rb", "spec/crud_spec.rb", "spec/ruby_forker.rb", "spec/spec.opts", "spec/spec_helper.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/datamapper/dm-more/tree/master/adapters/dm-rest-adapter}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{REST Adapter for DataMapper}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.12"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.12"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.12"])
  end
end
