# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{text-format}
  s.version = "1.0.0"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Austin Ziegler"]
  s.autorequire = %q{text/format}
  s.cert_chain = nil
  s.date = %q{2005-06-24}
  s.description = %q{Text::Format is provides the ability to nicely format fixed-width text with knowledge of the writeable space (number of columns), margins, and indentation settings. Text::Format can work with either TeX::Hyphen or Text::Hyphen to hyphenate words when formatting.}
  s.email = %q{austin@rubyforge.org}
  s.extra_rdoc_files = ["README", "Changelog", "Install"]
  s.files = ["Changelog", "Install", "lib", "metaconfig", "pre-setup.rb", "Rakefile", "README", "setup.rb", "tests", "ToDo", "lib/text", "lib/text/format", "lib/text/format.rb", "lib/text/format/alpha.rb", "lib/text/format/number.rb", "lib/text/format/roman.rb", "tests/tc_text_format.rb", "tests/testall.rb"]
  s.homepage = %q{http://rubyforge.org/projects/text-format}
  s.rdoc_options = ["--title", "Text::Format", "--main", "README", "--line-numbers"]
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubyforge_project = %q{text-format}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{Text::Format formats fixed-width text nicely.}
  s.test_files = ["tests/tc_text_format.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<text-hyphen>, ["~> 1.0.0"])
    else
      s.add_dependency(%q<text-hyphen>, ["~> 1.0.0"])
    end
  else
    s.add_dependency(%q<text-hyphen>, ["~> 1.0.0"])
  end
end
