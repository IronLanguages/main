require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#method_missing" do
  before(:each) do
    @sstr = "a".to_clr_string 
  end

  it "doesn't allow mutating methods" do
    lambda { @sstr.chop! }.should raise_error TypeError
    lambda { @sstr[0] = "b" }.should raise_error TypeError
  end

  it "throws NoMethodError for methods that don't exist" do
    lambda { @sstr.foo }.should raise_error NoMethodError
  end
  
  it "throws NoMethodError for methods that don't exist that look like mutating methods" do
    lambda { @sstr.foo! }.should raise_error NoMethodError
    lambda { @sstr.foo="b" }.should raise_error NoMethodError 
  end
end
