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
        public void Scenario_Assignment1() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = 1
a += 2
puts a");
            }, @"3");
        }

        /// <summary>
        /// Order of evaluation.
        /// </summary>
        public void Scenario_ParallelAssignment1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def one; puts 'one'; 1; end
def two; puts 'two'; 2; end
def three; puts 'three'; 3; end

a,b = one,two,three
puts a.inspect,b.inspect
");
            }, @"
one
two
three
1
2
");
        }

        public void Scenario_ParallelAssignment2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo()
  x, y = 3, 4
  puts x, y
  x, y = y, x
  puts x, y
  x, y = 1
  puts x, y
  x, y = 5, 6, 7
  puts x, y
  z = (x, y = 5, 6, 7)
  puts z
end

foo
");
            }, @"
3
4
4
3
1
nil
5
6
5
6
7");
        }

        public void Scenario_ParallelAssignment4() {
            TestOutput(@"
ra = (a = *4)
rb = (b = *[4])
rc = (c = *[*4])
rd = (d = 1,*4)
re = (e = 1,*[4])
rf = (f = 1,*[*4])
puts a.inspect,b.inspect,c.inspect,d.inspect,e.inspect,f.inspect
puts ra.inspect,rb.inspect,rc.inspect,rd.inspect,re.inspect,rf.inspect
", @"
[4]
[4]
[4]
[1, 4]
[1, 4]
[1, 4]
[4]
[4]
[4]
[1, 4]
[1, 4]
[1, 4]
");
        }

        public void Scenario_ParallelAssignment5() {
            AssertOutput(delegate() {
                CompilerTest(@"
r = (x,(y,(z1,z2)),(*u),v,w = 1,[*[1,2]],3,*4)

puts 'r = ' + r.inspect,
  'x = ' + x.inspect,
  'y = ' + y.inspect,
  'z1 = ' + z1.inspect,
  'z2 = ' + z2.inspect,
  'u = ' + u.inspect,
  'v = ' + v.inspect,
  'w = ' + w.inspect
");
            }, @"
r = [1, [1, 2], 3, 4]
x = 1
y = 1
z1 = 2
z2 = nil
u = [3]
v = 4
w = nil
");
        }

        /// <summary>
        /// Non-simple LHS, simple RHS. Difference between |LHS| > 0 (r0 and r3) and |LHS| == 0 (r1).
        /// </summary>
        public void Scenario_ParallelAssignment6() {
            AssertOutput(delegate() {
                CompilerTest(@"
r0 = (x,y = [1,2])
r1 = (*v = [1,2])
r2 = (*w = *[1,2])
r3 = (p,*q = [1,2])
puts r0.inspect, r1.inspect, r2.inspect, r3.inspect, '*'
puts x.inspect, y.inspect, '*', v.inspect, '*', w.inspect, '*', p.inspect, q.inspect
");
            }, @"
[1, 2]
[1, 2]
[1, 2]
[1, 2]
*
1
2
*
[1, 2]
*
[1, 2]
*
1
[2]
");
        }

        /// <summary>
        /// Simple LHS and splat only RHS.
        /// </summary>
        public void Scenario_ParallelAssignment7() {
            TestOutput(@"
a = (ax = *1)
b = (bx = *[])
c = (cx = *[1])
d = (dx = *[1,2])

puts a.inspect, ax.inspect, b.inspect, bx.inspect, c.inspect, cx.inspect, d.inspect, dx.inspect
", @"
[1]
[1]
[]
[]
[1]
[1]
[1, 2]
[1, 2]
");
        }

        /// <summary>
        /// Simple RHS.
        /// </summary>
        public void Scenario_ParallelAssignment8() {
            TestOutput(@"
r1 = (a = [1,2])
r2 = (b,c = [1,2])
r3 = (d,e = *[1,2])
r4 = (f,g = 1)

puts r1.inspect, r2.inspect, r3.inspect, r4.inspect
puts b.inspect, c.inspect, d.inspect, e.inspect, f.inspect, g.inspect
", @"
[1, 2]
[1, 2]
[1, 2]
1
1
2
1
2
1
nil
");
        }

        /// <summary>
        /// Inner splat-only LHS.
        /// </summary>
        public void Scenario_ParallelAssignment9() {
            TestOutput(@"
c = ((*a),(*b) = [1,2],[3,4])
puts a.inspect, b.inspect, c.inspect
", @"
[1, 2]
[3, 4]
[[1, 2], [3, 4]]
");
        }

        /// <summary>
        /// Recursion in L(1,-).
        /// </summary>
        public void Scenario_ParallelAssignment10() {
            TestOutput(@"
ra = ((a,) = *[])
rb = ((b,) = 1,*[])
puts a.inspect, ra.inspect
puts b.inspect, rb.inspect
", @"
nil
[]
1
[1]
");
        }

        /// <summary>
        /// ArrayItemAccess and AttributeAccess read target ones in an in-place assignment.
        /// </summary>
        public void SimpleInplaceAsignmentToIndirectLeftValues1() {
            TestOutput(@"
class Array
  def x; 1; end
  def x= value; puts 'x=' end
end

def foo
  puts 'foo'
  [0]
end

p foo[0] += 1
p foo::x += 2
p foo[0] &&= 3
p foo::x &&= 4
p foo[0] ||= 5
p foo::x ||= 6
", @"
foo
1
foo
x=
3
foo
3
foo
x=
4
foo
0
foo
1
");
        }

        public void SetterCallValue() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  def x= value
    puts value
    'foo'
  end
  
  def x value
    puts value
    puts 'foo'
  end
end

foo = C.new

puts(foo.x=('bar'))
puts '---'
puts(foo.x('bar'))
");
            }, @"
bar
bar
---
bar
foo
nil");

        }

        private const string/*!*/ MemberAssignmentDefs = @"
class String
  def + other
    puts ""#{self} + #{other}""
    ""#{self}#{other}""
  end
end

class C
  def x= value
    puts ""write: #{value}""
  end
  
  def x 
    puts ""read""
    $result
  end
end

def target
  puts 'target'
  C.new
end

def rhs
  puts 'rhs'
  '_rhs'
end
";

        public void MemberAssignmentExpression1() {
            AssertOutput(delegate {
                CompilerTest(MemberAssignmentDefs + @"
$result = 'result'
puts(target.x += rhs)
");
            }, @"
target
read
rhs
result + _rhs
write: result_rhs
result_rhs
");
        }

        public void MemberAssignmentExpression2() {
            AssertOutput(delegate {
                CompilerTest(MemberAssignmentDefs + @"
$result = true
puts(target.x &&= rhs)
puts
$result = false
puts(target.x &&= rhs)
");
            }, @"
target
read
rhs
write: _rhs
_rhs

target
read
false
");
        }

        public void MemberAssignmentExpression3() {
            AssertOutput(delegate {
                CompilerTest(MemberAssignmentDefs + @"
$result = true
puts(target.x ||= rhs)
puts
$result = false
puts(target.x ||= rhs)
");
            }, @"
target
read
true

target
read
rhs
write: _rhs
_rhs
");
        }
    }
}
