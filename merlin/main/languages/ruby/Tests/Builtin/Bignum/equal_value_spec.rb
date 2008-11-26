require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#==" do
  it "should true if self has the same value as other" do
    (0x80000000 == 0x80000000).should == true
    (0x987654321 == 0x987654321).should == true
    (0x80000000 == 0x80000003).should_not == true
    (0x80000000 == -0x80000000).should_not == true
    (0x80000000 == 2147483648.0).should == true
    (0x987654321 == 5.4).should_not == true
    (0x800000000 == 121).should_not == true
  end

  it "calls 'other == self' if coercion fails" do
    class X
      def ==(other)
        0x80000000 == other
      end
    end
    (0x987654321 == X.new).should == false
    (0x80000000 == X.new).should == true
  end
end
