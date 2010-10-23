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

require "../../util/simple_test.rb"

describe "Module constant lookup" do
  it "modules can access constants on Object" do
    class Object
      def test_const_lookup
        Object
      end
    end

    # this should successfully find Object
    x = module TestModule
      test_const_lookup.name
    end
    x.should == "Object"
    TestModule.test_const_lookup.should.equal? Object
  end
end

describe "Module#ancestors" do
  it "ancestors on builtin types" do
    Kernel.ancestors.should == [Kernel]
    Object.ancestors.should == [Object, Kernel]
    Class.ancestors.should == [Class, Module, Object, Kernel]
    Enumerable.ancestors.should == [Enumerable]
    Comparable.ancestors.should == [Comparable]
    Fixnum.ancestors.should == [Fixnum, Integer, Precision, Numeric, Comparable, Object, Kernel]
    Array.ancestors.should == [Array, Enumerable, Object, Kernel]
    Hash.ancestors.should == [Hash, Enumerable, Object, Kernel]
    String.ancestors.should == [String, Enumerable, Comparable, Object, Kernel]
  end
  
  it "ancestors on user types" do
    # TODO: modules including other modules
    # (doesn't work today)
    module AncestorModule1; end
    module AncestorModule2; end
    AncestorModule1.ancestors.should == [AncestorModule1]
    AncestorModule2.ancestors.should == [AncestorModule2]
    
    class AncestorsClass1; end
    AncestorsClass1.ancestors.should == [AncestorsClass1, Object, Kernel]
    
    class AncestorsClass1
       include AncestorModule1
    end
    AncestorsClass1.ancestors.should == [AncestorsClass1, AncestorModule1, Object, Kernel]
    
    class AncestorsClass2 < AncestorsClass1
      include AncestorModule2
    end
    AncestorsClass2.ancestors.should == [AncestorsClass2, AncestorModule2, AncestorsClass1, AncestorModule1, Object, Kernel]
  end
end

describe "Module#module_eval" do
  it "module_eval allows defining methods on a module" do
    module TestModuleEval
      def bar; "hello"; end
    end
    x = TestModuleEval.module_eval do
      def foo; bar; end
      self
    end
    x.name.should == "TestModuleEval"
    should_raise(NoMethodError) { TestModuleEval.bar }
    should_raise(NoMethodError) { TestModuleEval.foo }
     
    class TestModuleEval2
      include TestModuleEval
      def baz; foo; end
    end
    t = TestModuleEval2.new
    t.foo.should == "hello" 
    t.baz.should == "hello"    
    
    # try module method, constant
    module TestModuleEval
        def self.m1; "m1"; end
        CONST1 = "const"
    end 
    x = TestModuleEval.module_eval do 
        def self.m2; self.m1.upcase; end
        self::CONST2 = TestModuleEval::CONST1.upcase
    end 
    x.should == "CONST"
    TestModuleEval::CONST2.should == "CONST"
    TestModuleEval.m2.should == "M1"
    
    # empty block
    x = TestModuleEval.module_eval {}
    x.should == nil
  end
 
  it "module_eval with no block should raise an error" do
    skip "TODO: Current exception message is - wrong number or type of arguments" do
        should_raise(ArgumentError, 'block not supplied') { Enumerable::module_eval }
    end
  end
  
  it "module_eval allows defining methods on a class" do
    class TestModuleEvalClass
      def bar; "hello"; end
      def self.m1; 'm1'; end 
    end
    x = TestModuleEvalClass.module_eval do
      def foo; bar; end
      def TestModuleEvalClass.m2; self.m1.upcase; end 
      self
    end
    x.should.equal? TestModuleEvalClass
    should_raise(NoMethodError) { TestModuleEvalClass.bar }
    should_raise(NoMethodError) { TestModuleEvalClass.foo }
    t = TestModuleEvalClass.new
    t.foo.should == "hello"
    TestModuleEvalClass.m2.should == "M1"
  end

  it "module_eval and break" do
    x = TestModuleEval
    y = x.module_eval { break 123 }
    y.should == 123
    def foo(&blk)
      TestModuleEval.module_eval(&blk)
    end
    y = foo { break 456 }
    y.should == 456    
  end

  it "module_eval and next" do
    x = TestModuleEval
    y = x.module_eval { next 123 }
    y.should == 123
    y = foo { next 456 }
    y.should == 456      
  end

  it "module_eval and return" do
    x = TestModuleEval
    should_raise(LocalJumpError) { x.module_eval { return 'test0' } }
    def bar
      TestModuleEval.module_eval { return 'test1' }
    end
    bar.should == 'test1'
    def bar
      foo { return 'test2' }
    end
    bar.should == 'test2'
  end
  
  it "module_eval and redo" do
    x = TestModuleEval
    again = true
    y = x.module_eval do
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

  it "module_eval and retry" do
    x = TestModuleEval
    total = 0
    y = x.module_eval do
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
end

describe "Module#class_eval" do
  it "class_eval is an alias for module_eval" do
    class TestClassEval
      def bar2; "hello"; end
    end
    x = TestClassEval.class_eval do
      def foo2; bar2; end
      self
    end
    x.should.equal? TestClassEval
    should_raise(NoMethodError) { TestClassEval.bar2 }
    should_raise(NoMethodError) { TestClassEval.foo2 }
    t = TestClassEval.new
    t.bar2.should == 'hello'
    t.foo2.should == 'hello'

    skip "TODO: Current exception message is - wrong number or type of arguments" do
        should_raise(ArgumentError, 'block not supplied') { Enumerable::class_eval }
    end
  end
end

finished
