require File.dirname(__FILE__) + '/../../spec_helper'

describe "C# decimal numbers" do
  it "should be able to check as equal to Float" do
    #regression test for [#19872] IComparableOps.CompareTo 
    #throws argument error when type is Decimal
    klass = Klass.new
    klass.my_decimal = 10
    klass.my_decimal.should == 10
    klass.my_decimal = 1.0
    klass.my_decimal.should == 1.0
  end
end
