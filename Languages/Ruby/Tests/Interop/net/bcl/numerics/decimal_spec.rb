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
  it_behaves_like :numeric_size, System::Decimal
  it_behaves_like :numeric_conversion, System::Decimal
  
  it "can be induced via an int" do
    a = @class.induced_from(Fixnum.MinValue)
    b = @class.induced_from(Fixnum.MaxValue)
    a.should be_kind_of System::Decimal
    b.should be_kind_of System::Decimal
  end
  
  it "properly initializes and compares" do
    a = System::Decimal.new 0.5
    a.should == 0.5
    a.should_not == 0

  end

  it "properly parses and compares" do
    b = System::Decimal.parse '1.5'
    b.should == 1.5
    b.should_not == 1
  end
end
