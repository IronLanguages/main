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
        public void RubyHosting1() {
            ScriptScope scope = Engine.Runtime.CreateScope();
            scope.SetVariable("x", 1);
            scope.SetVariable("y", 2);

            // TODO: could we replace def self.z with z = ? - via dynamic binding
            Engine.Execute("def self.z; x + y; end", scope);
            Engine.Execute("def self.w(a); a + 1; end", scope);

            int z = scope.GetVariable<Func<int>>("z")();
            Assert(z == 3);

            int w = scope.GetVariable<Func<int, int>>("w")(1);
            Assert(w == 2);
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

            Debug.Assert(ruby.Setup.InterpretedMode == true);
        }

        public void Scenario_RubyEngine1() {
            ScriptScope scope = Runtime.CreateScope();
            object x = Engine.CreateScriptSourceFromString("1 + 1").Execute(scope);
            AssertEquals(x, 2);
        }

        public void Scenario_RubyInteractive() {
            ScriptScope scope = Runtime.CreateScope();
            AssertOutput(delegate() {
                Engine.CreateScriptSourceFromString("1+1", SourceCodeKind.InteractiveCode).Execute(scope);
            }, "=> 2");
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
            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute("class C; alias foo bar; end"));
            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute("class C; define_method(:goo) {}; end"));
            AssertExceptionThrown<InvalidOperationException>(() => Engine.Execute(@"
module M; end
class C; include M; end
"));
        }

        public void Scenario_RubyConsole2() {
#if OBSOLETE
            // TODO: interop
            ScriptScope module = ScriptDomainManager.CurrentManager.CreateModule("Scenario_RubyConsole2");
            module.SetVariable("a", 0);
            RB.Execute(module, RB.CreateScriptSourceFromString("10.times { |x| a = a + x + 1}", SourceCodeKind.Statements));
            object a = module.LookupVariable("a");
            Assert((int)a == 55);

            module.SetVariable("b", 1);
            RB.Execute(module, RB.CreateScriptSourceFromString("10.times { |x| b = b + x + 1}", SourceCodeKind.Statements));
            object b = module.LookupVariable("b");
            Assert((int)b == 56);
#endif
        }

        public void Scenario_RubyConsole3() {
            // TODO: bug in top-level scope

            //            ScriptModule module = ScriptDomainManager.CurrentManager.CreateModule("Scenario_RubyConsole3");
            //            RB.Execute(@"
            //for i in [11] do
            //    j = 1
            //end", module);
            //            object a = module.LookupVariable("j");
            //            Assert((int)a == 1);
        }

        public void Scenario_RubyConsole4() {

            //            XAssertOutput(delegate() {
            //                RB.ExecuteInteractiveCode("x = 1");
            //                RB.ExecuteInteractiveCode("puts x");
            //            }, @"
            //=> 1
            //=> nil
            //1");
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
            Debug.Assert(obj != null && obj.Class.Name == "C");

            obj = Engine.Operations.InvokeMember(cls, "new") as RubyObject;
            Debug.Assert(obj != null && obj.Class.Name == "C");

            var foo = Engine.Operations.GetMember(obj, "foo") as RubyMethod;
            Debug.Assert(foo != null && foo.Name == "foo" && foo.Target == obj);

            AssertOutput(() => Engine.Operations.Call(foo, 1, 2), "[1, 2]");
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
            var py = Runtime.GetEngine("python");
            Engine.Execute(@"
class C
  def foo
    puts 'foo'
  end
end
");

            py.CreateScriptSourceFromString(@"
import C
C.new().foo()    # TODO: C().foo()
", SourceCodeKind.Statements).Execute();
        }

        public void PythonInterop2() {
            var py = Runtime.GetEngine("python");

            py.CreateScriptSourceFromString(@"
class C(object):
  def foo(self):
    print 'foo'
", SourceCodeKind.Statements).Execute(Runtime.Globals);

            Engine.Execute(@"
p C             #TODO: C.new
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
            Debug.Assert(obj != null);
            var ictd = Engine.Operations.CreateInstance(cls) as ICustomTypeDescriptor;
            Debug.Assert(ictd != null);
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
            Debug.Assert(obj != null);
            var ictd = Engine.Operations.CreateInstance(cls) as ICustomTypeDescriptor;
            Debug.Assert(ictd != null);
            Assert(ictd.GetClassName() == "D");
            var props = ictd.GetProperties();
            Assert(props.Count == 1);
            props[0].SetValue(obj, "abc");
            Assert(Engine.Operations.InvokeMember(obj, "a").Equals("abc"));
        }
    }
}
