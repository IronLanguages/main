require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Method parameter binding" do
  TE = TypeError
  AE = ArgumentError
  OE = System::OverflowException
  RE = RangeError #TODO: Should this map to OverflowException?
  @matrix = {
    #      NoArg Int32Arg DoubleArg BigIntegerArg StringArg BooleanArg SByteArg Int16Arg Int64Arg SingleArg ByteArg UInt16Arg UInt32Arg UInt64Arg CharArg DecimalArg ObjectArg NullableInt32Arg
    "" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "a" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "abc" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
          
    "System::String''" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "System::String'a'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "System::String'abc'" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => AE, :BigIntegerArg => TE, :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => AE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },

    
    :a => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg", :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => RE, :Int16Arg => RE, :Int64Arg => "Int64Arg", :SingleArg => TE, 
          :ByteArg => RE, :UInt16Arg => RE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    :abc => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg", :StringArg => "StringArg", 
          :BooleanArg => "BooleanArg", :SByteArg => RE, :Int16Arg => RE, :Int64Arg => "Int64Arg", :SingleArg => TE, 
          :ByteArg => RE, :UInt16Arg => RE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => "CharArg", :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    
    "false" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE, 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE, 
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "nil" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg", 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, 
          :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, 
          :DecimalArg => TE, :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "obj" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE,
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE,
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    "true" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => TE, :StringArg => TE, 
          :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE, :SingleArg => TE,
          :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => TE },
    
    "BigIntegerOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg",
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "BigIntegerZero" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", :BigIntegerArg => "BigIntegerArg", 
          :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", :Int16Arg => "Int16Arg",
          :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", :UInt16Arg => "UInt16Arg", 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    
    "FixnumMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "FixnumMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, :DecimalArg => "DecimalArg",
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "FixnumMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE,
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => "DecimalArg", 
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "FixnumMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg",
          :BigIntegerArg => "BigIntegerArg", :StringArg => AE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE,
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => "DecimalArg",
          :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    
    "FloatMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => OE },
    "FloatMaxValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => OE },
    "FloatMinValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => OE },
    "FloatMinValuePlusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, 
          :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, :DecimalArg => OE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => OE },
    
    "Int32?Null" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => TE, :BigIntegerArg => "BigIntegerArg",
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, 
          :Int64Arg => TE, :SingleArg => TE, :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, 
          :UInt64Arg => TE, :CharArg => TE, :DecimalArg => TE, :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => "NullableInt32Arg" },
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
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "System::ByteMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg",
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "System::ByteMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    "System::ByteMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg",
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg" },
    
    "System::CharMaxValue" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => "DoubleArg", :BigIntegerArg => TE,
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, 
          :Int64Arg => TE, :SingleArg => "SingleArg", :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, 
          :UInt64Arg => TE, :CharArg => "CharArg", :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", 
          :NullableInt32Arg => TE},
    "System::CharMaxValue" => {:NoArg => AE, :Int32Arg => TE, :DoubleArg => "DoubleArg", :BigIntegerArg => TE, 
          :StringArg => "StringArg", :BooleanArg => "BooleanArg", :SByteArg => TE, :Int16Arg => TE, :Int64Arg => TE,
          :SingleArg => "SingleArg", :ByteArg => TE, :UInt16Arg => TE, :UInt32Arg => TE, :UInt64Arg => TE,
          :CharArg => "CharArg", :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => TE},
    
    "System::DecimalMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => TE},
    "System::DecimalMaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => TE},
    "System::DecimalMinValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => TE},
    "System::DecimalMinValuePlusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, :Int16Arg => OE,
          :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE,
          :CharArg => OE, :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => TE},
          
    "System::Int16MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::Int16MaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::Int16MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::Int16MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
          
    "System::Int64MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::Int64MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::Int64MinValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::Int64MinValuePlusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
          
    "System::SByteMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::SByteMaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::SByteMinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::SByteMinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
          
    "System::SingleMaxValueMinusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::SingleMaxValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::SingleMinValue" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::SingleMinValuePlusOne" => {:NoArg => AE, :Int32Arg => RE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => OE, :CharArg => TE, 
          :DecimalArg => OE, :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
          
    "System::UInt16MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::UInt16MaxValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::UInt16MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::UInt16MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
          
    "System::UInt32MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::UInt32MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::UInt32MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::UInt32MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
          
    "System::UInt64MaxValueMinusOne" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::UInt64MaxValue" => {:NoArg => AE, :Int32Arg => OE, :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => OE, 
          :Int16Arg => OE, :Int64Arg => OE, :SingleArg => "SingleArg", :ByteArg => OE, 
          :UInt16Arg => OE, :UInt32Arg => OE, :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => OE},
    "System::UInt64MinValue" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
    "System::UInt64MinValuePlusOne" => {:NoArg => AE, :Int32Arg => "Int32Arg", :DoubleArg => "DoubleArg", 
          :BigIntegerArg => "BigIntegerArg", :StringArg => TE, :BooleanArg => "BooleanArg", :SByteArg => "SByteArg", 
          :Int16Arg => "Int16Arg", :Int64Arg => "Int64Arg", :SingleArg => "SingleArg", :ByteArg => "ByteArg", 
          :UInt16Arg => "UInt16Arg", :UInt32Arg => "UInt32Arg", :UInt64Arg => "UInt64Arg", :CharArg => TE, 
          :DecimalArg => "DecimalArg", :ObjectArg => "ObjectArg", :NullableInt32Arg => "NullableInt32Arg"},
  }    
  before(:each) do
    @target = ClassWithMethods.new
    @values = @target.numeric_and_string_args
    nil #extraneous puts statement?
  end
    
  @matrix.each do |input, results|
    results.each do |meth, result|
      it "binds '#{meth}' for '#{input}' with '#{result.to_s}'" do
        if result.class == Class && result < Exception
          lambda { @target.send(meth, @values[input])}.should raise_error result
        else 
          @target.send(meth, @values[input]).should == result
        end
      end
    
      next if result.class == Class && result < Exception
      
      it "passes the correct input (#{input}) into method (#{meth})" do
        value = @values[input]
        @target.send(meth, value)
        #TODO: there has to be a better way
        result = case meth.to_s
                 when /Boolean/
                   value ? true : false
                 when /Single/
                   if value.is_a?(System::UInt32) || value.is_a?(System::Int32)
                     System::Single.parse(value.to_s)
                   elsif value.is_a? System::Char
                     System::Single.induced_from(System::Convert.to_int32(value))
                   elsif value.is_a? Float
                     System::Convert.to_single(value)
                   end
                 when /(Double|Decimal)/
                   if value.is_a? System::Char
                     System.const_get($1).induced_from(System::Convert.to_int32(value))
                   end
                 when /Char/
                   if value.is_a? System::String
                     value[0]
                   elsif value.is_a? Symbol
                     value.to_s[0..0]
                   elsif value.is_a? String
                     value[0..0]
                   end
                 when /Int32|BigInteger/
                   if value.is_a? Symbol
                     value.to_i
                   end
                 when /(Int64|UInt32|UInt64)/
                   if value.is_a? Symbol
                     System.const_get($1).induced_from(value.to_i)
                   end
                 else
                   value
                 end
        result.nil? ? result = value : nil
        @target.tracker.should == [result]
      end
    end
  end
end
