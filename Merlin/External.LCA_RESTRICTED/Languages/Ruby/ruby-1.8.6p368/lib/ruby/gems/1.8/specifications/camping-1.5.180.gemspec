# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{camping}
  s.version = "1.5.180"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["why the lucky stiff"]
  s.cert_chain = nil
  s.date = %q{2007-01-05}
  s.default_executable = %q{camping}
  s.description = %q{minature rails for stay-at-home moms}
  s.email = %q{why@ruby-lang.org}
  s.executables = ["camping"]
  s.extra_rdoc_files = ["README", "CHANGELOG", "COPYING"]
  s.files = ["COPYING", "README", "Rakefile", "bin/camping", "doc/camping.1.gz", "test/test_xhtml_trans.rb", "lib/camping", "lib/camping.rb", "lib/camping-unabridged.rb", "lib/camping/reloader.rb", "lib/camping/fastcgi.rb", "lib/camping/session.rb", "lib/camping/db.rb", "lib/camping/webrick.rb", "extras/permalink.gif", "extras/Camping.gif", "extras/flipbook_rdoc.rb", "examples/campsh.rb", "examples/tepee.rb", "examples/blog.rb", "CHANGELOG"]
  s.homepage = %q{http://code.whytheluckystiff.net/camping/}
  s.rdoc_options = ["--quiet", "--title", "Camping, the Documentation", "--opname", "index.html", "--line-numbers", "--main", "README", "--inline-source", "--exclude", "^(examples|extras)\\/", "--exclude", "lib/camping.rb"]
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new(">= 1.8.2")
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{minature rails for stay-at-home moms}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, [">= 1.3.1"])
      s.add_runtime_dependency(%q<markaby>, [">= 0.5"])
      s.add_runtime_dependency(%q<metaid>, ["> 0.0.0"])
    else
      s.add_dependency(%q<activesupport>, [">= 1.3.1"])
      s.add_dependency(%q<markaby>, [">= 0.5"])
      s.add_dependency(%q<metaid>, ["> 0.0.0"])
    end
  else
    s.add_dependency(%q<activesupport>, [">= 1.3.1"])
    s.add_dependency(%q<markaby>, [">= 0.5"])
    s.add_dependency(%q<metaid>, ["> 0.0.0"])
  end
end
