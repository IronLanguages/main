require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#inspect" do
  it "Outputs a ' delimited string" do
    "".to_clr_string.inspect.should == "''"
    "a".to_clr_string.inspect.should == "'a'"
    "a word".to_clr_string.inspect.should == "'a word'"
    "some\nlines\"".to_clr_string.inspect.should == "'some\\nlines\"'"
  end
end
