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
using System.Linq;
using IronRuby.Builtins;

namespace IronRuby.Tests {
    public partial class Tests {

        public void Scenario_RubyArrays1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts Array.[]('a','b','c')
puts Array[1,2,3]
puts ['e','f','g']
");
            }, String.Format("a{0}b{0}c{0}1{0}2{0}3{0}e{0}f{0}g{0}", Environment.NewLine));
        }

        public void Scenario_RubyArrays2() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = ['e',['f'],'g']
a[2] = a[0]
print a[2]
print a[1][0]
");
            }, "ef");
        }

        public void Scenario_RubyArrays3() {
            TestOutput(@"
puts %w{}.size
puts %W{}.size
puts %w{x}
puts %W{x}
puts %w{          y          }
puts %W{          y          }
puts %w{hello world}
puts %W{hello world}
puts %w<cup<T> cup<co-phi>>
puts %W<cup<T> cup<co-phi>>
puts %w{hello w#{0}r#{1}d}
puts %W{hello w#{0}r#{1}d}
", @"
0
0
x
x
y
y
hello
world
hello
world
cup<T>
cup<co-phi>
cup<T>
cup<co-phi>
hello
w#{0}r#{1}d
hello
w0r1d
");
        }

        public void Scenario_RubyArrays4() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = [*x = [1,2]]
puts a.object_id == x.object_id
puts a.inspect
");
            }, @"
true
[1, 2]");
        }

        public void Scenario_RubyArrays5() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = [*4]
b = [*[4]]
c = [*[*4]]
d = [*[*[4]]]
puts a.inspect,b.inspect,c.inspect,d.inspect
");
            }, @"
[4]
[4]
[4]
[4]
");
        }

        public void Scenario_RubyArrays6() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = [1,*[[],[*4], *[*[]]]]
puts a.inspect
");
            }, @"[1, [], [4]]");
        }

        public void IListOps_EnumerateRecursively1() {
            var a = new RubyArray();
            var b = new RubyArray();
            b.Add(new RubyArray { new RubyArray { b } });
            a.Add(b);

            var result = IListOps.EnumerateRecursively(new Runtime.ConversionStorage<System.Collections.IList>(Context), a, -1, list => {
                Assert(list == b);
                return 123;
            }).ToArray();

            Assert(result.Length == 1 && (int)result[0] == 123);
        }
    }
}
