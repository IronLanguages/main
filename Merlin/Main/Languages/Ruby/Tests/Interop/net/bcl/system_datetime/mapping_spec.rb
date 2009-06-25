require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::DateTime" do
  it "is Time" do
    System::DateTime.should == Time
  end
end
