require File.dirname(__FILE__) + "/../../spec_helper"

describe "Calling methods on a non-visible type" do
  it "works" do
    type = System::Type.get_type("System.Int32".to_clr_string)
    type.full_name.should equal_clr_string "System.Int32"
    type.is_assignable_from(type).should == true
  end
end
