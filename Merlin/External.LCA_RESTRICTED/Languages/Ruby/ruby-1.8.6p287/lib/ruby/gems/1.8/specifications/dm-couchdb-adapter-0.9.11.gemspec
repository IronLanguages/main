# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-couchdb-adapter}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Bernerd Schaefer"]
  s.date = %q{2009-03-10}
  s.description = %q{CouchDB Adapter for DataMapper}
  s.email = ["bernerd [a] wieck [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = [".gitignore", "History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/couchdb_adapter.rb", "lib/couchdb_adapter/attachments.rb", "lib/couchdb_adapter/couch_resource.rb", "lib/couchdb_adapter/json_object.rb", "lib/couchdb_adapter/version.rb", "lib/couchdb_adapter/view.rb", "spec/couchdb_adapter_spec.rb", "spec/couchdb_attachments_spec.rb", "spec/couchdb_view_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "spec/testfile.txt", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/adapters/dm-couchdb-adapter}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{CouchDB Adapter for DataMapper}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.11"])
      s.add_runtime_dependency(%q<mime-types>, ["~> 1.15"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
      s.add_dependency(%q<mime-types>, ["~> 1.15"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
    s.add_dependency(%q<mime-types>, ["~> 1.15"])
  end
end
