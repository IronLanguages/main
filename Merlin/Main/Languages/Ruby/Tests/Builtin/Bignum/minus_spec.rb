require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#-" do
  it "returns self minus other, where other is Bignum" do
    (0x90000009 - 0x90000009).should == 0
    (0x90000009 - 0x10000009).should == 0x80000000
    (-0x80000000 - 0x80000000).should == -0x100000000
    (0x80000000 - (-0x80000000)).should == 0x100000000
  end
  it "normalizes to Fixnum if necessary" do
    (0x80000000 - 0x80000000).class.to_s.should == 'Fixnum'
    (0x80000005 - 0x40000006).class.to_s.should == 'Fixnum'
  end
  it "returns self minus other, where other is Fixnum" do
    (0x80000030 - 0x30).should == 0x80000000
    (-0x80000000 - 0x30).should == -0x80000030
    (0x80000000 - (-0x30)).should == 0x80000030
  end
  it "returns self minus other, where other is Float" do
    (0x80000000 - 56.7).should_be_close(2147483591.3, TOLERANCE)
    (-0x80000000 - 2147483648.0).should_be_close(-4294967296.0, TOLERANCE)
    (0x80000000 - (-56.7)).should_be_close(2147483704.7, TOLERANCE)
  end
end
