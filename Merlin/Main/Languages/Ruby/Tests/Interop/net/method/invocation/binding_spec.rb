require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

def run_matrix(results, input)
  results.each do |meth, result|
    it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (ClassWithMethods)" do
      meth_call = (input == "NoArg" ? lambda { @target.send(meth)} : lambda {@target.send(meth, @values[input])})
      if result.class == Class && result < Exception
        meth_call.should raise_error result
      else 
        res, ref = meth_call.call
        res.should == result
      end
    end
  
    it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (RubyClassWithMethods)" do
      meth_call = (input == "NoArg" ? lambda { @target2.send(meth)} : lambda {@target2.send(meth, @values[input])})
      if result.class == Class && result < Exception
        meth_call.should raise_error result
      else 
        res, ref = meth_call.call
        res.should == result
      end
    end
  
    next if result.class == Class && result < Exception
    
    it "passes the correct input (#{input}) into method (#{meth}) (ClassWithMethods)" do
      value = @values[input]
      meth_call = (input == "NoArg" ? lambda { @target.send(meth)} : lambda {@target.send(meth, value)})
      res, ref = meth_call.call
      if input != "NoArg"
        result = Helper.result(meth,value)
        @target.tracker.should == [*result]
      else
        result = case meth.to_s
                 when /ParamsInt32ArrArg/
                  [[]]
                 when /DefaultInt32Arg/
                   [10]
                 when /NoArg/
                   []
                 else
                   nil
                 end
        @target.tracker.should == result
      end
      ref.should == result if ref
    end
    
    it "passes the correct input (#{input}) into method (#{meth}) (RubyClassWithMethods)" do
      value = @values[input]
      meth_call = (input == "NoArg" ? lambda { @target2.send(meth)} : lambda {@target2.send(meth, value)})
      res, ref = meth_call.call
      if input != "NoArg"
        result = Helper.result(meth,value)
        @target2.tracker.should == [*result]
      else
        result = case meth.to_s
                 when /ParamsInt32ArrArg/
                  [[]]
                 when /DefaultInt32Arg/
                   [10]
                 when /NoArg/
                   []
                 else
                   nil
                 end
        @target2.tracker.should == result
      end
      ref.should == result if ref
    end
  end
end

describe "Method parameter binding" do
  TE = TypeError
  AE = ArgumentError
  OE = System::OverflowException
  RE = RangeError 
  @matrix = {
    #      NoArg Int32Arg DoubleArg BigIntegerArg StringArg BooleanArg SByteArg Int16Arg Int64Arg SingleArg ByteArg UInt16Arg UInt32Arg UInt64Arg CharArg DecimalArg ObjectArg NullableInt32Arg
    "" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "a" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "abc" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
          
    "System::String''" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "System::String'a'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "System::String'abc'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },

    "MyString''" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "MyString'a'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "MyString'abc'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
          
    :a => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => TE, 
          :BigIntegerArg => "BigIntegerArg", :StringArg => "StringArg", :BooleanArg => "BooleanArg",
          :SByteArg => RE, :Int16Arg => RE, :Int64Arg => "Int64Arg", :SingleArg => TE, :ByteArg => RE,
          :UInt16Arg => RE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE,
          :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    :abc => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => TE, 
          :BigIntegerArg => "BigIntegerArg", :StringArg => "StringArg", :BooleanArg => "BooleanArg", 
          :SByteArg => RE, :Int16Arg => RE, :Int64Arg => "Int64Arg", :SingleArg => TE, :ByteArg => RE, 
          :UInt16Arg => RE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    
    "false" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE, 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "nil" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg", 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, 
          :DecimalArg => TE, :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg", :ParamsInt32ArrArg => "ParamsInt32ArrArg" },
    "obj" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE,
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE,
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    "true" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE, 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE,
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg" },
    
    "BigIntegerOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg",
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg" },
    "BigIntegerZero" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", :BigIntegerArg => "BigIntegerArg", 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", :Int16Arg => "Int16Arg",
          :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", :UInt16Arg => "UInt16Arg", 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg" },
    
    "FixnumMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg" },
    "FixnumMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg",
          :ObjectArg => "ObjectArg" },
    "FixnumMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE,
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg" },
    "FixnumMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg",
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => "DecimalArg",
          :ObjectArg => "ObjectArg" },

    "Convert::ToI" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, 
          :CharArg => TE, :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    "Convert::ToInt" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", :BigIntegerArg => "BigIntegerArg", 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", :Int16Arg => "Int16Arg",
          :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", :UInt16Arg => "UInt16Arg", 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg" },
    "Convert::ToIntToI" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", :BigIntegerArg => "BigIntegerArg", 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", :Int16Arg => "Int16Arg",
          :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", :UInt16Arg => "UInt16Arg", 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg" },
          
    "Convert::ToS" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, 
          :CharArg => TE, :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    "Convert::ToStr" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, 
          :CharArg => "CharArg", :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    "Convert::ToStrToS" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, 
          :CharArg => "CharArg", :DecimalArg => TE, :ObjectArg => "ObjectArg" },
    
    "FloatMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg" },
    "FloatMaxValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg" },
    "FloatMinValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg" },
    "FloatMinValuePlusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg" },
    
    "Int32?Null" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg",
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, 
          :Int64Arg => TE, :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, 
          :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => "NullableInt32Arg", :ParamsInt32ArrArg => "ParamsInt32ArrArg" },
    "Int32?One" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => "StringArg", :BooleanArg => "BooleanArg",
          :SByteArg => "SByteArg", :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", 
          :ByteArg => "ByteArg", :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg",
          :CharArg => TE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "Int32?MinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    
    "System::ByteMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg" },
    "System::ByteMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg",
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg" },
    "System::ByteMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg" },
    "System::ByteMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg",
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg" },
    
    "System::CharMaxValue" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => "DoubleArg", :BigIntegerArg => TE,
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, 
          :Int64Arg => TE, :SingleArg => "SingleArg", :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, 
          :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::CharMaxValue" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => "DoubleArg", :BigIntegerArg => TE, 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE,
          :SingleArg => "SingleArg", :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE,
          :CharArg => "CharArg", :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    
    "System::DecimalMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::DecimalMaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::DecimalMinValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::DecimalMinValuePlusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::Int16MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int16MaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int16MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int16MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::Int64MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int64MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int64MinValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::Int64MinValuePlusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::SByteMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::SByteMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::SByteMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::SByteMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::SingleMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg"},
    "System::SingleMaxValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg"},
    "System::SingleMinValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg"},
    "System::SingleMinValuePlusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg"},
          
    "System::UInt16MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt16MaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt16MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt16MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::UInt32MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt32MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt32MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt32MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
          
    "System::UInt64MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt64MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt64MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},
    "System::UInt64MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg"},

    "NoArg" => {:NoArg => "NoArg", :Int32Arg => AE, :DoubleArg => AE, 
          :BigIntegerArg => AE, :StringArg => AE, :BooleanArg => AE, :SByteArg => AE, 
          :Int16Arg => AE, :Int64Arg => AE, :SingleArg => AE, :ByteArg => AE, 
          :UInt16Arg => AE, :UInt32Arg => AE, :UInt64Arg => AE, :CharArg => AE, 
          :DecimalArg => AE, :ObjectArg => AE, :DefaultInt32Arg => "DefaultInt32Arg", :ParamsInt32ArrArg => "ParamsInt32ArrArg"},
  }    
  
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.numeric_and_string_args
    nil #extraneous puts statement?
  end
    
  @matrix.each do |input, results|
    [:RefInt32Arg, :ParamsInt32ArrArg, :Int32ArgParamsInt32ArrArg, :NullableInt32Arg, :DefaultInt32Arg, :Int32ArgDefaultInt32Arg].each do |key|
      results[key] ||= (results[:Int32Arg] == "Int32Arg" ? key.to_s : results[:Int32Arg])
    end
    run_matrix(results, input)
  end
end

describe "Method parameter binding with Class-like parameters" do
  #IInterfaceArg ImplementsIInterfaceArg DerivedFromImplementsIInterfaceArg CStructArg StructImplementsIInterfaceArg AbstractClassArg DerivedFromAbstractArg CustomEnumArg EnumIntArg
  @matrix = {
    "anonymous class" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "anonymous classInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
  
    "metaclass" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg" },
    "BindingSpecs::RubyImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "ImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"},
    
    "DerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => "ImplementsIInterfaceArg", :DerivedFromImplementsIInterfaceArg => "DerivedFromImplementsIInterfaceArg", :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "DerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "DerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "AbstractClass" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "BindingSpecs::RubyDerivedFromDerivedFromAbstract" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "BindingSpecs::RubyDerivedFromDerivedFromAbstractInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => "AbstractClassArg", :DerivedFromAbstractArg => "DerivedFromAbstractArg", :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Class" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ClassInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "Object" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "ObjectInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CStruct" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CStructInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => "CStructArg", :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "StructImplementsIInterface" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "StructImplementsIInterfaceInstance" => {:IInterfaceArg => "IInterfaceArg", :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => "StructImplementsIInterfaceArg", :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "EnumInt" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "EnumIntInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "CustomEnum" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => TE, :EnumIntArg => TE, :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    "CustomEnumInstance" => {:IInterfaceArg => TE, :ImplementsIInterfaceArg => TE, :DerivedFromImplementsIInterfaceArg => TE, :CStructArg => TE, :StructImplementsIInterfaceArg => TE, :AbstractClassArg => TE, :DerivedFromAbstractArg => TE, :CustomEnumArg => "CustomEnumArg", :EnumIntArg => "EnumIntArg", :ObjectArg => "ObjectArg", :NoArg => AE, :BooleanArg => "BooleanArg"}, 
    
    "NoArg" => {:IInterfaceArg => AE, :ImplementsIInterfaceArg => AE, :DerivedFromImplementsIInterfaceArg => AE, :CStructArg => AE, :StructImplementsIInterfaceArg => AE, :AbstractClassArg => AE, :DerivedFromAbstractArg => AE, :CustomEnumArg => AE, :EnumIntArg => AE, :ObjectArg => AE, :NoArg => "NoArg", :BooleanArg => AE}, 
  }
  before(:each) do
    @target = ClassWithMethods.new
    @target2 = RubyClassWithMethods.new
    @values = Helper.classlike_args
    nil #extraneous puts statement?
  end
    
  @matrix.each do |input, results|
    run_matrix(results, input)
  end
end
