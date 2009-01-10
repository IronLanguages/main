require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#[]" do
  it "returns the nth bit in the binary representation of self" do
    (0x80001384)[2].should == 1
    (0x80001384)[9.2].should == 1
    (0x80001384)[21].should == 0
  end
  it "returns 0 if index is negative" do
    (0x800000000)[-10].should == 0
    (-0x800000000)[-10].should === 0
  end
  it "returns 1 if self is negative and the index is greater than the most significant bit" do
    (-0x80000000)[33].should == 1
    (-0x80000000)[0x80000000].should == 1
  end
  it "returns 0 if self is positive and the index is greater than the most significant bit" do
    (0x80000000)[33].should == 0
    (0x80000000)[0x80000000].should == 0
  end
  class X
    def to_int
     2
    end
  end
  it "converts index to integer using to_int" do
    (0x800012834)[X.new].should == 1
    (0x800012831)[X.new].should == 0
  end
  it "dynamically invokes [] after converting index to_int" do
    class Bignum
      alias :old_ref :[]
      def [](other)
        2
      end
    end
    (0x800012834)[X.new].should == 2
    (0x800012831)[X.new].should == 2
    class Bignum
      remove_method :[]
      alias :[] :old_ref
    end
  end
end
require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#[]" do
  it "returns the nth bit in the binary representation of self" do
    (0x80001384)[2].should == 1
    (0x80001384)[9.2].should == 1
    (0x80001384)[21].should == 0
  end
  it "returns 0 if index is negative" do
    (0x800000000)[-10].should == 0
    (-0x800000000)[-10].should === 0
  end
  it "returns 1 if self is negative and the index is greater than the most significant bit" do
    (-0x80000000)[33].should == 1
    (-0x80000000)[0x80000000].should == 1
  end
  it "returns 0 if self is positive and the index is greater than the most significant bit" do
    (0x80000000)[33].should == 0
    (0x80000000)[0x80000000].should == 0
  end
  class X
    def to_int
     2
    end
  end
  it "converts index to integer using to_int" do
    (0x800012834)[X.new].should == 1
    (0x800012831)[X.new].should == 0
  end
  it "dynamically invokes [] after converting index to_int" do
    class Bignum
      alias :old_ref :[]
      def [](other)
        2
      end
    end
    (0x800012834)[X.new].should == 2
    (0x800012831)[X.new].should == 2
    class Bignum
      remove_method :[]
      alias :[] :old_ref
    end
  end
end
