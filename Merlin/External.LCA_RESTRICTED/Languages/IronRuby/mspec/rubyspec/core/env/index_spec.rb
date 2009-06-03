require File.dirname(__FILE__) + '/../../spec_helper'

describe "ENV.index" do
  it "returns nil for nonexistant values" do
    ENV.index("foo").should == nil
  end

  it "returns the first key for the found value" do
    orig = ENV.to_hash
    begin
      ENV.clear
      ENV["1"] = "3"
      ENV.index("3").should == "1"
    ensure
      ENV.replace orig
    end
  end

  it "freezes its return value" do
    orig = ENV.to_hash
    begin
      ENV.clear
      ENV["1"] = "3"
      ENV["2"] = "3"
      ENV.index("3").frozen?.should == true
    ensure
      ENV.replace orig
    end
  end
end
