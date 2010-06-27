require File.dirname(__FILE__) + '/../../spec_helper'

describe "Mixed TypeGroups (with non-generic member)" do
  it "allow static methods to be called on the non-generic member" do
    #regression for RubyForge 24106
    lambda {StaticMethodTypeGroup.Return(100)}.should_not raise_error
    StaticMethodTypeGroup.Return(100).should == 100
  end

  it "allow both generic and nongeneric members" do
    lambda {System::Nullable}.should_not raise_error
    lambda {System::Nullable.of(System::Int32)}.should_not raise_error
  end
end
