require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

describe "Bignum#divmod" do
    it "returns self divided by other, where other is Fixnum" do
      (0x80000000).divmod(4)[0].should == 0x20000000
      (0x80000000).divmod(4)[1].should == 0
      (0x80000001).divmod(4)[0].should == 0x20000000
      (0x80000001).divmod(4)[1].should == 1
      (0x8000000D).divmod(4)[0].should == 0x20000003
      (0x8000000D).divmod(4)[1].should == 1
    end
    it "rounds toward minus infinity" do
      (0x8000000D).divmod(4)[0].should == 0x20000003
      (0x8000000D).divmod(4)[1].should == 1
      (-0x8000000D).divmod(4)[0].should == -0x20000004
      (-0x8000000D).divmod(4)[1].should == 3
      (0x8000000D).divmod(-4)[0].should == -0x20000004
      (0x8000000D).divmod(-4)[1].should == -3
      (-0x8000000D).divmod(-4)[0].should == 0x20000003
      (-0x8000000D).divmod(-4)[1].should == -1
    end
    it "returns self divided by other (rounded toward minus infinity), where other is Bignum" do
      (0x80000000).divmod(16.2)[0].should == 132560719
      (0x80000000).divmod(16.2)[1].should_be_close(0.2, TOLERANCE)
      (0x80000000).divmod(0x80000000)[0].should == 1
      (0x80000000).divmod(0x80000000)[1].should_be_close(0, TOLERANCE)
    end
    it "always produces an integer as the div part" do    
      (0x80000000).divmod(16.2)[0].class.to_s.should_not == 'Float'
    end
    it "normalizes values to Fixnum as necessary" do
      ((0x180000000).divmod(2))[0].class.to_s.should == 'Bignum'
      ((0x180000000).divmod(2))[1].class.to_s.should == 'Fixnum'
      ((0x8ffffffff).divmod(0x800000000))[0].class.to_s.should == 'Fixnum'
      ((0x8ffffffff).divmod(0x800000000))[1].class.to_s.should == 'Bignum'
    end
    it "raises ZeroDivisionError if other is zero and not a Float" do
      should_raise(ZeroDivisionError) { (0x80000000).divmod(0) }
    end
    it "raises FloatDomainError if other is zero and is a Float" do
      should_raise(FloatDomainError) { (0x80000000).divmod(0.0) }
      should_raise(FloatDomainError) { (0x80000000).divmod(-0.0) }
    end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.divmod(X.new)[0].should == 0x4c3b2a190
    0x987654321.divmod(X.new)[1].should == 1
    0x80000000.divmod(X.new)[0].should == 0x40000000
    0x80000000.divmod(X.new)[1].should == 0
  end
end
require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

describe "Bignum#divmod" do
    it "returns self divided by other, where other is Fixnum" do
      (0x80000000).divmod(4)[0].should == 0x20000000
      (0x80000000).divmod(4)[1].should == 0
      (0x80000001).divmod(4)[0].should == 0x20000000
      (0x80000001).divmod(4)[1].should == 1
      (0x8000000D).divmod(4)[0].should == 0x20000003
      (0x8000000D).divmod(4)[1].should == 1
    end
    it "rounds toward minus infinity" do
      (0x8000000D).divmod(4)[0].should == 0x20000003
      (0x8000000D).divmod(4)[1].should == 1
      (-0x8000000D).divmod(4)[0].should == -0x20000004
      (-0x8000000D).divmod(4)[1].should == 3
      (0x8000000D).divmod(-4)[0].should == -0x20000004
      (0x8000000D).divmod(-4)[1].should == -3
      (-0x8000000D).divmod(-4)[0].should == 0x20000003
      (-0x8000000D).divmod(-4)[1].should == -1
    end
    it "returns self divided by other (rounded toward minus infinity), where other is Bignum" do
      (0x80000000).divmod(16.2)[0].should == 132560719
      (0x80000000).divmod(16.2)[1].should_be_close(0.2, TOLERANCE)
      (0x80000000).divmod(0x80000000)[0].should == 1
      (0x80000000).divmod(0x80000000)[1].should_be_close(0, TOLERANCE)
    end
    it "always produces an integer as the div part" do    
      (0x80000000).divmod(16.2)[0].class.to_s.should_not == 'Float'
    end
    it "normalizes values to Fixnum as necessary" do
      ((0x180000000).divmod(2))[0].class.to_s.should == 'Bignum'
      ((0x180000000).divmod(2))[1].class.to_s.should == 'Fixnum'
      ((0x8ffffffff).divmod(0x800000000))[0].class.to_s.should == 'Fixnum'
      ((0x8ffffffff).divmod(0x800000000))[1].class.to_s.should == 'Bignum'
    end
    it "raises ZeroDivisionError if other is zero and not a Float" do
      should_raise(ZeroDivisionError) { (0x80000000).divmod(0) }
    end
    it "raises FloatDomainError if other is zero and is a Float" do
      should_raise(FloatDomainError) { (0x80000000).divmod(0.0) }
      should_raise(FloatDomainError) { (0x80000000).divmod(-0.0) }
    end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.divmod(X.new)[0].should == 0x4c3b2a190
    0x987654321.divmod(X.new)[1].should == 1
    0x80000000.divmod(X.new)[0].should == 0x40000000
    0x80000000.divmod(X.new)[1].should == 0
  end
end
