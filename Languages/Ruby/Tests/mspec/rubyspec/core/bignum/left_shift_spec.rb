require File.expand_path('../../../spec_helper', __FILE__)

describe "Bignum#<< with n << m" do
  before(:each) do
    @bignum = bignum_value() * 16
  end

  it "returns n shifted left m bits when n > 0, m > 0" do
    (@bignum << 4).should == 2361183241434822606848
  end

  it "returns n shifted left m bits when n < 0, m > 0" do
    (-@bignum << 9).should == -75557863725914323419136
  end

  it "returns n shifted right m bits when n > 0, m < 0" do
    (@bignum << -1).should == 73786976294838206464
  end

  it "returns self shifted the given amount of bits to the left" do
    (@bignum << 4).should == 147573952589676413072
    (@bignum << 9).should == 4722366482869645218304
  end
  
  it "returns n shifted right m bits when n < 0, m < 0" do
    (-@bignum << -2).should == -36893488147419103232
  end

  it "performs a right-shift if given a negative value" do
    (@bignum << -2).should == (@bignum >> 2)
    (@bignum << -4).should == (@bignum >> 4)
  end
  
  it "returns n when n > 0, m == 0" do
    (@bignum << 0).should == @bignum
  end

  it "tries to convert the given argument to an Integer using to_int" do
    (@bignum << 4.5).should == 147573952589676413072
    (obj = mock('4')).should_receive(:to_int).and_return(4)
    (@bignum << obj).should == 147573952589676413072
  end
  
  it "returns n when n < 0, m == 0" do
    (-@bignum << 0).should == -@bignum
  end

  it "returns 0 when m < 0 and m == p where 2**p > n >= 2**(p-1)" do
    (@bignum << -68).should == 0
  end

  it "raises a TypeError when the given argument can't be converted to Integer" do
    obj = mock("Converted to Integer")
    lambda { @bignum << obj }.should raise_error(TypeError)
    obj.should_receive(:to_int).and_return("asdf")
    lambda { @bignum << obj }.should raise_error(TypeError)
  end
  
  not_compliant_on :rubinius, :jruby do
    it "returns 0 when m < 0 and m is a Bignum" do
      (@bignum << -bignum_value()).should == 0
    end
  end

  deviates_on :rubinius do
    it "raises a RangeError when m < 0 and m is a Bignum" do
      lambda { @bignum << -bignum_value() }.should raise_error(RangeError)
    end
  end

  it "returns a Fixnum == fixnum_max() when (fixnum_max() * 2) << -1 and n > 0" do
    result = (fixnum_max() * 2) << -1
    result.should be_an_instance_of(Fixnum)
    result.should == fixnum_max()
  end

  platform_is :wordsize => 32 do
    it "raises a RangeError when the given argument is a Bignum" do
      lambda { @bignum << bignum_value }.should raise_error(RangeError)
      lambda { -@bignum << bignum_value }.should raise_error(RangeError)

      obj = mock("Converted to Integer")
      obj.should_receive(:to_int).exactly(2).times.and_return(bignum_value)
      lambda { @bignum << obj }.should raise_error(RangeError)
      lambda { -@bignum << obj }.should raise_error(RangeError)
    end
  end
  
  it "returns a Fixnum == fixnum_min() when (fixnum_min() * 2) << -1 and n < 0" do
    result = (fixnum_min() * 2) << -1
    result.should be_an_instance_of(Fixnum)
    result.should == fixnum_min()
  end

  it "calls #to_int to convert the argument to an Integer" do
    obj = mock("4")
    obj.should_receive(:to_int).and_return(4)

    (@bignum << obj).should == 2361183241434822606848
  end

  it "raises a TypeError when #to_int does not return an Integer" do
    obj = mock("a string")
    obj.should_receive(:to_int).and_return("asdf")

    lambda { @bignum << obj }.should raise_error(TypeError)
  end

  platform_is :wordsize => 64 do
    it "raises a RangeError when the given argument is a Bignum" do
      # check against 2**64 for 64-bit machines.
      lambda { @bignum << (bignum_value << 1) }.should raise_error(RangeError)
      lambda { -@bignum << (bignum_value << 1) }.should raise_error(RangeError)

      obj = mock("Converted to Integer")
      obj.should_receive(:to_int).exactly(2).times.and_return(bignum_value << 1)
      lambda { @bignum << obj }.should raise_error(RangeError)
      lambda { -@bignum << obj }.should raise_error(RangeError)
    end
  end
    
  it "raises a TypeError when passed nil" do
    lambda { @bignum << nil }.should raise_error(TypeError)
  end

  it "raises a TypeError when passed a String" do
    lambda { @bignum << "4" }.should raise_error(TypeError)
  end
end
