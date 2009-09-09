reference "System.dll"
csc <<-EOL
using Microsoft.Scripting.Math;
using System.Collections.Generic;
EOL
def method_block(type, prefix, suffix = "", &blk)
  val = <<-EOL
  public #{type} #{prefix}SignatureOverload#{suffix}() { #{blk.call "SO void" }; }
  public #{type} #{prefix}SignatureOverload#{suffix}(string foo) { #{blk.call "SO string"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(int foo) { #{blk.call "SO int"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(string foo, params int[] bar) { #{blk.call "SO string params(int[])"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(string foo, params string[] bar) { #{blk.call "SO string params(string[])"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(string foo, int bar, int baz) { #{ blk.call "SO string int int"};}
  public #{type} #{prefix}SignatureOverload#{suffix}(params int[] args) { #{blk.call "SO params(int[])"};}
  public #{type} #{prefix}SignatureOverload#{suffix}(ref string foo) { #{blk.call "SO ref string"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(out int foo) { foo = 1;#{blk.call "SO out int"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(string foo, ref string bar) { #{blk.call "SO string ref"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(ref string foo, string bar) { #{blk.call "SO ref string"}; }
  public #{type} #{prefix}SignatureOverload#{suffix}(out string foo, ref string bar) { foo = "out"; #{blk.call "SO out ref"}; }
  EOL
end
@methods_string = <<-EOL
  #region private methods
  private string Private1Generic0Arg<T>() {
    return "private generic no args";
  }
  
  private string Private1Generic1Arg<T>(T arg0) {
    return Public1Generic1Arg<T>(arg0);
  }

  private string Private1Generic2Arg<T>(T arg0, string arg1) {
    return Public1Generic2Arg<T>(arg0, arg1);
  }

  private string Private2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public2Generic2Arg<T, U>(arg0, arg1);
  }

  private string Private2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2);
  }

  private string Private3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public3Generic3Arg<T, U, V>(arg0, arg1, arg2);
  }

  private string Private3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return Public3Generic4Arg<T, U, V>(arg0, arg1, arg2, arg3);
  }
  #endregion
  
  #region protected methods
  protected string Protected1Generic0Arg<T>() {
    return "protected generic no args";
  }
  
  protected string Protected1Generic1Arg<T>(T arg0) {
    return Public1Generic1Arg<T>(arg0);
  }

  protected string Protected1Generic2Arg<T>(T arg0, string arg1) {
    return Public1Generic2Arg<T>(arg0, arg1);
  }

  protected string Protected2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public2Generic2Arg<T, U>(arg0, arg1);
  }

  protected string Protected2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2);
  }

  protected string Protected3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public3Generic3Arg<T, U, V>(arg0, arg1, arg2);
  }

  protected string Protected3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return Public3Generic4Arg<T, U, V>(arg0, arg1, arg2, arg3);
  }
  #endregion
 
  #region public methods
  public string Public1Generic0Arg<T>() {
    return "public generic no args";
  }

  public string Public1Generic1Arg<T>(T arg0) {
    return arg0.ToString();
  }

  public string Public1Generic2Arg<T>(T arg0, string arg1) {
    return System.String.Format("{0} {1}", arg0, arg1);
  }

  public string Public2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public1Generic2Arg<T>(arg0, arg1.ToString());
  }

  public string Public2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return System.String.Format("{0} {1} {2}", arg0, arg1, arg2);
  }

  public string Public3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2.ToString());
  }

  public string Public3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return System.String.Format("{0} {1} {2} {3}", arg0, arg1, arg2, arg3);
  }
  #endregion
  
  #region Constrained methods
  public T StructConstraintMethod<T>(T arg0)
  where T : struct {
    return arg0;
  }

  public T ClassConstraintMethod<T>(T arg0)
  where T : class {
    return arg0;
  }

  public T ConstructorConstraintMethod<T>()
  where T : new() {
    return new T();
  }

  public T TypeConstraintMethod<T, TBase>(T arg0)
  where T : TBase {
    return arg0;
  }
  #endregion
EOL

@conflicting_method_string = <<-EOL
  public string Public1Generic2Arg<T>(T arg0, K arg1) {
    return Public2Generic2Arg<T, K>(arg0, arg1);
  }
  
  public string ConflictingGenericMethod<K>(K arg0) {
    return arg0.ToString();
  }
EOL
csc <<-EOL

  public abstract partial class AbstractClassWithMethods {
    public abstract string PublicMethod();
    protected abstract string ProtectedMethod();
  }


  public partial class Klass{
    public static int StaticVoidMethod() {
      return 1;
    }

    private int _foo;
    
    public int Foo {
      get { return _foo; }
    }

    public Klass() {
      _foo = 10;
    }
  }

  public partial class SubKlass : Klass {}
#pragma warning disable 693
  public partial class GenericClassWithMethods<K> {
  #{@methods_string}
  #{@conflicting_method_string}
  }
  
  public partial class GenericClass2Params<K, J> {
  #{@methods_string}
  #{@conflicting_method_string}
  }
#pragma warning restore 693
  
  public partial class ClassWithIndexer {
    public int[,] Values = new int[,] { {0, 10}, {20, 30} };

    public int this[int i, int j] { 
      get { return Values[i,j]; } 
      set { Values[i,j] = value; } 
    }
  }
  
  internal partial class PartialClassWithMethods {
    internal int Foo(){ return 1; }
  }
  
  public partial class ClassWithOverloads {
    public string Overloaded() { return "empty"; }
    public string Overloaded(int arg) { return "one arg"; }
    public string Overloaded(int arg1, int arg2) { return "two args"; }
    public string Tracker { get; set;}

    public string PublicProtectedOverload(){
      return "public overload";
    }
    
    protected string PublicProtectedOverload(string str) {
      return "protected overload";
    }

    #{method_block("void", "Void") {|el| "Tracker = \"#{el}\""}}
    #{method_block("string", "Ref") {|el| "return \"#{el}\""} }
    #{method_block("string[]", "RefArray") {|el| "return new string[]{\"#{el}\"}"} }
    #{method_block("int", "Val") {|el| "Tracker = \"#{el}\";\nreturn 1"} }
    #{method_block("int[]", "ValArray") {|el| "Tracker = \"#{el}\";\nreturn new int[]{1}" }}
    #{method_block("string", "Generic", "<T>") {|el| "return \"#{el}\" "}}
  }
  public class DerivedFromImplementsIInterface : ImplementsIInterface {}
  public struct StructImplementsIInterface : IInterface { public void m() {}}
  public partial class ClassWithMethods {
    public ClassWithMethods() {
      Tracker = new ArrayList();
    }
    public string PublicMethod() { return "public";}
    protected string ProtectedMethod() { return "protected";}
    private string PrivateMethod() { return "private";}
    public ArrayList Tracker { get; set;}
    #{@methods_string}
    public void Reset() { Tracker = new ArrayList();}
    public int SummingMethod(int a, int b){
      return a+b;
    }

    // no args
    public string NoArg() { return "NoArg";}

    //primitive types
    // 1. Ruby native

    public string Int32Arg(Int32 arg) { Tracker.Add(arg); return "Int32Arg";}                 // Fixnum
    public string DoubleArg(Double arg) { Tracker.Add(arg); return "DoubleArg";}              // Float
    public string BigIntegerArg(BigInteger arg) { Tracker.Add(arg); return "BigIntegerArg";}  // Bignum
    public string StringArg(String arg) { Tracker.Add(arg); return "StringArg";}              // String
    public string BooleanArg(Boolean arg) { Tracker.Add(arg); return "BooleanArg";}           // TrueClass/FalseClass/NilClass
    public string ObjectArg(object arg) { Tracker.Add(arg); return "ObjectArg";}              // Object 

    // 2. not Ruby native 
    // 2.1 -- signed
    public string SByteArg(SByte arg) { Tracker.Add(arg); return "SByteArg";}
    public string Int16Arg(Int16 arg) { Tracker.Add(arg); return "Int16Arg";}
    public string Int64Arg(Int64 arg) { Tracker.Add(arg); return "Int64Arg";}
    public string SingleArg(Single arg) { Tracker.Add(arg); return "SingleArg";}
    // 2.2 -- unsigned 
    public string ByteArg(Byte arg) { Tracker.Add(arg); return "ByteArg";}
    public string UInt16Arg(UInt16 arg) { Tracker.Add(arg); return "UInt16Arg";}
    public string UInt32Arg(UInt32 arg) { Tracker.Add(arg); return "UInt32Arg";}
    public string UInt64Arg(UInt64 arg) { Tracker.Add(arg); return "UInt64Arg";}
    // 2.3 -- special
    public string CharArg(Char arg) { Tracker.Add(arg); return "CharArg";}
    public string DecimalArg(Decimal arg) { Tracker.Add(arg); return "DecimalArg";}

    //
    // Reference type or value type
    //
    public string IInterfaceArg(IInterface arg) { Tracker.Add(arg); return "IInterfaceArg";}
    public string ImplementsIInterfaceArg(ImplementsIInterface arg) { Tracker.Add(arg); return "ImplementsIInterfaceArg";}
    public string DerivedFromImplementsIInterfaceArg(DerivedFromImplementsIInterface arg) { Tracker.Add(arg); return "DerivedFromImplementsIInterfaceArg";}
    public string CStructArg(CStruct arg) { Tracker.Add(arg); return "CStructArg";}
    public string StructImplementsIInterfaceArg(StructImplementsIInterface arg) { Tracker.Add(arg); return "StructImplementsIInterfaceArg";}

    public string AbstractClassArg(AbstractClass arg) { Tracker.Add(arg); return "AbstractClassArg";}
    public string DerivedFromAbstractArg(DerivedFromAbstract arg) { Tracker.Add(arg); return "DerivedFromAbstractArg";}

    public string CustomEnumArg(CustomEnum arg) { Tracker.Add(arg); return "CustomEnumArg";}
    public string EnumIntArg(EnumInt arg) { Tracker.Add(arg); return "EnumIntArg";}

    // 
    // array
    //
    public string Int32ArrArg(Int32[] arg) { Tracker.Add(arg); return "Int32ArrArg";}
    public string IInterfaceArrArg(IInterface[] arg) { Tracker.Add(arg); return "IInterfaceArrArg";}

    //
    // params array 
    //
    public string ParamsInt32ArrArg(params Int32[] arg) { Tracker.Add(arg); return "ParamsInt32ArrArg";}
    public string ParamsIInterfaceArrArg(params IInterface[] arg) { Tracker.Add(arg); return "ParamsIInterfaceArrArg";}
    public string ParamsCStructArrArg(params CStruct[] arg) { Tracker.Add(arg); return "ParamsCStructArrArg";}
    public string Int32ArgParamsInt32ArrArg(Int32 arg, params Int32[] arg2) { Tracker.Add(arg); return "Int32ArgParamsInt32ArrArg";}
    public string IInterfaceArgParamsIInterfaceArrArg(IInterface arg, params IInterface[] arg2) { Tracker.Add(arg); return "IInterfaceArgParamsIInterfaceArrArg";}

    //
    // collections/generics
    //
    public string IListOfIntArg(IList<int> arg) { Tracker.Add(arg); return "IListOfIntArg";} 
    public string ArrayArg(Array arg) { Tracker.Add(arg); return "ArrayArg";} 
    public string IEnumerableOfIntArg(IEnumerable<int> arg) { Tracker.Add(arg); return "IEnumerableOfIntArg";}
    public string IEnumeratorOfIntArg(IEnumerator<int> arg) { Tracker.Add(arg); return "IEnumeratorOfIntArg";}

    // Nullable
    public string NullableInt32Arg(Int32? arg) { Tracker.Add(arg); return "NullableInt32Arg";}

    // ByRef, Out
    public string RefInt32Arg(ref Int32 arg) { arg = 1; Tracker.Add(arg); return "RefInt32Arg";}
    public string OutInt32Arg(out Int32 arg) { arg = 2; Tracker.Add(arg); return "OutInt32Arg";}

    // Default Value
    public string DefaultInt32Arg([DefaultParameterValue(10)] Int32 arg) { Tracker.Add(arg); return "DefaultInt32Arg";}
    public string Int32ArgDefaultInt32Arg(Int32 arg, [DefaultParameterValue(10)] Int32 arg2) { Tracker.Add(arg); Tracker.Add(arg2); return "Int32ArgDefaultInt32Arg";}
  }

  public class VirtualMethodBaseClass { 
    public virtual string VirtualMethod() { return "virtual"; } 
  }
  public class VirtualMethodOverrideNew : VirtualMethodBaseClass { 
    new public virtual string VirtualMethod() { return "new"; } 
  }
  public class VirtualMethodOverrideOverride : VirtualMethodBaseClass {
    public override string VirtualMethod() { return "override"; } 
  }
EOL
  
no_csc do
  module BindingSpecs
    class MyString < String; end
    
    class RubyImplementsIInterface 
      include IInterface
      def m; end
    end

    class RubyDerivedFromImplementsIInterface < ImplementsIInterface; end

    class RubyDerivedFromDerivedFromImplementsIInterface < DerivedFromImplementsIInterface; end
    
    class RubyDerivedFromDerivedFromAbstractAndImplementsIInterface < DerivedFromAbstract 
      include IInterface
      def m; end
    end

    class RubyDerivedFromAbstract < AbstractClass; end
    
    class RubyDerivedFromDerivedFromAbstract < DerivedFromAbstract; end

    module Convert
      class ToI
        def to_i; 1; end
      end

      class ToInt
        def to_int; 1; end
      end

      class ToIntToI
        def to_i; 1; end

        def to_int; 2; end
      end

      class ToS
        def to_s; "to_s" end
      end

      class ToStr
        def to_str; "to_str" end
      end

      class ToStrToS
        def to_s; "to_s" end

        def to_str; "to_str" end
      end
    end
  end

  class Helper
    def self.run_matrix(results, input)
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
        
    def self.result(meth, value) 
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
               when /RefInt32/
                 1
               when /Int32ArgDefault/
                 [Fixnum.induced_from(value.to_int), 10]
               when /Int32|BigInteger/
                 if value.is_a?(Symbol) || value.is_a?(BindingSpecs::Convert::ToInt) || value.is_a?(BindingSpecs::Convert::ToIntToI)
                   value.to_int
                 end
               when /(S?Byte|U?Int16|UInt32|U?Int64)/
                 if value.is_a?(Symbol) || value.is_a?(BindingSpecs::Convert::ToInt) || value.is_a?(BindingSpecs::Convert::ToIntToI)
                   System.const_get($1).induced_from(value.to_int)
                 end
               when /EnumIntArg/
                 if value.is_a?(CustomEnum)
                   EnumInt.new
                 end
               when /CustomEnumArg/
                 if value.is_a?(EnumInt)
                   CustomEnum.new
                 end
               else
                 value
               end
      result.nil? ? result = value : nil
      result
    end
   
    # TODO: More BigIntegerValues near boundaries
    # TODO: More NullableIntegerValues near boundaries
    def self.numeric_and_string_args
      clr_values = {}
      #            Fixnum         Float
      clr_types = [System::Int32, System::Double, System::SByte, System::Int16, System::Int64, System::Single, System::Decimal]
      unsigned_clr_types = [System::Byte, System::UInt16, System::UInt32, System::UInt64]
      
      (clr_types + unsigned_clr_types).each do |e|
        clr_values["#{e.name}MaxValue"] = e.MaxValue
        clr_values["#{e.name}MaxValueMinusOne"] = e.induced_from((e.MaxValue - 1))
        clr_values["#{e.name}MinValue"] = e.MinValue
        clr_values["#{e.name}MinValuePlusOne"] = e.induced_from((e.MinValue + 1))
      end
      other = { "nil" => nil, "true" => true, "false" => false, "obj" => Object.new,
        "BigIntegerOne" => Bignum.One, "BigIntegerZero" => Bignum.Zero, 
        "Int32?Null" => System::Nullable[Fixnum].new, "Int32?One" => System::Nullable[Fixnum].new(1), "Int32?MinusOne" => System::Nullable[Fixnum].new(-1),
        "" => "", "a" => "a", "abc" => "abc",
        "System::String''" => System::String.new(""), "System::String'a'" => System::String.new("a"), "System::String'abc'" => System::String.new("abc"),
        "MyString''" => BindingSpecs::MyString.new(""), "MyString'a'" => BindingSpecs::MyString.new("a"), "MyString'abc'" => BindingSpecs::MyString.new("abc"),
        :a => :a, :abc => :abc,
        "Convert::ToI" => BindingSpecs::Convert::ToI.new, "Convert::ToInt" => BindingSpecs::Convert::ToInt.new, "Convert::ToIntToI" => BindingSpecs::Convert::ToIntToI.new,
        "Convert::ToS" => BindingSpecs::Convert::ToS.new, "Convert::ToStr" => BindingSpecs::Convert::ToStr.new, "Convert::ToStrToS" => BindingSpecs::Convert::ToStrToS.new,
        "System::CharMaxValue" => System::Char.MaxValue, "System::CharMinValue" => System::Char.MinValue
      }
      other.merge clr_values      
    end

    def self.classlike_args
      
       [BindingSpecs::RubyImplementsIInterface, ImplementsIInterface, BindingSpecs::RubyDerivedFromImplementsIInterface, DerivedFromImplementsIInterface,
        BindingSpecs::RubyDerivedFromDerivedFromImplementsIInterface, BindingSpecs::RubyDerivedFromDerivedFromAbstractAndImplementsIInterface, DerivedFromAbstract,
        BindingSpecs::RubyDerivedFromAbstract,BindingSpecs::RubyDerivedFromDerivedFromAbstract, 
        Class, Object, CStruct, StructImplementsIInterface, EnumInt, CustomEnum].inject({"anonymous class" => Class.new, "anonymous classInstance" => Class.new.new, "metaclass" => Object.new.metaclass, "AbstractClass" => AbstractClass}) do |result, klass|
          result[klass.name] = klass
          begin
            result["#{klass.name}Instance"] = klass.new 
          rescue Exception => e
            puts "Exception raised during instantiation of #{klass.name}"
            puts e
          end
          result
        end
    end

    private
    #returns random number between the given values. Using 0 for min value will
    #give an abnormlly high probability of 0 as the result.
    def self.rand_range(klass)
      klass.induced_from(rand * (rand> 0.5 ? klass.MinValue : klass.MaxValue))
    end
  end
  class RubyClassWithMethods < ClassWithMethods; end
end
