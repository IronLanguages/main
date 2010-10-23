require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/methods'

describe "Time#eql?" do
  it "returns true iff time is equal in seconds and usecs to other time" do
    Time.at(100, 100).should eql(Time.at(100, 100))
    Time.at(100, 100).should_not eql(Time.at(100, 99))
    Time.at(100, 100).should_not eql(Time.at(99, 100))
  end  

  it "returns false when comparing with another type" do
    Time.now.eql?("a string").should == false
  end
  
  it "returns true when comparing UTC time with local time that represents the same point in time" do
    with_timezone("CET", +1) do
      u = Time.utc(1994, 11, 6, 8, 49, 37)
      l = Time.local(1994, 11, 6, 9, 49, 37)
      u.eql?(l).should == true
    end
  end
end
