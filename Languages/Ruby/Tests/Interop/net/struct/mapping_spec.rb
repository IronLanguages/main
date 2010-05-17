require File.dirname(__FILE__) + "/../spec_helper"

describe "Structs" do
  it "map to Ruby classes" do
    [EmptyStruct, CStruct].each do |e|
      e.should be_kind_of Class
    end
  end
end
