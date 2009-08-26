require File.dirname(__FILE__) + '/spec_helper'

describe "Numeric helpers" do

  before :each do
    @controller = NumericExtSpecs.new(Merb::Request.new({}))
  end
  
  describe "with_delimiter" do
    
    before(:each) do
      @number = 12345678
    end
    
    it "should use the default formatting for numbers" do
      @number.with_delimiter.should == "12,345,678"
      @number.with_delimiter.should == @number.with_delimiter(:us)
    end
    
    it "should support passing another format" do
      @number.with_delimiter(:fr).should == "12 345 678"
    end
    
    it "should support passing overwriting options" do
      @number.with_delimiter(:fr, :delimiter => ',').should == "12,345,678"
      12345678.9.with_delimiter(:fr, :separator => ' et ').should == "12 345 678 et 9"
    end
  end


  describe "with_precision" do
     it "should use a default precision" do
       111.2345.with_precision.should == "111.235"
     end
     
     it "should support other precision formats" do
        111.2345.with_precision(:uk).should == "111.235"
      end
      
     it "should support overwriting precision options" do
        111.2345.with_precision(:uk, :precision => 1).should == "111.2"
        1234.567.with_precision(:us, :precision => 1, :separator => ',', :delimiter => '-').should == "1-234,6"
     end
   end
   
   
   describe "to_concurrency" do
     
     before(:each) do
       @number = 1234567890.50
     end
     
     it "should use the US$ by default" do
       @number.to_currency.should == "$1,234,567,890.50"
       @number.to_currency.should == @number.to_currency(:us)
       @number.to_currency.should == @number.to_currency(:default)
       result = @controller.render :to_concurrency_default
       result.should == "$1,234,567,890.50"
     end
     
     it "should use the precision settings of the format" do
       1234567890.506.to_currency(:us).should == "$1,234,567,890.51"
     end
     
     it "should support other formats" do
       @number.to_currency(:uk).should == "&pound;1,234,567,890.50"
       @number.to_currency(:fr).should == "1 234 567 890,50€"
     end
     
     it "should support overwriting options" do
       1234567890.506.to_currency(:us, :precision => 1).should == "$1,234,567,890.5"
       1234567890.516.to_currency(:us, :unit => "€").should == "€1,234,567,890.52"
       1234567890.506.to_currency(:us, :precision => 3, :unit => "€").should == "€1,234,567,890.506"
       1234567890.506.to_currency(:aus, :unit => "$AUD", :format => '%n %u').should == "1,234,567,890.51 $AUD"
     end
     
   end
   
   describe "Numeric::Transformer formats" do
     
     it "should be able to add a new format" do
       Numeric::Transformer.default_format.should be_instance_of(Hash)
     end
     
     it "should be able to change the default format" do
       original_default_format = Numeric::Transformer.default_format
       original_default_format[:currency][:unit].should == "$"
       Numeric::Transformer.change_default_format(:fr)
       Numeric::Transformer.default_format.should_not == original_default_format
       Numeric::Transformer.default_format[:currency][:unit].should == "€"
     end
     
     it "should be able to add a format" do
       merb_format = {:merb => 
                        {  :number => {      
                           :precision => 3, 
                           :delimiter => ' ', 
                           :separator => ','
                           },
                           :currency => { 
                             :unit => 'Merbollars',
                             :format => '%n %u',
                             :precision => 2 
                           }
                         }
                      }
       Numeric::Transformer.add_format(merb_format)
       Numeric::Transformer.change_default_format(:merb)
       12345.to_currency.should == "12 345,00 Merbollars"
     end
     
   end
   
   describe "other misc numeric helpers" do
     
     describe "two_digits" do
       it "should convert a numeric value in a 2 digit string (prepend with a zero if needed)" do
         7.two_digits.should == "07"
         result = @controller.render :two_digits
         result.should == "07"
       end
    end
    
    describe "minutes_to_hours" do
      it "should convert a number of minutes into an hour representation" do
        result = @controller.render :minutes_to_hours
        result.should == "05:15"
      end
      
    end
     
   end
  
  
end
