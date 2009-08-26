require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Float#>=" do
  it "returns true if self is greater than or equal to other" do
    (5.2 >= 5.2).should == true
    (9.71 >= 1).should == true
    (5.55382 >= bignum_value).should == false
    (nan_value >= 1.0).should == false
    (1.0 >= nan_value).should == false
  end

  it "coerces the value if it is not a numeric" do
    (2.1 >= FloatSpecs::CoerceToFloat.new).should == true
  end
end
