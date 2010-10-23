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
using IronRuby.Runtime;

namespace IronRuby.Tests {
    public partial class Tests {
        public void SimpleCall1() {
            TestOutput(@"
puts nil
", 
            @"nil");
        }

        public void SimpleCall2() {
            AssertExceptionThrown<ArgumentException>(delegate {
                CompilerTest(@"
def foo a,c=1,*e
end

foo
");
            });

        }

        public void SimpleCall3() {
            TestOutput(@"
y = nil
puts y

x = 123
puts x
", @"
nil
123
");

        }

        /// <summary>
        /// LambdaExpression gets converted to a wrapper.
        /// </summary>
        public void SimpleCall4() {
            TestOutput(@"
def foo a,b,c,d,e,f,g,h,i,j
  puts 123
end
foo 1,2,3,4,5,6,7,8,9,10
",
            @"123");
        }

        /// <summary>
        /// CallSite optimization for more arguments than there is Func generic overloads.
        /// </summary>
        public void SimpleCall5() {
            TestOutput(@"
def foo *args
  p args
end
foo 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,*[28,29,30]
",
@"[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30]");
        }

        public void SimpleCall6() {
            AssertOutput(delegate {
                Engine.CreateScriptSourceFromString(@"
class A
end

class B < A
end

B.new.foo rescue 0

class A
  def foo
    puts 'foo'
  end
end

B.new.foo
").ExecuteProgram();
            }, @"foo");
        }

        public void MethodParams1() {
            TestOutput(@"
def foo a,b,c=:c,d=:d,*e,f,g
  p [a,b,c,d,e,f,g]
end

foo rescue p $!
foo 1 rescue p $!
foo 1,2 rescue p $!
foo 1,2,3 rescue p $!
foo 1,2,3,4
foo 1,2,3,4,5
foo 1,2,3,4,5,6
foo 1,2,3,4,5,6,7
foo 1,2,3,4,5,6,7,8
foo 1,2,3,4,5,6,7,8,9
foo(*[1,2,3,4])
foo(1,*[2,3,4])
foo(1,*[2,3],*[4,5,6,7])
foo(1,*[2,3,4,5,6,7,8,9,10,11,12])
", @"
#<ArgumentError: wrong number of arguments (0 for 4)>
#<ArgumentError: wrong number of arguments (1 for 4)>
#<ArgumentError: wrong number of arguments (2 for 4)>
#<ArgumentError: wrong number of arguments (3 for 4)>
[1, 2, :c, :d, [], 3, 4]
[1, 2, 3, :d, [], 4, 5]
[1, 2, 3, 4, [], 5, 6]
[1, 2, 3, 4, [5], 6, 7]
[1, 2, 3, 4, [5, 6], 7, 8]
[1, 2, 3, 4, [5, 6, 7], 8, 9]
[1, 2, :c, :d, [], 3, 4]
[1, 2, :c, :d, [], 3, 4]
[1, 2, 3, 4, [5], 6, 7]
[1, 2, 3, 4, [5, 6, 7, 8, 9, 10], 11, 12]
");

            TestOutput(@"
def []=(a=:a,b=:b,*c,d,e)
  p [a,b,c,d,e]
end

self[1,2,3,4,5] = 6
self[1,2,*[3,4,5,6]] = 7
self[1,*[]] = 2
", @"
[1, 2, [3, 4], 5, 6]
[1, 2, [3, 4, 5], 6, 7]
[:a, :b, [], 1, 2]
");

            TestOutput(@"
def []=(a=:a,b=:b,c,d)
  p [a,b,c]
end

self[1,2,3] = 6
self[1,*[2],*[]] = 6
self[1,*[],*[]] = 6
", @"
[1, 2, 3]
[1, :b, 2]
[:a, :b, 1]
");
        }
        
        public void MethodCallCaching1() {
            AssertOutput(() => CompilerTest(@"
module N
  def foo
    print 1
  end
end

module M
end

class A
  include N
  include M
end

A.new.foo

module M
  def foo
    print 2
  end
end

A.new.foo
"), 
"12");
        }

        public void MethodCallCaching2() {
            TestOutput(@"
module M
end

class C
  include M
end

module N
  def foo
    puts 'foo'
  end
end

module M
  include N
end

C.new.foo rescue puts 'error'

class C
  include M
end

C.new.foo
",
@"
error
foo
");
        }

        /// <summary>
        /// A method defined in a module is overridden by another module's method.
        /// </summary>
        public void MethodCallCaching3() {
            AssertOutput(() => CompilerTest(@"
module N0; def f; 0; end; end
module N1; def f; 1; end; end
module N2; def f; 2; end; end

class C; end
class D < C; include N2; end

print D.new.f                                   # cache N2::f in a dynamic site

class C
  include N0, N1, N2                            # def in N1 should invalidate site bound to def in N2
                                                # def in N0 shouldn't prevent invalidation
end

print C.new.f
"),
@"20");
        }

        /// <summary>
        /// method_missing
        /// </summary>
        public void MethodCallCaching_MethodMissing1() {
            LoadTestLibrary();
            
            AssertOutput(() => CompilerTest(@"
class A
  def method_missing name; name; end
end
class B < A
end
class C < B
end

puts C.new.h   
v1 = TestHelpers.get_class_version(C)   

class B
  def g; 'g:B'; end
end

v2 = TestHelpers.get_class_version(C)   

puts C.new.g

class B
  def h; 'h:B'; end
end

v3 = TestHelpers.get_class_version(C)   

puts C.new.h   
puts v1 == v2, v2 == v3

class B
  remove_method(:h)
  remove_method(:g)
end

puts C.new.g
puts C.new.h
"),
@"
h
g:B
h:B
true
false
g
h
");
        }

        /// <summary>
        /// method_missing
        /// </summary>
        public void MethodCallCaching_MethodMissing2() {
            AssertOutput(() => CompilerTest(@"
class A
  def method_missing name; name.to_s + ':A'; end
end
class B < A
end
class C < B
end

puts C.new.f

class B
  def method_missing name; name.to_s + ':B'; end 
end

puts C.new.f

class B
  remove_method :method_missing
end

puts C.new.f

class A
  remove_method :method_missing
end

C.new.f rescue puts 'error'
"),
@"
f:A
f:B
f:A
error
");
        }

        /// <summary>
        /// method_missing
        /// </summary>
        public void MethodCallCaching_MethodMissing3() {
            AssertOutput(() => CompilerTest(@"
class A
  def f; 'f:A' end
end
class B < A
end
class C < B
end

puts C.new.f

class B
  def method_missing name; name.to_s + ':B'; end 
end

puts C.new.f

class A
  remove_method :f
end

puts C.new.f
"),
@"
f:A
f:A
f:B
");
        }

        public void MethodCallCaching_MethodMissing4() {
            TestOutput(@"
class C
end

class D < C
  def method_missing(*args)
    puts 'mm'
  end
end

d = D.new
d.foo

class C
  def foo
    puts 'foo'
  end
end

d.foo
",
@"
mm
foo
");
        }

        public void MethodCallCaching_MethodMissing5() {
            Engine.Execute(@"
obj = Object.new
class << obj
  def method_missing(*args)
    raise
  end  
end

obj.f rescue nil

module Kernel
  def f
  end
end

obj.f");
        }

        /// <summary>
        /// Checks that if the same site is used twice and the first use failes on parameter conversion the second use is not affected.
        /// </summary>
        public void MethodCallCaching7() {
            TestOutput(@"
'hello'.send(:slice, nil) rescue puts 'error'
puts 'hello'.send(:slice, 1)
", @"
error
e
");

        }

        /// <summary>
        /// Caching of lookup failures on CLR types must distinguish between static and instance methods.
        /// </summary>
        public void MethodCallCaching8() {
            TestOutput(@"
System::Decimal.+(2) rescue puts 'error'
p System::Decimal.new(1).+(1) rescue p $!
", @"
error
2 (Decimal)
");
        }

        public void Send1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  def foo *a
    puts ""C::foo *#{a.inspect}, &#{block_given?}""
  end
  
  alias []= :send
end

x = C.new
q = lambda {}

x.send :foo
x.send :foo, &q
x.send :foo, &nil
x.send :foo, 1
x.send :foo, 1, &q
x.send :foo, 1, &nil
x.send :foo, 1, 2
x.send :foo, 1, 2, &q
x.send :foo, 1, 2, &nil
x.send :foo, 1, 2, 3
x.send :foo, 1, 2, 3, &q
x.send :foo, 1, 2, 3, &nil

x.send *[:foo, 1,2,3]
x.send :foo, 1, *[2,3], &q
x[:foo,*[1,2]] = 3
x[*:foo] = 1
x[] = :foo
", 1, 0);
            }, @"
C::foo *[], &false
C::foo *[], &true
C::foo *[], &false
C::foo *[1], &false
C::foo *[1], &true
C::foo *[1], &false
C::foo *[1, 2], &false
C::foo *[1, 2], &true
C::foo *[1, 2], &false
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &true
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &false
C::foo *[1, 2, 3], &true
C::foo *[1, 2, 3], &false
C::foo *[1], &false
C::foo *[], &false
");
        }

        /// <summary>
        /// Send propagates the current scope.
        /// </summary>
        public void Send2() {
             AssertOutput(delegate {
                CompilerTest(@"
class C
  public
  send :private
  
  def foo
  end   
  
  p C.private_instance_methods(false)
end
");
             }, @"
[:foo]
");
        }

        public void AttributeAccessors1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  attr_accessor :foo
  alias :bar :foo
  alias :bar= :foo=
end

x = C.new
x.foo = 123
x.bar = x.foo + 1
puts x.foo, x.bar
");
            }, @"
124
124
");
        }

        public void AttributeAccessors2() {
            AssertOutput(() => CompilerTest(@"
class C
  attr_accessor :foo
end

x = C.new
p x.send('foo=', 123)
p x.send('foo')

"), @"
123
123
");
        }

        public void AttributeAccessors3() {
            AssertOutput(() => CompilerTest(@"
class C
  attr_accessor :foo 
  
  alias set_foo foo=
end

c = C.new

c.foo = 1
c.set_foo(*[2])
c.set_foo(3,4) rescue p $!
p c.foo(*[])
p c.foo(1) rescue p $!
"), @"
#<ArgumentError: wrong number of arguments (2 for 1)>
2
#<ArgumentError: wrong number of arguments (1 for 0)>
");
        }

        public void MethodAdded1() {
            AssertOutput(delegate {
                CompilerTest(@"
class Module
  def method_added name    
    puts name
  end
end
");
            }, @"
method_added
");
        }

        public void VisibilityCaching1() {
            AssertOutput(delegate {
                CompilerTest(@"
class C
  def foo
    puts 'foo'
  end

  def method_missing name
    puts 'mm'
  end 
end

x = C.new
4.times do |i|
  $i = i
  class C
    case $i
      when 0; private :foo
      when 1; public :foo
      when 2; private :foo
    end
  end
  x.foo  
end
");
            }, @"
mm
foo
mm
mm
");
        }

        public void VisibilityCaching2() {
            AssertOutput(() => CompilerTest(@"
class B
  def method_missing name; name; end
end

class C < B
  private
  def foo; 'foo:C'; end  
end

class D < C
end

puts D.new.foo

class D
  def foo; 'foo:D'; end  
end

class B
  remove_method :method_missing
end

puts D.new.foo
D.new.bar rescue puts 'error'
"), @"
foo
foo:D
error
");
        }

        public void Visibility1() {
            AssertOutput(() => CompilerTest(@"
class A
  def foo
  end
end

module M
  def bar
  end
end

class A
  include M
  
  alias_method :foo, :bar
  
  p instance_method(:foo)           # DeclaringModule of foo should be M
  
  private :foo                      # should make a copy of foo that rewrites the current foo, not define a super-forwarder
  
  alias_method :xxx, :foo           # should find foo
end
"), @"
#<UnboundMethod: A(M)#foo>
");
        }

        /// <summary>
        /// public/private/protected define a super-forwarder - a method that calls super.
        /// </summary>
        public void Visibility2A() {
            TestOutput(@"
class A
  private
  def foo
    puts 'A::foo'
  end
end

class B < A
end

class C < B
  public :foo      # declaring module of D#foo is A 
  
  p instance_method(:foo) 
end

class A
  remove_method(:foo)
end

class B
  private
  def foo
    puts 'B::foo'
  end
end

C.new.foo
", @"
#<UnboundMethod: C(A)#foo>
B::foo
");
        }

        public void Visibility2B() {
            TestOutput(@"
module M 
  def foo; puts 'ok'; end
end

class A
  include M
  private :foo
end

class B < A
  include M
  public :foo
end

B.new.foo
", @"
ok
");

            TestOutput(@"
module N 
  def foo; puts 'ok'; end
end

class D
  include N
  private :foo
  public :foo
end

D.new.foo
", @"
ok
");
        }

        public void Visibility2C() {
            TestOutput(@"
module M
  private
  def foo; puts 1; end
end

module N 
  include M
  public :foo               # defines a public super-forwarder that remembers to forward to 'foo'
end

module O
  include N                         
  alias bar foo             # stores N#foo public super-forwarder under name 'bar' to O's m-table
  module_function :foo      # defines a module-function that is a copy of M#foo, not N#foo super-forwarder
end

module M
  def foo; puts 2; end           
end

O.foo                       # this works, hence m-f foo is a copy of the real method

class C 
  include O
end
C.new.bar                   # invokes the new M#foo method - alias didn't make a copy of original M#foo and super-forwarder forwards to 'foo'
", @"
1
2
");

        }
        
        /// <summary>
        /// Protected visibility and singletons.
        /// </summary>
        public void Visibility3() {
            AssertOutput(() => CompilerTest(@"
c = class C; new; end

class << c
  protected
  def foo; end
end

c.foo rescue p $!
"), @"
#<NoMethodError: protected method `foo' called for #<C:*>>
", OutputFlags.Match);
        }

        /// <summary>
        /// Protected visibility + caching.
        /// </summary>
        public void Visibility4() {
            AssertOutput(() => CompilerTest(@"
class C
  protected
  def foo
    puts 'foo'
  end
end

class D < C
  def method_missing name
    puts 'mm'
  end
end

class X; end

c,d,x = C.new,D.new,X.new

# test visibility caching:
2.times do
  [[d,c], [c,d], [x,c], [x,d]].each do |s,r| 
    s.instance_eval { r.foo } rescue p $! 
  end
end
"), @"
foo
foo
#<NoMethodError: protected method `foo' called for #<C:*>>
mm
foo
foo
#<NoMethodError: protected method `foo' called for #<C:*>>
mm
", OutputFlags.Match);
        }

        public void ModuleFunctionVisibility1() {
            TestOutput(@"
module M
  private
  def f
  end
  
  module_function :f
end

p M.singleton_methods(false)
p M.private_instance_methods(false)
p M.public_instance_methods(false)
", @"
[:f]
[:f]
[]
");            
        }

        /// <summary>
        /// module_function/private/protected/public doesn't copy a method that is already private/private/protected/public.
        /// </summary>
        public void ModuleFunctionVisibility2() {
            TestOutput(@"
module A
  private
  def pri; end
  protected
  def pro; end
  public
  def pub; end
end

module B
  include A
  module_function :pri
  private :pri
  protected :pro
  public :pub
  
  p private_instance_methods(false)
  p protected_instance_methods(false)
  p public_instance_methods(false)
  p singleton_methods(false)
end
", @"
[]
[]
[]
[:pri]
");
        }

        /// <summary>
        /// define_method copies given method and sets its visibility according to the the current scope flags.
        /// </summary>
        public void DefineMethodVisibility1() {
            TestOutput(@"
class A
  def foo
    puts 'foo'
  end
end

class B < A
  private
  define_method(:foo, instance_method(:foo)) 
end

B.new.foo rescue p $!

class A
  remove_method :foo
end

B.new.send :foo
", @"
foo
foo
");
        }

#if OBSOLETE
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void DefineMethodVisibility2A() {
            Test_DefineMethodVisibility2();
        }
#endif

        public void DefineMethodVisibility2B() {
            Test_DefineMethodVisibility2();
        }

        public void Test_DefineMethodVisibility2() {
            TestOutput(@"
module M                                              # the inner module is the same module we call define_method on
  1.times do  
    module_function
    M.send :define_method, :a, lambda { }              
    private
    M.send :define_method, :b, lambda { }              
  end
end

module N                                               # the inner module different from the one we call define_method on
  1.times do  
    module_function
    M.send :define_method, :c, lambda { }              
    private
    M.send :define_method, :d, lambda { }               
  end
end

p M.public_instance_methods(false).sort
p M.private_instance_methods(false).sort
", @"
[:a, :b, :c, :d]
[]
");
        }

        /// <summary>
        /// alias, alias_method ignore the current scope visibility flags and copy methods with their visibility unmodified.
        /// </summary>
        public void AliasedMethodVisibility1() {
            TestOutput(@"
class A
  def pub; end
  private
  def pri; end
  protected
  def pro; end
end

class B < A
  private
  alias a_pub pub
  protected
  alias a_pri pri
  public
  alias a_pro pro
  
  p public_instance_methods(false).sort
  p private_instance_methods(false).sort
  
  private
  alias_method :am_pub, :pub
  protected
  alias_method :am_pri, :pri
  public
  alias_method :am_pro, :pro
                       
  p public_instance_methods(false).sort
  p private_instance_methods(false).sort
end
", @"
[:a_pub]
[:a_pri]
[:a_pub, :am_pub]
[:a_pri, :am_pri]
");
        }

        public void AttributeAccessorsVisibility1() {
            TestOutput(@"
class C
  1.times {                                       # we need to use visibility flags of the module scope
    private
    attr_accessor :foo
  }   
 
  m = private_instance_methods(false)
  p m.include?(:foo), m.include?(:foo=)
end
", @"
true
true
");
        }
        
        private string MethodDefinitionInDefineMethodCode1 = @"
class A
  $p = lambda { def foo; end }
end

class B
  define_method :f, &$p    
end

B.new.f

puts A.send(:remove_method, :foo) rescue puts B.send(:remove_method, :foo)
";

        public void MethodDefinitionInDefineMethod1A() {
            AssertOutput(() => CompilerTest(MethodDefinitionInDefineMethodCode1), "A");
        }

#if OBSOLETE
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void MethodDefinitionInDefineMethod1B() {
            AssertOutput(() => CompilerTest(MethodDefinitionInDefineMethodCode1), "B");
        }
#endif

        private string MethodDefinitionInDefineMethodCode2 = @"
class B
  define_method :m do    
    def foo; end
  end
end

class A < B
end

A.new.m

puts A.send(:remove_method, :foo) rescue puts B.send(:remove_method, :foo)
";
        public void MethodDefinitionInDefineMethod2A() {
            AssertOutput(() => CompilerTest(MethodDefinitionInDefineMethodCode2), "B");
        }

#if OBSOLETE
        /// <summary>
        /// MRI 1.8 actually prints A. We consider it a bug that we won't copy.
        /// </summary>
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void MethodDefinitionInDefineMethod2B() {
            AssertOutput(() => CompilerTest(MethodDefinitionInDefineMethodCode2), "B");
        }
#endif

        private string MethodDefinitionInModuleEvalCode = @"
class A
  $p = lambda { def foo; end }
end

class B
  module_eval(&$p)
end

puts A.send(:remove_method, :foo) rescue puts B.send(:remove_method, :foo)
";

        public void MethodDefinitionInModuleEval1A() {
            AssertOutput(() => CompilerTest(MethodDefinitionInModuleEvalCode), "B");
        }

#if OBSOLETE
        [Options(Compatibility = RubyCompatibility.Ruby186)]
        public void MethodDefinitionInModuleEval1B() {
            AssertOutput(() => CompilerTest(MethodDefinitionInModuleEvalCode), "B");
        }
#endif

        public void Scenario_ModuleOps_Methods() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def ifoo
    puts 'ifoo'
  end
end

class << C
  $C1 = self
  
  def foo
    puts 'foo'
  end
end

class C
  alias_method(:bar,:foo) rescue puts 'Error 1'
  instance_method(:foo) rescue puts 'Error 2'
  puts method_defined?(:foo)
  foo
  
  alias_method(:ibar,:ifoo)
  instance_method(:ifoo)
  puts method_defined?(:ifoo)
  ifoo rescue puts 'Error 3'
  
  remove_method(:ifoo)
end

C.new.ifoo rescue puts 'Error 4'
C.new.ibar
");
            }, @"
Error 1
Error 2
false
foo
true
Error 3
Error 4
ifoo
");
        }

        public void Methods1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def foo a,b
    puts a + b
  end
end

class D < C
end

c = C.new
p m = c.method(:foo)
p u = m.unbind
p n = u.bind(D.new)

m[1,2]
n[1,2]
");
            }, @"
#<Method: C#foo>
#<UnboundMethod: C#foo>
#<Method: D(C)#foo>
3
3
");
        }

        public void MethodDef1() {
            TestOutput(@"
2.times do |i|
  def foo a
    puts a
  end

  foo i
end
", @"
0
1
");
        }
    }
}
