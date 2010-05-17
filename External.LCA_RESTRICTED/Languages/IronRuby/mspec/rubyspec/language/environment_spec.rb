require File.dirname(__FILE__) + "/../spec_helper"

describe "RUBYLIB environment variable" do
  before(:each) do
    @rubylib = ENV["RUBYLIB"]
    @tmp_loadpath = tmp("loadpath")
    ENV["RUBYLIB"] = @tmp_loadpath
  end

  after(:all) do
    ENV["RUBYLIB"] = @rubylib
  end

  it "adds load paths to $:" do
    ruby_exe(fixture(__FILE__, "loadpath.rb")).chomp.split.include?(@tmp_loadpath).should == true
  end

  it "adds load paths after -I load paths" do
    ruby_exe(fixture(__FILE__, "loadpath.rb"), :options => '-I lib').chomp.split[1].should == @tmp_loadpath
  end
  
  it "adds load paths before normal $: startup items" do
    ruby_exe(fixture(__FILE__, "loadpath.rb")).chomp.split[0].should == @tmp_loadpath
  end
end
