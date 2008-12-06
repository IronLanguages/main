require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

describe "Bignum#-" do
  it "returns self plus other, where other is Bignum" do
    (0x80000000 - 0x80000000).should == 0
    (0x80000000 - (-0x80000000)).should == 0x100000000
    ((-0x80000000) - 0x80000000).should == -0x100000000
    ((-0x80000000) - (-0x80000000)).should == 0
  end
  it "returns self plus other, where other is Fixnum" do
    (0x80000000 - 5).should == 0x7ffffffb
    (0x80000000 - (-5)).should == 0x80000005
    ((-0x80000000) - 5).should == -0x80000005
    ((-0x80000000) - (-5)).should == -0x7ffffffb
    (0x80000000 - 1).should == 0x7fffffff
    (0x80000000 - 0).should == 0x80000000
  end
  it "normalizes result to a Fixnum as necessary" do
    (0x80000000 - 0).class.to_s.should == 'Bignum'
    (0x80000000 - 0x80000000).class.to_s.should == 'Fixnum'
  end
  it "returns self plus other, where other is Float" do
    (0x80000000 - 7.4).should_be_close(2147483640.6, TOLERANCE)
    (0x80000000 - (-7.4)).should_be_close(2147483655.4, TOLERANCE)
    ((-0x80000000) - 7.4).should_be_close(-2147483655.4, TOLERANCE)
    ((-0x80000000) - (-7.4)).should_be_close(-2147483640.6, TOLERANCE)
    (0x80000000 - 0.001).should_be_close(2147483647.999, TOLERANCE)
    (0x80000000 - 0.0).should_be_close(2147483648.0, TOLERANCE)
    (0x80000000 - 0.0).class.to_s.should == 'Float'
  end
  it "returns NaN if other is NaN" do
    (0x80000000 - (0.0/0.0)).to_s.should == 'NaN'
  end
  it "returns Infinity if other is -Infinity" do
    (0x80000000 - (-1.0/0.0)).to_s.should == 'Infinity'
  end
  it "returns -Infinity if other is Infinity" do
    (0x80000000 - (1.0/0.0)).to_s.should == '-Infinity'
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    (0x987654321 - X.new).should == 0x98765431f
    (0x80000000 - X.new).should == 0x7ffffffe
  end
  it "dynamically invokes the - method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_sub :-
      def -(other)
        33
      end
    end
    (0x987654321.old_sub(X.new)).should == 33
    (0x80000000.old_sub(X.new)).should == 33
    (0x80000000.old_sub(0)).should == 0x80000000
    class Bignum
      remove_method :-
      alias :- :old_sub
    end
  end
end
