# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{facade}
  s.version = "1.0.2"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.cert_chain = nil
  s.date = %q{2007-06-10}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/facade.rb", "CHANGES", "MANIFEST", "Rakefile", "README", "test/tc_facade.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An easy way to implement the facade pattern in your class}
  s.test_files = ["test/tc_facade.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
