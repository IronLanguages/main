# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

desc "generate initializers.generated.cs file for IronRuby.Libraries"
task :gen do
  cmd = "#{build_path + 'ClassInitGenerator.exe'} #{build_path + 'IronRuby.Libraries.dll'} /libraries:IronRuby.Builtins;IronRuby.StandardLibrary.Threading;IronRuby.StandardLibrary.Sockets;IronRuby.StandardLibrary.OpenSsl;IronRuby.StandardLibrary.Digest;IronRuby.StandardLibrary.Zlib;IronRuby.StandardLibrary.StringIO;IronRuby.StandardLibrary.StringScanner;IronRuby.StandardLibrary.Enumerator;IronRuby.StandardLibrary.FunctionControl;IronRuby.StandardLibrary.FileControl;IronRuby.StandardLibrary.BigDecimal /out:#{IronRubyCompiler.dir(:libraries) + 'Initializers.Generated.cs'}"
  exec_net cmd
end

desc "generate initializers.generated.cs file for IronRuby.Libraries.Yaml"
task :gen_yaml do
  cmd = "#{build_path + 'ClassInitGenerator'} #{build_path + 'IronRuby.Libraries.Yaml.dll'} /libraries:IronRuby.StandardLibrary.Yaml /out:#{IronRubyCompiler.dir(:yaml) + 'Initializer.Generated.cs'}"
  exec_net cmd
end

def path_exists?(paths, command)
  paths.each do |path|
    return true if (path + command).exists?
  end
  false
end

STDLIB_CLASSES = {}

def walk_classes(klass)
  klass.constants.each do |constant|
    current_klass = klass.const_get(constant)
    if current_klass.is_a?(Module)
      return if STDLIB_CLASSES.has_key?(current_klass.name)
      STDLIB_CLASSES[current_klass.name] = true
      walk_classes(current_klass)
    end
  end
end

desc "perform gap analysis on app's library method usage and IronRuby"
task :gap => [:compile_scanner] do
  libraries_file = Pathname.new(Dir.tmpdir) + "libraries.txt"
  exec "#{build_path + 'IronRuby.Libraries.Scanner.exe'} > \"#{libraries_file}\""

  library_methods = {}
  IO.foreach(libraries_file) { |method| library_methods[method.strip] = true }

  # Generate list of methods used by program
  trace_output_stream = File.open(Pathname.new(Dir.tmpdir) + 'trace.txt', 'w')
  function_table = {}

  ARGV.delete_at 0
  if ARGV.length < 1
    rake_output_message "usage: rake gap program [args]"
    exit(-1)
  end

  app_name = ARGV.first
  ARGV.delete_at 0

  walk_classes(self.class)

  set_trace_func proc { |event, file, line, id, binding, klass|
    if event == "c-call" || event == "call"
      method_name = klass.to_s + "#" + id.to_s
      function_table[method_name] = true
    end
  }

  at_exit do
    rake_output_message 'shutdown ...'
    function_table.keys.sort.each do |method_name|
      class_name = method_name.split("#").first
      if STDLIB_CLASSES.has_key?(class_name) && !library_methods.has_key?(method_name)
        # output methods that aren't in standard library
        trace_output_stream.puts method_name
      end
    end
    trace_output_stream.close
  end

  load app_name
  rake_output_message 'about to exit ...'
  exit
end

desc "is the environment setup for an IronRuby dev?"
task :happy do
  commands = !mono? ? ['resgen.exe', 'csc.exe'] : ['resgen', 'gmcs']

  paths = ENV['PATH'].split(File::PATH_SEPARATOR).collect { |path| Pathname.new path.gsub(/\'|\"/,'') }

  failure = false
  commands.each do |command|
    if !path_exists? paths, command
      rake_output_message "Cannot find #{command} on system path."
      failure = true
    end
  end

  if failure
    rake_output_message "\n"
    rake_output_message "***** Missing commands! You must have the .NET redist and the SDK"
    rake_output_message "***** (for resgen.exe) installed. "
    abort
  end
end
