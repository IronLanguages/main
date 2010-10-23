require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#format" do
  it "handles basic formating" do
    ("%0.3f".to_clr_string % 1.0).should equal_clr_string("1.000")
  end

  it "returns a System::String" do
    ("%0.3f".to_clr_string % 1.0).should be_kind_of System::String
  end
end
