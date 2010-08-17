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

using System;
namespace IronRuby.Tests {
    public partial class Tests {
        public void Eval1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo()
    a = 1
    eval('puts a')
end

foo
");
            }, "1");
        }

        public void Eval2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def b
  'undefined'
end

def goo
  a = 1
  eval('a = 2; b = 3; puts a,b')
  puts a,b
end

goo
");
            }, @"
2
3
2
undefined");
        }

        public void Eval3() {
            string body = @"
1.times {
  a = 1
  
  eval <<-END
    b = a + 1
    eval <<-END2
      c = b + 1
    END2
  END
  
  puts b rescue p $!
  
  eval <<-END
    puts b, c
  END
}

puts a rescue p $!
puts b rescue p $!
  
eval <<-END
  puts a rescue p $!
  puts b rescue p $!
END
";
            string output = @"
#<NoMethodError: * `b' *>
2
3
#<NoMethodError: * `a' *>
#<NoMethodError: * `b' *>
#<NoMethodError: * `a' *>
#<NoMethodError: * `b' *>
";

            AssertOutput(() => CompilerTest(String.Format(@"def foo; {0}; end; foo", body)), output, OutputFlags.Match);
            AssertOutput(() => CompilerTest(String.Format(@"module M; {0}; end", body)), output, OutputFlags.Match);
            AssertOutput(() => CompilerTest(String.Format(@"class C; {0}; end", body)), output, OutputFlags.Match);
            AssertOutput(() => CompilerTest(String.Format(@"class << Object.new; {0}; end", body)), output, OutputFlags.Match);
            AssertOutput(() => CompilerTest(body), output, OutputFlags.Match);
        }

        /// <summary>
        /// Assigning to a variable defined in an outer scope shouldn't define a new variable in the currenct scope.
        /// </summary>
        public void Eval4() {
            AssertOutput(delegate() {
                CompilerTest(@"
1.times {
  x = nil
  1.times {
    eval('1.times { x = 2 }')
  } 
  
  module M
    eval('x = 3')
  end

  puts x
}
");
            }, "2");
        }

        public void Eval5() {
            TestOutput(@"
class C
end

z = 1
instance_eval 'x = z + 1'
C.class_eval 'y = x + 1; z = 4'

eval('p x,y,z')", @"
2
3
4
");
        }

        public void Eval6() {
            TestOutput(@"
$b = []
a = 1
1.times do
  b = 2
  eval <<-end1
    c = 3
    1.times do
      d = 4
      instance_eval <<-end3
        e = 5 
        $b[4] = binding
      end3
      $b[3] = binding
    end
    $b[2] = binding  
  end1
  $b[1] = binding  
end
$b[0] = binding 

$b.each do |bin|
  eval <<-END, bin
    p [ begin; a; rescue; end,
        begin; b; rescue; end,
        begin; c; rescue; end,
        begin; d; rescue; end,
        begin; e; rescue; end,
      ]
  END
end", @"
[1, nil, nil, nil, nil]
[1, 2, 3, nil, nil]
[1, 2, 3, nil, nil]
[1, 2, 3, 4, 5]
[1, 2, 3, 4, 5]
");
        }

        public void LocalNames1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def f
  a = 1
  puts local_variables.sort
  b = 2
end

f
");
            }, @"
a
b");
        }

        public void LocalNames2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def bar
  1.times { |x|
     y = 1
     1.times { |z|
       $b = binding
     }
  }
end

bar

eval('puts local_variables.sort', $b)
");
            }, @"
x
y
z
");
        }

        // module scope creates a local dictionary that can be captured by a binding:
        public void LocalNames3() {
            AssertOutput(delegate() {
                CompilerTest(@"
$p = [] 
$i = 0
while $i < 2    
  module M
    x = $i
    $p << binding
  end 
  $i += 1
end  
    
eval('puts x', $p[0])
eval('puts x', $p[1])
");
            }, @"
0
1
");
        }

        // module scope stops local variable lookup
        public void LocalNames4() {
            AssertOutput(delegate() {
                CompilerTest(@"
def bar
  1.times { |x|
     eval('
       module M
         y = 1
         1.times { |z|
           $b = binding
         }
       end
     ');
  }
end

bar

eval('puts local_variables.sort', $b)
");
            }, @"
y
z
");
        }

        public void LiftedParameters1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo a
  1.times {
    eval('puts a')
  }
end

foo 3
");
            }, @"3");
        }

        public void Binding1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  x = 1
  y = 2
  $b = binding
end

foo

p eval('x+y', $b)
");
            }, "3");
        }

        public void TopLevelBinding_RubyProgram() {
            AssertOutput(delegate() {
                Engine.CreateScriptSourceFromString(@"
def bar
  4
end

def baz
  p eval('a+bar', TOPLEVEL_BINDING)
  p eval('b', TOPLEVEL_BINDING)
end

a = 3
baz
b = 1
").ExecuteProgram();
            }, @"
7
nil");
        }

        public void EvalWithProcBinding1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def goo
  x = 1
  y = 2
  lambda { 
    z = -1
  }
end

def z
  1
end

p eval('x+y+z', goo)
");
            }, @"4");
        }

        public void ModuleEvalProc1() {
            TestOutput(@"
module M
end

puts M.module_eval { |*a| 
  p a, self 
  'result'
}
", @"
[]
M
result
");
        }

        /// <summary>
        /// Break from module_eval'd proc works.
        /// </summary>
        public void ModuleEvalProc2() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
end

puts M.module_eval {
  break 'result'
}
");
            }, @"
result
");
        }

        public void ModuleInstanceEvalProc3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  D = 1
end

C.module_eval {
  p Module.nesting
  puts D rescue puts 'error'
  
  def foo
    puts 'foo'
  end
}

C.instance_eval {
  p Module.nesting
  puts D rescue puts 'error'

  def bar
    puts 'bar'
  end
}

C.new.foo
C.bar
");
            }, @"
[]
error
[]
error
foo
bar
");
        }

        /// <summary>
        /// module_eval uses yield semantics for invoking the proc.
        /// (return in a yield to inactive lambda doesn't work).
        /// </summary>
        public void ModuleEvalProc3() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
end

$p = lambda {
  return 'ok'
}

puts $p.call
puts M.module_eval(&$p) rescue puts 'error'
");
            }, @"
ok
error
");
        }

        /// <summary>
        /// instance_eval creates an instance singleton and sets the scope of method definitions and aliases to it.
        /// </summary>
        public void InstanceEvalProc1() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = Object.new
x.instance_eval { 
  def foo
    puts 'foo'
  end
}
x.foo
");
            }, @"foo");
        }

        /// <summary>
        /// instance_eval sets public visibility flag on the evaluated block scope.
        /// </summary>
        public void InstanceEvalProc2() {
            TestOutput(@"
x = Object.new
x.instance_eval { 
  def foo
    puts 'foo'
  end
}
class << x
  puts instance_methods(false).include?(:foo)
end
", @"
true
");
        }

        /// <summary>
        /// module_eval is used in Class.new
        /// </summary>
        public void ModuleClassNew1() {
            AssertOutput(delegate() {
                CompilerTest(@"
C = Class.new { |*args|
  p args
  p self
  
  def foo
    puts 'hello'
  end
}

C.new.foo
");
            }, @"
[#<Class:*>]
#<Class:*>
hello
", OutputFlags.Match);
        }

        /// <summary>
        /// Class.new uses yield (as module_eval does). If the block doesn't break the return value is the class.
        /// </summary>
        public void ModuleClassNew2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  def foo
    puts 'bye'
  end
end

C = Class.new(B) {
  def foo
    puts 'hello'
  end

  break B
}

C.new.foo
");
            }, @"
bye
");
        }

        public void ModuleEvalString1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module N
  module M
  end
end

N::M.module_eval('p self, Module.nesting')
");
            }, @"
N::M
[N::M]
");
        }
        
        public void InstanceEvalString1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module N
  module M
    class << self
      C = 123
    end
  end
end

N::M.instance_eval('p self, C, Module.nesting')
");
            }, @"
N::M
123
[#<Class:N::M>]
");
        }

        public void ModuleEvalString2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
end

C.module_eval('def foo; puts 1; end')
C.new.foo
");
            }, @"1");
        }

        public void InstanceEvalString2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
end

C.instance_eval('def foo; puts 1; end')
C.foo
");
            }, @"1");
        }

        public void ModuleInstanceEvalString3() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
end

def a
  puts 'method'
end

def foo
  a = 1
  M.module_eval('puts a')
  M.instance_eval('puts a')
end

foo
");
            }, @"
1
1");
        }
    }
}
