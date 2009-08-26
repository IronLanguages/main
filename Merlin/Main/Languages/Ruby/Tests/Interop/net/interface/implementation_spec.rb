require File.dirname(__FILE__) + '/../spec_helper'

describe "Implementing interfaces" do
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
  EOL
  before(:all) do
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
  end
  
  it "works with normal interfaces" do
    stuff = RubyImplementsIDoStuff.new
    ConsumeIDoStuff.ConsumeStuffFoo(stuff).should == 1
    ConsumeIDoStuff.ConsumeStuffBar(stuff).should == "2"
  end
  
  it "works with overloaded methods on an interface" do
    foo = RubyImplementsIDoFoo.new
    ConsumeIDoFoo.ConsumeFoo1(foo).should == 1  
    ConsumeIDoFoo.ConsumeFoo2(foo).should == 1
    ConsumeIDoFoo.ConsumeFoo3(foo).should == 1
  end
  
  it "works with Hash (regression for CP#814)" do
    class HashSubclass < Hash
      include System::Collections::IDictionary
    end

    lambda { HashSubclass.new }.should_not raise_error(TypeError)
  end
end

describe "Implementing interfaces with default methods" do
  before(:all) do
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
  end
  
  it "allows instantiation" do
    lambda { foo = RubyImplementsIDoFooDefaults.new }.should_not raise_error
    lambda { stuff = RubyImplementsIDoStuffDefaults.new }.should_not raise_error
  end

  describe "allows" do
    before(:each) do
      @foo = RubyImplementsIDoFooDefaults.new
      @stuff = RubyImplementsIDoStuffDefaults.new
    end
    
    it "allows method calls" do
      lambda {@foo.foo(1)}.should raise_error NoMethodError
      lambda {@stuff.stuff_foo(1)}.should raise_error NoMethodError
    end

    it "is kind_of the interface type" do
      @foo.should be_kind_of IDoFoo
      @stuff.should be_kind_of IDoStuff
    end

    it "can be passed as an interface object" do
      lambda { ConsumeIDoStuff.ConsumeStuffFoo(@stuff) }.should raise_error NoMethodError
      lambda { ConsumeIDoFoo.ConsumeFoo2(@foo) }.should raise_error NoMethodError
    end
  end

  describe "with MethodMissing" do
    before(:each) do
      @foo = RubyImplementsIDoFooMM.new
      @stuff = RubyImplementsIDoStuffMM.new
    end

    it "calls method missing from Ruby" do
      @foo.foo(1).should == 1
      @foo.tracker.should == "IDoFoo MM foo(1)"
      @stuff.stuff_bar(1).should == "IDoStuff MM stuff_bar(1)"
    end

    it "calls method missing from C#" do
      ConsumeIDoStuff.ConsumeStuffBar(@stuff).should == "IDoStuff MM StuffBar(2)"
      ConsumeIDoFoo.ConsumeFoo1(@foo).should == 1  
      @foo.tracker.should ==  "IDoFoo MM Foo(hello)"
    end
  end
end

describe "Implementing interfaces that define events" do
  csc <<-EOL
  public interface IExposing {
    event EventHandler<EventArgs> IsExposedChanged;
    bool IsExposed {get; set;}
  }
  EOL

  before(:all) do
    class RubyExposerDefault
      include IExposing
    end

    class RubyExposerMM
      include IExposing
      attr_reader :handlers
      
      def initialize
        reset
      end
      
      def reset
        @handlers = []
      end
      
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
      attr_reader :handlers
      def initialize
        reset 
      end

      def reset
        @handlers = []
      end
      
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

  it "allows empty implementation without TypeLoadException" do
    lambda {RubyExposerDefault.new}.should_not raise_error
  end

  it "allows method_missing to be the event managment methods" do 
    exposer = RubyExposerMM.new
    l = lamdba {|s,e| [s,e]}
    (exposer.is_exposed_changed.add(l)).should == "Method Missing add"
    exposer.handlers.should include l
    (exposer.is_exposed_changed.remove(l)).should == "Method Missing remove"
    exposer.handlers.should be_empty
  end

  it "allows add and remove event definitions" do
    exposer = RubyExposer.new
    l = lamdba {|s,e| [s,e]}
    (exposer.is_exposed_changed.add(l)).should == "Ruby add handler"
    exposer.handlers.should include l
    (exposer.is_exposed_changed.remove(l)).should == "Ruby remove handler"
    exposer.handlers.should be_empty
  end
end
