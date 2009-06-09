require File.dirname(__FILE__) + '/spec_helper'

describe "String" do
  before :each do
    @str = "This is a fairly long string to test with!"
  end

  it "should default to appending ..." do
    @str.truncate(5).should == "Th..."
  end

  it "should default to a length of 30" do
    @str.truncate().should == "This is a fairly long strin..."
  end

  it "should truncate to a given length with a given suffix" do
    @str.truncate(15, "--more").should == "This is a--more"
  end
end
