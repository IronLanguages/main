require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#**" do
  it "returns self to the power of other, where other is Bignum" do
    (0x80000000 ** 0x80000000).to_s.should == 'Infinity'
    (0x80000000 ** -0x80000000).should == 0.0
    ((-0x80000000) ** 0x80000000).to_s.should == 'Infinity'
    ((-0x80000000) ** (-0x80000000)).should == 0.0
  end
  it "returns self to the power of other, where other is Fixnum" do
    (0x80000000 ** 5).should == 0x800000000000000000000000000000000000000
    (0x80000000 ** (-5)).should_be_close(2.18952885050753e-047, TOLERANCE)
    ((-0x80000000) ** 5).should == -0x800000000000000000000000000000000000000
    ((-0x80000000) ** (-5)).should_be_close(2.18952885050753e-047, TOLERANCE)
    (0x80000000 ** 1).should == 0x80000000
    (0x80000000 ** 0).should == 1
  end
  it "normalizes result to a Fixnum as necessary" do
    (0x80000000 ** 1).class.to_s.should == 'Bignum'
    (0x80000000 ** 0).class.to_s.should == 'Fixnum'
  end
  it "returns self to the power of other, where other is Float" do
    (0x80000000 ** 7.4).to_s.should == '1.13836361284227e+069'
    (0x80000000 ** -7.4).to_s.should == '8.78453939249866e-070'
    ((-0x80000000) ** 7.4).to_s.should == 'NaN'
    ((-0x80000000) ** (-7.4)).to_s.should == 'NaN'
    (0x80000000 ** 0.001).should_be_close(1.0217200827143, TOLERANCE)
    (0x80000000 ** 0.0).should_be_close(1.0, TOLERANCE)
    (0x80000000 ** 0.0).class.to_s.should == 'Float'
  end
  it "returns 1 if other is 0" do
    (0x80000000 ** 0).should == 1
    (0x80000000 ** 0).class.to_s.should == 'Fixnum'
    (0x80000000 ** 0.0).should == 1.0
    (0x80000000 ** 0.0).class.to_s.should == 'Float'
  end
  it "returns NaN if other is NaN" do
    (0x80000000 ** (0.0/0.0)).to_s.should == 'NaN'
  end
  it "returns NaN if self < 0 and other is not an Integer or Infinity" do
    ((-0x80000000) ** -1.2).to_s.should == 'NaN'
  end
  it "returns 0 if other is -Infinity" do
    (0x80000000 ** (-1.0/0.0)).should == 0
  end
  it "returns Infinity if other is Infinity" do
    (0x80000000 ** (1.0/0.0)).to_s.should == 'Infinity'
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    class X
      def coerce(other)
        [other, 2]
      end
    end
    (0x987654321 ** X.new).should == 0x5accbaad2cd7a44a41
    (0x80000000 ** X.new).should == 0x4000000000000000
  end
end

