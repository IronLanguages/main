require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#dump" do
  it "surrounds the string with ''" do
    "a".to_clr_string.dump.should == "'a'"
  end

  it "dumps empty strings" do
    "".to_clr_string.dump.should == "''"
  end
end
