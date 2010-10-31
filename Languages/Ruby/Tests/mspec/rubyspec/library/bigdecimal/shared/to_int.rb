require 'bigdecimal'

describe :bigdecimal_to_int , :shared => true do
  ruby_bug "fixed_in_ruby_1_8_7@25799", "1.8.7.202" do
    it "returns nil if BigDecimal is infinity or NaN" do
      BigDecimal("Infinity").send(@method).should == nil
      BigDecimal("NaN").send(@method).should == nil
    end
  end

  it "returns Integer or Bignum otherwise for single significant digit before the decimal point" do
    BigDecimal("3E-1").send(@method).should == 0
    
    BigDecimal("2E5").send(@method).should == 2 * (10 ** 5)
    BigDecimal("2.1E5").send(@method).should == 2.1 * (10 ** 5)
    
    BigDecimal("2").send(@method).should == 2
    BigDecimal("2.1").send(@method).should == 2    

    BigDecimal("200.123").send(@method).should == 200
    BigDecimal("200.001").send(@method).should == 200

    BigDecimal("3.14159").send(@method).should == 3
  end

  it "returns Integer or Bignum otherwise for multiple significant digit before the decimal point" do
    BigDecimal("123").send(@method).should == 123
    BigDecimal("123.00456").send(@method).should == 123
    BigDecimal("123.456").send(@method).should == 123
    BigDecimal("12300.456").send(@method).should == 12300
    BigDecimal("12300.00456").send(@method).should == 12300
  end
end
