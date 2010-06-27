require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::Object maps to" do
  it "Object" do
    System::Object.should == Object
  end
end
