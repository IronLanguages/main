require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#^" do
  it "returns self bitwise-xor other, where other is Fixnum" do
    (0x80000010 ^ 3).should == 0x80000013
    (0x80000005 ^ 52).should == 0x80000031
  end
  it "return self bitwise-xor other, where other is Bignum" do
    (0x80000005 ^ 0x80000034).should == 0x31
  end
  it "returns self bitwise-xor other, where other is float (downcast to int using Float.floor)" do
    (0x80000005 ^ 3.4).should == 0x80000006
  end
  it "returns self bitwise-xor other, where self or other are negative" do
    (0x80000001 ^ -1).should == -0x80000002
    (-0x80000001 ^ 1).should == -0x80000002
    (-0x80000001 ^ -1).should == 0x80000000
  end
  it "returns self bitwise-xor other, normalizing values to Fixnum as necessary" do
    (0x80000005 ^ 52).class.to_s.should == 'Bignum'
    (0x80000005 ^ 0x80000034).class.to_s.should == 'Fixnum'
  end
  it "returns self bitwise-xor other, calling Float.to_int dynamically" do
    # Redefine Float#to_int
    class Float
     alias :old_to_int :to_int
     def to_int
       55
     end
    end
    
    (0x80000005 ^ 3.4).should == 0x80000032
    
    # Clear the redefined to_int
    class Float
     alias :dead_to_int :to_int
     alias :to_int :old_to_int
    end
  end
end
