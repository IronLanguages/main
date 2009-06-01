require File.dirname(__FILE__) + '/../spec_helper'

describe "Getting enumeration values" do
  it "as methods" do
    meth = EnumInt.method(:a)
    meth.should be_kind_of(Method)
    meth.call.value__.should == EnumInt.A.value__
  end
  
  # This test is purely testing the .NET framework, but I think it serves a
  # valuble documentation purpose
  it "as an array" do
    System::Enum.get_names(EnumInt.to_clr_type).map { |e| e.to_s }.should == ['A','B','C']
  end
end