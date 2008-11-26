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



        public void Scenario_RubyHashes1A() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = Hash.[](1, 'a', 2, 'b')
print x[1], x[2]
");
            }, @"ab");
        }

        public void Scenario_RubyHashes1B() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts Hash.[]()
");
            }, "");
        }

        public void Scenario_RubyHashes1C() {
            AssertExceptionThrown<ArgumentException>(delegate() {
                CompilerTest(@"
puts Hash.[](1)
");
            });
        }

        public void Scenario_RubyHashes2() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = { 1 => 'a' }
puts x[1]
");
            }, @"
a
");
        }

        public void Scenario_RubyHashes3() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo(a,b,c)
  puts a, b, c
end

foo Hash.[](1 => 'a'), { 2 => 'b' }, 3 => 'c'
");
            }, @"
1a
2b
3c
");
        }

        public void Scenario_RubyHashes4() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
    def []=(a,b)
      print a[1], a[2], b
    end
end

C.new[1 => 'a', 2 => 'b'] = 'c'
");
            }, @"abc");
        }


        /// <summary>
        /// Equality comparer doesn't call 'eql?' if the values are reference-equal.
        /// </summary>
        public void EqualityComparer1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def eql? other
    puts 'eql?'
  end
end

c = C.new

hash = { c => 1 }
p hash[c]
");
            }, @"1");
        }
    }
}
