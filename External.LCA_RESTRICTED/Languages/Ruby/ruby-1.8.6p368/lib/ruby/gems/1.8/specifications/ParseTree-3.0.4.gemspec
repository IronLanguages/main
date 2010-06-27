# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ParseTree}
  s.version = "3.0.4"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ryan Davis"]
  s.cert_chain = ["-----BEGIN CERTIFICATE-----\nMIIDPjCCAiagAwIBAgIBADANBgkqhkiG9w0BAQUFADBFMRMwEQYDVQQDDApyeWFu\nZC1ydWJ5MRkwFwYKCZImiZPyLGQBGRYJemVuc3BpZGVyMRMwEQYKCZImiZPyLGQB\nGRYDY29tMB4XDTA5MDMwNjE4NTMxNVoXDTEwMDMwNjE4NTMxNVowRTETMBEGA1UE\nAwwKcnlhbmQtcnVieTEZMBcGCgmSJomT8ixkARkWCXplbnNwaWRlcjETMBEGCgmS\nJomT8ixkARkWA2NvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALda\nb9DCgK+627gPJkB6XfjZ1itoOQvpqH1EXScSaba9/S2VF22VYQbXU1xQXL/WzCkx\ntaCPaLmfYIaFcHHCSY4hYDJijRQkLxPeB3xbOfzfLoBDbjvx5JxgJxUjmGa7xhcT\noOvjtt5P8+GSK9zLzxQP0gVLS/D0FmoE44XuDr3iQkVS2ujU5zZL84mMNqNB1znh\nGiadM9GHRaDiaxuX0cIUBj19T01mVE2iymf9I6bEsiayK/n6QujtyCbTWsAS9Rqt\nqhtV7HJxNKuPj/JFH0D2cswvzznE/a5FOYO68g+YCuFi5L8wZuuM8zzdwjrWHqSV\ngBEfoTEGr7Zii72cx+sCAwEAAaM5MDcwCQYDVR0TBAIwADALBgNVHQ8EBAMCBLAw\nHQYDVR0OBBYEFEfFe9md/r/tj/Wmwpy+MI8d9k/hMA0GCSqGSIb3DQEBBQUAA4IB\nAQAY59gYvDxqSqgC92nAP9P8dnGgfZgLxP237xS6XxFGJSghdz/nI6pusfCWKM8m\nvzjjH2wUMSSf3tNudQ3rCGLf2epkcU13/rguI88wO6MrE0wi4ZqLQX+eZQFskJb/\nw6x9W1ur8eR01s397LSMexySDBrJOh34cm2AlfKr/jokKCTwcM0OvVZnAutaovC0\nl1SVZ0ecg88bsWHA0Yhh7NFxK1utWoIhtB6AFC/+trM0FQEB/jZkIS8SaNzn96Rl\nn0sZEf77FLf5peR8TP/PtmIg7Cyqz23sLM4mCOoTGIy5OcZ8TdyiyINUHtb5ej/T\nFBHgymkyj/AOSqKRIpXPhjC6\n-----END CERTIFICATE-----\n"]
  s.date = %q{2009-06-23}
  s.description = %q{ParseTree is a C extension (using RubyInline) that extracts the parse
tree for an entire class or a specific method and returns it as a
s-expression (aka sexp) using ruby's arrays, strings, symbols, and
integers.

As an example:

  def conditional1(arg1)
    if arg1 == 0 then
      return 1
    end
    return 0
  end

becomes:

  [:defn,
    :conditional1,
    [:scope,
     [:block,
      [:args, :arg1],
      [:if,
       [:call, [:lvar, :arg1], :==, [:array, [:lit, 0]]],
       [:return, [:lit, 1]],
       nil],
      [:return, [:lit, 0]]]]]}
  s.email = ["ryand-ruby@zenspider.com"]
  s.executables = ["parse_tree_abc", "parse_tree_audit", "parse_tree_deps", "parse_tree_show"]
  s.extra_rdoc_files = ["History.txt", "Manifest.txt", "README.txt"]
  s.files = [".autotest", "History.txt", "Manifest.txt", "README.txt", "Rakefile", "bin/parse_tree_abc", "bin/parse_tree_audit", "bin/parse_tree_deps", "bin/parse_tree_show", "demo/printer.rb", "lib/gauntlet_parsetree.rb", "lib/parse_tree.rb", "lib/parse_tree_extensions.rb", "lib/unified_ruby.rb", "test/pt_testcase.rb", "test/something.rb", "test/test_parse_tree.rb", "test/test_parse_tree_extensions.rb", "test/test_unified_ruby.rb", "validate.sh"]
  s.homepage = %q{http://rubyforge.org/projects/parsetree/}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib", "test"]
  s.rubyforge_project = %q{parsetree}
  s.rubygems_version = %q{1.3.4}
  s.summary = %q{ParseTree is a C extension (using RubyInline) that extracts the parse tree for an entire class or a specific method and returns it as a s-expression (aka sexp) using ruby's arrays, strings, symbols, and integers}
  s.test_files = ["test/test_parse_tree.rb", "test/test_parse_tree_extensions.rb", "test/test_unified_ruby.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<RubyInline>, [">= 3.7.0"])
      s.add_runtime_dependency(%q<sexp_processor>, [">= 3.0.0"])
      s.add_development_dependency(%q<hoe>, [">= 2.3.0"])
    else
      s.add_dependency(%q<RubyInline>, [">= 3.7.0"])
      s.add_dependency(%q<sexp_processor>, [">= 3.0.0"])
      s.add_dependency(%q<hoe>, [">= 2.3.0"])
    end
  else
    s.add_dependency(%q<RubyInline>, [">= 3.7.0"])
    s.add_dependency(%q<sexp_processor>, [">= 3.0.0"])
    s.add_dependency(%q<hoe>, [">= 2.3.0"])
  end
end
