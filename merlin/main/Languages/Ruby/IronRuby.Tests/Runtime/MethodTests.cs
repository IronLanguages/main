/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

namespace IronRuby.Tests {
    public partial class Tests {
        public void Scenario_RubySimpleCall1() {
            AssertOutput(delegate {
                CompilerTest(@"
puts nil
");
            }, "nil");

        }

        public void Scenario_RubySimpleCall2() {
            AssertExceptionThrown<ArgumentException>(delegate {
                CompilerTest(@"
def foo a,c=1,*e
end

foo

");
            });

        }

        public void Scenario_RubySimpleCall3() {
            AssertOutput(delegate {
                CompilerTest(@"
y = nil
puts y

x = 123
puts x
");
            }, @"
nil
123");

        }

        /// <summary>
        /// LambdaExpression gets converted to a wrapper.
        /// </summary>
        public void Scenario_RubySimpleCall4() {
            AssertOutput(delegate {
                CompilerTest(@"
def foo a,b,c,d,e,f,g,h,i,j
  puts 123
end
foo 1,2,3,4,5,6,7,8,9,10
");
            }, @"123");

        }

        public void Scenario_RubySimpleCall5() {
            AssertOutput(delegate {
                Engine.CreateScriptSourceFromString(@"
class A
end

class B < A
end

B.new.foo rescue 0

class A
  def foo
    puts 'foo'
  end
end

B.new.foo
").ExecuteProgram();
            }, @"foo");
        }
        
        public void Send1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  def foo *a
    puts ""C::foo *#{a.inspect}, &#{block_given?}""
  end
  
  alias []= :send
end

x = C.new
q = lambda {}

x.send :foo
x.send :foo, &q
x.send :foo, &nil
x.send :foo, 1
x.send :foo, 1, &q
x.send :foo, 1, &nil
x.send :foo, 1, 2
x.send :foo, 1, 2, &q
x.send :foo, 1, 2, &nil
x.send :foo, 1, 2, 3
x.send :foo, 1, 2, 3, &q
x.send :foo, 1, 2, 3, &nil

x.send *[:foo, 1,2,3]
x.send :foo, 1, *[2,3], &q
x[:foo,*[1,2]] = 3
x[*:foo] = 1
x[] = :foo
", 1, 0);
            }, @"
C::foo *[], &false
C::foo *[], &true
C::foo *[], &false
C::foo *[1], &false
C::foo *[1], &true
C::foo *[1], &false
C::foo *[1, 2], &false
C::foo *[1, 2], &true
C::foo *[1, 2], &false
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &true
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &true
C::foo *[1, 2, 3], &false
C::foo *[1], &false
C::foo *[], &false
");
        }

        /// <summary>
        /// Send propagates the current scope.
        /// </summary>
        public void Send2() {
             AssertOutput(delegate {
                CompilerTest(@"
class C
  public
  send :private
  
  def foo
  end   
  
  p C.private_instance_methods(false)
end
");
             }, @"
[""foo""]
");
        }

        public void AttributeAccessors1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  attr_accessor :foo
  alias :bar :foo
  alias :bar= :foo=
end

x = C.new
x.foo = 123
x.bar = x.foo + 1
puts x.foo, x.bar
");
            }, @"
124
124
");
        }

        public void AttributeAccessors2() {
            AssertOutput(() => CompilerTest(@"
class C
  attr_accessor :foo
end

x = C.new
p x.send('foo=', 123)
p x.send('foo')

"), @"
123
123
");
        }

        public void MethodAdded1() {
            AssertOutput(delegate {
                CompilerTest(@"
class Module
  def method_added name    
    puts name
  end
end
");
            }, @"
method_added
");
        }
        
        public void VisibilityCaching1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  def foo
    puts 'foo'
  end

  def method_missing name
    puts 'mm'
  end 
end

x = C.new
4.times do |$i|
  class C
    case $i
      when 0: private :foo
      when 1: public :foo
      when 2: private :foo
    end
  end
  x.foo  
end
");
            }, @"
mm
foo
mm
mm
");            
        }

        public void ModuleFunctionVisibility1() {
            AssertOutput(delegate {
                CompilerTest(@"
module M
  private
  def f
  end
  
  module_function :f
end

p M.singleton_methods(false)
p M.private_instance_methods(false)
p M.public_instance_methods(false)
");
            }, @"
[""f""]
[""f""]
[]
");            
        }
    }
}
