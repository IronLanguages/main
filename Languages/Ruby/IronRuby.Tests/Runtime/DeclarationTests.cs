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
namespace IronRuby.Tests {
    public partial class Tests {

        public void Scenario_RubyDeclarations1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  puts 'C'
end
");
            }, "C");}

        public void Scenario_RubyDeclarations1A() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo
    print 'F'
  end
end

C.new.foo
");
            }, "F");
        }

        public void Scenario_RubyDeclarations1B() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def f
    print 'F'
  end
end

class C
  def g
    print 'G'
  end
end

C.new.f
C.new.g
");
            }, "FG");
        }

        public void Scenario_RubyDeclarations1C() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  X = 1
end

class D < C
  Y = 2
end

print D::X, D::Y
");
            }, "12");
        }

        public void Scenario_RubyDeclarations2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
    def foo()
        puts ""This is foo in C.""
    end
    def C.sfoo()
        puts ""This is static foo in C.""
    end
end

class D < C
    def bar()
        puts ""This is bar in D""
        foo
    end
    def D.sbar()
        puts ""This is static bar in D.""
        sfoo
    end
end

def baz()
  puts 'This is baz, a global function'
end

D.sbar()

print 'Hello'
Kernel.print "" world""
puts '!'

D.new.bar
baz()
");
            },
@"This is static bar in D.
This is static foo in C.
Hello world!
This is bar in D
This is foo in C.
This is baz, a global function
");
        }

        public void Scenario_RubyDeclarations3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
    module M
        module N
        end
    end

    module ::Foo
        module X
        end
        module X::Y
        end
    end
end

puts C
puts C::M
puts C::M::N
puts Foo
puts Foo::X
puts Foo::X::Y");
            },
@"
C
C::M
C::M::N
Foo
Foo::X
Foo::X::Y
");
        }
        
        public void Scenario_RubyDeclarations4() {
            AssertExceptionThrown<InvalidOperationException>(() => 
                CompilerTest(@"
class C
  class << self
    class D < self   # error: can't make subclass of virtual class
    end
  end
end
"));

            AssertExceptionThrown<InvalidOperationException>(() =>
                CompilerTest(@"
class Module
  alias mf module_function
end

class C
  mf                # error: module_function must be called for modules
end
"));
        }
    }
}
