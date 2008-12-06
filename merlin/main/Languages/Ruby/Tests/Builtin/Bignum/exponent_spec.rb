require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#**" do
  it "returns self raised to other power, where other is Fixnum" do
    (0x80000001 ** 4).class.to_s.should == 'Bignum'
    (0x80000001 ** 4).should == 0x10000000800000018000000200000001
  end
  it "returns self raised to other power, where other is Float" do
    (0x80000001 ** 5.2).class.to_s.should == 'Float'
    (10000000000 ** 0.5).should == 100000.0
    (0x80000001 ** 5.2).to_s.should == '3.35764906138532e+048'
  end
end
