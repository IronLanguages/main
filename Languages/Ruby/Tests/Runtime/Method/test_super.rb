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

class Base1
  def foo    
    "Base1#foo()"
  end
  def goo(x)
    "Base1#goo(#{x.inspect})"
  end
  def hoo(x,y=1,*z)
    "Base1#hoo(#{x.inspect}, #{y.inspect}, *#{z.inspect})"
  end
  def woo(x)
    "Base1#woo(#{x.inspect}), proc yielded: #{yield}"
  end
  def yoo(&b)
    "Base1#yoo(), proc returned: #{b.call(123)}"
  end
  def zoo(x,y=1,*z,&b)
    "Base1#zoo(#{x.inspect}, #{y.inspect}, *#{z.inspect}), proc returned: #{b.call(x, y)}"
  end
end

class Test1 < Base1
  def foo
    "<" + super() + ">"
  end
  def goo(x)
    "<" + super(x) + ">"
  end
  def hoo(*z)
    "<" + super(*z) + ">"
  end
  def woo(x)
    "<" + super(x) { yield } + ">"
  end
  def yoo(&b)
    "<" + super(&b) + ">"
  end
  def zoo(x,y=1,*z,&b)
    "<" + super(x,y,*z,&b) + ">"
  end
end

t = Test1.new

describe "super calls with explict args" do
  it "calling method with no args" do
    t.foo.should == "<Base1#foo()>"
  end
  
  it "calling method with one arg" do
    t.goo(42).should == "<Base1#goo(42)>"
  end
  
  it "calling method with optional & splatted args" do
    t.hoo(42).should == "<Base1#hoo(42, 1, *[])>"
    t.hoo(22, 33).should == "<Base1#hoo(22, 33, *[])>"
    t.hoo(77, 88, 1, 2, 3).should == "<Base1#hoo(77, 88, *[1, 2, 3])>"
  end
  
  it "calling method with block" do
    t.yoo { "the question" }.should == "<Base1#yoo(), proc returned: the question>"
    t.woo(42) { "the answer" }.should == "<Base1#woo(42), proc yielded: the answer>"    
  end
  
  it "calling method with all kinds of arguments" do
    t.zoo(42) { |x,y| x*y+1 }.should == "<Base1#zoo(42, 1, *[]), proc returned: 43>"
    t.zoo(22, 33) { |x,y| x*y+2 }.should == "<Base1#zoo(22, 33, *[]), proc returned: 728>"
    t.zoo(77, 88, 1, 2, 3) { |x,y| x*y+3 }.should == "<Base1#zoo(77, 88, *[1, 2, 3]), proc returned: 6779>"
  end
end


class Test2 < Base1
  def foo
    "<" + super + ">"
  end
  def goo(x)
    "<" + super + ">"
  end
  def hoo(x,y=1,*z)
    "<" + super + ">"
  end
  def woo(x)
    "<" + super + ">"
  end
  def yoo(&b)
    "<" + super + ">"
  end
  def zoo(x,y=1,*z,&b)
    "<" + super + ">"
  end
end

t = Test2.new

describe "super calls with implict args" do
  it "calling method with no args" do
    t.foo.should == "<Base1#foo()>"
  end
  
  it "calling method with one arg" do
    t.goo(42).should == "<Base1#goo(42)>"
  end
  
  it "calling method with optional & splatted args" do
    t.hoo(42).should == "<Base1#hoo(42, 1, *[])>"
    t.hoo(22, 33).should == "<Base1#hoo(22, 33, *[])>"
    t.hoo(77, 88, 1, 2, 3).should == "<Base1#hoo(77, 88, *[1, 2, 3])>"
  end
  
  it "calling method with block" do
    t.yoo { "the question" }.should == "<Base1#yoo(), proc returned: the question>"
    t.woo(42) { "the answer" }.should == "<Base1#woo(42), proc yielded: the answer>"    
  end
  
  it "calling method with all kinds of arguments" do
    t.zoo(42) { |x,y| x*y+1 }.should == "<Base1#zoo(42, 1, *[]), proc returned: 43>"
    t.zoo(22, 33) { |x,y| x*y+2 }.should == "<Base1#zoo(22, 33, *[]), proc returned: 728>"
    t.zoo(77, 88, 1, 2, 3) { |x,y| x*y+3 }.should == "<Base1#zoo(77, 88, *[1, 2, 3]), proc returned: 6779>"
  end
end

class Base2
  def self.foo(x,y)
    x + y
  end
  
  def self.goo(x,y)
    x * y
  end
end

class Base3 < Base2
  def Base3.foo(x,y)
    super(x,y) + 123
  end
  def self.goo(x,y)
    super + 456
  end
end

class Test3 < Base3
  def Test3.foo(x,y)
    "foo: #{super(x,y)}"
  end
  def self.goo(x,y)
    "goo: #{super}"
  end
end

describe "super calls on class/singleton methods" do
  it "super works from singleton methods" do
    x = "abc"
    def x.to_s; super + "def"; end
    x.to_s.should == "abcdef"
  end
  
  it "super works in class methods" do
    Test3.foo(4,5).should == "foo: 132"  # 123 + (4+5)
    Test3.goo(4,5).should == "goo: 476"  # 456 + (4*5)
  end
end

class TestInit1
  def initialize(y)
    @y = y
  end
  
  def gety; @y; end
end

class TestInit2 < TestInit1
  def initialize(x,y)
    @x = x
    super(y)
  end

  def getx; @x; end
end

class TestInit3 < TestInit2
  def initialize(x,y)
    super
    @z = @x + @y
  end
  
  def getz; @z; end
end

describe "super calls from initialize" do
  it "super in initialize calls parent method" do
    t2 = TestInit2.new(123, 456)
    t2.getx.should == 123
    t2.gety.should == 456
    
    t3 = TestInit3.new(22,44)
    [t3.getx, t3.gety].should == [22, 44]
    t3.getz.should == 66
  end
end

module M0
  def foo
    "<M0>foo</M0>"
  end
end
module M1
  def foo
    "<M1>" + super + "</M1>"
  end
end
module M2
  def foo
    "<M2>" + super + "</M2>"
  end
end
module M3
  def foo
    "<M3>" + super + "</M3>"
  end
end
module M4
  def foo
    "<M4>" + super + "</M4>"
  end
end
module M5
  def foo
    "<M5>" + super + "</M5>"
  end
end

class C1
  include M1
  include M0
  def foo
    "<C1>" + super + "</C1>"
  end
end
class C2 < C1
  include M3
  include M2
  def foo
    "<C2>" + super + "</C2>"
  end
end
class C3 < C2
  include M5
  include M4
  def foo
    "<C3>" + super + "</C3>"
  end
end

describe "super skips one in the method resolution order (MRO)" do
  it "class method will call mixin methods" do
    module Mixin1
      def bar(x)
        "Mixin1#bar(#{x})"
      end
    end
    class Class1
      def bar(x)
        "Class1#bar, " + super(x)
      end
    end
    should_raise(NoMethodError) { Class1.new.bar(222) }
    class Class1
      include Mixin1
    end
    Class1.new.bar(555).should == "Class1#bar, Mixin1#bar(555)" 
  end
  
  it "mixin methods call each other" do
    module Mixin2
      def bar(x)
        "Mixin2#bar, " + super(x)
      end
    end
    class Class2a
      include Mixin1
      include Mixin2
    end
    
    Class2a.new.bar(888).should == "Mixin2#bar, Mixin1#bar(888)"
    
    # order is reversed if they are added on the same line
    class Class2b
      include Mixin1, Mixin2
    end
    
    Class2b.new.bar(888).should == "Mixin1#bar(888)"
  end
  
  it "mixin method can call base class method" do
    class Class3
      def bar(x)
        "Class3#bar(#{x})"
      end      
    end
    class Class4 < Class3
      include Mixin2
    end
    Class4.new.bar(111).should == "Mixin2#bar, Class3#bar(111)"
  end
  
  it "super calls can be chained and follow the MRO" do
    C3.new.foo.should == "<C3><M4><M5><C2><M2><M3><C1><M0>foo</M0></C1></M3></M2></C2></M5></M4></C3>"
  end  
end

describe "super throws an error when no super method is available" do
  it "super from top level code is an error" do
    should_raise(NoMethodError, "super called outside of method") { super }
  end
  
  it "super with no super method is an error" do
    class Class5
      def zoo
        super
      end
    end
    
    should_raise(NoMethodError, "super: no superclass method `zoo'") { Class5.new.zoo }
    
    x = Object.new
    def x.zoo
      super(1,2,3)
    end
    
    should_raise(NoMethodError, "super: no superclass method `zoo'") { x.zoo }
  end
  
  it "raise an error if super method has been undefined" do
    class Class6
      def yoo; "yoo"; end
    end    
    class Class7 < Class6
      def yoo; super; end
    end
    x = Class7.new
    x.yoo.should == "yoo"
    Class6.send(:undef_method, :yoo)
    should_raise(NoMethodError, "super: no superclass method `yoo'") { x.yoo }
  end
end

finished

