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
if engine == 'ironruby'
  $" << "resolv.rb"
end
class MSpecScript
  if RUBY_PLATFORM =~ /(mswin|mingw|bccwin|wince)/i
    ir_cmd = 'ir.cmd'
  else
    ir_cmd = 'ir.sh'
  end
  # The default implementation to run the specs.
  set :target, File.join(ENV['DLR_ROOT'], "Test","Scripts","ir.cmd")
  # config[:prefix] must be set before filtered is used
  set :prefix, File.join(ENV['DLR_ROOT'], "External.LCA_RESTRICTED","Languages","IronRuby","mspec","rubyspec")
  
  set :core1sub1,filtered("core","[ac-i]")
  set :core1sub2,[ #want to keep basicobject out of the 1.8 list
    File.join("core","bignum"),
    File.join("core","binding"),
    File.join("core","builtin_constants")
  ]
  set :core2, filtered("core", "[j-z]").reject{|el| el =~ /thread/i}
  set :lang, [
    "language"
    ]
  set :cli, [
    "command_line"
    ]
  set :lib1, filtered("library", "[a-o]").reject {|el| el =~ /basicobject/ }
  set :lib2, filtered("library", "[p-z]").reject {|el| el =~ /prime/}
  #.NET interop
  set :netinterop, [
    File.join("..","..","..","..","..","Main","Languages","Ruby","Tests","Interop","net")
    ]
  
  set :netcli, [
    File.join("..","..","..","..","..","Main","Languages","Ruby","Tests","Interop","cli")
    ]

  set :cominterop, [
    File.join("..","..","..","..","..","Main","Languages","Ruby","Tests","Interop","com")
    ]
  
  set :thread, [
    File.join("core","thread"),
    File.join("core","threadgroup")
    ]

  #combination tasks
  set :core1, get(:core1sub1) + get(:core1sub2)
  set :core, get(:core1) + get(:core2)
  set :lib, get(:lib1) + get(:lib2)
  set :interop, get(:netinterop) + get(:cominterop)
  set :ci_files, get(:core) + get(:lang) + get(:cli) + get(:lib) + get(:interop)


  # The set of substitutions to transform a spec filename
  # into a tag filename.
  set :tags_patterns, [
                        [%r(rubyspec/), 'ironruby-tags/'],
                        [/interop\//i, 'interop/tags/'],
                        [/_spec.rb$/, '_tags.txt']
                      ]
end

