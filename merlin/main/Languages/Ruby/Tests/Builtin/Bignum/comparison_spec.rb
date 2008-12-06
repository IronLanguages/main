require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#<=>" do
  it "returns -1, 0, 1 when self is less than, equal, or greater than other, where other is Bignum" do
    (0x80000001 <=> 0x80000002).should == -1
    (0x80000002 <=> 0x80000002).should == 0
    (0x80000002 <=> 0x80000001).should == 1
    (-0x80000001 <=> -0x80000002).should == 1
    (-0x80000002 <=> -0x80000002).should == 0
    (-0x80000002 <=> -0x80000001).should == -1
  end
  
  it "returns -1, 0, 1 when self is less than, equal, or greater than other, where other is Fixnum" do
    (0x80000001 <=> 55).should == 1
    (-0x80000002 <=> 55).should == -1
    (0x80000002 <=> -55).should == 1
    (-0x80000001 <=> -55).should == -1
  end

  it "returns -1, 0, 1 when self is less than, equal, or greater than other, where other is Float" do
    (0x80000001 <=> 2147483650.4).should == -1
    (0x80000002 <=> 2147483650.0).should == 0
    (0x80000002 <=> 2147483649.0).should == 1
    (-0x80000001 <=> -2147483649.5).should == 1
    (-0x80000002 <=> -2147483650.0).should == 0
    (-0x80000002 <=> -2147483649.5).should == -1
  end
end
