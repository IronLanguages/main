require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#hash" do
  it "has the property that a.eql?(b) implies a.hash == b.hash" do
    0x80000005.hash.should == 0x80000005.hash
    (0x80000000 + 5).hash.should == (0x8000000A - 5).hash
	0x80000000.hash.should_not == 0x80000005.hash
  end
end
