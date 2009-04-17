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
  
  
end