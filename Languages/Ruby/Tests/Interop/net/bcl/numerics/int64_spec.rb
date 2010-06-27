require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::Int64" do
  before(:each) do
    @size = NumericHelper.size_of_int64
  end
  
  it_behaves_like "A .NET numeric", System::Int64
  it_behaves_like :numeric_size, System::Int64
  it_behaves_like :numeric_conversion, System::Int64
end
