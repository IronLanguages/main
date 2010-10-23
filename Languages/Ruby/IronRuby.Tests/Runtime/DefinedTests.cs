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

namespace IronRuby.Tests {
    public partial class Tests {
        public void DefinedOperator_Globals1() {
            AssertOutput(delegate() {
                CompilerTest(@"
p defined? $+
alias $plus $+
p defined? $+
p defined? $plus
"
                    );
            }, @"
nil
nil
""global-variable""
");
        }

        public void DefinedOperator_Globals2() {
            AssertOutput(delegate() {
                CompilerTest(@"
alias $foo $bar
p defined? $foo
p defined? $bar
$foo = nil
p defined? $foo
p defined? $bar
$foo = 1
p defined? $foo
p defined? $bar
"
                    );
            }, @"
nil
nil
""global-variable""
""global-variable""
""global-variable""
""global-variable""
");
        }

        public void DefinedOperator_Methods1() {
            CompilerTest(@"
module M
  def foo; end
end

module N
  def foo_defined?
     $o1 = defined? foo                  
     undef foo rescue $o2 = 1            
  end
end

class C
  include M,N    
end

C.new.foo_defined?
");
            object o = Context.GetGlobalVariable("o1");
            AssertEquals(o.ToString(), "method");

            o = Context.GetGlobalVariable("o2");
            AssertEquals(o, 1);
        }

        public void DefinedOperator_Methods2() {
            AssertOutput(() => CompilerTest(@"
def foo
  puts 'foo'
  self
end

puts defined? 1.+
puts defined? 1.method_that_doesnt_exist
puts defined? raise.foo
puts defined? foo.foo(puts(1))                  # foo is private
public :foo
puts defined? foo.foo
"), @"
method
nil
nil
foo
nil
foo
method
");
        }

        public void DefinedOperator_Constants1() {
            AssertOutput(delegate() {
                CompilerTest(@"
W = 0

module M
  X = 1
end

module N
  Y = 2
  def cs_defined?
     puts defined? ::W
     puts defined? ::Undef
     puts defined? M::X
     puts defined? M::Undef
     puts defined? X           
     puts defined? Y
     puts defined? Z
  end
end

class C
  include M,N    
  Z = 3
end

C.new.cs_defined?
");
            }, @"
constant
nil
constant
nil
nil
constant
nil
");
        }

        public void DefinedOperator_Constants2() {
            TestOutput(@"
module M
  C = 1
end

def foo
  M
end

class << Object
  def const_missing name
    puts ""missing #{name}""
    M
  end
end

puts defined?(X::C)
puts defined?(raise::C)
puts defined?(foo::C)
", @"
missing X
constant
nil
constant
");
        }

        public void DefinedOperator_Constants3() {
            AssertOutput(() => CompilerTest(@"
print '1' unless defined? FOO
print '2' unless defined? FOO
print '.' unless not (defined? Object and defined? FOO)
print '.' unless defined? FOO or defined? Object
print '.' if defined? FOO
print '.' if defined? FOO
print '7' if not (defined? Object and defined? FOO)
print '8' if defined? FOO or defined? Object
", 2, 0), @"
1278
");
        }

        public void DefinedOperator_Expressions1() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts defined? true
puts defined? false
puts defined? self

puts defined? 1
puts defined? 'foo'

puts defined?((1+3)+1)

puts defined? x = 1
puts defined? x &= 1
puts defined? x &&= 1
");
            }, @"
true
false
self
expression
expression
method
assignment
assignment
expression
");
        }

        public void DefinedOperator_InstanceVariables1() {
            AssertOutput(delegate() {
                CompilerTest(@"
p defined? @x
@x = nil
p defined? @x
@x = 1
p defined? @x
");
            }, @"
nil
""instance-variable""
""instance-variable""
");
        }

        public void DefinedOperator_ClassVariables1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  def foo; @@foo = 1; end
  def foo_defined_on_M?
     puts defined? @@foo                  
  end
end

module N
  def foo_defined?
     puts defined? @@foo                  
  end
end

class C
  include M,N    
end

c = C.new
c.foo
c.foo_defined?
c.foo_defined_on_M?
");
            }, @"
nil
class variable
");
        }

        public void DefinedOperator_ClassVariables2() {
            AssertOutput(delegate() {
                CompilerTest(@"
@@x = 1
puts defined? @@x
");
            }, @"
class variable
");
        }

        public void DefinedOperator_Yield1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  defined? yield
end

puts foo
puts foo {}
");
            }, @"
nil
yield
");
        }

        public void DefinedOperator_Locals1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def bob a
  x = 1
  1.times { |y|
    z = 2
    puts defined? w, defined? x, defined? y, defined? z, defined? a
  }
end

bob 1
");
            }, @"
nil
local-variable
local-variable
local-variable
local-variable
");
        }

        public void DefinedOperator_Super1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    defined?(super(puts(1)))
  end
end

class D < C
  def foo
    defined?(super(puts(1)))
  end

  def bar
    defined?(super(puts(1)))
  end
end

puts C.new.foo
puts D.new.foo
puts D.new.bar
");
            }, @"
nil
super
nil
");
        }
    }
}
