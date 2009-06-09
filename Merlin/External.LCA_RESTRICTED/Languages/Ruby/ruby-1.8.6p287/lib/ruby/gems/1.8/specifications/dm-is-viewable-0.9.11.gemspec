# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-is-viewable}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Cory O'Daniel"]
  s.date = %q{2009-03-10}
  s.description = %q{Viewable of a DataMapper plugin}
  s.email = ["dm-is-viewable [a] coryodaniel [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/dm-is-viewable.rb", "lib/dm-is-viewable/is/version.rb", "lib/dm-is-viewable/is/viewable.rb", "spec/data/location.rb", "spec/data/person.rb", "spec/integration/viewable_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-is-viewable}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Viewable of a DataMapper plugin}

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
