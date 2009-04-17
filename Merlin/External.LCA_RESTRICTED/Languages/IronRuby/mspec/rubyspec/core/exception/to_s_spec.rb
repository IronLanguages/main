require File.dirname(__FILE__) + '/../../spec_helper'

describe "Exception#to_s" do
  before :each do
    @e = Exception.new("Ouch!")
  end
  
  it "returns the exception message" do
    @e.to_s.should == "Ouch!"
  end  

  it "is the class name by default" do
    Exception.new.to_s.should == "Exception"
  end
  
  it "can return a non-String" do
    m = mock("message")
    Exception.new(m).to_s.should equal(m)
  end
end
