require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

describe "Bignum#remainder" do
	it "returns remainder from self divided by other, where other is Fixnum" do
	  (0x80000000).remainder(4).should == 0
	  (0x80000001).remainder(4).should == 1
	  (0x8000000D).remainder(4).should == 1
	end
	it "does not round toward minus infinity" do
	  (0x8000000D).remainder(4).should == 1
	  (-0x8000000D).remainder(4).should == -1
	  (0x8000000D).remainder(-4).should == 1
	  (-0x8000000D).remainder(-4).should == -1
	  (0x8000000D).remainder(4.0).should == 1
	  (-0x8000000D).remainder(4.0).should == -1
	  (0x8000000D).remainder(-4.0).should == 1
	  (-0x8000000D).remainder(-4.0).should == -1
	end
	it "returns remainder from self divided by other, where other is Bignum" do
	  (0x80000000).remainder(0x80000000).should == 0
	end
	it "returns remainder from self divided by other, where other is Float" do
	  (0x80000000).remainder(16.2).should_be_close(0.2, TOLERANCE)
	end
	it "normalizes values to Fixnum as necessary" do
	  ((0x180000000).remainder(2)).class.to_s.should == 'Fixnum'
	  ((0x8ffffffff).remainder(0x800000000)).class.to_s.should == 'Bignum'
	end

  it "raises ZeroDivisionError if other is zero and not a Float" do
    should_raise(ZeroDivisionError) { 0x80000000.remainder(0) }
  end
  
  it "does NOT raise ZeroDivisionError if other is zero and is a Float" do
    (0x80000000.remainder(0.0)).to_s.should == 'NaN'
    (0x80000000.remainder(-0.0)).to_s.should == 'NaN'
  end

  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.remainder(X.new).should == 1
    0x80000000.remainder(X.new).should == 0
  end
  it "dynamically invokes the remainder method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_rem :remainder
      def remainder(other)
        33
      end
    end
    (0x987654321.old_rem(X.new)).should == 33
    (0x80000000.old_rem(X.new)).should == 33
    (0x80000000.old_rem(0x80000000)).should == 0
    class Bignum
      remove_method :remainder
      alias :remainder :old_rem
    end
  end
end
