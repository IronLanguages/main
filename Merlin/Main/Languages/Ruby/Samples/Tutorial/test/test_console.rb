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

require 'rubygems'
require "test/spec"
require "stringio"
require "console_tutorial"

describe "ConsoleTutorial" do 
  before(:each) do
    @in = StringIO.new
    @out = StringIO.new
    @app = ConsoleTutorial.new nil, @in, @out
  end
  
  it "should early out" do
    @in.string = ["0"].join("\n")
    @app.run
    @out.string.should =~ /Bye!/
  end

  it 'should chose a section' do
    @in.string = ["1", "0", "0"].join("\n")
    @app.run
    @out.string.should =~ /Bye!/
  end
end

describe "Tutorial" do
  before(:each) do
    @in = StringIO.new
    @out = StringIO.new
    @app = ConsoleTutorial.new nil, @in, @out
  end

  app = ConsoleTutorial.new
  app.tutorial.sections.each_index do |s|
    section = app.tutorial.sections[s]
    section.chapters.each_index do |c|
      chapter = section.chapters[c]
      test_name = "should test #{section.name} #{chapter.name}"
      it test_name do
        codes = chapter.tasks.collect { |task| task.code }
        @in.string = ([(s + 1).to_s, (c + 1).to_s] + codes).join("\n")
        @app.run
        @out.string.should =~ /Chapter completed successfully!/
      end
    end
  end
end
