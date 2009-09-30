require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::String#include?" do
  before(:each) do
    @str = "string".to_clr_string
  end

  it "takes System::Strings" do
    @str.include?("rin".to_clr_string).should be_true
  end

  it "takes Ruby Strings" do
    @str.include?("rin").should be_true
  end

  it "takes System::Char's" do
    @str.include?(System::Char.new("i")).should be_true
  end
end
