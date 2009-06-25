require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::Text::StringBuilder" do
  it "creates a .NET StringBuilder object" do
    str = System::Text::StringBuilder.new
    str.append("abc")
    str.append("def")
    str.append(100)
    str.insert(3, "012")

    str.to_string.should equal_clr_string("abc012def100")

    str.capacity = 20
    str.capacity.should == 20
    str.length.should == 12
  end
end
