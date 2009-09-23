require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::SByte" do
  before(:each) do
    @size = NumericHelper.size_of_s_byte
  end

  it_behaves_like "A .NET numeric", System::SByte
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::SByte
  it_behaves_like :numeric_size, System::SByte
  it_behaves_like :numeric_conversion, System::SByte
end
