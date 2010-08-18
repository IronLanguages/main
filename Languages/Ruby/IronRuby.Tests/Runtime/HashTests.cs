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
using Microsoft.Scripting;

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
            TestOutput(@"
puts Hash.[]()
", @"
{}
");
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
            TestOutput(@"
def foo(a,b,c)
  puts a, b, c
end

foo Hash.[](1 => :a), { 2 => :b }, 3 => :c
", @"
{1=>:a}
{2=>:b}
{3=>:c}
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

        public void Scenario_RubyHashes5() {
            TestOutput(@"
h = { a: 1, b: 2 }
puts h[:a], h[:b]

def foo *args
  p args[0][:a], args[0][:b] 
end

foo a: 1, b: 2
", @"
1
2
1
2
"
);
            // a list of expressions no longer supported:
            AssertExceptionThrown<SyntaxErrorException>(() => Engine.Execute(@"h = {1,2,3,4}"));
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
