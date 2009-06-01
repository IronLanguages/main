require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::String#" do
  describe "<=>" do
    before(:each) do
      @sstr = "a".to_clr_string
    end

    it "compares to System::Strings" do
      (@sstr <=> "b".to_clr_string).should == -1
    end

    it "compares to MutableStrings" do
      (@sstr <=> "b").should == -1
    end

    it "compares to arbitrary Objects" do
      (@sstr <=> Object.new).should == nil
    end
  end

  describe "==" do
    before(:each) do
      @sstr = "a".to_clr_string
    end

    it "compares to System::Strings" do
      (@sstr == "b".to_clr_string).should == false
      (@sstr == "a".to_clr_string).should == true
    end

    it "compares to MutableStrings" do
      (@sstr == "b").should == false
      (@sstr == "a").should == true
    end

    it "compares to arbitrary Objects" do
      (@sstr ==  Object.new).should == nil
    end
  end

  describe "empty?" do
    it "reports if the string is empty" do
      "a".to_clr_string.empty?.should == false
      "".to_clr_string.empty?.should == true
    end
  end
end
