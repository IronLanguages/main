require File.dirname(__FILE__) + '/../../spec_helper'

describe "Float#+" do
  it "returns self plus other" do
    (491.213 + 2).should be_close(493.213, TOLERANCE)
    (9.99 + bignum_value).should be_close(9223372036854775808.000, TOLERANCE)
    (1001.99 + 5.219).should be_close(1007.209, TOLERANCE)
  end
  
  it "calls #coerce on the passed argument with self" do
    (m = mock('10')).should_receive(:coerce).with(1.5).and_return([10.0, 1.5])
    (1.5 + m).should == 11.5
  end

  it "calls #method_missing(:coerce) on the passed argument" do
    m = mock('10')
    m.should_not_receive(:respond_to?).with(:coerce)
    m.should_receive(:method_missing).with(:coerce, 1.5).and_return([10.0, 1.5])
    (1.5 + m).should == 11.5
  end

  it "allows large operands without error" do
    lambda { 1.0 + 2**50000 }.should_not raise_error
  end
end
