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

namespace IronRuby.Tests {
    public partial class Tests {
        private void Super1_Test(bool eval) {
            AssertOutput(delegate() {
                CompilerTest(Eval(eval, @"
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
"));
            }, @"
D.foo
C.foo
arg");
        }

        public void Super1() {
            Super1_Test(false);
        }

        public void SuperEval1() {
            Super1_Test(true);
        }

        public void SuperParameterless1() {
            SuperParameterless1_Test(false);
        }

        public void SuperParameterlessEval1() {
            SuperParameterless1_Test(true);
        }

        public void SuperParameterless1_Test(bool eval) {
            AssertOutput(delegate() {
                CompilerTest(Eval(eval, @"
class C
  def foo a
    puts 'C.foo'
    puts a
  end
end

class D < C
  def foo a
    puts 'D.foo'
    #<super#>
  end
end

D.new.foo 'arg'
"));
            }, @"
D.foo
C.foo
arg");
        }

        public void SuperParameterless2() {
            SuperParameterless2_Test(false);
        }

        public void SuperParameterlessEval2() {
            SuperParameterless2_Test(true);
        }

        public void SuperParameterless2_Test(bool eval) {
            AssertOutput(delegate() {
                CompilerTest(Eval(eval, @"
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
    #<super#> { puts 'block' }
  end
end

D.new.foo 'arg'
"));
            }, @"
D.foo
C.foo
arg
block
");
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

        // TODO: parameters
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
         #<super#>
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
        public void SuperInDefineMethod2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A 
  def def_lambda a
    puts 'A.def_lambda'
    puts a
  end
end

class B < A
  def def_lambda a
    1.times {
      $p = lambda { 
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
");
            }, @"
B
A.def_lambda
arg
");
        }

        /// <summary>
        /// Super call in defined_method's proc: 
        /// 1.8: parameters are taken from block parameters
        /// 1.9: not-supported exception is thrown
        /// </summary>
        public void SuperInDefineMethod3() {
            // TODO:
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  def m *a
    p a
  end
end

class C < B
  define_method :m do |*a|
    p a
    super
  end
end

C.new.m 1,2
");
            }, @"
[1, 2]
[1, 2]
");
        }

        public void SuperInTopLevelCode1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  def m
    puts 'B::m'
  end
end

class C < B
  define_method :m do
    super
  end
end

C.new.m
");
            }, @"
B::m
");
        }

        /// <summary>
        /// Alias doesn't change DeclaringModule of the method => super call uses the class in which the method is defined.
        /// Parameters
        /// </summary>
        public void SuperInAliasedDefinedMethod1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  def m *a
    puts 'B::m'
  end
end

class C < B
  define_method :m do
    puts 'C::m'
    super
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
    }

}
