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
end
