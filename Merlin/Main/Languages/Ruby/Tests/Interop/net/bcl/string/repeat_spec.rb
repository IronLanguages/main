require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#*" do
  before(:each) do
    @sstr = "a".to_clr_string
  end
  
  it "repeats the string the a number of times equal to the multiplier" do
    (@sstr * 1).should equal_clr_string("a")
    (@sstr * 0).should equal_clr_string("")
    (@sstr * 2).should equal_clr_string("aa")
  end

  it "raises an ArgumentError for negative values" do
    lambda { @sstr * -1 }.should raise_error ArgumentError
  end
end
