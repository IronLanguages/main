require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#to_clr_string" do
  it "returns self" do
    a = "a".to_clr_string
    a.to_clr_string.object_id.should == a.object_id
  end
end

describe "String#to_clr_string" do
  it "returns a System::String equivalent to self" do
    a = "a".to_clr_string
    a.should be_kind_of System::String
    a.should == "a"
  end
end
