/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.PyAnalysis;
using Microsoft.PyAnalysis.Values;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using IronPython.Runtime.Exceptions;

namespace AnalysisTest {
    [TestClass]
    public partial class AnalysisTest {
        private static ScriptEngine _engine;
        private static PythonType PyObjectType, IntType, StringType, FloatType, TypeType, ListType, TupleType, BoolType, FunctionType, ComplexType, GeneratorType;
        private static string[] _objectMembers, _objectMembersClr, _functionMembers;
        private static string[] _strMembers, _strMembersClr;
        private static string[] _listMembers, _intMembers;

        public static int Main(string[] args) {
            AnalysisTest test = new AnalysisTest();

            if (args.Length > 0 && args[0] == "PERF") {
                args = ArrayUtils.ShiftLeft(args, 1);
                return test.RunTests(args, typeof(PerfMethodAttribute));
            }

            return test.RunTests(args, typeof(TestMethodAttribute)); 
        }

        private int RunTests(string[] args, Type testAttr) {            
            var fg = Console.ForegroundColor;
            int failures = 0;
            foreach (var mi in typeof(AnalysisTest).GetMethods()) {
                if ((args.Length == 0 || (args.Length > 0 && args.Contains(mi.Name))) &&
                    mi.IsDefined(testAttr, false)) {

                    try {
                        mi.Invoke(this, new object[0]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Test passed: {0}", mi.Name);
                    } catch (Exception e) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Test failed: {0}", mi.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(e);
                        failures++;
                    }
                }
            }

            Console.WriteLine();
            if (failures == 0) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No failures");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} failures", failures);
            }
            Console.ForegroundColor = fg;
            return failures;
        }

        #region Test Cases

        [TestMethod]
        public void TestImportStar() {
            var entry = ProcessText(@"
from nt import *
            ");

            var members = entry.GetMembers("", 1).Select(x => x.Name);

            AssertContains(members, "abort");

            entry = ProcessText(@"");

            // make sure abort hasn't become a builtin, if so this test needs to be updated
            // with a new name
            members = entry.GetMembers("", 1).Select(x => x.Name);
            foreach (var member in members) {
                if(member == "abort") {
                    Assert.Fail("abort has become a builtin, or a clean module includes it for some reason");
                }
            }
        }

        [TestMethod]
        public void TestMutatingReferences() {
            var state = new ProjectState(_engine);

            var text1 = @"
import mod2

class C(object):
    def SomeMethod(self):
        pass

mod2.D(C())
";

            var text2 = @"
class D(object):
    def __init__(self, value):
        self.value = value
        self.value.SomeMethod()
";

            var mod1 = ParseText(state, GetSourceUnit(text1, "mod1"), "mod1");
            var mod2 = ParseText(state, GetSourceUnit(text2, "mod2"), "mod2");

            VerifyReferences(mod1.Analysis.GetVariables("SomeMethod", GetLineNumber(text1, "SomeMethod")), 
                new VariableLocation(5, 5, VariableType.Definition), new VariableLocation(5, 9, VariableType.Reference));

            // mutate 1st file
            text1 = text1.Substring(0, text1.IndexOf("    def")) + Environment.NewLine + text1.Substring(text1.IndexOf("    def"));
            Prepare(mod1, GetSourceUnit(text1, "mod1"));
            mod1.Analyze();
            
            VerifyReferences(mod1.Analysis.GetVariables("SomeMethod", GetLineNumber(text1, "SomeMethod")),
                new VariableLocation(6, 5, VariableType.Definition), new VariableLocation(5, 9, VariableType.Reference));

            // mutate 2nd file
            text2 = Environment.NewLine + text2;
            Prepare(mod2, GetSourceUnit(text2, "mod1"));
            mod2.Analyze();

            VerifyReferences(mod1.Analysis.GetVariables("SomeMethod", GetLineNumber(text1, "SomeMethod")),
                new VariableLocation(6, 5, VariableType.Definition), new VariableLocation(6, 9, VariableType.Reference));

        }

        /// <summary>
        /// Verify importing wpf will add a reference to the WPF assemblies
        /// </summary>
        [TestMethod]
        public void TestWpfReferences() {
            var entry = ProcessText(@"
import wpf
from System.Windows.Media import Colors
");

            AssertContains(entry.GetMembersFromName("Colors", 1), "Blue");
        }

        [TestMethod]
        public void TestGenerator() {
            var entry = ProcessText(@"
def f():
    yield 1
    yield 2
    yield 3

a = f()
b = a.next()

for c in f():
    print c
            ");

            AssertContainsExactly(entry.GetTypesFromName("a", 1), GeneratorType);
            AssertContainsExactly(entry.GetTypesFromName("b", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("c", 1), IntType);

            var text = @"
def f():
    yield 1
    x = yield 2

a = f()
b = a.next()
c = a.send('abc')";
            entry = ProcessText(text);

            AssertContainsExactly(entry.GetTypesFromName("a", 1), GeneratorType);
            AssertContainsExactly(entry.GetTypesFromName("b", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("c", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("x", GetLineNumber(text, "yield 2")), StringType);
        }

        [TestMethod]
        public void TestEnum() {
            var entry = ProcessText(@"
import System
x = System.StringComparison.OrdinalIgnoreCase
            ");

            var x = entry.GetValues("x", 1).First();
            Assert.AreEqual(x.ResultType, ResultType.EnumInstance);
        }

        /*
        [TestMethod]
        public void TestListComprehensions() {
            var entry = ProcessText(@"
x = [2,3,4]
y = [a for a in x]
z = y[0]
            ");

            AssertContainsExactly(entry.GetTypesFromName("z", 0), IntType);
        }*/

        [TestMethod]
        public void TestForSequence() {
            var entry = ProcessText(@"
x = [('abc', 42, True), ('abc', 23, False),]
for some_str, some_int, some_bool in x:
	print some_str		
	print some_int		
    print some_bool	    
");
            AssertContainsExactly(entry.GetTypesFromName("some_str", 1), StringType);
            AssertContainsExactly(entry.GetTypesFromName("some_int", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("some_bool", 1), BoolType);
        }

        [TestMethod]
        public void TestDynamicAttributes() {
            var entry = ProcessText(@"
class x(object):
    def __getattr__(self, name):
        return 42
    def f(self): 
        return 'abc'
        
a = x().abc
b = x().f()

class y(object):
    def __getattribute__(self, x):
        return 'abc'
        
c = y().abc
");

            AssertContainsExactly(entry.GetTypesFromName("a", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", 1), StringType);
            AssertContainsExactly(entry.GetTypesFromName("c", 1), StringType);
        }
        
        [TestMethod]
        public void TestListAppend() {
            var entry = ProcessText(@"
x = []
x.append('abc')
y = x[0]
");

            AssertContainsExactly(entry.GetTypesFromName("y", 1), StringType);

            entry = ProcessText(@"
x = []
x.extend(('abc', ))
y = x[0]
");
            AssertContainsExactly(entry.GetTypesFromName("y", 1), StringType);

            entry = ProcessText(@"
x = []
x.insert(0, 'abc')
y = x[0]
");
            AssertContainsExactly(entry.GetTypesFromName("y", 1), StringType);

            entry = ProcessText(@"
x = []
x.append('abc')
y = x.pop()
");

            AssertContainsExactly(entry.GetTypesFromName("y", 1), StringType);

            entry = ProcessText(@"
class ListTest(object):
    def reset(self):
        self.items = []
        self.pushItem(self)
    def pushItem(self, item):
        self.items.append(item)

a = ListTest()
b = a.items[0]");

            AssertContains(entry.GetMembersFromName("b", 1), "pushItem");
        }

        [TestMethod]
        public void TestSlicing() {
            var entry = ProcessText(@"
x = [2]
y = x[:-1]
z = y[0]
");

            AssertContainsExactly(entry.GetTypesFromName("z", 1), IntType);

            entry = ProcessText(@"
x = (2, 3, 4)
y = x[:-1]
z = y[0]
");

            AssertContainsExactly(entry.GetTypesFromName("z", 1), IntType);
        }

        [TestMethod]
        public void TestColor() {

            var entry = ProcessText(@"
import clr
clr.AddReference('PresentationFramework')
clr.AddReference('PresentationCore')

from System.Windows.Media import Colors

class C(object):
    def __init__(self):
        if False:
            self.some_color = Colors.Black
        else:
            self.some_color = Colors.White


a = C()
b = a.some_color
");

            AssertContainsExactly(entry.GetTypesFromName("b", 1).Select(x => PythonType.Get__name__(x)), "Color");
        }

        [TestMethod]
        public void TestConstantIndex() {
            var entry = ProcessText(@"
ZERO = 0
ONE = 1
TWO = 2
x = ['abc', 42, True)]


some_str = x[ZERO]
some_int = x[ONE]
some_bool = x[TWO]
");
            AssertContainsExactly(entry.GetTypesFromName("some_str", 1), StringType);
            AssertContainsExactly(entry.GetTypesFromName("some_int", 1), IntType);
            AssertContainsExactly(entry.GetTypesFromName("some_bool", 1), BoolType);
        }
        [TestMethod]
        public void TestCtorSignatures() {
            var entry = ProcessText(@"
class C: pass

class D(object): pass

class E(object):
    def __init__(self): pass

class F(object):
    def __init__(self, one): pass

class G(object):
    def __new__(cls): pass

class H(object):
    def __new__(cls, one): pass

            ");

            var result = entry.GetSignatures("C", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 0);

            result = entry.GetSignatures("D", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 0);

            result = entry.GetSignatures("E", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 0);

            result = entry.GetSignatures("F", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 1);

            result = entry.GetSignatures("G", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 0);

            result = entry.GetSignatures("H", 1).ToArray();
            Assert.AreEqual(result.Length, 1);
            Assert.AreEqual(result[0].Parameters.Length, 1);
        }
        
        [TestMethod]
        public void TestBuiltinTypeSignatures() {
            var entry = ProcessText(@"
import System
x = str
x = int

y = str
y = int
");

            var result = entry.GetSignatures("System.Collections.Generic.Dictionary[int, int]", 1).ToArray();
            Assert.AreEqual(result.Length, 6);

            // 2 possible types
            result = entry.GetSignatures("System.Collections.Generic.Dictionary[x, int]", 1).ToArray();
            Assert.AreEqual(result.Length, 12);

            // 4 possible types
            result = entry.GetSignatures("System.Collections.Generic.Dictionary[x, y]", 1).ToArray();
            Assert.AreEqual(result.Length, 24);
        }

        [TestMethod]
        public void TestBuiltinMethodSignatures() {
            var entry = ProcessText(@"
const = """".capitalize
constructed = str().capitalize
");

            string[] testCapitalize = new[] { "const", "constructed" };
            foreach (var test in testCapitalize) {
                var result = entry.GetSignatures(test, 1).ToArray();
                Assert.AreEqual(result.Length, 1);
                Assert.AreEqual(result[0].Parameters.Length, 0);
            }

            entry = ProcessText(@"
import clr
const = """".Contains
constructed = str().Contains
");

            string[] testContains = new[] { "const", "constructed" };
            foreach (var test in testContains) {
                var result = entry.GetSignatures(test, 1).ToArray();
                Assert.AreEqual(result.Length, 1);
                Assert.AreEqual(result[0].Parameters.Length, 1);
                Assert.AreEqual(result[0].Parameters[0].Name, "value");
                Assert.AreEqual(result[0].Parameters[0].IsOptional, false);
            }
        }

        [TestMethod]
        public void TestDel() {
            string text = @"
del foo
del foo[2]
del foo.bar
del (foo)
del foo, bar
";
            var entry = ProcessText(text);

            // We do no analysis on del statements, nothing to test
        }

        [TestMethod]
        public void TryExcept() {
            string text = @"
class MyException(Exception): pass

def f():
    try:
    except TypeError, e1:
        pass

def g():
    try:
    except MyException, e2:
        pass
";
            var entry = ProcessText(text);

            AssertContainsExactly(GetTypes(entry.GetValues("e1", GetLineNumber(text, ", e1"))), PythonExceptions.TypeError);

            AssertContainsExactly(GetTypeNames(entry.GetValues("e2", GetLineNumber(text, ", e2"))), "MyException instance");
        }

        private IEnumerable<PythonType> GetTypes(IEnumerable<IAnalysisValue> analysisValues) {
            foreach (var value in analysisValues) {
                yield return value.PythonType;
            }
        }

        private IEnumerable<string> GetTypeNames(IEnumerable<IAnalysisValue> analysisValues) {
            foreach (var value in analysisValues) {
                yield return value.ShortDescription;
            }
        }

        class VariableLocation {
            public readonly int StartLine;
            public readonly int StartCol;
            public readonly VariableType Type;

            public VariableLocation(int startLine, int startCol, VariableType type) {
                StartLine = startLine;
                StartCol = startCol;
                Type = type;
            }
        }

        [TestMethod]
        public void TestReferences() {
            // instance variables
            var text = @"
# add ref w/o type info
class C(object):
    def __init__(self, foo):
        self.abc = foo
        del self.abc
        print self.abc

";
            var entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("self.abc", GetLineNumber(text, "self.abc")), new VariableLocation(5, 9, VariableType.Definition), new VariableLocation(6, 13, VariableType.Reference), new VariableLocation(7, 15, VariableType.Reference));
            VerifyReferences(entry.GetVariables("foo", GetLineNumber(text, "foo")), new VariableLocation(4, 24, VariableType.Definition), new VariableLocation(5, 20, VariableType.Reference));

            text = @"
# add ref w/ type info
class D(object):
    def __init__(self, foo):
        self.abc = foo
        del self.abc
        print self.abc

D(42)";
            entry = ProcessText(text);

            VerifyReferences(entry.GetVariables("self.abc", GetLineNumber(text, "self.abc")), new VariableLocation(5, 9, VariableType.Definition), new VariableLocation(6, 13, VariableType.Reference), new VariableLocation(7, 15, VariableType.Reference));
            VerifyReferences(entry.GetVariables("foo", GetLineNumber(text, "foo")), new VariableLocation(4, 24, VariableType.Definition), new VariableLocation(5, 20, VariableType.Reference));
            VerifyReferences(entry.GetVariables("D", GetLineNumber(text, "D(42)")), new VariableLocation(9, 1, VariableType.Reference), new VariableLocation(3, 1, VariableType.Definition));

            // function definitions
            text = @"
def f(): pass

x = f()";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("f", GetLineNumber(text, "x =")), new VariableLocation(4, 5, VariableType.Reference), new VariableLocation(2, 1, VariableType.Definition));

            text = @"
from System import EventHandler
def g():
    x = EventHandler(f)
    
def f(sender, args): pass
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("f", GetLineNumber(text, "x =")), new VariableLocation(4, 22, VariableType.Reference), new VariableLocation(6, 1, VariableType.Definition));

            text = @"
from System import EventHandler
def f(sender, args): pass

x = EventHandler(f)";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("f", GetLineNumber(text, "x =")), new VariableLocation(5, 18, VariableType.Reference), new VariableLocation(3, 1, VariableType.Definition));
            
            // left hand side is unknown, right hand side should still have refs added
            text = @"
from System import EventHandler
def f(sender, args): pass

a.foo += EventHandler(f)
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("f", GetLineNumber(text, "a.foo +=")), new VariableLocation(5, 23, VariableType.Reference), new VariableLocation(3, 1, VariableType.Definition));
            

            text = @"
def f(): pass

x = f";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("f", GetLineNumber(text, "x =")), new VariableLocation(4, 5, VariableType.Reference), new VariableLocation(2, 1, VariableType.Definition));

            // class variables
            text = @"

class D(object):
    abc = 42
    print abc
    del abc
";
            entry = ProcessText(text);

            VerifyReferences(entry.GetVariables("abc", GetLineNumber(text, "abc =")), new VariableLocation(4, 5, VariableType.Definition), new VariableLocation(5, 11, VariableType.Reference), new VariableLocation(6, 9, VariableType.Reference));

            // class definition
            text = @"
class D(object): pass

a = D
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("D", GetLineNumber(text, "a =")), new VariableLocation(4, 5, VariableType.Reference), new VariableLocation(2, 1, VariableType.Definition));

            // method definition
            text = @"
class D(object): 
    def f(self): pass

a = D().f()
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("D().f", GetLineNumber(text, "a =")), 
                new VariableLocation(5, 5, VariableType.Reference), new VariableLocation(3, 5, VariableType.Definition));

            // globals
            text = @"
abc = 42
print abc
del abc
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("abc", GetLineNumber(text, "abc =")), new VariableLocation(4, 5, VariableType.Reference), new VariableLocation(2, 1, VariableType.Definition), new VariableLocation(3, 7, VariableType.Reference));

            // parameters
            text = @"
def f(abc):
    print abc
    abc = 42
    del abc
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("abc", GetLineNumber(text, "abc =")), new VariableLocation(2, 7, VariableType.Definition), new VariableLocation(4, 5, VariableType.Definition), new VariableLocation(3, 11, VariableType.Reference), new VariableLocation(5, 9, VariableType.Reference));
        
        
            // grammer test - statements
            text = @"
def f(abc):
    try: pass
    except abc: pass

    try: pass
    except TypeError, abc: pass    

    abc, bar = 42, 23
    abc[23] = 42
    abc.foo = 42
    abc += 2

    class D(abc): pass

    for x in abc: print x

    import abc
    from xyz import abc
    from xyz import bar as abc

    if abc: print 'hi'
    elif abc: print 'bye'
    else: abc

    with abc:
        return abc

    print abc
    assert abc, abc

    raise abc
    raise abc, abc, abc

    while abc:
        abc
    else:
        abc

    for x in foo: 
        print x
    else:
        print abc

    try: pass
    except TypeError:
    else:
        abc
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("abc", GetLineNumber(text, "f(abc)")), 
                new VariableLocation(2, 7, VariableType.Definition), 
                new VariableLocation(4, 12, VariableType.Reference), 
                new VariableLocation(7, 23, VariableType.Definition),
                
                new VariableLocation(9, 5, VariableType.Definition),
                new VariableLocation(10, 5, VariableType.Reference),
                new VariableLocation(11, 5, VariableType.Reference),
                new VariableLocation(12, 5, VariableType.Reference),
                
                new VariableLocation(14, 13, VariableType.Reference),
                
                new VariableLocation(16, 14, VariableType.Reference),

                new VariableLocation(18, 5, VariableType.Reference), 
                new VariableLocation(19, 5, VariableType.Reference), 
                new VariableLocation(20, 5, VariableType.Reference),

                new VariableLocation(22, 8, VariableType.Reference),
                new VariableLocation(23, 10, VariableType.Reference),
                new VariableLocation(24, 11, VariableType.Reference),

                new VariableLocation(26, 10, VariableType.Reference),
                new VariableLocation(27, 16, VariableType.Reference),

                new VariableLocation(29, 11, VariableType.Reference),
                new VariableLocation(30, 12, VariableType.Reference),
                new VariableLocation(30, 17, VariableType.Reference),

                new VariableLocation(32, 11, VariableType.Reference),
                new VariableLocation(33, 11, VariableType.Reference),
                new VariableLocation(33, 16, VariableType.Reference),
                new VariableLocation(33, 21, VariableType.Reference),

                new VariableLocation(35, 11, VariableType.Reference),
                new VariableLocation(36, 9, VariableType.Reference),
                new VariableLocation(38, 9, VariableType.Reference),

                new VariableLocation(43, 15, VariableType.Reference),

                new VariableLocation(48, 9, VariableType.Reference) 
            );


            // grammer test - expressions
            text = @"
def f(abc):
    x = abc + 2
    x = 2 + abc
    x = l[abc]
    x = abc[l]
    x = abc.foo
    
    g(abc)

    abc if abc else abc

    {abc:abc},
    [abc, abc]
    (abc, abc)
    {abc}

    yield abc
    [x for x in abc]
    (x for x in abc)

    abc or abc
    abc and abc

    +abc
    x[abc:abc:abc]

    abc == abc
    not abc

    lambda : abc
";
            entry = ProcessText(text);
            VerifyReferences(entry.GetVariables("abc", GetLineNumber(text, "f(abc)")),
                new VariableLocation(2, 7, VariableType.Definition),

                new VariableLocation(3, 9, VariableType.Reference),
                new VariableLocation(4, 13, VariableType.Reference),

                new VariableLocation(5, 10, VariableType.Reference),        // BUGBUG: should be 5,11
                new VariableLocation(6, 9, VariableType.Reference),
                new VariableLocation(7, 9, VariableType.Reference),
                new VariableLocation(9, 7, VariableType.Reference),

                new VariableLocation(11, 5, VariableType.Reference),
                new VariableLocation(11, 12, VariableType.Reference),
                new VariableLocation(11, 21, VariableType.Reference),

                new VariableLocation(13, 6, VariableType.Reference),
                new VariableLocation(13, 10, VariableType.Reference),
                new VariableLocation(14, 6, VariableType.Reference),
                new VariableLocation(14, 11, VariableType.Reference),
                new VariableLocation(15, 6, VariableType.Reference),
                new VariableLocation(15, 11, VariableType.Reference),
                new VariableLocation(16, 6, VariableType.Reference),

                new VariableLocation(18, 11, VariableType.Reference),
                new VariableLocation(19, 17, VariableType.Reference),
                new VariableLocation(20, 17, VariableType.Reference),

                new VariableLocation(22, 5, VariableType.Reference),
                new VariableLocation(22, 12, VariableType.Reference),
                new VariableLocation(23, 5, VariableType.Reference),
                new VariableLocation(23, 13, VariableType.Reference),

                new VariableLocation(25, 6, VariableType.Reference),
                new VariableLocation(26, 7, VariableType.Reference),
                new VariableLocation(26, 11, VariableType.Reference),
                new VariableLocation(26, 15, VariableType.Reference),

                new VariableLocation(28, 5, VariableType.Reference),
                new VariableLocation(28, 12, VariableType.Reference),
                new VariableLocation(29, 9, VariableType.Reference)

                //new VariableLocation(30, 14, VariableType.Reference) // BUGBUG: Enable when lambda bodies are walked
            );
        }

        private void VerifyReferences(IEnumerable<IAnalysisVariable> variables, params VariableLocation[] variableType) {
            var vars = new List<IAnalysisVariable>(variables);
            if (vars.Count == 0) {
                Assert.Fail("Got no references");
            }

            int removed = 0;
            bool removedOne = false;
            do {
                for (int j = 0; j < variableType.Length; j++) {
                    var expected = variableType[j];

                    bool found = false;
                    for (int i = 0; i < vars.Count; i++) {
                        var have = vars[i];

                        if (have.Location.Line == expected.StartLine &&
                            have.Location.Column == expected.StartCol &&
                            have.Type == expected.Type) {
                            vars.RemoveAt(i);
                            removed++;
                            removedOne = found = true;
                            break;
                        }
                    }

                    if (!found) {
                        StringBuilder error = new StringBuilder(String.Format("Failed to find location: {0} {1} {2}" + Environment.NewLine, expected.StartLine, expected.StartCol, expected.Type));
                        LocationNames(vars, error);

                        Assert.Fail(error.ToString());
                    }
                }
            } while (vars.Count != 0 && removedOne);

            if (vars.Count != 0) {
                StringBuilder error = new StringBuilder("Didn't use all locations - had " + variables.Count() + Environment.NewLine);
                LocationNames(vars, error);
                Assert.Fail(error.ToString());
            }
        }

        private static void LocationNames(List<IAnalysisVariable> vars, StringBuilder error) {
            foreach (var var in vars) {
                error.AppendFormat("   {0} {1} {2}", var.Location.Line, var.Location.Column, var.Type);
                error.AppendLine();
            }
        }

        [TestMethod]
        public void TestSignatureDefaults() {
            var entry = ProcessText(@"
def f(x = None): pass

def g(x = {}): pass

def h(x = {2:3}): pass

def i(x = []): pass

def j(x = [None]): pass

def k(x = ()): pass

def l(x = (2, )): pass
");

            var tests = new[] {
                new { FuncName = "f", ParamName="x = None" },
                new { FuncName = "g", ParamName="x = {}" },
                new { FuncName = "h", ParamName="x = {...}" },
                new { FuncName = "i", ParamName="x = []" },
                new { FuncName = "j", ParamName="x = [...]" },
                new { FuncName = "k", ParamName="x = ()" },
                new { FuncName = "l", ParamName="x = (...)" },
            };

            foreach (var test in tests) {
                var result = entry.GetSignatures(test.FuncName, 1).ToArray();
                Assert.AreEqual(result.Length, 1);
                Assert.AreEqual(result[0].Parameters[0].Name, test.ParamName);
            }
        }

        [TestMethod]
        public void TestGetVariablesDictionaryGet() {
            var entry = ProcessText(@"
x = {42:'abc'}
            ");

            foreach (var varRef in entry.GetValues("x.get", 1)) {
                Assert.AreEqual("built-in method get", varRef.Description);
            }
        }

        [TestMethod]
        public void TestLambdaExpression() {
            var entry = ProcessText(@"
x = lambda a: a
y = x(42)
");

            AssertContainsExactly(entry.GetTypesFromName("y", 1), IntType);

            entry = ProcessText(@"
def f(a):
    return a

x = lambda b: f(b)
y = x(42)
");

            AssertContainsExactly(entry.GetTypesFromName("y", 1), IntType);
        }

        [TestMethod]
        public void TestRecursiveClass() {
            var entry = ProcessText(@"
cls = object

class cls(cls): 
    abc = 42
");

            entry.GetMembersFromName("cls", 1);
            AssertContainsExactly(entry.GetMembers("cls().abc.", 1).Select(member => member.Name), _intMembers);
            AssertContainsExactly(entry.GetMembers("cls.abc.", 1).Select(member => member.Name), _intMembers);
        }

        [TestMethod]
        public void TestBadMethod() {
            var entry = ProcessText(@"
class cls(object): 
    def f(): 
        return 42

abc = cls()
foo = abc.f()
");

            AssertContainsExactly(entry.GetMembers("foo.", 1).Select(member => member.Name), _intMembers);
        }

        [TestMethod]
        public void TestKeywordArguments() {
            var funcDef = "def f(a, b, c): pass";
            var classWithInit  = @"class f(object):
    def __init__(self, a, b, c):
        pass";
            var classWithNew = @"class f(object):
    def __new__(cls, a, b, c):
        pass";
            var method = @"class x(object):
    def g(self, a, b, c):
        pass

f = x().g";
            var decls = new []  { funcDef, classWithInit, classWithNew, method };

            foreach (var decl in decls) {
                string[] testCalls = new[] { 
                    "f(c = 'abc', b = 42, a = 3j)", "f(3j, c = 'abc', b = 42)", "f(3j, 42, c = 'abc')",
                    "f(c = 'abc', b = 42, a = 3j, d = 42)",  // extra argument
                    "f(3j, 42, 'abc', d = 42)",
                };

                foreach (var testCall in testCalls) {
                    var text = decl + Environment.NewLine + testCall;
                    var entry = ProcessText(text);

                    AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "pass")), ComplexType);
                    AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "pass")), IntType);
                    AssertContainsExactly(entry.GetTypesFromName("c", GetLineNumber(text, "pass")), StringType);
                }
            }
        }

        [TestMethod]
        public void TestPositionalSplat() {
            var funcDef = "def f(a, b, c): pass";            
            var classWithInit = @"class f(object):
    def __init__(self, a, b, c):
        pass";
            var classWithNew = @"class f(object):
    def __new__(cls, a, b, c):
        pass";
            var method = @"class x(object):
    def g(self, a, b, c):
        pass

f = x().g";
            var decls = new[] { funcDef, classWithInit, classWithNew, method };

            foreach (var decl in decls) {
                string[] testCalls = new[] { 
                    "f(*(3j, 42, 'abc'))", 
                    "f(*[3j, 42, 'abc'])", 
                    "f(*(3j, 42, 'abc', 4L))",  // extra argument
                    "f(*[3j, 42, 'abc', 4L])",  // extra argument
                };

                foreach (var testCall in testCalls) {
                    var text = decl + Environment.NewLine + testCall;
                    var entry = ProcessText(text);

                    AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "pass")), ComplexType);
                    AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "pass")), IntType);
                    AssertContainsExactly(entry.GetTypesFromName("c", GetLineNumber(text, "pass")), StringType);
                }
            }
        }
        [TestMethod]
        public void TestForwardRef() {
            var text = @"

class D(object):
    def bar(self, x):
        abc = C()
        abc.foo(2)
        a = abc.foo(2.0)
        a.bar(('a', 'b', 'c', 'd'))

class C(object):
    def foo(self, x):
        D().bar('abc')
        D().bar(['a', 'b', 'c'])
        return D()
    def baz(self): pass
";
            var entry = ProcessText(text);

            var fifty = entry.GetVariablesNoBuiltins(GetLineNumber(text, "abc.foo")).ToSet();
            AssertContainsExactly(fifty, "C", "D", "a", "abc", "self", "x");

            var three = entry.GetVariablesNoBuiltins(GetLineNumber(text, "lass D")).ToSet();
            AssertContainsExactly(three, "C", "D", "bar");

            var allFifty = entry.GetMembersFromName("abc", GetLineNumber(text, "abc.foo")).ToSet();
            AssertContainsExactly(allFifty, GetUnion(_objectMembers, "baz", "foo"));

            var xTypes = entry.GetTypesFromName("x", GetLineNumber(text, "abc.foo")).ToSet();
            AssertContainsExactly(xTypes, ListType, StringType, TupleType);

            var xMembers = entry.GetMembersFromName("x", GetLineNumber(text, "abc.foo")).ToSet();
            AssertContainsExactly(xMembers, GetIntersection(_strMembers, _listMembers));
        }

        private static int GetLineNumber(string text, string substring) {
            string[] splitLines = text.Split('\n');
            for (int i = 0; i < splitLines.Length; i++) {
                if (splitLines[i].IndexOf(substring) != -1) {
                    return i + 1;
                }
            }

            throw new InvalidOperationException();
        }

        [TestMethod]
        public void TestBuiltins() {
            var text = @"
booltypetrue = True
booltypefalse = False
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("booltypetrue", 1), BoolType);
            AssertContainsExactly(entry.GetTypesFromName("booltypefalse", 1), BoolType);
        }

        [TestMethod]
        public void TestDictionaryFunctionTable() {
            var text = @"
def f(a, b):
    print a, b
    
def g(a, b):
    x, y = a, b

x = {'foo': f, 'bar' : g}
x['foo'](42, [])
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "print")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "print")), ListType);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "x, y")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "x, y")), ListType);
        }

        [TestMethod]
        public void TestDictionaryAssign() {
            var text = @"
x = {'abc': 42}
y = x['foo']
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("y", 1), IntType);
        }

        [TestMethod]
        public void TestDictionaryFunctionTableGet2() {
            var text = @"
def f(a, b):
    print a, b
    
def g(a, b):
    x, y = a, b

x = {'foo': f, 'bar' : g}
x.get('foo')(42, [])
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "print")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "print")), ListType);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "x, y")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "x, y")), ListType);
        }

        [TestMethod]
        public void TestDictionaryFunctionTableGet() {
            var text = @"
def f(a, b):
    print a, b
    
def g(a, b):
    x, y = a, b

x = {'foo': f, 'bar' : g}
y = x.get('foo', None)
if y is not None:
    y(42, [])
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "print")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "print")), ListType);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "x, y")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "x, y")), ListType);
        }

        [TestMethod]
        public void TestSimpleGlobals() {
            var text = @"
class x(object):
    def abc(self):
        pass
        
a = x()
x.abc()
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetVariablesNoBuiltins(1), "a", "x");
            AssertContainsExactly(entry.GetMembersFromName("x", 1), GetUnion(_objectMembers, "abc"));
        }

        [TestMethod]
        public void TestFuncCallInIf() {
            var text = @"
def Method(a, b, c):
    print a, b, c
    
if not Method(42, 'abc', []):
    pass
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "print")), IntType);
            AssertContainsExactly(entry.GetTypesFromName("b", GetLineNumber(text, "print")), StringType);
            AssertContainsExactly(entry.GetTypesFromName("c", GetLineNumber(text, "print")), ListType);
        }

        [TestMethod]
        public void TestWithStatement() {
            var text = @"
class x(object):
    def x_method(self):
        pass
        
with x() as foo:
    print foo
    
with x():
    pass
";
            var entry = ProcessText(text);
            var foo = entry.GetMembersFromName("foo", GetLineNumber(text, "print foo"));
            AssertContainsExactly(foo, GetUnion(_objectMembers, "x_method"));
        }

        [TestMethod]
        public void TestOverrideFunction() {
            var text = @"
class bar(object):
    def Call(self, xvar, yvar):
        pass

class baz(bar):
    def Call(self, xvar, yvar):
        pass

class Cxxxx(object):
    def __init__(self):
        self.foo = baz()
        
    def Cmeth(self, avar, bvar):
        self.foo.Call(avar, bvar)
        


abc = Cxxxx()
abc.Cmeth(['foo'], 'bar')
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("xvar", GetLineNumber(text, "pass")), ListType);
        }

        [TestMethod]
        public void TestSimpleMethodCall() {
            var text = @"
class x(object):
    def abc(self, foo):
        pass
        
a = x()
a.abc('abc')
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("foo", GetLineNumber(text, "pass")), StringType);
            AssertContainsExactly(entry.GetMembersFromName("self", GetLineNumber(text, "pass")), GetUnion(_objectMembers, "abc"));
        }

        [TestMethod]
        public void TestSystemFromImport() {
            var text = @"
from System import Environment
Environment.GetCommandLineArgs()
";
            var entry = ProcessText(text);
            Assert.IsTrue(entry.GetMembersFromName("Environment", 1).Any(s => s == "CommandLine"));
        }

        [TestMethod]
        public void TestImportAs() {
            var text = @"
import System.Collections as coll
";
            var entry = ProcessText(text);
            Assert.IsTrue(entry.GetMembersFromName("coll", 1).Any(s => s == "ArrayList"));
        }

        [TestMethod]
        public void TestSystemImport() {
            var text = @"
import System
System.Environment.GetCommandLineArgs()
x = System.Environment
";
            var entry = ProcessText(text);
            var system = entry.GetMembersFromName("System", 1).ToSet();
            // defined in mscorlib
            AssertContains(system, "AccessViolationException");
            // defined in System
            AssertContains(system, "CodeDom");

            AssertContains(entry.GetMembersFromName("x", 1), "GetEnvironmentVariables");
        }

        [TestMethod]
        public void TestSystemMembers() {
            var text = @"
import System
System.Environment.GetCommandLineArgs()
x = System.Environment
args = x.GetCommandLineArgs()
";
            var entry = ProcessText(text);

            var args = entry.GetTypesFromName("args", GetLineNumber(text, "args =")).ToSet();
            AssertContainsExactly(args, ClrModule.GetPythonType(typeof(string[])));

            Assert.IsTrue(entry.GetMembersFromName("args", GetLineNumber(text, "args =")).Any(s => s == "AsReadOnly"));
        }

        [TestMethod]
        public void TestNamespaceMembers() {
            var text = @"
import System
x = System.Collections
";
            var entry = ProcessText(text);
            var x = entry.GetMembersFromName("x", GetLineNumber(text, "x =")).ToSet();
            Assert.IsTrue(x.Contains("Generic"));
            Assert.IsTrue(x.Contains("ArrayList"));
        }

        [TestMethod]
        public void TestBuiltinRetval() {
            var text = @"
x = [2,3,4]
a = x.index(2)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", GetLineNumber(text, "x =")).ToSet(), ListType);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "a =")).ToSet(), IntType);
        }

        [TestMethod]
        public void TestBuiltinFuncRetval() {
            var text = @"
x = ord('a')
y = range(5)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", GetLineNumber(text, "x = ")).ToSet(), IntType);
            AssertContainsExactly(entry.GetTypesFromName("y", GetLineNumber(text, "y = ")).ToSet(), ListType);
        }

        [TestMethod]
        public void TestFunctionMembers() {
            var text = @"
def f(x): pass
f.abc = 32
";
            var entry = ProcessText(text);
            AssertContains(entry.GetMembersFromName("f", 1), "abc");

            text = @"
def f(x): pass

";
            entry = ProcessText(text);
            AssertDoesntContain(entry.GetMembersFromName("f", 1), "x");
            AssertContainsExactly(entry.GetMembersFromName("f", 1), _functionMembers);

            AssertContainsExactly(entry.GetMembersFromName("f.func_name", 1), _strMembers);
        }


        [TestMethod]
        public void TestRangeIteration() {
            var text = @"
for i in range(5):
    pass
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("i", GetLineNumber(text, "for i")).ToSet(), IntType);
        }

        [TestMethod]
        public void TestBuiltinImport() {
            var text = @"
import sys
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetVariablesNoBuiltins(1), "sys");
            Assert.IsTrue(entry.GetMembersFromName("sys", 1).Any((s) => s == "winver"));
        }

        [TestMethod]
        public void TestBuiltinImportInFunc() {
            var text = @"
def f():
    import sys
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetVariablesNoBuiltins(GetLineNumber(text, "sys")), "f", "sys");
            AssertContains(entry.GetMembersFromName("sys", GetLineNumber(text, "sys")), "winver");
        }

        [TestMethod]
        public void TestBuiltinImportInClass() {
            var text = @"
class C:
    import sys
";
            var entry = ProcessText(text);

            AssertContainsExactly(entry.GetVariablesNoBuiltins(GetLineNumber(text, "sys")), "C", "sys");
            Assert.IsTrue(entry.GetMembersFromName("sys", GetLineNumber(text, "sys")).Any((s) => s == "winver"));
        }

        [TestMethod]
        public void TestNoImportClr() {
            var text = @"
x = 'abc'
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", 1), StringType);
            AssertContainsExactly(entry.GetMembersFromName("x", 1), _strMembers);
        }

        [TestMethod]
        public void TestImportClr() {
            var text = @"
import clr
x = 'abc'
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetMembersFromName("x", 1), _strMembersClr);
        }

        [TestMethod]
        public void TestClrAddReference() {
            var text = @"
import clr
clr.AddReference('System.Drawing')
from System.Drawing import Point
";
            var entry = ProcessText(text);
            Assert.AreEqual(35, entry.GetMembersFromName("Point", GetLineNumber(text, "from System.")).ToList().Count);
        }

        [TestMethod]
        public void TestClrAddReferenceByName() {
            var text = @"
import clr
clr.AddReferenceByName('Microsoft.Scripting')
from Microsoft.Scripting import SourceUnit
";
            var entry = ProcessText(text);
            Assert.AreEqual(40, entry.GetMembersFromName("SourceUnit", GetLineNumber(text, "from Microsoft.")).ToList().Count);
        }

        [TestMethod]
        public void TestMutualRecursion() {
            var text = @"
class C:
    def f(self, other, depth):
        if depth == 0:
            return 'abc'
        return other.g(self, depth - 1)

class D:
    def g(self, other, depth):
        if depth == 0:
            return ['d', 'e', 'f']
        
        return other.f(self, depth - 1)

x = D().g(C(), 42)

";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetMembersFromName("other", GetLineNumber(text, "other.g")), "g");
            AssertContainsExactly(entry.GetTypesFromName("x", GetLineNumber(text, "x =")), ListType, StringType);
            AssertContainsExactly(entry.GetMembersFromName("x", GetLineNumber(text, "x =")),
                GetIntersection(_listMembers, _strMembers));
        }

        [TestMethod]
        public void TestForwardRefVars() {
            var text = @"
class x(object):
    def __init__(self, val):
        self.abc = []
    
x(42)
x('abc')
x([])
";
            var entry = ProcessText(text);
            Assert.AreEqual(1, entry.GetValues("self.abc", GetLineNumber(text, "self.abc")).ToList().Count);
        }

        [TestMethod]
        public void TestReturnFunc() {
            var text = @"
def g():
    return []

def f():
    return g
    
x = f()()
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", 1), ListType);
        }

        [TestMethod]
        public void TestReturnArg() {
            var text = @"
def g(a):
    return a

x = g(1)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", 1), IntType);
        }

        [TestMethod]
        public void TestReturnArg2() {
            var text = @"

def f(a):
    def g():
        return a
    return g

x = f(2)()
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("x", 1), IntType);
        }

        [TestMethod]
        public void TestMemberAssign() {
            var text = @"
class C:
    def func(self):
        self.abc = 42

a = C()
a.func()
foo = a.abc
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("foo", 1), IntType);
            AssertContainsExactly(entry.GetMembersFromName("foo", 1), _intMembers);
            AssertContainsExactly(entry.GetMembersFromName("a", 1), "abc", "func");
        }

        [TestMethod]
        public void TestMemberAssign2() {
            var text = @"
class D:
    def func2(self):
        a = C()
        a.func()
        return a.abc

class C:
    def func(self):
        self.abc = [2,3,4]

foo = D().func2()
";
            var entry = ProcessText(text);
            // TODO: AssertContainsExactly(entry.GetTypesFromName("foo", 0), ListType);
        }

        [TestMethod]
        public void TestUnfinishedDot() {
            // the partial dot should be ignored and we shouldn't see g as
            // a member of D
            var text = @"
class D(object):
    def func(self):
        self.
        
def g(a, b, c): pass
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetMembersFromName("self", GetLineNumber(text, "self")),
                GetUnion(_objectMembers, "func"));
        }

        [TestMethod]
        public void TestCrossModule() {
            var text1 = @"
import mod2
";
            var text2 = @"
x = 42
";

            PermutedTest("mod", new[] { text1, text2 }, (pe) => {
                AssertContainsExactly(pe[0].Analysis.GetMembersFromName("mod2", 1), "x");
            });
        }

        [TestMethod]
        public void TestCrossModuleCall() {
            var text1 = @"
import mod2
y = mod2.f('abc')
";
            var text2 = @"
def f(x):
	return x
";

            PermutedTest("mod", new[] { text1, text2 }, (pe) => {
                AssertContainsExactly(pe[1].Analysis.GetTypesFromName("x", GetLineNumber(text2, "return x")), StringType);
                AssertContainsExactly(pe[0].Analysis.GetTypesFromName("y", GetLineNumber(text1, "y")), StringType);
            });
        }

        [TestMethod]
        public void TestCrossModuleCallType() {
            var text1 = @"
import mod2
y = mod2.c('abc').x
";
            var text2 = @"
class c:
    def __init__(self, x):
        self.x = x
";

            PermutedTest("mod", new[] { text1, text2 }, (pe) => {
                AssertContainsExactly(pe[1].Analysis.GetTypesFromName("x", GetLineNumber(text2, "= x")), StringType);
                AssertContainsExactly(pe[0].Analysis.GetTypesFromName("y", GetLineNumber(text1, "y")), StringType);
            });
        }

        [TestMethod]
        public void TestCrossModuleCallType2() {
            var text1 = @"
from mod2 import c
class x(object):
    def Foo(self):
        y = c('abc').x
";
            var text2 = @"
class c:
    def __init__(self, x):
        self.x = x
";

            PermutedTest("mod", new[] { text1, text2 }, (pe) => {
                AssertContainsExactly(pe[1].Analysis.GetTypesFromName("x", GetLineNumber(text2, "= x")), StringType);
                AssertContainsExactly(pe[0].Analysis.GetTypesFromName("y", GetLineNumber(text1, "y =")), StringType);
            });
        }

        [TestMethod]
        public void TestCrossModuleFuncAndType() {
            var text1 = @"
class Something(object):
    def f(self): pass
    def g(self): pass


def SomeFunc():
    x = Something()
    return x
";
            var text2 = @"
from mod1 import SomeFunc

x = SomeFunc()
";

            var text3 = @"
from mod2 import x
a = x
";

            PermutedTest("mod", new[] { text1, text2, text3 }, (pe) => {
                AssertContainsExactly(pe[2].Analysis.GetMembersFromName("a", GetLineNumber(text3, "a = ")),
                    GetUnion(_objectMembers, "f", "g"));
            });
        }

        [TestMethod]
        public void TestMembersAfterError() {
            var text = @"
class X(object):
    def f(self):
        return self.
        
    def g(self):
        pass
        
    def h(self):
        pass
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetMembersFromName("self", GetLineNumber(text, "self")),
                GetUnion(_objectMembers, "f", "g", "h"));
        }

        [TestMethod]
        public void TestGenericIndexing() {
            // indexing into a generic type should know how the type info
            // flows through
            var text = @"
from System.Collections.Generic import List
x = List[int]()
";
            var entry = ProcessText(text);

            // AreEqual(entry.GetMembersFromName('x', len(text) - 1), 
            //     get_intersect_members_clr(List[int]))
            var self = new List<string>(entry.GetMembersFromName("x", GetLineNumber(text, "x =")));
            Assert.IsTrue(self.Contains("AddRange"));
        }

        [TestMethod]
        public void TestReturnTypesCollapsing() {
            // indexing into a generic type should know how the type info
            // flows through
            var text = @"
from System import AppDomain
asm = AppDomain.CurrentDomain.DefineDynamicAssembly()
mod = asm.DefineDynamicModule()
mod.
";
            var entry = ProcessText(text);
            var tooltips = entry.GetMembers("mod.", GetLineNumber(text, "mod ="))
                .Where(m => m.Name == "CreateGlobalFunctions")
                .Select(m => m.ToolTip)
                .ToArray();
            Assert.AreEqual(1, tooltips.Length);

            var indexes = tooltips[0].FindIndexesOf("CreateGlobalFunctions").ToArray();
            Assert.AreEqual(1, indexes.Length);
        }

        [TestMethod]
        public void TestProperty() {
            var text = @"
class x(object):
    @property
    def SomeProp(self):
        return 42

a = x().SomeProp
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "a =")), IntType);
        }

        [TestMethod]
        public void TestStaticMethod() {
            var text = @"
class x(object):
    @staticmethod
    def StaticMethod(value):
        return value

a = x().StaticMethod(4.0)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "a = ")), FloatType);
        }

        [TestMethod]
        public void TestClassMethod() {
            var text = @"
class x(object):
    @classmethod
    def ClassMethod(cls):
        return cls

a = x().ClassMethod()
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "a =")), TypeType);
        }

        [TestMethod]
        public void TestAssignSelf() {
            var text = @"
class x(object):
    def __init__(self):
        self.x = 'abc'
    def f(self):
        pass
";
            var entry = ProcessText(text);
            AssertContains(entry.GetMembersFromName("self", GetLineNumber(text, "pass")), "x");
            AssertContainsExactly(entry.GetMembers("self.x.", GetLineNumber(text, "pass")).Select(m => m.Name), _strMembers);
        }

        [TestMethod]
        public void TestAssignEvent() {
            var text = @"
import System

def f(sender, args):
    pass
    
System.AppDomain.CurrentDomain.AssemblyLoad += f
";
            var entry = ProcessText(text);
            Assert.IsTrue(entry.GetMembersFromName("args", GetLineNumber(text, "pass")).Any(s => s == "LoadedAssembly"));
        }

        /*
        [TestMethod]
        public void TestOverrideParams() {
            var text = @"
import System

class MyArrayList(System.Collections.ArrayList):
    def AddRange(self, col):
        x = col
";
            var entry = ProcessText(text);
            var x = entry.GetMembersFromName("x", text.IndexOf("x = col")).ToSet();
            AssertContainsExactly(x, GetMembers(ClrModule.GetPythonType(typeof(System.Collections.ICollection)), true));
        }*/

        class EmptyAnalysisCookie : IAnalysisCookie {
            public static EmptyAnalysisCookie Instance = new EmptyAnalysisCookie();
            public string GetLine(int lineNo) {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void TestPackage() {
            var src1 = GetSourceUnit("", @"C:\\Test\\Lib\\foo\\__init__.py");

            var src2 = GetSourceUnit(@"
from foo.y import abc
import foo.y as y
", @"C:\\Test\\Lib\\foo\\x.py");

            var src3 = GetSourceUnit(@"
abc = 42
", @"C:\\Test\\Lib\\foo\\y.py");

            var state = new ProjectState(_engine);
            
            var package = state.AddModule("foo", src1.Path, EmptyAnalysisCookie.Instance);
            var x = state.AddModule("foo.x", src2.Path, EmptyAnalysisCookie.Instance);
            var y = state.AddModule("foo.y", src3.Path, EmptyAnalysisCookie.Instance);

            Prepare(package, src1);
            Prepare(x, src2);
            Prepare(y, src3);

            package.Analyze();
            x.Analyze();
            y.Analyze();

            Assert.AreEqual(x.Analysis.GetValues("y", 1).First().Description, "Python module foo.y");
            AssertContainsExactly(x.Analysis.GetTypesFromName("abc", 1), IntType);
        }

        private static void Prepare(ProjectEntry entry, SourceUnit sourceUnit) {
            CollectingErrorSink errorSink = new CollectingErrorSink();
            using (var parser = Utils.CreateParser(sourceUnit, errorSink)) {
                entry.UpdateTree(parser.ParseFile(true), null);
            }
        }


        /// <summary>
        /// Verify that the analyzer has the proper algorithm for turning a filename into a package name
        /// </summary>
        [TestMethod]
        public void TestPathToModuleName() {
            string nzmathPath = Path.Combine(Environment.GetEnvironmentVariable("DLR_ROOT"), @"External.LCA_RESTRICTED\Languages\IronPython\Math");

            Assert.AreEqual(PythonAnalyzer.PathToModuleName(Path.Combine(nzmathPath, @"nzmath\factor\__init__.py")), "nzmath.factor");
            Assert.AreEqual(PythonAnalyzer.PathToModuleName(Path.Combine(nzmathPath, @"nzmath\factor\find.py")), "nzmath.factor.find");
        }

        [TestMethod]
        public void TestDefaults() {
            var text = @"
def f(x = 42):
    return x
    
a = f()
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("a", GetLineNumber(text, "a =")), IntType);
        }

        [TestMethod]
        public void TestNegCallProperty() {
            // invalid code, this shouldn't crash us.
            var text = @"
import System
x = System.String.Length()
y = System.Environment.CurrentDirectory()
";
            ProcessText(text);
        }

        [TestMethod]
        public void TestClassInit() {
            var text = @"
class X:
    def __init__(self, value):
        self.value = value

a = X(2)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("value", GetLineNumber(text, " = value")), IntType);
        }

        [TestMethod]
        public void TestClassNew() {
            var text = @"
class X:
    def __new__(cls, value):
        res = object.__new__(cls)
        res.value = value
        return res

a = X(2)
";
            var entry = ProcessText(text);
            AssertContainsExactly(entry.GetTypesFromName("value", GetLineNumber(text, " = value")), IntType);
        }

        private static IEnumerable<string> GetVariableDescriptions(ModuleAnalysis entry, string variable, int position) {
            return entry.GetValues(variable, position).Select(m => m.Description);
        }

        [TestMethod]
        public void TestQuickInfo() {
            var text = @"
import System
import sys
from System.Collections import ArrayList
a = 41.0
b = 42L
c = 'abc'
x = (2, 3, 4)
y = [2, 3, 4]
z = 43

class foo(object):
    @property
    def f(self): pass

    def g(self): pass
    
d = foo()

e = System.Collections.ArrayList()

def f():
    print 'hello'
    return 'abc'

def g():
    return System
    return c.Length
";
            var entry = ProcessText(text);

            AssertContainsExactly(GetVariableDescriptions(entry, "foo()", 1), "foo instance");
            AssertContainsExactly(GetVariableDescriptions(entry, "int()", 1), "int");
            AssertContainsExactly(GetVariableDescriptions(entry, "a", 1), "float");
            AssertContainsExactly(GetVariableDescriptions(entry, "a", 1), "float");
            AssertContainsExactly(GetVariableDescriptions(entry, "b", 1), "long");
            AssertContainsExactly(GetVariableDescriptions(entry, "c", 1), "str");
            AssertContainsExactly(entry.GetValues("x", 1).Select(v => v.Description.Substring(0, 5)), "tuple");
            AssertContainsExactly(entry.GetValues("y", 1).Select(v => v.Description.Substring(0, 4)), "list");
            AssertContainsExactly(GetVariableDescriptions(entry, "z", 1), "int");
            AssertContainsExactly(GetVariableDescriptions(entry, "min", 1), "built-in function min");
            AssertContainsExactly(GetVariableDescriptions(entry, "list.append", 1), "built-in method append");
            AssertContainsExactly(GetVariableDescriptions(entry, "System.String.Length", 1), "property of type <type 'int'>");
            AssertContainsExactly(GetVariableDescriptions(entry, "\"abc\".Length", 1), "int");
            AssertContainsExactly(GetVariableDescriptions(entry, "c.Length", 1), "int");
            AssertContainsExactly(GetVariableDescriptions(entry, "System.Environment.CurrentDirectory", 1), "str");
            AssertContainsExactly(GetVariableDescriptions(entry, "d", 1), "foo instance");
            AssertContainsExactly(GetVariableDescriptions(entry, "e", 1), "ArrayList");
            AssertContainsExactly(GetVariableDescriptions(entry, "ArrayList", 1), "type ArrayList");
            AssertContainsExactly(GetVariableDescriptions(entry, "e.Count", 1), "int");
            AssertContainsExactly(GetVariableDescriptions(entry, "sys", 1), "built-in module sys");
            AssertContainsExactly(GetVariableDescriptions(entry, "f", 1), "def f(...)");
            AssertContainsExactly(GetVariableDescriptions(entry, "foo.f", 1), "def f(...)");
            AssertContainsExactly(GetVariableDescriptions(entry, "foo().g", 1), "method g of foo objects ");
            AssertContainsExactly(GetVariableDescriptions(entry, "foo", 1), "class foo");
            AssertContainsExactly(GetVariableDescriptions(entry, "System.DBNull.Value", 1), "DBNull");
            AssertContainsExactly(GetVariableDescriptions(entry, "System.StringSplitOptions", 1), "type StringSplitOptions");
            //AssertContainsExactly(GetVariableDescriptions(entry, "System.StringSplitOptions.RemoveEmptyEntries", 0), "field of type StringSplitOptions");
            AssertContainsExactly(GetVariableDescriptions(entry, "g", 1), "def g(...)");    // return info could be better
            AssertContainsExactly(GetVariableDescriptions(entry, "System.AppDomain.DomainUnload", 1), "event of type System.EventHandler");
            AssertContainsExactly(GetVariableDescriptions(entry, "None", 1), "None");


        }

        [TestMethod]
        public void TestRecurisveDataStructures() {
            var text = @"
d = {}
d[0] = d
";
            var entry = ProcessText(text);

            AssertContainsExactly(GetVariableDescriptions(entry, "d", 1), "dict({int : dict}");
        }

        /// <summary>
        /// Variable is refered to in the base class, defined in the derived class, we should know the type information.
        /// </summary>
        [TestMethod]
        public void TestBaseReferencedDerivedDefined() {
            var text = @"
class Base(object):
    def f(self):
        x = self.map

class Derived(Base):
    def __init__(self):
        self.map = {}

pass
";

            var entry = ProcessText(text);
            var members = entry.GetMembers("Derived().", GetLineNumber(text, "pass")).ToArray();
            var map = members.First(x => x.Name == "map");

            Assert.AreEqual(map.MemberType, ResultType.Field);
        }

        #endregion

        #region Helpers

        static AnalysisTest() {
            _engine = Python.CreateEngine();
            PyObjectType = ClrModule.GetPythonType(typeof(object));
            IntType = ClrModule.GetPythonType(typeof(int));
            ComplexType = ClrModule.GetPythonType(typeof(Complex));
            StringType = ClrModule.GetPythonType(typeof(string));
            FloatType = ClrModule.GetPythonType(typeof(double));
            TypeType = ClrModule.GetPythonType(typeof(PythonType));
            ListType = ClrModule.GetPythonType(typeof(List));
            TupleType = ClrModule.GetPythonType(typeof(PythonTuple));
            BoolType = ClrModule.GetPythonType(typeof(bool));
            FunctionType = ClrModule.GetPythonType(typeof(PythonFunction));
            GeneratorType = ClrModule.GetPythonType(typeof(PythonGenerator));

            _objectMembers = GetMembers(PyObjectType, false);
            _strMembers = GetMembers(StringType, false);
            _listMembers = GetMembers(ListType, false);
            _intMembers = GetMembers(IntType, false);
            _functionMembers = GetMembers(FunctionType, false);

            _objectMembersClr = GetMembers(PyObjectType, true);
            _strMembersClr = GetMembers(StringType, true);

            Assert.IsTrue(_objectMembers.Length < _objectMembersClr.Length);
            Assert.IsTrue(_strMembers.Length < _strMembersClr.Length);
        }

        private static string[] GetMembers(object obj, bool showClr) {
            var dir = showClr ? ClrModule.DirClr(obj) : ClrModule.Dir(obj);
            int len = dir.__len__();
            string[] result = new string[len];
            for (int i = 0; i < len; i++) {
                Assert.IsTrue(dir[i] is string);
                result[i] = dir[i] as string;
            }
            return result;
        }

        private static SourceUnit GetSourceUnit(string text, string name) {
            var textContent = new MyTextContentProvider(text);
            var languageContext = HostingHelpers.GetLanguageContext(_engine);
            return new SourceUnit(languageContext, textContent, name, SourceCodeKind.File);
        }

        private static SourceUnit GetSourceUnit(string text) {
            return GetSourceUnit(text, "foo");
        }

        private static ModuleAnalysis ProcessText(string text) {
            var sourceUnit = GetSourceUnit(text, "foo");
            return ParseText(new ProjectState(_engine), sourceUnit, "foo").Analysis;
        }

        /// <summary>
        /// Returns all the permutations of the set [0 ... n-1]
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static IEnumerable<List<int>> Permutations(int n) {
            if (n <= 0) {
                yield return new List<int>();
            } else {
                foreach (var prev in Permutations(n - 1)) {
                    for (int i = n - 1; i >= 0; i--) {
                        var result = new List<int>(prev);
                        result.Insert(i, n - 1);
                        yield return result;
                    }
                }
            }
        }

        private IEnumerable<ProjectEntry[]> MakeModulePermutations(string prefix, string[] code) {
            foreach (var p in Permutations(code.Length)) {
                var result = new ProjectEntry[code.Length];
                var state = new ProjectState(_engine);
                for (int i = 0; i < code.Length; i++) {
                    result[p[i]] = state.AddModule(prefix + (p[i] + 1).ToString(), "foo", null);
                }
                for (int i = 0; i < code.Length; i++) {
                    Prepare(result[p[i]], GetSourceUnit(code[p[i]]));
                }
                for (int i = 0; i < code.Length; i++) {
                    result[p[i]].Analyze();
                }
                yield return result;
            }
        }

        /// <summary>
        /// For a given set of module definitions, build analysis info for each unique permutation
        /// of the ordering of the defintions and run the test against each analysis.
        /// </summary>
        /// <param name="prefix">Prefix for the module names. The first source text will become prefix + "1", etc.</param>
        /// <param name="code">The source code for each of the modules</param>
        /// <param name="test">The test to run against the analysis</param>
        private void PermutedTest(string prefix, string[] code, Action<ProjectEntry[]> test) {
            foreach (var pe in MakeModulePermutations(prefix, code)) {
                test(pe);
            }
        }

        private string MakeText<T>(IEnumerable<T> values) {
            var sb = new StringBuilder("{");
            foreach (var value in values) {
                if (sb.Length > 1) {
                    sb.Append(", ");
                }
                if (value is PythonType) {
                    sb.AppendFormat("Type({0})", PythonType.Get__name__((PythonType)(object)value));
                } else {
                    sb.Append(value.ToString());
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        private void AssertContains<T>(IEnumerable<T> source, T value) {
            foreach (var v in source) {
                if (v.Equals(value)) {
                    return;
                }
            }

            Assert.Fail(String.Format("{0} does not contain {1}", MakeText(source), value));
        }

        private void AssertDoesntContain<T>(IEnumerable<T> source, T value) {
            foreach (var v in source) {
                if (v.Equals(value)) {
                    Assert.Fail(String.Format("{0} does not contain {1}", MakeText(source), value));
                }
            }

        }

        private void AssertContainsExactly<T>(IEnumerable<T> source, params T[] values) {
            AssertContainsExactly(new HashSet<T>(source), values);
        }

        private void AssertContainsExactly<T>(HashSet<T> set, params T[] values) {
            if (set.ContainsExactly(values)) {
                return;
            }
            Assert.Fail(String.Format("Expected {0}, got {1}", MakeText(values), MakeText(set)));
        }

        private static string[] GetUnion(params object[] objs) {
            var result = new HashSet<string>();
            foreach (var obj in objs) {
                if (obj is string) {
                    result.Add((string)obj);
                } else if (obj is IEnumerable<string>) {
                    result.UnionWith((IEnumerable<string>)obj);
                } else {
                    throw new NotImplementedException("Non-string member");
                }
            }
            return result.ToArray();
        }

        private static string[] GetIntersection(IEnumerable<string> first, params IEnumerable<string>[] remaining) {
            var result = new HashSet<string>(first);
            foreach (var obj in remaining) {
                result.IntersectWith((IEnumerable<string>)obj);
            }
            return result.ToArray();
        }

        #endregion
    }
}
