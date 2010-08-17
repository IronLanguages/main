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
        public void RubyForLoop1() {
            AssertOutput(delegate() {
                CompilerTest(@"
for a in [1,2,3]
    x = 'ok'
    print a
end

print x         # x visible here, for-loop doesn't define a scope
");
            }, "123ok");
        }

        public void WhileLoop1() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
x = while i < 4 do 
  puts i
  i = i + 1
end
puts x
");
            }, @"
0
1
2
3
nil
");
        }

        /// <summary>
        /// Break in a while loop.
        /// </summary>
        public void LoopBreak1() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 3
x = while i > 0 do 
  puts i
  if i == 2 then
    break
  end
  i = i - 1
end
puts x
");
            }, @"
3
2
nil
");
        }

        /// <summary>
        /// Break with a value in a while loop.
        /// </summary>
        public void LoopBreak2() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 3
x = while i > 0 do 
  puts i
  if i == 2 then
    break 'foo'
  end
  i = i - 1
end
puts x
");
            }, @"
3
2
foo
");
        }

        /// <summary>
        /// Loop break from within class declaration.
        /// </summary>
        public void LoopBreak3() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
while i < 5 do
  puts i

  class C
    puts 'in C'
    break
  end
  
  i = i + 1
end
");
            }, @"
0
in C
");
        }

        /// <summary>
        /// Redo in a while loop.
        /// </summary>
        public void LoopRedo1() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 3
j = 2
x = while i > 0 do 
  puts i
  if i == 2 and j > 0 then
    j = j - 1
    redo
  end
  i = i - 1
end
puts x
");
            }, @"
3
2
2
2
1
nil
");
        }

        /// <summary>
        /// Next in a while loop.
        /// </summary>
        public void LoopNext1() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 3
j = 2
x = while i > 0 do 
  puts i
  if i == 2 and j > 0 then
    j = j - 1
    next 'foo'
  end
  i = i - 1
end
puts x
");
            }, @"
3
2
2
2
1
nil
");
        }

        /// <summary>
        /// Break, retry, redo and next in a method, out of loop/rescue.
        /// </summary>
        public void MethodBreakRetryRedoNext1() {
            TestOutput(@"
def _break; break rescue p $!; end
def _redo; redo rescue puts 'not caught here'; end
def _retry; retry rescue puts 'not caught here'; end
def _next; next rescue puts 'not caught here'; end
def e_break; eval('break') rescue p $!; end
def e_redo; eval('redo') rescue puts 'not caught here'; end
def e_retry; eval('retry') rescue puts 'not caught here'; end           # // TODO: bug
def e_next; eval('next') rescue puts 'not caught here'; end             # // TODO: bug

_break
_retry rescue p $!
_redo rescue p $!
_next rescue p $!
e_break
e_retry rescue p $!
e_redo rescue p $!
e_next rescue p $!
", @"
#<LocalJumpError: unexpected break>
#<LocalJumpError: retry used out of rescue>
#<LocalJumpError: unexpected redo>
#<LocalJumpError: unexpected next>
#<LocalJumpError: unexpected break>
#<LocalJumpError: retry used out of rescue>
not caught here
not caught here
");
        }

        public void Scenario_RubyUntilLoop1() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 1
until i > 3 do
  puts i
  i = i + 1
end
");
            }, @"
1
2
3");
        }

        public void Scenario_WhileLoopCondition1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def w x
  while x; p x; break; end
end

w []
w 0
w 1
w nil
w true
w false
");
            }, @"
[]
0
1
true
");
        }

        public void PostTestWhile1() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  puts 1
end while (puts 2)
");
            }, @"
1
2
");
        }

        public void PostTestUntil1() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  puts 1
end until (puts(2); true)
");
            }, @"
1
2
");
        }

        public void WhileModifier1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 1 while (puts 2)
");
            }, @"
2
");
        }

        public void UntilModifier1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 1 until (puts(2); true)
");
            }, @"
2
");
        }

        public void WhileModifier2() {
            AssertOutput(delegate() {
                CompilerTest(@"
$i = false
def c
  puts 'c'
  $i = !$i 
end

def x
  puts 'x'
end

(x) while c
");
            }, @"
c
x
c
");
        }

        public void UntilModifier2() {
            AssertOutput(delegate() {
                CompilerTest(@"
$i = true
def c
  puts 'c'
  $i = !$i 
end

def x
  puts 'x'
end

(x) until c
");
            }, @"
c
x
c
");
        }
    }
}
