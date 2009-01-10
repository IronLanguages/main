require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#<<" do
  it "returns self shifted left other bits" do
    (0x80000001 << 4).should == 0x800000010
    (0x80000002 << 1).should == 0x100000004
    (0x80000002 << 0).should == 0x80000002
    (0x80000001 << 1.5).should == 0x100000002
    (0x80000002 << -1).should == 0x40000001
    (0x987654321 << 9).should == 0x130eca864200
  end
  it "return the right shift alignment" do
   ((-0xffffffff) << -32).should == -1
   ((-0xffffffff) << -33).should == -1
   ((-0x7fffffffffffffff) << -63).should == -1 
   ((-0xffffffffffffffff) << -64).should == -1 
  end
end
