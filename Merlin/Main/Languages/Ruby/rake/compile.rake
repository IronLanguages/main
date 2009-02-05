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
  IronRubyCompiler.clean
end

desc "compile extension attribute assembly" 
task :compile_extension_attributes => [:clean_build] do
  IronRubyCompiler.compile :dlr_extension
end

desc "compile DLR (Microsoft.Scripting.dll and Microsoft.Scripting.Core.dll)"
task :compile_dlr => [:compile_extension_attributes] do
  IronRubyCompiler.compile :dlr_core
  IronRubyCompiler.compile :dlr_libs
  IronRubyCompiler.compile :dlr_com
end

desc "compile ClassInitGenerator.exe"
task :compile_generator => [:compile_ruby]  do
  IronRubyCompiler.compile :generator
end

desc "compile IronRuby.dll"
task :compile_ruby => [:compile_dlr] do
  IronRubyCompiler.compile :ironruby
end

desc "compile IronRuby.Libraries.dll"
task :compile_libraries => [:compile_ruby] do
  IronRubyCompiler.compile :libraries
end

desc "compile IronRuby console"
task :compile_console => [:compile_libraries] do
  IronRubyCompiler.compile :console
  IronRubyCompiler.move_config
end

desc "compile IronRuby.Tests"
task :compile_testhost => [:compile_libraries] do
  IronRubyCompiler.compile :test_runner
end

desc "compile IronRuby.Libraries.Scanner"
task :compile_scanner => [:compile_libraries] do
  IronRubyCompiler.compile :scanner
end

desc "compile Yaml"
task :compile_yaml => [:compile_libraries] do
  IronRubyCompiler.compile :yaml
end

desc "compile everything"
task :compile => [:happy, :clean_build, :compile_dlr, :compile_ruby, :compile_libraries, :compile_console, :compile_testhost, :compile_generator, :compile_yaml] do
end
