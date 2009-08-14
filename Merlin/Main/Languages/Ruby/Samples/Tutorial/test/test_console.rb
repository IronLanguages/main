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

orig_dir = Dir.pwd
if $0 == __FILE__
  filename = File.expand_path __FILE__, orig_dir
else
  filename = __FILE__
end
dirname = File.dirname filename

require 'rubygems'
require 'test/spec'
require 'stringio'
require 'fileutils'
require dirname + '/../tutorial.rb'
require dirname + '/../console_tutorial'
require dirname + '/../html_tutorial'

if $LOAD_PATH.include? "."
  $LOAD_PATH.delete "."
  $LOAD_PATH << Dir.pwd
end

FileUtils.cd(File.expand_path('/')) # This ensures that the tutorial can be launched from any folder

describe "ReplContext" do
  before(:each) do
    @context = Tutorial::ReplContext.new
  end
  
  it "works with single-line input" do
    @context.interact("2+2").result.should == 4
  end

  it "works with multi-line code" do
    code = ["if true", "101", "else", "102", "end"].join("\n")
    @context.interact(code).result.should == 101
  end

  it "works with multi-line input" do
    result = nil
    ["if true", "101", "else", "102", "end"].each {|i| result = @context.interact i }
    result.result.should == 101
  end

  it "can be reset" do
    ["if true", "101", "else"].each {|i| @context.interact i }
    @context.reset_input
    @context.interact("2+2").result.should == 4
  end
end

describe "ConsoleTutorial" do 
  before(:each) do
    @in = StringIO.new
    @out = StringIO.new
    tutorial = Tutorial.get_tutorial(dirname + '/../Tutorials/tryruby_tutorial.rb')
    @app = ConsoleTutorial.new tutorial, @in, @out
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

# Helper module to programatically create "test_xxx" methods for each task in each chapter
module TutorialTests  
  def self.format_interaction_result code, result
    "code = #{code.inspect} #{result}"
  end
  
  def self.create_tests testcase, tutorial_path
    tutorial = Tutorial.get_tutorial tutorial_path
    context = Tutorial::ReplContext.new
    tutorial.sections.each_index do |s|
      section = tutorial.sections[s]
      section.chapters.each_index do |c|
        chapter = section.chapters[c]
        test_name = "#{section.name} - #{chapter.name}"
        
        testcase.it(test_name) { TutorialTests.run_test context, chapter }
      end
    end
  end
  
  def self.assert_task_success(task, code, result, success=true)
    res = TutorialTests.format_interaction_result(code, result)
    if success
      task.success?(result, true).should.blaming(res) == true
    else
      task.success?(result).should.blaming(res) == false
    end
  end

  def self.run_test context, chapter
    chapter.tasks.each do |task| 
      if not task.should_run? context.bind
        1.should == 1 # we do a dummy expectation here. TODO - There should be another way to indicate the test passed without having any expectations
        return
      end
      task.setup.call(context.bind) if task.setup
      result = context.interact "" # Ensure that the user can try unrelated code snippets without moving to the next task
      if task.code.respond_to? :to_ary
        task.code.each do |code|
          assert_task_success task, "before : #{code}", result, false
          result = context.interact code
        end
        assert_task_success task, task.code.last, result
      else
        assert_task_success task, "before : #{task.code_string}", result, false
        result = context.interact task.code_string
        assert_task_success task, task.code_string, result
      end
      task.test_hook.call(:cleanup, context.bind) if task.test_hook
    end
  end
end

describe "IronRubyTutorial" do
  TutorialTests.create_tests self, dirname + '/../Tutorials/ironruby_tutorial.rb' if defined? RUBY_ENGINE
end

describe "HostingTutorial" do
  TutorialTests.create_tests self, dirname + '/../Tutorials/hosting_tutorial.rb' if defined? RUBY_ENGINE
end

describe "TryRubyTutorial" do
  TutorialTests.create_tests self, dirname + '/../Tutorials/tryruby_tutorial.rb'
end

describe "HtmlGeneratorTests" do
  it "basically works" do
    tutorial = Tutorial.get_tutorial(dirname + '/../Tutorials/tryruby_tutorial.rb')
    html_tutorial = HtmlTutorial.new tutorial
    html = html_tutorial.generate_html
    assert_match %r{<h2>Table of Contents</h2>}, html
  end
end
