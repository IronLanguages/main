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

using System.IO;
namespace IronRuby.Tests {
    public partial class Tests {
        public void ClassDuplication1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  X = 'const'
  @@x = 'cls-var'
  def foo
    puts self.class 
    puts @@x
  end
end

D = C.dup
D.new.foo
puts D::X
");
            }, @"
D
cls-var
const
");
        }

        public void ClassDuplication2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Object
  def foo
    puts 'foo'
  end
end

O = Object.dup

class Object
  remove_method :foo
end

O.new.foo
");
            }, @"
foo
");
        }

        public void ClassDuplication3() {
            AssertOutput(delegate() {
                CompilerTest(@"
S = String.dup
p S.ancestors
puts S.instance_methods(false) - String.instance_methods(false) == []
puts s = S.new('ghk')
puts s.reverse
");
            }, @"
[S, Enumerable, Comparable, Object, Kernel]
true
ghk
khg
");
        }

        /// <summary>
        /// This is different from MRI. In MRI ancestors of a duplicated Object class are [O, Kernel].
        /// </summary>
        public void ClassDuplication4() {
            AssertOutput(delegate() {
                CompilerTest(@"
O = Object.dup
p O.ancestors
p O.constants.include?('Object')
");
            }, @"
[O, Kernel, Object, Kernel]
true
");
        }

        public void ClassDuplication5() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
p System::Collections.dup.constants.include?('IEnumerable')
");
            }, @"
true
");
        }

        public void ClassDuplication6() {
            var output = @"
""foo""
[1, 2]
""default""
1..2
/foo/
""""
[""a"", ""b""]
""foo""
";

            AssertOutput(delegate() {
                CompilerTest(@"
p String.dup.new('foo')
p Array.dup.new([1,2])
p Hash.dup.new('default')[1]
p Range.dup.new(1,2)
p Regexp.dup.new('foo')
p Module.dup.new.name
p Struct.dup.new(:a,:b).dup[1,2].dup.members
p Proc.dup.new { 'foo' }[]
");
            }, output);

            AssertOutput(delegate() {
                CompilerTest(@"
class S < String; end
class A < Array; end
class H < Hash; end
class R < Range; end
class RE < Regexp; end
class M < Module; end
class St < Struct; end
class P < Proc; end

p S.dup.new('foo')
p A.dup.new([1,2])
p H.dup.new('default')[1]
p R.dup.new(1,2)
p RE.dup.new('foo')
p M.dup.new.name
p St.dup.new(:a,:b).dup[1,2].dup.members
p P.dup.new { 'foo' }[]
");
            }, output);
        }

        public void ClassDuplication7() {
            TestOutput(@"
class C
  def foo
  end
end

D = C.dup
p D.instance_method(:foo)
", @"
#<UnboundMethod: D#foo>
");
        }

        public void Structs1() {
            AssertOutput(() => CompilerTest(@"
S = Struct.new(:f,:g)
class S
  alias set_f f=
end

s = S[1,2]
s.set_f rescue p $!
s.set_f(3)
puts s.f
s.set_f(4,5) rescue p $!
s.set_f(*[]) rescue p $!
s.set_f(*[6])
puts s.f
s.set_f(*[6,7]) rescue p $!
puts s.f(*[])
puts s.f(*[1]) rescue p $!
"
                ), @"
#<ArgumentError: wrong number of arguments (0 for 1)>
3
#<ArgumentError: wrong number of arguments (2 for 1)>
#<ArgumentError: wrong number of arguments (0 for 1)>
6
#<ArgumentError: wrong number of arguments (2 for 1)>
6
#<ArgumentError: wrong number of arguments (1 for 0)>
");
        }

        public void MetaModules1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Meta < Module
  def bar
    puts 'bar'
  end
end

M = Meta.new

module M
  def foo
    puts 'foo'
  end
end

class C
  include M
end

C.new.foo
M.bar
");
            }, @"
foo
bar
");
        }

        public void MetaModulesDuplication1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Meta < Module
end

M = Meta.new
puts M.name
puts M.class

N = M.dup
puts N.name
puts N.class
");
            }, @"
M
Meta
N
Meta
");
        }

        /// <summary>
        /// Autoload removes the constant from the declaring module before it loads the target file.
        /// </summary>
        public void Autoload1() {
            if (_driver.PartialTrust) return;

            var file = @"
class D 
  p defined? X
  X = 123
end
";
            using (_driver.MakeTempFile("file", ".rb", file)) {
                TestOutput(@"
class D
  autoload(:X, $file)
end

class C < D
  p X
end

", @"
nil
123
");
            }
        }

        /// <summary>
        /// Freezing and module initializers.
        /// </summary>
        public void ModuleFreezing1() {
            if (_driver.PartialTrust) return;

            TestOutput(@"
Float.freeze
puts defined? Float::EPSILON         # this should work, Float is a builtin
puts defined? 1.2.+

Enumerable.freeze
begin
  require 'enumerator'               # monkey-patches Enumerable
rescue Exception
  p $!
end
", @"
constant
method
#<TypeError: can't modify frozen module>
");
        }

        /// <summary>
        /// Tests recursive singleton freezing.
        /// </summary>
        public void ModuleFreezing2() {
            TestOutput(@"
[Module.new, Object.new].each do |obj|
  obj.freeze
  p obj.frozen?
  class << obj
    p frozen?
    class << self
      p frozen?
      class << self
        p frozen?
      end
    end
  end
end
", @"
true
true
true
true
true
true
true
true
");
        }
    }
}
