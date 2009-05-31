require File.dirname(__FILE__) + '/../../spec_helper'
require 'bigdecimal'

describe "BigDecimal#+" do

  before(:each) do
    @one = BigDecimal("1")
    @zero = BigDecimal("0")
    @two = BigDecimal("2")
    @three = BigDecimal("3")
    @ten = BigDecimal("10")
    @eleven = BigDecimal("11")
    @nan = BigDecimal("NaN")
    @infinity = BigDecimal("Infinity")
    @infinity_minus = BigDecimal("-Infinity")
    @one_minus = BigDecimal("-1")
    @frac_1 = BigDecimal("1E-99999")
    @frac_2 = BigDecimal("0.9E-99999")
  end

  it "returns a + b" do

    (@two + @one).should == @three
    (@one + @two).should == @three
    (@one + @one_minus).should == @zero
    (@zero + @one).should == @one
    (@ten + @one).should == @eleven
    (@frac_1 + @frac_2).should == BigDecimal("1.9E-99999")
    (@frac_2 + @frac_1).should == BigDecimal("1.9E-99999")
    (@frac_1 + @frac_1).should == BigDecimal("2E-99999")
# can't do it this way because * isn't implemented yet
#    (@frac_1 + @frac_1).should == 2 * @frac_1
  end

  it "returns NaN if NaN is involved" do
    (@one + @nan).nan?.should == true
    (@nan + @one).nan?.should == true
  end

  it "returns Infinity or -Infinity if these are involved" do
    (@zero + @infinity).should == @infinity
    (@frac_2 + @infinity).should == @infinity
    (@two + @infinity_minus).should == @infinity_minus
  end

  it "returns NaN if Infinity + (- Infinity)" do
    (@infinity + @infinity_minus).nan?.should == true
  end

  it "calls #coerce on the passed argument with self" do
    (m = mock('10')).should_receive(:coerce).with(@eleven).and_return([@ten, @eleven])
    (@eleven + m).should == @eleven + 10
  end

  it "calls #method_missing(:coerce) on the passed argument" do
    m = mock('10')
    m.should_not_receive(:respond_to?).with(:coerce)
    m.should_receive(:method_missing).with(:coerce, @eleven).and_return([@ten, @eleven])
    (@eleven + m).should == @eleven +  10
  end
end
