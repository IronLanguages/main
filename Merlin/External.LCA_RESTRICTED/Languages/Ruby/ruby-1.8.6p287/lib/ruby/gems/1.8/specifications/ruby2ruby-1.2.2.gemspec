# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ruby2ruby}
  s.version = "1.2.2"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis"]
  s.date = %q{2009-01-20}
  s.default_executable = %q{r2r_show}
  s.description = %q{ruby2ruby provides a means of generating pure ruby code easily from ParseTree's Sexps. This makes making dynamic language processors much easier in ruby than ever before.}
  s.email = ["ryand-ruby@zenspider.com"]
  s.executables = ["r2r_show"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = [".autotest", "History.txt", "Manifest.txt", "README.txt", "Rakefile", "bin/r2r_show", "lib/ruby2ruby.rb", "test/test_ruby2ruby.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://seattlerb.rubyforge.org/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{seattlerb}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{ruby2ruby provides a means of generating pure ruby code easily from ParseTree's Sexps}
  s.test_files = ["test/test_ruby2ruby.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<ParseTree>, ["~> 3.0"])
      s.add_development_dependency(%q<hoe>, [">= 1.8.2"])
    else
      s.add_dependency(%q<ParseTree>, ["~> 3.0"])
      s.add_dependency(%q<hoe>, [">= 1.8.2"])
    end
  else
    s.add_dependency(%q<ParseTree>, ["~> 3.0"])
    s.add_dependency(%q<hoe>, [">= 1.8.2"])
  end
end
