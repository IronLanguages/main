require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

@bignum_modulo = shared "Bignum modulo" do |cmd|
  it "returns self modulo by other, where other is Fixnum" do
	  (0x80000000).send(cmd, 4).should == 0
	  (0x80000001).send(cmd, 4).should == 1
	  (0x8000000D).send(cmd, 4).should == 1
  end
  it "rounds toward minus infinity" do
	  (0x8000000D).send(cmd, 4).should == 1
	  (-0x8000000D).send(cmd, 4).should == 3
	  (0x8000000D).send(cmd, -4).should == -3
	  (-0x8000000D).send(cmd, -4).should == -1
  end
  it "returns self modulo other, where other is Bignum" do
	  (0x80000000).send(cmd, 0x80000000).should == 0
  end
	it "returns self modulo other, where other is Float" do
	  (0x80000000).send(cmd, 16.2).should_be_close(0.2, TOLERANCE)
	end
	it "normalizes values to Fixnum as necessary" do
	  ((0x180000000).send(cmd, 2)).class.to_s.should == 'Fixnum'
	  ((0x8ffffffff).send(cmd, 0x800000000)).class.to_s.should == 'Bignum'
	end
  it "invokes divmod directly, when other is Bignum" do
    class Bignum
      alias :old_divmod :divmod
      def divmod(other)
        [55,56]
      end
    end
    (0x80000000).send(cmd, 0x80000000).should_not == 56
    (0x80000000).old_divmod(0x80000000).should == [1,0]
    class Bignum
      remove_method :divmod
      alias :divmod :old_divmod
    end
  end
	it "raises ZeroDivisionError if other is zero and not a Float" do
	  should_raise(ZeroDivisionError) { (0x80000000).send(cmd, 0) }
	end
	it "raises FloatDomainError if other is zero and is a Float" do
	  (0x80000000).send(cmd, 0.0).to_s.should == 'NaN'
	  (0x80000000).send(cmd, -0.0).to_s.should == 'NaN'
	end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.send(cmd, X.new).should == 1
    0x80000000.send(cmd, X.new).should == 0
  end
end

describe "Bignum#%" do
  it_behaves_like(@bignum_modulo, :%)
end

describe "Bignum#modulo" do
  it_behaves_like(@bignum_modulo, :modulo)
end

describe "Bignum#% (dynamic behaviour)" do
  it "dynamically invokes the % method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_mod :%
      def %(other)
        33
      end
    end
    (0x987654321.old_mod(X.new)).should == 33
    (0x987654321.modulo(X.new)).should_not == 33
    (0x80000000.old_mod(X.new)).should == 33
    (0x80000000.modulo(X.new)).should_not == 33
    (0x80000000.old_mod(0x80000000)).should == 0
    (0x80000000.modulo(0x80000000)).should == 0
    class Bignum
      remove_method :%
      alias :% :old_mod
    end
  end
end

describe "Bignum#modulo (dynamic behaviour)" do
  it "dynamically invokes the modulo method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_mod :modulo
      def modulo(other)
        33
      end
    end
    (0x987654321.old_mod(X.new)).should == 33
    (0x987654321 % X.new).should_not == 33
    (0x80000000.old_mod(X.new)).should == 33
    (0x80000000 % X.new).should_not == 33
    (0x80000000.old_mod(0x80000000)).should == 0
    (0x80000000 % 0x80000000).should == 0
    class Bignum
      remove_method :modulo
      alias :modulo :old_mod
    end
  end
end
