require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#**" do
  before(:each) do
    @bignum = bignum_value(47)
  end
  
  it "returns self raised to other power" do
    (@bignum ** 4).should == 7237005577332262361485077344629993318496048279512298547155833600056910050625
    (@bignum ** 1.2).should be_close(57262152889751597425762.57804, TOLERANCE)
    (@bignum ** @bignum).finite?.should == false
    (@bignum ** -1).should be_close(1.0842021724855e-019, TOLERANCE)
  end

  it "raises a TypeError when given a non-Integer" do
    lambda { @bignum ** mock('10') }.should raise_error
    lambda { @bignum ** "10" }.should raise_error
    lambda { @bignum ** :symbol }.should raise_error
  end
end
