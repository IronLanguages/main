require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::UInt16" do
  before(:each) do
    @size = NumericHelper.size_of_u_int16
  end

  it_behaves_like "A .NET numeric", System::UInt16
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::UInt16
  it_behaves_like :numeric_size, System::UInt16
  it_behaves_like :numeric_conversion, System::UInt16
end
