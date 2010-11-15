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

using System.IO;
using IronRuby.Runtime;

namespace IronRuby.Tests {
    public partial class Tests {
        public void BasicObject1() {
            TestOutput(@"
class Class
  def f
    puts 'Class'
  end
end

class BasicObject
  class << self
    def f
      puts 'BO'
    end
  end
end

Object.f
", @"
BO
");            
        }

        /// <summary>
        /// TODO: Unqualified constant lookup doesn't fallback to Object on BasicObject and its subclasses.
        /// </summary>
        public void BasicObject2() {
            XTestOutput(@"
module M
  R = 1
end

class BasicObject
  include ::M

  p defined?(R)

  X = 1
  p defined?(Object)
  class Z
    p defined?(Object)
  end
  
  module Q
    p defined?(Object)
  end
end

class BO < BasicObject
  p defined?(Object)  
end

class C
  p constants(true)
end
", @"
""constant""
nil
""constant""
""constant""
nil
[]
");
        }

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
            TestOutput(@"
S = String.dup
p S.ancestors
puts S.instance_methods(false) - String.instance_methods(false) == []
puts s = S.new('ghk')
puts s.reverse
", @"
[S, Comparable, Object, Kernel, BasicObject]
true
ghk
khg
");
        }

        /// <summary>
        /// BasicObject
        /// </summary>
        public void ClassDuplication4() {
            TestOutput(@"
O = Object.dup
p O.ancestors
p O.constants.include?(:Object)
BasicObject.dup rescue p $!
", @"
[O, Kernel, BasicObject]
true
#<TypeError: can't copy the root class>
");
        }

        public void ClassDuplication5() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
p System::Collections.dup.constants.include?(:IEnumerable)
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
nil
[:a, :b]
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

Object.freeze
begin
  require 'yaml'               # monkey-patches Object
rescue Exception
  p $!
end
", @"
constant
method
#<RuntimeError: can't modify frozen class>
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

        public void InstanceVariables1() {
            TestOutput(@"
# class with no static instance variables
class A
end

# class with 1 static instance variable
class B < A
  def initialize
    @a = 1
  end
end

# class with 3 static instance variables (1 inherited)
class C < B
  def initialize
    super
    @b = 2
    @d = 3
  end
end

# class with 4 static instance variables (3 inherited)
class D < C
  def foo
    @c = @a + @b
  end

  def bar
    ""> #@d""                            # instance variable in string
  end
end

d = D.new
puts d.foo
puts d.bar
", @"
3
> 3
");
        }

        public void InstanceVariables2() {
            TestOutput(@"
module M
  attr_reader :m

  def setm
    @m = @a + @x
  end
end

class C
  include M

  def initialize
    @a, @b = 1, 2
  end

  def isdef
    defined?(@a)
  end
end

c = C.new
puts c.isdef
c.instance_variables.each { |x| p x.to_sym }
puts c.instance_variable_get(:@a)

c.instance_variable_set(:@a, 3)
puts c.instance_variable_get(:@a)

c.instance_variable_set(:@x, 10)
puts c.instance_variable_get(:@x)

c.setm
p c.m

p c.instance_variable_defined?(:@a)
p c.instance_variable_defined?(:@x)
p c.instance_variable_defined?(:@w)
", @"
instance-variable
:@a
:@b
1
3
10
13
true
true
false
");
        }

        /// <summary>
        /// Instance variable removal.
        /// </summary>
        public void InstanceVariables3() {
            TestOutput(@"
module M
  def dreport
    puts defined?(@a), defined?(@b), defined?(@c), defined?(@d), defined?(@w)
  end
end

class C
  include M

  def foo
    @a, @b = 1, 2
  end

  def sreport
    puts defined?(@a), defined?(@b)
  end
end

c = C.new
c.foo
c.instance_variable_set(:@c, 3)
c.instance_variable_set(:@d, 3)

p c.send(:remove_instance_variable, :@c)
p c.send(:remove_instance_variable, :@a)
c.send(:remove_instance_variable, :@c) rescue p $!
c.send(:remove_instance_variable, :@a) rescue p $!
c.send(:remove_instance_variable, :@w) rescue p $!
p c.instance_variable_get(:@a)
p c.instance_variable_get(:@c)
p c.instance_variable_get(:@w)
puts '---'
c.sreport
puts '---'
c.dreport
", @"
3
1
#<NameError: instance variable `@c' not defined>
#<NameError: instance variable `@a' not defined>
#<NameError: instance variable `@w' not defined>
nil
nil
nil
---
nil
instance-variable
---
nil
instance-variable
nil
instance-variable
nil
");
        }

        /// <summary>
        /// Object cloning.
        /// </summary>
        public void InstanceVariables4() {
            TestOutput(@"
class C 
  def foo
    @a, @b, @c = 1, 2, 3
  end

  def sreport
    p [@a, @b, @c]
  end

  def dreport
    eval('p [@a, @b, @c, @d]')
  end
end

c = C.new
c.foo

c.dup.sreport
c.dup.dreport
c.instance_variable_set(:@d, 4)
c.clone.sreport
c.clone.dreport
", @"
[1, 2, 3]
[1, 2, 3, nil]
[1, 2, 3]
[1, 2, 3, 4]
");
        }

        /// <summary>
        /// Subclasses of builtin types and CLR types.
        /// </summary>
        public void InstanceVariables5() {
            XTestOutput(@"
class R < Range
  def foo
    @a = 1
    instance_variable_set(:@aa, 11)
  end
end

class S < String
  def foo
    @b = 2
    instance_variable_set(:@bb, 22)
  end
end

class T < System::Collections::ArrayList
  def foo
    @c = 3
    instance_variable_set(:@cc, 33)
  end
end

class U
  include System::IDisposable
  
  def foo
    @d = 4
    instance_variable_set(:@dd, 44)
  end
end

r, s, t, u = R.new(1,2), S.new('x'), T.new, U.new
r.foo
s.foo
t.foo
u.foo

u.instance_variable_get(:@d)
#p r.instance_variable_get(:@a), s.instance_variable_get(:@b), t.instance_variable_get(:@c), u.instance_variable_get(:@d)
#p r.instance_variable_get(:@aa), s.instance_variable_get(:@bb), t.instance_variable_get(:@cc), u.instance_variable_get(:@dd)
", @"
1
2
3
4
11
22
33
44
");
        }

        /// <summary>
        /// Anonymous classes with a basse class that has static instance variables.
        /// </summary>
        public void InstanceVariables6() {
            TestOutput(@"
class C
  def initialize 
    @a = 1 
  end
end

D = Class.new(C)
p D.new.instance_variable_get(:@a)
", @"
1
");
        }

        public void InstanceVariables7() {
            TestOutput(@"
class C
  def initialize(value)
    @value = value
  end

  def foo(other)
    [@value, other.instance_eval{ @value }]
  end
end

a, b = C.new(1), C.new(2)
p a.foo(b)
", @"
[1, 2]
");
        }

        // Fixnum doesn't have identity in Ruby
        public void InstanceVariables10() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Fixnum
  def foo= a
    @x = a
  end
  def foo
    @x
  end
end

a = 1
b = 1
c = 2

a.foo = 1
b.foo = 2
c.foo = 3
puts a.foo, b.foo, c.foo
");
            },
            @"
2
2
3");
        }

        // Float has an identity in Ruby
        public void InstanceVariables20() {
            TestOutput(@"
class Float
  def foo= a
    @x = a
  end
  def foo
    @x
  end
end

a = 1.0
b = 1.0

a.foo = 1
b.foo = 2
puts a.foo, b.foo
", @"
1
2
");
        }
    }
}
