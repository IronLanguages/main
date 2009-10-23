require File.dirname(__FILE__) + '/../../spec_helper'
require 'strscan'

describe "StringScanner#initialize" do
  before :each do
    @str = "This is a test"
    @s = StringScanner.new(@str)
  end

  it "is a private method" do
    @s.private_methods.should include("initialize")
  end

  it "returns an instance of StringScanner" do
    @s.should be_kind_of(StringScanner)
    @s.tainted?.should be_false
    @s.eos?.should be_false
  end

  it "calls to_str on it's argument" do
    s = mock('to_s')
    s.should_receive(:to_str).and_return("bob")
    StringScanner.new(s)
  end

  it "ignores second parameter" do
    str = '123'
    s = StringScanner.new(str, true)
    s.string.object_id.should == str.object_id
    s = StringScanner.new(str, false)
    s.string.object_id.should == str.object_id
  end

  it "holds the passed string by reference" do
    @s.string.object_id.should == @str.object_id
  end
end
