Gem::Specification.new do |s|
  s.name = "github"
  s.version = "0.3.4"

  s.specification_version = 2 if s.respond_to? :specification_version=

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Chris Wanstrath, Kevin Ballard, Scott Chacon"]
  s.date = %q{2008-05-18}
  s.default_executable = %q{gh}
  s.description = %q{The official `github` command line helper for simplifying your GitHub experience.}
  s.email = %q{chris@ozmm.org}
  s.executables = ["github", "gh"]
  s.extra_rdoc_files = ["bin/github", "bin/gh", "lib/github/extensions.rb", "lib/github/command.rb", "lib/github/helper.rb", "lib/github.rb", "LICENSE", "README"]
  s.files = ["bin/github", "lib/commands/network.rb", "lib/commands/commands.rb", "lib/commands/helpers.rb", "lib/github/extensions.rb", "lib/github/command.rb", "lib/github/helper.rb", "lib/github.rb", "LICENSE", "Manifest", "README", "spec/command_spec.rb", "spec/extensions_spec.rb", "spec/github_spec.rb", "spec/helper_spec.rb", "spec/spec_helper.rb", "spec/ui_spec.rb", "spec/windoze_spec.rb", "github-gem.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/}
  s.rdoc_options = ["--line-numbers", "--inline-source", "--title", "Github", "--main", "README"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{github}
  s.rubygems_version = %q{1.1.1}
  s.summary = %q{The official `github` command line helper for simplifying your GitHub experience.}

  # s.add_dependency(%q<launchy>, [">= 0"])
  s.add_dependency('json_pure', [">= 0"])
end
