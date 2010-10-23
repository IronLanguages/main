/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronRuby.Tests {
    public partial class Tests {
        public void Super1() {
            TestOutputWithEval(@"
class C
  def foo a
    puts 'C.foo'
    puts a
  end
end

class D < C
  def foo
    puts 'D.foo'
    #<super('arg')#>  
  end
end

D.new.foo
", @"
D.foo
C.foo
arg"
            );
        }

        public void SuperParameterless1() {
            TestOutputWithEval(@"
class C
  def foo *a
    puts 'C.foo'
    p a
  end
end

class D < C
  def foo a, b=1, c=2, d=3, e=4, f=5, g=6, h=7, i=8, j=9, k=a, *rest, &blck
    puts 'D.foo'
    p rest
    y = 1
    z = 2    
    #<super#>
  end
end

D.new.foo 0
", @"
D.foo
[]
C.foo
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0]
"
            );
        }

        public void SuperParameterless2A() {
            TestOutputWithEval(@"
class C
  def foo a
    puts 'C.foo'
    puts a
    yield
  end
end

class D < C
  def foo a
    puts 'D.foo'
    #<super { puts 'block' }#>
  end
end

D.new.foo 'arg'
", @"
D.foo
C.foo
arg
block
"
            );
        }

        /// <summary>
        /// Super passes the most recent values of the parameter variables but keeps the original block value.
        /// This is consistent with yield.
        /// </summary>
        public void SuperParameterless2B() {
            TestOutputWithEval(@"
class C
  def foo *a
    p a
    yield
  end
end

class D < C
  def foo a, &b
    a = 2
    b = lambda { puts '2' }    
    1.times { #<super#> }
  end
end

D.new.foo(1) { puts '1' }
", @"
[2]
1
"
            );
        }

        public void SuperParameterless3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    puts 'C.foo'
  end
end

class D < C
  def foo a
    super()
  end
end

D.new.foo 'arg'
");
            }, @"
C.foo
");
        }

        /// <summary>
        /// Calls to super with block and no arguments (was bug in parser/AST).
        /// </summary>
        public void Super2() {
            AssertOutput(delegate() {
                CompilerTest(@"
$p = proc {}

class C
  def foo *a
    p a, block_given?
  end
end

class D < C
  def foo a 
    super                     # pass 'a'
    super()    
    super &$p                 
    super(&$p)
    super { }                 # pass 'a'
    super() { }
  end
end

D.new.foo(1)
puts
D.new.foo(2) { }
", 1, 0);
            }, @"
[1]
false
[]
false
[]
true
[]
true
[1]
true
[]
true

[2]
true
[]
true
[]
true
[]
true
[2]
true
[]
true
");
        }

        public void SuperToAttribute1() {
            AssertOutput(() => CompilerTest(@"
class C
  attr_accessor :foo
end

class D < C
  def foo
    super
  end
  
  def foo= v
    super
  end
end

d = D.new
p d.foo = 123
p d.foo
p d.foo {}
p d.foo(1) rescue p $!
"), @"
123
123
123
#<ArgumentError: wrong number of arguments (1 for 0)>
");
        }

        /// <summary>
        /// Super calls method_missing.
        /// </summary>
        public void SuperAndMethodMissing1() {
            AssertOutput(() => CompilerTest(@"
class C
  def method_missing name
    puts ""mm(C): #{name}""
  end
end

class D < C
  def method_missing name
    puts ""mm(D): #{name}""
  end
  
  def bar
    puts 'D: bar'    
    super                                  # calls D#method_missing!
  end
end

D.new.bar
"), @"
D: bar
mm(D): bar
");
        }

        public void SuperAndMethodMissing2() {
            AssertOutput(() => CompilerTest(@"
class D
  def bar
    super                                
  end
end

D.new.bar rescue p $!
"), @"
#<NoMethodError: super: no superclass method `bar'>
");
        }

        public void SuperCaching1() {
            AssertOutput(() => CompilerTest(@"
module M
  def bar(x)
    puts 'M'
  end
end
class C
  def bar(x)
    puts 'C'
    super(x)
  end
end
C.new.bar(1) rescue puts 'error'
class C
  include M
end
C.new.bar(1)
"), @"
C
error
C
M
");
        }

        public void SuperInDefineMethod1() {
            SuperInDefineMethod1_Test(false);
        }

        public void SuperInDefineMethodEval1() {
            SuperInDefineMethod1_Test(true);
        }

        /// <summary>
        /// Super in a proc invoked via a call to a method defined by define_method uses 
        /// the method's name and declaring module for super-method lookup.
        /// </summary>
        public void SuperInDefineMethod1_Test(bool eval) {
            AssertOutput(delegate() {
                CompilerTest(Eval(eval, @"
def def_lambda
  1.times {
    $p = lambda { 
       1.times { 
         p self.class
         #<super()#>
       }
    }
  }
end

class C
  def foo
    puts 'C.foo'
  end
end

def_lambda

class D < C
  define_method :foo, &$p
end

D.new.foo
"));
            }, @"
D
C.foo
");
        }

        /// <summary>
        /// Super in a proc invoked via "call" uses the self and parameters captured in closure by the block.
        /// </summary>
        public void SuperInBlocks1() {
            TestOutput(@"
class A 
  def def_lambda a
    puts 'A.def_lambda'
    puts a
  end
end

class B < A
  def def_lambda a
    1.times {
      $p = lambda { |x|
        1.times { 
          p self.class
          super
        }
      }
    }
  end
end

B.new.def_lambda 'arg'
$p.call 'foo'
", @"
B
A.def_lambda
arg
");
        }

        /// <summary>
        /// define_method returns a lambda that behaves like a method-block when called.
        /// This behavior is not very well defined in CRuby (see http://redmine.ruby-lang.org/issues/show/2419).
        /// </summary>
        public void SuperInDefineMethod2() {
            TestOutput(@"
class B
  def m
    puts self.class.to_s + '::m'    
  end
end

class C < B
  q = nil

  C.new.instance_eval do   # we need to close the block over self that is an instance of C
    q = Proc.new do
      super()
    end
  end

  mq = define_method :m, &q
  puts mq.object_id == q.object_id  
  
  q.call rescue p $!
  mq.call
end
", @"
false
#<NoMethodError: super called outside of method>
C::m
");
        }

        /// <summary>
        /// Caching - a single super call site can be used for invocation of two different methods,
        /// if it is defined in a block that is used in define_method.
        /// </summary>
        public void SuperInDefineMethod3() {
            TestOutput(@"
class B
  def foo
    puts 'B::foo'
  end

  def bar
    puts 'B::bar'
  end
end

class C < B
  def foo
    1.times { $p = Proc.new { 1.times { 1.times { super() } } } }
    1.times(&$p)
  end
end

c = C.new
c.foo

class C
  define_method(:bar, &$p)
end

c.bar
", @"
B::foo
B::bar
");
        }

        /// <summary>
        /// Caching - The same as SuperInDefineMethod4 but with implicit arguments at super call site. 
        /// This should throw an exception as in Ruby 1.9.
        /// </summary>
        public void SuperInDefineMethod4() {
            TestOutput(@"
class B
  def foo
    puts 'B::foo'
  end

  def bar
    puts 'B::bar'
  end
end

class C < B
  def foo
    1.times { $p = Proc.new { 1.times { 1.times { super } } } }
    1.times(&$p)
  end
end

c = C.new
c.foo

class C
  define_method(:bar, &$p)
end

c.bar rescue p $!
c.foo
", @"
B::foo
#<RuntimeError: implicit argument passing of super from method defined by define_method() is not supported. Specify all arguments explicitly.>
B::foo
");
        }

        /// <summary>
        /// Caching - the same super call site can be used to invoke different method names.
        /// </summary>
        public void SuperInDefineMethod5() {
            TestOutput(@"
class C
  def a; 'a'; end
  def b; 'b'; end
end

class D < C
  $p = Proc.new do
    super()
  end

  [:a, :b].each do |name|
    define_method(name, &$p)
  end
end

puts D.new.a, D.new.b, D.new.a
", @"
a
b
a
");
        }

        public void SuperInTopLevelCode1() {
            TestOutput(@"
class B
  def m
    puts 'B::m'
  end
end

1.times do
  $p = Proc.new do
    1.times do
      super()
    end
  end
end

$p.call rescue p $!

class C < B
  define_method(:m, &$p)
end

C.new.m
", @"
#<NoMethodError: super called outside of method>
B::m
");
        }

        /// <summary>
        /// Alias doesn't change DeclaringModule of the method => super call uses the class in which the method is defined.
        /// </summary>
        public void SuperInAliasedDefinedMethod1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  def m
    puts 'B::m'
  end
end

class C < B
  define_method :m do
    puts 'C::m'
    super()
  end
  
  def n
    puts 'C::n'
  end
end

class D < C
  alias n m
end

D.new.n
");
            }, @"
C::m
B::m
");
        }

        /// <summary>
        /// super doesn't use self, declaring-module defined by module_eval/instance_eval.
        /// </summary>
        public void SuperInModuleEval1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A
  def foo
    puts 'A::foo'
  end
end

class B < A
  def foo
    puts 'B::foo'
  end
end

class C < B
  def foo
    A.module_eval {
      super
    }
    A.new.instance_eval {
      super
    }
  end
end 

C.new.foo
");
            }, @"
B::foo
B::foo
");
        }

#if OBSOLETE
        /// <summary>
        /// MRI 1.8: The self object captured by Kernel#binding is the receiver of the binding call.
        /// Thus the self used in eval might be different from the one the scope holds on.
        /// Super call uses the scope's self object.
        /// </summary>
        public void SuperCallInEvalWithBinding18() {
            TestOutput(@"
module Kernel
  public :binding
end

class A
  def m
    puts 'A::m'
  end
end

class D < A
  def m
    puts 'D::m'
    p self.class
    
    cb = C.new.binding             # binding captures C.new as self
    eval('
        p self.class               # self is different from RubyScope.SelfObject here
        super                      # super picks up RubyScope.SelfObject
    ', cb)
  end
end

class C
end

D.new.m", @"
D::m
D
C
A::m
");
        }
#endif

        /// <summary>
        /// MRI 1.9: The self object captured by Kernel#binding is the one captured by the scope, not the receiver of the binding call.
        /// </summary>
        public void SuperCallInEvalWithBinding19() {
            TestOutput(@"
class C
  def foo
    123
  end
end

b = C.new.send(:binding)
p self
eval('
  p self
  p self.foo rescue p $!
', b)
", @"
main
main
#<NoMethodError: undefined method `foo' for main:Object>
");
        }
    }

}
