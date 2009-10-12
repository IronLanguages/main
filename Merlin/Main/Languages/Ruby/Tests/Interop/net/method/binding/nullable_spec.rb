require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/classes'

 #Int16NullableProperty Int32NullableProperty Int64NullableProperty UInt16NullableProperty UInt32NullableProperty UInt64NullableProperty ByteNullableProperty SByteNullableProperty DecimalNullableProperty SingleNullableProperty DoubleNullableProperty CharNullableProperty CustomEnumNullableProperty BooleanNullableProperty  StaticInt16NullableProperty StaticInt32NullableProperty StaticInt64NullableProperty StaticUInt16NullableProperty StaticUInt32NullableProperty StaticUInt64NullableProperty StaticByteNullableProperty StaticSByteNullableProperty StaticDecimalNullableProperty StaticSingleNullableProperty StaticDoubleNullableProperty StaticCharNullableProperty StaticCustomEnumNullableProperty StaticBooleanNullableProperty Int16NullableArg Int32NullableArg Int64NullableArg UInt16NullableArg UInt32NullableArg UInt64NullableArg ByteNullableArg SByteNullableArg DecimalNullableArg SingleNullableArg DoubleNullableArg CharNullableArg CustomEnumNullableArg BooleanNullableArg StaticInt16NullableArg StaticInt32NullableArg StaticInt64NullableArg StaticUInt16NullableArg StaticUInt32NullableArg StaticUInt64NullableArg StaticByteNullableArg StaticSByteNullableArg StaticDecimalNullableArg StaticSingleNullableArg StaticDoubleNullableArg StaticCharNullableArg StaticCustomEnumNullableArg StaticBooleanNullableArg 

describe "Method parameter binding with Nullable parameters" do
  @keys = [ :Int16NullableArg, :Int32NullableArg, :Int64NullableArg, :UInt16NullableArg, :UInt32NullableArg, :UInt64NullableArg, :ByteNullableArg, :SByteNullableArg, :DecimalNullableArg, :SingleNullableArg, :DoubleNullableArg, :CharNullableArg, :CustomEnumNullableArg, :BooleanNullableArg ]
  @matrix ={
    "Int16?Value" => { :Int16NullableArg => "Int16NullableArg"},
    "Int32?Value" => { :Int32NullableArg => "Int32NullableArg"},
    "Int64?Value" => { :Int64NullableArg => "Int64NullableArg"},
    "UInt16?Value" => { :UInt16NullableArg => "UInt16NullableArg"},
    "UInt32?Value" => { :UInt32NullableArg => "UInt32NullableArg"},
    "UInt64?Value" => { :UInt64NullableArg => "UInt64NullableArg"},
    "Byte?Value" => { :ByteNullableArg => "ByteNullableArg"},
    "SByte?Value" => { :SByteNullableArg => "SByteNullableArg"},
    "Decimal?Value" => { :DecimalNullableArg => "DecimalNullableArg"},
    "Single?Value" => { :SingleNullableArg => "SingleNullableArg"},
    "Char?Value" => { :CharNullableArg => "CharNullableArg"},
    "Double?Value" => { :DoubleNullableArg => "DoubleNullableArg"},
    "Boolean?Value" => { :BooleanNullableArg => "BooleanNullableArg"},
    "CustomEnum?Value" => { :CustomEnumNullableArg => "CustomEnumNullableArg"},
    "nil" => { :Int16NullableArg => "Int16NullableArg", :Int32NullableArg => "Int32NullableArg", :Int64NullableArg => "Int64NullableArg", :UInt16NullableArg => "UInt16NullableArg", :UInt32NullableArg => "UInt32NullableArg", :UInt64NullableArg => "UInt64NullableArg", :ByteNullableArg => "ByteNullableArg", :SByteNullableArg => "SByteNullableArg", :DecimalNullableArg => "DecimalNullableArg", :SingleNullableArg => "SingleNullableArg", :DoubleNullableArg => "DoubleNullableArg", :CharNullableArg => "CharNullableArg", :CustomEnumNullableArg => "CustomEnumNullableArg", :BooleanNullableArg => "BooleanNullableArg"},
    "obj" => {},
    "true" => { :BooleanNullableArg => "BooleanNullableArg"},
    "false" => { :BooleanNullableArg => "BooleanNullableArg"},
}

  @property_keys = [ :Int16NullableProperty, :Int32NullableProperty, :Int64NullableProperty, :UInt16NullableProperty, :UInt32NullableProperty, :UInt64NullableProperty, :ByteNullableProperty, :SByteNullableProperty, :DecimalNullableProperty, :SingleNullableProperty, :DoubleNullableProperty, :CharNullableProperty, :CustomEnumNullableProperty, :BooleanNullableProperty ]
  @property_matrix ={
    "Int16?Value" => { :Int16NullableProperty => "Int16NullableProperty"},
    "Int32?Value" => { :Int32NullableProperty => "Int32NullableProperty"},
    "Int64?Value" => { :Int64NullableProperty => "Int64NullableProperty"},
    "UInt16?Value" => { :UInt16NullableProperty => "UInt16NullableProperty"},
    "UInt32?Value" => { :UInt32NullableProperty => "UInt32NullableProperty"},
    "UInt64?Value" => { :UInt64NullableProperty => "UInt64NullableProperty"},
    "Byte?Value" => { :ByteNullableProperty => "ByteNullableProperty"},
    "SByte?Value" => { :SByteNullableProperty => "SByteNullableProperty"},
    "Decimal?Value" => { :DecimalNullableProperty => "DecimalNullableProperty"},
    "Single?Value" => { :SingleNullableProperty => "SingleNullableProperty"},
    "Char?Value" => { :CharNullableProperty => "CharNullableProperty"},
    "Double?Value" => { :DoubleNullableProperty => "DoubleNullableProperty"},
    "Boolean?Value" => { :BooleanNullableProperty => "BooleanNullableProperty"},
    "CustomEnum?Value" => { :CustomEnumNullableProperty => "CustomEnumNullableProperty"},
    "nil" => { :Int16NullableProperty => "Int16NullableProperty", :Int32NullableProperty => "Int32NullableProperty", :Int64NullableProperty => "Int64NullableProperty", :UInt16NullableProperty => "UInt16NullableProperty", :UInt32NullableProperty => "UInt32NullableProperty", :UInt64NullableProperty => "UInt64NullableProperty", :ByteNullableProperty => "ByteNullableProperty", :SByteNullableProperty => "SByteNullableProperty", :DecimalNullableProperty => "DecimalNullableProperty", :SingleNullableProperty => "SingleNullableProperty", :DoubleNullableProperty => "DoubleNullableProperty", :CharNullableProperty => "CharNullableProperty", :CustomEnumNullableProperty => "CustomEnumNullableProperty", :BooleanNullableProperty => "BooleanNullableProperty"},
    "obj" => {},
    "true" => { :BooleanNullableProperty => "BooleanNullableProperty"},
    "false" => { :BooleanNullableProperty => "BooleanNullableProperty"},
  }
  before(:each) do
    @target = ClassWithNullableMethods.new
    @target2 = RubyClassWithNullableMethods.new
    @values = Helper.args
  end

  @matrix.each do |input, results|
    Helper.run_matrix(results, input, @keys)
  end

  @property_matrix.each do |input, results|
    Helper.run_property_matrix(results, input, @property_keys)
  end
end

describe "Static method parameter binding with Nullable parameters on" do
  @keys = [ :StaticInt16NullableArg, :StaticInt32NullableArg, :StaticInt64NullableArg, :StaticUInt16NullableArg, :StaticUInt32NullableArg, :StaticUInt64NullableArg, :StaticByteNullableArg, :StaticSByteNullableArg, :StaticDecimalNullableArg, :StaticSingleNullableArg, :StaticDoubleNullableArg, :StaticCharNullableArg, :StaticCustomEnumNullableArg, :StaticBooleanNullableArg ]
  @matrix ={
    "Int16?Value" => { :StaticInt16NullableArg => "StaticInt16NullableArg"},
    "Int32?Value" => { :StaticInt32NullableArg => "StaticInt32NullableArg"},
    "Int64?Value" => { :StaticInt64NullableArg => "StaticInt64NullableArg"},
    "UInt16?Value" => { :StaticUInt16NullableArg => "StaticUInt16NullableArg"},
    "UInt32?Value" => { :StaticUInt32NullableArg => "StaticUInt32NullableArg"},
    "UInt64?Value" => { :StaticUInt64NullableArg => "StaticUInt64NullableArg"},
    "Byte?Value" => { :StaticByteNullableArg => "StaticByteNullableArg"},
    "SByte?Value" => { :StaticSByteNullableArg => "StaticSByteNullableArg"},
    "Decimal?Value" => { :StaticDecimalNullableArg => "StaticDecimalNullableArg"},
    "Single?Value" => { :StaticSingleNullableArg => "StaticSingleNullableArg"},
    "Char?Value" => { :StaticCharNullableArg => "StaticCharNullableArg"},
    "Double?Value" => { :StaticDoubleNullableArg => "StaticDoubleNullableArg"},
    "Boolean?Value" => { :StaticBooleanNullableArg => "StaticBooleanNullableArg"},
    "CustomEnum?Value" => { :StaticCustomEnumNullableArg => "StaticCustomEnumNullableArg"},
    "nil" => { :StaticInt16NullableArg => "StaticInt16NullableArg", :StaticInt32NullableArg => "StaticInt32NullableArg", :StaticInt64NullableArg => "StaticInt64NullableArg", :StaticUInt16NullableArg => "StaticUInt16NullableArg", :StaticUInt32NullableArg => "StaticUInt32NullableArg", :StaticUInt64NullableArg => "StaticUInt64NullableArg", :StaticByteNullableArg => "StaticByteNullableArg", :StaticSByteNullableArg => "StaticSByteNullableArg", :StaticDecimalNullableArg => "StaticDecimalNullableArg", :StaticSingleNullableArg => "StaticSingleNullableArg", :StaticDoubleNullableArg => "StaticDoubleNullableArg", :StaticCharNullableArg => "StaticCharNullableArg", :StaticCustomEnumNullableArg => "StaticCustomEnumNullableArg", :StaticBooleanNullableArg => "StaticBooleanNullableArg"},
    "obj" => {},
    "true" => { :StaticBooleanNullableArg => "StaticBooleanNullableArg"},
    "false" => { :StaticBooleanNullableArg => "StaticBooleanNullableArg"},
  }

  @property_keys = [ :StaticInt16NullableProperty, :StaticInt32NullableProperty, :StaticInt64NullableProperty, :StaticUInt16NullableProperty, :StaticUInt32NullableProperty, :StaticUInt64NullableProperty, :StaticByteNullableProperty, :StaticSByteNullableProperty, :StaticDecimalNullableProperty, :StaticSingleNullableProperty, :StaticDoubleNullableProperty, :StaticCharNullableProperty, :StaticCustomEnumNullableProperty, :StaticBooleanNullableProperty ]
  @property_matrix ={
    "Int16?Value" => { :StaticInt16NullableProperty => "StaticInt16NullableProperty"},
    "Int32?Value" => { :StaticInt32NullableProperty => "StaticInt32NullableProperty"},
    "Int64?Value" => { :StaticInt64NullableProperty => "StaticInt64NullableProperty"},
    "UInt16?Value" => { :StaticUInt16NullableProperty => "StaticUInt16NullableProperty"},
    "UInt32?Value" => { :StaticUInt32NullableProperty => "StaticUInt32NullableProperty"},
    "UInt64?Value" => { :StaticUInt64NullableProperty => "StaticUInt64NullableProperty"},
    "Byte?Value" => { :StaticByteNullableProperty => "StaticByteNullableProperty"},
    "SByte?Value" => { :StaticSByteNullableProperty => "StaticSByteNullableProperty"},
    "Decimal?Value" => { :StaticDecimalNullableProperty => "StaticDecimalNullableProperty"},
    "Single?Value" => { :StaticSingleNullableProperty => "StaticSingleNullableProperty"},
    "Char?Value" => { :StaticCharNullableProperty => "StaticCharNullableProperty"},
    "Double?Value" => { :StaticDoubleNullableProperty => "StaticDoubleNullableProperty"},
    "Boolean?Value" => { :StaticBooleanNullableProperty => "StaticBooleanNullableProperty"},
    "CustomEnum?Value" => { :StaticCustomEnumNullableProperty => "StaticCustomEnumNullableProperty"},
    "nil" => { :StaticInt16NullableProperty => "StaticInt16NullableProperty", :StaticInt32NullableProperty => "StaticInt32NullableProperty", :StaticInt64NullableProperty => "StaticInt64NullableProperty", :StaticUInt16NullableProperty => "StaticUInt16NullableProperty", :StaticUInt32NullableProperty => "StaticUInt32NullableProperty", :StaticUInt64NullableProperty => "StaticUInt64NullableProperty", :StaticByteNullableProperty => "StaticByteNullableProperty", :StaticSByteNullableProperty => "StaticSByteNullableProperty", :StaticDecimalNullableProperty => "StaticDecimalNullableProperty", :StaticSingleNullableProperty => "StaticSingleNullableProperty", :StaticDoubleNullableProperty => "StaticDoubleNullableProperty", :StaticCharNullableProperty => "StaticCharNullableProperty", :StaticCustomEnumNullableProperty => "StaticCustomEnumNullableProperty", :StaticBooleanNullableProperty => "StaticBooleanNullableProperty"},
    "obj" => {},
    "true" => { :StaticBooleanNullableProperty => "StaticBooleanNullableProperty"},
    "false" => { :StaticBooleanNullableProperty => "StaticBooleanNullableProperty"},
  }
  before(:each) do
    @target = StaticClassWithNullableMethods
    @target2 = RubyStaticClassWithNullableMethods
    @target.reset()
    @target2.reset()
    @values = Helper.args
  end

  @matrix.each do |input, results|
    Helper.run_matrix(results, input, @keys)
  end
  
  @property_matrix.each do |input, results|
    Helper.run_property_matrix(results, input, @property_keys, false)
  end
end
