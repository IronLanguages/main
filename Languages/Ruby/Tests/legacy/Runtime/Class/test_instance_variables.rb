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

require '../../util/simple_test.rb'

describe "Instance variables used in a type" do
  it "basic usage" do
    class My_variables
        def initialize
            @iv = 20
        end 

        def check_iv; @iv; end 
    end 

    x = My_variables.new
    should_raise(NoMethodError) { x.iv }

    x.check_iv.should == 20   

    x.instance_variables.should == ["@iv"]
    x.instance_variable_get(:@iv).should == 20
  end
  
  it "instance variables are initialized to nil" do
    class My_instance_variable
        @iv = 10
        def check_iv; @iv; end
        def set_iv; @iv = 20; end
    end 

    x = My_instance_variable.new 
    x.check_iv.should == nil           ## @iv is not assigned during the class creation
    x.set_iv
    x.check_iv.should == 20
  end
  
  it "instance variables are per instance" do
    class TestVars1
      def x= v; @x=v; end
      def x; @x; end
    end
    b = TestVars1.new
    b.x = 5
    b.x.should == 5
    b2 = TestVars1.new
    b2.x.should == nil
    b.x.should == 5
    b2.x = "abc"
    b2.x.should == "abc"
    b.x.should == 5
  end
  
  it "instance variable assignment stores references (e.g. it doesn't copy the value)" do    
    b = TestVars1.new
    val = "foo"
    b.x = val
    b.x.should == "foo"
    val << "bar"
    b.x.should == "foobar"
  end
  
  it "multiple instance variables" do
    class TestVars2
      def x= v; @x=v; end
      def x; @x; end
      def y= v; @y=v; end
      def y; @y; end
      def z= v; @z=v; end
      def z; @z; end      
    end
    
    a = TestVars2.new
    b = TestVars2.new
    a.x = "a.x"
    a.y = "a.y"
    b.y = "b.y"
    b.z = "b.z"
    
    [a.x, a.y, a.z].should == ["a.x", "a.y", nil]
    [b.x, b.y, b.z].should == [nil, "b.y", "b.z"]
  end
end

describe "Instance variables are shared by base classes" do
  it "derivied class and base use the same field for instance vars" do
    class TestBase
      def bx= v; @x=v; end
      def bx; @x; end
    end
    class TestDerived < TestBase
      def dx= v; @x=v; end
      def dx; @x; end
    end
    
    a = TestDerived.new
    [a.bx,a.dx].should == [nil,nil]
    a.bx = 123
    a.dx.should == 123
    a.bx.should == 123
    a.dx = "test"
    a.bx.should == "test"
    a.dx.should == "test"
  end
  
  it "test with multiple base classes" do
    class TestClass1
      def x1= v; @x=v; end
      def x1; @x; end
    end
    class TestClass2 < TestClass1
      def x2= v; @x=v; end
      def x2; @x; end
    end
    class TestClass3 < TestClass2
      def x3= v; @x=v; end
      def x3; @x; end
    end
    class TestClass4 < TestClass3
      def x4= v; @x=v; end
      def x4; @x; end
    end
    class TestClass5 < TestClass4
      def x5= v; @x=v; end
      def x5; @x; end
    end
    
    t = TestClass5.new
    t.x5 = 5
    [t.x5,t.x4,t.x3,t.x2,t.x1].each { |i| i.should == 5 }
    t.x4 = 4
    [t.x5,t.x4,t.x3,t.x2,t.x1].each { |i| i.should == 4 }
    t.x3 = 3
    [t.x5,t.x4,t.x3,t.x2,t.x1].each { |i| i.should == 3 }
    t.x2 = 2
    [t.x5,t.x4,t.x3,t.x2,t.x1].each { |i| i.should == 2 }
    t.x1 = 1
    [t.x5,t.x4,t.x3,t.x2,t.x1].each { |i| i.should == 1 }    
  end
  
end

describe "Instance variables are shared by mixins" do
  it "mixins and the containing class use the same instance variables" do
    module TestMixin1
      def mx= v; @x=v; end
      def mx; @x; end
    end
    class TestDerived < TestBase
      include TestMixin1
    end
    
    a = TestDerived.new
    [a.mx, a.bx, a.dx].should == [nil, nil, nil]
    a.mx = :foobar
    a.bx.should == :foobar
    a.dx.should == :foobar
    a.mx.should == :foobar
    a.bx = 123
    a.dx.should == 123
    a.bx.should == 123
    a.mx.should == 123
    a.dx = "test"
    a.bx.should == "test"
    a.dx.should == "test"
    a.mx.should == "test"
  end
  
  it "the same mixin can be included in multiple classes" do
    # if the same mixin is used by multiple classes,
    # it always sees instance variables on the current instance.
    
    class TestDerived2
      include TestMixin1
      def d2x= v; @x=v; end
      def d2x; @x; end      
    end
    
    a = TestDerived.new
    b = TestDerived2.new
    a.mx = :A
    b.mx = :B
    [a.dx,a.mx,b.d2x,b.mx].should == [:A,:A,:B,:B]
  end
end

describe "Instance variables used in modules/functions" do
  it "test module level get/set methods" do
    def set_instance_var; @iv = 123; end
    def get_instance_var; @iv; end

    class Using_module_get_set_functions
      def check_iv; get_instance_var; end
      def set_iv; set_instance_var; end  
    end

    x = Using_module_get_set_functions.new
    x.check_iv.should == nil
    x.set_iv.should == 123
    x.check_iv.should == 123
    x.instance_variables.should == ["@iv"]
    x.instance_variable_get(:@iv).should == 123
  end

  it "test module instance variables" do
    instance_variables.should == []
    @modvar = {1=>2}
    @modvar.should == {1=>2}
    instance_variable_get(:@modvar).should == {1=>2}
    instance_variables.should == ["@modvar"]    
  end
  
  it "test module instance variable is still set in a new scope" do
    @modvar.should == {1=>2}
  end  
end

describe "Instance variables used in class scope" do
    class TestClassScope
      @abc = "original"
      @foo = "bar"
      
      def setvars x, y
        @abc = x
        @foo = y
      end
      
      @abc = "test"
      
      def self.setvars x, y
        @abc << x
        @foo << y 
      end
      def self.getvars
        [@abc, @foo]
      end  
    end  

  it "instance vars in class scope set the var on the class" do
    TestClassScope.instance_variables.sort.should == ["@abc", "@foo"]
    TestClassScope.instance_variable_get(:@abc).should == "test"
    TestClassScope.instance_variable_get(:@foo).should == "bar"
  end
  
  it "instance variables on an instance don't interfere with the class's instance vars" do
    t = TestClassScope.new
    t.instance_variables.should == []
    t.setvars("test2", "foobar")
    t.instance_variables.sort.should == ["@abc", "@foo"]
    t.instance_variable_get(:@abc).should == "test2"
    t.instance_variable_get(:@foo).should == "foobar"
    TestClassScope.instance_variable_get(:@abc).should == "test"
    TestClassScope.instance_variable_get(:@foo).should == "bar"
  end
  
  it "class instance vars can be changed in the class method" do 
    TestClassScope.setvars "1", "2"
    TestClassScope.instance_variable_get(:@abc).should == "test1"
    TestClassScope.instance_variable_get(:@foo).should == "bar2"
    t = TestClassScope.new 
    t.setvars "&", "*"
    TestClassScope.setvars "3", "4"
    TestClassScope.getvars.should == [ 'test13', 'bar24' ]   
  end 
end

describe "dynamic instance variable methods" do
  it "basic usage" do
    x = Object.new
    x.instance_variables.should == []
    x.instance_variable_get(:@iv).should == nil
    x.instance_variables.should == []
    x.instance_variable_set(:@iv, "test").should == "test"
    x.instance_variables.should == ["@iv"]
    x.instance_variable_defined?(:@iv).should == true
    x.instance_variable_defined?("@iv").should == true
    x.instance_variable_defined?(:@iv2).should == false
    x.instance_variable_defined?("@iv2").should == false
    x.inspect.split[1].should == '@iv="test">'
    x.instance_variable_get(:@iv).should == "test"
    x.instance_variable_get("@iv").should == "test"
    x.instance_variable_set(:@iv, nil).should == nil
    x.instance_variable_defined?(:@iv).should == true
  end
  
  it "bad instance variable names cause a NameError" do
    x = Object.new
    should_raise(NameError) { x.instance_variable_get :@@iv }
    should_raise(NameError) { x.instance_variable_get "@@iv" }
    should_raise(NameError) { x.instance_variable_get :iv }
    should_raise(NameError) { x.instance_variable_get "iv" }
    should_raise(NameError) { x.instance_variable_get "@iv@x" }
    should_raise(NameError) { x.instance_variable_set :@@iv, nil }
    should_raise(NameError) { x.instance_variable_set "@@iv", nil }
    should_raise(NameError) { x.instance_variable_set "@i@v", nil }
    should_raise(NameError) { x.instance_variable_set :iv, nil }
    should_raise(NameError) { x.instance_variable_defined? :@@iv }
    should_raise(NameError) { x.instance_variable_defined? "@@iv" }
    should_raise(NameError) { x.instance_variable_defined? "@i@v" }
    should_raise(NameError) { x.instance_variable_defined? :iv }
  end
  
  it "test setting instance vars on Fixnums (value equality)" do
    5.instance_variable_get(:@a).should == nil
    5.instance_variable_defined?(:@a).should == false
    5.instance_variables.should == []
    5.instance_variable_set(:@a, "test").should == "test"
    5.instance_variable_get(:@a).should == "test"
    5.instance_variables.should == ["@a"]
    (6-1).instance_variable_set(:@b, [1,2,3,4]).should == [1,2,3,4]
    (1+4).instance_variables.sort.should == ["@a", "@b"]
    (2+3).instance_variable_get(:@b).should == [1,2,3,4]
    
    # Ruby Fixnums are in the range 0x3FFFFFFF <= fixnum <= -0x40000000
    # However, in .NET we can use the full 32 bits, e.g. 0x7FFFFFFF <= int32 <= -0x80000000
    # at least test that we can represent the full Ruby Fixnum range
    0x3FFFFFFF.instance_variable_set(:@test, 123).should == 123
    0x3FFFFFFF.instance_variable_get(:@test).should == 123
    (-0x40000000).instance_variable_set(:@test, 456).should == 456
    (-0x40000000).instance_variable_get(:@test).should == 456
  end
  
  it "test setting instance vars on Symbols (value equality)" do
    :foo.instance_variable_get(:@a).should == nil
    :foo.instance_variable_defined?(:@a).should == false
    :foo.instance_variables.should == []
    :foo.instance_variable_set(:@a, "test").should == "test"
    :foo.instance_variable_get(:@a).should == "test"
    :foo.instance_variables.should == ["@a"]
    "foo".to_sym.instance_variable_set(:@b, [1,2,3,4]).should == [1,2,3,4]
    ("f" + "oo").to_sym.instance_variables.sort.should == ["@a", "@b"]
    ("f" + "o" + "o").to_sym.instance_variable_get(:@b).should == [1,2,3,4]
    
    :@test.instance_variable_set(:@a, "test").should == "test"
    "@test".to_sym.instance_variable_get(:@a).should == "test"
    "@test".to_sym.instance_variables.should == ["@a"]    
  end

  it "test instance vars on strings (reference equality)" do
    x = "abc"
    x.instance_variable_get(:@a).should == nil
    x.instance_variable_defined?(:@a).should == false
    x.instance_variables.should == []
    x.instance_variable_set(:@a, "test").should == "test"
    x.instance_variable_get(:@a).should == "test"
    x.instance_variables.should == ["@a"]
    y = "abc"
    y.instance_variable_set(:@b, [1,2,3,4]).should == [1,2,3,4]
    x.instance_variables.should == ["@a"]
    y.instance_variables.should == ["@b"]
    y.instance_variable_get(:@b).should == [1,2,3,4]
  end
  
  it "test instance vars on nil (value equality)" do
    nil.instance_variables.should == []
    nil.instance_variable_set(:@a, "test").should == "test"
    nil.instance_variable_get(:@a).should == "test"
    nil.instance_variables.should == ["@a"]
  end
  
  it "test instance vars on floats (reference equality)" do
    x = 2.0
    x.instance_variable_set(:@a, "test").should == "test"
    x.instance_variable_get(:@a).should == "test"
    x.instance_variables.should == ["@a"]
    y = 2.0
    y.instance_variable_set(:@b, [1,2,3,4]).should == [1,2,3,4]
    x.instance_variables.should == ["@a"]
    y.instance_variables.should == ["@b"]
    y.instance_variable_get(:@b).should == [1,2,3,4]
  end
  
end

finished
