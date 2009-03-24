require File.dirname(__FILE__) + '/../../spec_helper'

describe "Creating a .NET array" do
  it "takes a generic parameter" do
    System::Array.of(Fixnum).new(1).should == [0]
  end

  it "takes a size parameter" do
    System::Array.of(Fixnum).new(5).should == [0,0,0,0,0]
  end

  it "can't be resized" do
    lambda {System::Array.of(Fixnum).new(1)[1] = 5}.should raise_error(System::NotSupportedException)
  end
end
