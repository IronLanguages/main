require 'fileutils'

module Rack
  
  module Deploy

    # Given a directory, it initializes it to be a rack-enabled ASP.NET application
    class ASPNETApplication
    
      APP_TYPES = [:rails, :sinatra]
      FILES_TO_EMIT = ['web.config', 'config.ru']
      IRONRUBY_BINARIES = %W(
        IronRuby.dll
        IronRuby.Libraries.dll
        IronRuby.Libraries.Yaml.dll
        Microsoft.Dynamic.dll
        Microsoft.Scripting.dll
        Microsoft.Scripting.Metadata.dll
        ir.exe
      )
      IRONRUBY_BINARIES_CLR2 = %W(
        Microsoft.Scripting.Core.dll 
      )
      RACK_BINARIES = %W(
        IronRuby.Rack.dll
        Cassini.exe
      )
    
      def initialize(app_dir, app_type = nil)
        require 'erb'
        
        if app_type && !APP_TYPES.include?(app_type.to_sym)
          raise "Invalid application_type \"#{app_type}\": valid types are #{
            APP_TYPES.map{|i| "\"#{i}\""}.join(',')
          }"
        end

        FileUtils.mkdir_p app_dir unless ::File.exist? app_dir

        @app_dir = app_dir
        @template_dir = ::File.join ::File.dirname(__FILE__), "template"
        @bin_dir = ::File.join ::File.dirname(__FILE__), '..', '..', '..', 'bin'
        @app_type = app_type.to_s
        
        default_app_type_config
      end

      def generate
        write_templates
        write_binaries
        write_log
      end

      def config
        @config ||= ASPNETConfig.new
      end

    protected

      def default_app_type_config
        if @app_type == 'rails'
          config.rack_version = '=1.0.1'
        end
      end

      def write_templates
        templates.each do |file, template|
          path = ::File.join @app_dir, file
          ::File.open(path, 'w'){|f| f.write(template)} unless ::File.exist? path
        end
      end

      def write_binaries
        app_bin_dir = ::File.join @app_dir, 'bin' 
        FileUtils.mkdir app_bin_dir unless ::File.exist? app_bin_dir

        RACK_BINARIES.map{|i| "#{@bin_dir}/#{i}"}.each{|b| FileUtils.cp b, app_bin_dir }

        require 'rbconfig'
        IRONRUBY_BINARIES.each do |bin|
          FileUtils.cp ::File.join(RbConfig::CONFIG['bindir'], bin), app_bin_dir
        end

        write_ir_exe_config app_bin_dir

        IRONRUBY_BINARIES_CLR2.each do |bin|
          FileUtils.cp ::File.join(RbConfig::CONFIG['bindir'], bin), app_bin_dir
        end if config.target_framework.to_f < 4.0
      end

      def write_log
        log_dir = ::File.join @app_dir, 'log'
        FileUtils.mkdir log_dir unless ::File.exist? log_dir
      end

      def write_public
        public_dir = ::File.join(@app_dir, config.public_dir)
        FileUtils.mkdir public_dir unless ::File.exist? public_dir
      end
      
      def write_ir_exe_config(app_bin_dir)
        ::File.open("#{app_bin_dir}/ir.exe.config", 'w') do |f|
          f.write "<?xml version=\"1.0\"?><configuration>#{erb template('ir.exe.config')}</configuration>"
        end
      end

      def templates
        FILES_TO_EMIT.inject({}) do |h, file|
          h[file] = erb template(file)
          h
        end
      end

      def erb(file, state = nil)
        e = ERB.new(::File.open(file){|f| f.read})
        e.filename = file
        e.result(state || binding)
      end

      def template(file)
        _file = ::File.join(@template_dir, @app_type, "#{file}.erb")
        unless ::File.exist? _file
          _file = ::File.join(@template_dir, "#{file}.erb")
        end
        _file
      end
    end
  
    # Defines configuration options that will make their way into web.config
    class ASPNETConfig
      require 'rbconfig'
    
      # IronRuby's library path
      attr_writer :library_path
      
      def library_path
        @library_path ||= [RbConfig::CONFIG['topdir'], RbConfig::CONFIG['rubylibdir'], RbConfig::CONFIG['sitelibdir']].join(';')
      end
      
      # Enable CLR debugging -- for debugging C# IronRuby.Rack.dll code only
      attr_writer :clr_debug
      
      def clr_debug
        !@clr_debug.nil? && @clr_debug
      end
      
      # Enable DLR debugging -- for debugging your Ruby code with Visual Studio
      attr_writer :dlr_debug
      
      def dlr_debug
        if @dlr_debug.nil?
          IronRuby.configuration.debug_mode
        else
          @dlr_debug
        end
      end
      
      # Version of the .NET framework to run on (eg: "4.0")
      attr_writer :target_framework
      
      def target_framework
        @target_framework || System::Environment.version.to_s.split('.')[0..1].join('.')
      end
      
      # Your application's root directory, where config.ru is located (eg: ".")
      attr_writer :app_root
      
      def app_root
        @app_root || '.'
      end
      
      # Path and filename of rack-ironruby's logfile (eg: "log/rack-aspnet.log")
      attr_writer :log
      
      def log
        @log || 'log/ironruby-rack.log'
      end
      
      # Path to a non-default Gem repository
      attr_accessor :gem_path
      
      # Environment Rack should run in ("development" or "deployment")
      attr_accessor :rack_env
      
      # Rack version to use
      attr_accessor :rack_version
      
      # IronRuby's version
      attr_writer :ironruby_version
      
      def ironruby_version
        @ironruby_version || begin
          require ::File.dirname(__FILE__) + '/../../../bin/IronRuby.Rack'
          IronRubyRack::IronRubyEngine.init
          IronRubyRack::IronRubyEngine.context.class.iron_ruby_version_string
        end
      end
      
      # public-key token IronRuby uses
      attr_writer :public_key_token
      
      def public_key_token
        @public_key_token || System::Reflection::Assembly.get_executing_assembly.full_name.split('=').last
      end
      
      # name of the public directory, for serving static content
      attr_accessor :public_dir

    end
  end
  
end