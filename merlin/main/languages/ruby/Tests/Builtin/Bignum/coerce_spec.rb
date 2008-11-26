require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#coerce" do
  it "returns [other, self] both as Bignum if other is an Integer" do
    (0x80000001).coerce(2).class.to_s.should == 'Array'
    (0x80000001).coerce(2).length.should == 2
    (0x80000001).coerce(2)[0].should == 2
    (0x80000001).coerce(2)[1].should == 0x80000001
    (0x80000001).coerce(2)[0].class.to_s.should == 'Bignum'
    (0x80000001).coerce(2)[1].class.to_s.should == 'Bignum'
  end
  
  it "returns [Bignum, Bignum] if other is a Bignum" do
    (0x80000001).coerce(0x80000002).class.to_s.should == 'Array'
    (0x80000001).coerce(0x80000002).length.should == 2
    (0x80000001).coerce(0x80000002)[0].should == 0x80000002
    (0x80000001).coerce(0x80000002)[1].should == 0x80000001
    (0x80000001).coerce(0x80000002)[0].class.to_s.should == 'Bignum'
    (0x80000001).coerce(0x80000002)[1].class.to_s.should == 'Bignum'
  end
end
