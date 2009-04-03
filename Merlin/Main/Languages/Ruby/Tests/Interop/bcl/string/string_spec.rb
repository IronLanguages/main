require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::String" do
  before :each do
    @a = System::Char::Parse("a")
    @b = System::Char::Parse("b")
    @aa = System::String.new(@a, 2)
    @bbb = System::String.new(@b, 3)
    @str = System::String
  end

  it "can be parsed" do
    @a.should equal_clr_string("a")
  end

  it "can have it's methods used" do
    @str.concat(@aa, @bbb).should equal_clr_string("aabbb")
    @str.compare(@aa, @bbb).should == -1
    @str.compare(@bbb, @aa).should == 1
    @str.compare(@aa, @aa).should == 0
  end
end
