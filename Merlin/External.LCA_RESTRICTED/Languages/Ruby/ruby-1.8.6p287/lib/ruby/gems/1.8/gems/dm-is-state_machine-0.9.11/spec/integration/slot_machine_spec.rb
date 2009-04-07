require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'
require Pathname(__FILE__).dirname.expand_path.parent + 'examples/slot_machine'

describe SlotMachine do

  before(:each) do
    @sm = SlotMachine.new
  end

  it "should have an 'id' column" do
    @sm.attributes.should have_key(:id)
  end

  it "should have a 'mode' column" do
    @sm.attributes.should have_key(:mode)
  end

  it "should have a 'power_on' column" do
    @sm.attributes.should have_key(:power_on)
  end

  it "should not have a 'state' column" do
    @sm.attributes.should_not have_key(:state)
  end

  it "should start in the off state" do
    @sm.mode.should == "off"
  end

  it "should start with power_on == false" do
    @sm.power_on.should == false
  end

  describe "in :off mode" do

    before(:each) do
      @sm.mode = :off
      @sm.power_on = false
    end

    it "should allow the mode to be set" do
      @sm.mode = :idle
      @sm.save
      @sm.mode.should == "idle"
    end

    it "turn_on! should work from off mode" do
      @sm.turn_on!
      @sm.mode.should == "idle"
      @sm.power_on.should == true
    end

    it "turn_on! should not work twice in a row" do
      @sm.turn_on!
      lambda {
        @sm.turn_on!
      }.should raise_error(DataMapper::Is::StateMachine::InvalidEvent)
    end

  end

  describe "in :idle mode" do

    before(:each) do
      @sm.mode = :idle
      @sm.power_on = true
    end

    it "turn_on! should raise error" do
      lambda {
        @sm.turn_on!
      }.should raise_error(DataMapper::Is::StateMachine::InvalidEvent)
    end

    it "turn_off! should work" do
      @sm.turn_off!
      @sm.mode.should == "off"
      @sm.power_on.should == false
    end

    it "turn_off! should not work twice in a row" do
      @sm.turn_off!
      lambda {
        @sm.turn_off!
      }.should raise_error(DataMapper::Is::StateMachine::InvalidEvent)
    end

  end

end
