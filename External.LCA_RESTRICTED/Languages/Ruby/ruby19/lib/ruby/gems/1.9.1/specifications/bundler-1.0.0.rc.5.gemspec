# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{bundler}
  s.version = "1.0.0.rc.5"

  s.required_rubygems_version = Gem::Requirement.new(">= 1.3.6") if s.respond_to? :required_rubygems_version=
  s.authors = ["Carl Lerche", "Yehuda Katz", "André Arko"]
  s.date = %q{2010-08-10}
  s.default_executable = %q{bundle}
  s.description = %q{Bundler manages an application's dependencies through its entire life, across many machines, systematically and repeatably}
  s.email = ["carlhuda@engineyard.com"]
  s.executables = ["bundle"]
  s.files = ["bin/bundle", "bin/bundle.compiled.rbc", "lib/bundler/capistrano.rb", "lib/bundler/cli.rb", "lib/bundler/cli.rbc", "lib/bundler/definition.rb", "lib/bundler/definition.rbc", "lib/bundler/dependency.rb", "lib/bundler/dependency.rbc", "lib/bundler/dsl.rb", "lib/bundler/dsl.rbc", "lib/bundler/environment.rb", "lib/bundler/environment.rbc", "lib/bundler/gem_helper.rb", "lib/bundler/graph.rb", "lib/bundler/index.rb", "lib/bundler/index.rbc", "lib/bundler/installer.rb", "lib/bundler/installer.rbc", "lib/bundler/lazy_specification.rb", "lib/bundler/lazy_specification.rbc", "lib/bundler/lockfile_parser.rb", "lib/bundler/lockfile_parser.rbc", "lib/bundler/remote_specification.rb", "lib/bundler/remote_specification.rbc", "lib/bundler/resolver.rb", "lib/bundler/resolver.rbc", "lib/bundler/rubygems_ext.rb", "lib/bundler/rubygems_ext.rbc", "lib/bundler/runtime.rb", "lib/bundler/runtime.rbc", "lib/bundler/settings.rb", "lib/bundler/settings.rbc", "lib/bundler/setup.rb", "lib/bundler/shared_helpers.rb", "lib/bundler/shared_helpers.rbc", "lib/bundler/source.rb", "lib/bundler/source.rbc", "lib/bundler/spec_set.rb", "lib/bundler/spec_set.rbc", "lib/bundler/templates/Executable", "lib/bundler/templates/Gemfile", "lib/bundler/templates/newgem/Gemfile.tt", "lib/bundler/templates/newgem/gitignore.tt", "lib/bundler/templates/newgem/lib/newgem/version.rb.tt", "lib/bundler/templates/newgem/lib/newgem.rb.tt", "lib/bundler/templates/newgem/newgem.gemspec.tt", "lib/bundler/templates/newgem/Rakefile.tt", "lib/bundler/ui.rb", "lib/bundler/ui.rbc", "lib/bundler/vendor/thor/actions/create_file.rb", "lib/bundler/vendor/thor/actions/create_file.rbc", "lib/bundler/vendor/thor/actions/directory.rb", "lib/bundler/vendor/thor/actions/directory.rbc", "lib/bundler/vendor/thor/actions/empty_directory.rb", "lib/bundler/vendor/thor/actions/empty_directory.rbc", "lib/bundler/vendor/thor/actions/file_manipulation.rb", "lib/bundler/vendor/thor/actions/file_manipulation.rbc", "lib/bundler/vendor/thor/actions/inject_into_file.rb", "lib/bundler/vendor/thor/actions/inject_into_file.rbc", "lib/bundler/vendor/thor/actions.rb", "lib/bundler/vendor/thor/actions.rbc", "lib/bundler/vendor/thor/base.rb", "lib/bundler/vendor/thor/base.rbc", "lib/bundler/vendor/thor/core_ext/file_binary_read.rb", "lib/bundler/vendor/thor/core_ext/file_binary_read.rbc", "lib/bundler/vendor/thor/core_ext/hash_with_indifferent_access.rb", "lib/bundler/vendor/thor/core_ext/hash_with_indifferent_access.rbc", "lib/bundler/vendor/thor/core_ext/ordered_hash.rb", "lib/bundler/vendor/thor/core_ext/ordered_hash.rbc", "lib/bundler/vendor/thor/error.rb", "lib/bundler/vendor/thor/error.rbc", "lib/bundler/vendor/thor/invocation.rb", "lib/bundler/vendor/thor/invocation.rbc", "lib/bundler/vendor/thor/parser/argument.rb", "lib/bundler/vendor/thor/parser/argument.rbc", "lib/bundler/vendor/thor/parser/arguments.rb", "lib/bundler/vendor/thor/parser/arguments.rbc", "lib/bundler/vendor/thor/parser/option.rb", "lib/bundler/vendor/thor/parser/option.rbc", "lib/bundler/vendor/thor/parser/options.rb", "lib/bundler/vendor/thor/parser/options.rbc", "lib/bundler/vendor/thor/parser.rb", "lib/bundler/vendor/thor/parser.rbc", "lib/bundler/vendor/thor/shell/basic.rb", "lib/bundler/vendor/thor/shell/basic.rbc", "lib/bundler/vendor/thor/shell/color.rb", "lib/bundler/vendor/thor/shell/color.rbc", "lib/bundler/vendor/thor/shell/html.rb", "lib/bundler/vendor/thor/shell.rb", "lib/bundler/vendor/thor/shell.rbc", "lib/bundler/vendor/thor/task.rb", "lib/bundler/vendor/thor/task.rbc", "lib/bundler/vendor/thor/util.rb", "lib/bundler/vendor/thor/util.rbc", "lib/bundler/vendor/thor/version.rb", "lib/bundler/vendor/thor.rb", "lib/bundler/vendor/thor.rbc", "lib/bundler/version.rb", "lib/bundler/version.rbc", "lib/bundler.rb", "lib/bundler.rbc", "LICENSE", "README.md", "ROADMAP.md", "CHANGELOG.md", "TODO.md"]
  s.homepage = %q{http://gembundler.com}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{bundler}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{The best way to manage your application's dependencies}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<rspec>, [">= 0"])
    else
      s.add_dependency(%q<rspec>, [">= 0"])
    end
  else
    s.add_dependency(%q<rspec>, [">= 0"])
  end
end
