require File.dirname(__FILE__) + "/../../spec_helper"

describe "System::Char#inspect" do
  it "Outputs a ' delimited string with a (Char) annotation" do
    System::Char.new("a").inspect.should == "'a' (Char)"
    System::Char.new("\n").inspect.should == "'\\n' (Char)"
  end
end
