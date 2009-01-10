require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#size" do
  it "returns number of bytes in self" do
    (0x100000000000000).size.should == 8 #(256**7)
    (0x10000000000000000).size.should == 12 #(256**8)
    (0x1000000000000000000).size.should == 12 #(256**9)
    (0x100000000000000000000).size.should == 12 #(256**10)
    (0xffffffffffffffffffff).size.should == 12 #(256**10-1)
    (0x10000000000000000000000).size.should == 12 #(256**11)
    (0x1000000000000000000000000).size.should == 16 #(256**12)
    (0x10000000000000000000000000000000000000000).size.should == 24 #(256**20)
    (0xffffffffffffffffffffffffffffffffffffffff).size.should == 20 #(256**20-1)
    (0x100000000000000000000000000000000000000000000000000000000000000000000000000000000).size.should == 44 #(256**40)
    (0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff).size.should == 40 #(256**40-1)
  end
end
