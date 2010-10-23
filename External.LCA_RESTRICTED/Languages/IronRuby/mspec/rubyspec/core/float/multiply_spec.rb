require File.dirname(__FILE__) + '/../../spec_helper'

describe "Float#*" do
  it "returns self multiplied by other" do 
    (4923.98221 * 2).should be_close(9847.96442, TOLERANCE) 
    (6712.5 * 0.25).should be_close(1678.125, TOLERANCE) 
    (256.4096 * bignum_value).should be_close(2364961134621118431232.000, TOLERANCE)
  end

  #IronRuby was overflowing in this calculation with an OverflowError
  it "allows large multipliers without error" do
    lambda { 1.0 * 2**50000 }.should_not raise_error
  end
end
