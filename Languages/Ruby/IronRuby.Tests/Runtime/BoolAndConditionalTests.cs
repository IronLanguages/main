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
        public void Scenario_RubyBoolExpressions1() {
            TestOutput(@"
puts '!'
puts !false
puts !true
puts !nil
puts !0
puts !'foo'
puts '!!'
puts !!false
puts !!true
puts !!nil
puts !!0
puts !!'foo'
puts '!!!'
puts !!!false
puts !!!true
puts !!!nil
puts !!!0
puts !!!'foo'
", @"
!
true
false
true
false
false
!!
false
true
false
true
true
!!!
true
false
true
false
false");
        }

        public void Scenario_RubyBoolExpressions2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def t; print 'T '; true; end
def f; print 'F '; false; end

puts(t && t)
puts(f && t)
puts(t && f)
puts(f && f)

puts(t || t)
puts(f || t)
puts(t || f)
puts(f || f)

puts(f || f && t && t)
");
            }, @"
T T true
F false
T F false
F false
T true
F T true
T true
F F false
F F false
");
        }

        public void Scenario_RubyBoolExpressions3() {
            AssertOutput(delegate() {
                CompilerTest(@"
def t; print 'T '; true; end
def f; print 'F '; false; end

puts(x = (t and t))
puts(x = (f and t))
puts(x = (t and f))
puts(x = (f and f))

puts(x = (t or t))
puts(x = (f or t))
puts(x = (t or f))
puts(x = (f or f))

puts(x = (f or f and t and t))
");
            }, @"
T T true
F false
T F false
F false
T true
F T true
T true
F F false
F F false
");
        }

        public void Scenario_RubyBoolExpressions4() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = 'x'
y = 'y'
z = 'z'

a = ((x = 2.0) and (y = nil) and (z = true))
puts a,x,y,z
");
            }, @"
nil
2.0
nil
z
");
        }

        public void Scenario_RubyBoolExpressions5() {
            TestOutput(@"
class C
  def !
    puts '!'
    123
  end
end

c = C.new

if not c
  puts 'f'
else
  puts 't'
end

puts 'unless' unless c
puts 'unless not' unless not c

a = !c
b = c.!
c = not(c)
p a,b,c
", @"
!
f
!
!
!
!
123
123
123
");
        }

        public void Scenario_RubyBoolExpressionsWithReturn1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def is_true x
    x and return 'true'
    x or return 'false'
    puts 'X'
end

def run
    puts is_true(true)
    puts is_true(false)
    puts(true, puts('foo'), puts('bar'), (true and return (false or return 'baz')), puts('X'))
end

puts run
");
            }, @"
true
false
foo
bar
baz
");
        }

        public void Scenario_RubyBoolExpressionsWithReturn2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  false || return
  puts 'unreachable'
end
foo
");
            }, @"");
        }

        public void TernaryConditionalWithJumpStatements1() {
            TestOutput(@"
def foo a
  (a ? return : break) while true
  puts 'foo'
end

foo true
foo false
", @"
foo
");
        }

        public void TernaryConditionalWithJumpStatements2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo a
  (a ? 'foo' : return)
end

def bar a
  (a ? return : 'bar')
end

puts foo(true)
puts foo(false)
puts bar(true)
puts bar(false)
");
            }, @"
foo
nil
nil
bar
");
        }        

        public void Scenario_RubyBoolAssignment() {
            AssertOutput(delegate() {
                CompilerTest(@"
t = true
f = false
m = 0
n = nil

t &&= false
f ||= true
m &&= 1
n ||= 2

p t,f,m,n
");
            }, @"
false
true
1
2
");
        }

        /// <summary>
        /// Else-if clauses.
        /// </summary>
        public void Scenario_RubyIfExpression1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts(if nil then 1 end)
puts(if 1 then 1 end)
puts(if nil then 1 else 2 end)
puts(if 1 then 1 else 2 end)
puts(if nil then 1 elsif nil then 2 end)
puts(if nil then 1 elsif 1 then 2 end)
puts(if nil then 1 elsif nil then 2 else 3 end)
puts(if nil then 1 elsif 1 then 2 else 3 end)
");
            }, @"
nil
1
2
1
nil
2
3
2
");
        }

        /// <summary>
        /// Bodies.
        /// </summary>
        public void Scenario_RubyIfExpression2() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts(if nil then end)
puts(if 1 then end)
puts(if 1 then 1;11;111 end)
puts(if nil then 1 else 2;22 end)
");
            }, @"
nil
nil
111
22
");
        }

        public void Scenario_RubyUnlessExpression1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts(unless nil then 1 end)
puts(unless 1 then 1 end)
");
            }, @"
1
nil
");
        }

        public void Scenario_RubyConditionalExpression1() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = true ? 1 : 'foo'
y = nil ? 2.0 : 'foo'
z = 'foo' || 2
u = 3 && nil

puts x,y,z,u
");
            }, @"
1
foo
foo
nil
");
        }

        public void ConditionalStatement1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def t; puts 1; true; end
def f; puts 2; false; end 

t unless t
t unless f
f unless t
f unless f
t if t
t if t
f if t
f if f
");
            }, @"
1
2
1
1
2
2
1
1
1
1
1
2
2
");
        }

        public void ConditionalStatement2() {
            AssertOutput(delegate() {
                CompilerTest(@"
p x = (1 if true)
p x = (1 if false)
");
            }, @"
1
nil
");
        }
    }
}
