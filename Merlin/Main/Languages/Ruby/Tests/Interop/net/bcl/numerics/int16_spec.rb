require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"

describe "System::Int16" do
  csc <<-EOL
  public partial class NumericHelper {
    public static int SizeOfInt16() {
      return sizeof(Int16);
    }
  }
  EOL
  before(:each) do
    @size = NumericHelper.size_of_int16
    @minvalue = -32768
    @maxvalue = 32767
  end

  it_behaves_like "A .NET numeric", System::Int16
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Int16
  it_behaves_like :numeric_size, System::Int16
end
