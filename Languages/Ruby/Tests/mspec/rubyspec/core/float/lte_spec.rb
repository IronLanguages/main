require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'

describe "Float#<=" do
  it "returns true if self is less than or equal to other" do
    (2.0 <= 3.14159).should == true
    (-2.7183 <= -24).should == false
    (0.0 <= 0.0).should == true
    (9_235.9 <= bignum_value).should == true
    (nan_value <= 1.0).should == false
    (1.0 <= nan_value).should == false
  end

  it "coerces the value if it is not a numeric" do
    (2.1 <= FloatSpecs::CoerceToFloat.new).should == false
  end
end
