# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb_datamapper}
  s.version = "1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Jason Toy"]
  s.date = %q{2009-02-16}
  s.description = %q{DataMapper plugin providing DataMapper support for Merb}
  s.email = %q{jtoy@rubynow.com}
  s.extra_rdoc_files = ["LICENSE", "TODO"]
  s.files = ["LICENSE", "Rakefile", "TODO", "Generators", "lib/merb", "lib/merb/orms", "lib/merb/orms/data_mapper", "lib/merb/orms/data_mapper/connection.rb", "lib/merb/orms/data_mapper/database.yml.sample", "lib/merb/session", "lib/merb/session/data_mapper_session.rb", "lib/generators", "lib/generators/templates", "lib/generators/templates/views", "lib/generators/templates/views/new.html.erb", "lib/generators/templates/views/index.html.erb", "lib/generators/templates/views/edit.html.erb", "lib/generators/templates/views/show.html.erb", "lib/generators/templates/model.rb", "lib/generators/templates/migration.rb", "lib/generators/templates/resource_controller.rb", "lib/generators/templates/model_migration.rb", "lib/generators/data_mapper_migration.rb", "lib/generators/data_mapper_resource_controller.rb", "lib/generators/data_mapper_model.rb", "lib/merb_datamapper.rb", "lib/merb_datamapper", "lib/merb_datamapper/version.rb", "lib/merb_datamapper/merbtasks.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{DataMapper plugin providing DataMapper support for Merb}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, [">= 0.9.5"])
      s.add_runtime_dependency(%q<dm-migrations>, [">= 0.9.5"])
      s.add_runtime_dependency(%q<merb-core>, ["~> 1.1"])
    else
      s.add_dependency(%q<dm-core>, [">= 0.9.5"])
      s.add_dependency(%q<dm-migrations>, [">= 0.9.5"])
      s.add_dependency(%q<merb-core>, ["~> 1.1"])
    end
  else
    s.add_dependency(%q<dm-core>, [">= 0.9.5"])
    s.add_dependency(%q<dm-migrations>, [">= 0.9.5"])
    s.add_dependency(%q<merb-core>, ["~> 1.1"])
  end
end
