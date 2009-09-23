require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + "/../../shared/numeric"
require File.dirname(__FILE__) + "/../fixtures/classes"

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

describe "System::Decimal" do
  before(:each) do
    @size = NumericHelper.size_of_decimal
  end
  
  it_behaves_like "A .NET numeric", System::Decimal
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Decimal
  it_behaves_like :numeric_size, System::Decimal
  it_behaves_like :numeric_conversion, System::Decimal
end
