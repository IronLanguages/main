# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{fxri}
  s.version = "0.3.6"

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.bindir = %q{.}
  s.cert_chain = nil
  s.date = %q{2006-12-17}
  s.default_executable = %q{fxri}
  s.description = %q{FxRi is an FXRuby interface to the RI documentation, with a search engine that allows for search-on-typing.}
  s.email = %q{markus.prinz@qsig.org}
  s.executables = ["fxri"]
  s.files = ["lib", "fxri", "fxri.gemspec", "fxri-0.3.6.tar.gz", "fxri.rb", "lib/FoxDisplayer.rb", "lib/icons", "lib/Recursive_Open_Struct.rb", "lib/RiManager.rb", "lib/Globals.rb", "lib/fxirb.rb", "lib/Packet_Item.rb", "lib/Search_Engine.rb", "lib/Empty_Text_Field_Handler.rb", "lib/FoxTextFormatter.rb", "lib/Packet_List.rb", "lib/Icon_Loader.rb", "lib/icons/module.png", "lib/icons/method.png", "lib/icons/class.png", "./fxri"]
  s.homepage = %q{http://rubyforge.org/projects/fxri/}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubyforge_project = %q{fxri}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Graphical interface to the RI documentation, with search engine.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<fxruby>, [">= 1.2.0"])
    else
      s.add_dependency(%q<fxruby>, [">= 1.2.0"])
    end
  else
    s.add_dependency(%q<fxruby>, [">= 1.2.0"])
  end
end
