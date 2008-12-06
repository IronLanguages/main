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

        public void Scenario_MethodUndef1() {
            AssertOutput(delegate() {
                CompilerTest(@"
module M
  def m1
    puts 'M::m1'
  end
  
  def m2
    puts 'M::m2'
  end
  
  def w
    puts 'M::w'
  end
  
  def u
    undef :w
  end
end

module N
 def m1
    puts 'N::m1'
  end
  
  def m2
    puts 'N::m2'
  end
end


class C
  include M

  def c1
    puts 'c1'
  end
  
  def c2
    puts 'c2'
  end
  
  undef :d2 rescue puts $!
end

class E < C
  undef m1
end

class D < E
  include N
  
  def d1
    puts 'd1'
  end
  
  def d2
    puts 'd2'
  end
  
  def w
    puts 'D::w'
  end
  
  def method_missing name, *a
    puts ""missing: #{name}""
  end
end

c = C.new
d = D.new

c.m1
c.m2
c.c1
c.c2
d.m1
d.m2
d.c1
d.c2
d.d1
d.d2
d.u
d.w
");
            }, @"
undefined method `d2' for class `C'
M::m1
M::m2
c1
c2
N::m1
N::m2
c1
c2
d1
d2
D::w
");
        }

        public void Scenario_MethodUndef2() {
            Context.DefineGlobalVariable("o", 0);
            CompilerTest(@"
class C
  def foo
  end

  undef foo rescue $o += 1
  undef foo rescue $o += 10    # blows up here
end
");
            AssertEquals(Context.GetGlobalVariable("o"), 10);
        }

        public void MethodUndefExpression() {
            AssertOutput(delegate() {
                CompilerTest(@"
def u; end
p ((undef u))
", 1, 0); 
            }, "nil");
        }

        /// <summary>
        /// Undef (unlike alias) doesn't look up the method in Object.
        /// </summary>
        public void UndefMethodLookup() {
            AssertOutput(delegate() {
                CompilerTest(@"
module Kernel; def m_kernel; end; end
class Object;  def m_object; end; end

module N
  def m_n
  end

  module M
    undef m_object rescue puts '!undef m_object'
    undef m_kernel rescue puts '!undef m_kernel'
    undef m_n rescue puts '!undef m_n'
  end
end
");
            }, @"
!undef m_object
!undef m_kernel
!undef m_n
");
        }
    }
}
