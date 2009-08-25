# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-haml}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Yehuda Katz"]
  s.date = %q{2009-06-30}
  s.description = %q{Merb plugin that provides HAML support}
  s.email = %q{ykatz@engineyard.com}
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "Generators", "lib/generators/controller.rb", "lib/generators/layout.rb", "lib/generators/resource_controller.rb", "lib/generators/templates/controller/app/views/%file_name%/index.html.haml", "lib/generators/templates/layout/app/views/layout/%file_name%.html.haml", "lib/generators/templates/resource_controller/activerecord/app/views/%file_name%/edit.html.haml", "lib/generators/templates/resource_controller/activerecord/app/views/%file_name%/index.html.haml", "lib/generators/templates/resource_controller/activerecord/app/views/%file_name%/new.html.haml", "lib/generators/templates/resource_controller/activerecord/app/views/%file_name%/show.html.haml", "lib/generators/templates/resource_controller/datamapper/app/views/%file_name%/edit.html.haml", "lib/generators/templates/resource_controller/datamapper/app/views/%file_name%/index.html.haml", "lib/generators/templates/resource_controller/datamapper/app/views/%file_name%/new.html.haml", "lib/generators/templates/resource_controller/datamapper/app/views/%file_name%/show.html.haml", "lib/generators/templates/resource_controller/none/app/views/%file_name%/edit.html.haml", "lib/generators/templates/resource_controller/none/app/views/%file_name%/index.html.haml", "lib/generators/templates/resource_controller/none/app/views/%file_name%/new.html.haml", "lib/generators/templates/resource_controller/none/app/views/%file_name%/show.html.haml", "lib/generators/templates/resource_controller/sequel/app/views/%file_name%/edit.html.haml", "lib/generators/templates/resource_controller/sequel/app/views/%file_name%/index.html.haml", "lib/generators/templates/resource_controller/sequel/app/views/%file_name%/new.html.haml", "lib/generators/templates/resource_controller/sequel/app/views/%file_name%/show.html.haml", "lib/merb-haml/merbtasks.rb", "lib/merb-haml/template.rb", "lib/merb-haml.rb"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb plugin that provides HAML support}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, ["= 1.0.12"])
      s.add_runtime_dependency(%q<haml>, [">= 2.0.3"])
    else
      s.add_dependency(%q<merb-core>, ["= 1.0.12"])
      s.add_dependency(%q<haml>, [">= 2.0.3"])
    end
  else
    s.add_dependency(%q<merb-core>, ["= 1.0.12"])
    s.add_dependency(%q<haml>, [">= 2.0.3"])
  end
end
