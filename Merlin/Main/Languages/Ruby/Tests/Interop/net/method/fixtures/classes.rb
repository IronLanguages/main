require File.dirname(__FILE__) + "/../../bcl/fixtures/classes"
reference "System.dll"
csc <<-EOL
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
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
  public partial class GenericClassWithMethods<K> {
    public ArrayList Tracker { get; set;}
    public GenericClassWithMethods() {
      Tracker = new ArrayList();
    }

    public void Reset() { Tracker.Clear();}
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
  
  public class ClassWithNullableMethods {
    public ClassWithNullableMethods() {
      Tracker = new ArrayList();
    }
    public ArrayList Tracker { get; set;}
    public void Reset() { Tracker.Clear(); }
    public Int16? Int16NullableProperty {get; set;}
    public Int32? Int32NullableProperty {get; set;}
    public Int64? Int64NullableProperty {get; set;}
    public UInt16? UInt16NullableProperty {get; set;}
    public UInt32? UInt32NullableProperty {get; set;}
    public UInt64? UInt64NullableProperty {get; set;}
    public Byte? ByteNullableProperty {get; set;}
    public SByte? SByteNullableProperty {get; set;}
    public Decimal? DecimalNullableProperty {get; set;}
    public Single? SingleNullableProperty {get; set;}
    public Double? DoubleNullableProperty {get; set;}
    public Char? CharNullableProperty {get; set;}
    public CustomEnum? CustomEnumNullableProperty {get; set;}
    public Boolean? BooleanNullableProperty {get; set;}
    
    
    public string Int16NullableArg(Int16? arg) { Tracker.Add(arg); return "Int16NullableArg"; }
    public string Int32NullableArg(Int32? arg) { Tracker.Add(arg); return "Int32NullableArg"; }
    public string Int64NullableArg(Int64? arg) { Tracker.Add(arg); return "Int64NullableArg"; }
    public string UInt16NullableArg(UInt16? arg) { Tracker.Add(arg); return "UInt16NullableArg"; }
    public string UInt32NullableArg(UInt32? arg) { Tracker.Add(arg); return "UInt32NullableArg"; }
    public string UInt64NullableArg(UInt64? arg) { Tracker.Add(arg); return "UInt64NullableArg"; }
    public string ByteNullableArg(Byte? arg) { Tracker.Add(arg); return "ByteNullableArg"; }
    public string SByteNullableArg(SByte? arg) { Tracker.Add(arg); return "SByteNullableArg"; }
    public string DecimalNullableArg(Decimal? arg) { Tracker.Add(arg); return "DecimalNullableArg"; }
    public string SingleNullableArg(Single? arg) { Tracker.Add(arg); return "SingleNullableArg"; }
    public string DoubleNullableArg(Double? arg) { Tracker.Add(arg); return "DoubleNullableArg"; }
    public string CharNullableArg(Char? arg) { Tracker.Add(arg); return "CharNullableArg"; }
    public string CustomEnumNullableArg(CustomEnum? arg) { Tracker.Add(arg); return "CustomEnumNullableArg"; }
    public string BooleanNullableArg(Boolean? arg) { Tracker.Add(arg); return "BooleanNullableArg"; }
  }

  public class StaticClassWithNullableMethods {
    private static ArrayList _tracker = new ArrayList();
    public static ArrayList Tracker { get { return _tracker;}}
    public static void Reset() { 
      Tracker.Clear(); 
      StaticInt16NullableProperty = null;
      StaticInt32NullableProperty = null;
      StaticInt64NullableProperty = null;
      StaticUInt16NullableProperty = null;
      StaticUInt32NullableProperty = null;
      StaticUInt64NullableProperty = null;
      StaticByteNullableProperty = null;
      StaticSByteNullableProperty = null;
      StaticDecimalNullableProperty = null;
      StaticSingleNullableProperty = null;
      StaticDoubleNullableProperty = null;
      StaticCharNullableProperty = null;
      StaticCustomEnumNullableProperty = null;
      StaticBooleanNullableProperty = null;
    }
    
    public static Int16? StaticInt16NullableProperty {get; set;}
    public static Int32? StaticInt32NullableProperty {get; set;}
    public static Int64? StaticInt64NullableProperty {get; set;}
    public static UInt16? StaticUInt16NullableProperty {get; set;}
    public static UInt32? StaticUInt32NullableProperty {get; set;}
    public static UInt64? StaticUInt64NullableProperty {get; set;}
    public static Byte? StaticByteNullableProperty {get; set;}
    public static SByte? StaticSByteNullableProperty {get; set;}
    public static Decimal? StaticDecimalNullableProperty {get; set;}
    public static Single? StaticSingleNullableProperty {get; set;}
    public static Double? StaticDoubleNullableProperty {get; set;}
    public static Char? StaticCharNullableProperty {get; set;}
    public static CustomEnum? StaticCustomEnumNullableProperty {get; set;}
    public static Boolean? StaticBooleanNullableProperty {get; set;}
    public static string StaticInt16NullableArg(Int16? arg) { Tracker.Add(arg); return "StaticInt16NullableArg"; }
    public static string StaticInt32NullableArg(Int32? arg) { Tracker.Add(arg); return "StaticInt32NullableArg"; }
    public static string StaticInt64NullableArg(Int64? arg) { Tracker.Add(arg); return "StaticInt64NullableArg"; }
    public static string StaticUInt16NullableArg(UInt16? arg) { Tracker.Add(arg); return "StaticUInt16NullableArg"; }
    public static string StaticUInt32NullableArg(UInt32? arg) { Tracker.Add(arg); return "StaticUInt32NullableArg"; }
    public static string StaticUInt64NullableArg(UInt64? arg) { Tracker.Add(arg); return "StaticUInt64NullableArg"; }
    public static string StaticByteNullableArg(Byte? arg) { Tracker.Add(arg); return "StaticByteNullableArg"; }
    public static string StaticSByteNullableArg(SByte? arg) { Tracker.Add(arg); return "StaticSByteNullableArg"; }
    public static string StaticDecimalNullableArg(Decimal? arg) { Tracker.Add(arg); return "StaticDecimalNullableArg"; }
    public static string StaticSingleNullableArg(Single? arg) { Tracker.Add(arg); return "StaticSingleNullableArg"; }
    public static string StaticDoubleNullableArg(Double? arg) { Tracker.Add(arg); return "StaticDoubleNullableArg"; }
    public static string StaticCharNullableArg(Char? arg) { Tracker.Add(arg); return "StaticCharNullableArg"; }
    public static string StaticCustomEnumNullableArg(CustomEnum? arg) { Tracker.Add(arg); return "StaticCustomEnumNullableArg"; }
    public static string StaticBooleanNullableArg(Boolean? arg) { Tracker.Add(arg); return "StaticBooleanNullableArg"; }
  }

public class GenericTypeInference {
    public static string Tx<T>(T x) {
        return typeof(T).ToString();
    }

    public static string TxTy<T>(T x, T y) {
        return typeof(T).ToString();
    }

    public static string TxTyTz<T>(T x, T y, T z) {
        return typeof(T).ToString();
    }

    public static string TParamsArrx<T>(params T[] x) {
        return typeof(T).ToString();
    }

    public static string TxTParamsArry<T>(T x, params T[] y) {
        return typeof(T).ToString();
    }

    public static string TRefx<T>(ref T x) {
        x = default(T);
        return typeof(T).ToString();
    }
    public static string TxClass<T>(T x) 
        where T : class {
        return typeof(T).ToString();
    }

    public static string TxStruct<T>(T x) 
        where T : struct {
        return typeof(T).ToString();
    }

    public static string TxIList<T>(T x) 
        where T : IList {
        return typeof(T).ToString();
    }

    public static string TxUyTConstrainedToU<T, U>(T x, U y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string ObjxUyTConstrainedToU<T, U>(object x, U y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxObjyTConstrainedToU<T, U>(T x, object y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxUyVzTConstrainedToUConstrainedToV<T, U, V>(T x, U y, V z)
        where T : U
        where U : V {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxUyVzTConstrainedToUConstrainedToClass<T, U, V>(T x, U y, V z)
        where T : U
        where U : class {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxUyVzTConstrainedToUConstrainedToIList<T, U, V>(T x, U y, V z)
        where T : U
        where U : IList {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TObjectx<T>(object x){
        return typeof(T).ToString();
    }

    public static string TxObjecty<T, U>(T x, object y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxFuncTy<T>(T x, Func<T> y) {
        return typeof(T).ToString();
    }

    public static string TxActionTy<T>(T x, Action<T> y) {
        return typeof(T).ToString();
    }

    public static string TxIListTy<T>(T x, IList<T> y) {
        return typeof(T).ToString();
    }

    public static string TxDictionaryTIListTy<T>(T x, Dictionary<T, IList<T>> y){
        return typeof(T).ToString();
    }

    public static string TxIEnumerableTy<T>(T x, IEnumerable<T> y) {
        return typeof(T).ToString();
    }

    public static string TxTConstrainedToIEnumerable<T>(T x) {
        return typeof(T).ToString();
    }

    public static string TxUyTConstrainedToIListU<T, U>(T x, U y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string TxUy<T, U>(T x, U y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public static string IEnumerableTx<T>(IEnumerable<T> x) {
        return typeof(T).ToString();
    }

    public static string IEnumerableTxFuncTBooly<T>(IEnumerable<T> x, Func<T, bool> y) {
        return typeof(T).ToString();
    }

    public static string IEnumerableTxFuncTIntBooly<T>(IEnumerable<T> x, Func<T, bool> y) {
        return typeof(T).ToString();
    }

    public static string ListTx<T>(List<T> x) {
        return typeof(T).ToString();
    }

    public static string ListListTx<T>(List<List<T>> x) {
        return typeof(T).ToString();
    }

    public static string DictionaryTTx<T>(Dictionary<T,T> x) {
        return typeof(T).ToString();
    }

    public static string ListTxClass<T>(List<T> x)
        where T : class {
        return typeof(T).ToString();
    }

    public static string ListTxStruct<T>(List<T> x)
        where T : struct {
        return typeof(T).ToString();
    }

    public static string ListTxNew<T>(List<T> x)
        where T : new() {
        return typeof(T).ToString();
    }
    
    public static string DictionaryDictionaryTTDictionaryTTx<T>(Dictionary<Dictionary<T,T>, Dictionary<T,T>> x) {
        return typeof(T).ToString();
    }

    public static string FuncTBoolxStruct<T>(Func<T, bool> x) 
        where T : struct {
        return typeof(T).ToString();
    }

    public static string FuncTBoolxIList<T>(Func<T, bool> x)
        where T : IList {
        return typeof(T).ToString();
    }

    public static string IListTxIListTy<T>(IList<T> x, IList<T> y) {
        return typeof(T).ToString();
    }
}
public class SelfEnumerable : IEnumerable<SelfEnumerable> {
    #region IEnumerable<Test> Members

    IEnumerator<SelfEnumerable> IEnumerable<SelfEnumerable>.GetEnumerator() {
        throw new NotImplementedException();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() {
        throw new NotImplementedException();
    }

    #endregion
}

public class GenericTypeInferenceInstance {
    public string Tx<T>(T x) {
        return typeof(T).ToString();
    }

    public string TxTy<T>(T x, T y) {
        return typeof(T).ToString();
    }

    public string TxTyTz<T>(T x, T y, T z) {
        return typeof(T).ToString();
    }

    public string TParamsArrx<T>(params T[] x) {
        return typeof(T).ToString();
    }

    public string TxTParamsArry<T>(T x, params T[] y) {
        return typeof(T).ToString();
    }

    public string TRefx<T>(ref T x) {
        x = default(T);
        return typeof(T).ToString();
    }

    public string TxClass<T>(T x) 
        where T : class {
        return typeof(T).ToString();
    }

    public string TxStruct<T>(T x) 
        where T : struct {
        return typeof(T).ToString();
    }

    public string TxIList<T>(T x) 
        where T : IList {
        return typeof(T).ToString();
    }

    public string TxUyTConstrainedToU<T, U>(T x, U y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string ObjxUyTConstrainedToU<T, U>(object x, U y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxObjyTConstrainedToU<T, U>(T x, object y)
        where T : U {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxUyVzTConstrainedToUConstrainedToV<T, U, V>(T x, U y, V z)
        where T : U
        where U : V {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxUyVzTConstrainedToUConstrainedToClass<T, U, V>(T x, U y, V z)
        where T : U
        where U : class {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxUyVzTConstrainedToUConstrainedToIList<T, U, V>(T x, U y, V z)
        where T : U
        where U : IList {
        return String.Format("{0}, {1}, {2}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TObjectx<T>(object x){
        return typeof(T).ToString();
    }

    public string TxObjecty<T, U>(T x, object y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxFuncTy<T>(T x, Func<T> y) {
        return typeof(T).ToString();
    }

    public string TxActionTy<T>(T x, Action<T> y) {
        return typeof(T).ToString();
    }

    public string TxIListTy<T>(T x, IList<T> y) {
        return typeof(T).ToString();
    }

    public string TxDictionaryTIListTy<T>(T x, Dictionary<T, IList<T>> y){
        return typeof(T).ToString();
    }

    public string TxIEnumerableTy<T>(T x, IEnumerable<T> y) {
        return typeof(T).ToString();
    }

    public string TxTConstrainedToIEnumerable<T>(T x) {
        return typeof(T).ToString();
    }

    public string TxUyTConstrainedToIListU<T, U>(T x, U y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string TxUy<T, U>(T x, U y) {
        return String.Format("{0}, {1}", typeof(T).ToString(), typeof(U).ToString());
    }

    public string IEnumerableTx<T>(IEnumerable<T> x) {
        return typeof(T).ToString();
    }

    public string IEnumerableTxFuncTBooly<T>(IEnumerable<T> x, Func<T, bool> y) {
        return typeof(T).ToString();
    }

    public string IEnumerableTxFuncTIntBooly<T>(IEnumerable<T> x, Func<T, bool> y) {
        return typeof(T).ToString();
    }

    public string ListTx<T>(List<T> x) {
        return typeof(T).ToString();
    }

    public string ListListTx<T>(List<List<T>> x) {
        return typeof(T).ToString();
    }

    public string DictionaryTTx<T>(Dictionary<T,T> x) {
        return typeof(T).ToString();
    }

    public string ListTxClass<T>(List<T> x)
        where T : class {
        return typeof(T).ToString();
    }

    public string ListTxStruct<T>(List<T> x)
        where T : struct {
        return typeof(T).ToString();
    }

    public string ListTxNew<T>(List<T> x)
        where T : new() {
        return typeof(T).ToString();
    }
    
    public string DictionaryDictionaryTTDictionaryTTx<T>(Dictionary<Dictionary<T,T>, Dictionary<T,T>> x) {
        return typeof(T).ToString();
    }

    public string FuncTBoolxStruct<T>(Func<T, bool> x) 
        where T : struct {
        return typeof(T).ToString();
    }

    public string FuncTBoolxIList<T>(Func<T, bool> x)
        where T : IList {
        return typeof(T).ToString();
    }

    public string IListTxIListTy<T>(IList<T> x, IList<T> y) {
        return typeof(T).ToString();
    }
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
    def self.binds(target, meth, input, result)
      lambda do
        target = eval target
        meth_call = (input == "NoArg" ? lambda { target.send(meth)} : lambda {target.send(meth, @values[input])})
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
    end

    def self.passes(target, meth, input, result)
      lambda do
        target = eval target
        value = @values[input]
        meth_call = (input == "NoArg" ? lambda { target.send(meth)} : lambda {target.send(meth, value)})
        res, ref = meth_call.call
        result = Helper.result(meth,value, input)
        target.tracker.should == [*result]
        if ref
          if result.is_a? ArrayList
            ref.should == result[0]
          else
            ref.should == result
          end
        end
      end
    end

    def self.run_matrix(results, input, keys)
      results[:OutInt32Arg] ||= TE
      keys.each do |meth|
        result = results[meth] || TE
        
        it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (ClassWithMethods)", &(binds("@target", meth, input, result))
      
        it "binds '#{meth}' for '#{input}' with '#{result.to_s}' (RubyClassWithMethods)", &(binds("@target2", meth, input, result))
      
        next if result.class == Class && result < Exception
        
        it "passes the correct input (#{input}) into method (#{meth}) (ClassWithMethods)", &(passes("@target", meth, input, result))
        
        it "passes the correct input (#{input}) into method (#{meth}) (RubyClassWithMethods)", &(passes("@target2", meth, input, result))
      end
    end

    def self.property_binds(target, meth, input, result)
      lambda do
        target = eval target
        value = @values[input]
        method = lambda {target.send("#{meth}=", value)}
        if result.class == Class && result < Exception
          method.should raise_error result
        else
          target.send(meth).should == nil
          method.call
          target.send(meth).should == value
        end
      end
    end
    def self.run_property_matrix(results, input, keys, targets_one_and_two = true)
      keys.each do |meth|
        result = results[meth] || TE
        
        it "binds property '#{meth}' with '#{input}' on (ClassWithMethods)", &(property_binds("@target", meth, input, result))
        next unless targets_one_and_two
        it "binds property '#{meth}' with '#{input}' on (RubyClassWithMethods)", &(property_binds("@target2", meth, input, result))
      end
    end
        
    class ConversionHelper
      class << self
        def convert_for_BooleanArg(v)
          !!v
        end
        alias_method :convert_for_RefBooleanArg, :convert_for_BooleanArg

        def convert_for_SingleArg(v)
          case v
          when System::UInt32, System::Int32
            System::Single.parse(v.to_s)
          when System::Char
            System::Single.induced_from(int32(v))
          when Float
            System::Convert.to_single(v)
          end
        end

        def convert_for_DoubleArg(v)
          System::Double.induced_from(int32(v)) if v.is_a? System::Char
        end
        
        def convert_for_DecimalArg(v)
          System::Decimal.induced_from(int32(v)) if v.is_a? System::Char
        end

        def convert_for_CharArg(v)
          case v
          when System::String
            v[0]
          when Symbol
            v.to_s[0..0]
          when String
            v[0..0]
          end
        end

        def convert_for_IEnumerableArg(v)
          if test_value(v)
            v.to_int
          else
            [v]
          end
        end
        alias_method :convert_for_IListArg, :convert_for_IEnumerableArg
        alias_method :convert_for_IListOfObjArg, :convert_for_IEnumerableArg
        alias_method :convert_for_IListOfIntArg, :convert_for_IEnumerableArg
        alias_method :convert_for_IEnumerableOfIntArg, :convert_for_IEnumerableArg
        alias_method :convert_for_ArrayArg, :convert_for_IEnumerableArg
        alias_method :convert_for_ArrayListArg, :convert_for_IEnumerableArg
        alias_method :convert_for_Int32ArrArg, :convert_for_IEnumerableArg
        alias_method :convert_for_ParamsInt32ArrArg, :convert_for_IEnumerableArg
        alias_method :convert_for_ParamsCStructArrArg, :convert_for_IEnumerableArg
        alias_method :convert_for_HashtableArg, :convert_for_IEnumerableArg
        
        def convert_for_RefInt32Arg(v)
          1
        end

        def convert_for_Int32ArgDefaultInt32Arg(v)
          [Fixnum.induced_from(v.to_int), 10]
        end

        def convert_for_Int32Arg(v)
          if test_value(v)
            v.to_int
          end
        end
        alias_method :convert_for_BigIntegerArg, :convert_for_Int32Arg
        alias_method :convert_for_DefaultInt32Arg, :convert_for_Int32Arg
        alias_method :convert_for_Int32ArgParamsInt32ArrArg, :convert_for_Int32Arg

        def convert_for_ParamsIInterfaceArrTestArg(v)
          if v == nil
            [true, [v]]
          else 
            [false, [v]]
          end
        end

        def convert_for_IListArg(v)
          [v,v.size]
        end
        alias_method :convert_for_IListOfCharArg, :convert_for_IListArg
        alias_method :convert_for_IListOfIntArg2, :convert_for_IListArg

        def convert_for_EnumIntArg(v)
          if v.is_a?(CustomEnum)
            EnumInt.new
          end
        end
        
        def convert_for_CustomEnumArg(v)
          if v.is_a?(EnumInt)
            CustomEnum.new
          end
        end

        [System::Byte, System::SByte, System::Int16, System::UInt16, System::UInt32, System::Int64, System::UInt64].each do |val|
          define_method("convert_for_#{val.name.gsub("System::","")}Arg") {|v| val.induced_from(v.to_int) if test_value(v)}
        end
        
        def convert_for_IEnumerableIteratingArg(v)
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
            convert_for_IEnumerableOfCharIteratingArg(v)
          end
        end

        def convert_for_ListOfIntArg(v)
          if [List[Fixnum], System::Array[Fixnum], List[Object], Array, 
              System::Array[System::Char], ArrayList, List[System::Char],
              System::Array[System::Byte], List[System::Byte], Hashtable, 
              DIntInt, DIntStr, DObjObj, Hash].any? {|e| v.is_a?(e)}
            ArrayList.new << v
          end
        end
        alias_method :convert_for_GenericArg, :convert_for_ListOfIntArg
        alias_method :convert_for_IDictionaryOfIntIntArg, :convert_for_ListOfIntArg

        def convert_for_RefByteArrArg(v)
          if v.is_a? String
            res = System::Array.of(System::Byte).new(v.size)
            i = 0
            v.each_byte {|e| res[i] = System::Byte.induced_from(e); i+=1}
            ArrayList.new << res
          elsif v.is_a? System::Array.of(System::Byte)
            ArrayList.new << v
          end
        end

        def convert_for_IEnumerableOfCharIteratingArg(v)
          if v.is_a? System::String
            v.to_s.split(//).map {|e| e.to_clr_string}
          end
        end

        def convert_for_IEnumeratorIteratingArg(v)
          if [TestList::TestListEnumerator, BindingSpecs::TestListOfInt::TestListEnumeratorOfInt, BindingSpecs::TestListOfChar::TestListEnumeratorOfChar].any? {|e| v.is_a? e}
            v.list
          end
        end
        
        def convert_for_IEnumeratorOfIntIteratingArg(v)
          if [BindingSpecs::TestListOfInt::TestListEnumeratorOfInt].any? {|e| v.is_a? e}
            v.list
          end
        end
        
        def convert_for_IEnumeratorOfCharIteratingArg(v)
          if [BindingSpecs::TestListOfChar::TestListEnumeratorOfChar].any? {|e| v.is_a? e}
            v.list
          end
        end

        def convert_for_DelegateArg(v)
          case v
          when IntIntDelegate
            11
          when Proc
            12
          when Method
            14
          end
        end
        alias_method :convert_for_IntIntDelegateArg, :convert_for_DelegateArg

        def convert_for_StringArg(arg)
          case arg
          when NilClass
            nil
          when Symbol
            arg.to_s
          when Fixnum
            arg.to_sym.to_s
          else
            arg.to_str
          end
        end
        
        def method_missing(meth, arg)
          if meth =~ /convert_for_.*?Arg/
            arg
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
    def self.result(meth, value, input) 
      result = if input == "NoArg"
                 case meth.to_s
                 when /Params(?:Int32|CStruct|IInterface)ArrArg/
                  [[]]
                 when /DefaultInt32Arg/
                   [10]
                 when /NoArg/
                   []
                 when /OutInt32Arg/
                   [2]
                 when /ParamsIInterfaceArrTestArg/
                   [false, []]
                 when /OutBoolean/
                   true
                 when /OutImplementsIInterface/
                   ImplementsIInterface.new
                 when /OutStructImplementsIInterface/
                   StructImplementsIInterface.new
                 when /OutNonByRefInt32Arg/
                   1
                 else
                   nil
                 end
               else
                 ConversionHelper.send("convert_for_#{meth}", value)
               end
      result.nil? ? result = value : nil
      result
    end
    
    def self.args
    # TODO: More BigIntegerValues near boundaries
    # TODO: More NullableIntegerValues near boundaries
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
      dii = DIntInt.new
      dii[1] = 1
      dii[2] = 2
      ht = Hashtable.new
      ht[1] = 1
      ht[2] = 2
      tl1 = TestList.new << 1 << 2 << 3
      tl2 = BindingSpecs::TestListOfInt.new << 1 << 2 << 3
      tl3 = BindingSpecs::TestListOfChar.new << System::Char.new("a") << System::Char.new("b") << System::Char.new("c")
      tl4 = TestList.new
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
      other.merge! clr_values      
      types = [BindingSpecs::RubyImplementsIInterface, ImplementsIInterface, BindingSpecs::RubyDerivedFromImplementsIInterface, DerivedFromImplementsIInterface,
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
      types.merge! other
      types.merge!({"ClassWithMethods" => ClassWithMethods.new, "int" => 1, "hash" => {1=> 1}, 
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
                "RubyImplementsIInterfaceInstance" => BindingSpecs::RubyImplementsIInterface.new, 
                "Int16?Value" => System::Nullable[System::Int16].new(1),
                "Int32?Value" => System::Nullable[System::Int32].new(1),
                "Int64?Value" => System::Nullable[System::Int64].new(1),
                "UInt16?Value" => System::Nullable[System::UInt16].new(1),
                "UInt32?Value" => System::Nullable[System::UInt32].new(1),
                "UInt64?Value" => System::Nullable[System::UInt64].new(1),
                "Byte?Value" => System::Nullable[System::Byte].new(1),
                "SByte?Value" => System::Nullable[System::SByte].new(1),
                "Decimal?Value" => System::Nullable[System::Decimal].new(1),
                "Single?Value" => System::Nullable[System::Single].new(1), 
                "Char?Value" => System::Nullable[System::Char].new("a"), 
                "Double?Value" => System::Nullable[System::Double].new(1),
                "Boolean?Value" => System::Nullable[System::Boolean].new(1), 
                "CustomEnum?Value" => System::Nullable[CustomEnum].new(CustomEnum.A),
                "obj" => Object.new, 
                "monkeypatched" => obj, 
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
                "TestListInstanceEmpty" => tl4, "TestListInstance" => tl1,
                "TestListEnumeratorInstanceEmpty" => tl4.get_enumerator, "TestListEnumeratorInstance" => tl1.get_enumerator, 
                "CStructInstance" => CStruct.new,
                "Int32Instance" => 1,
                "IInterfaceInstance" => BindingSpecs::RubyImplementsIInterface.new,
                "System::Collections::Generic::List[Fixnum]InstanceEmpty" => List[Fixnum].new, "System::Collections::Generic::List[Fixnum]Instance" => ( List[Fixnum].new << 1 << 2 << 3 ),
                "System::Collections::Generic::List[Object]InstanceEmpty" => List[Object].new, "System::Collections::Generic::List[Object]Instance" => ( List[Object].new << Object.new << Object.new << Object.new ),
      })
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
  class RubyClassWithNullableMethods < ClassWithNullableMethods; end
  class RubyStaticClassWithNullableMethods < StaticClassWithNullableMethods; end
  class RubyGenericTypeInference < GenericTypeInference
    def self.call_long_method(meth)
      method(meth).of(String).call("blah")
    end

    def self.call_short_method(meth)
      send(meth, "blah")
    end
  end
  class RubyGenericTypeInferenceInstance < GenericTypeInferenceInstance
    def call_long_method(meth)
      method(meth).of(String).call("blah")
    end

    def call_short_method(meth)
      send(meth, "blah")
    end
  end
end
