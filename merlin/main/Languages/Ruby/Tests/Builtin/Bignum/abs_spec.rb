require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#abs" do
  it "returns the absolute value" do
    (0x80000000).abs.should == 0x80000000
    (-0x80000000).abs.should == 0x80000000
    (0x987654321).abs.should == 0x987654321
    (-0x987654321).abs.should == 0x987654321
  end
end
