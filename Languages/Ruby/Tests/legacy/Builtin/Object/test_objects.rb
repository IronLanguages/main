# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../../Util/simple_test.rb"

describe "Object#send" do
  it "test dynamic site polymorphism" do
    # test the dynamic site to make sure it is truly polymorphic
    # mix some errors in as well
    def foo x, y, z; x.send y, z; end
    foo(1, :+, 2).should == 3
    foo(1, "-", 2).should == -1
    skip "TODO: The current exception message is - can't convert Array into Symbol" do
        should_raise(TypeError, "[:+] is not a symbol") { foo(1,[:+],2) }
    end
    foo("abc", :<<, "def").should == "abcdef"
    foo([1,2], :+, [3,4]).should == [1,2,3,4]
    skip "TODO: The current exception message is - can't convert Array into Symbol" do
        should_raise(TypeError, "[:+] is not a symbol") { foo(1,[:+],2) }
    end
    
    def foo2 x, *y; x.send *y; end
    foo2(1, :+, 2).should == 3
    should_raise(ArgumentError, "no method name given") { foo2(1) }
    foo2(1, *["-", 2]).should == -1
    should_raise(ArgumentError, "no method name given") { foo2(1, *[]) }
    foo2("abc", *[:<<, "def"]).should == "abcdef"
    should_raise(ArgumentError, "no method name given") { foo2(1) }
  end
  
  it "test overriding send works" do
    class TypeOverridingSend1
      def send x; x; end
      def inspect; "#<TypeOverridingSend1>"; end
    end
    
    a = TypeOverridingSend1.new
    a.send("test2").should == "test2"
    # TODO: error message is wrong; just test that the right exception is thrown
    should_raise(NoMethodError) { a.__send__("test2") }
    skip "TODO: our error message is wrong; it's not calling inspect" do
      should_raise(NoMethodError, "undefined method `test2' for #<TypeOverridingSend1>") { a.__send__("test2") }
    end
    a.__send__(:send, "test2") == "test2"
    # just because method bind failed we shouldn't try to fall back to Object#send
    should_raise(ArgumentError, "wrong number of arguments (2 for 1)") { a.send("==", a) }
  end
  
  it "test recursive send works" do
    x = [1,2,3]
    y = "[1, 2, 3]"
    x.send(:send, :__send__, "send", "__send__", :inspect).should == y
    # This is put at 20 instead of a bigger number because we it's a slow test
    # (our unsplatting logic for the recusive send w/ splatting is O(N^2) with the
    # number of arguments... however, this is a *highly* unlikely scenario in real code)
    [:send, :__send__, "send", "__send__"].each do |s|
      20.times do |i|
        args = Array.new(i, s) << :inspect
        x.send(*args).should == y
      end
    end
  end
  
  it "passing an object derived from String to send should work" do
    # you can derive a type from String, and it works, based on the string though
    # (none of these overloads will get called)
    class MyString < String
      def to_str; "hash"; end
      def to_s; "hash"; end
      def inspect; "hash"; end
      def to_sym; :hash; end
    end
    
    s = MyString.new
    s << "inspect"
    # Note: this should call inspect, not hash
    [1,2,3].send(s).should == "[1, 2, 3]"
  end

  # TODO: fix this once super works
  skip "TODO: test that super works in overriden send" do
    class TypeOverridingSend2
      # TODO: uncomment once super AST transformation works
      #def send x, y; super(x, y); end
      def inspect; "#<TypeOverridingSend2>"; end
    end
    
    a = TypeOverridingSend2.new
    a.send(:==, a).should == true
    a.send(:==, "123").should == false
  end
  
end
  
describe "Object#__send__" do
  it "__send__ can be overriden" do
    class TypeSendUnderbar1
      def __send__ x; x; end
      def inspect; "#<TypeSendUnderbar1>"; end
    end
    
    a = TypeSendUnderbar1.new
    a.__send__("test2").should == "test2"
    # TODO: error message is wrong; just test that the right exception is thrown
    should_raise(NoMethodError) { a.send("test2") }
    skip "TODO: our error message is wrong; it's not calling inspect" do
      should_raise(NoMethodError, "undefined method `test2' for #<TypeSendUnderbar1>") { a.send("test2") }
    end
    a.send(:__send__, "test2") == "test2"
    # just because method bind failed we shouldn't try to fall back to Object#__send__
    should_raise(ArgumentError, "wrong number of arguments (2 for 1)") { a.__send__("==", a) }
  end
end

describe "Object#dup" do
  it "dup should copy instance variables" do
    class TestVars
      def x= v; @x=v; end
      def x; @x; end
      def y= v; @y=v; end
      def y; @y; end
      def z= v; @z=v; end
      def z; @z; end      
    end
    a = TestVars.new
    a.x = "a.x"
    a.y = "a.y"
    b = a.dup
    [a.x, a.y, a.z].should == ["a.x", "a.y", nil]
    [b.x, b.y, b.z].should == ["a.x", "a.y", nil]
    a.x << "-test"
    b.z = "b.z"
    [a.x, a.y, a.z].should == ["a.x-test", "a.y", nil]
    [b.x, b.y, b.z].should == ["a.x-test", "a.y", "b.z"]    
  end
  
  it "dup should copy tainted state" do
    x = Object.new
    x.tainted?.should == false
    x.taint
    x.tainted?.should == true
    y = x.dup
    y.tainted?.should == true
    y.untaint
    y.tainted?.should == false
    x.tainted?.should == true
  end 
  
  it "dup should not copy frozen state" do
    x = Object.new
    x.frozen?.should == false
    x.freeze
    x.frozen?.should == true
    x.dup.frozen?.should == false
    x.frozen?.should == true
  end

  it "dup on builtin class (reference type)" do
    x = "test"
    foo = "foo"
    x.instance_variable_set(:@foo, foo)
    x.instance_variable_set(:@bar, "bar")
    y = x.dup
    y.should == x
    y.should == "test"
    y.instance_variable_get(:@foo).should == "foo"
    y.instance_variable_get(:@bar).should == "bar"
    foo << "bar"
    y.instance_variable_get(:@foo).should == "foobar"
    x.instance_variable_get(:@foo).should == "foobar"
    y.instance_variables.sort.should == ["@bar", "@foo"]
  end
  
  it "dup on builtin value type raises an error" do
    should_raise(TypeError, "can't dup NilClass") { nil.dup }
    should_raise(TypeError, "can't dup Fixnum") { 123.dup }
    should_raise(TypeError, "can't dup Symbol") { :abc.dup }
  end
end

describe "Object#clone" do
  it "clone should copy instance variables" do
    a = TestVars.new
    a.x = "a.x"
    a.y = "a.y"
    b = a.clone
    [a.x, a.y, a.z].should == ["a.x", "a.y", nil]
    [b.x, b.y, b.z].should == ["a.x", "a.y", nil]
    a.x << "-test"
    b.z = "b.z"
    [a.x, a.y, a.z].should == ["a.x-test", "a.y", nil]
    [b.x, b.y, b.z].should == ["a.x-test", "a.y", "b.z"]    
  end
  
  it "clone should copy tainted state" do
    x = Object.new
    x.tainted?.should == false
    x.taint
    x.tainted?.should == true
    y = x.clone
    y.tainted?.should == true
    y.untaint
    y.tainted?.should == false
    x.tainted?.should == true
  end 
  
  it "clone should copy frozen state" do
    x = Object.new
    x.frozen?.should == false
    x.clone.frozen?.should == false
    x.freeze
    x.frozen?.should == true
    x.clone.frozen?.should == true
  end

  it "clone on builtin class (reference type)" do
    x = "test"
    foo = "foo"
    x.instance_variable_set(:@foo, foo)
    x.instance_variable_set(:@bar, "bar")
    y = x.clone
    y.should == x
    y.should == "test"
    y.instance_variable_get(:@foo).should == "foo"
    y.instance_variable_get(:@bar).should == "bar"
    foo << "bar"
    y.instance_variable_get(:@foo).should == "foobar"
    x.instance_variable_get(:@foo).should == "foobar"
    y.instance_variables.sort.should == ["@bar", "@foo"]
  end
  
  it "clone on builtin value type raises an error" do
    should_raise(TypeError, "can't clone NilClass") { nil.clone }
    should_raise(TypeError, "can't clone Fixnum") { 123.clone }
    should_raise(TypeError, "can't clone Symbol") { :abc.clone }
  end
end

describe "Object#instance_eval" do
  it "instance_eval should change the self" do
    test_objects = [Object.new, {:key=>:value}, [1,2,3], "hello"]
    test_objects.each do |e|
      e.instance_eval { self.should.equal? e }
    end
  end

  it 'repeat the previous' do
      class C; end 
      x = C.new 
      x.instance_eval { def f; 12; end }
      x.f.should == 12
      
      #y = C.new 
      #should_raise(NoMethodError) { y.f }
      
      # TODO: add C.instance_eval scenario later
  end 
  
  it "instance_eval allows accessing instance variables" do
    class TestEval
      @@class_var = "xyz"
      def initialize
        @abc = "def"
      end
      def bar
        @abc = "zzz"
        @@class_var = "yyy"
      end
      def baz
        123
      end
      def TestEval.ClassVar
        @@class_var
      end 
    end
    t = TestEval.new
    x = t.instance_eval { @abc }
    x.should == "def"
    x = t.instance_eval { bar; baz }
    x.should == 123

    t.instance_variable_get(:@abc).should == "zzz"
    TestEval.ClassVar.should == "yyy"
  end
  
  it "return value" do 
    x = 'abc'.instance_eval { length }
    x.should == 3
    
    x = 'abc'.instance_eval {  }
    x.should == nil
  end 
  
=begin
  # IronRuby has a differnet error message
  it "instance_eval with no block should raise an error" do
    should_raise(ArgumentError, 'block not supplied') { instance_eval }
  end
=end
  
  it "instance_eval and break" do
    x = Object.new
    y = x.instance_eval { break 123 }
    y.should == 123
    def foo(&blk)
      instance_eval(&blk)
    end
    y = foo { break 456 }
    y.should == 456    
  end

  it "instance_eval and next" do
    x = Object.new
    y = x.instance_eval { next 123 }
    y.should == 123
    y = foo { next 456 }
    y.should == 456      
  end

  it "instance_eval and return" do
    x = Object.new
    should_raise(LocalJumpError) { x.instance_eval { return 'test0' } }
    def bar
      instance_eval { return 'test1' }
    end
    bar.should == 'test1'
    def bar
      foo { return 'test2' }
    end
    bar.should == 'test2'
  end
  
  it "instance_eval and redo" do
    x = Object.new
    again = true
    y = x.instance_eval do
      if again
        again = false
        redo
      else
        3.14
      end
    end
    y.should == 3.14    
    z = foo do
      if y < 9
        y *= 2
        redo
      end
      y
    end
    y.should == 12.56
    z.should == 12.56
  end

  it "instance_eval and retry" do
    x = Object.new
    total = 0
    y = x.instance_eval do
      total += 1
      if total < 8; retry; end
      total
    end
    y.should == 8
    total.should == 8
    y = foo do
      total *= 2
      if total < 1000; retry; end
      total
    end
    y.should == 1024
    total.should == 1024
  end
  
  it "instance_eval and exception" do 
    x = Object.new
    begin 
        x.instance_eval { 1/0 }
    rescue ZeroDivisionError
    else 
        raise("ZeroDivisionError should be thrown and caught")
    end
    
    x.instance_eval do 
        begin 
            1/0
        rescue ZeroDivisionError
        end 
    end 
  end 
  
  skip "acts on module/class too" do
    y = Kernel.instance_eval do 
        x = self.lambda { 2 }
        x.call
    end 
    y.should == 2
    
    y = Hash.instance_eval { new(99)  }
    y.should == {}
    y[0].should == 99
  end
  
end

finished
