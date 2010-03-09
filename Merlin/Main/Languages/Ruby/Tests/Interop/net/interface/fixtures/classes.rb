csc <<-EOL
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
EOL
no_csc do
  class RubyHasGenerics
    include IHaveGenerics

    def generics_here(arg)
      "ruby generics here"
    end

    def more_generics_here(arg)
      "ruby more generics here"
    end
  end
  
  class RubyImplementsIInterface
    include IInterface
    
    def m; end  
  end
  class RubyImplementsIDoFoo
    include IDoFoo
    def foo(str, i = 1)
      i
    end
  end
  
  class RubyImplementsIDoStuff
    include IDoStuff
    def stuff_foo(foo)
      foo
    end
    
    def stuff_bar(bar)
      bar.to_s
    end
  end
  class RubyImplementsIDoFooDefaults
    include IDoFoo
  end

  class RubyImplementsIDoStuffDefaults
    include IDoStuff
  end
  
  class RubyImplementsIDoFooMM
    include IDoFoo
    attr_reader :tracker 
    def method_missing(meth, *args, &blk)
      @tracker = "IDoFoo MM #{meth}(#{args})"   
      args.size
    end
  end

  class RubyImplementsIDoStuffMM
    include IDoStuff
    
    def method_missing(meth, *args, &blk)
      "IDoStuff MM #{meth}(#{args})"   
    end
  end
  module Events
    attr_reader :handlers
    
    def initialize
      reset
    end
    
    def reset
      @handlers = []
    end

    def trigger
      @handlers.each {|e| e.invoke(self, nil)}
    end
  end
  class RubyExposerDefault
    include IExposing
  end

  class RubyExposerMM
    include IExposing
    include Events 
    def method_missing(meth, *args, &blk)
      case meth.to_s
      when /^add_/
        @handlers << args[0]
        "Method Missing add"
      when /^remove_/
        @handlers.delete args[0]
        "Method Missing remove"
      else raise NoMethodError.new(meth, *args, &block)
      end
    end
  end
  
  class RubyExposer
    include IExposing
    include Events 
    def add_IsExposedChanged(h)
      @handlers << h
      "Ruby add handler"
    end

    def remove_IsExposedChanged(h)
      @handlers.delete h 
      "Ruby remove handler"
    end
  end
end
