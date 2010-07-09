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
using Microsoft.Scripting.Runtime;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;

namespace IronRuby.Tests {

    public partial class Tests {
        public void RangeConditionInclusive1() {
            TestRangeCondition(true, @"
0: b -> false
1: FALSE
1: b -> false
2: FALSE
2: b -> true
3: e -> true
4: TRUE
4: b -> true
5: e -> true
6: TRUE
6: b -> true
7: e -> false
8: TRUE
8: e -> true
9: TRUE
9: b -> false
10: FALSE
10: b -> false
11: FALSE
11: b -> true
12: e -> true
13: TRUE
13: b -> nil
14: FALSE
");
        }

        public void RangeConditionExclusive1() {
            TestRangeCondition(false, @"
0: b -> false
1: FALSE
1: b -> false
2: FALSE
2: b -> true
3: TRUE
3: e -> true
4: TRUE
4: b -> true
5: TRUE
5: e -> true
6: TRUE
6: b -> true
7: TRUE
7: e -> false
8: TRUE
8: e -> true
9: TRUE
9: b -> false
10: FALSE
10: b -> false
11: FALSE
11: b -> true
12: TRUE
12: e -> true
13: TRUE
13: b -> nil
14: FALSE
");
        }
        
        private void TestRangeCondition(bool inclusive, string/*!*/ expected) {
            AssertOutput(delegate() {
                CompilerTest(@"
F = false
T = true
x = X = '!'

B = [F,F,T,x,T,x,T,x,x,F,F,T,x]
E = [x,x,x,T,x,T,x,F,T,x,x,x,T]
       
def b
  r = B[$j]
  puts ""#{$j}: b -> #{r.inspect}""
  
  $j += 1
  
  $continue = !r.nil?  
  r == X ? raise : r  
end

def e
  r = E[$j]
  puts ""#{$j}: e -> #{r.inspect}""
  
  $j += 1
  
  $continue = !r.nil?  
  r == X ? raise : r  
end

$j = 0
$continue = true
while $continue
  if b<..>e 
    puts ""#{$j}: TRUE"" 
  else
    puts ""#{$j}: FALSE"" 
  end
end
".Replace("<..>", inclusive ? ".." : "..."));
            }, expected);
        }

        /// <summary>
        /// The state variable of a range condition is allocated in the inner-most non-block lexical scope.
        /// </summary>
        public void RangeCondition1A() {
            TestOutput(@"
$i = 0

def foo
  $i += 1
  $i == 1
end     

def test
  l = lambda {
    puts(foo..false ? 'true' : 'false')
  }
  
  1.times(&l)
  1.times(&l)
  1.times(&l)
  1.times(&l)
end

test
", @"
true
true
true
true
");
        }

        /// <summary>
        /// The state variable of a range condition allocated the inner-most non-block lexical scope.
        /// </summary>
        public void RangeCondition1B() {
            TestOutput(@"
$i = 0

2.times {
  module M  
    def self.foo
      $i += 1
      $i == 1
    end     

    2.times {
      puts(foo..false ? 'true' : 'false')
    }
  end
}  
", @"
true
true
false
false
");
        }

        /// <summary>
        /// The state variable of a range condition is statically allocated. 
        /// </summary>
        public void RangeCondition1C() {
            TestOutput(@"
$i = 0

2.times {
  module M  
    def self.foo
      $i += 1
      $i == 1
    end  

    2.times {
      eval(""puts(foo..false ? 'true' : 'false')"")
    }
  end
}  
", @"
true
false
false
false
");
        }

        /// <summary>
        /// Block expressions propagate 'in-condition' property to the last statement.
        /// </summary>
        public void RangeCondition2() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts((TRUE..FALSE) ? 'true' : 'false')
puts(begin TRUE..FALSE end ? 'true' : 'false')

#puts((0;1;TRUE..FALSE) ? 'true' : 'false')   # TODO: literals are removed with warning
");
            }, @"
true
true
");
        }
    }
}
