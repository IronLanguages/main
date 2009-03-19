# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-sound}
  s.version = "0.4.1"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.cert_chain = nil
  s.date = %q{2007-07-27}
  s.description = %q{A library for playing with sound on MS Windows.}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["CHANGES", "README", "MANIFEST"]
  s.files = ["examples/sound_test.rb", "lib/win32/sound.rb", "test/tc_sound.rb", "CHANGES", "examples", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-sound.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{A library for playing with sound on MS Windows.}
  s.test_files = ["test/tc_sound.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
