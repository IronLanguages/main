require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'

describe "System::Int64" do
  before(:each) do
    @size = NumericHelper.size_of_int64
    @minvalue = -9223372036854775808
    @maxvalue = 9223372036854775807
  end
  
  it_behaves_like "A .NET numeric", System::Int64
  it_behaves_like :numeric_size, System::Int64
end
