require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#&" do
  it "returns self bitwise-and other, where other is Fixnum" do
    (0x80000010 & 3).should == 0
    (0x80000005 & 52).should == 4
  end
  it "return self bitwise-and other, where other is Bignum" do
    (0x80000005 & 0x80000005).should == 0x80000005
  end
  it "returns self bitwise-and other, where other is float (downcast to int using Float.floor)" do
    (0x80000005 & 3.4).should == 1
  end
  it "returns self bitwise-and other, where self or other are negative" do
    (0x80000001 & -1).should == 0x80000001
    (-0x80000001 & 1).should == 1
    (-0x80000001 & -1).should == -0x80000001
  end
  it "normalizes values to Fixnum as necessary" do
    (0x80000005 & 0x80000001).class.to_s.should == 'Bignum'
    (0x80000005 & 5).class.to_s.should == 'Fixnum'
  end
  it "returns self bitwise-and other, calling Float.to_int dynamically" do
    # Redefine Float#to_int
    class Float
     alias :old_to_int :to_int
     def to_int
       55
     end
    end
    
    (0x80000005 & 3.4).should == 5
    
    # Clear the redefined to_int
    class Float
     alias :dead_to_int :to_int
     alias :to_int :old_to_int
    end
  end
end
