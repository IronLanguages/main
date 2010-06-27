# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-slices}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Fabien Franzen"]
  s.date = %q{2009-06-30}
  s.default_executable = %q{slice}
  s.description = %q{Merb-Slices is a Merb plugin for using and creating application 'slices' which help you modularize your application.}
  s.email = %q{info@fabien.be}
  s.executables = ["slice"]
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "Generators", "TODO", "lib/generators/base.rb", "lib/generators/full.rb", "lib/generators/templates/common/application.rb", "lib/generators/templates/common/lib/%base_name%/merbtasks.rb", "lib/generators/templates/common/lib/%base_name%/slicetasks.rb", "lib/generators/templates/common/lib/%base_name%/spectasks.rb", "lib/generators/templates/common/LICENSE", "lib/generators/templates/common/public/javascripts/master.js", "lib/generators/templates/common/public/stylesheets/master.css", "lib/generators/templates/common/Rakefile", "lib/generators/templates/common/TODO", "lib/generators/templates/full/app/controllers/application.rb", "lib/generators/templates/full/app/controllers/main.rb", "lib/generators/templates/full/app/helpers/application_helper.rb", "lib/generators/templates/full/app/views/layout/%symbol_name%.html.erb", "lib/generators/templates/full/app/views/main/index.html.erb", "lib/generators/templates/full/config/init.rb", "lib/generators/templates/full/config/router.rb", "lib/generators/templates/full/lib/%base_name%.rb", "lib/generators/templates/full/Rakefile", "lib/generators/templates/full/README", "lib/generators/templates/full/spec/%base_name%_spec.rb", "lib/generators/templates/full/spec/requests/main_spec.rb", "lib/generators/templates/full/spec/spec_helper.rb", "lib/generators/templates/full/stubs/app/controllers/application.rb", "lib/generators/templates/full/stubs/app/controllers/main.rb", "lib/generators/templates/full/TODO", "lib/generators/templates/thin/lib/%base_name%.rb", "lib/generators/templates/thin/README", "lib/generators/templates/thin/stubs/application.rb", "lib/generators/templates/thin/views/layout/%symbol_name%.html.erb", "lib/generators/templates/thin/views/main/index.html.erb", "lib/generators/templates/very_thin/lib/%base_name%.rb", "lib/generators/templates/very_thin/README", "lib/generators/thin.rb", "lib/generators/very_thin.rb", "lib/merb-slices/controller_mixin.rb", "lib/merb-slices/merbtasks.rb", "lib/merb-slices/module.rb", "lib/merb-slices/module_mixin.rb", "lib/merb-slices/router_ext.rb", "lib/merb-slices.rb", "spec/full_slice_generator_spec.rb", "spec/full_slice_spec.rb", "spec/merb-slice_spec.rb", "spec/slice_generator_spec.rb", "spec/slices/full-test-slice/app/controllers/application.rb", "spec/slices/full-test-slice/app/controllers/main.rb", "spec/slices/full-test-slice/app/helpers/application_helper.rb", "spec/slices/full-test-slice/app/views/layout/full_test_slice.html.erb", "spec/slices/full-test-slice/app/views/main/index.html.erb", "spec/slices/full-test-slice/config/init.rb", "spec/slices/full-test-slice/lib/full-test-slice/merbtasks.rb", "spec/slices/full-test-slice/lib/full-test-slice/slicetasks.rb", "spec/slices/full-test-slice/lib/full-test-slice/spectasks.rb", "spec/slices/full-test-slice/lib/full-test-slice.rb", "spec/slices/full-test-slice/LICENSE", "spec/slices/full-test-slice/public/javascripts/master.js", "spec/slices/full-test-slice/public/stylesheets/master.css", "spec/slices/full-test-slice/Rakefile", "spec/slices/full-test-slice/README", "spec/slices/full-test-slice/stubs/app/controllers/application.rb", "spec/slices/full-test-slice/stubs/app/controllers/main.rb", "spec/slices/full-test-slice/TODO", "spec/slices/thin-test-slice/application.rb", "spec/slices/thin-test-slice/lib/thin-test-slice/merbtasks.rb", "spec/slices/thin-test-slice/lib/thin-test-slice/slicetasks.rb", "spec/slices/thin-test-slice/lib/thin-test-slice.rb", "spec/slices/thin-test-slice/LICENSE", "spec/slices/thin-test-slice/public/javascripts/master.js", "spec/slices/thin-test-slice/public/stylesheets/master.css", "spec/slices/thin-test-slice/Rakefile", "spec/slices/thin-test-slice/README", "spec/slices/thin-test-slice/stubs/application.rb", "spec/slices/thin-test-slice/TODO", "spec/slices/thin-test-slice/views/layout/thin_test_slice.html.erb", "spec/slices/thin-test-slice/views/main/index.html.erb", "spec/slices/very-thin-test-slice/application.rb", "spec/slices/very-thin-test-slice/lib/very-thin-test-slice/merbtasks.rb", "spec/slices/very-thin-test-slice/lib/very-thin-test-slice/slicetasks.rb", "spec/slices/very-thin-test-slice/lib/very-thin-test-slice.rb", "spec/slices/very-thin-test-slice/LICENSE", "spec/slices/very-thin-test-slice/Rakefile", "spec/slices/very-thin-test-slice/README", "spec/slices/very-thin-test-slice/TODO", "spec/spec_helper.rb", "spec/thin_slice_generator_spec.rb", "spec/thin_slice_spec.rb", "spec/very_thin_slice_generator_spec.rb", "spec/very_thin_slice_spec.rb", "bin/slice"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb-Slices is a Merb plugin for using and creating application 'slices' which help you modularize your application.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.0.12"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.0.12"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.0.12"])
  end
end
