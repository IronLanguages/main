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
# Compilation tasks

desc "clean build directory"
task :clean_build => [:happy] do
  IronRuby.source_context do
    rd build_path
    mkdir build_path
  end
end

desc "compile extension attribute assembly" 
task :compile_extension_attributes => [:clean_build] do
  IronRuby.source_context do
    compile :dlr_core, :references => ['!System.dll'], :switches => ['target:library'], :output => 'Microsoft.Scripting.ExtensionAttribute.dll', :csproj => 'microsoft.scripting.extensionattribute.csproj'
  end
end

desc "compile DLR (Microsoft.Scripting.dll and Microsoft.Scripting.Core.dll)"
task :compile_dlr => [:compile_extension_attributes] do
  IronRuby.source_context do
    compile :dlr_core, :references => ['!System.dll', '!System.Configuration.dll', 'Microsoft.Scripting.ExtensionAttribute.dll'], :switches => ['target:library', 'define:MICROSOFT_SCRIPTING_CORE'], :output => 'Microsoft.Scripting.Core.dll', :csproj => 'Microsoft.Scripting.Core.csproj'
    resources = { Pathname.new('math') + 'MathResources.resx' => Pathname.new('Microsoft.Scripting.Math.MathResources.resources') }
    compile :dlr_libs, :references => ['Microsoft.Scripting.Core.dll', '!System.Xml.dll', '!System.dll', '!System.Configuration.dll', 'Microsoft.Scripting.ExtensionAttribute.dll','!System.Runtime.Remoting.dll'], :switches => ['target:library'], :resources => resources, :output => 'Microsoft.Scripting.dll', :csproj => 'Microsoft.Scripting.csproj'
    compile :dlr_com, :references => ['Microsoft.Scripting.Core.dll', '!System.Xml.dll', '!System.dll', 'Microsoft.Scripting.ExtensionAttribute.dll'], :switches => ['target:library', 'unsafe'], :output => 'Microsoft.Dynamic.dll'
  end
end

desc "compile ClassInitGenerator.exe"
task :compile_generator => [:compile_ruby]  do
  IronRuby.source_context do
    compile :generator, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'Microsoft.Scripting.ExtensionAttribute.dll', 'IronRuby.dll', '!System.dll'], :output => 'ClassInitGenerator.exe'
  end
end

desc "compile IronRuby.dll"
task :compile_ruby => [:compile_dlr] do
  IronRuby.source_context do
    compile :ironruby, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'Microsoft.Scripting.ExtensionAttribute.dll', '!System.dll', '!System.Configuration.dll'], :switches => ['target:library'], :output => 'IronRuby.dll'
  end
end

desc "compile IronRuby.Libraries.dll"
task :compile_libraries => [:compile_ruby] do
  IronRuby.source_context do
    compile :libraries, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'Microsoft.Scripting.ExtensionAttribute.dll', 'IronRuby.dll', '!System.dll'], :switches => ['target:library'], :output => 'IronRuby.Libraries.dll'
  end
end

desc "compile IronRuby console"
task :compile_console => [:compile_libraries] do
  IronRuby.source_context do
    compile :console, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll'], :output => IRONRUBY_COMPILER
    transform_config_file IronRuby.is_merlin? ? 'Merlin' : 'Svn', get_source_dir(:lang_root) + 'app.config', "#{build_path}\\ir.exe.config"
  end
end

desc "compile IronRuby.Tests"
task :compile_testhost => [:compile_libraries] do
  IronRuby.source_context do
    compile :test_runner, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.dll', '!System.Windows.Forms.dll'], :output => 'IronRuby.Tests.exe'
  end
end

desc "compile IronRuby.Libraries.Scanner"
task :compile_scanner => [:compile_libraries] do
  IronRuby.source_context do
    compile :scanner, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.Core.dll'], :output => 'IronRuby.Libraries.Scanner.exe'
  end
end

desc "compile Yaml"
task :compile_yaml => [:compile_libraries] do
  IronRuby.source_context do
    compile :yaml, :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.dll'], :switches => ['target:library'], :output => 'IronRuby.Libraries.Yaml.dll'
  end
end

desc "compile everything"
task :compile => [:happy, :clean_build, :compile_dlr, :compile_ruby, :compile_libraries, :compile_console, :compile_testhost, :compile_generator, :compile_yaml] do
end
