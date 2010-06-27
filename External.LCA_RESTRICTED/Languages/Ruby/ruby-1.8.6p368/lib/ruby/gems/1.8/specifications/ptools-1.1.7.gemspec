# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ptools}
  s.version = "1.1.7"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger"]
  s.date = %q{2009-07-27}
  s.description = %q{      The ptools (power tools) library provides several handy methods to
      Ruby's core File class, such as File.which for finding executables,
      File.null to return the null device on your platform, and so on.
}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["README", "CHANGES", "MANIFEST"]
  s.files = ["lib/ptools.rb", "test/test_binary.rb", "test/test_constants.rb", "test/test_file1.txt", "test/test_file2.txt", "test/test_head.rb", "test/test_image.rb", "test/test_middle.rb", "test/test_nlconvert.rb", "test/test_null.rb", "test/test_tail.rb", "test/test_touch.rb", "test/test_wc.rb", "test/test_whereis.rb", "test/test_which.rb", "README", "CHANGES", "MANIFEST"]
  s.homepage = %q{http://www.rubyforge.org/projects/shards}
  s.licenses = ["Artistic 2.0"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{shards}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{Extra methods for the File class}
  s.test_files = ["test/test_binary.rb", "test/test_constants.rb", "test/test_file1.txt", "test/test_file2.txt", "test/test_head.rb", "test/test_image.rb", "test/test_middle.rb", "test/test_nlconvert.rb", "test/test_null.rb", "test/test_tail.rb", "test/test_touch.rb", "test/test_wc.rb", "test/test_whereis.rb", "test/test_which.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<test-unit>, [">= 2.0.3"])
    else
      s.add_dependency(%q<test-unit>, [">= 2.0.3"])
    end
  else
    s.add_dependency(%q<test-unit>, [">= 2.0.3"])
  end
end
