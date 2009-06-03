require File.dirname(__FILE__) + '/../../spec_helper'

describe "Float#<=>" do
  it "returns -1, 0, 1 when self is less than, equal, or greater than other" do
    (1.5 <=> 5).should == -1
    (2.45 <=> 2.45).should == 0
    ((bignum_value*1.1) <=> bignum_value).should == 1
  end

  it "returns nil when self is NaN" do
    (nan <=> 5).should == nil
    (nan <=> 2.45).should == nil
    (2.44 <=> nan).should == nil
    (nan <=> bignum_value).should == nil
  end

  it "returns nil when it can't be compared" do
    (1.0 <=> Class.new).should == nil
  end
end
