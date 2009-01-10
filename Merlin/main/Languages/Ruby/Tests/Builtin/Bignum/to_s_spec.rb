require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#to_s" do  
  it "returns a string representation of self, base 10" do
    0x80000000.to_s.should == "2147483648"
    (-0x80000000).to_s.should == "-2147483648"
    0x987654321.to_s.should == "40926266145"
  end
  
  it "returns a string with the representation of self in base x"  do 
    # 18446744073709551616 == 2**64
    18446744073709551616.to_s(2).should == "10000000000000000000000000000000000000000000000000000000000000000" 
    18446744073709551616.to_s(8).should == "2000000000000000000000"
    18446744073709551616.to_s(16).should == "10000000000000000"
    18446744073709551616.to_s(32).should == "g000000000000" 
  end
  
  it "raises an ArgumentError exception if argument is 0" do
    should_raise(ArgumentError){ 18446744073709551616.to_s(0) }
  end
  
  it "raises an ArgumentError exception if argument is bigger than 36" do 
    should_raise(ArgumentError){ 18446744073709551616.to_s(37) } # Max is 36
  end
end
