require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'

describe "System::Int32" do
  csc <<-EOL
  public partial class NumericHelper {
    public static int SizeOfInt32() {
      return sizeof(Int32);
    }
  }
  EOL
  before(:each) do
    @size = NumericHelper.size_of_int32
    @minvalue = -2147483648
    @maxvalue = 2147483647
  end
  
  it_behaves_like "A .NET numeric", System::Int32
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Int32

  it "is Fixnum" do
    System::Int32.should == Fixnum
  end

  it "has a size" do
    1.size.should == @size
  end
end
