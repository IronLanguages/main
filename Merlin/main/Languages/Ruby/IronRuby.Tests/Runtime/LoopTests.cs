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


        public void Scenario_RubyForLoop1() {
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

        public void Scenario_RubyForLoop2() {
#if OBSOLETE
            ScriptScope module = ScriptDomainManager.CurrentManager.CreateModule("x");
            module.SetVariable("list", PY.Execute(module, PY.CreateScriptSourceFromString("[1,2,3]")));
            
            AssertOutput(delegate() {
                RB.Execute(module, RB.CreateScriptSourceFromString(@"
for a in list
    print a
end
", SourceCodeKind.Statements));
            }, "123");
#endif
        }

        public void Scenario_RubyWhileLoop1() {
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

        public void Scenario_RubyWhileLoop2() {
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
        /// Break in a while loop.
        /// </summary>
        public void Scenario_RubyWhileLoop3() {
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
        /// Redo in a while loop.
        /// </summary>
        public void Scenario_RubyWhileLoop4() {
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
        public void Scenario_RubyWhileLoop5() {
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
        /// Loop break from within class declaration.
        /// </summary>
        public void Scenario_RubyWhileLoop6() {
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
