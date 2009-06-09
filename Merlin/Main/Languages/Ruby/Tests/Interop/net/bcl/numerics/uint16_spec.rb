require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"

describe "System::UInt16" do
  csc <<-EOL
  public partial class NumericHelper {
    public static int SizeOfUInt16() {
      return sizeof(UInt16);
    }
  }
  EOL
  before(:each) do
    @size = NumericHelper.size_of_u_int16
    @minvalue = 0
    @maxvalue = 65535
  end

  it_behaves_like "A .NET numeric", System::UInt16
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::UInt16
  it_behaves_like :numeric_size, System::UInt16
end
