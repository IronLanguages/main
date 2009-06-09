require File.dirname(__FILE__) + '/../../spec_helper'

describe "Fixnum#+" do
  it "returns self plus the given Integer" do
    (491 + 2).should == 493
    (90210 + 10).should == 90220

    (9 + bignum_value).should == 9223372036854775817
    (1001 + 5.219).should == 1006.219
  end

  it "raises a TypeError when given a non-Integer" do
    lambda {
      (obj = mock('10')).should_receive(:to_int).any_number_of_times.and_return(10)
      13 + obj
    }.should raise_error(TypeError)
    lambda { 13 + "10"    }.should raise_error(TypeError)
    lambda { 13 + :symbol }.should raise_error(TypeError)
  end

  it "calls #coerce on the passed argument with self" do
    (m = mock('10')).should_receive(:coerce).with(13).and_return([10, 13])
    (13 + m).should == 23
  end

  it "calls #method_missing(:coerce) on the passed argument" do
    m = mock('10')
    m.should_not_receive(:respond_to?).with(:coerce)
    m.should_receive(:method_missing).with(:coerce, 13).and_return([10, 13])
    (13 + m).should == 23
  end
end
