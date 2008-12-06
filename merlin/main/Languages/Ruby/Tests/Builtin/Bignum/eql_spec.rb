require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#eql?" do
  it "returns true if other is a Bignum with the same value, where other is Bignum" do
    (0x80000000).eql?(0x80000000).should == true
    (0x987654321).eql?(0x987654321).should == true
    (0x80000000).eql?(0x987654321).should_not == true
    (0x80000000).eql?(-0x80000000).should_not == true
    (-0x987654321).eql?(0x987654321).should_not == true
  end
  it "returns false if other is a Fixnum" do
    (0x80000000).eql?(4).should_not == true
    (0x80000000).eql?(-5).should_not == true
  end
  it "returns false if other is a Float" do
    (0x80000000).eql?(3.14).should_not == true
    (0x80000000).eql?(2147483648.0).should_not == true
  end
  it "returns false if other is a not a Fixnum, Bignum or Float" do
    class X
    end
    (0x80000000).eql?(X.new).should_not == true
    (0x80000000).eql?(X.new).should_not == true
  end
end
