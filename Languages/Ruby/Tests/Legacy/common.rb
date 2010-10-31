# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

module TestPath
  def TestPath.get_environment_variable(x)
    ENV[x.to_s]
  end 
  
  def TestPath.get_directory_name(dir)
    File.basename dir
  end 
  
  def TestPath.get_directory(dir)
    File.dirname dir
  end 
  
  if get_environment_variable('DLR_ROOT')
    DLR_ROOT   = get_environment_variable('DLR_ROOT')
    TEST_DIR    = DLR_ROOT + "/Languages/Ruby/Tests"
    CORECLR_ROOT  = DLR_ROOT + "/Util/Internal/Silverlight/x86ret"
    CRUBY_EXE     = get_environment_variable('RUBY18_EXE')
    
    DLR_BIN     = get_environment_variable('DLR_BIN')

    # assume we are running inside snap
    if DLR_BIN
      IRUBY_EXE     = DLR_ROOT + "/Test/Scripts/ir.cmd"
      IPYTHON_EXE   = DLR_BIN + "/ipy.exe"
    else
      ir_cmd      = DLR_ROOT + "/Test/Scripts/ir.cmd"
      if File.exists? ir_cmd
        IRUBY_EXE   = ir_cmd
      else
        IRUBY_EXE   = DLR_ROOT + "/bin/debug/ir.exe" # ir.cmd does not exist in GIT
      end
      IPYTHON_EXE   = DLR_ROOT + "/bin/debug/ipy.exe"
    end
        
    PARSEONLY_EXE   = IPYTHON_EXE + " " + TEST_DIR + "/Tools/parseonly.py " 
  else
    TEST_DIR    = File.expand_path(File.dirname(__FILE__))
    DLR_ROOT   = File.dirname(File.dirname(TEST_DIR))
    IRUBY_EXE     = DLR_ROOT + "/Test/Scripts/ir.cmd"
    CRUBY_EXE     = "ruby.exe"
    
    IPYTHON_EXE   = nil
    PARSEONLY_EXE   = nil    
  end
  
  # generate logs to TestResults directory, so they will be copied to the snap server
  TEMP = get_environment_variable("INSTALLROOT")
  RESULT_DIR = TEMP ? File.join(TEMP, "TestResults") : TEST_DIR
end 

module Test
  class Logger
    attr_reader :log_name
    
    def initialize(prefix)
      current = Time.now.strftime("%m%d%H%M")
      @log_name = File.join(TestPath::RESULT_DIR, "#{prefix}_#{current}.log")
    end 
    
    def append(*lines)
      lines.each do |line|
        open(@log_name, "a+") { |f| f << line << "\n" }
      end
    end 
    
    def to_s
      @log_name
    end 
  end 
  
  
  class BaseDriver
    attr_reader :name, :logger, :redirect_error, :append_to_log
    
    def initialize(name, redirect_error=true, append_to_log=true)
      @name = name.downcase
      @logger = Logger.new(name)
      
      @redirect_error = redirect_error
      @append_to_log = append_to_log
      @cmd_line = nil 
    end 
    
    def run(f, log_file)
      saved = Dir.pwd
      Dir.chdir(File.dirname(File.expand_path(f, File.dirname(__FILE__)))) do
        cmd_line = get_command_line(f)
        if log_file
          cmd_line << " #{@append_to_log ? ">>" : ">"} #{log_file} #{@redirect_error ? "2>&1" : ""}"
        end
        @logger.append("cd /d #{saved}", cmd_line)
        system(cmd_line)
        return $?.exitstatus
      end
    end 
  end 
  
  class CRubyDriver < BaseDriver
    def initialize(redirect_error=true, append_to_log=true)
      super("cruby", redirect_error, append_to_log)
    end 
    
    def get_command_line(f)
      "#{TestPath::CRUBY_EXE} -W0 #{File.basename(f)}"
    end 

    def to_s
      "CRubyDriver"
    end
  end 
  
  class IronRubyDriver < BaseDriver
    attr_reader :mode
    
    @@mode_mapping = {
      1 => "-D",
      2 => "",
      3 => "-D -X:SaveAssemblies",
    }
    
    def initialize(mode, name, redirect_error=true, append_to_log=true)
      @mode_string = @@mode_mapping[mode]
      super(name, redirect_error, append_to_log)
    end 
    
    def get_command_line(f)
      "#{TestPath::IRUBY_EXE} #{@mode_string} #{File.basename(f)}"
    end 
    
    def to_s
      "IronRubyDriver ( #{TestPath::IRUBY_EXE} #{@mode_string} )"
    end 
  end 
  
  class CoreClrDriver < BaseDriver
    def initialize(redirect_error=true, append_to_log=true)
      super("coreclr", redirect_error, append_to_log)
    end 
    
    def run(f, logfile)
      f = File.expand_path(f)
      saved = Dir.pwd
      Dir.chdir(TestPath::CORECLR_ROOT) do
        cmd_line = "fxprun.exe thost.exe -fxprun_byname /nologo /lang:rb /run:#{f} /paths:#{File.dirname(f)} #{@append_to_log ? ">>" : ">"} #{logfile} #{@redirect_error ? "2>&1" : ""}"
        @logger.append("cd /d #{TestPath::CORECLR_ROOT}", cmd_line)
        system(cmd_line)
        return $?.exitstatus
      end
    end
  end
  
  # const
  CRuby = CRubyDriver.new
  Iron_m1 = IronRubyDriver.new(1, 'ironm1')
  Iron_m2 = IronRubyDriver.new(2, 'ironm2')
  Iron_m3 = IronRubyDriver.new(3, 'ironm3')
  Iron_cc = CoreClrDriver.new
end 
