require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"

describe "System::Byte" do
  before(:each) do
    @size = NumericHelper.size_of_byte
    @minvalue = 0
    @maxvalue = 255
  end

  it_behaves_like "A .NET numeric", System::Byte
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Byte
  it_behaves_like :numeric_size, System::Byte
end
