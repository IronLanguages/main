require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::Char#inspect" do
  it "Outputs a ' delimited string with a (Char) annotation" do
    "a".to_clr_string.inspect.should == "'a' (Char)"
    "\n".to_clr_string.inspect.should == "'\n' (Char)"
  end
end
