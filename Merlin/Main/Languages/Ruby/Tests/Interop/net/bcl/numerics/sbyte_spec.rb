require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"

describe "System::SByte" do
  before(:each) do
    @size = NumericHelper.size_of_s_byte
    @minvalue = -128
    @maxvalue = 127
  end

  it_behaves_like "A .NET numeric", System::SByte
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::SByte
  it_behaves_like :numeric_size, System::SByte
end
