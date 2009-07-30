require File.dirname(__FILE__) + "/../../spec_helper"

describe "Inheriting from classes with" do
  describe "parameter attributes" do
    csc <<-EOL
    using System.Runtime.InteropServices;
    EOL

    csc <<-EOL
    public class ClassWithOptionalConstructor {
      public int Arg {get; set;}
      
      public ClassWithOptionalConstructor([Optional]int arg) {
        Arg = arg;
      }
    }
    EOL

    before(:all) do
      class RubyClassWithOptionalConstructor < ClassWithOptionalConstructor; end
    end

    it "passes on optional constructors to subclasses" do
      RubyClassWithOptionalConstructor.new.arg.should == 0
    end


  end

  describe "method attributes that are abstract" do
    #TODO: the marshal attribute shouldn't be needed. this was due to a super
    #bug not a marshal bug.
    csc <<-EOL
      public abstract class Unsafe {
        [return: MarshalAs(UnmanagedType.U1)]
        public virtual bool Foo() { return true;}
      }
    EOL

    before(:all) do
      class SubUnsafe < Unsafe
      end
    end

    it "can call super" do
      lambda {class SubUnsafe < Unsafe
        def foo
          super
        end
      end}.should_not raise_error
    end
  end
end
