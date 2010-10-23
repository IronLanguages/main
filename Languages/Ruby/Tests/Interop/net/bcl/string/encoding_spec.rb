require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::String#encoding" do
  it "returns utf-8" do
    "a".to_clr_string.encoding.to_s.should == "utf-8"
  end
end
