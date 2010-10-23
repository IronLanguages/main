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
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Tests {
    public partial class Tests {

        public void Loader_Assemblies1() {
            string assembly;
            string type;
            string str;
            bool b;
            
            str = "a.rb";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == false);

            str = "IronRuby";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == false);
            
            str = @"..\foo\bar\a,b.rb";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == false);
            
            str = "IronRuby.Runtime.RubyContext, IronRuby, Version=" + RubyContext.IronRubyVersionString + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == true &&
                assembly == "IronRuby, Version=" + RubyContext.IronRubyVersionString + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35" &&
                type == "IronRuby.Runtime.RubyContext"
            );

            str = "IronRuby, Version=" + RubyContext.IronRubyVersionString + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == true && assembly == str && type == null);

            str = "IronRuby, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == true && assembly == str && type == null);

            str = "IronRuby, Version=" + RubyContext.IronRubyVersionString;
            b = Loader.TryParseAssemblyName(str, out type, out assembly);
            Assert(b == true && assembly == str && type == null);
        }

        public void Require1() {
            if (_driver.PartialTrust) return;

            try {
                string temp = _driver.MakeTempDir();
                Context.Loader.SetLoadPaths(new[] { temp });
                File.WriteAllText(Path.Combine(temp, "a.rb"), @"C = 123");

                AssertOutput(delegate() {
                    CompilerTest(@"
puts(require('a'))
puts C
");
                }, @"
true
123
");

                AssertOutput(delegate() {
                    CompilerTest(@"
puts(require('a.rb'))
puts C
");
                }, @"
false
123
");
            } finally {
                File.Delete("a.rb");
            }
        }

        public void Load1() {
            if (_driver.PartialTrust) return;

            try {
                string temp = _driver.MakeTempDir();
                Context.Loader.SetLoadPaths(new[] { temp });

                File.WriteAllText(Path.Combine(temp, "a.rb"), @"C = 123");
                
                AssertOutput(delegate() {
                    CompilerTest(@"
puts(load('a.rb', true))
puts C rescue puts 'error'
");
                }, @"
true
error
");
            } finally {
                File.Delete("a.rb");
            }
        }

        public void RequireInterop1() {
            if (_driver.PartialTrust || !_driver.RunPython) return;

            try {
                string temp = _driver.MakeTempDir();
                Context.Loader.SetLoadPaths(new[] { temp });

                File.WriteAllText(Path.Combine(temp, "a.py"), @"
print 'Hello from Python'
");
                AssertExceptionThrown<LoadError>(() => CompilerTest(@"require('a')"));
            } finally {
                File.Delete("b.py");
            }
        }

        public void RequireInterop2() {
            if (_driver.PartialTrust || !_driver.RunPython) return;

            try {
                string temp = _driver.MakeTempDir();
                Context.Loader.SetLoadPaths(new[] { temp });

                File.WriteAllText(Path.Combine(temp, "a.py"), @"
def WhoIsThis():
  print 'Python'

Foo = 1

class Bar(object):
  def baz(self):
    print Foo
");

                TestOutput(@"
a = IronRuby.require('a')
scopes = IronRuby.loaded_scripts.collect { |z| z.value }
a.who_is_this
a.foo += 1
a.bar.baz             # a Python class is callable so we get Bar's instance from a.bar
puts scopes[0].Foo
", @"
Python
2
2
");

            } finally {
                File.Delete("b.py");
            }
        }

        public class TestLibraryInitializer1 : LibraryInitializer {
            protected override void LoadModules() {
                Context.ObjectClass.SetConstant("TEST_LIBRARY", "hello from library");
                ExtendClass(typeof(Object), 0, null, ObjectMonkeyPatch, null, null, RubyModule.EmptyArray);  
            }

            private void ObjectMonkeyPatch(RubyModule/*!*/ module) {
                Debug.Assert(module == Context.ObjectClass);

                DefineLibraryMethod(module, "object_monkey", 0x9, new[] {
                    LibraryOverload.Reflect(new Func<object, string>(MonkeyWorker)),
                });
            }

            public static string MonkeyWorker(object obj) {
                return "This is monkey!";
            }
        }

        public void LibraryLoader1() {
            Context.DefineGlobalVariable("lib_name", MutableString.CreateAscii(typeof(TestLibraryInitializer1).AssemblyQualifiedName));

            AssertOutput(delegate() {
                CompilerTest(@"
require($lib_name)
puts TEST_LIBRARY
puts object_monkey
");
            }, @"
hello from library
This is monkey!
");

        }

        public class TestLibraryInitializer2 : LibraryInitializer {
            protected override void LoadModules() {
                DefineGlobalModule("LibModule", typeof(Object), 0, null, LibModuleSingletonMethods, null, RubyModule.EmptyArray);
            }

            private void LibModuleSingletonMethods(RubyModule/*!*/ module) {
                DefineLibraryMethod(module, "bar", (int)RubyMethodAttributes.PublicSingleton, new[] {
                    LibraryOverload.Reflect(new Func<RubyModule, string>(Bar)),
                });
            }

            public static string Bar(RubyModule/*!*/ self) {
                return "bar";
            }
        }

        public void LibraryLoader2() {
            Context.DefineGlobalVariable("lib_name", MutableString.CreateAscii(typeof(TestLibraryInitializer2).AssemblyQualifiedName));

            TestOutput(@"
module LibModule
  def self.foo
    'foo'
  end
end
require($lib_name)
puts LibModule.foo
puts LibModule.bar
", @"
foo
bar
");

        }
    }
}
