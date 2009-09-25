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
require "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Scripts\\irtests"
class IRTestTask < Rake::TaskLib
  attr_accessor :name
  @@irtest = nil 
  def initialize(name, &blk)
    @name = name
    unless @@irtest
      @@irtest = IRTest.new
      @@irtest.exit_report
    end
    @block = blk
    define
  end

  def irtest
    @@irtest
  end

  def test(arg)
    @@irtest.test(arg)
  end
  
  def define
    task name => :happy do
      @block.call(self)
    end
  end
end
namespace :test do
  desc "remove output files and generated debugging info from tests directory"
  task :clean do
    Dir.chdir("#{project_root + 'Languages/Ruby/Tests'}") do
      exec "del /s *.log"
      exec "del /s *.pdb"
      exec "del /s *.exe"
      exec "del /s *.dll"
    end
  end

  desc "Run the IronRuby Dev unit test suite"
  IRTestTask.new :smoke do |t|
     t.test(:Smoke)
  end
  
  desc "Run mspec psuedo-folders :lang, :cli, :netinterop, :cominterop, :thread, :netcli"
  IRTestTask.new :spec_a do |t|
    t.test(:RubySpec_A)
  end

  desc "Run mspec psuedo-folders :core1, :lib1"
  IRTestTask.new :spec_b do |t|
    t.test(:RubySpec_B)
  end

  desc "Run mspec psuedo-folders :core2, lib2"
  IRTestTask.new :spec_c do |t|
    t.test(:RubySpec_C)
  end

  desc "Run all mspecs"
  task :specs => [:spec_a,:spec_b,:spec_c]
  
  desc "Run legacy Ruby tests"
  IRTestTask.new :legacy do |t|
    t.test(:Legacy)
  end

  desc "Run app specific tests (Rubygems, Rake and YAML)"
  task :apps => [:gems, :rake, :yaml, :rails]

  desc "Run rake tests"
  IRTestTask.new :rake do |t|
    t.test(:Rake)
  end

  desc "Run gems tests"
  IRTestTask.new :gems do |t|
    t.test(:RubyGems)
  end

  desc "Run Yaml tests"
  IRTestTask.new :yaml do |t|
    t.test(:Yaml)
  end

  desc "Run Rails specific tests"
  task :rails => [:actionpack, :activesupport]
  
  desc "Run ActionPack tests"
  IRTestTask.new :actionpack do |t|
    t.test(:ActionPack)
  end
  
  desc "Run ActiveSupport tests"
  IRTestTask.new :activesupport do |t|
    t.test(:ActiveSupport)
  end

  desc "Run tests corresponding to samples"
  IRTestTask.new :samples do |t|
    t.test(:Tutorial)
  end

  desc "Run all tests"
  task :all => [:compile, "compile:ironpython", :smoke, :legacy, :spec_a, :spec_b, :spec_c, :apps, :samples]
end

task :default => "test:all"
