require File.dirname(__FILE__) + "/../spec_helper"

describe "Structs" do
  csc <<-EOL
    public struct EmptyStruct {}
    public struct Struct { public int m1() {return 1;}}
  EOL
  it "map to Ruby classes" do
    [EmptyStruct, Struct].each do |e|
      e.should be_kind_of Class
    end
  end
end
