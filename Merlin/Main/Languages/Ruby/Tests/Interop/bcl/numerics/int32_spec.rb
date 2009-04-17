require File.dirname(__FILE__) + '/../../spec_helper'

describe "Int32" do
  it "is Fixnum" do
    System::Int32.should == Fixnum
  end
end