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

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Builtins;
using Microsoft.Scripting.Math;
using System.Runtime.InteropServices;

namespace InteropTests.Generics1 {
    public class C {
        public virtual int Arity { get { return 0; } }
    }

    public class C<T> {
        public virtual int Arity { get { return 1; } }
    }

    public class C<T,S> {
        public virtual int Arity { get { return 2; } }
    }

    public class D : C {
        public override int Arity { get { return 10; } }
    }

    public class D<T> : C<T> {
        public override int Arity { get { return 11; } }
    }
}

namespace IronRuby.Tests {
    public partial class Tests {        
#pragma warning disable 169 // private field not used
        public class ClassWithFields {
            public int Field = 1;
            public readonly int RoField = 2;

            public static int StaticField = 3;
            public const int ConstField = 4;
        }
#pragma warning restore 169

        public void ClrFields1() {
            Context.DefineGlobalVariable("obj", new ClassWithFields());

            AssertOutput(delegate() {
                CompilerTest(@"
C = $obj.class

puts $obj.field
puts $obj.RoField

$obj.field = 10
puts $obj.field

($obj.RoField = 20) rescue puts $!.class

puts C.static_field
puts C.const_field

C.static_field = 30
puts C.static_field

(C.const_field = 40) rescue puts $!.class
");
            }, @"
1
2
10
NoMethodError
3
4
30
NoMethodError
");
        }

        public class ClassWithMethods1 {
            public int RoProperty { get { return 3; } }
            public int RwProperty { get { return 4; } set { } }
            public int WoProperty { set { } }
            
            public int M() { return 1; }
            public static int StaticMethod() { return 2; }
        }

        public void ClrMethods1() {
            Context.DefineGlobalVariable("obj", new ClassWithMethods1());
            Context.DefineGlobalVariable("cls", typeof(ClassWithMethods1));

            AssertOutput(delegate() {
                CompilerTest(@"
C = $obj.class

puts $obj.m
puts C.static_method
puts $cls.static_methods rescue puts $!.class

($obj.RoProperty = 10) rescue puts $!.class
$obj.WoProperty = 20 
$obj.RwProperty = 30 

puts $obj.RoProperty
puts $obj.WoProperty rescue puts $!.class
puts $obj.RwProperty

");
            }, @"
1
2
NoMethodError
NoMethodError
3
NoMethodError
4
");
        }

        /// <summary>
        /// Order of initialization of CLR methods.
        /// </summary>
        public void ClrMethods2() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithMethods1)));
            XTestOutput(@"
class C
  def m; 2; end     # replace CLR method with Ruby method before we use the CLR one
  remove_method :m  # remove Ruby method
  new.m rescue p $! # we shouldn't call the CLR method 
end
", @"
");
        }

        public class ClassWithEquals1 {
            // define overload of Equals so that we get a method group with mixed instance and static methods inherited from Object's method group
            public static bool Equals(object o1, object o2, object o3) { return o1.Equals(o2); }
        }

        /// <summary>
        /// Mixing instance and static methods - instance Object::Equals(Object), static Object::Equals(Object, Object).
        /// </summary>
        public void ClrMethods3() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithEquals1)));
            TestOutput(@"
puts 1.Equals(2)                # instance
puts 1.Equals(2,3) rescue p $!  # static invoked with instance

puts C.Equals(C)                # instance invoked on a class (statically)
puts C.Equals(3,3)              # static
puts C.Equals(3,3,4)            # overload
", @"
false
#<ArgumentError: wrong number of arguments (2 for 1)>
true
true
true
");
        }

        /// <summary>
        /// Builtin types only expose CLR methods under unmangled names (mangling is no applied).
        /// </summary>
        public void ClrMethods4() {
            TestOutput(@"
a = Exception.new
p a.method(:data) rescue p $!
p a.method(:Data)
", @"
#<NameError: undefined method `data' for class `Exception'>
#<Method: Exception#Data>
");
        }

        public void ClrMembers1() {
            TestOutput(@"
a = [1,2,3]
p m = a.clr_member(:count)
p m[]

p m = a.clr_member(:binary_search)
p m.clr_members.size
p m.overloads(Object).clr_members
", @"
#<Method: Array#count>
3
#<Method: Array#binary_search>
3
[Int32 BinarySearch(System.Object)]
");

            TestOutput(@"
class C < Array
end

p C.new.clr_member(:get_enumerator).call.move_next
", @"
false
");
        }

        public class ProtectedA {
            protected string Foo(int a) { return "Foo(I): " + a; }
            public string Bar(int a) { return "Bar(I): " + a; }

            protected string PG<T>(T a) { return "PG<T>(T)"; }
        }

        public class ProtectedB : ProtectedA {
            public string Foo(object a) { return "Foo(O): " + a.ToString(); }
            internal protected string Bar(object a) { return "Bar(O): " + a; }

            protected int Prop1 { get; set; }
            public int Prop2 { get; internal protected set; }

            private string Baz(int a) { return "Baz(I): " + a; }
            public string Baz(object a) { return "Baz(O): " + a; }

            protected static string StaticM() { return "StaticM"; }
            protected static string StaticGenericM<T>(T f) { return "StaticGenericM: " + f.ToString(); }

            internal protected string PG<T>(T a, int b) { return "PG<T>(T,int)"; }

            // TODO:
            // protected int Fld;
            // protected static int Fld;
            // protected event System.Func<object> Evnt;
        }

        public void ClrVisibility1() {
            Debug.Assert(!Engine.Runtime.Setup.PrivateBinding);
            Context.ObjectClass.SetConstant("A", Context.GetClass(typeof(ProtectedA)));
            Context.ObjectClass.SetConstant("B", Context.GetClass(typeof(ProtectedB)));

            // methods:
            AssertOutput(delegate() {
                CompilerTest(@"
class C < B; end
a, b, c = A.new, B.new, C.new

puts c.foo(1)
puts b.foo(2)
puts b.bar(22)
a.foo(3) rescue p $!

class A
  def foo; 4; end
  def bar; 5; end
end
puts c.foo(6)
b.bar(7) rescue p $!

B.StaticM rescue p $!
puts C.StaticM
puts C.method(:StaticGenericM).of(Fixnum)[123]
");
            }, @"
Foo(I): 1
Foo(O): 2
Bar(I): 22
#<NoMethodError: CLR protected method `foo' called for *ProtectedA*>
Foo(O): 6
#<NoMethodError: CLR protected method `bar' called for *ProtectedB*>
#<NoMethodError: CLR protected method `StaticM' called for *ProtectedB*>
StaticM
StaticGenericM: 123
", OutputFlags.Match);

            // generic methods:
            TestOutput(@"
class C < B; end
c = C.new

puts c.method(:PG).of(Fixnum).call(1)
puts c.method(:PG).of(Fixnum).call(1,2)
", @"
PG<T>(T)
PG<T>(T,int)
");

            // properties:
            AssertOutput(delegate() {
                CompilerTest(@"
class C < B; end
a, b, c = A.new, B.new, C.new

c.prop1 = 1
c.prop2 = 2
puts c.prop1
puts c.prop2
(b.prop2 = 10) rescue p $!
puts b.prop2
");
            }, @"
1
2
#<NoMethodError: CLR protected method `prop2=' called for *ProtectedB*>
0
", OutputFlags.Match);
        }

        [Options(PrivateBinding = true)]
        public void ClrVisibility2() {
            Debug.Assert(Engine.Runtime.Setup.PrivateBinding);
            if (_driver.PartialTrust) return;
            Context.ObjectClass.SetConstant("A", Context.GetClass(typeof(ProtectedA)));
            Context.ObjectClass.SetConstant("B", Context.GetClass(typeof(ProtectedB)));

            // methods, properties:
            TestOutput(@"
class C < B; end
a, b, c = A.new, B.new, C.new

puts a.foo(3)

class A
  def bar; 5; end
end
puts b.bar(7)
puts b.baz(1)

b.prop2 = 10
puts b.prop2
",
@"
Foo(I): 3
Bar(O): 7
Baz(I): 1
10
");
        }

        public static class OverloadInheritance1 {
            public class A {
                public string Foo(int a, int b, int c) {
                    return "Foo: " + a.ToString() + ", " + b.ToString() + ", " + c.ToString();
                }

                public string Skip() {
                    return "Skip";
                }
            }

            public class B : A {
                public string Foo(int a) {
                    return "Foo: " + a.ToString();
                }

                public virtual string Foo(int a, int b) {
                    return "Foo: " + a.ToString() + ", " + b.ToString();
                }

                public string Bar(int a) {
                    return "Bar: " + a.ToString();
                }

                public string Hidden(int a) {
                    return "Hidden: " + a.ToString();
                }

                public string Middle(int a) {
                    return "Middle: " + a.ToString();
                }
            }

            public class C : B {
                public new string Foo(int a) {
                    return "NewFoo: " + a.ToString();
                }

                public override string Foo(int a, int b) {
                    return "OverriddenFoo: " + a.ToString() + ", " + b.ToString();
                }

                public string Bar(int a, int b) {
                    return "Bar: " + a.ToString() + ", " + b.ToString();
                }

                public string Hidden(int a, int b) {
                    return "Hidden: " + a.ToString() + ", " + b.ToString();
                }

                public string Skip(int a) {
                    return "Skip: " + a.ToString();
                }
            }
        }
        
        public void ClrOverloadInheritance1() {
            Context.ObjectClass.SetConstant("Obj", new OverloadInheritance1.C());

            AssertOutput(() => CompilerTest(@"
puts Obj.foo(1)
puts Obj.foo(1, 2)
puts Obj.foo(1, 2, 3)
puts Obj.bar(1)
puts Obj.bar(1, 2)
puts Obj.middle(1)
puts Obj.skip
puts Obj.skip(1)
Obj.GetHashCode
"), @"
NewFoo: 1
OverriddenFoo: 1, 2
Foo: 1, 2, 3
Bar: 1
Bar: 1, 2
Middle: 1
Skip
Skip: 1
");
            
            AssertOutput(() => CompilerTest(@"
p Obj.method(:foo)
p Obj.method(:bar)
p Obj.method(:middle)
p Obj.method(:skip)
"), @"
#<Method: *C#foo>
#<Method: *C#bar>
#<Method: *C(*B)#middle>
#<Method: *C#skip>
", OutputFlags.Match);

            // hides Hidden method when called using mangled name "hidden":
            Context.GetClass(typeof(OverloadInheritance1.B)).HideMethod("hidden");

            AssertOutput(() => CompilerTest(@"
puts Obj.hidden(1) rescue puts 'error'
puts Obj.Hidden(1)
puts Obj.hidden(1, 2)
puts Obj.Hidden(1, 2)
"), @"
error
Hidden: 1
Hidden: 1, 2
Hidden: 1, 2
");
        }

        /// <summary>
        /// No CLR names should be returned for builtin types and singletons.
        /// </summary>
        public void ClrMethodEnumeration1() {
            // built-ins:
            var irModules = new[] { "IronRuby" };

            using (Context.ClassHierarchyLocker()) {
                Context.ObjectClass.EnumerateConstants((module, name, value) => {
                    RubyModule m = value as RubyModule;
                    if (m != null && Array.IndexOf(irModules, m.Name) == -1) {
                        AssertNoClrNames(ModuleOps.GetInstanceMethods(m, true), m.Name);
                        AssertNoClrNames(ModuleOps.GetPrivateInstanceMethods(m, true), m.Name);
                        AssertNoClrNames(ModuleOps.GetInstanceMethods(m.SingletonClass, true), m.Name);
                        AssertNoClrNames(ModuleOps.GetPrivateInstanceMethods(m.SingletonClass, true), m.Name);
                    }
                    return false;
                });
            }

            // singletons:
            AssertNoClrNames(Engine.Execute(@"class << self; instance_methods + private_instance_methods; end"), null);
            AssertNoClrNames(Engine.Execute(@"class << self; class << self; instance_methods + private_instance_methods; end; end"), null);
            AssertNoClrNames(Engine.Execute(@"class << Class; instance_methods + private_instance_methods; end"), null);
        }

        public void ClrMethodEnumeration2() {
            TestOutput(@"
class System::Decimal
  instance_methods(false).each do |name|
    mangled = '__' + name
    
    alias_method(mangled, name)
    private mangled
    
    define_method(name) do |*args|
      puts ""method called: #{name}""
      send mangled, *args
    end
  end
end
x, y = System::Decimal.new(1), System::Decimal.new(2)
x + y       
x.CompareTo(y)
", @"
method called: +
method called: compare_to
");
        }

        private void AssertNoClrNames(object/*!*/ methods, string moduleName) {
            var array = (RubyArray)methods;
            int idx = array.FindIndex((name) => name is ClrName);
            Assert(idx == -1, moduleName + "::" + (idx == -1 ? null : ((ClrName)array[idx]).ActualName));
        }

        public static class OverloadInheritance2 {
            public class A { public virtual string f(int a) { return "f1"; } }
            public class B : A { }
            public class C : B { }
            public class D : C { public virtual string f(int a, int b) { return "f2"; } }
            public class E : D { }
            public class F : E { public virtual string f(int a, int b, int c, int d) { return "f4"; } }
            public class G : F { }

            public class X : B { public virtual string f(int a, int b, int c) { return "f3"; } }
            public class Y : X { }
            
            public static void Load(RubyContext/*!*/ context) {
                context.ObjectClass.SetConstant("A", context.GetClass(typeof(A)));
                context.ObjectClass.SetConstant("B", context.GetClass(typeof(B)));
                context.ObjectClass.SetConstant("C", context.GetClass(typeof(C)));
                context.ObjectClass.SetConstant("D", context.GetClass(typeof(D)));
                context.ObjectClass.SetConstant("E", context.GetClass(typeof(E)));
                context.ObjectClass.SetConstant("F", context.GetClass(typeof(F)));
                context.ObjectClass.SetConstant("G", context.GetClass(typeof(G)));
                context.ObjectClass.SetConstant("X", context.GetClass(typeof(X)));
                context.ObjectClass.SetConstant("Y", context.GetClass(typeof(Y)));
            }
        }

        /// <summary>
        /// Dynamic site and group caching.
        /// </summary>
        public void ClrOverloadInheritance2() {
            OverloadInheritance2.Load(Context);

            // method definition hides overloads:
            AssertOutput(() => CompilerTest(@"
puts E.new.f(1,2)                     # marks D::f2 and A::f1 as used in d.s.

class C; def f; 'f:C'; end; end       # overrides A::f1 => invalidates all 'f'-groups in subtree of C

puts E.new.f(1) rescue puts 'error'   # recreates D::f2 => A::f1 not visible
puts E.new.f(1,2)                     # D::f still visible => marked as used in d.s.
"), @"
f2
error
f2
");

            // module inclusion hides overloads:
            AssertOutput(() => CompilerTest(@"
puts Y.new.f(1)                            

module M; def f; 'f:M' end; end
class X; include M; end               # hides A::f1, but not X::f3

puts Y.new.f(1) rescue puts 'error' 
puts Y.new.f(1,2,3)
"), @"
f1
error
f3
");
        }

        public void ClrOverloadInheritance3() {
            OverloadInheritance2.Load(Context);

            // method definition hides overloads:
            AssertOutput(() => CompilerTest(@"
p D.instance_method(:f).clr_members.collect { |x| x.to_string }  # creates groups in A and D that are not used in d.s.

class B; def f; 'f:B'; end; end                                  # hides A::f1

p D.instance_method(:f).clr_members.collect { |x| x.to_string }  # only one overload should be present in the group
"), @"
['System.String f(Int32)', 'System.String f(Int32, Int32)']
['System.String f(Int32, Int32)']
");
        }
        
        public void ClrOverloadInheritance4() {
            OverloadInheritance2.Load(Context);

            AssertOutput(() => CompilerTest(@"
puts D.new.f(1)
puts D.new.f(1,2)
class B; 
  def f; 'f:B'; end
end
puts D.new.f(1) rescue puts 'error'
puts D.new.f(1,2)
class B
  remove_method :f                                # f not used in DS, DS needs to be invalidated though 
end
puts D.new.f(1)
puts D.new.f(1,2)
"), @"
f1
f2
error
f2
f1
f2
");
        }

        /// <summary>
        /// Removing an overload barrier.
        /// </summary>
        public void ClrOverloadInheritance5() {
            OverloadInheritance2.Load(Context);

            AssertOutput(() => CompilerTest(@"
puts E.new.f(1)

class C; def f; 'f:C'; end; end

E.new.f(1) rescue puts 'error'
puts E.new.f(1,2)                              
puts G.new.f(1,2,3,4)                          # group created after C:f barrier defined

C.send :remove_method, :f

puts G.new.f(1)
puts E.new.f(1)
"), @"
f1
error
f2
f4
f1
f1
");
        }

        public class OverloadInheritance6 {
            public class A {
                public int foo(int a) { return 1; }
                public int Foo(int a) { return 2; }
            }

            public class B : A {
                public int Foo(short a) { return 3; }
            }

            public class C : B {
                public int foo(bool a) { return 4; }
            }

            public class D : C {
                public int Foo(double a) { return 5; }
            }
            
            public static void Load(RubyContext/*!*/ context) {
                context.ObjectClass.SetConstant("A", context.GetClass(typeof(A)));
                context.ObjectClass.SetConstant("B", context.GetClass(typeof(B)));
                context.ObjectClass.SetConstant("C", context.GetClass(typeof(C)));
                context.ObjectClass.SetConstant("D", context.GetClass(typeof(D)));
            }
        }

        /// <summary>
        /// Method group should include methods of both casings.
        /// It might depend on the order of method calls what overloads are available otherwise.
        /// D.new.foo finds Foo and 
        ///   - includes [foo(double), foo(int)] into the group if C.new.foo was invoked previously
        ///   - includes [Foo(bool)] into the group otherwise.
        /// </summary>
        public void ClrOverloadInheritance6() {
            OverloadInheritance6.Load(Context);
            
            TestOutput(@"
p C.new.method(:foo).clr_members.size
p D.new.method(:foo).clr_members.size
p A.new.method(:foo).clr_members.size
p B.new.method(:foo).clr_members.size
", @"
4
5
2
3
");

            TestOutput(@"
p C.new.method(:Foo).clr_members.size
p D.new.method(:Foo).clr_members.size
p A.new.method(:Foo).clr_members.size
p B.new.method(:Foo).clr_members.size
", @"
2
3
1
2
");

            // prefer overload whose name matches the call site exactly:
            TestOutput(@"
p A.new.foo(1)
", @"
1
");

        }

        // TODO: CLR overloads
        // - alias/pri/pub/pro/mf/dm/generics/explicit-overload

        public class ClassWithIndexer1 {
            public int[,] Values = new int[,] { {0, 10}, {20, 30} };

            public int this[int i, int j] { get { return Values[i,j]; } set { Values[i,j] = value; } }
        }

        public void ClrIndexers1() {
            Context.ObjectClass.SetConstant("CI", Context.GetClass(typeof(ClassWithIndexer1)));
            
            // default indexers:
            AssertOutput(() => CompilerTest(@"
c = CI.new
c[0,1] += 1
p c[0, 1]
"), @"
11
");
            // non-default indexers:
            // TODO: We need to use VB or generate IL to test this.
            // TODO: improve access
            //   If a property accessor with parameters is called without arguments the result is a PropertyAccessor object with [], []= defined.
            //   Then the calls could look like c.foo[1,2] = 3. 

//            AssertOutput(() => CompilerTest(@"
//c = CI.new
//c.method(:foo=).call(1, 0, c.method(:foo).call(1, 0) + 5)
//p c.method(:foo).call(1, 0)
//"), @"
//25
//");
        }

#pragma warning disable 67 // event not used
        public class GenericMethods {
            public static string M0<T>() {
                return "M0<" + typeof(T).Name + ">()";
            }

            public static string M1() {
                return "M1()";
            }
            
            public static string M1<T>() {
                return "M1<" + typeof(T).Name + ">()";
            }

            public static string M1<S, T>() {
                return "M1<" + typeof(S).Name + ", " + typeof(T).Name + ">()";
            }

            public static string M1<T>(int foo) {
                return "M1<" + typeof(T).Name + ">(Fixnum)";
            }

            public static string M2(int foo) {
                return "M2(Fixnum)";
            }

            public static string M2<T>(int foo) {
                return "M2<" + typeof(T).Name + ">(Fixnum)";
            }

            public static int Field;
            public static object Property { get; set; }
            public static event Action<Object> Event;
        }
#pragma warning restore

        public void ClrGenericMethods1() {
            Context.ObjectClass.SetConstant("GM", Context.GetClass(typeof(GenericMethods)));
            AssertOutput(() => CompilerTest(@"
m = GM.method(:M1)
puts m.call
puts m.of().call
puts m.of(String).call
puts m.of(String, Fixnum).call
puts m.call(1) rescue p $!
puts m.of(String, String, String) rescue p $!
"), @"
M1()
M1()
M1<MutableString>()
M1<MutableString, Int32>()
#<ArgumentError: wrong number of arguments (1 for 0)>
#<ArgumentError: wrong number of generic arguments for `M1'>
"
            );

            AssertOutput(() => CompilerTest(@"
m = GM.method(:M2)
puts m.call(1)

puts GM.method(:field).of(Fixnum) rescue p $!
puts GM.method(:property).of(Fixnum) rescue p $!
puts GM.method(:event).of(Fixnum) rescue p $!
"), @"
M2(Fixnum)
#<ArgumentError: wrong number of generic arguments for `field'>
#<ArgumentError: wrong number of generic arguments for `property'>
#<ArgumentError: wrong number of generic arguments for `event'>
"
            );
        }

        public class OverloadedMethods {
            public static string M1(RubyScope scope) {
                return "M1()";
            }

            public static string M1(RubyContext context, MutableString foo) {
                return "M1(String)";
            }

            public static string M1(BinaryOpStorage storage, RubyContext context, double foo) {
                return "M1(Float)";
            }

            public static string M1(int foo, MutableString bar) {
                return "M1(Fixnum, String)";
            }

            public static string M1(MutableString foo, MutableString bar) {
                return "M1(String, String)";
            }

            public static string M1(int foo, params object[] bar) {
                return "M1(Fixnum, Object*)";
            }

            public static string M1(int foo, object bar) {
                return "M1(Fixnum, Object)";
            }

            public static string M1(int foo) {
                return "M1(Fixnum)";
            }


            public static string M2(int foo) {
                return "M2(Fixnum)";
            }

            public static string M2(object foo) {
                return "M2(Object)";
            }

            public static string M2<T>(int foo) {
                return "M2<" + typeof(T).Name + ">(Fixnum)";
            }

            public static string M2<T>(object foo) {
                return "M2<" + typeof(T).Name + ">(Object)";
            }
        }
        
        public void ClrOverloadSelection1() {
            Context.ObjectClass.SetConstant("OM", Context.GetClass(typeof(OverloadedMethods)));

            AssertOutput(() => CompilerTest(@"
m = OM.method(:M1)
puts m.overloads.call
puts m.overloads(String).call('')
puts m.overloads(Float).call(1.0)
puts m.overloads(Fixnum, String).call(1, '')
puts m.overloads(String, String).call('', '')
puts m.overloads(Fixnum, System::Array.of(Object)).call(1, 2, 3)
puts m.overloads(Fixnum, Object).call(1, 2)
puts m.overloads(Fixnum).call(1)
"), @"
M1()
M1(String)
M1(Float)
M1(Fixnum, String)
M1(String, String)
M1(Fixnum, Object*)
M1(Fixnum, Object)
M1(Fixnum)
"
            );

            AssertOutput(() => CompilerTest(@"
m = OM.method(:M2)
puts m.clr_members.size
puts m.of.clr_members.size
puts m.of(Object).clr_members.size
puts m.overloads(Object).clr_members.size
puts m.of(Object).overloads(Object).clr_members.size
puts m.overloads(Object).of(Object).clr_members.size
"), @"
4
2
2
2
1
1
"
            );

            AssertOutput(() => CompilerTest(@"
m = OM.method(:M2)
puts m.call(1)
puts m.of(Float).call('')
puts m.of(Float).overloads(Fixnum).call(1)
puts m.overloads(Object).of(String).call(1)
"), @"
M2(Fixnum)
M2<Double>(Object)
M2<Double>(Fixnum)
M2<MutableString>(Object)
"
            );

            AssertOutput(() => CompilerTest(@"
m = OM.method(:M2)
m.overloads(Object, String) rescue p $!
m.overloads() rescue p $!
"), @"
#<ArgumentError: no overload of `M2' matches given parameter types>
#<ArgumentError: no overload of `M2' matches given parameter types>
"
            );

            // overlods called on a Ruby method
            AssertOutput(() => CompilerTest(@"
def foo a,b
  [a,b]
end

def bar a,*b
  [a,*b]
end

p method(:foo).overloads(Object, Object).call(1,2)
method(:foo).overloads(Object) rescue p $!

p method(:bar).overloads(Object).call(1)
p method(:bar).overloads(Object, Object).call(1,2)
p method(:bar).overloads(Object, Object, Object).call(1,2,3)
"), @"
[1, 2]
#<ArgumentError: no overload of `foo' matches given parameter types>
[1]
[1, 2]
[1, 2, 3]
"
            );

            // overlods called on a Ruby attribute accessor
            AssertOutput(() => CompilerTest(@"
class C
  attr_reader :foo
  attr_writer :foo
  
  c = new
  c.foo = 1
  p instance_method(:foo).overloads.bind(c).call
  instance_method(:foo=).overloads(Object).bind(c).call(2)
  p c.foo
end
"), @"
1
2
"
            );
        }

        public static class OverloadSelection2 {
            public class A {
                public A() {
                }

                public A(int a) {
                }

                public A(RubyClass cls, double a) {
                }

                public static int Foo() { return 1; }
                public static int Foo(RubyScope scope, BlockParam block, int a) { return 2; }
            }

            public class B : A {
                public static int Foo(double a) { return 3; }
                public static int Foo(RubyClass cls, string b) { return 4; }
            }

            public static void Load(RubyContext/*!*/ context) {
                context.ObjectClass.SetConstant("A", context.GetClass(typeof(A)));
                context.ObjectClass.SetConstant("B", context.GetClass(typeof(B)));
            }
        }

        public void ClrOverloadSelection2() {
            OverloadSelection2.Load(Context);

            // TODO: constructor overload selection
            // B.overloads(Fixnum).new(1) ???

            // static methods:
            TestOutput(@"
m = B.method(:foo)
puts m.overloads(Fixnum).clr_members.size          # RubyScope and BlockParam are hidden
puts m.overloads(System::String) rescue p $!       # RubyClass is not hidden here
", @"
1
#<ArgumentError: no overload of `foo' matches given parameter types>
");

            // library methods:
            TestOutput(@"
m = method(:print)
puts m.arity
puts m.overloads().arity
puts m.overloads(Object).arity
puts m.overloads(System::Array[Object]).arity
", @"
-1
0
1
-1
");
        }

        public class ClassWithInterfaces1 : IEnumerable {
            public IEnumerator GetEnumerator() {
                yield return 1;
                yield return 2;
                yield return 3;
            }
        }

        public interface InterfaceFoo1 {
            int Foo();
        }

        public interface InterfaceFoo2 {
            int Foo();
        }

        public interface InterfaceBar1 {
            int Bar();
        }

        public interface InterfaceBaz1 {
            int Baz();
        }

        internal class InterfaceClassBase1 {
            // inherited:
            public IEnumerator GetEnumerator() {
                yield return 1;
                yield return 2;
                yield return 3;
            }
        }

        internal class InternalClass1 : InterfaceClassBase1, IEnumerable, IComparable, InterfaceBaz1, InterfaceFoo1, InterfaceFoo2, InterfaceBar1 {
            // simple:
            public int Baz() { return 123; }

            // simple explicit:
            int IComparable.CompareTo(object obj) { return 0; }

            // explicit + implicit
            public int Bar() { return 0;}
            int InterfaceBar1.Bar() { return 0; }

            // multiple explicit with the same signature 
            int InterfaceFoo1.Foo() { return 1; }
            int InterfaceFoo2.Foo() { return 2; }
        }

        public class ClassWithInterfaces2 : ClassWithInterfaces1, IComparable {
            public int CompareTo(object obj) {
                return 0;
            }
        }

        public void ClrInterfaces1() {
            Context.DefineGlobalVariable("obj1", new ClassWithInterfaces1());
            Context.DefineGlobalVariable("obj2", new ClassWithInterfaces2());

            AssertOutput(delegate() {
                CompilerTest(@"
$obj1.each { |x| print x }
puts

$obj2.each { |x| print x }
puts

puts($obj2 <=> 1)
");
            }, @"
123
123
0
");
        }

        /// <summary>
        /// Calling (explicit) interface methods on internal classes.
        /// A method that is accessible via any interface should be called. 
        /// If there is more than one then regular overload resolution should kick in.
        /// 
        /// </summary>
        public void ClrInterfaces2() {
            Context.ObjectClass.SetConstant("Inst", new InternalClass1());
            Context.ObjectClass.SetConstant("InterfaceFoo1", Context.GetModule(typeof(InterfaceFoo1)));
            Context.ObjectClass.SetConstant("InterfaceFoo2", Context.GetModule(typeof(InterfaceFoo2)));
            Context.ObjectClass.SetConstant("InterfaceBar1", Context.GetModule(typeof(InterfaceBar1)));

            // TODO: explicit interface impl
            TestOutput(@"
p Inst.GetEnumerator().nil?
#p Inst.CompareTo(nil)
#p Inst.Bar                               
#p Inst.as(InterfaceFoo1).Foo   # or Inst.interface_method(InterfaceFoo1, :Foo).call ?
#p Inst.as(InterfaceFoo2).Foo
#p Inst.as(InterfaceBar1).Bar
", @"
false
");
        }

        /// <summary>
        /// Type represents a class object - it is equivalent to RubyClass.
        /// </summary>
        public void ClrTypes1() {
            TestTypeAndTracker(typeof(ClassWithMethods1));
            TestTypeAndTracker(ReflectionCache.GetTypeTracker(typeof(ClassWithMethods1)));
        }

        public void TestTypeAndTracker(object type) {
            Context.DefineGlobalVariable("type", type);
            AssertOutput(delegate() {
                CompilerTest(@"
puts $type
puts $type.to_s
puts $type.to_class
puts $type.to_module
puts $type.to_class.static_method
puts $type.static_method rescue puts $!.class
");
            }, String.Format(@"
{0}
{0}
{1}
{1}
2
NoMethodError
", type, Context.GetTypeName(typeof(ClassWithMethods1), false)), OutputFlags.Match);
        }

        public void ClrNamespaces1() {
            TestOutput(@"
puts defined? System::Collections

module System
  remove_const(:Collections)
end

puts defined? System::Collections

class System::Collections
  puts self
end
", @"
constant
nil
System::Collections
");
        }
        
        public void ClrGenerics1() {
            Runtime.LoadAssembly(typeof(Tests).Assembly);

            TestOutput(@"
include InteropTests::Generics1
p C
p D
p C.new
p D.new                               # test if we don't use cached dispatch to C.new again
p C.clr_new
p D.clr_new
p C.superclass
p D.superclass
", @"
#<TypeGroup: InteropTests::Generics1::C, InteropTests::Generics1::C[T], InteropTests::Generics1::C[T, S]>
#<TypeGroup: InteropTests::Generics1::D, InteropTests::Generics1::D[T]>
InteropTests.Generics1.C
InteropTests.Generics1.D
InteropTests.Generics1.C
InteropTests.Generics1.D
Object
InteropTests::Generics1::C
");

            TestOutput(@"
include InteropTests::Generics1
p C.new.arity
p C[String].new.arity
p D[String].new.arity
p C[Fixnum, Fixnum].new.arity
p D[String]
p C[Fixnum, Fixnum]
", @"
0
1
11
2
InteropTests::Generics1::D[String]
InteropTests::Generics1::C[Fixnum, Fixnum]
");
        }

        public class ClassWithNestedGenericTypes1 {
            public class D {
            }

            public class C {
                public int Id { get { return 0; } }
            }

            public class C<T> {
                public int Id { get { return 1; } }
            }
        }

        public class ClassWithNestedGenericTypes2 : ClassWithNestedGenericTypes1 {
            public new class C<T> {
                public int Id { get { return 2; } }
            }

            public class C<T, S> {
                public int Id { get { return 3; } }
            }
        }

        public void ClrGenerics2() {
            Context.ObjectClass.SetConstant("C1", Context.GetClass(typeof(ClassWithNestedGenericTypes1)));
            Context.ObjectClass.SetConstant("C2", Context.GetClass(typeof(ClassWithNestedGenericTypes2)));

            AssertOutput(() => CompilerTest(@"
p C1::D
p C1::C
p C2::C
"), @"
*::ClassWithNestedGenericTypes1::D
#<TypeGroup: *::ClassWithNestedGenericTypes1::C, *::ClassWithNestedGenericTypes1::C[T]>
#<TypeGroup: *::ClassWithNestedGenericTypes2::C[T], *::ClassWithNestedGenericTypes2::C[T, S]>
", OutputFlags.Match);
        }

        /// <summary>
        /// Require, namespaces.
        /// </summary>
        public void ClrRequireAssembly1() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
puts System::AppDomain.current_domain.friendly_name
");
            }, AppDomain.CurrentDomain.FriendlyName);
        }

        /// <summary>
        /// CLR class re-def and inclusions.
        /// </summary>
        public void ClrInclude1() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
include System::Collections
class ArrayList
  puts self.object_id == System::Collections::ArrayList.object_id
end
");
            }, "true");
        }

        /// <summary>
        /// Instantiation.
        /// </summary>
        public void ClrNew1() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
b = System::Text::StringBuilder.new
b.Append 1
b.Append '-'
b.Append true
puts b.to_string
puts b.length
");
            }, @"
1-True
6
");
        }

        public void ClrNew2() {
            TestOutput(@"
puts c = Thread.clr_ctor
puts c.overloads(System::Threading::ThreadStart).call(lambda { puts 'hello' }).status
puts Thread.clr_new(System::Threading::ThreadStart.new(lambda { puts 'hello' })).status
", @"
#<Method: Class(Thread)#.ctor>
unstarted
unstarted
");
        }

        /// <summary>
        /// Alias.
        /// </summary>
        public void ClrAlias1() {
            AssertOutput(delegate() {
                CompilerTest(@"
require 'mscorlib'
include System::Collections

class ArrayList
  alias :old_add :Add
  def foo x
    old_add x
  end 
end

a = ArrayList.new
a.Add 1
a.add 2
a.foo 3
puts a.count");
            }, "3");
        }

        public class ClassWithEnum {
            public enum MyEnum {
                Foo = 1, Bar = 2, Baz = 3
            }

            public MyEnum MyProperty { get; set; }
        }

        public void ClrEnums1() {
            Context.DefineGlobalVariable("obj", new ClassWithEnum());
            Context.DefineGlobalVariable("enum", typeof(ClassWithEnum.MyEnum));

            AssertOutput(delegate() {
                CompilerTest(@"
E = $enum.to_class

$obj.my_property = E.foo
puts $obj.my_property

$obj.my_property = E.bar
puts $obj.my_property
");
            }, @"
Foo
Bar        
");
        }

        [Flags]
        public enum FlagsInt { A = 1, B = 2 }

        [Flags]
        public enum FlagsULong : ulong { C = Int64.MaxValue, D = 2 }

        [Flags]
        public enum FlagsByte : byte { N = 0, E = 4, F = 8 }
        
        public void ClrEnums2() {
            Context.ObjectClass.SetConstant("A", FlagsInt.A);
            Context.ObjectClass.SetConstant("B", FlagsInt.B);
            Context.ObjectClass.SetConstant("C", FlagsULong.C);
            Context.ObjectClass.SetConstant("D", FlagsULong.D);
            Context.ObjectClass.SetConstant("E", FlagsByte.E);
            Context.ObjectClass.SetConstant("F", FlagsByte.F);

            AssertOutput(delegate() {
                CompilerTest(@"
p(x = A | B, x.class)
p(x = A | E, x.class) rescue puts $!
p(x = C ^ D, x.class) 
p(x = E & F, x.class) 
p(x = ~C, x.class)
");
            }, @"
A, B
*::FlagsInt
wrong argument type *::FlagsByte (expected *::FlagsInt)
9223372036854775805
*::FlagsULong
N
*::FlagsByte
9223372036854775808
*::FlagsULong
", OutputFlags.Match);
        }

        public delegate int Delegate1(string foo, int bar);

        public void ClrDelegates1() {
            Context.DefineGlobalVariable("delegateType", typeof(Delegate1));
            CompilerTest(@"
D = $delegateType.to_class
$d = D.new { |foo, bar| $foo = foo; $bar = bar; 777 }
");
            object d = Context.GetGlobalVariable("d");
            Assert(d is Delegate1);
            AssertEquals(((Delegate1)d)("hello", 123), 777);
            AssertEquals(Context.GetGlobalVariable("foo"), "hello");
            AssertEquals(Context.GetGlobalVariable("bar"), 123);
        }
        
        public void ClrDelegates2() {
            Runtime.LoadAssembly(typeof(Func<>).Assembly);
            Runtime.LoadAssembly(typeof(Action).Assembly);

            var f = Engine.Execute<Func<int, int>>(@"System::Func.of(Fixnum, Fixnum).new { |a| a + 1 }");
            Assert(f(1) == 2);

            Engine.Execute<Action>(@"System::Action.new { $x = 1 }")();
            Assert((int)Context.GetGlobalVariable("x") == 1);

            Engine.Execute<Action<int>>(@"System::Action[Fixnum].new { |x| $x = x + 1 }")(10);
            Assert((int)Context.GetGlobalVariable("x") == 11);
        }

        public void ClrEvents1() {
            // TODO:
            if (_driver.PartialTrust) return;

            AssertOutput(delegate() {
                CompilerTest(@"
require 'System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
require 'System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

Form = System::Windows::Forms::Form

f = Form.new

x = 'outer var'

f.shown do |sender, args|
    puts x
    sender.close
end

f.Text = 'hello'
f.BackColor = System::Drawing::Color.Green

System::Windows::Forms::Application.run f
");
            }, "outer var");
        }

        public class ClassWithEvents {
            public event Action<string, string> OnFoo;

            public bool Foo() {
                if (OnFoo != null) {
                    OnFoo("hello", "world");
                    return true;
                } else {
                    return false;
                }
            }
        }

        // TODO: method comparison
        public void ClrEvents2() {
            Context.DefineGlobalVariable("e", new ClassWithEvents());
            AssertOutput(() => CompilerTest(@"
def handler a,b
  puts ""handler: #{a} #{b}""
end

h = method(:handler)

$e.on_foo.add(h)
puts $e.foo
$e.on_foo.remove(h)
puts $e.foo
"), @"
handler: hello world
true
false
");
        }

        public void ClrEvents3() {
            Context.DefineGlobalVariable("e", new ClassWithEvents());
            AssertOutput(() => CompilerTest(@"
h = $e.on_foo do |a,b|
  puts ""handler: #{a} #{b}""
end

p h.class

puts $e.foo
$e.on_foo.remove(h)
puts $e.foo
"), @"
Proc
handler: hello world
true
false
");
        }

        public void ClrEvents4() {
            Context.DefineGlobalVariable("e", new ClassWithEvents());
            AssertOutput(() => CompilerTest(@"
handler = lambda do |a,b|
  puts ""handler: #{a} #{b}""
end

p $e.on_foo(&handler) == handler

puts $e.foo
$e.on_foo.remove(handler)
puts $e.foo
"), @"
true
handler: hello world
true
false
");
        }

        public class ClassWithVirtuals {
            public virtual string M1() {
                return "CM1 ";
            }

            public virtual string M2() {
                return "CM2 ";
            }

            public virtual string P1 {
                get { return "CP1 "; }
            }

            private string _p2 = "CP2 ";

            public virtual string P2 {
                get { return _p2; }
                set { _p2 = value; }
            }

            public virtual string P3 {
                get { return "CP3 "; }
            }

            public string Summary {
                get { return M1() + M2() + P1 + P2 + P3; }
            }
        }

        public void ClrOverride1() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithVirtuals)));
            TestOutput(@"
class D < C
  def m1                  # mangled name works
    'RM1 '
  end

  def p2= value
  end

  def P3                  # unmangled name works as well
    'RP3 '
  end
end

c = C.new
c.p2 = 'RP2 '
puts c.summary

d = D.new
d.p2 = 'RP2 '
puts d.summary
", @"
CM1 CM2 CP1 RP2 CP3 
RM1 CM2 CP1 CP2 RP3 
");
        }

        /// <summary>
        /// Virtual methods of built-in types cannot be overridden.
        /// </summary>
        public void ClrOverride2() {
            var e = Engine.Execute<Exception>(@"
class E < Exception
  def data; 123; end
  new
end
");
            Assert(e.Data is IDictionary);

            var obj = Engine.Execute(@"
class C < Object
  def equals(other); raise; end
  new
end
");
            Assert(obj.Equals(obj));
        }

        public class ClassCallingVirtualInCtor1 {
            public ClassCallingVirtualInCtor1() {
                VirtualMethod();
            }

            public virtual int VirtualMethod() {
                return 1;
            }
        }

        /// <summary>
        /// We need to fully initialize the derived type before calling base ctor.
        /// The ebase ctor can call virtual methods that require _class to be set.
        /// </summary>
        public void ClrOverride3() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassCallingVirtualInCtor1)));
            TestOutput(@"
class D < C
end

p D.new.virtual_method
", @"
1
");
        }

        /// <summary>
        /// Super call in an override.
        /// </summary>
        public void ClrOverride4() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassCallingVirtualInCtor1)));
            TestOutput(@"
class D < C
  def virtual_method 
    10 + super
  end
end

class E < C
  def VirtualMethod 
    20 + super
  end
end

p D.new.VirtualMethod
p E.new.virtual_method
", @"
11
21
");
        }

        public class ClassWithNonEmptyConstructor {
            public int P { get; set; }

            public ClassWithNonEmptyConstructor() {
            }

            public ClassWithNonEmptyConstructor(BinaryOpStorage storage, RubyScope scope, RubyClass self, string p) {
            }

            public ClassWithNonEmptyConstructor(RubyContext context, int i) {
                P = i;
            }

            public ClassWithNonEmptyConstructor(RubyContext context, RubyClass cls1, RubyClass cls2) {
            }
        }

        public class ClassWithNonEmptyConstructor2 {
            public int P { get; set; }

            public ClassWithNonEmptyConstructor2() {
                P = 0;
            }

            public ClassWithNonEmptyConstructor2(RubyClass cls) {
                P = 1;
            }

            public ClassWithNonEmptyConstructor2(RubyContext context) {
                P = 2;
            }
        }

        public class ExceptionWithDefaultConstructor1 : Exception {
            public ExceptionWithDefaultConstructor1() {
            }
        }

        public class ExceptionWithStdConstructors1 : Exception {
            public int P { get; set; }
            public ExceptionWithStdConstructors1() {
                P = 0;
            }

            public ExceptionWithStdConstructors1(string message) : base(message) {
                P = 1;
            }
        }

        public class ClassWithNoDefaultConstructor {
            public string P { get; set; }

            public ClassWithNoDefaultConstructor(string p) {
                P = p;
            }
        }

        private static bool IsCtorAvailable(RubyClass cls, params Type[] parameters) {
            var method = cls.GetUnderlyingSystemType().GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
            return method != null && !method.IsPrivate && !method.IsFamilyAndAssembly;
        }

        public void ClrConstructor1() {
            Context.ObjectClass.SetConstant("C1", Context.GetClass(typeof(ClassWithNonEmptyConstructor)));

            var cls1 = Engine.Execute<RubyClass>("class D1 < C1; self; end");
            Assert(IsCtorAvailable(cls1, typeof(RubyClass)));
            Assert(IsCtorAvailable(cls1, typeof(BinaryOpStorage), typeof(RubyScope), typeof(RubyClass), typeof(string)));
            Assert(IsCtorAvailable(cls1, typeof(RubyClass), typeof(int)));
            Assert(IsCtorAvailable(cls1, typeof(RubyClass), typeof(RubyClass), typeof(RubyClass)));

            Context.ObjectClass.SetConstant("C2", Context.GetClass(typeof(ClassWithNonEmptyConstructor2)));
            var cls2 = Engine.Execute<RubyClass>("class D2 < C2; self; end");
            Assert(IsCtorAvailable(cls2, typeof(RubyClass)));
            Assert(!IsCtorAvailable(cls2, typeof(RubyContext)));
            Assert(!IsCtorAvailable(cls2));

            Assert(Engine.Execute<int>("D2.new.p") == 1);

            Context.ObjectClass.SetConstant("E1", Context.GetClass(typeof(ExceptionWithDefaultConstructor1)));
            var ex1 = Engine.Execute<RubyClass>("class F1 < E1; self; end");
            Assert(IsCtorAvailable(ex1, typeof(RubyClass)));

            Context.ObjectClass.SetConstant("E2", Context.GetClass(typeof(ExceptionWithStdConstructors1)));
            var ex2 = Engine.Execute<RubyClass>("class F2 < E2; self; end");
            Assert(IsCtorAvailable(ex2, typeof(RubyClass)));
            Assert(IsCtorAvailable(ex2, typeof(RubyClass), typeof(string)));

            Assert(Engine.Execute<int>("F2.new.p") == 1);
        }

        public void ClrConstructor2() {
            Context.ObjectClass.SetConstant("DefaultAndParam", Context.GetClass(typeof(ClassWithNonEmptyConstructor)));
            AssertOutput(delegate() {
                CompilerTest(@"
class D < DefaultAndParam; end

d = D.new 123
puts d.p
");
            }, @"
123
");
        }

        public class ClassWithDefaultAndParamConstructor {
            public string P { get; set; }

            public ClassWithDefaultAndParamConstructor() {
            }

            public ClassWithDefaultAndParamConstructor(string p) {
                P = p;
            }
        }

        public void ClrConstructor3() {
            Context.ObjectClass.SetConstant("DefaultAndParam", Context.GetClass(typeof(ClassWithDefaultAndParamConstructor)));
            Context.ObjectClass.SetConstant("NoDefault", Context.GetClass(typeof(ClassWithNoDefaultConstructor)));
            AssertOutput(delegate() {
                CompilerTest(@"
module I
  def initialize(arg)
    self.p = arg
    puts 'init'
  end
end

class D < DefaultAndParam
  include I
end

class E < NoDefault
  include I
end

class F < NoDefault
  def self.new(*args)
    puts 'ctor'
    super args.join(' ')
  end
end

puts D.new('test').p
E.new('test').p rescue p $!
puts F.new('hello', 'world').p
");
            }, @"
init
test
#<TypeError: can't allocate class `E' that derives from type `*::ClassWithNoDefaultConstructor' with no default constructor; define E#new singleton method instead of I#initialize>
ctor
hello world
", OutputFlags.Match);
        }

        /// <summary>
        /// TODO: Currently we don't narrow results of arithmetic operations to the self-type even if the value fits.
        /// That might by just fine, but in some scenarios (overload resolution) it might be better to narrow the result type.
        /// Shelveset "Numerics-MetaOpsWithNarrowing" contains prototype implementation of the narrowing.
        /// </summary>
        public void ClrPrimitiveNumericTypes1() {
            AssertOutput(delegate() {
                CompilerTest(@"
[System::Byte, System::SByte, System::UInt16, System::Int16, System::UInt32, System::Int64, System::UInt64, System::Single].each_with_index do |t,i|
  p t.ancestors
  p t.new(i).class
  p x = t.new(i) + 1, x.class   
  p t.new(i).size rescue puts 'no size method'
end
");
            }, @"
[System::Byte, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::Byte
1
Fixnum
1
[System::SByte, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::SByte
2
Fixnum
1
[System::UInt16, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::UInt16
3
Fixnum
2
[System::Int16, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::Int16
4
Fixnum
2
[System::UInt32, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::UInt32
5
Fixnum
4
[System::Int64, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::Int64
6
Fixnum
8
[System::UInt64, Integer, Precision, Numeric, Comparable, Object, Kernel]
System::UInt64
7
Fixnum
8
[System::Single, Precision, Numeric, Comparable, Object, Kernel]
System::Single
8.0
Float
no size method
");
        }

        public void ClrArrays1() {
            Context.SetGlobalConstant("A2", Context.GetClass(typeof(int[,])));
            AssertOutput(delegate() {
                CompilerTest(@"
# multi-dim array:
a2 = A2.new(2,4)
a2[0,1] = 123
p a2[0,1]

# vectors:                                                     
p System::Array[Fixnum].new(10) { |i| i + 1 }
p System::Array[Fixnum].new(3)
p System::Array[Fixnum].new([1,2,3])
p System::Array[Fixnum].new(3, 12)

# TODO: conversions:
# p System::Array[System::Char].new(3, 'x')
");
            }, @"
123
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
[0, 0, 0]
[1, 2, 3]
[12, 12, 12]
");
        }
        
        public void ClrChar1() {
            AssertOutput(delegate() {
                CompilerTest(@"
p System::Char.ancestors
p System::String.ancestors

a = System::Array[System::Char].new([System::Char.new('a'), System::Char.new('b')])  # TODO: implicit conversion
p System::Char.new(a)

x = System::Char.new('foo')
p x.size
p x.index('f')
p x + 'oo'
p x == 'f'
p System::Char.new('9').to_i
");
            }, @"
[System::Char, IronRuby::Clr::String, Enumerable, Comparable, System::ValueType, Object, Kernel]
[System::String, IronRuby::Clr::String, Enumerable, Comparable, Object, Kernel]
'a'
1
0
'foo'
true
9
");
        }

        public void ClrOperators1() {
            AssertOutput(delegate() {
                CompilerTest(@"
a = System::Decimal.new(16)
b = System::Decimal.new(4)
p a + b
p a - b
p a / b 
p a * b
p a % b
p a == b
p a != b
p a > b
p a >= b
p a < b
p a <= b
p(-a)
p(+a)
");
            }, @"
20
12
4
64
0
false
true
true
true
false
false
-16
16
");
        }

        public void ClrOperators2() {
            AssertOutput(delegate() {
                CompilerTest(@"
p :b == true                # Symbol hides SymbolId::op_Equality
p String == Fixnum          # Only instance operator calls are allowed (MutableString::op_Equality shound be ignored)

class C < Numeric
  def to_f
    1.2
  end
end

p C.new.ceil                 # Numeric#ceil uses self with DefaultProtocol attribute
");
            }, @"
false
false
2
");
        }

        public class Conversions1 {
            public int F(int? a, int? b) {
                return a ?? b ?? 3;
            }

            public object[] Numerics(byte a, sbyte b, short c, ushort d, int e, uint f, long g, ulong h, BigInteger i, Complex64 j, Convertible1 k) {
                return new object[] { a, b, c, d, e, f, g, h, i, j, k };
            }

            public Delegate Delegate(Func<object, object> d) {
                return d;
            }

            public int Foo(int a) {
                return a + 1;
            }

            public int Double(double a) {
                return (int)a;
            }

            public int FixnumDefaultProtocol([DefaultProtocol]int a) {
                return a + 2;
            }

            public string Bool([Optional]bool a) {
                return a ? "T" : "F";
            }

            public int ListOrString(IList a) {
                return a.Count;
            }

            public int ListOrString([DefaultProtocol]MutableString str) {
                return 0;
            }

            public int ListAndStrings(IList a, MutableString str1) {
                return a.Count + str1.GetCharCount();
            }
        }

        public class Convertible1 {
            public object Value { get; set; }

            public static implicit operator Convertible1(int value) {
                return new Convertible1() { Value = value };
            }

            public static implicit operator int(Convertible1 value) {
                return 11;
            }

            public static implicit operator double(Convertible1 value) {
                return 16.0;
            }

            public static explicit operator string(Convertible1 value) {
                return "foo";
            }

            public static explicit operator MutableString(Convertible1 value) {
                return MutableString.CreateMutable("hello", RubyEncoding.UTF8);
            }

            public override string ToString() {
                return "Convertible(" + Value + ")";
            }
        }

        public void ClrConversions1() {
            Context.SetGlobalConstant("Inst", new Conversions1());
            Context.SetGlobalConstant("Conv", new Convertible1());

            // Nullable<T>
            TestOutput(@"
[[1, 2], [nil, 2], [nil, nil]].each { |a,b| p Inst.f(a,b) }
", @"
1
2
3
");
            // Boolean
            TestOutput(@"
print Inst.Bool(), Inst.Bool(true), Inst.Bool(false), Inst.Bool(nil), Inst.Bool(0), Inst.Bool(Object.new), Inst.Bool(1.4)
", @"
FTFFTTT
");

            // Double
            TestOutput(@"
p Inst.Double(2.0), Inst.Double(4), Inst.Double(System::Byte.new(8)), Inst.Double(Conv), Inst.Double('28.4'), Inst.Double('29.5'.to_clr_string)
", @"
2
4
8
16
28
29
");
            
            // primitive numerics:
            TestOutput(@"
p Inst.numerics(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)
", @"
1
2
3
4
5
6
7
8
9
(10+0j)
Convertible(11)
");

            // protocol conversions:
            TestOutput(@"
class C
  def to_ary
    [1,2,3,4,5]
  end

  def to_str
    'xxx'
  end
end
p Inst.ListOrString(C.new)
p Inst.ListAndStrings(C.new, C.new)
p Inst.FixnumDefaultProtocol(Conv)
", @"
0
8
13
");
            
            // protocol conversions:
            TestOutput(@"
a = System::Collections::ArrayList.new
p Inst.Foo(a) rescue p $!
class System::Collections::ArrayList
  def to_int
    100
  end
end
p Inst.Foo(a) rescue p $!
", @"
#<TypeError: can't convert System::Collections::ArrayList into Fixnum>
101
");
            // meta-object conversions:
            TestOutput(@"
p Inst.delegate(Proc.new { |x| x + 1 }).invoke(123)
", @"
124
");
        }
    }
}
