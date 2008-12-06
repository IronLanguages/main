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

    }
}
