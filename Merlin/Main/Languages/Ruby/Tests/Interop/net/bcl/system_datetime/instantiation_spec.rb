require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::DateTime instantiation" do
  it "via Time.now" do
    t1 = System::DateTime.now
    t2 = Time.now

    (t2 - t1).should < 60.0
  end
  
  it "via Ruby's time constructor" do
    t1 = System::DateTime.new
    t2 = Time.now

    (t2 - t1).should < 60.0
  end
  
  it "via System::DateTime's constructors" do
    System::DateTime.new(2000, 1, 1).should == Time.local(2000,1,1) 
  end
end
