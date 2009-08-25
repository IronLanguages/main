# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{metaid}
  s.version = "1.0"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["why the lucky stiff"]
  s.autorequire = %q{metaid}
  s.cert_chain = nil
  s.date = %q{2006-01-16}
  s.email = %q{why@ruby-lang.org}
  s.files = ["metaid.rb"]
  s.homepage = %q{http://whytheluckystiff.net/metaid/}
  s.require_paths = ["."]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubyforge_project = %q{hobix}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{slight metaprogramming helpers}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
