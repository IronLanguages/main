# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{RubyInline}
  s.version = "3.8.1"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis"]
  s.date = %q{2008-10-24}
  s.description = %q{Inline allows you to write foreign code within your ruby code. It automatically determines if the code in question has changed and builds it only when necessary. The extensions are then automatically loaded into the class/module that defines it.  You can even write extra builders that will allow you to write inlined code in any language. Use Inline::C as a template and look at Module#inline for the required API.}
  s.email = ["ryand-ruby@zenspider.com"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = ["History.txt", "Manifest.txt", "README.txt", "Rakefile", "demo/fastmath.rb", "demo/hello.rb", "example.rb", "example2.rb", "lib/inline.rb", "test/test_inline.rb", "tutorial/example1.rb", "tutorial/example2.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://rubyforge.org/projects/rubyinline/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.requirements = ["A POSIX environment and a compiler for your language."]
  s.rubyforge_project = %q{rubyinline}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Inline allows you to write foreign code within your ruby code}
  s.test_files = ["test/test_inline.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<ZenTest>, [">= 0"])
      s.add_development_dependency(%q<hoe>, [">= 1.8.0"])
    else
      s.add_dependency(%q<ZenTest>, [">= 0"])
      s.add_dependency(%q<hoe>, [">= 1.8.0"])
    end
  else
    s.add_dependency(%q<ZenTest>, [">= 0"])
    s.add_dependency(%q<hoe>, [">= 1.8.0"])
  end
end
