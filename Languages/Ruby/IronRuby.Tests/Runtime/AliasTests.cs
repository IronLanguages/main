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

        /// <summary>
        /// alias keyword binds lexically while alias_method binds to self.class.
        /// </summary>
        public void Scenario_MethodAliases1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
    def y
		puts 'M::y'
    end
    
	def do_alias
		alias aliased_y y		
	end
	
	def do_alias_method
		self.class.send :alias_method, :method_aliased_y, :y
	end
end

class C
	include M
	
	def y
		puts 'C::y'
	end
end

c = C.new
c.do_alias
c.do_alias_method
c.aliased_y
c.method_aliased_y
");
            }, @"
M::y
C::y
");
        }

        public void Scenario_MethodAliases2() {
            TestOutput(@"
class C
  private
  def foo; end
  
  public
  alias bar foo 
  
  p private_instance_methods(false).sort
  p public_instance_methods(false).sort
  
  public :bar
  
  p private_instance_methods(false).sort
  p public_instance_methods(false).sort
end
", @"
[:bar, :foo]
[]
[:foo]
[:bar]
");
        }

        /// <summary>
        /// Alias (unlike undef) looks up the method in Object.
        /// </summary>
        public void AliasMethodLookup1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module Kernel; def m_kernel; end; end
class Object;  def m_object; end; end

module N
  def m_n
  end

  module M
    alias x m_object rescue puts '!alias m_object'
    alias x m_kernel rescue puts '!alias m_kernel'
    alias x m_n rescue puts '!alias m_n'
  end
end
");
            }, @"
!alias m_n
");
        }

#if OBSOLETE
        /// <summary>
        /// Alias in define_method.
        /// </summary>
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void AliasMethodLookup2() {
            TestOutput(@"
class D
end

class E
  D.send(:define_method, :bar) {
    alias g f rescue p $!
  }
end

d = D.new
d.bar
", @"
<NameError: undefined method `f' for class `D'>
");
        }
#endif

        /// <summary>
        /// Alias in define_method.
        /// </summary>
        public void AliasMethodLookup3() {
            TestOutput(@"
class D
end

class E
  D.send(:define_method, :bar) {
    alias g f rescue p $!
  }
end

d = D.new
d.bar
", @"
#<NameError: undefined method `f' for class `E'>
");
        }

        /// <summary>
        /// "alias" uses the same lookup algorithm as method definition (see RubyScope.GetMethodDefinitionOwner).
        /// </summary>
        public void AliasInModuleEval1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class D
  def foo
    puts 'D::foo'
  end
end

class C
  def foo
    puts 'C::foo'
  end

  D.module_eval {
    alias bar foo
  }
end

D.new.bar
");
            }, @"
D::foo
");
        }

        public void MethodAliasInModuleEval1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class D
  def foo
    puts 'D::foo'
  end
end

class E
  def foo
    puts 'E::foo'
  end
end

class C
  def foo
    puts 'C::foo'
  end

  D.module_eval {
    E.send :alias_method, :bar, :foo
  }
end

E.new.bar
");
            }, @"
E::foo
");
        }

        public void MethodLookup1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module Kernel
  def k__; end
end

class Module
  def m__; end;
end

module MyModule  
end

puts x = ((MyModule.send(:alias_method, :x, :k__); true) rescue false)  
puts x = ((MyModule.send(:alias_method, :x, :m__); true) rescue false)

puts x = ((MyModule.send(:public, :k__); true) rescue false)  
puts x = ((MyModule.send(:public, :m__); true) rescue false)

puts x = ((MyModule.send(:method, :k__); true) rescue false)  
puts x = ((MyModule.send(:method, :m__); true) rescue false)

puts x = ((MyModule.send(:undef_method, :k__); true) rescue false)  
puts x = ((MyModule.send(:undef_method, :m__); true) rescue false)
");
            }, @"
true
false
true
false
true
true
false
false
");
        }

        public void MethodAliasExpression() {
            AssertOutput(delegate() {
                CompilerTest(@"
def b; end
p ((alias a b))
", 1, 0);
            }, "nil");
        }

        public void AliasUndefWithQuotedSymbols() {
            TestOutput(@"
class A
  def ab
  end

  def c
  end

  alias :'d' :'c'  
  
  p instance_methods(false).sort
  
  undef :""a#{'b'}"", :'c'
  
  p instance_methods(false).sort
end
", @"
[:ab, :c, :d]
[:d]
");
        }
    }
}
