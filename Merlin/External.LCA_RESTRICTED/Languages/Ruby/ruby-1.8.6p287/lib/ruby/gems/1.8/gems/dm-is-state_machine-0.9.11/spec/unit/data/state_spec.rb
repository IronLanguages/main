require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent.parent + 'spec_helper'

module StateHelper
  def new_state(*args)
    DataMapper::Is::StateMachine::Data::State.new(*args)
  end
end

describe DataMapper::Is::StateMachine::Data::State do
  include StateHelper

  before(:each) do
    @machine = mock("machine")
    @state = new_state(:off, @machine)
  end

  it "#initialize should work" do
    @state.name.should    == :off
    @state.machine.should == @machine
  end
end
