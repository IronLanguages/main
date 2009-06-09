# this is an extremely hacky spec
# intended purely to test the Windoze-specific code

require 'rubygems'
require 'spec'

describe "github/command.rb" do
  before(:all) do
    @orig_platform = RUBY_PLATFORM
    Object.send :remove_const, :RUBY_PLATFORM
    Object.const_set :RUBY_PLATFORM, "mswin"
  end

  after(:all) do
    Object.send :remove_const, :RUBY_PLATFORM
    Object.const_set :RUBY_PLATFORM, @orig_platform
  end

  before(:each) do
    @filename = File.dirname(__FILE__) + "/../lib/github/command.rb"
    @data = File.read(@filename)
  end

  it "should require win32/open3 under Windows" do
    mod = Module.new
    mod.should_receive(:require).with("win32/open3")
    mod.class_eval @data, @filename
  end

  it "should blow up if win32/open3 isn't present under Windows" do
    mod = Module.new
    mod.should_receive(:require).with("win32/open3").and_return { raise LoadError }
    mod.should_receive(:warn).with("You must 'gem install win32-open3' to use the github command on Windows")
    lambda { mod.class_eval @data, @filename }.should raise_error(SystemExit)
  end
end
