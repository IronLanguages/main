# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{text-hyphen}
  s.version = "1.0.0"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.autorequire = %q{text/hyphen}
  s.cert_chain = nil
  s.date = %q{2004-12-20}
  s.description = %q{}
  s.email = %q{text-hyphen@halostatue.ca}
  s.extra_rdoc_files = ["README", "LICENCE", "INSTALL", "ChangeLog"]
  s.files = ["bin", "Changelog", "INSTALL", "lib", "LICENCE", "Rakefile", "README", "tests", "bin/hyphen", "lib/text", "lib/text/hyphen", "lib/text/hyphen.rb", "lib/text/hyphen/language", "lib/text/hyphen/language.rb", "lib/text/hyphen/language/ca.rb", "lib/text/hyphen/language/cs.rb", "lib/text/hyphen/language/da.rb", "lib/text/hyphen/language/de1.rb", "lib/text/hyphen/language/de2.rb", "lib/text/hyphen/language/en_uk.rb", "lib/text/hyphen/language/en_us.rb", "lib/text/hyphen/language/es.rb", "lib/text/hyphen/language/et.rb", "lib/text/hyphen/language/eu.rb", "lib/text/hyphen/language/fi.rb", "lib/text/hyphen/language/fr.rb", "lib/text/hyphen/language/ga.rb", "lib/text/hyphen/language/hr.rb", "lib/text/hyphen/language/hsb.rb", "lib/text/hyphen/language/hu1.rb", "lib/text/hyphen/language/hu2.rb", "lib/text/hyphen/language/ia.rb", "lib/text/hyphen/language/id.rb", "lib/text/hyphen/language/is.rb", "lib/text/hyphen/language/it.rb", "lib/text/hyphen/language/la.rb", "lib/text/hyphen/language/mn.rb", "lib/text/hyphen/language/nl.rb", "lib/text/hyphen/language/no1.rb", "lib/text/hyphen/language/no2.rb", "lib/text/hyphen/language/pl.rb", "lib/text/hyphen/language/pt.rb", "lib/text/hyphen/language/sv.rb", "tests/tc_text_hyphen.rb", "ChangeLog"]
  s.homepage = %q{http://rubyforge.org/projects/text-format/}
  s.rdoc_options = ["--title", "Text::Hyphen", "--main", "README", "--line-numbers"]
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubyforge_project = %q{text-format}
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{Multilingual word hyphenation according to modified TeX hyphenation pattern files.}
  s.test_files = ["tests/tc_text_hyphen.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
