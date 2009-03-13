//./class/mapping_spec.rb:4
public class EmptyClass {}
    public class Klass {public int m() {return 1;}}
    public abstract class EmptyAbstractClass {}
    public abstract class AbstractClass {public abstract int m();}
    public static class EmptyStaticClass {}
    public static class StaticClass {public static int m() {return 1;}}
//./delegate/mapping_spec.rb:4
public delegate void VoidVoidDelegate();
//./enum/mapping_spec.rb:4
public enum EnumInt : int { A, B, C}
//./interface/mapping_spec.rb:4
public interface IEmptyInterface {}
    public interface IInterface { void m();}
//./interfacegroup/mapping_spec.rb:4
public interface IEmptyInterfaceGroup { }
    public interface IEmptyInterfaceGroup<T> { }

    public interface IEmptyInterfaceGroup1<T> {}
    public interface IEmptyInterfaceGroup1<T,V> {}

    public interface IInterfaceGroup {void m1();}
    public interface IInterfaceGroup<T> {void m1();}

    public interface IInterfaceGroup1<T> {void m1();}
    public interface IInterfaceGroup1<T,V> {void m1();}
//./method/modification/override_spec.rb:41
public class VirtualMethodBaseClass { 
      public virtual string VirtualMethod() { return "virtual"; } 
    }
    public class VirtualMethodOverrideNew : VirtualMethodBaseClass { 
      new public virtual string VirtualMethod() { return "new"; } 
    }
    public class VirtualMethodOverrideOverride : VirtualMethodBaseClass {
      public override string VirtualMethod() { return "override"; } 
    }
//./method/reflection_spec.rb:4
public partial class ClassWithMethods {
      public string PublicMethod() {return "public";}
      protected string ProtectedMethod() {return "protected";}
      private string PrivateMethod() {return "private";}
    }
//./method/reflection_spec.rb:39
public abstract partial class AbstractClassWithMethods {
      public abstract string PublicMethod();
      protected abstract string ProtectedMethod();
    }
//./method/reflection_spec.rb:88
public partial class ClassWithOverloads {
      public string Overloaded() { return "empty"; }
      public string Overloaded(int arg) { return "one arg"; }
      public string Overloaded(int arg1, int arg2) { return "two args"; }
    }
//./struct/mapping_spec.rb:4
public struct EmptyStruct {}
    public struct Struct { public int m1() {return 1;}}
//./typegroup/invocation/nongeneric_spec.rb:4
public class StaticMethodTypeGroup {
    public static int Return(int retval) { return retval; }
  }
  public class StaticMethodTypeGroup<T> {
    public static T Return(T retval) { return retval;}
  }
//./typegroup/mapping_spec.rb:4
public class EmptyTypeGroup { }
    public class EmptyTypeGroup<T> { }

    public class EmptyTypeGroup1<T> {}
    public class EmptyTypeGroup1<T,V> {}

    public class TypeGroup {int m1() {return 1;}}
    public class TypeGroup<T> {int m1() {return 1;}}

    public class TypeGroup1<T> {int m1() {return 1;}}
    public class TypeGroup1<T,V> {int m1() {return 1;}}