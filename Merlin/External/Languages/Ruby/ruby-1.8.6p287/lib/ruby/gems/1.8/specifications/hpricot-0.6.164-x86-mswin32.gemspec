# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{hpricot}
  s.version = "0.6.164"
  s.platform = %q{mswin32}

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["why the lucky stiff"]
  s.date = %q{2008-10-30}
  s.description = %q{a swift, liberal HTML parser with a fantastic library}
  s.email = %q{why@ruby-lang.org}
  s.extra_rdoc_files = ["README", "CHANGELOG", "COPYING"]
  s.files = ["CHANGELOG", "COPYING", "README", "Rakefile", "test/test_paths.rb", "test/test_preserved.rb", "test/test_parser.rb", "test/files", "test/files/cy0.html", "test/files/pace_application.html", "test/files/basic.xhtml", "test/files/utf8.html", "test/files/boingboing.html", "test/files/week9.html", "test/files/tenderlove.html", "test/files/immob.html", "test/files/why.xml", "test/files/uswebgen.html", "test/load_files.rb", "test/test_builder.rb", "test/test_xml.rb", "test/test_alter.rb", "lib/hpricot", "lib/hpricot/tags.rb", "lib/hpricot/builder.rb", "lib/hpricot/traverse.rb", "lib/hpricot/elements.rb", "lib/hpricot/modules.rb", "lib/hpricot/inspect.rb", "lib/hpricot/tag.rb", "lib/hpricot/blankslate.rb", "lib/hpricot/xchar.rb", "lib/hpricot/htmlinfo.rb", "lib/hpricot/parse.rb", "lib/hpricot.rb", "extras/mingw-rbconfig.rb", "ext/hpricot_scan/hpricot_scan.h", "ext/hpricot_scan/hpricot_gram.h", "ext/hpricot_scan/HpricotScanService.java", "ext/fast_xs/FastXsService.java", "ext/hpricot_scan/hpricot_scan.c", "ext/hpricot_scan/hpricot_gram.c", "ext/fast_xs/fast_xs.c", "ext/hpricot_scan/test.rb", "ext/hpricot_scan/extconf.rb", "ext/fast_xs/extconf.rb", "ext/hpricot_scan/hpricot_scan.rl", "ext/hpricot_scan/hpricot_scan.java.rl", "ext/hpricot_scan/hpricot_common.rl", "lib/hpricot_scan.so", "lib/fast_xs.so"]
  s.has_rdoc = true
  s.homepage = %q{http://code.whytheluckystiff.net/hpricot/}
  s.rdoc_options = ["--quiet", "--title", "The Hpricot Reference", "--main", "README", "--inline-source"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{hobix}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{a swift, liberal HTML parser with a fantastic library}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
