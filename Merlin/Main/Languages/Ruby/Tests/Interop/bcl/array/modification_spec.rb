require File.dirname(__FILE__) + '/../../spec_helper'

describe "Modifying .NET arrays" do
  before :each do
    @array = System::Array.of(Fixnum).new(10)
  end

  it "doesn't dynamicly resize" do
    lambda {@array[10] = 1}.should raise_error(System::NotSupportedException)
  end
end
