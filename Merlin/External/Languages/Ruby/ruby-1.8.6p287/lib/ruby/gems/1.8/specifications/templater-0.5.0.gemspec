# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{templater}
  s.version = "0.5.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Jonas Nicklas, Michael Klishin"]
  s.autorequire = %q{templater}
  s.date = %q{2008-11-24}
  s.description = %q{File generation system}
  s.email = %q{jonas.nicklas@gmail.com, michael.s.klishin@gmail.com}
  s.extra_rdoc_files = ["README", "LICENSE", "ROADMAP"]
  s.files = ["LICENSE", "README", "Rakefile", "ROADMAP", "lib/templater", "lib/templater/actions", "lib/templater/actions/action.rb", "lib/templater/actions/directory.rb", "lib/templater/actions/empty_directory.rb", "lib/templater/actions/evaluation.rb", "lib/templater/actions/file.rb", "lib/templater/actions/template.rb", "lib/templater/capture_helpers.rb", "lib/templater/cli", "lib/templater/cli/generator.rb", "lib/templater/cli/manifold.rb", "lib/templater/cli/parser.rb", "lib/templater/core_ext", "lib/templater/core_ext/kernel.rb", "lib/templater/core_ext/string.rb", "lib/templater/description.rb", "lib/templater/discovery.rb", "lib/templater/generator.rb", "lib/templater/manifold.rb", "lib/templater/spec", "lib/templater/spec/helpers.rb", "lib/templater.rb", "spec/actions", "spec/actions/custom_action_spec.rb", "spec/actions/directory_spec.rb", "spec/actions/empty_directory_spec.rb", "spec/actions/evaluation_spec.rb", "spec/actions/file_spec.rb", "spec/actions/template_spec.rb", "spec/core_ext", "spec/core_ext/string_spec.rb", "spec/generator", "spec/generator/actions_spec.rb", "spec/generator/arguments_spec.rb", "spec/generator/desc_spec.rb", "spec/generator/destination_root_spec.rb", "spec/generator/empty_directories_spec.rb", "spec/generator/files_spec.rb", "spec/generator/generators_spec.rb", "spec/generator/glob_spec.rb", "spec/generator/invocations_spec.rb", "spec/generator/invoke_spec.rb", "spec/generator/options_spec.rb", "spec/generator/render_spec.rb", "spec/generator/source_root_spec.rb", "spec/generator/templates_spec.rb", "spec/manifold_spec.rb", "spec/options_parser_spec.rb", "spec/results", "spec/results/erb.rbs", "spec/results/file.rbs", "spec/results/random.rbs", "spec/results/simple_erb.rbs", "spec/spec_helper.rb", "spec/spec_helpers_spec.rb", "spec/templater_spec.rb", "spec/templates", "spec/templates/erb.rbt", "spec/templates/glob", "spec/templates/glob/arg.js", "spec/templates/glob/hellothar.%feh%", "spec/templates/glob/hellothar.html.%feh%", "spec/templates/glob/README", "spec/templates/glob/subfolder", "spec/templates/glob/subfolder/jessica_alba.jpg", "spec/templates/glob/subfolder/monkey.rb", "spec/templates/glob/test.rb", "spec/templates/literals_erb.rbt", "spec/templates/simple.rbt", "spec/templates/simple_erb.rbt"]
  s.has_rdoc = true
  s.homepage = %q{http://templater.rubyforge.org/}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{File generation system}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<highline>, [">= 1.4.0"])
      s.add_runtime_dependency(%q<diff-lcs>, [">= 1.1.2"])
      s.add_runtime_dependency(%q<extlib>, [">= 0.9.5"])
    else
      s.add_dependency(%q<highline>, [">= 1.4.0"])
      s.add_dependency(%q<diff-lcs>, [">= 1.1.2"])
      s.add_dependency(%q<extlib>, [">= 0.9.5"])
    end
  else
    s.add_dependency(%q<highline>, [">= 1.4.0"])
    s.add_dependency(%q<diff-lcs>, [">= 1.1.2"])
    s.add_dependency(%q<extlib>, [">= 0.9.5"])
  end
end
