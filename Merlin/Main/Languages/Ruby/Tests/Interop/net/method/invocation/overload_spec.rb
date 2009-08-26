require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overload resolution" do
  def method_block(type, prefix, suffix = "", &blk)
    val = <<-EOL
public #{type} #{prefix}SignatureOverload#{suffix}() { #{blk.call "SO void" }; }
    public #{type} #{prefix}SignatureOverload#{suffix}(string foo) { #{blk.call "SO string"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(int foo) { #{blk.call "SO int"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(string foo, params int[] bar) { #{blk.call "SO string params(int[])"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(string foo, params string[] bar) { #{blk.call "SO string params(string[])"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(string foo, int bar, int baz) { #{ blk.call "SO string int int"};}
    public #{type} #{prefix}SignatureOverload#{suffix}(params int[] args) { #{blk.call "SO params(int[])"};}
    public #{type} #{prefix}SignatureOverload#{suffix}(ref string foo) { #{blk.call "SO ref string"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(out int foo) { foo = 1;#{blk.call "SO out int"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(string foo, ref string bar) { #{blk.call "SO string ref"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(ref string foo, string bar) { #{blk.call "SO ref string"}; }
    public #{type} #{prefix}SignatureOverload#{suffix}(out string foo, ref string bar) { foo = "out"; #{blk.call "SO out ref"}; }
    EOL
  end
  
  csc <<-EOL
  public partial class ClassWithOverloads {
    public string Tracker { get; set;}

    public string PublicProtectedOverload(){
      return "public overload";
    }
    
    protected string PublicProtectedOverload(string str) {
      return "protected overload";
    }

    #{method_block("void", "Void") {|el| "Tracker = \"#{el}\""}}
    #{method_block("string", "Ref") {|el| "return \"#{el}\""} }
    #{method_block("string[]", "RefArray") {|el| "return new string[]{\"#{el}\"}"} }
    #{method_block("int", "Val") {|el| "Tracker = \"#{el}\";\nreturn 1"} }
    #{method_block("int[]", "ValArray") {|el| "Tracker = \"#{el}\";\nreturn new int[]{1}" }}
    #{method_block("string", "Generic", "<T>") {|el| "return \"#{el}\" "}}
  }
  EOL
 
  before(:each) do
    @klass = ClassWithOverloads.new
    @overloaded_methods = @klass.method(:Overloaded)
    @void_method = @klass.method(:void_signature_overload)
    @val_methods = [:val_signature_overload, :val_array_signature_overload].map {|meth| @klass.method(meth)}
    @ref_methods = [:ref_signature_overload, :ref_array_signature_overload].map {|meth| @klass.method(meth)}
    @generic_method = @klass.method(:generic_signature_overload)
    @calls = [[lambda {|meth| meth.call}, "SO void"], 
            [lambda {|meth| meth.call("Hello")}, "SO string"],
            [lambda {|meth| meth.call(1)}, "SO int"],
            [lambda {|meth| meth.call("a",1,1,1)}, "SO string params(int[])"],
            [lambda {|meth| meth.call("a","b","c")}, "SO string params(string[])"],
            [lambda {|meth| meth.call("a",1,1)}, "SO string int int"],
            [lambda {|meth| meth.call(1,2,3)}, "SO params(int[])"]]
    @out_or_ref_cals = [[lambda {|meth| meth.overload(System::String.GetType.MakeByRefType.to_class).call()}, "SO ref string"]] #this array will hold more once this works.
  end

  it "is performed" do
    @overloaded_methods.call(100).should equal_clr_string("one arg")
    @overloaded_methods.call(100, 100).should equal_clr_string("two args")
    @klass.overloaded(100).should equal_clr_string("one arg")
    @klass.overloaded(100, 100).should equal_clr_string("two args")
    @calls.each do |meth, result|
      meth.call(@void_method)
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[0]).should == 1
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[1]).should == System::Array.of(Fixnum).new(1,1)
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@ref_methods[0]).should equal_clr_string result
      meth.call(@ref_methods[1]).should == System::Array.of(System::String).new(1,result.to_clr_string)
    end
  end

  it "correctly binds with methods of different visibility" do
    method = @klass.method(:public_protected_overload)
    @klass.public_protected_overload.should equal_clr_string("public overload")
    
    lambda { @klass.public_protected_overload("abc") }.should raise_error(ArgumentError, /1 for 0/)
    
    method.call.should equal_clr_string("public overload")
    lambda { method.call("abc").should equal_clr_string("protected overload") }.should raise_error(ArgumentError, /1 for 0/)
  end
  
  #http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1849
  it "is performed for various ref and out calls" do
    @out_or_ref_calls.each do |meth, result| 
      meth.call(@void_method)
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[0]).should == 1
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[1]).should == System::Array.of(Fixnum).new(1,1)
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@ref_methods[0]).should equal_clr_string result
      meth.call(@ref_methods[1]).should == System::Array.of(System::String).new(1,result.to_clr_string)
    end
  end
end

describe "Selecting .NET overloads" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end
  
  it "is allowed" do
    @methods.overload(Fixnum,Fixnum).call(100,100).should equal_clr_string("two args")
  end

  it "correctly reports error message" do
    #regression test for RubyForge 24112
    lambda {@methods.overload(Fixnum).call}.should raise_error(ArgumentError, /0 for 1/)
  end
end

