require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent.parent + 'spec_helper'

module MachineHelper
  def new_machine(*args)
    DataMapper::Is::StateMachine::Data::Machine.new(*args)
  end

  def new_state(name, machine, options = {})
    mock(name, :name => name, :machine => machine, :options => options)
  end

  def new_event(name, machine)
    mock(name, :name => name, :machine => machine)
  end
end

describe DataMapper::Is::StateMachine::Data::Machine do
  include MachineHelper

  describe "new Machine, no events" do
    before(:each) do
      @machine = new_machine(:power, :off)
    end

    it "#column should work" do
      @machine.column.should == :power
    end

    it "#initial should work" do
      @machine.initial.should == :off
    end

    it "#events should work" do
      @machine.events.should == []
    end

    it "#states should work" do
      @machine.states.should == []
    end

    it "#find_event should return nothing" do
      @machine.find_event(:turn_on).should == nil
    end

    it "#fire_event should raise error" do
      lambda {
        @machine.fire_event(:turn_on, nil)
      }.should raise_error(DataMapper::Is::StateMachine::InvalidEvent)
    end
  end

  describe "new Machine, 2 states, 1 event" do
    before(:each) do
      @machine = new_machine(:power, :off)
      @machine.states << (@off_state = new_state(:off, @machine))
      @machine.states << (@on_state = new_state(:on, @machine))
      @machine.events << (@turn_on = new_event(:turn_on, @machine))
      @turn_on.stub!(:transitions).and_return([{ :from => :off, :to => :on }])
    end

    it "#column should work" do
      @machine.column.should == :power
    end

    it "#initial should work" do
      @machine.initial.should == :off
    end

    it "#events should work" do
      @machine.events.should == [@turn_on]
    end

    it "#states should work" do
      @machine.states.should == [@off_state, @on_state]
    end

    it "#current_state should work" do
      @machine.current_state.should == @off_state
    end

    it "#current_state_name should work" do
      @machine.current_state_name.should == :off
    end

    it "#find_event should return nothing" do
      @machine.find_event(:turn_on).should == @turn_on
    end

    it "#fire_event should change state" do
      resource = mock("resource")
      resource.should_receive(:run_hook_if_present).exactly(2).times.with(nil)
      @machine.fire_event(:turn_on, resource)
      @machine.current_state.should == @on_state
      @machine.current_state_name.should == :on
    end

  end

  # TODO: spec fire_event where :run_hook_if_present fires two times,
  # but with :enter the first and :exit the second.

end
