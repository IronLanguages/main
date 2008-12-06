require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

@bignum_divide = shared "Bignum divide" do |cmd|
  it "returns self divided by other, where other is Fixnum" do
    (0x80000000).send(cmd, 4).should == 0x20000000
    (0x80000001).send(cmd, 4).should == 0x20000000
    (0x8000000D).send(cmd, 4).should == 0x20000003
  end
  it "rounds toward minus infinity" do
    (0x8000000D).send(cmd, 4).should == 0x20000003
    (-0x8000000D).send(cmd, 4).should == -0x20000004
    (0x8000000D).send(cmd, -4).should == -0x20000004
    (-0x8000000D).send(cmd, -4).should == 0x20000003
  end
  it "returns self divided by other (rounded toward minus infinity)" do
    (0x80000000).send(cmd, 16.2).should_be_close(132560719.012346, TOLERANCE)
    (0x80000000).send(cmd, 0x80000000).should == 1
  end
  it "invokes divmod directly, when other is Bignum" do
    class Bignum
      alias :old_divmod :divmod
      def divmod(other)
        [55,56]
      end
    end
    (0x80000000).send(cmd, 0x80000000).should_not == 55
    (0x80000000).old_divmod(0x80000000).should == [1,0]
    class Bignum
      remove_method :divmod
      alias :divmod :old_divmod
    end
  end
  it "normalizes values to Fixnum as necessary" do
    ((0x180000000).send(cmd, 2)).class.to_s.should == 'Bignum'
    (0x80000000).send(cmd, 0x80000000).class.to_s.should == 'Fixnum'
  end
  it "raises ZeroDivisionError if other is zero and not a Float" do
    should_raise(ZeroDivisionError) { (0x80000000).send cmd, 0 }
  end
  it "does NOT raise ZeroDivisionError if other is zero and is a Float" do
    (0x80000000).send(cmd, 0.0).to_s.should == 'Infinity'
    (0x80000000).send(cmd, -0.0).to_s.should == '-Infinity'
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.send(cmd, X.new).should == 0x4c3b2a190
    0x80000000.send(cmd, X.new).should == 0x40000000
  end
end

describe "Bignum#/" do
  it_behaves_like(@bignum_divide, :/)
end
describe "Bignum#div" do
  it_behaves_like(@bignum_divide, :div)
end

describe "Bignum#/ (dynamic behaviour)" do
  it "dynamically invokes the / method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_div :/
      def /(other)
        33
      end
    end
    (0x987654321.old_div(X.new)).should == 33
    (0x987654321.div(X.new)).should_not == 33
    (0x80000000.old_div(X.new)).should == 33
    (0x80000000.div(X.new)).should_not == 33
    (0x80000000.old_div(0x80000000)).should == 1
    (0x80000000.div(0x80000000)).should == 1
    class Bignum
      remove_method :/
      alias :/ :old_div
    end
  end
end

describe "Bignum#div (dynamic behaviour)" do
  it "dynamically invokes the div method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_div :div
      def div(other)
        33
      end
    end
    (0x987654321.old_div(X.new)).should == 33
    (0x987654321 / X.new).should_not == 33
    (0x80000000.old_div(X.new)).should == 33
    (0x80000000 / X.new).should_not == 33
    (0x80000000.old_div(0x80000000)).should == 1
    (0x80000000 / 0x80000000).should == 1
    class Bignum
      remove_method :div
      alias :div :old_div
    end
  end
end
require File.dirname(__FILE__) + '/../../spec_helper'

class X
  def coerce(other)
    [other, 2]
  end
end

@bignum_divide = shared "Bignum divide" do |cmd|
  it "returns self divided by other, where other is Fixnum" do
    (0x80000000).send(cmd, 4).should == 0x20000000
    (0x80000001).send(cmd, 4).should == 0x20000000
    (0x8000000D).send(cmd, 4).should == 0x20000003
  end
  it "rounds toward minus infinity" do
    (0x8000000D).send(cmd, 4).should == 0x20000003
    (-0x8000000D).send(cmd, 4).should == -0x20000004
    (0x8000000D).send(cmd, -4).should == -0x20000004
    (-0x8000000D).send(cmd, -4).should == 0x20000003
  end
  it "returns self divided by other (rounded toward minus infinity)" do
    (0x80000000).send(cmd, 16.2).should_be_close(132560719.012346, TOLERANCE)
    (0x80000000).send(cmd, 0x80000000).should == 1
  end
  it "invokes divmod directly, when other is Bignum" do
    class Bignum
      alias :old_divmod :divmod
      def divmod(other)
        [55,56]
      end
    end
    (0x80000000).send(cmd, 0x80000000).should_not == 55
    (0x80000000).old_divmod(0x80000000).should == [1,0]
    class Bignum
      remove_method :divmod
      alias :divmod :old_divmod
    end
  end
  it "normalizes values to Fixnum as necessary" do
    ((0x180000000).send(cmd, 2)).class.to_s.should == 'Bignum'
    (0x80000000).send(cmd, 0x80000000).class.to_s.should == 'Fixnum'
  end
  it "raises ZeroDivisionError if other is zero and not a Float" do
    should_raise(ZeroDivisionError) { (0x80000000).send cmd, 0 }
  end
  it "does NOT raise ZeroDivisionError if other is zero and is a Float" do
    (0x80000000).send(cmd, 0.0).to_s.should == 'Infinity'
    (0x80000000).send(cmd, -0.0).to_s.should == '-Infinity'
  end
  it "coerces on other if other is not Fixnum, Bignum or Float" do
    0x987654321.send(cmd, X.new).should == 0x4c3b2a190
    0x80000000.send(cmd, X.new).should == 0x40000000
  end
end

describe "Bignum#/" do
  it_behaves_like(@bignum_divide, :/)
end
describe "Bignum#div" do
  it_behaves_like(@bignum_divide, :div)
end

describe "Bignum#/ (dynamic behaviour)" do
  it "dynamically invokes the / method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_div :/
      def /(other)
        33
      end
    end
    (0x987654321.old_div(X.new)).should == 33
    (0x987654321.div(X.new)).should_not == 33
    (0x80000000.old_div(X.new)).should == 33
    (0x80000000.div(X.new)).should_not == 33
    (0x80000000.old_div(0x80000000)).should == 1
    (0x80000000.div(0x80000000)).should == 1
    class Bignum
      remove_method :/
      alias :/ :old_div
    end
  end
end

describe "Bignum#div (dynamic behaviour)" do
  it "dynamically invokes the div method if other is not a Fixnum, Bignum or Float" do
    class Bignum
      alias :old_div :div
      def div(other)
        33
      end
    end
    (0x987654321.old_div(X.new)).should == 33
    (0x987654321 / X.new).should_not == 33
    (0x80000000.old_div(X.new)).should == 33
    (0x80000000 / X.new).should_not == 33
    (0x80000000.old_div(0x80000000)).should == 1
    (0x80000000 / 0x80000000).should == 1
    class Bignum
      remove_method :div
      alias :div :old_div
    end
  end
end
