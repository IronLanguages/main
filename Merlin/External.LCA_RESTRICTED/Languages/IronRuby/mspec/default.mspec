#replace the Assert popup window with a message
engine = RUBY_ENGINE rescue 'notironruby'
if (ENV["THISISSNAP"] || ENV["SILENTASSERT"]) && engine == 'ironruby'
  class MyTraceListener < System::Diagnostics::DefaultTraceListener
    def fail(msg, detailMsg=nil)
      puts "ASSERT FAILED: #{msg}"
      puts "               #{detailMsg}" if detailMsg
    end
  end
  System::Diagnostics::Debug.Listeners.clear
  System::Diagnostics::Debug.Listeners.add(MyTraceListener.new)
end

class MSpecScript
  # The default implementation to run the specs.
  set :target, "#{ENV['MERLIN_ROOT']}\\Test\\Scripts\\ir.cmd"
  # config[:prefix] must be set before filtered is used
  set :prefix, "#{ENV['MERLIN_ROOT']}\\..\\External.LCA_RESTRICTED\\Languages\\IronRuby\\mspec\\rubyspec"
  
  set :core1sub1,filtered("core","[ac-i]")
  set :core1sub2,[ #want to keep basicobject out of the 1.8 list
    "core\\bignum",
    "core\\binding",
    "core\\builtin_constants"
  ]
  set :core2, filtered("core", "[j-z]")
  set :lang, [
    "language"
    ]
  set :cli, [
    "command_line"
    ]
  set :lib1, filtered("library", "[a-o]").reject {|el| el =~ /basicobject/ }
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
  set :core1, get(:core1sub1) + get(:core1sub2)
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

