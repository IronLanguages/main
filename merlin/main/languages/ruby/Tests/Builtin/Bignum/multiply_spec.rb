require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

describe "Bignum#*" do
  it "returns self multiplied by other, where other is Bignum" do
    (0x80000000 * 0x80000000).should == 0x4000000000000000
    (0x80000000 * -0x80000000).should == -0x4000000000000000
    (-0x80000000 * 0x80000000).should == -0x4000000000000000
    (-0x80000000 * -0x80000000).should == 0x4000000000000000
  end
  it "returns self multiplied by other, where other is Fixnum" do
    (0x80000000 * 5).should == 0x280000000
    (0x80000000 * -5).should == -0x280000000
    (-0x80000000 * 5).should == -0x280000000
    (-0x80000000 * -5).should == 0x280000000
    (0x80000000 * 1).should == 0x80000000
    (0x80000000 * 0).should == 0
  end
  it "normalizes result to a Fixnum as necessary" do
    (0x80000000 * 0).class.to_s.should == 'Fixnum'
  end
  it "returns self multiplied by other, where other is Float" do
    (0x80000000 * 7.4).should_be_close(15891378995.2, TOLERANCE)
    (0x80000000 * -7.4).should_be_close(-15891378995.2, TOLERANCE)
    (-0x80000000 * 7.4).should_be_close(-15891378995.2, TOLERANCE)
    (-0x80000000 * -7.4).should_be_close(15891378995.2, TOLERANCE)
    (0x80000000 * 0.001).should_be_close(2147483.648, TOLERANCE)
    (0x80000000 * 0.0).should_be_close(0.0, TOLERANCE)
    (0x80000000 * 0.0).class.to_s.should == 'Float'
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    (0x987654321 * X.new).should == 0x130eca8642
    (0x80000000 * X.new).should == 0x100000000
  end
  it "dynamically invokes the * method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_mult :*
      def *(other)
        33
      end
    end
    (0x987654321.old_mult(X.new)).should == 33
    (0x80000000.old_mult(X.new)).should == 33
    (0x80000000.old_mult(0)).should == 0
    class Bignum
      remove_method :*
      alias :* :old_mult
    end
  end
end
