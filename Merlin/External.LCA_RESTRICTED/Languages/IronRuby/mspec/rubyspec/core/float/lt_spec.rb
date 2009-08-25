require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Float#<" do
  it "returns true if self is less than other" do
    (71.3 < 91.8).should == true
    (192.6 < -500).should == false
    (-0.12 < bignum_value).should == true
    (nan_value < 1.0).should == false
    (1.0 < nan_value).should == false
  end

  it "coerces the value if it is not a numeric" do
    (2.1 < FloatSpecs::CoerceToFloat.new).should == false
  end
end
