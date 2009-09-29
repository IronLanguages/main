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
EOL
no_csc do
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
