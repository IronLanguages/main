require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#+" do
  it "returns self plus other, where other is Bignum" do
    (0x80000000 + 0x80000000).should == 0x100000000
    (0x80000000 + -0x80000000).should == 0
    (-0x80000000 + 0x80000000).should == 0
    (-0x80000000 + -0x80000000).should == -0x100000000
  end
  it "returns self plus other, where other is Fixnum" do
    (0x80000000 + 5).should == 0x80000005
    (0x80000000 + -5).should == 0x7ffffffb
    (-0x80000000 + 5).should == -0x7ffffffb
    (-0x80000000 + -5).should == -0x80000005
    (0x80000000 + 1).should == 0x80000001
    (0x80000000 + 0).should == 0x80000000
  end
  it "normalizes result to a Fixnum as necessary" do
    (0x80000000 + (-0x70000000)).class.to_s.should == 'Fixnum'
    (-0x80000000 + 0x70000000).class.to_s.should == 'Fixnum'
  end
  it "returns self plus other, where other is Float" do
    (0x80000000 + 7.4).should_be_close(2147483655.4, TOLERANCE)
    (0x80000000 + -7.4).should_be_close(2147483640.6, TOLERANCE)
    (-0x80000000 + 7.4).should_be_close(-2147483640.6, TOLERANCE)
    (-0x80000000 + -7.4).should_be_close(-2147483655.4, TOLERANCE)
    (0x80000000 + 0.0).should_be_close(2147483648.0, TOLERANCE)
    (0x80000000 + 0.0).class.to_s.should == 'Float'
  end
end
