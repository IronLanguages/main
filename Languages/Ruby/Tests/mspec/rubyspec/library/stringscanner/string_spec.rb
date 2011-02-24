require File.dirname(__FILE__) + '/../../spec_helper'
require 'strscan'

describe "StringScanner#string" do
  before :each do
    @s = StringScanner.new("This is a test")
  end

  it "returns the string being scanned" do
    @s.string.should == "This is a test"
    @s << " case"
    @s.string.should == "This is a test case"
  end
end

describe "StringScanner#string=" do
  before :each do
    @s = StringScanner.new("This is a test")
  end

  it "changes the string being scanned to the argument and resets the scanner" do
    @s.scan(/\w+/)
    @s.pos.should == 4
    a = "Hello world"
    @s.string = a
    @s.string.should == "Hello world"
    @s.pos.should == 0
  end
end
