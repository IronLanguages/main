require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Float#>" do
  it "returns true if self is greater than other" do
    (1.5 > 1).should == true
    (2.5 > 3).should == false
    (45.91 > bignum_value).should == false
    (nan_value > 1.0).should == false
    (1.0 > nan_value).should == false
  end

  it "coerces the value if it is not a numeric" do
    (2.1 > FloatSpecs::CoerceToFloat.new).should == true
  end
end
