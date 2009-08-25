# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-mailer}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Yehuda Katz"]
  s.date = %q{2009-06-30}
  s.description = %q{Merb plugin that provides mailer functionality to Merb}
  s.email = %q{ykatz@engineyard.com}
  s.extra_rdoc_files = ["README.textile", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README.textile", "Rakefile", "TODO", "Generators", "lib/generators/mailer_generator.rb", "lib/generators/templates/mailer/app/mailers/%file_name%_mailer.rb", "lib/generators/templates/mailer/app/mailers/views/%file_name%_mailer/notify_on_event.text.erb", "lib/generators/templates/mailer/spec/mailers/%file_name%_mailer_spec.rb", "lib/merb-mailer/mail_controller.rb", "lib/merb-mailer/mailer.rb", "lib/merb-mailer/mailer_mixin.rb", "lib/merb-mailer.rb", "spec/mail_controller_spec.rb", "spec/mailer_generator_spec.rb", "spec/mailer_spec.rb", "spec/mailers/views/layout/application.html.erb", "spec/mailers/views/layout/application.text.erb", "spec/mailers/views/test_mail_controller/eighth.html.erb", "spec/mailers/views/test_mail_controller/eighth.text.erb", "spec/mailers/views/test_mail_controller/first.html.erb", "spec/mailers/views/test_mail_controller/first.text.erb", "spec/mailers/views/test_mail_controller/generates_absolute_url.text.erb", "spec/mailers/views/test_mail_controller/generates_relative_url.text.erb", "spec/mailers/views/test_mail_controller/ninth.html.erb", "spec/mailers/views/test_mail_controller/ninth.text.erb", "spec/mailers/views/test_mail_controller/second.text.erb", "spec/mailers/views/test_mail_controller/third.html.erb", "spec/spec_helper.rb"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb plugin that provides mailer functionality to Merb}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_runtime_dependency(%q<mailfactory>, [">= 1.2.3"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_dependency(%q<mailfactory>, [">= 1.2.3"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.0.12"])
    s.add_dependency(%q<mailfactory>, [">= 1.2.3"])
  end
end
