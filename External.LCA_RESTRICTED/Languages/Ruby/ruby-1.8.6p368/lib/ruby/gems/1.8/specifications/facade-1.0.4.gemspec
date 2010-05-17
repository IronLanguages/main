# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{facade}
  s.version = "1.0.4"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2009-08-02}
  s.description = %q{      The facade library allows you to mixin singleton methods from classes
      or modules as instance methods of the extending class.
}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["CHANGES", "facade.gemspec", "lib/facade.rb", "MANIFEST", "Rakefile", "README", "test/test_facade.rb"]
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.licenses = ["Artistic 2.0"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{shards}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{An easy way to implement the facade pattern in your class}
  s.test_files = ["test/test_facade.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
