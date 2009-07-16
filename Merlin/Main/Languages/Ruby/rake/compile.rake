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
namespace :compile do
  desc "compile extension attribute assembly" 
  task :extension_attributes => [:clean_build] do
    IronRubyCompiler.compile :dlr_extension
  end

  desc "compile DLR (Microsoft.Scripting.dll and Microsoft.Scripting.Core.dll)"
  task :dlr => [:extension_attributes] do
    IronRubyCompiler.compile :dlr_core
    IronRubyCompiler.compile :dlr_libs
    IronRubyCompiler.compile :dlr_com
    IronRubyCompiler.compile :dlr_debug
  end

  desc "compile ClassInitGenerator.exe"
  task :generator => [:ruby]  do
    IronRubyCompiler.compile :generator
  end

  desc "compile IronRuby.dll"
  task :ruby => [:dlr] do
    IronRubyCompiler.compile :ironruby
  end

  desc "compile IronRuby.Libraries.dll"
  task :libraries => [:ruby] do
    IronRubyCompiler.compile :libraries
  end

  desc "compile IronRuby console"
  task :console => [:libraries] do
    IronRubyCompiler.compile :console
    IronRubyCompiler.move_config
  end

  desc "compile IronRuby.Tests"
  task :testhost => [:libraries] do
    IronRubyCompiler.compile :test_runner
    IronRubyCompiler.move_config "IronRuby.Tests.exe.config"
  end

  desc "compile IronRuby.Libraries.Scanner"
  task :scanner => [:libraries] do
    IronRubyCompiler.compile :scanner
  end

  desc "compile Yaml"
  task :yaml => [:libraries] do
    IronRubyCompiler.compile :yaml
  end

  desc "compile IronPython"
  task :ironpython => [:dlr] do
    [:ironpython, :ipyw, :ipy, :ironpython_modules].each do |target|
      IronRubyCompiler.compile target
    end
  end
  desc "compile IronRuby and IronPython"
  task :all => [:compile, :ironpython]
end
desc "compile everything"
task :compile => %w{happy clean_build compile:dlr compile:ruby compile:libraries compile:console compile:testhost compile:generator compile:yaml}


