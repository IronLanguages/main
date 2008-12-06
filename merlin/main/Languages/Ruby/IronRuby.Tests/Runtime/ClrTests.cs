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

namespace IronRuby.Tests.Interop.Generics1 {
    public class C {
        public int Arity { get { return 0; } }
    }

    public class C<T> {
        public int Arity { get { return 1; } }
    }

    public class C<T,S> {
        public int Arity { get { return 2; } }
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

            // TODO:
            protected string ProtectedField;
            private string PrivateField;
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
            
            public int Method() { return 1; }
            public static int StaticMethod() { return 2; }
        }

        public void ClrMethods1() {
            Context.DefineGlobalVariable("obj", new ClassWithMethods1());
            Context.DefineGlobalVariable("cls", typeof(ClassWithMethods1));

            AssertOutput(delegate() {
                CompilerTest(@"
C = $obj.class

puts $obj.method
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

        public class ClassWithMethods2 {
            public string Data;
            
            protected string ProtectedMethod() { return "ProtectedMethod"; }
            private string PrivateMethod() { return "PrivateMethod"; }

            protected string ProtectedProperty { 
                get { return "ProtectedProperty"; } 
                set { Data = value; } 
            }

            private string PrivateProperty { get { return "PrivateProperty"; } set { Data = value; } }

            public string ProtectedGetter { protected get { return "ProtectedGetter"; } set { Data = value; } }
            public string PrivateGetter { private get { return "PrivateGetter"; } set { Data = value; } }
            public string ProtectedSetter { get { return "ProtectedSetter"; } protected set { Data = value; } }
            public string PrivateSetter { get { return "PrivateSetter"; } private set { Data = value; } }

            protected string ProtectedWithPrivateGetter { private get { return "ProtectedWithPrivateGetter"; } set { Data = value; } }
            protected string ProtectedWithPrivateSetter { get { return "ProtectedWithPrivateSetter"; } private set { Data = value; } }
        }

        public void ClrMethodsVisibility1() {
            // TODO: 
            if (_driver.PartialTrust) return;

            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithMethods2)));
            AssertOutput(delegate() {
                CompilerTest(@"
class D < C
  def test name
    self.data = 'none'
    sname = name.to_s    

    begin
      puts 'ok read: ' + send(sname) 
    rescue 
      puts 'error read: ' + sname 
    end

    begin
      send(sname + '=', sname) 
      print 'ok write: ' + sname + ' = '
    rescue
      puts 'error write: ' + sname
    end

    puts self.data
  end
 
  def foo
    puts protected_method
    private_method rescue puts 'error call: private_method'

    test :protected_property
    test :private_property

    test :protected_getter
    test :protected_setter
    test :private_getter
    test :private_setter

    test :protected_with_private_getter    
    test :protected_with_private_setter
  end
end

D.new.foo
");
            }, @"
ProtectedMethod
error call: private_method
ok read: ProtectedProperty
ok write: protected_property = protected_property
error read: private_property
error write: private_property
none
ok read: ProtectedGetter
ok write: protected_getter = protected_getter
ok read: ProtectedSetter
ok write: protected_setter = protected_setter
error read: private_getter
ok write: private_getter = private_getter
ok read: PrivateSetter
error write: private_setter
none
error read: protected_with_private_getter
ok write: protected_with_private_getter = protected_with_private_getter
ok read: ProtectedWithPrivateSetter
error write: protected_with_private_setter
none
");
        }

        public class ClassWithInterfaces1 : IEnumerable {
            public IEnumerator GetEnumerator() {
                yield return 1;
                yield return 2;
                yield return 3;
            }
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
#<{0}:0x*>
#<{0}:0x*>
{1}
{1}
2
NoMethodError
", RubyUtils.GetQualifiedName(type.GetType()), RubyUtils.GetQualifiedName(typeof(ClassWithMethods1))), OutputFlags.Match);
        }
        
        public void ClrGenerics1() {
            Runtime.LoadAssembly(typeof(Tests).Assembly);

            var generics1 = typeof(Interop.Generics1.C).Namespace.Replace(".", "::");

            AssertOutput(delegate() {
                CompilerTest(@"
include " + generics1 + @"
p C
p C.new.arity
p C[String].new.arity
p C[Fixnum, Fixnum].new.arity
");
            }, @"
TypeGroup of C
0
1
2
");
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

            public MyEnum MyProperty { get { return MyEnum.Baz; } set { _p = value; } }
            private MyEnum _p;
        }

        /// <summary>
        /// Enums.
        /// </summary>
        public void ClrEnums1() {
            // TODO:            
            Context.DefineGlobalVariable("obj", new ClassWithEnum());
            Context.DefineGlobalVariable("enum", typeof(ClassWithEnum.MyEnum));

            XAssertOutput(delegate() {
                CompilerTest(@"
E = $enum.to_class

$obj.my_property = E.foo
puts $obj.my_property

$obj.my_property = 1
puts $obj.my_property
");
            }, "TODO");
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

            var f = Engine.Execute<Func<int, int>>(@"
System::Func.of(Fixnum, Fixnum).new { |a| a + 1 }
");

            Assert(f(1) == 2);
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

        public class ClassWithVirtuals {
            public virtual string M1() {
                return "CM1";
            }
            public virtual string M2() {
                return "CM2";
            }
            public virtual string P1 {
                get { return "CP1"; }
            }
            private string _p2 = "CP2";
            public virtual string P2 {
                get { return _p2; }
                set { _p2 = value; }
            }
            public virtual string P3 {
                get { return "CP3"; }
            }
            public string Summary {
                get { return M1() + M2() + P1 + P2 + P3; }
            }
        }

        public void ClrOverride1() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithVirtuals)));
            AssertOutput(delegate() {
                CompilerTest(@"
class D < C
  def m1
    'RM1'
  end
  def p2= value
  end
  def p3
    'RP3'
  end
end

$c = C.new
$c.p2 = 'RP2'
puts $c.summary

$d = D.new
$d.p2 = 'RP2'
puts $d.summary
");
            }, @"
CM1CM2CP1RP2CP3
RM1CM2CP1CP2RP3
");
        }

        public class ClassWithNonEmptyConstructor {
            public string P { get; set; }
            public ClassWithNonEmptyConstructor() {
            }
            public ClassWithNonEmptyConstructor(string p) {
                P = p;
            }
        }

        private static bool IsAvailable(MethodBase method) {
            return method != null && !method.IsPrivate && !method.IsFamilyAndAssembly;
        }

        public void ClrConstructor1() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithNonEmptyConstructor)));
            var cls = Engine.Execute(@"
class D < C; end
D.new
D
");
            var rubyClass = (cls as RubyClass);
            Debug.Assert(rubyClass != null);

            Type baseType = rubyClass.GetUnderlyingSystemType();
            Assert(IsAvailable(baseType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(RubyClass) }, null)));
            Assert(IsAvailable(baseType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(RubyClass), typeof(string) }, null)));
#if !SILVERLIGHT
            Assert(IsAvailable(baseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {
                typeof(SerializationInfo), typeof(StreamingContext) }, null)));
#endif
        }

        public void ClrConstructor2() {
            // TODO: Requires allocator support
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ClassWithNonEmptyConstructor)));
            AssertOutput(delegate() {
                CompilerTest(@"
class D < C; end

$d = D.new 'test'
puts $d.p
");
            }, @"
test
");
        }

    }
}
