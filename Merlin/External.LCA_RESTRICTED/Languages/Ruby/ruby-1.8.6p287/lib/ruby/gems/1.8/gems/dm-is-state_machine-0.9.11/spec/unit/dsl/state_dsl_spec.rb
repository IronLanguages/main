require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent.parent + 'spec_helper'

describe "StateDsl" do

  describe "state" do

    before(:each) do
      class Earth
        extend DataMapper::Is::StateMachine::StateDsl
        stub!(:state_machine_context?).and_return(true)
      end
      machine = mock("machine", :states => [])
      Earth.instance_variable_set(:@is_state_machine, { :machine => machine })
    end

    it "declaration should succeed" do
      class Earth
        state :day
      end
    end

  end

end
