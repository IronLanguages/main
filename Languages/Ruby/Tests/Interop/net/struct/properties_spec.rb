require File.dirname(__FILE__) + "/../spec_helper"

describe "Properties on structs" do
  before(:each) do
    @struct = StructWithMethods.new
  end

  it "allow getting and setting" do
    @struct.short_field.should == 0
    @struct.short_field = 42
    @struct.short_field.should == 42
  end
end
