class MSpecScript
  set :core1, [
    "core\\argf",
    "core\\array",
    "core\\bignum",
    "core\\binding",
    "core\\class",
    "core\\comparable",
    "core\\continuation",
    "core\\dir",
    "core\\enumerable",
    "core\\env",
    "core\\exception",
    "core\\false",
    "core\\file",
    "core\\filetest",
    "core\\fixnum",
    "core\\float",
    "core\\gc",
    "core\\hash",
    "core\\integer",
    "core\\io"
    ]
  set :core2, [
    "core\\kernel",
    "core\\marshal",
    "core\\matchdata",
    "core\\math",
    "core\\method",
    "core\\module",
    "core\\nil",
    "core\\numeric",
    "core\\object",
    "core\\objectspace",
    "core\\precision",
    "core\\proc",
    "core\\process",
    "core\\range",
    "core\\regexp",
    "core\\signal",
    "core\\string",
    "core\\struct",
    "core\\symbol",
    "core\\systemexit",
    "core\\thread",
    "core\\threadgroup",
    "core\\time",
    "core\\true",
    "core\\unboundmethod",
    ]
  set :lang, [
    "language"
    ]
  set :lib1, [
    "library\\abbrev",
    "library\\base64",
    "library\\bigdecimal",
    "library\\cgi",
    "library\\complex",
    "library\\csv",
    "library\\date",
    "library\\digest",
    "library\\drb",
    "library\\enumerator",
    "library\\erb",
    "library\\etc",
    "library\\ftools",
    "library\\generator",
    "library\\getoptlong",
    "library\\iconv",
    "library\\ipaddr",
    "library\\logger",
    "library\\mathn",
    "library\\matrix",
    "library\\net",
    "library\\observer",
    "library\\openssl",
    "library\\openstruct",
    ]
  set :lib2, [
    "library\\parsedate",
    "library\\pathname",
    "library\\ping",
    "library\\prime",
    "library\\rational",
    "library\\readline",
    "library\\resolv",
    "library\\rexml",
    "library\\scanf",
    "library\\securerandom",
    "library\\set",
    "library\\shellwords",
    "library\\singleton",
    "library\\socket",
    "library\\stringio",
    "library\\stringscanner",
    "library\\syslog",
    "library\\tempfile",
    "library\\time",
    "library\\timeout",
    "library\\tmpdir",
    "library\\uri",
    "library\\yaml",
    "library\\zlib",
    ]
  #.NET interop
  net_interop_root = "..\\..\\..\\..\\..\\Main\\Languages\\Ruby\\Tests\\Interop"
  set :derivation, [
    net_interop_root + "\\derivation"
    ]
  set :load, [
    net_interop_root + "\\load"
    ]
  set :mapping, [
    net_interop_root + "\\mapping"
    ]
  set :special, [
    net_interop_root + "\\special"
    ]
  set :using, [
    net_interop_root + "\\using"
    ]
  
  set :thread, [
    "core\\thread",
    "core\\threadgroup"
    ]

  #combination tasks
  set :netinterop, get(:derivation) + get(:load) + get(:mapping) + get(:special) + get(:using)
  set :core, get(:core1) + get(:core2)
  set :lib, get(:lib1) + get(:lib2)



  # The set of substitutions to transform a spec filename
  # into a tag filename.
  set :tags_patterns, [
                        [%r(rubyspec/), 'ironruby-tags/'],
                        [/_spec.rb$/, '_tags.txt']
                      ]
  # The default implementation to run the specs.
  # TODO: change this to rely on an environment variable
  set :target, "#{ENV['MERLIN_ROOT']}\\Test\\Scripts\\ir.cmd"
  set :prefix, "#{ENV['MERLIN_ROOT']}\\..\\External\\Languages\\IronRuby\\mspec\\rubyspec"
end

