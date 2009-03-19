require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overload resolution" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end

  it "is performed" do
    @methods.call(100).to_s.should == "one arg"
    @methods.call(100, 100).to_s.should == "two args"
  end
end

describe "Selecting .NET overloads" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end
  
  it "is allowed" do
    @methods.overloads(Fixnum,Fixnum).call(100,100).to_s.should == "two args"
  end

  it "correctly reports error message" do
    #regression test for RubyForge 24112
    lambda {@methods.overloads(Fixnum).call}.should raise_error(ArgumentError, /0 for 1/)
  end
end

