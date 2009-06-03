require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"

describe "System::Byte" do
  csc <<-EOL
  public partial class NumericHelper {
    public static int SizeOfByte() {
      return sizeof(Byte);
    }
  }
  EOL
  before(:each) do
    @size = NumericHelper.size_of_byte
    @minvalue = 0
    @maxvalue = 255
  end

  it_behaves_like "A .NET numeric", System::Byte
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Byte
  it_behaves_like :numeric_size, System::Byte
end
