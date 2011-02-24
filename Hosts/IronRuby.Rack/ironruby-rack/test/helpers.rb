# IronRuby.Rack test helpers

include System::IO

# random temporary path where test applications will be generated
TestApp = Path.combine Path.get_temp_path, "ironruby-rack-tests-#{Path.get_random_file_name}"

if File.exist? TestApp
  begin
    require 'fileutils'
    FileUtils.rm_r TestApp
  rescue Errno::EACCES
    $stderr.puts "ERROR: The temporary directory \"#{TestApp}\" is being used by another process (most likely Cassini.exe)."
    exit(1)
  end
end

require 'rubygems'

# RackPath is the path to whatever version of Rack the tests should run against.
RackPath = if ENV['DLR_ROOT']
  # This is a dev environment, see http://wiki.github.com/ironruby/ironruby/contributing
  
  # Since this is a dev environment, update the IronRuby.Rack binaries first.
  require 'rake'
  load File.dirname(__FILE__) + '/../Rakefile'
  Rake::Task['update-bin'].invoke begin
    # default build configuration should be whatever IronRuby build is running this,
    # or a .NET 4 debug build if running an released IronRuby version
    require 'rbconfig'
    build_config = RbConfig::CONFIG['bindir'].split('/').last
    build_config = 'Debug' if build_config == 'bin'
    build_config
  end
  
  # in a dev-environment, use a set version of Rack
  File.expand_path 'Languages/Ruby/Tests/Libraries/rack-1.1.0', ENV['DLR_ROOT']
else
  # in any other environment, use the path to the latest installed version of Rack
  Gem.find_files('rack.rb').first.split('/')[0..-3].join('/')
end
$:.unshift File.expand_path('test', RackPath)

# test-spec with patches for IronRuby
require 'test/ispec'

module IronRubyRackTest
  
  module Helpers
  
    def start_server(port, vpath, path, configru)
      app = Rack::Deploy::ASPNETApplication.new(path)
      app.config.dlr_debug = true
      app.generate
  
      generate_config_ru_for path, configru
    
      server = IronRubyRackTest::Helpers::CassiniLauncher.new
      server.start port, vpath, path
      
      raise "Server failed to start" unless server.running?
      
      server
    end

    def stop_server(server)
      path = server.path
      server.stop
      
      raise "Server still running" unless server.stopped?
      
      destroy_config_ru_for path
      
      require 'fileutils'
      FileUtils.rm_r path
    end

    def generate_config_ru_for(app_path, config_ru)
      File.open(File.join(app_path, "config.ru"), 'w') do |f|
        f.write config_ru
      end
    end
  
    def destroy_config_ru_for(app_path)
      path = File.join(app_path, "config.ru")
      File.delete(path) if ::File.exist? path
    end

    CassiniPath = File.expand_path('bin', "#{File.dirname(__FILE__)}/..").gsub('/', '\\')
    
    class CassiniOutOfProcLauncher
      def start port, vpath, path
        @port, @vpath, @path, @host = port, vpath, path.gsub('/', "\\"), 'localhost'

        @cassini = System::Diagnostics::Process.new
        @cassini.StartInfo.use_shell_execute = true
        @cassini.start_info.arguments = "#{@path} #{@port} #{@vpath}"
        @cassini.StartInfo.file_name = File.join(CassiniPath, "Cassini.exe")
        @cassini.StartInfo.create_no_window = false
        @cassini.start_info.window_style = System::Diagnostics::ProcessWindowStyle.hidden
        @cassini.start

        trap(:INT) { stop }
        at_exit { stop }
      end
      
      def stop
        if @cassini
          @cassini.kill
          @cassini = @port = @vpath = @path = @host = nil
        end
      end
      
      def running?
        process_running? && processing_requests?
      end
      
      def stopped?
        process_stopped?
      end
      
      private
        
        def process_stopped?
          10.times do |i|
            if (@cassini.has_exited rescue true) && shell_stopped?
              @pid = nil
              return true
            end
            sleep 0.1
          end
          return false
        end
        
        def process_running?
          done = false
          10.times do |i|
            unless (@cassini.has_exited rescue false)
              @pid = @cassini.id
              return true if shell_running?
            end
            sleep 0.1
          end
          return false
        end
        
        def shell_running?
          @pid && `tasklist`.grep(/#{@pid}/).size > 0
        end
        
        def shell_stopped?
          @pid && (`tasklist`.grep(/#{@pid}/).size == 0)
        end
        
        def processing_requests?
          require 'net/http'
          Net::HTTP.start(@host, @port) { |http|
            res = http.get(@vpath)
            return res.code.to_i == 200
          }
          return false
        end
    end
    
    class CassiniInProcLauncher
      $:.unshift CassiniPath
      
      # FIXME:
      # >>> require 'bin/Cassini'
      # => true
      # >>> server = Cassini::Server.new(2061, '/', 'c:\temp\sin')
      # => Cassini.Server
      # >>> server.start
      # => nil
      # >>>
      # (GET http://localhost:2060/) =>
      # Unhandled Exception: System.InvalidCastException: Unable to cast transparent proxy to type 'Cassini.Host'.
      #    at Cassini.Server.GetHost()
      #    at Cassini.Server.<>c__DisplayClass2.<Start>b__1(Object )
      #    at System.Threading.QueueUserWorkItemCallback.WaitCallback_Context(Object state)
      #    at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean ignoreSyncCtx)
      #    at System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
      #    at System.Threading.ThreadPoolWorkQueue.Dispatch()
      #    at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()

      def start port, vpath, path
        require 'Cassini'
        raise "Server already started" if @server
        @server = Cassini::Server.new(port, vpath, path)
        @server.start
        trap(:INT) { stop }
      end
    
      def stop
        if @server
          @server.stop 
          @server = nil
        end
      end
      
      def running?
        !@server.nil?
      end
      
      def stopped?
        !running?
      end
    end
    
    class CassiniLauncher
      attr_reader :launcher, :path
      DefaultLauncher = CassiniOutOfProcLauncher
      # DefaultLauncher = CassiniInProcLauncher

      def initialize
        @launcher = DefaultLauncher.new
      end

      def start port, vpath, path
        @launcher.start port, vpath, @path = path
      end
      
      def stop
        @launcher.stop
      end
      
      def running?
        @launcher.running?
      end
      
      def stopped?
        @launcher.stopped?
      end
    end
  end

end