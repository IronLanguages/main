require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#~" do
  it "returns self bitwise inverted" do
    (~0x80000000).should == -0x80000001
    (~(-0x80000000)).should == 0x7fffffff
  end
  
  it "normalizes to a fixnum if necessary" do
    (~0x80000001).class.to_s.should == 'Bignum'
	#This test is dependent upon FIXNUM_MAX
    (~(-0x80000000)).class.to_s.should == 'Fixnum' if 0x7fffffff.class.to_s == 'Fixnum'
  end
end
