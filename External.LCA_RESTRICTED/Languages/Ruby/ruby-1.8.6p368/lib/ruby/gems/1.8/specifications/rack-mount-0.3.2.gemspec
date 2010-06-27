# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{rack-mount}
  s.version = "0.3.2"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Joshua Peek"]
  s.date = %q{2009-12-04}
  s.description = %q{Stackable dynamic tree based Rack router}
  s.email = %q{josh@joshpeek.com}
  s.extra_rdoc_files = ["README.rdoc", "LICENSE"]
  s.files = ["lib/rack/mount/analysis/frequency.rb", "lib/rack/mount/analysis/histogram.rb", "lib/rack/mount/analysis/splitting.rb", "lib/rack/mount/exceptions.rb", "lib/rack/mount/generatable_regexp.rb", "lib/rack/mount/generation/route.rb", "lib/rack/mount/generation/route_set.rb", "lib/rack/mount/mixover.rb", "lib/rack/mount/multimap.rb", "lib/rack/mount/prefix.rb", "lib/rack/mount/recognition/code_generation.rb", "lib/rack/mount/recognition/route.rb", "lib/rack/mount/recognition/route_set.rb", "lib/rack/mount/regexp_with_named_groups.rb", "lib/rack/mount/route.rb", "lib/rack/mount/route_set.rb", "lib/rack/mount/strexp/parser.rb", "lib/rack/mount/strexp/tokenizer.rb", "lib/rack/mount/strexp.rb", "lib/rack/mount/utils.rb", "lib/rack/mount/vendor/multimap/multimap.rb", "lib/rack/mount/vendor/multimap/multiset.rb", "lib/rack/mount/vendor/multimap/nested_multimap.rb", "lib/rack/mount/vendor/reginald/reginald/alternation.rb", "lib/rack/mount/vendor/reginald/reginald/anchor.rb", "lib/rack/mount/vendor/reginald/reginald/atom.rb", "lib/rack/mount/vendor/reginald/reginald/character.rb", "lib/rack/mount/vendor/reginald/reginald/character_class.rb", "lib/rack/mount/vendor/reginald/reginald/collection.rb", "lib/rack/mount/vendor/reginald/reginald/expression.rb", "lib/rack/mount/vendor/reginald/reginald/group.rb", "lib/rack/mount/vendor/reginald/reginald/parser.rb", "lib/rack/mount/vendor/reginald/reginald/tokenizer.rb", "lib/rack/mount/vendor/reginald/reginald.rb", "lib/rack/mount.rb", "README.rdoc", "LICENSE"]
  s.homepage = %q{http://github.com/josh/rack-mount}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Stackable dynamic tree based Rack router}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<rack>, [">= 1.0.0"])
    else
      s.add_dependency(%q<rack>, [">= 1.0.0"])
    end
  else
    s.add_dependency(%q<rack>, [">= 1.0.0"])
  end
end
