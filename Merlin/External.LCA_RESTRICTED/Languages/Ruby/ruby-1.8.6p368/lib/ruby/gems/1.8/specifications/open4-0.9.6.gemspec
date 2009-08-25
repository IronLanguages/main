# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{open4}
  s.version = "0.9.6"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Ara T. Howard"]
  s.autorequire = %q{open4}
  s.cert_chain = nil
  s.date = %q{2007-07-17}
  s.email = %q{ara.t.howard@noaa.gov}
  s.files = ["gemspec.rb", "install.rb", "lib", "lib/open4.rb", "README", "sample", "sample/bg.rb", "sample/block.rb", "sample/exception.rb", "sample/simple.rb", "sample/spawn.rb", "sample/stdin_timeout.rb", "sample/timeout.rb", "white_box", "white_box/leak.rb"]
  s.homepage = %q{http://codeforpeople.com/lib/ruby/open4/}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{open4}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
