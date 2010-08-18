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

using IronRuby.Builtins;
using System;
using System.IO;
using Microsoft.Scripting.Hosting.Providers;
using IronRuby.Runtime;
namespace IronRuby.Tests {

    public partial class Tests {
        public void Constants1A() {
            TestOutput(@"
class Const
  def get_const
    CONST
  end
  
  CONST = 1
end

puts Const.new.get_const
", @"
1
");
        }

        public void Constants1B() {
            TestOutput(@"
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
", @"
100
100
99
123
100
");
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
[:U, :X, :Y, :Z]
[:U, :W, :X, :Y, :Z]
[:X]
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
        /// The default interop constant_missing looks to scope only if called on Object.
        /// </summary>
        public void GlobalConstants1() {
            TestOutput(@"
class Bar
end

module Foo
end

p defined?(Foo::Bar)
Foo::Bar rescue p $!
", @"
nil
#<NameError: uninitialized constant Foo::Bar>
");
        }

        public void ConstantCaching_Unqualified1() {
            TestOutput(@"
module M
  C = 1
  module N
    i = 0
    while i < 2
      p C
      const_set(:C, 2) if i == 0 
      i += 1
    end
  end
end
", @"
1
2
");
        }

        public void ConstantCaching_Unqualified2() {
            TestOutput(@"
module M
  module N
    i = 0
    while i < 2
      puts defined?(C)                             # tested cache
      const_set(:C, 2) if i == 0 
      i += 1
    end
  end
end
", @"
nil
constant
");
        }

        public void ConstantCaching_Unqualified3() {
            Runtime.Globals.SetVariable("C", 1);
            TestOutput(@"
module M
  $M = self
  class ::Object
    i = 0
    while i < 3
      p C                                          # tested cache
      $M.const_set(:C, 2) if i == 0 
      $M.send(:remove_const, :C) if i == 1
      i += 1
    end
  end
end
", @"
1
2
1
");
        }

        /// <summary>
        /// GlobalConstantAccess version needs to be invalidate on inclusion.
        /// </summary>
        public void ConstantCaching_Unqualified4() {
            TestOutput(@"
class C
  def self.foo
	X                         # tested cache
  end
end

C.foo rescue p $!

module M
  X = 1
end

class C
  include M
end

p C.foo
", @"
#<NameError: uninitialized constant C::X>
1
");
        }

        /// <summary>
        /// module_eval {} in Ruby 1.9 changes constant lookup chain.
        /// </summary>
        public void ConstantCaching_Unqualified5() {
#if TODO
            TestOutput(@"
module A
 
end

module B
  $q = Proc.new { X = 1 }
  A.module_eval &$q
end

module C
  module_eval &$q
end

p A.constants
p B.constants
p C.constants
", @"
[:X]
[]
[:X]
");
#endif
        }

        /// <summary>
        /// Update of strong value needs to set the weak value as well (and vice versa).
        /// </summary>
        public void ConstantCaching_Unqualified6() {
            TestOutput(@"
X = [1]                       # assign a non-primitive value so that the cache needs to use a WeakRef

class C
  $C = self
  def self.foo
    X                         # tested cache
  end
end

p $C.foo, $C.foo              # test twice - 1) Op-call returns the value 2) the cache returns the value

module M
  $M = self
  X = 1                       # assign primitive value
end

class C
  include $M                  # changes cached value from non-primitive (weak ref) to primitive (strong ref)
end

p $C.foo, $C.foo
", @"
[1]
[1]
1
1
");
        }

        /// <summary>
        /// Check to see whether we don't unwrap WeakReferences accidentally, preserve object identity and unwrap null correctly.
        /// </summary>
        public void ConstantCaching_Unqualified7() {
            var wr = new WeakReference(new object());
            Context.DefineGlobalVariable("wr", wr);
            var result = Engine.Execute<RubyArray>(@"
C = $wr
def c; C; end   # tested cache 
r = [c, c]
C = nil
r + [c, c]
");
            Assert(ReferenceEquals(result[0], wr));
            Assert(ReferenceEquals(result[1], wr));
            Assert(ReferenceEquals(result[2], null));
            Assert(ReferenceEquals(result[3], null));
        }

        public void ConstantCaching_Unqualified_IsDefined1() {
            TestOutput(@"
a = []
2.times { a << defined?(Object) }
a[0][0] = ?x
puts a
", @"
xonstant
constant
");
        }

        public void ConstantCaching_Qualified1() {
            AssertOutput(() => CompilerTest(@"
module A
  module B
    $B = self
    module C
      module D
      end
    end
  end
end

module NC
  $NC = self
  D = 1
end

i = 0 
while i < 2 
  p A::B::C::D, ::A::B::C::D
  $B.const_set(:C, $NC) if i == 0
  i += 1
end
", 0, 1), @"
A::B::C::D
A::B::C::D
1
1
");
        }

        public void ConstantCaching_Qualified2() {
            TestOutput(@"
module A
  module B
    $B = self

    module C
      D = 1
    end

    def self.const_missing(name)
      puts 'missing: ' + name.to_s
      $C
    end
  end
end

module C
  $C = self
  module D    
  end
end

module E
  $E = self
  D = 2
end

i = 0
while i < 6
  p A::B::C::D                                    # tested cache
  $B.send(:remove_const, :C) if i == 1
  $B.send(:const_set, :C, $E) if i == 3
  i += 1
end
", @"
1
1
missing: C
C::D
missing: C
C::D
2
2
");
        }

        public void ConstantCaching_Qualified_IsDefined1() {
            TestOutput(@"
module A
  module B
    $B = self

    module C
      D = 1                              # D is not module here
    end

    def self.const_missing(name)
      puts 'missing: ' + name.to_s
      $C
    end
  end
end

module C
  $C = self
  module D    
    F = 1
  end
end

module E
  $E = self
  module D  
    F = 2
  end
end

i = 0
while i < 6
  puts defined?(A::B::C::D::F)              # tested cache
  $B.send(:remove_const, :C) if i == 1
  $B.send(:const_set, :C, $E) if i == 3
  i += 1
end
", @"
nil
nil
missing: C
constant
missing: C
constant
constant
constant
");
        }

        public void ConstantCaching_Qualified_IsDefined2() {
            TestOutput(@"
def foo
  $M
end

module M
  $M = self  
  A = 1
end

i = 0
while i < 3
  puts defined?(foo::A)              # tested cache
  puts foo::A rescue p $!            # tested cache
  $M.send(:remove_const, :A) if i == 1
  i += 1
end
", @"
constant
1
constant
1
nil
#<NameError: uninitialized constant M::A>
");
        }

        public void ConstantCaching_CrossRuntime1() {
            var engine2 = Ruby.CreateEngine();

            var c = Engine.Execute(@"
module C
  X = 1
end
C
");
            ((RubyContext)HostingHelpers.GetLanguageContext(engine2)).DefineGlobalVariable("C", c); 

            var m = engine2.Execute(@"
module M
  module N
    O = $C
  end
end
M
");
            Context.DefineGlobalVariable("M", m); 

            TestOutput(@"
module D
  E = $M
end

module Z
  module O
    X = 2
  end
end

i = 0
while i < 6
  puts D::E::N::O::X rescue p $!         # tested cache
  $M.send(:remove_const, :N) if i == 1
  $M.send(:const_set, :N, Z) if i == 3
  i += 1
end
", @"
1
1
#<NameError: uninitialized constant M::N>
#<NameError: uninitialized constant M::N>
2
2
");
        }

        public void ConstantCaching_AutoUpdating1A() {
            if (_driver.PartialTrust) return;

            var autoloaded = @"
$C.const_set(:D, 1)
$C.const_set(:E, 1)

module A
  remove_const(:B)
  class B
    class C
      D = 2
    end
  end
end
";
            using (_driver.MakeTempFile("file", ".rb", autoloaded)) {
                TestOutput(@"
module A
  module B
    module C   
      $C = self 
      autoload(:D, $file)
    end
  end
end

i = 0
while i < 2
  puts A::B::C::D              # tested cache
  i += 1
end
", @"
1
2
");
            }
        }

        public void ConstantCaching_AutoUpdating1B() {
            if (_driver.PartialTrust) return;

            var autoloaded = @"
class X
  D = 1
end

$B.const_set(:C, X)
$A.send(:remove_const, :B)
";

            using (_driver.MakeTempFile("file", ".rb", autoloaded)) {
                TestOutput(@"
module A
  $A = self
  module B
    $B = self 
    autoload(:C, $file)
  end
end

i = 0
while i < 2
  puts defined?(A::B::C::D)           # tested cache
  i += 1
end
", @"
constant
nil
");
            }
        }
    }
}
