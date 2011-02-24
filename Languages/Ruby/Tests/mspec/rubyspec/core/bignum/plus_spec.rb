require File.dirname(__FILE__) + '/../../spec_helper'

describe "Bignum#+" do
  before(:each) do
    @bignum = bignum_value(76)
  end
  
  it "returns self plus the given Integer" do
    (@bignum + 4).should == 9223372036854775888
    (@bignum + 4.2).should be_close(9223372036854775888.2, TOLERANCE)
    (@bignum + bignum_value(3)).should == 18446744073709551695
  end

  it "raises a TypeError when given a non-Integer" do
    lambda { @bignum + mock('10') }.should raise_error(TypeError)
    lambda { @bignum + "10" }.should raise_error(TypeError)
    lambda { @bignum + :symbol}.should raise_error(TypeError)
  end

  it "calls #coerce on the passed argument with self" do
    (m = mock('10')).should_receive(:coerce).with(@bignum).and_return([10, @bignum])
    (@bignum + m).should == @bignum + 10
  end

  it "calls #method_missing(:coerce) on the passed argument" do
    m = mock('10')
    m.should_not_receive(:respond_to?).with(:coerce)
    m.should_receive(:method_missing).with(:coerce, @bignum).and_return([10, @bignum])
    (@bignum + m).should == @bignum +  10
  end
end
