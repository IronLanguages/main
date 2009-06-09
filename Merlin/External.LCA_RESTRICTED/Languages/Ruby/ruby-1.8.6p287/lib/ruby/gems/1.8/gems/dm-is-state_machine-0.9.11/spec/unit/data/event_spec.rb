require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent.parent + 'spec_helper'

module EventHelper
  def new_event(*args)
    DataMapper::Is::StateMachine::Data::Event.new(*args)
  end
end

describe DataMapper::Is::StateMachine::Data::Event do
  include EventHelper

  before(:each) do
    @machine = mock("machine")
    @event = new_event(:ping, @machine)
  end

  it "#initialize should work" do
    @event.name.should        == :ping
    @event.machine.should     == @machine
    @event.transitions.should == []
  end

  it "#add_transition should work" do
    @event.add_transition(:nothing, :pinged)
    @event.transitions.should == [{:from => :nothing, :to => :pinged }]
  end
end
