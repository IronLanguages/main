using System;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Hosting;
  using IronRuby.Runtime;
  using IronRuby.Builtins;
#line 4 "./bcl/array/conversion_spec.rb"
public partial class Klass {
      public T[] ArrayAcceptingMethod<T>(T[] arg0) {
        return arg0;
      }
    }
#line 34 "./bcl/equality/equality_spec.rb"
public static class EqualityChecker {
      public static new bool Equals(object o1, object o2) { return o1.Equals(o2); }
    }
    
    public class Equatable {
      public override bool Equals(object other) { return (other is string) && ((string)other) == "ClrMarker"; }
      public override int GetHashCode() { throw new NotImplementedException(); }
    }
#line 38 "./bcl/equality/hashing_spec.rb"
public static class Hasher {
      public static int GetHashCode(object o) { return o.GetHashCode(); }
    }
    
    public class Hashable {
      public override int GetHashCode() { return 123; }
    }
#line 10 "./bcl/icomparable/comparable_spec.rb"
#line 13 "./bcl/icomparable/comparable_spec.rb"
public class IComparableConsumer {
    public static int Consume(IComparable icomp) {
      return icomp.CompareTo(1);
    }
  }
  
  public class IComparableProvider {
  
  }
#line 4 "./bcl/ienumerable/implementation_spec.rb"
#line 7 "./bcl/ienumerable/implementation_spec.rb"
public interface ITestList : IEnumerable {
  }

  public class Tester {
    public ArrayList Test(ITestList list) {
      ArrayList l = new ArrayList();
      foreach(var item in list) {
        l.Add(item);
      }
      return l;
    }
  }
#line 5 "./bcl/numerics/byte_spec.rb"
public partial class NumericHelper {
    public static int SizeOfByte() {
      return sizeof(Byte);
    }
  }
#line 4 "./bcl/numerics/decimal_spec.rb"
public partial class Klass {
    public decimal MyDecimal {get; set;}
  }
#line 5 "./bcl/numerics/int16_spec.rb"
public partial class NumericHelper {
    public static int SizeOfInt16() {
      return sizeof(Int16);
    }
  }
#line 5 "./bcl/numerics/int32_spec.rb"
public partial class NumericHelper {
    public static int SizeOfInt32() {
      return sizeof(Int32);
    }
  }
#line 5 "./bcl/numerics/int64_spec.rb"
public partial class NumericHelper {
    public static int SizeOfInt64() {
      return sizeof(Int64);
    }
  }
#line 5 "./bcl/numerics/sbyte_spec.rb"
public partial class NumericHelper {
    public static int SizeOfSByte() {
      return sizeof(SByte);
    }
  }
#line 5 "./bcl/numerics/uint16_spec.rb"
public partial class NumericHelper {
    public static int SizeOfUInt16() {
      return sizeof(UInt16);
    }
  }
#line 5 "./bcl/numerics/uint32_spec.rb"
public partial class NumericHelper {
    public static int SizeOfUInt32() {
      return sizeof(UInt32);
    }
  }
#line 5 "./bcl/numerics/uint64_spec.rb"
public partial class NumericHelper {
    public static int SizeOfUInt64() {
      return sizeof(UInt64);
    }
  }
#line 4 "./bcl/string/construction_spec.rb"
public partial class Klass {
    public string A(){
      return "a";
    }

    public string Aa(){
      return "aa";
    }
  }
#line 5 "./class/derivation/attribute_spec.rb"
#line 9 "./class/derivation/attribute_spec.rb"
public class ClassWithOptionalConstructor {
      public int Arg {get; set;}
      
      public ClassWithOptionalConstructor([Optional]int arg) {
        Arg = arg;
      }
    }
#line 33 "./class/derivation/attribute_spec.rb"
public abstract class Unsafe {
        [return: MarshalAs(UnmanagedType.U1)]
        public virtual bool Foo() { return true;}
      }
#line 14 "./class/instantiation/abstract_spec.rb"
public partial class DerivedFromAbstract : AbstractClass {
    public override int m() {return 1;}
  }
#line 32 "./class/instantiation/abstract_spec.rb"
public abstract partial class AbstractDerived : Klass {}
#line 9 "./class/instantiation/class_spec.rb"
public partial class OverloadedConstructorClass {
      public string val;

      public OverloadedConstructorClass() {
        val = "empty constructor";
      }

      public OverloadedConstructorClass(string str) {
        val = "string constructor";
      }

      public OverloadedConstructorClass(string str, int i) {
        val = "string int constructor";
      }
    }
#line 4 "./class/mapping_spec.rb"
public class EmptyClass {}
    public partial class Klass {public int m() {return 1;}}
    public abstract class EmptyAbstractClass {}
    public abstract class AbstractClass {public abstract int m();}
    public static class EmptyStaticClass {}
    public static class StaticClass {public static int m() {return 1;}}
    public sealed class SealedClass {public int m() {return 1;}}
    public sealed class EmptySealedClass {}
    public class EmptyGenericClass<T>{}
    public class GenericClass<T>{public int m() {return 1;}}
    public class EmptyGeneric2Class<T,U>{}
    public class Generic2Class<T,U>{public int m() {return 1;}}
#line 5 "./class/modification/addition_spec.rb"
public partial class Klass {
      public int BarI() {
        return 1;
      }
      
      public static int BarC() {
        return 2;
      }
    }
#line 10 "./delegate/conversion_spec.rb"
#line 15 "./delegate/conversion_spec.rb"
public partial class DelegateConversionClass {
    public delegate int Delegate1(string str);
    private ScriptEngine _engine;
    private Proc _lambda;

    public DelegateConversionClass(string lambdaExpr)  {
      _engine = IronRuby.Ruby.CreateEngine();
      _lambda = (Proc) _engine.Execute(lambdaExpr);
    }

    public int DirectInvoke() {
      return (int) _engine.Operations.Invoke(_lambda, "1");
    }

    public int ConvertToDelegate() {
      Delegate1 d = _engine.Operations.ConvertTo<Delegate1>(_lambda);
      return d("1");
    }
  }
#line 5 "./delegate/instantiation/array_ref_spec.rb"
public partial class DelegateHolder {
      public delegate string[] ARefVoidDelegate();
      public delegate string[] ARefRefDelegate(string foo);
      public delegate string[] ARefValDelegate(int foo);
      public delegate string[] ARefARefDelegate(string[] foo);
      public delegate string[] ARefAValDelegate(int[] foo);
      public delegate string[] ARefGenericDelegate<T>(T foo);
    }
#line 5 "./delegate/instantiation/array_val_spec.rb"
public partial class DelegateHolder {
      public delegate int[] AValVoidDelegate();
      public delegate int[] AValRefDelegate(string foo);
      public delegate int[] AValValDelegate(int foo);
      public delegate int[] AValARefDelegate(string[] foo);
      public delegate int[] AValAValDelegate(int[] foo);
      public delegate int[] AValGenericDelegate<T>(T foo);
    }
#line 5 "./delegate/instantiation/generic_spec.rb"
public partial class DelegateHolder {
      public delegate U GenericVoidDelegate<U>();
      public delegate U GenericRefDelegate<U>(string foo);
      public delegate U GenericValDelegate<U>(int foo);
      public delegate U GenericARefDelegate<U>(string[] foo);
      public delegate U GenericAValDelegate<U>(int[] foo);
      public delegate U GenericGenericDelegate<T, U>(T foo);
    }
#line 5 "./delegate/instantiation/ref_spec.rb"
public partial class DelegateHolder {
      public delegate string RefVoidDelegate();
      public delegate string RefRefDelegate(string foo);
      public delegate string RefValDelegate(int foo);
      public delegate string RefARefDelegate(string[] foo);
      public delegate string RefAValDelegate(int[] foo);
      public delegate string RefGenericDelegate<T>(T foo);
    }
#line 5 "./delegate/instantiation/val_spec.rb"
public partial class DelegateHolder {
      public delegate int ValVoidDelegate();
      public delegate int ValRefDelegate(string foo);
      public delegate int ValValDelegate(int foo);
      public delegate int ValARefDelegate(string[] foo);
      public delegate int ValAValDelegate(int[] foo);
      public delegate int ValGenericDelegate<T>(T foo);
    }
#line 5 "./delegate/instantiation/void_spec.rb"
public partial class DelegateHolder {
      public delegate void VoidVoidDelegate();
      public delegate void VoidRefDelegate(string foo);
      public delegate void VoidValDelegate(int foo);
      public delegate void VoidARefDelegate(string[] foo);
      public delegate void VoidAValDelegate(int[] foo);
      public delegate void VoidGenericDelegate<T>(T foo);
    }
#line 4 "./delegate/mapping_spec.rb"
public delegate void VoidVoidDelegate();
#line 4 "./enum/mapping_spec.rb"
public enum EnumInt : int { A, B, C}
#line 4 "./events/invocation_spec.rb"
public class ClassWithEvents {
      public event EventHandler FullEvent;
      public static event EventHandler StaticFullEvent; 

      public void InvokeFullEvent(int count) {
        if (FullEvent != null) FullEvent(this, count);
      }

      public static void InvokeStaticFullEvent(int count) {
        if (StaticFullEvent != null) StaticFullEvent(new object(), count);
      }
    }
#line 17 "./events/mapping_spec.rb"
#pragma warning disable 67
  public delegate void EventHandler(object source, int count);
  public partial class BasicEventClass {
    public event EventHandler OnEvent;
  }
  #pragma warning restore 67
#line 5 "./fields/access_spec.rb"
#pragma warning disable 414
  public partial class ClassWithFields {
    public string field = "field";
    public const string constField = "const";
    public readonly string readOnlyField = "readonly";
    public static string staticField = "static";
    public static readonly string staticReadOnlyField = "static readonly";
  
    private string privateField = "private field";
    private const string privateConstField = "private const";
    private readonly string privateReadOnlyField = "private readonly";
    private static string privateStaticField = "private static";
    private static readonly string privateStaticReadOnlyField = "private static readonly";
   
    protected string protectedField = "protected field";
    protected const string protectedConstField = "protected const";
    protected readonly string protectedReadOnlyField = "protected readonly";
    protected static string protectedStaticField = "protected static";
    protected static readonly string protectedStaticReadOnlyField = "protected static readonly";
  }

  #pragma warning disable 649
  public class InternalFieldTester {
    internal string MyField;

    public InternalFieldTester() {
      var runtime = ScriptRuntime.CreateFromConfiguration();
      var engine = runtime.GetEngine("IronRuby");
      var scope = engine.CreateScope();
      scope.SetVariable("foo", this);
      engine.Execute("foo.MyField = 'Hello'", scope);
    }
  }
  #pragma warning restore 414, 649
#line 4 "./interface/implementation_spec.rb"
public interface IDoFoo {
      int Foo(string str);
      int Foo(int i);
      int Foo(string str, int i);
    }
    
    public interface IDoStuff {
      int StuffFoo(int foo);
      string StuffBar(int bar);
    }
    
    public class ConsumeIDoFoo {
      public static int ConsumeFoo1(IDoFoo foo) {
        return foo.Foo("hello");
      }
      
      public static int ConsumeFoo2(IDoFoo foo) {
        return foo.Foo(1);
      }
      
      public static int ConsumeFoo3(IDoFoo foo) {
        return foo.Foo("hello", 1);
      }
    }
    
    public class ConsumeIDoStuff {
      public static int ConsumeStuffFoo(IDoStuff stuff) {
        return stuff.StuffFoo(1);
      }
      
      public static string ConsumeStuffBar(IDoStuff stuff) {
        return stuff.StuffBar(2);
      }
    }
#line 158 "./interface/implementation_spec.rb"
public interface IExposing {
    event EventHandler<EventArgs> IsExposedChanged;
    bool IsExposed {get; set;}
  }

  public partial class Klass {
    public object AddEvent(IExposing arg) {
      arg.IsExposedChanged += EH;
      return arg;
    }

    public object RemoveEvent(IExposing arg) {
      arg.IsExposedChanged -= EH;
      return arg;
    }

    public void EH(object sender, EventArgs e) {
      _foo += 1;
    }
  }
#line 4 "./interface/mapping_spec.rb"
public interface IEmptyInterface {}
    public interface IInterface { void m();}
#line 4 "./interface/reflection_spec.rb"
public class ImplementsIInterface : IInterface {
      public void m() {
        return;
      }
    }
#line 4 "./interfacegroup/mapping_spec.rb"
public interface IEmptyInterfaceGroup { }
    public interface IEmptyInterfaceGroup<T> { }

    public interface IEmptyInterfaceGroup1<T> {}
    public interface IEmptyInterfaceGroup1<T,V> {}

    public interface IInterfaceGroup {void m1();}
    public interface IInterfaceGroup<T> {void m1();}

    public interface IInterfaceGroup1<T> {void m1();}
    public interface IInterfaceGroup1<T,V> {void m1();}
#line 222 "./method/invocation/generic_spec.rb"
public partial class ClassWithMethods {
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

    }

    public partial class Klass {
      private int _foo;
      
      public int Foo {
        get { return _foo; }
      }

      public Klass() {
        _foo = 10;
      }
    }

    public partial class SubKlass : Klass {}
#line 260 "./method/invocation/generic_spec.rb"
#pragma warning disable 693
    public partial class GenericClassWithMethods<K> {
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

      public string Public1Generic2Arg<T>(T arg0, K arg1) {
    return Public2Generic2Arg<T, K>(arg0, arg1);
  }
  
  public string ConflictingGenericMethod<K>(K arg0) {
    return arg0.ToString();
  }

    }
    #pragma warning restore 693
#line 287 "./method/invocation/generic_spec.rb"
#pragma warning disable 693
    public partial class GenericClass2Params<K, J> {
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

      public string Public1Generic2Arg<T>(T arg0, K arg1) {
    return Public2Generic2Arg<T, K>(arg0, arg1);
  }
  
  public string ConflictingGenericMethod<K>(K arg0) {
    return arg0.ToString();
  }

    }
    #pragma warning restore 693
#line 4 "./method/invocation/indexers_spec.rb"
public partial class ClassWithIndexer {
      public int[,] Values = new int[,] { {0, 10}, {20, 30} };

      public int this[int i, int j] { 
        get { return Values[i,j]; } 
        set { Values[i,j] = value; } 
      }
    }
#line 5 "./method/invocation/internal_spec.rb"
internal partial class PartialClassWithMethods {
    internal int Foo(){ return 1; }
  }
#line 21 "./method/invocation/overload_spec.rb"
public partial class ClassWithOverloads {
    public string Tracker { get; set;}

    public string PublicProtectedOverload(){
      return "public overload";
    }
    
    protected string PublicProtectedOverload(string str) {
      return "protected overload";
    }

    public void VoidSignatureOverload() { Tracker = "SO void"; }
    public void VoidSignatureOverload(string foo) { Tracker = "SO string"; }
    public void VoidSignatureOverload(int foo) { Tracker = "SO int"; }
    public void VoidSignatureOverload(string foo, params int[] bar) { Tracker = "SO string params(int[])"; }
    public void VoidSignatureOverload(string foo, params string[] bar) { Tracker = "SO string params(string[])"; }
    public void VoidSignatureOverload(string foo, int bar, int baz) { Tracker = "SO string int int";}
    public void VoidSignatureOverload(params int[] args) { Tracker = "SO params(int[])";}
    public void VoidSignatureOverload(ref string foo) { Tracker = "SO ref string"; }
    public void VoidSignatureOverload(out int foo) { foo = 1;Tracker = "SO out int"; }
    public void VoidSignatureOverload(string foo, ref string bar) { Tracker = "SO string ref"; }
    public void VoidSignatureOverload(ref string foo, string bar) { Tracker = "SO ref string"; }
    public void VoidSignatureOverload(out string foo, ref string bar) { foo = "out"; Tracker = "SO out ref"; }

    public string RefSignatureOverload() { return "SO void"; }
    public string RefSignatureOverload(string foo) { return "SO string"; }
    public string RefSignatureOverload(int foo) { return "SO int"; }
    public string RefSignatureOverload(string foo, params int[] bar) { return "SO string params(int[])"; }
    public string RefSignatureOverload(string foo, params string[] bar) { return "SO string params(string[])"; }
    public string RefSignatureOverload(string foo, int bar, int baz) { return "SO string int int";}
    public string RefSignatureOverload(params int[] args) { return "SO params(int[])";}
    public string RefSignatureOverload(ref string foo) { return "SO ref string"; }
    public string RefSignatureOverload(out int foo) { foo = 1;return "SO out int"; }
    public string RefSignatureOverload(string foo, ref string bar) { return "SO string ref"; }
    public string RefSignatureOverload(ref string foo, string bar) { return "SO ref string"; }
    public string RefSignatureOverload(out string foo, ref string bar) { foo = "out"; return "SO out ref"; }

    public string[] RefArraySignatureOverload() { return new string[]{"SO void"}; }
    public string[] RefArraySignatureOverload(string foo) { return new string[]{"SO string"}; }
    public string[] RefArraySignatureOverload(int foo) { return new string[]{"SO int"}; }
    public string[] RefArraySignatureOverload(string foo, params int[] bar) { return new string[]{"SO string params(int[])"}; }
    public string[] RefArraySignatureOverload(string foo, params string[] bar) { return new string[]{"SO string params(string[])"}; }
    public string[] RefArraySignatureOverload(string foo, int bar, int baz) { return new string[]{"SO string int int"};}
    public string[] RefArraySignatureOverload(params int[] args) { return new string[]{"SO params(int[])"};}
    public string[] RefArraySignatureOverload(ref string foo) { return new string[]{"SO ref string"}; }
    public string[] RefArraySignatureOverload(out int foo) { foo = 1;return new string[]{"SO out int"}; }
    public string[] RefArraySignatureOverload(string foo, ref string bar) { return new string[]{"SO string ref"}; }
    public string[] RefArraySignatureOverload(ref string foo, string bar) { return new string[]{"SO ref string"}; }
    public string[] RefArraySignatureOverload(out string foo, ref string bar) { foo = "out"; return new string[]{"SO out ref"}; }

    public int ValSignatureOverload() { Tracker = "SO void";
return 1; }
    public int ValSignatureOverload(string foo) { Tracker = "SO string";
return 1; }
    public int ValSignatureOverload(int foo) { Tracker = "SO int";
return 1; }
    public int ValSignatureOverload(string foo, params int[] bar) { Tracker = "SO string params(int[])";
return 1; }
    public int ValSignatureOverload(string foo, params string[] bar) { Tracker = "SO string params(string[])";
return 1; }
    public int ValSignatureOverload(string foo, int bar, int baz) { Tracker = "SO string int int";
return 1;}
    public int ValSignatureOverload(params int[] args) { Tracker = "SO params(int[])";
return 1;}
    public int ValSignatureOverload(ref string foo) { Tracker = "SO ref string";
return 1; }
    public int ValSignatureOverload(out int foo) { foo = 1;Tracker = "SO out int";
return 1; }
    public int ValSignatureOverload(string foo, ref string bar) { Tracker = "SO string ref";
return 1; }
    public int ValSignatureOverload(ref string foo, string bar) { Tracker = "SO ref string";
return 1; }
    public int ValSignatureOverload(out string foo, ref string bar) { foo = "out"; Tracker = "SO out ref";
return 1; }

    public int[] ValArraySignatureOverload() { Tracker = "SO void";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(string foo) { Tracker = "SO string";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(int foo) { Tracker = "SO int";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(string foo, params int[] bar) { Tracker = "SO string params(int[])";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(string foo, params string[] bar) { Tracker = "SO string params(string[])";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(string foo, int bar, int baz) { Tracker = "SO string int int";
return new int[]{1};}
    public int[] ValArraySignatureOverload(params int[] args) { Tracker = "SO params(int[])";
return new int[]{1};}
    public int[] ValArraySignatureOverload(ref string foo) { Tracker = "SO ref string";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(out int foo) { foo = 1;Tracker = "SO out int";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(string foo, ref string bar) { Tracker = "SO string ref";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(ref string foo, string bar) { Tracker = "SO ref string";
return new int[]{1}; }
    public int[] ValArraySignatureOverload(out string foo, ref string bar) { foo = "out"; Tracker = "SO out ref";
return new int[]{1}; }

    public string GenericSignatureOverload<T>() { return "SO void" ; }
    public string GenericSignatureOverload<T>(string foo) { return "SO string" ; }
    public string GenericSignatureOverload<T>(int foo) { return "SO int" ; }
    public string GenericSignatureOverload<T>(string foo, params int[] bar) { return "SO string params(int[])" ; }
    public string GenericSignatureOverload<T>(string foo, params string[] bar) { return "SO string params(string[])" ; }
    public string GenericSignatureOverload<T>(string foo, int bar, int baz) { return "SO string int int" ;}
    public string GenericSignatureOverload<T>(params int[] args) { return "SO params(int[])" ;}
    public string GenericSignatureOverload<T>(ref string foo) { return "SO ref string" ; }
    public string GenericSignatureOverload<T>(out int foo) { foo = 1;return "SO out int" ; }
    public string GenericSignatureOverload<T>(string foo, ref string bar) { return "SO string ref" ; }
    public string GenericSignatureOverload<T>(ref string foo, string bar) { return "SO ref string" ; }
    public string GenericSignatureOverload<T>(out string foo, ref string bar) { foo = "out"; return "SO out ref" ; }

  }
#line 4 "./method/modification/override_spec.rb"
public partial class ClassWithMethods {
    public int SummingMethod(int a, int b){
      return a+b;
    }
  }
#line 45 "./method/modification/override_spec.rb"
public class VirtualMethodBaseClass { 
      public virtual string VirtualMethod() { return "virtual"; } 
    }
    public class VirtualMethodOverrideNew : VirtualMethodBaseClass { 
      new public virtual string VirtualMethod() { return "new"; } 
    }
    public class VirtualMethodOverrideOverride : VirtualMethodBaseClass {
      public override string VirtualMethod() { return "override"; } 
    }
#line 4 "./method/reflection_spec.rb"
public partial class ClassWithMethods {
      public string PublicMethod() {return "public";}
      protected string ProtectedMethod() {return "protected";}
      private string PrivateMethod() {return "private";}
    }
#line 59 "./method/reflection_spec.rb"
public abstract partial class AbstractClassWithMethods {
      public abstract string PublicMethod();
      protected abstract string ProtectedMethod();
    }
#line 107 "./method/reflection_spec.rb"
public partial class ClassWithOverloads {
      public string Overloaded() { return "empty"; }
      public string Overloaded(int arg) { return "one arg"; }
      public string Overloaded(int arg1, int arg2) { return "two args"; }
    }
#line 147 "./method/reflection_spec.rb"
public partial class Klass{
      public static int StaticVoidMethod() {
        return 1;
      }
    }
#line 4 "./namespaces/mapping_spec.rb"
namespace NotEmptyNamespace {
    public class Foo {
      public static int Bar() { return 1; }
    }
  }
#line 4 "./ruby/additions/clr_new_spec.rb"
namespace CLRNew {
    public class Ctor {
      public int Tracker {get; set;}

      public Ctor() {
        Tracker = 1; 
      }
    }
  }
#line 4 "./ruby/name_mangling/public_spec.rb"
public class PublicNameHolder {
      public string a() { return "a";}
      public string A() { return "A";}
      public string Unique() { return "Unique"; }
      public string snake_case() {return "snake_case";}
      public string CamelCase() {return "CamelCase";}
      public string Mixed_Snake_case() {return "Mixed_Snake_case";}
      public string CAPITAL() { return "CAPITAL";}
      public string PartialCapitalID() { return "PartialCapitalID";}
      public string PartialCapitalId() { return "PartialCapitalId";}
      public string __LeadingCamelCase() { return "__LeadingCamelCase";}
      public string __leading_snake_case() { return "__leading_snake_case";}
      public string foNBar() { return "foNBar"; }
      public string fNNBar() { return "fNNBar"; }
      public string NNNBar() { return "NNNBar"; }
      public string MyUIApp() { return "MyUIApp"; }
      public string MyIdYA() { return "MyIdYA"; }
      public string NaN() { return "NaN"; }
      public string NaNa() { return "NaNa"; }
    }

    public class SubPublicNameHolder : PublicNameHolder {
    }
#line 4 "./struct/mapping_spec.rb"
public struct EmptyStruct {}
    public struct Struct { public int m1() {return 1;}}
#line 4 "./typegroup/invocation/nongeneric_spec.rb"
public class StaticMethodTypeGroup {
    public static int Return(int retval) { return retval; }
  }
  public class StaticMethodTypeGroup<T> {
    public static T Return(T retval) { return retval;}
  }
#line 4 "./typegroup/mapping_spec.rb"
public class EmptyTypeGroup { }
    public class EmptyTypeGroup<T> { }

    public class EmptyTypeGroup1<T> {}
    public class EmptyTypeGroup1<T,V> {}

    public class TypeGroup {int m1() {return 1;}}
    public class TypeGroup<T> {int m1() {return 1;}}

    public class TypeGroup1<T> {int m1() {return 1;}}
    public class TypeGroup1<T,V> {int m1() {return 1;}}