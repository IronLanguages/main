require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe "InvalidStates" do

  it "should get InvalidContext when requiring" do
    lambda {
      require File.join( File.dirname(__FILE__), "..", "examples", "invalid_states" )
    }.should raise_error(DataMapper::Is::StateMachine::InvalidContext)
  end

end
