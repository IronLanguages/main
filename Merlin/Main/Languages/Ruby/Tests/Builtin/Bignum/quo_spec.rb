require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#quo" do
  it "returns the floating-point result of self divided by other, where other is BigInteger" do
    (0x80000005.quo(0x80000000)).should_be_close(1.00000000232831, TOLERANCE)
    (0x80000000.quo(0x80000000)).should_be_close(1.0, TOLERANCE)
  end
  it "returns the floating-point result of self divided by other, where other is Fixnum" do
    (0x80000000.quo(13)).should_be_close(165191049.846154, TOLERANCE)
  end
  it "returns the floating-point result of self divided by other, where other is Float" do
    (0x80000000.quo(2.5)).should_be_close(858993459.2, TOLERANCE)
  end
  it "does NOT raise an exception when other is zero" do
    (0x80000000.quo(0.0)).to_s.should == "Infinity"
    (0x80000000.quo(-0.0)).to_s.should == "-Infinity"
    (0x80000000.quo(0)).to_s.should == "Infinity"
    (0x80000000.quo(-0)).to_s.should == "Infinity"
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    class X
      def coerce(other)
        [other, 2]
      end
    end
    (0x987654321.quo(X.new)).should == 20463133072.5
    (0x80000000.quo(X.new)).should == 1073741824.0
  end
end
