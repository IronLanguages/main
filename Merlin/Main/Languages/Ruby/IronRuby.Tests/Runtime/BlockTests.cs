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
using IronRuby.Runtime;

namespace IronRuby.Tests {

    public partial class Tests {
        public void BlockEmpty() {
            CompilerTest("1.times { }");
        }

        public void RubyBlocks0() {
            AssertOutput(delegate() {
                CompilerTest(@"
3.times { |x| print x }
");
            }, "012");
        }

        public void RubyBlocks_Params1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def y; yield 0,1,2,3,4,5,6,7,8,9; end

y { |x0,x1,x2,x3,x4,x5,x6,x7,x8,x9| print x0,x1,x2,x3,x4,x5,x6,x7,x8,x9 }
");
            }, "0123456789");
        }

        public void RubyBlocks_Params2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def y; yield 0,1,2,3,4,5,6,7,8,9; end

y { |x0,x1,x2,x3,x4,x5,x6,x7,*z| print x0,x1,x2,x3,x4,x5,x6,x7,z[0],z[1]; }
");
            }, "0123456789");
        }

        public void ProcYieldCaching1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo 
  yield
end

foo { print 'A' }
foo { print 'B' }
foo { print 'C' }
");
            }, "ABC");
        }

        public void ProcCallCaching1() {
            AssertOutput(delegate() {
                CompilerTest(@"
$p = lambda { puts 1 }
$q = lambda { puts 2 } 

$p.call
$q.call
");
            }, @"
1
2
");
        }

        public void ProcSelf1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  1.times { p self }
end        
");
            }, @"M");
        }

        public void RubyBlocks2() {
            AssertExceptionThrown<MissingMethodException>(delegate() {
                CompilerTest(@"
3.times { |x| z = 1 }
puts z # undef z
");
            });
        }

        public void RubyBlocks3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    puts 'X',yield(1,2,3)
  end
end

C.new.foo { |a,b,c| puts a,b,c; 'foo' }
");
            }, @"
1
2
3
X
foo
");
        }

        public void RubyBlocks5() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo 
    puts block_given?
  end
end

C.new.foo { puts 'goo' }
C.new.foo
            ");
            }, @"
true
false
            ");
        }

        /// <summary>
        /// Return, yield and retry in a method.
        /// </summary>
        public void RubyBlocks6() {
            TestOutputWithEval(@"
def do_until(cond)
  if cond then #<return#> end
  #<yield#>
  #<retry#>
end

i = 0
do_until(i > 4) do
  puts i
  i = i + 1
end
", @"
0
1
2
3
4
"
            );
        }

        /// <summary>
        /// Break in a block.
        /// </summary>
        public void RubyBlocks7() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = 4.times { |x|
  puts x
  break 'foo'
}
puts x
");
            }, @"
0
foo
");
        }

        /// <summary>
        /// Redo in a block.
        /// </summary>
        public void RubyBlocks8() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
x = 2.times { |x|
  puts x
  i = i + 1
  if i < 3 then redo end
}
puts x
");
            }, @"
0
0
0
1
2
");
        }

        /// <summary>
        /// Next in a block.
        /// </summary>
        public void RubyBlocks9() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
x = 5.times { |x|
  puts x
  i = i + 1  
  if i < 3 then next end
  puts 'bar'
}
");
            }, @"
0
1
2
bar
3
bar
4
bar
");
        }

        /// <summary>
        /// Retry in a block.
        /// </summary>
        public void RubyBlocks10A() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
3.times { |x| 
  puts x
  i = i + 1
  if i == 2 then retry end
}
");
            }, @"
0
1
0
1
2
");
        }

        /// <summary>
        /// Retry in a block called via Proc#call.
        /// </summary>
        public void RubyBlocks10B() {
            TestOutput(@"
proc { retry rescue puts 'not caught here' }.call rescue p $!    # TODO: bug (should not be caught by the inner rescue
", @"
not caught here
");
        }

        /// <summary>
        /// Return with stack unwinding.
        /// </summary>
        public void RubyBlocks11() {
            TestOutputWithEval(@"
def foo
    puts 'begin'
    1.times {
        1.times {
            puts 'block'
            #<return 'result'#>
        }
    }
    puts 'end'
ensure
    puts 'ensure'
end

puts foo
",@"
begin
block
ensure
result
");
        }

        public void RubyBlocks12() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  yield 1,2,3
end

foo { |a,*b| puts a,'-',b }
");
            }, @"
1
-
2
3
");
        }

        public void RubyBlocks13() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  yield 1,2,3
end

foo { |a,b| puts a,b }
");
            }, @"
1
2");
        }

        /// <summary>
        /// Nested yielding.
        /// </summary>
        public void RubyBlocks14A() {
            TestOutputWithEval(@"
def bar
  yield
  yield  # shouldn't be called
end

def foo
  bar {
    print 'x'
    #<yield#>
  } 
end

foo { break }
", @"
x
");
        }

        /// <summary>
        /// Covers RubyOps.YieldBlockBreak.
        /// </summary>
        public void RubyBlocks14B() {
            TestOutput(@"
def proc_conv(&b)
  1.times { puts 'yielding'; yield }
end

def foo
  proc_conv { puts 'breaking'; break }
end

foo
", @"
yielding
breaking
");
        }

        /// <summary>
        /// Covers RubyOps.YieldBlockBreak error path.
        /// </summary>
        public void RubyBlocks14C() {
            TestOutputWithEval(@"
def proc_conv(&b)
  $x = b
end

def y
  1.times { #<yield#> rescue p $! }   # proc-converter is not active any more, hence error
end

def foo
  proc_conv { break }
  y(&$x)
end

foo
", @"
#<LocalJumpError: break from proc-closure>
");
        }

        /// <summary>
        /// Retry for-loop: for-loop should behave like x.each { } method call with a block, that is x is reevaluted on retry.
        /// </summary>
        public void RubyBlocks15() {
            TestOutput(@"
def foo x
  puts ""foo(#{x})""
  x * ($i + 1)
end

$i = 0

for i in [foo(1), foo(2), foo(3)] do
  puts ""i = #{i}""
  
  if $i == 0 then
    $i = 1
    retry
  end  
end
", @"
foo(1)
foo(2)
foo(3)
i = 1
foo(1)
foo(2)
foo(3)
i = 2
i = 4
i = 6
");
        }

        /// <summary>
        /// Tests optimization of block break from another block. 
        /// 
        /// Yield yields to a block that breaks to its proc-converter, which is foo.
        /// So the example should retrun 1 from the foo call.
        /// 
        /// Break is propagated thru yields in two ways:
        /// 1) returning ReturnReason == Break via BlockParam (fast path)
        /// 2) throwing MethodUnwinder exception (slow path)
        /// 
        /// ReturnReason should be propagated by yields as long as the owner of the block that contains the yield 
        /// is the target frame for the break. That's the case for for-loop blocks in this test.
        /// </summary>
        public void RubyBlocks16() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
    for j in [0]
        for i in [1]
            yield
        end 
        puts 'Unreachable'
    end
    puts 'Unreachable'
end 

x = foo do
    break 1
end
puts x
");
            }, @"1");
        }

        /// <summary>
        /// Retry is propagated to the 'each' call.
        /// </summary>
        public void RubyBlocks17() {
            TestOutputWithEval(@"
def foo
    for i in [1, 2, 3]
        puts ""i = #{i}""
        x = #<yield#>
    end 
    puts x
end 

def bar
    $c = 0
    foo do
		puts $c
        $c += 1
        retry if $c < 3
        'done'
    end 
end 

bar
", @"
i = 1
0
i = 1
1
i = 1
2
i = 2
3
i = 3
4
done");
        }

        public void RubyBlocks18() {
            TestOutput(@"
class C
  def y
    yield
  end 

  def foo
    y { p self.class }
  end
end

C.new.foo
", @"
C
");
        }

        /// <summary>
        /// Block return propagates thru a single library method with a block without throwing unwinding exception.
        /// </summary>
        public void BlockReturnOptimization1() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def foo
  10.times do
    return 123
  end
end

puts foo
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 0);

            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def foo
  x = proc do
    return 123
  end  
  eval('10.times(&x)')
end

puts foo
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 1);
        }

        /// <summary>
        /// Block return propagates thru multiple library method calls with a block without throwing unwinding exception.
        /// </summary>
        public void BlockReturnOptimization2() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def foo
  10.times do
    10.times do
      10.times do
        return 123
      end
    end
  end
end

puts foo
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 0);
        }

        /// <summary>
        /// Block return propagates thru user method calls with a block without throwing unwinding exception.
        /// </summary>
        public void BlockReturnOptimization3() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def f0
  $b = proc { return 123 }
  f1 {}
end

def f1
  f2 {} 
end

def f2 
  f3(&$b)
end

def f3
  yield
end

puts f0
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 0);
        }

        /// <summary>
        /// An unwinding exception is thrown if any frame is called w/o a block.
        /// </summary>
        public void BlockReturnOptimization4() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def f0
  $b = proc { return 123 }
  f1
end

def f1
  f2 {} 
end

def f2 
  f3(&$b)
end

def f3
  yield
end

puts f0
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 1);
        }

        /// <summary>
        /// Return propagates thru proc/lambda calls.
        /// </summary>
        public void BlockReturnOptimization5() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def foo
  l = lambda do
    yield 
  end
  l.call
  puts 'un'
end

def bar
  foo { return 123 }
end

p bar
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 1);
        }

        /// <summary>
        /// Return propagates thru proc/lambda calls.
        /// </summary>
        public void BlockReturnOptimization6() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def make_block(&b); b; end

def foo
  b = make_block { return 123 }
  l = lambda { b.call }
  l.call

  'unreachable'
end

puts foo
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 1);
        }

        /// <summary>
        /// Return propagates thru yield in a block.
        /// </summary>
        public void BlockReturnOptimization7() {
            StackUnwinder.InstanceCount = 0;
            TestOutput(@"
def f1
  f2 { return 123 } 
  puts 'unreachable'
end

def f2
  1.times { yield }
end

p f1
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 0);

            TestOutput(@"
def f1
  f2 { return 123 } 
  puts 'unreachable'
end

def f2
  1.times { eval('yield') }
end

p f1
", @"
123
");
            Assert(StackUnwinder.InstanceCount == 1);
        }

        public void Scenario_RubyBlockArgs1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def a; yield; end 
def b; yield 1; end 
def c; yield 1,2; end 
def d; yield []; end 
def e; yield [1]; end 
def f; yield [1,2]; end 
def g; yield *[]; end;

a { |x| puts x.inspect }
b { |x| puts x.inspect }
c { |x| puts x.inspect }
d { |x| puts x.inspect }
e { |x| puts x.inspect }
f { |x| puts x.inspect }
g { |(x,)| puts x.inspect }
", 1, 2); // 2 runtime warnings
            }, @"
nil
1
[1, 2]
[]
[1]
[1, 2]
nil
");
        }

        public void Scenario_RubyProcCallArgs1A() {
            AssertOutput(delegate() {
                CompilerTest(@"
lambda { |x| puts x.inspect }.call
lambda { |x| puts x.inspect }.call 1
lambda { |x| puts x.inspect }.call 1,2
lambda { |x| puts x.inspect }.call []
lambda { |x| puts x.inspect }.call [1]
lambda { |x| puts x.inspect }.call [1,2]
lambda { |x| puts x.inspect }.call *[1]
lambda { |(x,)| puts x.inspect }.call
lambda { |(x,)| puts x.inspect }.call 1,2,3,4 
lambda { |(x,y)| puts x.inspect }.call rescue puts 'error'
", 1, 2); // 1 syntax warning, 2 runtime warnings
            }, @"
nil
1
[1, 2]
[]
[1]
[1, 2]
1
nil
1
error
");
        }

        public void Scenario_RubyProcCallArgs1B() {
            AssertOutput(delegate() {
                CompilerTest(@"
Proc.new { |x| puts x.inspect }.call
Proc.new { |x| puts x.inspect }.call 1
Proc.new { |x| puts x.inspect }.call 1,2
Proc.new { |x| puts x.inspect }.call []
Proc.new { |x| puts x.inspect }.call [1]
Proc.new { |x| puts x.inspect }.call [1,2]
Proc.new { |x| puts x.inspect }.call *[1]
Proc.new { |(x,)| puts x.inspect }.call *[]
", 2, 2); // 2 syntax warning, 2 runtime warnings
            }, @"
nil
1
[1, 2]
[]
[1]
[1, 2]
1
nil
");
        }

        public void Scenario_RubyBlockArgs2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def a; yield; end 
def b; yield 1; end 
def c; yield 1,2; end 
def d; yield []; end 
def e; yield [1]; end 
def f; yield [1,2]; end 

a { |x,y| p [x, y] }
b { |x,y| p [x, y] }
c { |x,y| p [x, y] }
d { |x,y| p [x, y] }
e { |x,y| p [x, y] }
f { |x,y| p [x, y] }
");       
            }, @"
[nil, nil]
[1, nil]
[1, 2]
[nil, nil]
[1, nil]
[1, 2]
");
        }

        public void Scenario_RubyProcCallArgs2A() {
            AssertOutput(delegate() {
                CompilerTest(@"
lambda { |x,y| p [x, y] }.call rescue puts 'error'
lambda { |x,y| p [x, y] }.call 1 rescue puts 'error'
lambda { |x,y| p [x, y] }.call 1,2
lambda { |x,y| p [x, y] }.call [] rescue puts 'error'
lambda { |x,y| p [x, y] }.call [1] rescue puts 'error'
lambda { |x,y| p [x, y] }.call [1,2] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[1,2] 
lambda { |x,y| p [x, y] }.call *[[1]] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[[1,2]] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[[1,2,3]] rescue puts 'error'
", 4, 0);
            }, @"
error
error
[1, 2]
error
error
error
[1, 2]
error
error
error
");
        }

        public void Scenario_RubyProcCallArgs2B() {
            AssertOutput(delegate() {
                CompilerTest(@"
Proc.new { |x,y| p [x, y] }.call 
Proc.new { |x,y| p [x, y] }.call 1
Proc.new { |x,y| p [x, y] }.call 1,2
Proc.new { |x,y| p [x, y] }.call []
Proc.new { |x,y| p [x, y] }.call [1]
Proc.new { |x,y| p [x, y] }.call [1,2]
Proc.new { |x,y| p [x, y] }.call *[1,2] 
Proc.new { |x,y| p [x, y] }.call *[[1]]
Proc.new { |x,y| p [x, y] }.call *[[1,2]]
Proc.new { |x,y| p [x, y] }.call *[[1,2,3]]
", 4, 0);
            }, @"
[nil, nil]
[1, nil]
[1, 2]
[nil, nil]
[1, nil]
[1, 2]
[1, 2]
[1, nil]
[1, 2]
[1, 2]
");
        }

        public void Scenario_RubyProcCallArgs2C() {
            AssertOutput(delegate() {
                CompilerTest(@"
Proc.new { || p [] }.call 
Proc.new { |x| p [x] }.call 1
Proc.new { |x,y| p [x,y] }.call 1,2
Proc.new { |x,y,z| p [x,y,z] }.call 1,2,3
Proc.new { |x,y,z,w| p [x,y,z,w] }.call 1,2,3,4
Proc.new { |x,y,z,w,u| p [x,y,z,w,u] }.call 1,2,3,4,5
Proc.new { |x,y,z,w,u,v| p [x,y,z,w,u,v] }.call 1,2,3,4,5,6
");
            }, @"
[]
[1]
[1, 2]
[1, 2, 3]
[1, 2, 3, 4]
[1, 2, 3, 4, 5]
[1, 2, 3, 4, 5, 6]
");
        }

        /// <summary>
        /// Tests MRI inconsistency in Yield1 vs YieldNoSplat1 when invoked from Call1.
        /// </summary>
        public void Scenario_RubyProcCallArgs2D() {
            AssertOutput(delegate() {
                CompilerTest(@"
f = proc{|x,| x}
p f.call(1)
p f.call([1])
p f.call([[1]])
p f.call([1,2])
");
            }, @"
1
[1]
[[1]]
[1, 2]
");
        }


        public void Scenario_RubyProcYieldArgs1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def y *a
  yield *a
end

y() { || p [] }
y(1) { |x| p [x] }
y(1,2) { |x,y| p [x,y] }
y(1,2,3) { |x,y,z| p [x,y,z] }
y(1,2,3,4) { |x,y,z,w| p [x,y,z,w] }
y(1,2,3,4,5) { |x,y,z,w,u| p [x,y,z,w,u] }
y(1,2,3,4,5,6) { |x,y,z,w,u,v| p [x,y,z,w,u,v] }
puts '---'
y(1,2,3,4,5,6) { || p [] }
y(1,2,3,4,5,6) { |x| p [x] }
y(1,2,3,4,5,6) { |x,y| p [x,y] }
y(1,2,3,4,5,6) { |x,y,z| p [x,y,z] }
y(1,2,3,4,5,6) { |x,y,z,w| p [x,y,z,w] }
y(1,2,3,4,5,6) { |x,y,z,w,u| p [x,y,z,w,u] }
y(1,2,3,4,5,6) { |x,y,z,w,u,v| p [x,y,z,w,u,v] }
puts '---'
y(1,2,3) { || p [] }
y(1,2,3) { |x| p [x] }
y(1,2,3) { |x,y| p [x,y] }
y(1,2,3) { |x,y,z| p [x,y,z] }
y(1,2,3) { |x,y,z,w| p [x,y,z,w] }
y(1,2,3) { |x,y,z,w,u| p [x,y,z,w,u] }
y(1,2,3) { |x,y,z,w,u,v| p [x,y,z,w,u,v] }
", 1, 2);
            }, @"
[]
[1]
[1, 2]
[1, 2, 3]
[1, 2, 3, 4]
[1, 2, 3, 4, 5]
[1, 2, 3, 4, 5, 6]
---
[]
[[1, 2, 3, 4, 5, 6]]
[1, 2]
[1, 2, 3]
[1, 2, 3, 4]
[1, 2, 3, 4, 5]
[1, 2, 3, 4, 5, 6]
---
[]
[[1, 2, 3]]
[1, 2]
[1, 2, 3]
[1, 2, 3, nil]
[1, 2, 3, nil, nil]
[1, 2, 3, nil, nil, nil]
");
        }

        /// <summary>
        /// RHS is list, LHS is not simple, but contains splatting.
        /// </summary>
        public void Scenario_RubyBlockArgs3() {
            AssertOutput(delegate() {
                CompilerTest(@"
def baz
   yield [1,2,3]
end
baz { |*a| puts a.inspect }
");
            }, @"[[1, 2, 3]]");
        }

        /// <summary>
        /// !L(1,-) && R(0,*), empty array to splat.
        /// </summary>
        public void Scenario_RubyBlockArgs4A() {
            AssertOutput(delegate() {
                CompilerTest(@"
def y
   yield *[]
end

y { |*a| puts a.inspect }
", 1, 0);
            }, @"[]");
        }

        /// <summary>
        /// Anonymous unsplat parameters.
        /// </summary>
        public void Scenario_RubyBlockArgs4B() {
            AssertOutput(delegate() {
                CompilerTest(@"
def y
  a = [1,2,3,4,5]
  yield a,[6]
end

y { |(x,y,*),*| p x,y }
puts '-'
y { |(x,y,*a),*| p x,y,a }
puts '-'
y { |(x,y,*),*a| p x,y,a }
");
            }, @"
1
2
-
1
2
[3, 4, 5]
-
1
2
[[6]]
");
        }

        /// <summary>
        /// L(M,*) := R(N,*,=) where M is less then N.
        /// </summary>
        public void Scenario_RubyBlockArgs5() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b,c,*p|
    print a,b,c,'|',*p
  }
end

c = C.new
c[1,2,3,4,5,6,7,*[8]] = 9
");
            }, @"123|456789");
        }

        /// <summary>
        /// L(M,*) := R(N,*,=) where M is greater then N.
        /// </summary>
        public void Scenario_RubyBlockArgs6() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b,c,*p|
    print a,b,c,'|',*p
  }
end

c = C.new
c[1,*[2]] = 3
");
            }, @"123|");
        }

        /// <summary>
        /// Wrong number of arguments.
        /// </summary>
        public void Scenario_RubyBlockArgs7() {
            AssertExceptionThrown<ArgumentException>(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b| }
end

c = C.new
c[1,2,*[]] = 3
");
            });
        }

        /// <summary>
        /// L(1, -) := R(0,*0,=)
        /// </summary>
        public void Scenario_RubyBlockArgs8() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a|
    p a
  }
end

c = C.new
c[*[]] = 1
");
            }, @"1");
        }

        /// <summary>
        /// L(1, -) := R(N,*,=)
        /// </summary>
        public void Scenario_RubyBlockArgs9() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a|
    p a
  }
end

c = C.new
c[1,*[2]] = 3
", 0, 1);
            }, @"[1, 2, 3]");
        }

        /// <summary>
        /// L(2..5, -) := R(N,*,=)
        /// </summary>
        public void Scenario_RubyBlockArgs10() {
            // L(2, -) := R(N,*,=)
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b|
    print a,',',b
  }
end

c = C.new
c[1,*[]] = 2
");
            }, @"1,2");

            // L(3, -) := R(N,*,=)
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b,c|
    print a,',',b,',',c
  }
end

c = C.new
c[1,2,*[]] = 3
");
            }, @"1,2,3");

            // L(4, -) := R(N,*,=)
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b,c,d|
    print a,',',b,',',c,',',d
  }
end

c = C.new
c[1,*[2,3]] = 4
");
            }, @"1,2,3,4");

            // L(5, -) := R(N,*,=)
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  define_method('[]=') { |a,b,c,d,e|
    print a,',',b,',',c,',',d,',',e
  }
end

c = C.new
c[1,2,*[3,4]] = 5
");
            }, @"1,2,3,4,5");
        }

        public void Scenario_RubyBlockArgs11() {
            TestOutput(@"
class C
  def to_a
    [1,2]
  end
end

def baz
  yield C.new
end
baz { |a,b| p b }

class C
  def to_ary
    1
  end
end
baz { |a,b| p b } rescue p $!

class C
  def to_ary
    [3,4]
  end
end
baz { |a,b| p b }
", @"
nil
#<TypeError: C#to_ary should return Array>
4
");
        }
        
        public void RubyProcs1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo x,y,&f
  yield x,y
  f[3,4]
end

foo(1,2) do |a,b|
  puts a,b
end
");
            }, @"
1
2
3
4
");
        }

        /// <summary>
        /// Assigning to a block parameter should not affect yield.
        /// </summary>
        public void RubyProcs2() {
            TestOutput(@"
def foo(&b)
  b = nil
  yield
end

foo { puts 'foo' }
", @"
foo
");
        }

        public void RubyProcArgConversion1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_proc
    lambda { |x| puts x }
  end
end

class D
  def to_proc
    lambda { |x| puts x + 1 }
  end
end

class E  
end

1.times(&C.new)
1.times(&D.new)
1.times(&E.new) rescue puts 'error'
");
            }, @"
0
1
error
");
        }

        public void RubyProcArgConversion2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_proc; 1; end
end

class D
  def to_proc; lambda { puts 'ok2' }; end
end

1.times(&lambda { puts 'ok1' })
1.times(&C.new) rescue puts $!
1.times(&D.new)
");
            }, @"
ok1
C#to_proc should return Proc
ok2
");
        }

        public void RubyProcArgConversion3() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo &b
  p b
end

foo(&nil)
");
            }, @"nil");
        }

        public void RubyProcArgConversion4() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def respond_to? name
    puts name
    $has_to_proc
  end

  def to_proc
    lambda { puts 'ok' }
  end
end

c = C.new

$has_to_proc = false
1.times(&c) rescue puts 'error'

$has_to_proc = true
1.times(&c)
");
            }, @"
to_proc
error
to_proc
ok
");
        }

        public void ProcNew1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  1.times { |x| $x = Proc.new }
end

y = lambda { puts 'foo' }
foo(&y)
p $x.object_id == y.object_id
");
            }, @"true");
        }

        public void ProcNew2() {
            TestOutput(@"
class P < Proc
end

def foo
  1.times { |x| $x = P.new }
end

y = lambda { puts 'foo' }
foo(&y)
p $x.object_id == y.object_id
p $x.class
", @"
false
P
");
        }

        public void ProcNew3() {
            TestOutput(@"
$x = 0

class P < Proc
  def initialize *args, &b
    p args, b.class, self.class, block_given?
    $x += 1
    retry if $x < 2
    123
  end
end

def arg
  puts 'arg'
  []
end

def foo
  P.new(*arg) { puts 2 }
end

foo { puts 1 }
", @"
arg
[]
Proc
P
true
arg
[]
Proc
P
true
");
        }

        public void ProcNew4() {
            TestOutput(@"
class P < Proc
  def initialize *args, &b
    p args, b.class, self.class, block_given?
    123
  end
end

def arg
  puts 'arg'
  []
end

def foo
  P.new(*arg)
end

foo { puts 1 }
", @"
arg
[]
NilClass
P
false
");
        }

        public void MethodToProc1() {
            AssertOutput(() => CompilerTest(@"
class C
  def foo a, b=2, *args
    p self, a, b, args
    123
  end
end

class D < C
end

x = 'hello'
q = C.new.method(:foo).to_proc
p q[1]
p q[] rescue p $!

# to_proc captures the caller's binding:
eval('puts x, self', q.binding)

p D.new.instance_eval(&q)
"), @"
#<C:0x*>
1
2
[]
123
#<ArgumentError: wrong number of arguments (0 for 1)>
hello
main
#<C:0x*>
#<D:0x*>
2
[]
123
", OutputFlags.Match);
        }

        public void DefineMethod1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    $x = lambda {
      puts self.class
    }
    
    $x.call
  end
end

C.new.foo

class D
  define_method :goo, &$x
end

D.new.goo
");
            }, @"
C
D");
        }

        /// <summary>
        /// define_method and class_eval change owner of the method definition.
        /// </summary>
        public void DefineMethod2() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  $p = lambda {    
    def goo
      def bar
        puts 'bar'
      end
    end  
  }      
end

class C
  define_method :foo,&$p
end

class D
 class_eval(&$p)
end

c = C.new
d = D.new

d.goo
c.foo
c.goo

p M.instance_methods(false).sort
p C.instance_methods(false).sort
p D.instance_methods(false).sort
");
            }, @"
[""bar""]
[""foo"", ""goo""]
[""goo""]
");

        }

        [Options(Compatibility = RubyCompatibility.Ruby19)]
        public void ProcPosition1() {
             AssertOutput(() => CompilerTest(@"
def foo &q 
  p q
end

class C < Proc
end

foo { }                         # line 9
p lambda {}                     
p Proc.new {}                   
p method(:foo).to_proc          # TODO: source info not available
p C.new {}                      
"), @"
#<Proc:0x*@*ProcPosition1.rb:9>
#<Proc:0x*@*ProcPosition1.rb:10 (lambda)>
#<Proc:0x*@*ProcPosition1.rb:11>
#<Proc:0x*>
#<C:0x*@*ProcPosition1.rb:13>
", OutputFlags.Match);
        }

        public void BlockArity1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts '== -4'

p Proc.new{|(a,b,c,*)|}.arity

puts '== -2'

p Proc.new{|(a,b,c,*),*|}.arity

puts '== -1'

p Proc.new{|(*)|}.arity
p Proc.new{}.arity
p Proc.new{|*|}.arity

puts '== 0'

p Proc.new{||}.arity

puts '== 1'

p Proc.new{|x|}.arity    
p Proc.new{|x,|}.arity    
p Proc.new{|(x,)|}.arity 
p Proc.new{|(x,),|}.arity
p Proc.new{|((x,))|}.arity
p Proc.new{|((x,y))|}.arity
p Proc.new{|((x,y,))|}.arity
p Proc.new{|(x,y,),|}.arity
p Proc.new{|(*),|}.arity
p Proc.new{|(x,*),|}.arity

puts '== 2'

p Proc.new{|x,y|}.arity  
p Proc.new{|x,y,|}.arity 
p Proc.new{|x,y,|}.arity 
p Proc.new{|(x,y)|}.arity
p Proc.new{|(x,y,)|}.arity

puts '== 3'

p Proc.new{|x,y,z|}.arity
p Proc.new{|x,y,z,|}.arity
p Proc.new{|(x,y,z)|}.arity
p Proc.new{|(x,y,z,)|}.arity
");
            }, @"
== -4
-4
== -2
-2
== -1
-1
-1
-1
== 0
0
== 1
1
1
1
1
1
1
1
1
1
1
== 2
2
2
2
2
2
== 3
3
3
3
3
");
        }
        
        public void Proc_RhsAndBlockArguments1() {
            AssertOutput(() => CompilerTest(@"
class Proc
  alias []= []
end

x = Proc.new { |*a| p a, block_given? }

x.call(1,2,&lambda {})
x[1,2] = 3

"), @"
[1, 2]
false
[1, 2, 3]
false
");
        }

        public void EvalBreak1() {
            AssertOutput(() => CompilerTest(@"
x = 10.times do
  puts 'in 1st loop'
  eval('break 1') 
end
p x 

x = while true
  puts 'in 2nd loop'   
  eval('break 2')
end
p x

x = Array.new(10) do
  eval('break 3')
end
p x

class C
  define_method(:foo) do
    eval('break 4')
  end
end
p C.new.foo

x = Kernel.module_eval do
  eval('break 5')
end
p x

p 10.times { break eval('while true do eval(""break 6""); end') }
"), @"
in 1st loop
1
in 2nd loop
2
3
4
5
6
");
        }

        /// <summary>
        /// Break from a block called via Proc#call.
        /// </summary>
        public void EvalBreak2() {
            TestOutput(@"
def f(&b)
  b.call
end
p f { break 123 }
", @"
123
");
        }

        public void EvalBreak3() {
            AssertOutput(() => CompilerTest(@"
def foo
  eval('break')
rescue 
  p $!.class
end

foo

class C
  eval('break')
rescue 
  p $!.class
end

begin
  eval('break')
rescue 
  p $!.class
end
"), @"
LocalJumpError
LocalJumpError
LocalJumpError
");
        }

        /// <summary>
        /// Block needs to set InLoop and InRescue flags on RFC to "false".
        /// </summary>
        public void EvalRetry1() {
            AssertOutput(() => CompilerTest(@"
def foo &p
  p[]
end

$x = 0

begin
  begin
    puts 'in body'
    raise
  rescue
    foo do
      puts 'in block'
      eval('retry') if ($x += 1) < 3    
    end
  end
rescue 
  p $!
end
"), @"
in body
in block
#<LocalJumpError: retry from proc-closure>
");
        }

        public void EvalRetry2() {
            // retry in block:
            AssertOutput(() => CompilerTest(@"
$x = 0
1.times do |i|
  puts 'in block'
  module M
    eval('retry') if ($x += 1) < 2  
  end  
end
"), @"
in block
in block
");

            // retry in rescue:
            AssertOutput(() => CompilerTest(@"
$x = 0
1.times do
  begin
    raise
  rescue 
    puts 'in rescue'
    module M
      eval('retry') if ($x += 1) < 2
    end
  end
end
"), @"
in rescue
in rescue
");
            // TODO:
            // retry in a define_method block:
            AssertOutput(() => CompilerTest(@"
$x = 0
class C
  define_method :foo do
    puts 'define_method'
    begin
      eval('module M; retry; end') if ($x += 1) < 3 

# TODO: should not catch here
#   rescue
#     p 'unreachable'
    end
  end
end

begin
  C.new.foo
rescue
  p $!
end"), @"
define_method
#<LocalJumpError: retry from proc-closure>
");

            // retry in a method taking a block:
            AssertOutput(() => CompilerTest(@"
$x = 0
def foo *a
  yield
  eval('module M; retry; end') if ($x += 1) < 2 
end

foo(puts('in arg')) { puts 'in block' }
"), @"
in arg
in block
in arg
in block
");
        }

        public void EvalRedo1() {
            // redo in loop:
            AssertOutput(() => CompilerTest(@"
$x = 0
while (puts('in condition'); true)
  puts 'in loop'
  eval('module M; redo; end') if ($x += 1) < 2
  break
end
"), @"
in condition
in loop
in loop
");

            // redo in block:
            AssertOutput(() => CompilerTest(@"
$x = 0
2.times do |i|
  puts 'in block ' + i.to_s
  eval('module M; redo; end') if ($x += 1) < 2
end
"), @"
in block 0
in block 0
in block 1
");

            // redo in define_method block:
            AssertOutput(() => CompilerTest(@"
$x = 0
class C
  define_method :foo do
    puts 'in block'
    eval('module M; redo; end') if ($x += 1) < 2
  end
end
C.new.foo
"), @"
in block
in block
");
        }

        public void EvalNext1() {
            // next in loop:
            AssertOutput(() => CompilerTest(@"
$x = 0
while (puts('in condition'); true)
  puts 'in loop'
  eval('module M; next; end') if ($x += 1) < 2
  break
end
"), @"
in condition
in loop
in condition
in loop
");

            // next in block:
            AssertOutput(() => CompilerTest(@"
$x = 0
2.times do |i|
  puts 'in block ' + i.to_s
  eval('module M; next; end') if ($x += 1) < 2
end
"), @"
in block 0
in block 1
");

            // next in define_method block:
            AssertOutput(() => CompilerTest(@"
$x = 0
class C
  define_method :foo do
    puts 'in block'
    eval('module M; next; end') if ($x += 1) < 2
    puts 'unreachable'
  end
end
C.new.foo
"), @"
in block
");
            // next returning a value:
            AssertOutput(() => CompilerTest(@"
class C
  define_method :foo do
    eval('next 123')
  end
end

p C.new.foo
"), @"
123
");
        }

        public void EvalReturn1() {
            TestOutputWithEval(@"
def y
  yield
end

def foo
  $b = Proc.new {  
    eval('return 123')
  }
  goo
end

def goo
  y(&$b)
end

p foo
", @"
123
"
            );
        }

        public void EvalReturn2() {
            TestOutputWithEval(@"
def foo
  $p.call
end

def owner
  $p = lambda do
    #<return 123#>
  end

  p foo

  puts 'owner.end'
end

owner
", @"
123
owner.end
"
            );
        }

        public void EvalReturn3() {
            TestOutputWithEval(@"
def foo
  $p.call
rescue 
  p $!
end

def owner
  $p = Proc.new do
    #<return 123#>
  end
end

owner
foo
", @"
#<LocalJumpError: unexpected return>
"
            );
        }

        public void EvalReturn4() {
            TestOutput(@"
def foo
  eval <<-END
    10.times do
      return 123
    end
  END
end

puts foo
", @"
123
");
        }

        public void BEGIN1() {
            TestOutput(@"
x = 1
BEGIN {
  p x rescue p $!
  y = 1
  z = 2
  1.times { p y + z }  
  $binding = binding
}

p eval('x', $binding) rescue p $!
p eval('y+z', $binding)
", @"
#<NoMethodError: undefined method `x' for main:Object>
3
#<NoMethodError: undefined method `x' for main:Object>
3
");
        }

        public void BEGIN2() {
            TestOutput(@"
puts '5'
BEGIN {
  puts '2'
  BEGIN {
    puts '1'
  }   
  puts '3'
}
puts '6'
BEGIN {
  puts '4'
}
puts '7'
", @"
1
2
3
4
5
6
7
");
        }

        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void BEGIN3() {
            TestOutput(@"
def f1
end

BEGIN {
  def f2
  end
}

class C
  private
  while true
    eval <<-END
      BEGIN {
        def f3
        end
      
        private
      
        break
        puts 'unreachable'
      } 
    END
  end
  
  def f4
  end
end

p self.private_methods(false).include?('f1')
p self.public_methods(false).include?('f2')

p C.public_instance_methods(false).include?('f3')
p C.private_instance_methods(false).include?('f4')
", @"
true
true
true
true
");
        }

        public void SymbolToProc1() {
            TestOutput(@"
class C
  def bar
    yield(self) rescue p $!
  end

  private
  def foo; 'foo'; end
  def to_s; 'C'; end
end

C.new.bar(&:foo)

n = :nil?.to_proc
p n.call(nil)
n.call() rescue p $!
p n.call([1,2])
", @"
#<NoMethodError: private method `foo' called for C:C>
true
#<ArgumentError: no receiver given>
false
");
        }
    }
}

