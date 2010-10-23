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

# make class_variable_set and class_variable_get callable
describe "Dynamic class variable methods" do
  skip "(BUG: private visibility is not working) class variable methods have correct visibility" do
    should_raise(NoMethodError, "private method `class_variable_get' called for Object:Class") do
      Object.class_variable_get :@@foo
    end
    should_raise(NoMethodError, "private method `class_variable_set' called for Object:Class") do
      Object.class_variable_set :@@foo, 'foo'
    end
    should_raise(NoMethodError, "private method `class_variable_get' called for Enumerable:Module") do
      Enumerable.class_variable_get :@@foo
    end
    should_raise(NoMethodError, "private method `class_variable_set' called for Enumerable:Module") do
      Enumerable.class_variable_set :@@foo, 'foo'
    end
    Object.class_variables.should == []
    Object.class_variable_defined?(:@@foo).should == false
    Enumerable.class_variables.should == []
    Enumerable.class_variable_defined?(:@@foo).should == false
  end
  
  it "class variable methods shouldn't exist on objects" do
    should_raise(NoMethodError) { Object.new.class_variable_get :@@foo }
    should_raise(NoMethodError) { Object.new.class_variable_set :@@foo, 'foo' }
    should_raise(NoMethodError) { Object.new.class_variables }
    should_raise(NoMethodError) { Object.new.class_variable_defined? :@@foo }
  end
  
  it "Module#class_variable_set, Module#class_variable_get" do
    # make the method visible
    class Module
      def class_var_get x
        class_variable_get x
      end
      def class_var_set x, v
        class_variable_set x, v
      end
    end
    
    class Bob 
        def bar; @@foo = 200; end 
    end
    should_raise(NameError, 'uninitialized class variable @@foo in Bob') { Bob.class_var_get(:@@foo) }
    Bob.class_variables.should == []
    Bob.class_var_set(:@@foo, 123).should == 123
    Bob.class_var_get('@@foo').should == 123
    Bob.class_variables.should == ['@@foo']
    Bob.class_variable_defined?(:@@foo).should == true    
    Bob.class_var_set('@@foo', '654').should == '654'
    Bob.class_var_get(:@@foo).should == '654'
  end
  
  it "bad class variable names cause a name error" do
    should_raise(NameError) { Object.class_variable_defined?(:@iv) }
    should_raise(NameError) { Object.class_variable_defined?("@iv") }
    should_raise(NameError) { Object.class_variable_defined?(:iv) }
    should_raise(NameError) { Object.class_variable_defined?("iv") }
    should_raise(NameError) { Object.class_variable_defined?("@@iv@x") }
    should_raise(NameError) { Object.class_var_get(:@iv) }
    should_raise(NameError) { Object.class_var_get("@iv") }
    should_raise(NameError) { Object.class_var_get(:iv) }
    should_raise(NameError) { Object.class_var_get("iv") }
    should_raise(NameError) { Object.class_var_get("@@iv@x") }
    should_raise(NameError) { Object.class_var_set(:@iv, 10) }
    should_raise(NameError) { Object.class_var_set('CV', 10) }
    should_raise(NameError) { Object.class_var_set(:lv, 10) }
  end
end

describe "Class variables used inside a type" do
  it "basic usages inside the class" do
    class My_variables
        @@sv = 10
        def check_sv; @@sv; end
        def My_variables.check_sv; @@sv; end 
        def My_variables.check_sv2; @@sv2; end 
    end 
    My_variables::check_sv.should == 10
    My_variables.check_sv.should == 10
    should_raise(NameError, 'uninitialized class variable @@sv2 in My_variables') { My_variables.check_sv2 }
    My_variables.class_variable_defined?(:@@sv).should == true
    My_variables.class_variable_defined?(:@@sv2).should == false

    x = My_variables.new
    should_raise(NoMethodError) { My_variables.sv }
    should_raise(NoMethodError) { My_variables::sv }
    x.check_sv.should == 10
    x.class.class_variables.should == ["@@sv"]
    x.class.class_var_get(:@@sv).should == 10
  end
  
  it "class variables are different from instance variables" do
    class My_variables2
      @@v = 555
      @v = 789  
      def initialize
        @@v = 123
        @v = 456
      end
      def self.check_sv; @@v; end
      def self.check_iv; @v; end
      def check_sv; @@v; end
      def check_iv; @v; end
    end
    
    My_variables2.check_sv.should == 555
    My_variables2.check_iv.should == 789
    x = My_variables2.new
    x.check_sv.should == 123
    x.check_iv.should == 456
    x.class.check_sv.should == 123
    x.class.check_iv.should == 789   
    
    My_variables2.class_variables.should == [ '@@v' ]
  end  
end

describe "class variables are stored on the module/class where they are first set" do
  it "class variables show all variables in the base & mixins" do
    module Test1Module1
      def m1_a= x; @@a = x; end
      def m1_a; @@a; end
    end
    module Test1Module2
      def m2_b= x; @@b = x; end
      def m2_b; @@b; end
    end
    class Test1Class1
      include Test1Module1
      def c1_c= x; @@c = x; end
      def c1_c; @@c; end
    end
    class Test1Class2 < Test1Class1
      include Test1Module2
      def c2_d= x; @@d = x; end
      def c2_vars; [@@a, @@b, @@c, @@d]; end
    end
    
    Test1Class2.class_variables.should == []
    x = Test1Class2.new
    x.m1_a = 123
    x.m2_b = 456
    x.c1_c = 789
    x.c2_d = 555
    Test1Class2.class_variables.should == ["@@d", "@@b", "@@c", "@@a"]
    Test1Class1.class_variables.should == ["@@c", "@@a"]
    Test1Module1.class_variables.should == ["@@a"]
    Test1Module2.class_variables.should == ["@@b"]
    x.c2_vars.should == [123, 456, 789, 555]
    [x.m1_a, x.m2_b, x.c1_c].should == [123, 456, 789]
  end
  
  it "class variables stay where they are first set" do
    module Test2Module1
      def m1_a= x; @@a = x; end
    end
    module Test2Module2
      def m2_b= x; @@b = x; end
    end
    class Test2Class1
      include Test2Module1
      def c1_c= x; @@c = x; end
    end
    class Test2Class2 < Test2Class1
      include Test2Module2
      def c2_a= x; @@a = x; end
      def c2_b= x; @@b = x; end
      def c2_c= x; @@c = x; end
      def c2_d= x; @@d = x; end
      def c2_vars; [@@a, @@b, @@c, @@d]; end
    end
    
    Test2Class2.class_variables.should == []
    x = Test2Class2.new
    x.c2_a = 12.1
    x.c2_b = 34.3
    x.c2_c = 56.5
    x.c2_d = 78.8
    Test2Class2.class_variables.sort.should == ["@@a", "@@b", "@@c", "@@d"]
    x.c2_vars.should == [12.1, 34.3, 56.5, 78.8]
    Test2Class1.class_variables.should == []
    Test2Module1.class_variables.should == []
    Test2Module2.class_variables.should == []
    
    # now set the variables on the base class & included modules
    x.c1_c = 'testC1'
    Test2Class2.class_variables.sort.should == ["@@a", "@@b", "@@c", "@@d"]
    Test2Class1.class_variables.should == ['@@c']
    Test2Module1.class_variables.should == []
    Test2Module2.class_variables.should == []
    x.m1_a = 'testM1'
    Test2Class1.class_variables.should == ['@@c', '@@a']
    Test2Module1.class_variables.should == ['@@a']
    Test2Module2.class_variables.should == []
    x.m2_b = 'testM2'
    Test2Class1.class_variables.should == ['@@c', '@@a']
    Test2Module1.class_variables.should == ['@@a']
    Test2Module2.class_variables.should == ['@@b']
    x.c2_vars.should == [12.1, 34.3, 56.5, 78.8]
    Test2Module1.class_var_get('@@a').should == 'testM1'
    Test2Module2.class_var_get('@@b').should == 'testM2'
    Test2Class1.class_var_get('@@a').should == 'testM1'
    Test2Class1.class_var_get('@@c').should == 'testC1'
  end
  
  it "variables set on mixins or the super class can't be set only to the derived class" do
    module Test3Module1
      def m1_a= x; @@a = x; end
      def m1_a; @@a; end
    end
    module Test3Module2
      def m2_b= x; @@b = x; end
      def m2_b; @@b; end
    end
    class Test3Class1
      include Test3Module1
      def c1_c= x; @@c = x; end
      def c1_c; @@c; end
    end
    class Test3Class2 < Test3Class1
      include Test3Module2
      def c2_a= x; @@a = x; end
      def c2_b= x; @@b = x; end
      def c2_c= x; @@c = x; end
      def c2_d= x; @@d = x; end
      def c2_vars; [@@a, @@b, @@c, @@d]; end
      def c2_vars_a_b; [@@a, @@b]; end                 
    end
    
    Test3Class2.class_variables.should == []
    x = Test3Class2.new
    x.m1_a = 123
    x.m2_b = 456
    x.c2_vars_a_b.should == [123, 456]
    x.c1_c = 789
    x.c2_a = 'aaa'
    x.c2_vars_a_b.should == ['aaa', 456]
    x.c2_b = 'bbb'
    x.c2_c = 'ccc'
    x.c2_d = 'ddd'
    Test3Class2.class_variables.sort.should == ["@@a", "@@b", "@@c", "@@d"]
    Test3Class1.class_variables.should == ["@@c", "@@a"]
    Test3Module1.class_variables.should == ["@@a"]
    Test3Module2.class_variables.should == ["@@b"]
    x.c2_vars.should == ['aaa', 'bbb', 'ccc', 'ddd']
    [x.m1_a, x.m2_b, x.c1_c].should == ['aaa', 'bbb', 'ccc']
  end
  
  it 'class variables with same name' do
    module Test4Module1
        def m1_a= x; @@a = x; end
        def m1_a; @@a; end
    end
    module Test4Module2
        def m2_a= x; @@a = x; end
        def m2_a; @@a; end
    end
    class Test4Class1
        include Test4Module1
        def c1_a= x; @@a = x; end
        def c1_a; @@a; end
    end
    class Test4Class2 < Test4Class1
        include Test4Module2
        def c2_a= x; @@a = x; end
        def c2_a; @@a; end
    end
    
    x = Test4Class2.new
    
    x.c1_a = 456
    should_raise(NameError) { x.m1_a }
    should_raise(NameError) { x.m2_a }
    x.c1_a.should == 456
    x.c2_a.should == 456
        
    x.m1_a = 123
    x.m1_a.should == 123
    should_raise(NameError) { x.m2_a }
    x.c1_a.should == 456
    x.c2_a.should == 456
    
    x.c2_a = 789
    x.m1_a.should == 123
    should_raise(NameError) { x.m2_a }
    x.c1_a.should == 789
    x.c2_a.should == 789
    
    x.m2_a = 210
    x.m1_a.should == 123
    x.m2_a.should == 210
    x.c1_a.should == 789
    x.c2_a.should == 210    
    
    Test4Class2.class_var_get("@@a").should == 210
    Test4Class1.class_var_get("@@a").should == 789
    Test4Module2.class_var_get("@@a").should == 210
    Test4Module1.class_var_get("@@a").should == 123
  end 
end

describe "class variables are lexically bound to the surrounding class/module" do
  it "class variables in nested modules get the correct scope" do    
    module Foo
      @@bob = 123
      
      def self.foo
        @@bob
      end
      
      module Bar
        module Baz
          def self.baz
            @@bob = 555
          end
        end
        
        @@bob = 456
        def self.bar
          @@bob
        end
      end
      
      class Bob
        @@bob = 789
        
        def self.bob
          @@bob
        end
      end      

      module Zoo
          def self.zoo
              @@bob
          end 
      end 
    end

    Foo.foo.should == 123
    Foo::Bar.bar.should == 456
    Foo::Bob.bob.should == 789
    Foo::Bar::Baz.baz.should == 555
    
    should_raise(NameError, "uninitialized class variable @@bob in Foo::Zoo") { Foo::Zoo.zoo }
  end
  
  it "blocks don't interfere with scoping" do
    class Module
      def call_test_block
        yield
      end
    end
    
    module Foo
      call_test_block do
        @@abc = 'test1'
      end
      def self.get_abc; @@abc; end
      
      call_test_block do
        module Bar
          def self.get_def; @@def; end
          @@def = 'test2'
        end        
      end      
    end
    
    Foo.get_abc.should == 'test1'    
    Foo::Bar::get_def.should == 'test2'
  end
  
  it "module level get/set methods set variables on Object (this test needs to be last)" do
    # test module level get/set methods
    def set_class_var x; @@osv = x; end
    def get_class_var; @@osv; end

    class Using_module_get_set_functions
      def check_sv; get_class_var; end
      def set_sv x; set_class_var x; end
    end
    
    x = Using_module_get_set_functions.new
    should_raise(NameError) { x.check_sv }
    x.set_sv(777).should == 777
    x.check_sv.should == 777
    x.class.class_variables.should == ['@@osv']
    Object.class_variables.should == ['@@osv']
    Object.class_var_get(:@@osv).should == 777
    x.class.class_var_get('@@osv').should == 777
    Array.class_var_get('@@osv').should == 777
    Comparable.class_variables.should == []
  end
end

finished
