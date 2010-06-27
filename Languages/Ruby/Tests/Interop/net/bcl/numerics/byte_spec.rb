require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::Byte" do
  before(:each) do
    @size = NumericHelper.size_of_byte
  end

  it_behaves_like "A .NET numeric", System::Byte
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Byte
  it_behaves_like :numeric_size, System::Byte
  it_behaves_like :numeric_conversion, System::Byte
end
