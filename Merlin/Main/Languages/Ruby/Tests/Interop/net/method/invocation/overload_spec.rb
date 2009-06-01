require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overload resolution" do
  csc <<-EOL
  public partial class ClassWithOverloads {
    public string PublicProtectedOverload(){
      return "public overload";
    }
    
    protected string PublicProtectedOverload(string str) {
      return "protected overload";
    }
  }
  EOL
  before(:each) do
    @klass = ClassWithOverloads.new
    @methods = @klass.method(:Overloaded)
  end

  it "is performed" do
    @methods.call(100).should equal_clr_string("one arg")
    @methods.call(100, 100).should equal_clr_string("two args")
    @klass.overloaded(100).should equal_clr_string("one arg")
    @klass.overloaded(100, 100).should equal_clr_string("two args")
  end

  it "correctly binds with methods of different visibility" do
    method = @klass.method(:public_protected_overload)
    @klass.public_protected_overload.should equal_clr_string("public overload")
    
    lambda { @klass.public_protected_overload("abc") }.should raise_error(ArgumentError, /1 for 0/)
    
    method.call.should equal_clr_string("public overload")
    lambda { method.call("abc").should equal_clr_string("protected overload") }.should raise_error(ArgumentError, /1 for 0/)
  end
end

describe "Selecting .NET overloads" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end
  
  it "is allowed" do
    @methods.overloads(Fixnum,Fixnum).call(100,100).should equal_clr_string("two args")
  end

  it "correctly reports error message" do
    #regression test for RubyForge 24112
    lambda {@methods.overloads(Fixnum).call}.should raise_error(ArgumentError, /0 for 1/)
  end
end

