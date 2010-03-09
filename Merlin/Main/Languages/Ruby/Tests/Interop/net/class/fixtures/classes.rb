csc <<-EOL
using System.Runtime.InteropServices;
EOL
csc <<-EOL
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
EOL

no_csc do
  class TestDerived < Klass
    def foo
      m
    end
  end
  
  class SubUnsafe < Unsafe
  end
  
  class RubyOverloadedConstructorClass < OverloadedConstructorClass
    def initialize(val)
      super val
    end
  end
  
  class RubyClassWithOptionalConstructor < ClassWithOptionalConstructor; end
  
  class RubyDerivedFromAbstract < DerivedFromAbstract
    def m
      1
    end
  end

  class RubyHasAnEvent < AbstractHasAnEvent
    
  end
end
