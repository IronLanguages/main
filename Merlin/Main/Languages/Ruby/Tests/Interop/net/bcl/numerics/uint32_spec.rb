require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'

describe "System::UInt32" do
  before(:each) do
    @size = NumericHelper.size_of_u_int32
    @minvalue = 0
    @maxvalue = 4294967295
  end
  
  it_behaves_like "A .NET numeric", System::UInt32
  it_behaves_like :numeric_size, System::UInt32
end
