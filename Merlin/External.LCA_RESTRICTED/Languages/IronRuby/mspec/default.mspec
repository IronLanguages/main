class MSpecScript
    # The default implementation to run the specs.
  # TODO: change this to rely on an environment variable
  if ENV['ROWAN_BIN']
    set :target, "#{ENV['MERLIN_ROOT']}\\Test\\Scripts\\ir.cmd"
  else
    set :target, "#{ENV['MERLIN_ROOT']}\\bin\\debug\\ir.exe"    
  end
  # config[:prefix] must be set before filtered is used
  set :prefix, "#{ENV['MERLIN_ROOT']}\\..\\External.LCA_RESTRICTED\\Languages\\IronRuby\\mspec\\rubyspec"
  
  set :core1, filtered("core","[a-i]")
  set :core2, filtered("core", "[j-z]")
  set :lang, [
    "language"
    ]
  set :cli, [
    "command_line"
    ]
  set :lib1, filtered("library", "[a-o]")
  set :lib2, filtered("library", "[p-z]")
  #.NET interop
  set :netinterop, [
    "..\\..\\..\\..\\..\\Main\\Languages\\Ruby\\Tests\\Interop"
    ]
  
  set :thread, [
    "core\\thread",
    "core\\threadgroup"
    ]

  #combination tasks
  set :core, get(:core1) + get(:core2)
  set :lib, get(:lib1) + get(:lib2)
  set :ci_files, get(:core) + get(:lang) + get(:cli) + get(:lib) + get(:netinterop)


  # The set of substitutions to transform a spec filename
  # into a tag filename.
  set :tags_patterns, [
                        [%r(rubyspec/), 'ironruby-tags/'],
                        [/interop\//i, 'interop/tags/'],
                        [/_spec.rb$/, '_tags.txt']
                      ]
end

