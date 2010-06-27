require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::UInt32" do
  before(:each) do
    @size = NumericHelper.size_of_u_int32
  end
  
  it_behaves_like "A .NET numeric", System::UInt32
  it_behaves_like :numeric_size, System::UInt32
  it_behaves_like :numeric_conversion, System::UInt32
end
