require File.dirname(__FILE__) + '/../../spec_helper'

describe "Modifying .NET arrays" do
  before :each do
    @array = [10].to_clr_array(Fixnum)
  end

  it "doesn't dynamicly resize" do
    lambda {@array[10] = 1}.should raise_error(IndexError)
  end
end
