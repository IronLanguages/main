require File.dirname(__FILE__) + '/../../spec_helper'

describe "Exception#message" do
  before :each do
    @e = Exception.new("Ouch!")
  end
  
  it "calls to_s" do
    @e.should_receive(:to_s).and_return("to_s response")
    @e.message.should == "to_s response"
  end
end
