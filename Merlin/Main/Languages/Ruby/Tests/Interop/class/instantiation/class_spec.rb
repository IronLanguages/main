require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/instantiable'

describe "General .NET classes" do
  it_behaves_like :instantiable_class, Klass  
end

describe "Ruby classes derived from .NET classes with overloaded constructors" do
  csc <<-EOL
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
  EOL
  class RubyOverloadedConstructorClass < OverloadedConstructorClass
    def initialize(val)
      super val
    end
  end
  
  it_behaves_like :instantiable_class, OverloadedConstructorClass

  it "properly selects overloaded constructors" do
    OverloadedConstructorClass.new("hello").should be_kind_of(OverloadedConstructorClass)
    OverloadedConstructorClass.new("hello").val.to_s.should == "string constructor"
  end
  
  it "properly selects overloaded constructors for super" do
    RubyOverloadedConstructorClass.new("hello").should be_kind_of(RubyOverloadedConstructorClass)
    OverloadedConstructorClass.new("hello").val.to_s.should == "string constructor"
end

end
