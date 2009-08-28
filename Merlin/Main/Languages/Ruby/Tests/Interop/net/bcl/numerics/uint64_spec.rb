require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'

describe "System::UInt64" do
  before(:each) do
    @size = NumericHelper.size_of_u_int64
    @minvalue = 0
    @maxvalue = 18446744073709551615
  end
  
  it_behaves_like "A .NET numeric", System::UInt64
  it_behaves_like :numeric_size, System::UInt64
end
