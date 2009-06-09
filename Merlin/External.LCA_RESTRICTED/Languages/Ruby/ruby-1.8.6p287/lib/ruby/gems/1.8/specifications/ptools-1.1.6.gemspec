# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ptools}
  s.version = "1.1.6"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.cert_chain = nil
  s.date = %q{2007-08-31}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/ptools.rb", "CHANGES", "MANIFEST", "README", "Rakefile", "test/tc_binary.rb", "test/tc_constants.rb", "test/tc_head.rb", "test/tc_image.rb", "test/tc_middle.rb", "test/tc_nlconvert.rb", "test/tc_null.rb", "test/tc_tail.rb", "test/tc_touch.rb", "test/tc_wc.rb", "test/tc_whereis.rb", "test/tc_which.rb", "test/test_file1.txt", "test/test_file2.txt", "test/ts_all.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Extra methods for the File class}
  s.test_files = ["test/ts_all.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
