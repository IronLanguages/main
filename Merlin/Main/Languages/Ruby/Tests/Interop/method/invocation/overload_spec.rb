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
    @methods.call(100).to_s.should == "one arg"
    @methods.call(100, 100).to_s.should == "two args"
    @klass.overloaded(100).to_s.should == "one arg"
    @klass.overloaded(100, 100).to_s.should == "two args"
  end

  it "correctly binds with methods of different visibility" do
    method = @klass.method(:public_protected_overload)
    @klass.public_protected_overload.to_s.should == "public overload"
    @klass.public_protected_overload("abc").to_s.should == "protected overload"
    method.call.to_s.should == "public overload"
    method.call("abc").to_s.should == "protected overload"
  end
end

describe "Selecting .NET overloads" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end
  
  it "is allowed" do
    @methods.overloads(Fixnum,Fixnum).call(100,100).to_s.should == "two args"
  end

  it "correctly reports error message" do
    #regression test for RubyForge 24112
    lambda {@methods.overloads(Fixnum).call}.should raise_error(ArgumentError, /0 for 1/)
  end
end

