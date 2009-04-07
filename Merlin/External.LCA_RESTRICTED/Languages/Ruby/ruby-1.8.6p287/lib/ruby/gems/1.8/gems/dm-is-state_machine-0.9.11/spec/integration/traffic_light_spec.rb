require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'
require Pathname(__FILE__).dirname.expand_path.parent + 'examples/traffic_light'

describe TrafficLight do

  before(:each) do
    @t = TrafficLight.new
  end

  it "should have an 'id' column" do
    @t.attributes.should have_key(:id)
  end

  it "should have a 'color' column" do
    @t.attributes.should have_key(:color)
  end

  it "should not have a 'state' column" do
    @t.attributes.should_not have_key(:state)
  end

  it "should start off in the green state" do
    @t.color.should == "green"
  end

  it "should allow the color to be set" do
    @t.color = :yellow
    @t.save
    @t.color.should == "yellow"
  end

  it "should have called the :enter Proc" do
    @t.log.should == %w(G)
  end

  it "should call the original initialize method" do
    @t.init.should == [:init]
  end

  describe 'forward!' do

    it "should respond to :forward!" do
      @t.respond_to?(:forward!).should == true
    end

    it "should transition to :yellow, :red, :green" do
      @t.color.should == "green"
      @t.forward!
      @t.color.should == "yellow"
      @t.log.should == %w(G Y)
      @t.forward!
      @t.color.should == "red"
      @t.log.should == %w(G Y R)
      @t.forward!
      @t.color.should == "green"
      @t.log.should == %w(G Y R G)
      @t.new_record?.should == true
    end

    it "should skip to :yellow then transition to :red, :green, :yellow" do
      @t.color = :yellow
      @t.color.should == "yellow"
      @t.log.should == %w(G)
      @t.forward!
      @t.color.should == "red"
      @t.log.should == %w(G R)
      @t.forward!
      @t.color.should == "green"
      @t.log.should == %w(G R G)
      @t.forward!
      @t.color.should == "yellow"
      @t.log.should == %w(G R G Y)
      @t.new_record?.should == true
    end

    it "should skip to :red then transition to :green, :yellow, :red" do
      @t.color = :red
      @t.color.should == "red"
      @t.log.should == %w(G)
      @t.forward!
      @t.color.should == "green"
      @t.log.should == %w(G G)
      @t.forward!
      @t.color.should == "yellow"
      @t.log.should == %w(G G Y)
      @t.forward!
      @t.color.should == "red"
      @t.log.should == %w(G G Y R)
      @t.new_record?.should == true
    end

  end

  describe 'backward!' do

    it "should respond to 'backward!'" do
      @t.respond_to?(:backward!).should == true
    end

    it "should transition to :red, :yellow, :green" do
      @t.color.should == "green"
      @t.log.should == %w(G)
      @t.backward!
      @t.color.should == "red"
      @t.log.should == %w(G R)
      @t.backward!
      @t.color.should == "yellow"
      @t.log.should == %w(G R Y)
      @t.backward!
      @t.color.should == "green"
      @t.log.should == %w(G R Y G)
      @t.new_record?.should == true
    end

    it "should skip to :yellow then transition to :green, :red, :yellow" do
      @t.color = :yellow
      @t.color.should == "yellow"
      @t.log.should == %w(G)
      @t.backward!
      @t.color.should == "green"
      @t.log.should == %w(G G)
      @t.backward!
      @t.color.should == "red"
      @t.log.should == %w(G G R)
      @t.backward!
      @t.color.should == "yellow"
      @t.log.should == %w(G G R Y)
      @t.new_record?.should == true
    end

    it "should skip to :red then transition to :yellow, :green, :red" do
      @t.color = :red
      @t.color.should == "red"
      @t.log.should == %w(G)
      @t.backward!
      @t.color.should == "yellow"
      @t.log.should == %w(G Y)
      @t.backward!
      @t.color.should == "green"
      @t.log.should == %w(G Y G)
      @t.backward!
      @t.color.should == "red"
      @t.log.should == %w(G Y G R)
      @t.new_record?.should == true
    end

  end

  describe "hooks" do

    it "should log initial state before state is changed on a before hook" do
      @t.forward!
      @t.before_hook_log.should == %w(green)
      @t.forward!
      @t.before_hook_log.should == %w(green yellow)
      @t.forward!
      @t.before_hook_log.should == %w(green yellow red)
    end

    it "should log final state before state is changed on a before hook" do
      @t.forward!
      @t.after_hook_log.should == %w(yellow)
      @t.forward!
      @t.after_hook_log.should == %w(yellow red)
      @t.forward!
      @t.after_hook_log.should == %w(yellow red green)
    end

  end

  describe "overwriting event methods" do

    before(:all) do
      TrafficLight.class_eval "def forward!(added_param); log << added_param; transition!(:forward); end"
    end

    it "should transition normally with added functionality" do
      @t.color.should == "green"
      @t.forward!("test")
      @t.color.should == "yellow"
      @t.log.should == %w(G test Y)
    end

  end
end
