require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Interfaces" do
  it "be in ancestor list" do
    ImplementsIInterface.ancestors.should include(IInterface)
    RubyImplementsIInterface.ancestors.should include(IInterface)
  end
end
