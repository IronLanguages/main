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
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using IronRuby.Builtins;
using System.Diagnostics;
using IronRuby.Runtime.Calls;

namespace IronRuby.Tests {
    public partial class Tests {
        public void RubyHosting_DelegateConversions() {
            var lambda = Engine.Execute(@"lambda { |a| a + 1 }");
            var result = Engine.Operations.Invoke(lambda, 5);
            Assert((int)result == 6);

            var func = Engine.Operations.ConvertTo<Func<int, int>>(lambda);
            Assert(func(10) == 11);

            var method = Engine.Execute(@"def foo(a,b); a + b; end; method(:foo)");
            var func2 = Engine.Operations.ConvertTo<Func<int, int, int>>(method);
            Assert(func2(1, 2) == 3);

            Engine.Runtime.Globals.SetVariable("F1", typeof(Func<int, int>));
            var func3 = (Func<int, int>)Engine.Execute(@"F1.to_class.new { |a| a + 3 }");
            Assert(func3(1) == 4);

            var func4 = (Func<int, int>)Engine.Execute(@"F1.to_class.new lambda { |a| a + 4 }");
            Assert(func4(1) == 5);

            var func5 = (Func<int, int>)Engine.Execute(@"F1.to_class.new(*[lambda { |a| a + 5 }])");
            Assert(func5(1) == 6);

            AssertExceptionThrown<ArgumentException>(() => Engine.Execute(@"F1.to_class.new(*[])"));

            if (_driver.RunPython) {
                var py = Runtime.GetEngine("python");

                Engine.Runtime.Globals.SetVariable("F2", typeof(Func<string[], string[], string[]>));
                var pyAdd = py.Execute(@"
def py_add(a, b): 
  return a + b
py_add
");
                Engine.Runtime.Globals.SetVariable("PyAdd", pyAdd);
                var pyFunc = (Func<string[], string[], string[]>)Engine.Execute(@"F2.to_class.new PyAdd");
                Assert(String.Join(";", pyFunc(new[] { "x" }, new[] { "y" })) == "x;y");
            }
        }

        public void RubyHosting1A() {
            ScriptScope scope = Engine.Runtime.CreateScope();
            scope.SetVariable("x", 1);
            scope.SetVariable("y", 2);

            // reads x, y from scope via method_missing;
            // copies z, w main-singleton methods to the scope:
            Engine.Execute("def self.z; x + y; end", scope);
            Engine.Execute("def self.w(a); a + 1; end", scope);

            int z = scope.GetVariable<Func<int>>("z")();
            Assert(z == 3);

            int w = scope.GetVariable<Func<int, int>>("w")(1);
            Assert(w == 2);
        }

        public void RubyHosting1B() {
            ScriptScope scope = Engine.Runtime.CreateScope();
            scope.SetVariable("x", 1);
            scope.SetVariable("y", 2);

            // "tmp" is defined in the top-level binding, which is associated with the scope:
            Engine.Execute("tmp = x + y", scope);
            
            // "tmp" symbol is extracted from scope's top-level binding and passed to the compiler as a compiler option
            // so that the parser treats it as a local variable.
            string tmpDefined = Engine.Execute<MutableString>("defined?(tmp)", scope).ToString();
            Assert(tmpDefined == "local-variable");

            // result= is turned into a scope variable assignment in method_missing:
            Engine.Execute("self.result = tmp", scope);

            int result = scope.GetVariable<int>("result");
            Assert(result == 3);

            // Ruby local variables are not exposed:
            Assert(scope.ContainsVariable("tmp") == false);
        }

        public void RubyHosting1C() {
            // Main singleton in a scope-unbound code doesn't define method_missing:
            AssertExceptionThrown<MemberAccessException>(
                () => Engine.Execute("class << self; remove_method(:method_missing); end")
            );
            
            // Main singleton in a scope-bound code defines method_missing:
            Engine.Execute("class << self; remove_method(:method_missing); end", Engine.CreateScope());

            var scope = Engine.CreateScope();
            Engine.Execute("self.tmp = 1", scope);
            Assert(scope.ContainsVariable("tmp"));
            
            AssertExceptionThrown<MissingMethodException>(() => Engine.Execute("self.tmp = 1"));
        }

        public void RubyHosting2() {
            Hashtable hash = new Hashtable();
            hash.Add("foo", "bar");
            
            ScriptScope scope = Engine.CreateScope();
            scope.SetVariable("h", hash);

            AssertOutput(() => 
                Engine.Execute(@"
def h.method_missing name
  get_Item(name.to_clr_string)
end

puts h.foo
", scope), @"
bar
");
        }

        public void RubyHosting3() {
            var searchPaths = Engine.GetSearchPaths();
            Assert(new List<string>(searchPaths)[searchPaths.Count - 1] == ".");

            // TODO:
            // the library paths are incorrect (not combined with location of .exe file) in partial trust:
            if (_driver.PartialTrust) return;

            bool result = Engine.RequireRubyFile("fcntl");
            Assert(result == true);

            var module = Runtime.Globals.GetVariable("Fcntl");
            Assert(module is RubyModule && ((RubyModule)module).Name == "Fcntl");
        }

        public void RubyHosting4() {
            // TODO: LanguageSetup should have an indexer:
            //var ruby = Ruby.CreateEngine((setup) => {
            //    setup["InterpretedMode"] = true;
            //    setup["SearchPaths"] = "blah";
            //});

            var ruby = Ruby.CreateEngine((setup) => {
                setup.InterpretedMode = true;
            });

            Assert(ruby.Setup.InterpretedMode == true);
        }

        public void Scenario_RubyEngine1() {
            ScriptScope scope = Runtime.CreateScope();
            object x = Engine.CreateScriptSourceFromString("1 + 1").Execute(scope);
            AssertEquals(x, 2);
        }

        public void Scenario_RubyInteractive1() {
            ScriptScope scope = Runtime.CreateScope();
            AssertOutput(delegate() {
                Engine.CreateScriptSourceFromString("1+1", SourceCodeKind.InteractiveCode).Execute(scope);
            }, "=> 2");
        }

        public void Scenario_RubyInteractive2() {
            string s;
            Assert((s = Engine.Operations.Format(new RubyArray(new[] { 1,2,3 }))) == "[1, 2, 3]");
            
            var obj = Engine.Execute(@"class C; def to_s; 'hello'; end; new; end");
            Assert((s = Engine.Operations.Format(obj)) == "hello");

            obj = Engine.Execute(@"class C; def to_s; 'bye'; end; new; end");
            Assert((s = Engine.Operations.Format(obj)) == "bye");
            
            obj = Engine.Execute(@"class C; def inspect; [7,8,9]; end; new; end");
            Assert((s = Engine.Operations.Format(obj)) == "789");

            var scope = Engine.CreateScope();
            scope.SetVariable("ops", Engine.Operations);
            s = Engine.Execute<string>(@"ops.format({1 => 2, 3 => 4})", scope);
            Assert(s == "{1=>2, 3=>4}");
        }

        public void Scenario_RubyConsole1() {
            ScriptScope module = Runtime.CreateScope();
            AssertOutput(delegate() {
                Engine.CreateScriptSourceFromString("class C; def foo() puts 'foo'; end end", SourceCodeKind.InteractiveCode).Execute(module);
                Engine.CreateScriptSourceFromString("C.new.foo", SourceCodeKind.InteractiveCode).Execute(module);
            }, @"
=> nil
foo
=> nil
");
        }
        
        public void CrossRuntime1() {
            AssertOutput(() => {
                CompilerTest(@"
load_assembly 'IronRuby.Libraries', 'IronRuby.StandardLibrary.IronRubyModule'
engine = IronRuby.create_engine
puts engine.execute('1+1')
puts engine.execute('Fixnum')
engine.execute('class Fixnum; def + other; 123; end; end')
puts 1+1
puts engine.execute('1+1')
");
            }, @"
2
Fixnum@*
2
123
", OutputFlags.Match);
        }
        
        public void CrossRuntime2() {
            var engine2 = Ruby.CreateEngine();
            Engine.Runtime.Globals.SetVariable("C", engine2.Execute("class C; def bar; end; self; end"));
            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute("class C; def foo; end; end"));
            
            // alias operates in the runtime of the class within which scope it is used:
            Engine.Execute("class C; alias foo bar; end");

            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute("class C; define_method(:goo) {}; end"));
            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute(@"
module M; end
class C; include M; end
"));
        }

        public void ObjectOperations1() {
            var cls = Engine.Execute(@"
class C
  def foo *a
    p a
  end
  self
end
");
            var obj = Engine.Operations.CreateInstance(cls) as RubyObject;
            Assert(obj != null && obj.Class.Name == "C");

            obj = Engine.Operations.InvokeMember(cls, "new") as RubyObject;
            Assert(obj != null && obj.Class.Name == "C");

            var foo = Engine.Operations.GetMember(obj, "foo") as RubyMethod;
            Assert(foo != null && foo.Name == "foo" && foo.Target == obj);

            AssertOutput(() => Engine.Operations.Invoke(foo, 1, 2), "[1, 2]");
            AssertOutput(() => Engine.Operations.InvokeMember(obj, "foo", 1, 2), "[1, 2]");
        }

        public void ObjectOperations2() {
            var cls = Engine.Execute(@"
class C
  def foo *a
    p a
  end
  def self.bar
  end
  undef_method :freeze
  self
end
");
            var names = Engine.Operations.GetMemberNames(cls);
            Assert(!names.Contains("foo"));
            Assert(names.Contains("taint"));
            Assert(names.Contains("bar"));

            var obj = Engine.Operations.CreateInstance(cls);
            names = Engine.Operations.GetMemberNames(obj);
            Assert(names.Contains("foo"));
            Assert(names.Contains("taint"));
            Assert(!names.Contains("freeze"));
            Assert(!names.Contains("bar"));
        }

        public void PythonInterop1() {
            if (!_driver.RunPython) return;

            var py = Runtime.GetEngine("python");
            Engine.Execute(@"
class C
  def foo
    123
  end
end
");

            var result = py.Execute<int>(@"
import C
C().foo()
");
            Assert(result == 123);
        }

        public void PythonInterop2() {
            if (!_driver.RunPython) return;

            var py = Runtime.GetEngine("python");

            py.Execute(@"
class C(object):
  def foo(self, a, b):
    return a + b
", Runtime.Globals);

            Assert(Engine.Execute<int>("C.new.foo(3,4)") == 7);
        }

        public void PythonInterop3() {
            if (!_driver.RunPython) return;

            var py = Runtime.GetEngine("python");

            var scope = py.CreateScope();
            py.Execute(@"
def python():
  return 'Python'
", scope);

            Engine.Execute(@"
def self.ruby
  python.call + ' + Ruby'
end
", scope);

            AssertOutput(() => py.Execute(@"
print ruby()
", scope), @"
Python + Ruby
");
        }

        public void PythonInterop4() {
            if (!_driver.RunPython) return;

            var py = Runtime.GetEngine("python");

            var scope = py.CreateScope();
            py.Execute(@"
def get_python_class():
  class C(object): 
    def __str__(self):
      return 'this is C'

  return C()
", scope);

            AssertOutput(() => Engine.Execute(@"
p get_python_class.call
", scope), @"
this is C
");
        }

        public void CustomTypeDescriptor1() {
            var cls = Engine.Execute(@"
class C
  attr_accessor :a
  def b
  end
  def b= x
    @written = true
  end
  def c
    @written
  end
  def d= x
  end
  def e x
  end
  def e= x
  end
  def initialize
    @a = 'Hello world'
    @written = false
  end
  self
end
");
            var obj = Engine.Operations.CreateInstance(cls);
            Assert(obj != null);
            var ictd = Engine.Operations.CreateInstance(cls) as ICustomTypeDescriptor;
            Assert(ictd != null);
            Assert(ictd.GetClassName() == "C");
            var props = ictd.GetProperties();
            Assert(props.Count == 2);
            props[0].SetValue(obj, "abc");
            props[1].SetValue(obj, "abc");
            Assert(Engine.Operations.InvokeMember(obj, "a").Equals("abc"));
            Assert(Engine.Operations.InvokeMember(obj, "c").Equals(true));
        }

        public void CustomTypeDescriptor2() {
            Context.ObjectClass.SetConstant("C", Context.GetClass(typeof(ArrayList)));
            var cls = Engine.Execute(@"
class D < C
  attr_accessor :a
  self
end
");
            var obj = Engine.Operations.CreateInstance(cls);
            Assert(obj != null);
            var ictd = Engine.Operations.CreateInstance(cls) as ICustomTypeDescriptor;
            Assert(ictd != null);
            Assert(ictd.GetClassName() == "D");
            var props = ictd.GetProperties();
            Assert(props.Count == 1);
            props[0].SetValue(obj, "abc");
            Assert(Engine.Operations.InvokeMember(obj, "a").Equals("abc"));
        }
    }
}
