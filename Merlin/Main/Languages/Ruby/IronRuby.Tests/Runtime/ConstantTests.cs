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
        public void Constants1A() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Const
  def get_const
    CONST
  end
  
  CONST = 1
end

puts Const.new.get_const
");
            }, @"1");
        }

        public void Constants1B() {
            AssertOutput(delegate() {
                CompilerTest(@"
OUTER_CONST = 99
class Const
  puts CONST = OUTER_CONST + 1
end

class Const2 < Const
end

puts Const::CONST
puts ::OUTER_CONST
puts Const::NEW_CONST = 123
puts Const2::CONST
");
            }, @"
100
100
99
123
100");
        }

        public void ConstantNames() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  X = 1
end

module N
  X = 1
  Z = 2
end

class C
  include M, N
  
  puts X,Z
  Y = 2
  U = 1
end

class D < C
  U = 3
  W = 4 
end

p C.constants.sort
p D.constants.sort
p M.constants.sort
");
            }, @"
1
2
[""U"", ""X"", ""Y"", ""Z""]
[""U"", ""W"", ""X"", ""Y"", ""Z""]
[""X""]
");
        }

        /// <summary>
        /// Class/module def makes name of the constant by appending its simple name to the name of the lexically containing class/module.
        /// </summary>
        public void Constants3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class B
  class D
    puts self
  end
end

module M
  class D
    puts self
  end
end

class C < B
  include M

  class D
    puts self    
  end
end
");
            }, @"
B::D
M::D
C::D
");
        }

        /// <summary>
        /// Top-level class/module definition does look-up in Object and all its mixins.
        /// </summary>
        public void Constants4() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  class CM
    $x1 = self
  end
end

module N
  class CN
    $y1 = self
  end
end

class Object
  include M
  
  class CM
    $x2 = self
  end
end

class Foo
  include N
  
  class CN
    $y2 = self
  end
end

puts $x1.object_id == $x2.object_id
puts $y1.object_id != $y2.object_id
");
            }, @"
true
true
");
        }

        /// <summary>
        /// Unqualified constant lookup should search in lexical scopes from inner most to the outer most 
        /// looking into modules for constant declared in those modules and their mixins.
        /// It shouldn't look to base classes nor mixins.
        /// </summary>
        public void UnqualifiedConstants1() {
             AssertOutput(delegate() {
                CompilerTest(@"
class B
  Q = 'Q in B'
end

module M
  P = 'P in M'
end

class C
  S = 'S in C'
  Q = 'Q in C'
  P = 'P in C'
  class C < B
    include M    
    S = 'S in C::C'

    puts C, P, Q, S
  end 
end
");
             }, @"
C::C
P in C
Q in C
S in C::C
");
        }

        /// <summary>
        /// If a constant is not found in the current scope chain, the inner-most module scope's module/class and its ancestors are checked.
        /// </summary>
        public void UnqualifiedConstants2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class D
  D = 'U in D'
end

class B
  W = 'W in B'
end

module M
  V = 'V in M'

  def puts_U
    puts U rescue puts $!
    puts W rescue puts $!
  end
end

class C < D
  class C < B
    include M    

    def puts_consts
      puts U rescue puts $!
      puts V, W
      puts_U
    end
  end 
end

C::C.new.puts_consts
");
            }, @"
uninitialized constant C::C::U
V in M
W in B
uninitialized constant M::U
uninitialized constant M::W
");
        }

        /// <summary>
        /// Global constants defined in loaded code are defined on the anonymous module created by the load.
        /// </summary>
        public void LoadAndGlobalConstants() {
            // TODO:
        }

        /// <summary>
        /// Global constants are visible in Runtime.Globals.
        /// </summary>
        public void GlobalConstantsInterop() {
            CompilerTest("C = 1");
            object value;
            Runtime.Globals.TryGetVariable("C", out value);
            AssertEquals(value, 1);
        }
    }
}
