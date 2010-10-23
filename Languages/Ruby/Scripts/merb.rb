DLR_ROOT = ENV['DLR_ROOT']

# this is used in Gem.default_dir in
# C:\M4\dlr\External.LCA_RESTRICTED\Languages\Ruby\ruby19\lib\ruby\gems\1.8\gems\rubygems-update-1.2.0\lib\rubygems\defaults.rb
RUBY_ENGINE = 'ruby'

# load_gems_in loads gems:
# C:\M4\dlr\External.LCA_RESTRICTED\Languages\Ruby\ruby19\lib\ruby\site_ruby\1.8\rubygems\source_index.rb

CURRENT_DIR = Dir.pwd
MERB_APP_ROOT = DLR_ROOT + "/External.LCA_RESTRICTED/Languages/IronRuby/merb/test_app"
ENV['GEM_HOME'] = DLR_ROOT + "/External.LCA_RESTRICTED/Languages/Ruby/ruby19/lib/ruby/gems/1.9.1"

puts "Booting merb ..."

# `attrib -r %MERB_APP_ROOT%/log/merb.main.pid`

Dir.chdir MERB_APP_ROOT

def trace_requires
  puts 'Tracing requires'
  
  $REQUIRE_DEPTH = 0
  Kernel.module_eval do
    alias x_require require
    alias x_load load
  
    def require path
      $REQUIRE_DEPTH += 1
      puts "#{$REQUIRE_DEPTH}\t" + ('| ' * $REQUIRE_DEPTH) + "> #{path}"
      x_require path
    ensure
      $REQUIRE_DEPTH -= 1
    end
    
    def load path, *wrap
      $REQUIRE_DEPTH += 1
      puts "#{$REQUIRE_DEPTH}\t" + ('| ' * $REQUIRE_DEPTH) + "> #{path} (LOAD)"
      x_load path, *wrap
    ensure
      $REQUIRE_DEPTH -= 1
    end
  end
end

if ARGV.include? "-tr"
  ARGV.delete "-tr"
  
  trace_requires
end

if ARGV.include? "-tc"
  ARGV.delete "-tc"

  call_depth = 0
  
  set_trace_func proc { |op, file, line, method, b, cls|
    if op == "call"
      puts "#{call_depth}\t" + ('| ' * call_depth) + "> #{cls}::#{method} (#{line} in #{file.nil? ? nil : file.gsub('\\','/')})"
      call_depth += 1
    elsif op == "return"  
      call_depth -= 1
    end
  }
end

ARGV << '-a' 
ARGV << 'webrick'

require 'rbconfig'

if ARGV.include? "-tcx"
  ARGV.delete "-tcx"

  require 'thread'
  
  def trace_thread_calls id
    call_depth = 0
    tag_open = false
    trace_out = File.open("#{CURRENT_DIR}/trace_#{defined?(IRONRUBY_VERSION) ? 'ir' : 'mri'}_#{id}.xml", "a")
    
    Thread.current[:__tracekey__] = lambda { |op, file, line, method, cls|
      if op == "call"
        if tag_open
          trace_out.puts(">")
        end
        
        trace_out.print(('  ' * call_depth) + %Q{<c m="#{cls}::#{method.to_s.gsub('<', '{').gsub('>', '}')}" p="#{file.to_s.gsub('/','\\')}" l="#{line}"})
        tag_open = true
        call_depth += 1      
      elsif op == "return" and call_depth > 0 
        call_depth -= 1
        
        if tag_open
          trace_out.puts("/>")
          tag_open = false
        else
          trace_out.puts(('  ' * call_depth) + "</c>")
        end      
      end
      trace_out.flush
    }
    
    puts "tracing for #{id}: #{trace_out.path}"
  end
  
  class Thread
    $TRACE_THREAD_ID = 0
    TRACE_MUTEX = Mutex.new
    
    class << self
      alias __new new
    
      def new *a, &p
        __new *a do
          id = nil
          TRACE_MUTEX.synchronize { $TRACE_THREAD_ID += 1; id = $TRACE_THREAD_ID }
        
          trace_thread_calls id
          p.call
        end
      end
    end  
  end
  
  trace_thread_calls "main"
  
  set_trace_func proc { |op, file, line, method, _, cls|
    if op == "call" or op == "return"
       t = Thread.current[:__tracekey__]
       t[op, file, line, method, cls] unless t.nil?
    end
  }
end

# make Hash.each sorted so that traces match
class Hash
  alias __eachXXX each

  def each &p
    entries = []
    __eachXXX { |k,v| entries << [k, v]; }
    
    entries.sort! { |x,y| 
      x[0].inspect <=> y[0].inspect
    }
    
    entries.each &p
    self
  end
  
  def each_value &p
    each { |k,v| p[v] }
  end
  
  def each_key &p
    each { |k,v| p[k] }
  end
end

load DLR_ROOT + "/External.LCA_RESTRICTED/Languages/Ruby/ruby19/bin/merb"
