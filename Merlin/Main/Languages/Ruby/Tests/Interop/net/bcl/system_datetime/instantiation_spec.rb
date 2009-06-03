require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::DateTime instantiation" do
  it "via Time.now" do
    System::DateTime.now.should be_close(Time.now, TOLERANCE)
  end
  
  it "via Ruby's time constructor" do
    System::DateTime.new.should be_close(Time.now, TOLERANCE)
  end
  
  it "via System::DateTime's constructors" do
    System::DateTime.new(2000, 1, 1).should == Time.local(2000,1,1) 
  end
end
