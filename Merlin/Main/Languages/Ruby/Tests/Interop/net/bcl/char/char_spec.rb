require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::Char" do
  it "can be created from a string" do
    System::Char.new("a").should be_kind_of System::Char
    System::Char.new("a".to_clr_string).should be_kind_of System::Char
  end

  it "can be created from a char array" do
    System::Char.new(System::Array.of(System::Char).new(1)).should be_kind_of System::Char
  end
  
  it "raises errors for empty strings" do
    lambda { System::Char.new("") }.should raise_error ArgumentError
    lambda { System::Char.new("".to_clr_string) }.should raise_error ArgumentError
    lambda { System::Char.new(System::Array.of(System::Char).new(0)) }.should raise_error ArgumentError
  end
end
