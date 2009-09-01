require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::Int32" do
  before(:each) do
    @size = NumericHelper.size_of_int32
  end
  
  it_behaves_like "A .NET numeric", System::Int32
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Int32
  it_behaves_like :numeric_size, System::Int32
  it_behaves_like :numeric_conversion, System::Int32
  
  it "is Fixnum" do
    System::Int32.should == Fixnum
  end
end
