using System;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Hosting;
using IronRuby.Runtime;
using IronRuby.Builtins;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
#line 1 "./bcl/fixtures/classes.rb"
#line 5 "./bcl/fixtures/classes.rb"
public partial class Klass {
    public T[] ArrayAcceptingMethod<T>(T[] arg0) {
      return arg0;
    }
    public decimal MyDecimal {get; set;}
    public string A(){
      return "a";
    }

    public string Aa(){
      return "aa";
    }
  }

  public static class EqualityChecker {
    public static new bool Equals(object o1, object o2) { return o1.Equals(o2); }
  }
  
  public class Equatable {
    public override bool Equals(object other) { return (other is string) && ((string)other) == "ClrMarker"; }
    public override int GetHashCode() { throw new NotImplementedException(); }
  }

  public static class Hasher {
    public static int GetHashCode(object o) { return o.GetHashCode(); }
  }
  
  public class Hashable {
    public override int GetHashCode() { return 123; }
  }

  public class IComparableConsumer {
    public static int Consume(IComparable icomp) {
      return icomp.CompareTo(1);
    }
  }

  public class IComparableProvider {

  }

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

  public partial class NumericHelper {
    public static int SizeOfByte() {
      return sizeof(Byte);
    }
    public static int SizeOfInt16() {
      return sizeof(Int16);
    }
    public static int SizeOfInt32() {
      return sizeof(Int32);
    }
    public static int SizeOfInt64() {
      return sizeof(Int64);
    }
    public static int SizeOfSByte() {
      return sizeof(SByte);
    }
    public static int SizeOfUInt16() {
      return sizeof(UInt16);
    }
    public static int SizeOfUInt32() {
      return sizeof(UInt32);
    }
    public static int SizeOfUInt64() {
      return sizeof(UInt64);
    }
    public static int SizeOfDecimal() {
      return sizeof(Decimal);
    }
  }
  namespace RegressionSpecs {
    public class B { }
    public class C : B { }
    public interface I1 { int f(); }
    public interface I2 { int g(); }
  }
#line 1 "./class/fixtures/classes.rb"
#line 4 "./class/fixtures/classes.rb"
public class EmptyClass {}
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
  public partial class Klass {
    public int m() {return 1;}
    public int BarI() {
      return 1;
    }
    
    public static int BarC() {
      return 2;
    }
  }

  public partial class DerivedFromAbstract : AbstractClass {
    public override int m() {return 1;}
  }

  public abstract partial class AbstractDerived : Klass {}
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

  public class ClassWithOptionalConstructor {
    public int Arg {get; set;}
    
    public ClassWithOptionalConstructor([Optional]int arg) {
      Arg = arg;
    }
  }

  //TODO: the marshal attribute shouldn't be needed. this was due to a super
  //bug not a marshal bug.
  public abstract class Unsafe {
    [return: MarshalAs(UnmanagedType.U1)]
    public virtual bool Foo() { return true;}
  }

  public interface IHaveAnEvent {
    event EventHandler MyEvent;
  }

  public abstract class AbstractHasAnEvent : IHaveAnEvent {
    public abstract event EventHandler MyEvent;
  }

  public class ExplicitIInterface : IInterface {
    public int Tracker {get; set;}

    public ExplicitIInterface() {
      Tracker = 0;
    }

    public void Reset() {
      Tracker = 0;
    }
    void IInterface.m() {
      Tracker = 2;
    }

    public void m() {
      Tracker = 1;
    }
  }
#line 8 "./delegate/fixtures/classes.rb"
#line 13 "./delegate/fixtures/classes.rb"
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

  public partial class DelegateHolder {
    public delegate string[] ARefVoidDelegate();
    public delegate string[] ARefRefDelegate(string foo);
    public delegate string[] ARefValDelegate(int foo);
    public delegate string[] ARefARefDelegate(string[] foo);
    public delegate string[] ARefAValDelegate(int[] foo);
    public delegate string[] ARefGenericDelegate<T>(T foo);

    public delegate void VoidVoidDelegate();
    public delegate void VoidRefDelegate(string foo);
    public delegate void VoidValDelegate(int foo);
    public delegate void VoidARefDelegate(string[] foo);
    public delegate void VoidAValDelegate(int[] foo);
    public delegate void VoidGenericDelegate<T>(T foo);

    public delegate int ValVoidDelegate();
    public delegate int ValRefDelegate(string foo);
    public delegate int ValValDelegate(int foo);
    public delegate int ValARefDelegate(string[] foo);
    public delegate int ValAValDelegate(int[] foo);
    public delegate int ValGenericDelegate<T>(T foo);

    public delegate string RefVoidDelegate();
    public delegate string RefRefDelegate(string foo);
    public delegate string RefValDelegate(int foo);
    public delegate string RefARefDelegate(string[] foo);
    public delegate string RefAValDelegate(int[] foo);
    public delegate string RefGenericDelegate<T>(T foo);

    public delegate U GenericVoidDelegate<U>();
    public delegate U GenericRefDelegate<U>(string foo);
    public delegate U GenericValDelegate<U>(int foo);
    public delegate U GenericARefDelegate<U>(string[] foo);
    public delegate U GenericAValDelegate<U>(int[] foo);
    public delegate U GenericGenericDelegate<T, U>(T foo);

    public delegate int[] AValVoidDelegate();
    public delegate int[] AValRefDelegate(string foo);
    public delegate int[] AValValDelegate(int foo);
    public delegate int[] AValARefDelegate(string[] foo);
    public delegate int[] AValAValDelegate(int[] foo);
    public delegate int[] AValGenericDelegate<T>(T foo);
  }
#line 1 "./enum/fixtures/classes.rb"
public enum EnumInt : int { A, B, C}
public enum CustomEnum { A, B, C}
#line 1 "./events/fixtures/classes.rb"
#pragma warning disable 67
  public delegate void EventHandler(object source, int count);
  public partial class BasicEventClass {
    public event EventHandler OnEvent;
  }
  #pragma warning restore 67
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
#line 1 "./fields/fixtures/classes.rb"
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
#line 1 "./interface/fixtures/classes.rb"
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
  public interface IEmptyInterface {}
  public interface IInterface { void m();}
  public class ImplementsIInterface : IInterface {
    public void m() {
      return;
    }
  }
  public interface I1 { string M(); }
  public interface I2 { string M(); }
  public interface I3<T> { string M(); }
  public interface I4 { string M(int arg); }

  public class ClassI1_1 : I1 {
    string I1.M() { return "I1.M"; }
  }

  public class ClassI1_2 : I1 {
    string I1.M() { return "I1.M"; }
    public string M() { return "class M"; }
  }

  public class ClassI2I1 : I2, I1 {
    string I1.M() { return "I1.M"; }
    string I2.M() { return "I2.M"; }
  }

  public class ClassI3Obj : I3<object> {
    string I3<object>.M() { return "I3<object>.M"; }
    public string M() { return "class M"; }
  }

  public class ClassI1I2I3Obj : I1, I2, I3<object> {
    string I1.M() { return "I1.M"; }
    string I2.M() { return "I2.M"; }
    string I3<object>.M() { return "I3<object>.M"; }
    public string M() { return "class M"; }
  }

  public class ClassI3_1<T> : I3<T> {
    string I3<T>.M() { return "I3<T>.M"; }
    public string M() { return "class M"; }
  }

  public class ClassI3_2<T> : I3<T> {
    string I3<T>.M() { return "I3<T>.M"; }
  }

  public class ClassI3ObjI3Int : I3<object>, I3<int> {
    string I3<object>.M() { return "I3<object>.M";}
    string I3<int>.M() { return "I3<int>.M";}
  }

  public class ClassI1I4 : I1, I4 {
    string I1.M() { return "I1.M"; }
    string I4.M(int arg) { return "I4.M"; }
  }

  public class PublicIPublicInterface : IPublicInterface {
    public IPublicInterface Hello {
      get { return this; }
      set {}
    }

    public void Foo(IPublicInterface f) {
    }

    public IPublicInterface RetInterface() {
      return this;
    }

    public event PublicDelegateType MyEvent;
    public IPublicInterface FireEvent(PublicEventArgs args) {
      return MyEvent(this, args);
    }

    public PublicEventArgs GetEventArgs() {
      return new PublicEventArgs();
    }
  }

  public class PublicEventArgs : EventArgs { }
  class PrivateEventArgs : PublicEventArgs { }
  public delegate IPublicInterface PublicDelegateType(IPublicInterface sender, PublicEventArgs args);

  // Private class
  class PrivateClass : IPublicInterface {
      public IPublicInterface Hello {
          get { return this; }
          set { }
      }

      public void Foo(IPublicInterface f) {
      }

      public IPublicInterface RetInterface() {
          return this;
      }

      public event PublicDelegateType MyEvent;
      public IPublicInterface FireEvent(PublicEventArgs args) {
          return MyEvent(this, args);
      }

      public PublicEventArgs GetEventArgs() {
          return new PrivateEventArgs();
      }
  }

  //Public class
  public class PublicClass : IPublicInterface {
      public IPublicInterface Hello {
          get { return this; }
          set { }
      }

      public void Foo(IPublicInterface f) {
      }

      public IPublicInterface RetInterface() {
          return this;
      }

      public event PublicDelegateType MyEvent;
      public IPublicInterface FireEvent(PublicEventArgs args) {
          return MyEvent(this, args);
      }

      public PublicEventArgs GetEventArgs() {
          return new PublicEventArgs();
      }
  }

  // Public Interface
  public interface IPublicInterface {
      IPublicInterface Hello { get; set; }
      void Foo(IPublicInterface f);
      IPublicInterface RetInterface();
      event PublicDelegateType MyEvent;
      IPublicInterface FireEvent(PublicEventArgs args);
      PublicEventArgs GetEventArgs();
  }

  // Access the private class via the public interface
  public class InterfaceOnlyTest {
      public static IPublicInterface PrivateClass {
          get {
              return new PrivateClass(); 
          }
      }
  }

  public interface IHaveGenerics {
    T GenericsHere<T>(string arg1);
    T MoreGenericsHere<T,S>(S x);
  }

  public class EatIHaveGenerics {
    public static string TestGenericsHere(IHaveGenerics ihg){
      return ihg.GenericsHere<string>("test");
    }

    public static string TestMoreGenericsHere(IHaveGenerics ihg){
      return ihg.MoreGenericsHere<string, int>(1);
    }
  }
#line 1 "./interfacegroup/fixtures/classes.rb"
public interface IEmptyInterfaceGroup { }
  public interface IEmptyInterfaceGroup<T> { }

  public interface IEmptyInterfaceGroup1<T> {}
  public interface IEmptyInterfaceGroup1<T,V> {}

  public interface IInterfaceGroup {void m1();}
  public interface IInterfaceGroup<T> {void m1();}

  public interface IInterfaceGroup1<T> {void m1();}
  public interface IInterfaceGroup1<T,V> {void m1();}
#line 3 "./method/fixtures/classes.rb"
#line 147 "./method/fixtures/classes.rb"
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

      public int ValSignatureOverload() { Tracker = "SO void";return 1; }
  public int ValSignatureOverload(string foo) { Tracker = "SO string";return 1; }
  public int ValSignatureOverload(int foo) { Tracker = "SO int";return 1; }
  public int ValSignatureOverload(string foo, params int[] bar) { Tracker = "SO string params(int[])";return 1; }
  public int ValSignatureOverload(string foo, params string[] bar) { Tracker = "SO string params(string[])";return 1; }
  public int ValSignatureOverload(string foo, int bar, int baz) { Tracker = "SO string int int";return 1;}
  public int ValSignatureOverload(params int[] args) { Tracker = "SO params(int[])";return 1;}
  public int ValSignatureOverload(ref string foo) { Tracker = "SO ref string";return 1; }
  public int ValSignatureOverload(out int foo) { foo = 1;Tracker = "SO out int";return 1; }
  public int ValSignatureOverload(string foo, ref string bar) { Tracker = "SO string ref";return 1; }
  public int ValSignatureOverload(ref string foo, string bar) { Tracker = "SO ref string";return 1; }
  public int ValSignatureOverload(out string foo, ref string bar) { foo = "out"; Tracker = "SO out ref";return 1; }

      public int[] ValArraySignatureOverload() { Tracker = "SO void";return new int[]{1}; }
  public int[] ValArraySignatureOverload(string foo) { Tracker = "SO string";return new int[]{1}; }
  public int[] ValArraySignatureOverload(int foo) { Tracker = "SO int";return new int[]{1}; }
  public int[] ValArraySignatureOverload(string foo, params int[] bar) { Tracker = "SO string params(int[])";return new int[]{1}; }
  public int[] ValArraySignatureOverload(string foo, params string[] bar) { Tracker = "SO string params(string[])";return new int[]{1}; }
  public int[] ValArraySignatureOverload(string foo, int bar, int baz) { Tracker = "SO string int int";return new int[]{1};}
  public int[] ValArraySignatureOverload(params int[] args) { Tracker = "SO params(int[])";return new int[]{1};}
  public int[] ValArraySignatureOverload(ref string foo) { Tracker = "SO ref string";return new int[]{1}; }
  public int[] ValArraySignatureOverload(out int foo) { foo = 1;Tracker = "SO out int";return new int[]{1}; }
  public int[] ValArraySignatureOverload(string foo, ref string bar) { Tracker = "SO string ref";return new int[]{1}; }
  public int[] ValArraySignatureOverload(ref string foo, string bar) { Tracker = "SO ref string";return new int[]{1}; }
  public int[] ValArraySignatureOverload(out string foo, ref string bar) { foo = "out"; Tracker = "SO out ref";return new int[]{1}; }

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
#line 1 "./namespaces/fixtures/classes.rb"
namespace NotEmptyNamespace {
    public class Foo {
      public static int Bar() { return 1; }
    }
  }
#line 14 "./ruby/fixtures/classes.rb"
namespace CLRNew {
    public class Ctor {
      public int Tracker {get; set;}

      public Ctor() {
        Tracker = 1; 
      }
    }
  }
  //TODO: This will be used once nil.as(C) is supported
  public partial class Klass {
    public string Tracker {get;set;}
    public bool NullChecker(Klass arg1, ArrayList arg2) {
      Tracker = "Klass ArrayList";
      return arg1 == null && arg2 == null;
    }
    public bool NullChecker(Type arg1, string arg2) {
      Tracker = "type string";
      return arg1 == null && arg2 == null;
    }
  }
  public class PublicNameHolder {
    public string a(){return "a";}
    public string A(){return "A";}
    public string Unique(){return "Unique";}
    public string snake_case(){return "snake_case";}
    public string CamelCase(){return "CamelCase";}
    public string Mixed_Snake_case(){return "Mixed_Snake_case";}
    public string CAPITAL(){return "CAPITAL";}
    public string PartialCapitalID(){return "PartialCapitalID";}
    public string PartialCapitalId(){return "PartialCapitalId";}
    public string __LeadingCamelCase(){return "__LeadingCamelCase";}
    public string __leading_snake_case(){return "__leading_snake_case";}
    public string foNBar(){return "foNBar";}
    public string fNNBar(){return "fNNBar";}
    public string NNNBar(){return "NNNBar";}
    public string MyUIApp(){return "MyUIApp";}
    public string MyIdYA(){return "MyIdYA";}
    public string NaN(){return "NaN";}
    public string NaNa(){return "NaNa";}
    public string NoOfScenarios(){return "NoOfScenarios";}
    
  }

  public class StaticNameHolder {
    public static string a(){return "a";}
    public static string A(){return "A";}
    public static string Unique(){return "Unique";}
    public static string snake_case(){return "snake_case";}
    public static string CamelCase(){return "CamelCase";}
    public static string Mixed_Snake_case(){return "Mixed_Snake_case";}
    public static string CAPITAL(){return "CAPITAL";}
    public static string PartialCapitalID(){return "PartialCapitalID";}
    public static string PartialCapitalId(){return "PartialCapitalId";}
    public static string __LeadingCamelCase(){return "__LeadingCamelCase";}
    public static string __leading_snake_case(){return "__leading_snake_case";}
    public static string foNBar(){return "foNBar";}
    public static string fNNBar(){return "fNNBar";}
    public static string NNNBar(){return "NNNBar";}
    public static string MyUIApp(){return "MyUIApp";}
    public static string MyIdYA(){return "MyIdYA";}
    public static string NaN(){return "NaN";}
    public static string NaNa(){return "NaNa";}
    public static string NoOfScenarios(){return "NoOfScenarios";}
    
  }

  public class SubPublicNameHolder : PublicNameHolder {}

  public class SubStaticNameHolder : StaticNameHolder {}
    
  public class MangledBase {}
  public class NotMangled : MangledBase {
  public string Class(){return "Class";}
    public string Clone(){return "Clone";}
    public string Display(){return "Display";}
    public string Dup(){return "Dup";}
    public string Extend(){return "Extend";}
    public string Freeze(){return "Freeze";}
    public string Hash(){return "Hash";}
    public string Initialize(){return "Initialize";}
    public string Inspect(){return "Inspect";}
    public string InstanceEval(){return "InstanceEval";}
    public string InstanceExec(){return "InstanceExec";}
    public string InstanceVariableGet(){return "InstanceVariableGet";}
    public string InstanceVariableSet(){return "InstanceVariableSet";}
    public string InstanceVariables(){return "InstanceVariables";}
    public string Method(){return "Method";}
    public string Methods(){return "Methods";}
    public string ObjectId(){return "ObjectId";}
    public string PrivateMethods(){return "PrivateMethods";}
    public string ProtectedMethods(){return "ProtectedMethods";}
    public string PublicMethods(){return "PublicMethods";}
    public string Send(){return "Send";}
    public string SingletonMethods(){return "SingletonMethods";}
    public string Taint(){return "Taint";}
    public string Untaint(){return "Untaint";}
    
  }

  public class SubNotMangled : NotMangled {}
  
  public class StaticNotMangled : MangledBase {
  public static string Class(){return "Class";}
    public static string Clone(){return "Clone";}
    public static string Display(){return "Display";}
    public static string Dup(){return "Dup";}
    public static string Extend(){return "Extend";}
    public static string Freeze(){return "Freeze";}
    public static string Hash(){return "Hash";}
    public static string Initialize(){return "Initialize";}
    public static string Inspect(){return "Inspect";}
    public static string InstanceEval(){return "InstanceEval";}
    public static string InstanceExec(){return "InstanceExec";}
    public static string InstanceVariableGet(){return "InstanceVariableGet";}
    public static string InstanceVariableSet(){return "InstanceVariableSet";}
    public static string InstanceVariables(){return "InstanceVariables";}
    public static string Method(){return "Method";}
    public static string Methods(){return "Methods";}
    public static string ObjectId(){return "ObjectId";}
    public static string PrivateMethods(){return "PrivateMethods";}
    public static string ProtectedMethods(){return "ProtectedMethods";}
    public static string PublicMethods(){return "PublicMethods";}
    public static string Send(){return "Send";}
    public static string SingletonMethods(){return "SingletonMethods";}
    public static string Taint(){return "Taint";}
    public static string Untaint(){return "Untaint";}
    
  }

  public class SubStaticNotMangled : StaticNotMangled {}

  public static class ObjectExtensions {
    public static bool IsNotNull(this object value){
      return value != null;
    }

    public static bool IsNull(this object value){
      return value == null;
    }
  }
#line 1 "./struct/fixtures/classes.rb"
public struct EmptyStruct {}
  public struct CStruct { public int m1() {return 1;}}
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
#line 1 "./typegroup/fixtures/classes.rb"
public class StaticMethodTypeGroup {
    public static int Return(int retval) { return retval; }
  }
  public class StaticMethodTypeGroup<T> {
    public static T Return(T retval) { return retval;}
  }
  public class EmptyTypeGroup { }
  public class EmptyTypeGroup<T> { }

  public class EmptyTypeGroup1<T> {}
  public class EmptyTypeGroup1<T,V> {}

  public class TypeGroup {int m1() {return 1;}}
  public class TypeGroup<T> {int m1() {return 1;}}

  public class TypeGroup1<T> {int m1() {return 1;}}
  public class TypeGroup1<T,V> {int m1() {return 1;}}