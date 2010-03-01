require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overload resolution" do
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
    @out_or_ref_calls = [[lambda {|meth| meth.overload(System::String.to_clr_ref).call("1")}, "SO ref string"]] #this array will hold more once this works.
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
  
  it "is performed for various ref and out calls" do
    @out_or_ref_calls.each do |meth, result| 
      meth.call(@void_method).should == '1'
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[0]).should == [1,'1']
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@val_methods[1]).should == [System::Array.of(Fixnum).new(1,1), '1']
      @klass.tracker.should equal_clr_string result
      @klass.tracker = System::String.empty
      meth.call(@ref_methods[0]).should == [result, '1']
      meth.call(@ref_methods[1]).should == [System::Array.of(System::String).new(1,result.to_clr_string), '1']
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

