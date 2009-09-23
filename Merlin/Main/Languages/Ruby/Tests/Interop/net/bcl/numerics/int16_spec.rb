require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../../shared/numeric"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::Int16" do
  before(:each) do
    @size = NumericHelper.size_of_int16
  end

  it_behaves_like "A .NET numeric", System::Int16
  it_behaves_like "A .NET numeric, induceable from Fixnum", System::Int16
  it_behaves_like :numeric_size, System::Int16
  it_behaves_like :numeric_conversion, System::Int16
end
