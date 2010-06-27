# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{merb-action-args}
  s.version = "1.0.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Yehuda Katz"]
  s.date = %q{2009-06-30}
  s.description = %q{Merb plugin that provides support for ActionArgs}
  s.email = %q{ykatz@engineyard.com}
  s.extra_rdoc_files = ["README", "LICENSE", "TODO"]
  s.files = ["LICENSE", "README", "Rakefile", "TODO", "lib/merb-action-args/abstract_controller.rb", "lib/merb-action-args/get_args.rb", "lib/merb-action-args/jruby_args.rb", "lib/merb-action-args.rb", "spec/action_args_spec.rb", "spec/controllers/action-args.rb", "spec/spec_helper.rb"]
  s.homepage = %q{http://merbivore.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{merb}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Merb plugin that provides support for ActionArgs}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_runtime_dependency(%q<ruby2ruby>, [">= 1.1.9"])
      s.add_runtime_dependency(%q<ParseTree>, [">= 2.1.1"])
    else
      s.add_dependency(%q<merb-core>, [">= 1.0.12"])
      s.add_dependency(%q<ruby2ruby>, [">= 1.1.9"])
      s.add_dependency(%q<ParseTree>, [">= 2.1.1"])
    end
  else
    s.add_dependency(%q<merb-core>, [">= 1.0.12"])
    s.add_dependency(%q<ruby2ruby>, [">= 1.1.9"])
    s.add_dependency(%q<ParseTree>, [">= 2.1.1"])
  end
end
