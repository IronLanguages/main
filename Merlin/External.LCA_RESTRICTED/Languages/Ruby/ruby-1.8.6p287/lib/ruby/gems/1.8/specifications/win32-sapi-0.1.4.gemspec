# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{win32-sapi}
  s.version = "0.1.4"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.cert_chain = nil
  s.date = %q{2007-07-30}
  s.description = %q{An interface to the MS SAPI (Sound API) library.}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/win32/sapi5.rb", "CHANGES", "CVS", "examples", "lib", "MANIFEST", "Rakefile", "README", "test", "win32-sapi.gemspec", "test/CVS", "test/tc_sapi5.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{An interface to the MS SAPI (Sound API) library.}
  s.test_files = ["test/tc_sapi5.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
