require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#to_f" do
  it "returns self converted to Float" do
    0x80000000.to_f.should == 2147483648.0
    -0x80000000.to_f.should == -2147483648.0
    (0x987654321**100).to_f.to_s.should == 'Infinity'
    (-0x987654321**100).to_f.to_s.should == '-Infinity'
  end
end
