require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#-@" do
  it "negates self" do
    (0x80000000).send(:-@).should == (-0x80000000)
    (0x987654321).send(:-@).should == (-0x987654321)
    (-0x80000000).send(:-@).should == (0x80000000)
    (-0x987654321).send(:-@).should == (0x987654321)
  end
end
