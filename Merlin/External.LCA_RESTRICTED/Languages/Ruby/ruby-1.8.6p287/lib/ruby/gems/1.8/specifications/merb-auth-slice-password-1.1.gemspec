# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-auth-slice-password}
  s.version = "1.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel Neighman"]
  s.date = %q{2009-02-16}
  s.description = %q{Merb Slice that provides UI for password strategy of merb-auth.}
  s.email = %q{has.sox@gmail.com}
  s.extra_rdoc_files = ["README.textile", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README.textile", "Rakefile", "TODO", "lib/merb-auth-slice-password.rb", "lib/merb-auth-slice-password", "lib/merb-auth-slice-password/spectasks.rb", "lib/merb-auth-slice-password/slicetasks.rb", "lib/merb-auth-slice-password/merbtasks.rb", "spec/merb-auth-slice-password_spec.rb", "spec/spec_helper.rb", "app/controllers", "app/controllers/application.rb", "app/controllers/exceptions.rb", "app/controllers/sessions.rb", "app/views", "app/views/exceptions", "app/views/exceptions/unauthenticated.html.erb", "app/views/layout", "app/views/layout/merb-auth-slice-password.html.erb", "app/helpers", "app/helpers/application_helper.rb", "public/stylesheets", "public/stylesheets/master.css", "public/javascripts", "public/javascripts/master.js", "stubs/app", "stubs/app/controllers", "stubs/app/controllers/sessions.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://merbivore.com/}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Merb Slice that provides UI for password strategy of merb-auth.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-slices>, [">= 1.1"])
      s.add_runtime_dependency(%q<merb-auth-core>, [">= 1.1"])
      s.add_runtime_dependency(%q<merb-auth-more>, [">= 1.1"])
    else
      s.add_dependency(%q<merb-slices>, [">= 1.1"])
      s.add_dependency(%q<merb-auth-core>, [">= 1.1"])
      s.add_dependency(%q<merb-auth-more>, [">= 1.1"])
    end
  else
    s.add_dependency(%q<merb-slices>, [">= 1.1"])
    s.add_dependency(%q<merb-auth-core>, [">= 1.1"])
    s.add_dependency(%q<merb-auth-more>, [">= 1.1"])
  end
end
