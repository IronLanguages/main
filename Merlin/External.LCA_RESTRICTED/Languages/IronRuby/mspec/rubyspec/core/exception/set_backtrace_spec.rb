require File.dirname(__FILE__) + '/../../spec_helper'

describe "Exception#set_backtrace" do
  it "allows the user to set the backtrace to any array" do
    err = RuntimeError.new
    err.set_backtrace ["unhappy"]
    err.backtrace.should == ["unhappy"]
  end

  it "allows the user to set the backtrace to a string" do
    err = RuntimeError.new
    err.set_backtrace "unhappy"
    err.backtrace.should == ["unhappy"]
  end

  it "returns the new backtrace" do
    RuntimeError.new.set_backtrace("unhappy").should == ["unhappy"]
  end

  it "doesn't allow the user to set the backtrace to a array of non-strings" do
    err = RuntimeError.new
    lambda { err.set_backtrace [1] }.should raise_error TypeError
  end
end
