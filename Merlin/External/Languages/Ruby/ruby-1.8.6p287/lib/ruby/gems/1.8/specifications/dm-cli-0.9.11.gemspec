# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-cli}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Wayne E. Seguin"]
  s.date = %q{2009-03-10}
  s.default_executable = %q{dm}
  s.description = %q{DataMapper plugin allowing interaction with models through a CLI}
  s.email = ["wayneeseguin [a] gmail [d] com"]
  s.executables = ["dm"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "bin/.irbrc", "bin/dm", "lib/dm-cli.rb", "lib/dm-cli/cli.rb", "lib/dm-cli/version.rb", "spec/spec.opts", "spec/unit/cli_spec.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-cli}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{DataMapper plugin allowing interaction with models through a CLI}

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
