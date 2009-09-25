require File.dirname(__FILE__) + "/../../bcl/fixtures/classes"
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
    #{method_block("int", "Val") {|el| "Tracker = \"#{el}\";return 1"} }
    #{method_block("int[]", "ValArray") {|el| "Tracker = \"#{el}\";return new int[]{1}" }}
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
    private static ArrayList _staticTracker = new ArrayList();
    public static ArrayList StaticTracker { get { return _staticTracker;}}
    #{@methods_string}
    public void Reset() { Tracker.Clear(); }
    public static void StaticReset() { StaticTracker.Clear(); }
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
    public string ObjectArrArg(object[] arg) { Tracker.Add(arg); return "ObjectArrArg";}
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
    public string IListOfObjArg(IList<object> arg) { Tracker.Add(arg); return "IListOfObjArg";} 
    public string ArrayArg(Array arg) { Tracker.Add(arg); return "ArrayArg";} 
    public string IEnumerableOfIntArg(IEnumerable<int> arg) { Tracker.Add(arg); return "IEnumerableOfIntArg";}
    public string IEnumeratorOfIntArg(IEnumerator<int> arg) { Tracker.Add(arg); return "IEnumeratorOfIntArg";}
    public string IEnumerableArg(IEnumerable arg) { Tracker.Add(arg); return "IEnumerableArg";}
    public string IEnumeratorArg(IEnumerator arg) { Tracker.Add(arg); return "IEnumeratorArg";}
    public string ArrayListArg(ArrayList arg) { Tracker.Add(arg); return "ArrayListArg";}
    public string IDictionaryOfObjectObjectArg(IDictionary<object, object> arg) { Tracker.Add(arg); return "IDictionaryOfObjectObjectArg";}
    public string IDictionaryOfIntStringArg(IDictionary<int, string> arg) { Tracker.Add(arg); return "IDictionaryOfIntStringArg";}
    public string DictionaryOfObjectObjectArg(Dictionary<object, object> arg) { Tracker.Add(arg); return "DictionaryOfObjectObjectArg";}
    public string DictionaryOfIntStringArg(Dictionary<int, string> arg) { Tracker.Add(arg); return "DictionaryOfIntStringArg";}

    // Nullable
    public string NullableInt32Arg(Int32? arg) { Tracker.Add(arg); return "NullableInt32Arg";}

    // ByRef, Out
    public string RefInt32Arg(ref Int32 arg) { arg = 1; Tracker.Add(arg); return "RefInt32Arg";}
    public string OutInt32Arg(out Int32 arg) { arg = 2; Tracker.Add(arg); return "OutInt32Arg";}

    // Default Value
    public string DefaultInt32Arg([DefaultParameterValue(10)] Int32 arg) { Tracker.Add(arg); return "DefaultInt32Arg";}
    public string Int32ArgDefaultInt32Arg(Int32 arg, [DefaultParameterValue(10)] Int32 arg2) { Tracker.Add(arg); Tracker.Add(arg2); return "Int32ArgDefaultInt32Arg";}

    // static
    public static string StaticMethodNoArg() { StaticTracker.Add(null); return "StaticMethodNoArg";}
    public static string StaticMethodClassWithMethodsArg(ClassWithMethods arg) {StaticTracker.Add(arg); return "StaticMethodClassWithMethodsArg";}
    public string ClassWithMethodsArg(ClassWithMethods arg) {Tracker.Add(arg); return "ClassWithMethodsArg";}

    // generic method
    public string GenericArg<T>(T arg) {Tracker.Add(arg); return String.Format("GenericArg[{0}]", typeof(T));}

    // out on non-byref
    public string OutNonByRefInt32Arg([Out] int arg) {arg = 1; Tracker.Add(arg); return "OutNonByRefInt32Arg";}
    
    // what does passing in nil mean?
    public string ParamsIInterfaceArrTestArg(params IInterface[] args) { Tracker.Add(args == null); Tracker.Add(args); return "ParamsIInterfaceArrTestArg";}

    // ref, out, ...
    public string RefOutInt32Args(ref int arg1, out int arg2, int arg3) {arg1=arg2=arg3; Tracker.Add(arg1); Tracker.Add(arg2); Tracker.Add(arg3); return "RefOutInt32Args";}
    public string RefInt32OutArgs(ref int arg1, int arg2, out int arg3) {arg3=arg1=arg2; Tracker.Add(arg1); Tracker.Add(arg2); Tracker.Add(arg3); return "RefInt32OutArgs";}
    public string Int32RefOutArgs(int arg1, ref int arg2, out int arg3) {arg2=arg3=arg1; Tracker.Add(arg1); Tracker.Add(arg2); Tracker.Add(arg3); return "Int32RefOutArgs";}

    // eight args
    public string EightArgs(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) {
      Tracker.Add(arg1);
      Tracker.Add(arg2);
      Tracker.Add(arg3);
      Tracker.Add(arg4);
      Tracker.Add(arg5);
      Tracker.Add(arg6);
      Tracker.Add(arg7);
      Tracker.Add(arg8);
      return "EightArgs";
    }

    public string IDictionaryOfIntIntArg(IDictionary<int, int> arg){ Tracker.Add(arg); return "IDictionaryOfIntIntArg";}
    public string HashtableArg(Hashtable arg) { Tracker.Add(arg); return "HashtableArg";}
    public string ListOfIntArg(List<int> arg) { Tracker.Add(arg); return "ListOfIntArg";}

    // iterator support
    public string IEnumerableIteratingArg(IEnumerable arg) {
      IEnumerator ienum = arg.GetEnumerator(); 
      while (ienum.MoveNext()) 
        Tracker.Add(ienum.Current); 
      return "IEnumerableIteratingArg";
    }
    public string IEnumeratorIteratingArg(IEnumerator arg) {
      while (arg.MoveNext())
        Tracker.Add(arg.Current);
      return "IEnumeratorIteratingArg";
    }
    public string IListArg(IList arg) { Tracker.Add(arg); Tracker.Add(arg.Count); return "IListArg";}

    public string IEnumerableOfCharIteratingArg(IEnumerable<Char> arg) {
      IEnumerator ienum = arg.GetEnumerator(); 
      while (ienum.MoveNext()) 
        Tracker.Add(ienum.Current); 
      return "IEnumerableOfCharIteratingArg";
    }
    public string IEnumeratorOfCharIteratingArg(IEnumerator<Char> arg) {
      while (arg.MoveNext())
        Tracker.Add(arg.Current);
      return "IEnumeratorOfCharIteratingArg";
    }
    public string IListOfCharArg(IList<Char> arg) { Tracker.Add(arg); Tracker.Add(arg.Count); return "IListOfCharArg";}

    public string IEnumerableOfIntIteratingArg(IEnumerable<int> arg) {
      IEnumerator ienum = arg.GetEnumerator(); 
      while (ienum.MoveNext()) 
        Tracker.Add(ienum.Current); 
      return "IEnumerableOfIntIteratingArg";
    }
    public string IEnumeratorOfIntIteratingArg(IEnumerator<int> arg) {
      while (arg.MoveNext())
        Tracker.Add(arg.Current);
      return "IEnumeratorOfIntIteratingArg";
    }
    public string IListOfIntArg2(IList<int> arg) { Tracker.Add(arg); Tracker.Add(arg.Count); return "IListOfIntArg2";}

    // delegate
    public string DelegateArg(Delegate arg) {
      IntIntDelegate d = (IntIntDelegate)arg;
      Tracker.Add(d(10));
      return "DelegateArg";
    }

    public string IntIntDelegateArg(IntIntDelegate arg) { Tracker.Add(arg(10)); return "IntIntDelegateArg";}

    // byte array
    public string RefByteArrArg(ref Byte[] arg) { Tracker.Add(arg); return "RefByteArrArg";}
    public string ByteArrRefByteArrArg(Byte[] input, ref Byte[] arg) { arg = input; Tracker.Add(arg); return "ByteArrRefByteArrArg";}

    // keywords
    public string KeywordsArgs(int arg1, object arg2, ref string arg3) { arg3 = arg3.ToUpper(); Tracker.Add(arg3); return "KeywordsArgs";}

    //more ref/out
    public string RefStructImplementsIInterfaceArg(ref StructImplementsIInterface arg) { arg = new StructImplementsIInterface(); Tracker.Add(arg); return "RefStructImplementsIInterfaceArg";}
    public string OutStructImplementsIInterfaceArg(out StructImplementsIInterface arg) { arg = new StructImplementsIInterface(); Tracker.Add(arg); return "OutStructImplementsIInterfaceArg";}
    public string RefImplementsIInterfaceArg(ref ImplementsIInterface arg) { Tracker.Add(arg); return "RefImplementsIInterfaceArg";}
    public string OutImplementsIInterfaceArg(out ImplementsIInterface arg) { arg = new ImplementsIInterface(); Tracker.Add(arg); return "OutImplementsIInterfaceArg";}
    public string RefBooleanArg(ref Boolean arg) { Tracker.Add(arg); return "RefBooleanArg";}
    public string OutBooleanArg(out Boolean arg) { arg = true; Tracker.Add(arg); return "OutBooleanArg";}
    public string RefInt32Int32OutInt32Arg(ref int arg1, int arg2, out int arg3) { 
      arg3 = arg1 + arg2;
      arg1 = 100;
      Tracker.Add(arg1);
      Tracker.Add(arg2);
      Tracker.Add(arg3);
      return "RefInt32Int32OutInt32Arg";
    }
  }

  public struct StructWithMethods {
    private short _shortField;
    public short ShortField {
      get { 
        return _shortField;
      }
      set {
        _shortField = value;
      }
    }
  }

  public partial class GenericClassWithMethods<K> {
    public ArrayList Tracker { get; set;}
    public GenericClassWithMethods() {
      Tracker = new ArrayList();
    }
    public string GenericArg(K arg) { Tracker.Add(arg); return "GenericArg";}
  }

  public delegate int IntIntDelegate(int arg);

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
  include System::Collections
  include System::Collections::Generic
  TE = TypeError
  AE = ArgumentError
  OE = System::OverflowException
  RE = RangeError 
  SAF = System::Array[Fixnum]
  SAO = System::Array[Object]
  SAI = System::Array[IInterface]
  SAC = System::Array[CStruct]
  DObjObj = Dictionary[Object, Object]
  DIntStr = Dictionary[Fixnum, System::String]
  DIntInt = Dictionary[Fixnum, Fixnum]
  module BindingSpecs
    class ImplementsEnumerable
      include Enumerable
      def initialize
        @store = [1,2,3]
      end

      def reset
        @store = [1,2,3]
      end

      def each
        @store.each {|i| yield i}
      end
    end
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
    
    class TestListOfInt
      include IEnumerable.of(Fixnum)
      def initialize
        @store = []
      end

      def get_enumerator
        TestListEnumeratorOfInt.new(@store)
      end

      def <<(val)
        @store << val
        self
      end

      class TestListEnumeratorOfInt
        include IEnumerator.of(Fixnum)
        attr_reader :list
        def initialize(list)
          @list = list
          @position = -1
        end

        def move_next
          @position += 1
          valid?
        end

        def reset
          @position = -1
        end

        def valid?
          @position != -1 && @position < @list.length
        end

        def current
          if valid?
            @list[@position]
          else
            raise System::InvalidOperationException.new
          end
        end
      end
    end
    
    class TestListOfChar
      include IEnumerable.of(System::Char)
      def initialize
        @store = []
      end

      def get_enumerator
        TestListEnumeratorOfChar.new(@store)
      end

      def <<(val)
        @store << val
        self
      end

      class TestListEnumeratorOfChar
        include IEnumerator.of(System::Char)
        attr_reader :list
        def initialize(list)
          @list = list
          @position = -1
        end

        def move_next
          @position += 1
          valid?
        end

        def reset
          @position = -1
        end

        def valid?
          @position != -1 && @position < @list.length
        end

        def current
          if valid?
            @list[@position]
          else
            raise System::InvalidOperationException.new
          end
        end
      end
    end
  end

  class Helper
    def self.run_matrix(results, input, keys)
      results[:OutInt32Arg] ||= TE
      keys.each do |meth|
        result = results[meth] || TE
        it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (ClassWithMethods)" do
          meth_call = (input == "NoArg" ? lambda { @target.send(meth)} : lambda {@target.send(meth, @values[input])})
          if result.class == Class && result < Exception
            meth_call.should raise_error result
          elsif result.class == Regexp
            res, ref = meth_call.call
            (res =~ result).should == 0
          else
            res, ref = meth_call.call
            res.should == result
          end
        end
      
        it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (RubyClassWithMethods)" do
          meth_call = (input == "NoArg" ? lambda { @target2.send(meth)} : lambda {@target2.send(meth, @values[input])})
          if result.class == Class && result < Exception
            meth_call.should raise_error result
          elsif result.class == Regexp
            res, ref = meth_call.call
            (res =~ result).should == 0
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
                     when /Params(?:Int32|CStruct|IInterface)ArrArg/
                      [[]]
                     when /DefaultInt32Arg/
                       [10]
                     when /NoArg/
                       []
                     when /OutInt32Arg/
                       [2]
                     else
                       nil
                     end
            @target.tracker.should == result
          end
          if ref
            if result.is_a? ArrayList
              ref.should == result[0]
            else
              ref.should == result
            end
          end
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
                     when /Params(?:Int32|CStruct|IInterface)ArrArg/
                      [[]]
                     when /DefaultInt32Arg/
                       [10]
                     when /NoArg/
                       []
                     when /OutInt32Arg/
                       [2]
                     else
                       nil
                     end
            @target2.tracker.should == result
          end
          if ref
            if result.is_a? ArrayList
              ref.should == result[0]
            else
              ref.should == result
            end
          end
        end
      end
    end
        
    class ResultHelper
      class << self
        def test_BooleanArg(v)
          !!v
        end
        alias_method :test_RefBooleanArg, :test_BooleanArg

        def test_SingleArg(v)
          case v
          when System::UInt32, System::Int32
            System::Single.parse(v.to_s)
          when System::Char
            System::Single.induced_from(int32(v))
          when Float
            System::Convert.to_single(v)
          end
        end

        def test_DoubleArg(v)
          System::Double.induced_from(int32(v)) if v.is_a? System::Char
        end
        
        def test_DecimalArg(v)
          System::Decimal.induced_from(int32(v)) if v.is_a? System::Char
        end

        def test_CharArg(v)
          case v
          when System::String
            v[0]
          when Symbol
            v.to_s[0..0]
          when String
            v[0..0]
          end
        end

        def test_IEnumerableArg(v)
          if test_value(v)
            v.to_int
          else
            [v]
          end
        end
        alias_method :test_IListArg, :test_IEnumerableArg
        alias_method :test_IListOfObjArg, :test_IEnumerableArg
        alias_method :test_IListOfIntArg, :test_IEnumerableArg
        alias_method :test_IEnumerableOfIntArg, :test_IEnumerableArg
        alias_method :test_ArrayArg, :test_IEnumerableArg
        alias_method :test_ArrayListArg, :test_IEnumerableArg
        alias_method :test_Int32ArrArg, :test_IEnumerableArg
        alias_method :test_ParamsInt32ArrArg, :test_IEnumerableArg
        alias_method :test_ParamsCStructArrArg, :test_IEnumerableArg
        
        def test_RefInt32Arg(v)
          1
        end

        def test_Int32ArgDefaultInt32Arg(v)
          [Fixnum.induced_from(v.to_int), 10]
        end

        def test_Int32Arg(v)
          if test_value(v)
            v.to_int
          end
        end
        alias_method :test_BigIntegerArg, :test_Int32Arg
        alias_method :test_DefaultInt32Arg, :test_Int32Arg
        alias_method :test_Int32ArgParamsInt32ArrArg, :test_Int32Arg

        def test_EnumIntArg(v)
          if v.is_a?(CustomEnum)
            EnumInt.new
          end
        end
        
        def test_CustomEnumArg(v)
          if v.is_a?(EnumInt)
            CustomEnum.new
          end
        end

        [System::Byte, System::SByte, System::Int16, System::UInt16, System::UInt32, System::Int64, System::UInt64].each do |val|
          define_method("test_#{val.name.gsub("System::","")}Arg") {|v| val.induced_from(v.to_int) if test_value(v)}
        end
        
        def test_IEnumerableIteratingArg(v)
          case v
          when Hash
            KeyValuePair.of(Object,Object).new(1,1)
          when DObjObj
            [KeyValuePair.of(Object,Object).new(1,1),KeyValuePair.of(Object,Object).new(2,2)]
          when DIntInt
            [KeyValuePair.of(Fixnum,Fixnum).new(1,1),KeyValuePair.of(Fixnum,Fixnum).new(2,2)]
          when DIntStr
            [KeyValuePair.of(Fixnum, System::String).new(1,"1".to_clr_string),KeyValuePair.of(Fixnum, System::String).new(2,"2".to_clr_string)]
          when Hashtable
            [DictionaryEntry.new(2,2), DictionaryEntry.new(1,1)]
          when System::String
            test_IEnumerableOfCharIteratingArg(v)
          end
        end

        def test_ListOfIntArg(v)
          if [List[Fixnum], System::Array[Fixnum], List[Object], Array, 
              System::Array[System::Char], ArrayList, List[System::Char],
              System::Array[System::Byte], List[System::Byte]].any? {|e| v.is_a?(e)}
            ArrayList.new << v
          end
        end
        alias_method :test_GenericArg, :test_ListOfIntArg

        def test_RefByteArrArg(v)
          if v.is_a? String
            res = System::Array.of(System::Byte).new(v.size)
            i = 0
            v.each_byte {|e| res[i] = System::Byte.induced_from(e); i+=1}
            ArrayList.new << res
          elsif v.is_a? System::Array.of(System::Byte)
            ArrayList.new << v
          end
        end

        def test_IEnumerableOfCharIteratingArg(v)
          if v.is_a? System::String
            v.to_s.split(//).map {|e| e.to_clr_string}
          end
        end

        def test_IEnumeratorIteratingArg(v)
          if [TestList::TestListEnumerator, BindingSpecs::TestListOfInt::TestListEnumeratorOfInt, BindingSpecs::TestListOfChar::TestListEnumeratorOfChar].any? {|e| v.is_a? e}
            v.list
          end
        end
        
        def test_IEnumeratorOfIntIteratingArg(v)
          if [BindingSpecs::TestListOfInt::TestListEnumeratorOfInt].any? {|e| v.is_a? e}
            v.list
          end
        end
        
        def test_IEnumeratorOfCharIteratingArg(v)
          if [BindingSpecs::TestListOfChar::TestListEnumeratorOfChar].any? {|e| v.is_a? e}
            v.list
          end
        end

        def test_DelegateArg(v)
          case v
          when IntIntDelegate
            11
          when Proc
            12
          when Method
            14
          end
        end
        alias_method :test_IntIntDelegateArg, :test_DelegateArg

        def method_missing(meth, *args, &blk)
          if meth =~ /test_.*?Arg/
            value
          end
        end
        
        private
        def int32(v)
          System::Convert.to_int32(v)
        end

        def test_value(value)
          value.is_a?(Symbol) || value.is_a?(BindingSpecs::Convert::ToInt) || value.is_a?(BindingSpecs::Convert::ToIntToI)
        end
      end
    end
    def self.result(meth, value) 
      result = ResultHelper.send("test_#{meth}", value)
      result.nil? ? result = value : nil
      result
    end
    
    def self.collection_args
      #RubyArray , RubyHash , RubyClassWithEnumerable, IO, String, System::Array[int], System::Array[Object], IList, IEnumerable, IEnumerator, monkeypatched object
      obj = Object.new
      class << obj
        include Enumerable
        def each
          [1,2,3].each {|i| yield i}
        end

        def m;1;end
      end

      dobj = DObjObj.new
      dobj[1] = 1
      dobj[2] = 2
      dint = DIntStr.new
      dint[1] = "1".to_clr_string
      dint[2] = "2".to_clr_string
      tl1 = TestList.new
      tl2 = TestList.new << 1 << 2 << 3
      {"monkeypatched" => obj, 
      "ArrayInstanceEmpty" => [], "ArrayInstance" => [1,2,3], 
      "HashInstanceEmpty" => {}, "HashInstance" => {1=>2,3=>4,5=>6}, 
      "ImplementsEnumerableInstance" => BindingSpecs::ImplementsEnumerable.new, 
      "StringInstanceEmpty" => "", "StringInstance" => "abc", 
      #[]                                                       [2,2]
      "System::Array[Fixnum]InstanceEmpty" => SAF.new(0), "System::Array[Fixnum]Instance" => SAF.new(2,2), 
      #[]                                                       [Object.new,Object.new]
      "System::Array[Object]InstanceEmpty" => SAO.new(0), "System::Array[Object]Instance" => SAO.new(2,Object.new), 
      "System::Array[IInterface]InstanceEmpty" => SAI.new(0), "System::Array[IInterface]Instance" => SAI.new(2, BindingSpecs::RubyImplementsIInterface.new),
      "System::Array[CStruct]InstanceEmpty" => SAC.new(0), "System::Array[CStruct]Instance" => SAC.new(2, CStruct.new),
      "ArrayListInstanceEmpty" => ArrayList.new, "ArrayListInstance" => (ArrayList.new << 1 << 2 << 3),
      #{}                                                       {1=>1,2=>2}
      "Dictionary[Object,Object]InstanceEmpty" => DObjObj.new, "Dictionary[Object,Object]Instance" => dobj,
      #{}                                                       {1=>"1",2=>"2"}
      "Dictionary[Fixnum,String]InstanceEmpty" => DIntStr.new, "Dictionary[Fixnum,String]Instance" => dint,
      "TestListInstanceEmpty" => tl1, "TestListInstance" => tl2,
      "TestListEnumeratorInstanceEmpty" => tl1.get_enumerator, "TestListEnumeratorInstance" => tl2.get_enumerator, 
      "CStructInstance" => CStruct.new,
      "Int32Instance" => 1,
      "IInterfaceInstance" => BindingSpecs::RubyImplementsIInterface.new,
      "System::Collections::Generic::List[Fixnum]InstanceEmpty" => List[Fixnum].new, "System::Collections::Generic::List[Fixnum]Instance" => ( List[Fixnum].new << 1 << 2 << 3 ),
      "System::Collections::Generic::List[Object]InstanceEmpty" => List[Object].new, "System::Collections::Generic::List[Object]Instance" => ( List[Object].new << Object.new << Object.new << Object.new ),
      }
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
    
    def self.other_concern_args

      dobj = DObjObj.new
      dobj[1] = 1
      dobj[2] = 2
      dint = DIntStr.new
      dint[1] = "1".to_clr_string
      dint[2] = "2".to_clr_string
      dii = DIntInt.new
      dii[1] = 1
      dii[2] = 2
      ht = Hashtable.new
      ht[1] = 1
      ht[2] = 2
      tl1 = TestList.new << 1 << 2 << 3
      tl2 = BindingSpecs::TestListOfInt.new << 1 << 2 << 3
      tl3 = BindingSpecs::TestListOfChar.new << System::Char.new("a") << System::Char.new("b") << System::Char.new("c")
      types = {"ClassWithMethods" => ClassWithMethods.new, "int" => 1, "nil" => nil, "hash" => {1=> 1}, 
               "Dictionary[obj,obj]" => dobj, "Dictionary[int,str]" => dint, "Dictionary[int,int]" => dii,
               "hashtable" => ht, "List[int]" => ( List[Fixnum].new << 1 << 2 << 3 ),
               "List[obj]" => ( List[Object].new << 1 << 2 << 3), "Array[int]" => System::Array.of(Fixnum).new(2,3),
               "String" => "String", "Array" => [Object.new, 1, :blue], "Array[Char]" => System::Array.of(System::Char).new(2, System::Char.new("a")),
               "System::String" => "System::String".to_clr_string, "Array[System::String]" => System::Array.of(System::String).new(2, "a".to_clr_string),
               "ArrayList" => (ArrayList.new << 1 << 2 << 3),  "List[Char]" => ( List[System::Char].new << System::Char.new("a") << System::Char.new("b") << System::Char.new("c")), 
               "IEnumerator" => tl1.get_enumerator, "IEnumerator[int]" => tl2.get_enumerator, "IEnumerator[Char]" => tl3.get_enumerator,
               "IntIntDelegate" => IntIntDelegate.new {|a| a+1 }, "lambda" => lambda {|a| a+2}, 
              "proc" => proc {|a| a+2}, "method" => method(:test_method),
               "unboundmethod" => method(:test_method).unbind, "bool" => true, "Array[byte]" => System::Array.of(System::Byte).new(2, System::Byte.MinValue), 
               "List[byte]" => (List[System::Byte].new << System::Byte.parse("1") << System::Byte.parse("2") << System::Byte.parse("3")), 
               "self" => "self", "class" => "class", "this" => "this", "public" => "public",
               "StructImplementsIInterface" => StructImplementsIInterface.new, "RubyImplementsIInterface" => BindingSpecs::RubyImplementsIInterface.new, 
               "ImplementsIInterface" => ImplementsIInterface.new
               }
      #TODO: Add the byref types when make_by_ref_type works
      #byrefStructImplementsIInterfaceInstance
      #byrefRubyImplementsIInterface
      #byrefImpelemntsIInterface, byrefbool
      #ref_types = {"ByRefInt" => Fixnum.get_type.make_by_ref_type, "Array[ByRefByte]" => System::Array.of(System::Byte.get_type.make_by_ref_type),
                   #"List[ByRefByte]" => List[System::Byte.get_type.make_by_ref_type]} 
      types
    end

    
    def self.test_method(a)
      a+4
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
