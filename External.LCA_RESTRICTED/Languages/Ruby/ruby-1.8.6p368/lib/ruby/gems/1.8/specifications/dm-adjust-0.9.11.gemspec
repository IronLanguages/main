# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-adjust}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Sindre Aarsaether"]
  s.date = %q{2009-03-29}
  s.description = %q{DataMapper plugin providing methods to increment and decrement properties}
  s.email = ["sindre [a] identu [d] no"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/dm-adjust.rb", "lib/dm-adjust/adapters/data_objects_adapter.rb", "lib/dm-adjust/collection.rb", "lib/dm-adjust/model.rb", "lib/dm-adjust/repository.rb", "lib/dm-adjust/resource.rb", "lib/dm-adjust/version.rb", "spec/integration/adjust_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-adjust}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{DataMapper plugin providing methods to increment and decrement properties}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["= 0.9.11"])
    else
      s.add_dependency(%q<dm-core>, ["= 0.9.11"])
    end
  else
    s.add_dependency(%q<dm-core>, ["= 0.9.11"])
  end
end
