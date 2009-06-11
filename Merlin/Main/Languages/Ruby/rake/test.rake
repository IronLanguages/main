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
  task :smoke => :happy do
     load "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Tests\\Scripts\\irtest.rb"
  end
  
  desc "Run mspec psuedo-folders :lang, :cli, :netinterop, :cominterop, :thread"
  task :spec_a => :happy do
    IRTest.test(:RubySpec_A)
  end

  desc "Run mspec psuedo-folders :core1, :lib1"
  task :spec_b => :happy do
    IRTest.test(:RubySpec_B)
  end

  desc "Run mspec psuedo-folders :core2, lib2"
  task :spec_c => :happy do
    IRTest.test(:RubySpec_C)
  end

  desc "Run all mspecs"
  task :specs => [:spec_a,:spec_b,:spec_c]
  
  desc "Run legacy Ruby tests"
  task :legacy => :happy do
    IRTest.test(:Legacy)
  end

  desc "Run app specific tests (Rubygems and Rake)"
  task :apps => [:gems, :rake]

  desc "Run rake tests"
  task :rake => :happy do
    IRTest.test(:Rake)
  end

  desc "Run gems tests"
  task :gems => :happy do
    IRTest.test(:RubyGems)
  end

  desc "(NOT IMPLEMENTED) Run tests corresponding to samples"
  task :samples => :happy do

  end

  desc "Run all tests"
  task :all => [:compile, "compile:ironpython", :smoke, :legacy, :spec_a, :spec_b, :spec_c, :apps]
end

task :default => "test:all"
