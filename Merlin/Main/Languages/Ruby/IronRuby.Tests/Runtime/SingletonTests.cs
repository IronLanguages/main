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

using IronRuby.Runtime;
using System;
namespace IronRuby.Tests {
    public partial class Tests {
        public void MainSingleton1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts self.to_s
");
            }, "main");
        }

        public void MainSingleton2() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  def foo
    puts 'foo'
  end
end

include M
foo
");
            }, "foo");
        }

        /// <summary>
        /// Ruby class.
        /// </summary>
        public void Singletons1A() {
            Singletons1_Test("C", null);
        }

        /// <summary>
        /// Object.
        /// </summary>
        public void Singletons1B() {
            Singletons1_Test("Object", null);
        }

        /// <summary>
        /// A subclass of a CLR class.
        /// </summary>
        public void Singletons1C() {
            Singletons1_Test("X", "< System::Collections::ArrayList");
        }

        /// <summary>
        /// A CLR class.
        /// </summary>
        public void Singletons1D() {
            Singletons1_Test("System::Collections::ArrayList", null);
        }

        public void Singletons1_Test(string/*!*/ className, string inherits) {
            TestOutput(String.Format(@"
class {0} {1}
end

x, y = {0}.new, {0}.new

class << x
  def foo; 1; end
end

class << y
  def foo; 2; end
end

p x.foo, y.foo
", className, inherits), 
@"
1
2
");
        }

        public void Singletons2() {
            TestOutput(@"
class C
  def i_C
    puts 'i_C'
  end

  def self.c_C
    puts 'c_C'
  end
end

x = C.new
y = C.new

class << x
  $x1 = self

  def i_x1
    puts 'i_x1'
  end

  def self.c_x1
    puts 'c_x1'
  end
end

x.i_x1
x.i_C

x.c_x1 rescue puts 'X'
x.c_C rescue puts 'X'
y.i_x1 rescue puts 'X'
",
@"
i_x1
i_C
X
X
X
");
        }

        public void Singletons3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    puts 'C_foo'
  end   
end

class Class
  def foo
    puts 'Class_foo'
  end
end

x = C.new

class << x 
  $C1 = self
end

$C1.foo
");
            }, @"
Class_foo
");
        }

        public void SingletonCaching1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C  
  def foo
    puts 'foo'
  end
end

c = C.new
c.foo

class << c
  def foo
    puts 'bar'
  end
end
c.foo
");
            }, @"
foo
bar");
        }

        /// <summary>
        /// IRubyObjects.
        /// </summary>
        public void SingletonCaching2A() {
            AssertOutput(() => CompilerTest(@"
class C
end

foo = C.new
bar = C.new
def foo.to_s; 'ok'; end
puts bar.to_s
puts foo.to_s
"), @"
#<C:*>
ok
", OutputFlags.Match);
        }

        /// <summary>
        /// Object.
        /// </summary>
        public void SingletonCaching2B() {
            AssertOutput(() => CompilerTest(@"
foo = Object.new
bar = Object.new
def foo.to_s; 'ok'; end
puts bar.to_s
puts foo.to_s
"), @"
#<Object:*>
ok
", OutputFlags.Match);
        }

        /// <summary>
        /// CLR types.
        /// </summary>
        public void SingletonCaching2C() {
            TestOutput(@"
A = System::Collections::ArrayList
class A
  def to_s
    'base'
  end
end

foo, bar = A.new, A.new
puts bar.to_s
puts foo.to_s
def foo.to_s; 'singleton 1'; end
puts bar.to_s
puts foo.to_s
def foo.to_s; 'singleton 2'; end
puts bar.to_s
puts foo.to_s
", @"
base
base
base
singleton 1
base
singleton 2
");
        }

        public void Scenario_ClassVariables_Singletons() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
end

module M
  class << 'x'
    $Sx = self
    class << C
      $SC = self
      @@a = 1
      class_variable_set :@@ea, 1
    end
    @@b = 2
    class_variable_set :@@eb, 2
  end
  @@c = 3
  class_variable_set :@@em, 3
end

p M.class_variables.sort
p $SC.class_variables.sort
p $Sx.class_variables.sort
");
            }, @"
[""@@a"", ""@@b"", ""@@c"", ""@@em""]
[""@@ea""]
[""@@eb""]
");
        }

        public void AllowedSingletons1() {
            AssertOutput(delegate() {
                CompilerTest(@"
ok = ['x', true, false, nil, //]
error = [1 << 70, 1, 1.0, :foo]

ok.each { |x| def x.foo; end }

error.each { |x| 
  begin
    def x.foo; end
  rescue
  else
    raise 
  end
}

puts 'ok'
");
            }, @"ok");
        }
    }
}
