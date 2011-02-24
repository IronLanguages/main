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
using System.Linq;
using System.Text;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IronPythonTools.Library;
using System.Threading;

namespace UnitTests {
    partial class Program {
        public static IContentType PythonContentType = new MockContentType("Python", new IContentType[0]);
        public static ScriptEngine PythonEngine = Python.CreateEngine();

        [TestMethod]
        public void Scenario_MemberCompletion() {
            // TODO: Negative tests
            //       Import / from import tests
            MemberCompletionTest(-1, "x = 2\r\nx.", "x.");
            
            // combining various partial expressions with previous expressions
            var prefixes = new[] { "", "(", "a = ", "f(", "l[", "{", "if " };
            var exprs = new[] { "x[0].", "x(0).", "x", "x.y.", "f(x[2]).", "f(x, y).", "f({2:3}).", "f(a + b).", "f(a or b).", "{2:3}.", "f(x if False else y).", "(\r\nx\r\n)." };
            foreach (var prefix in prefixes) {
                foreach (var expr in exprs) {
                    string test = prefix + expr;
                    Console.WriteLine("   -- {0}", test);
                    MemberCompletionTest(-1, test, expr);
                }
            }
            
            var sigs = new[] { 
                new { Expr = "f(", Param = 0, Function="f" } ,
                new { Expr = "f(1,", Param = 1, Function="f" },
                new { Expr = "f(1, 2,", Param = 2, Function="f" }, 
                new { Expr = "f(1, (1, 2),", Param = 2, Function="f" }, 
                new { Expr = "f(1, a + b,", Param = 2, Function="f" }, 
                new { Expr = "f(1, a or b,", Param = 2, Function="f" }, 
                new { Expr = "f(1, a if True else b,", Param = 2, Function="f" }, 
                new { Expr = "a.f(1, a if True else b,", Param = 2, Function="a.f" }, 
                new { Expr = "a().f(1, a if True else b,", Param = 2, Function="a().f" }, 
                new { Expr = "a(2, 3, 4).f(1, a if True else b,", Param = 2, Function="a(2, 3, 4).f" }, 
                new { Expr = "a(2, (3, 4), 4).f(1, a if True else b,", Param = 2, Function="a(2, (3, 4), 4).f" }, 
            };
            
            foreach (var prefix in prefixes) {
                foreach (var sig in sigs) {
                    var test  = prefix + sig.Expr;
                    Console.WriteLine("   -- {0}", test);
                    SignatureTest(-1, test, sig.Function, sig.Param);
                }
            }
        }

        [TestMethod]
        public void Scenario_GotoDefinition() {
            string code = @"
class C:
    def fff(self): pass

C().fff";

            var emptyAnalysis = AnalyzeExpression(0, code);
            AreEqual(emptyAnalysis.Expression, "");

            for (int i = -1; i >= -3; i--) {
                var analysis = AnalyzeExpression(i, code);
                AreEqual(analysis.Expression, "C().fff");
            }

            var classAnalysis = AnalyzeExpression(-4, code);
            AreEqual(classAnalysis.Expression, "C()");

            var defAnalysis = AnalyzeExpression(code.IndexOf("def fff")+4, code);
            AreEqual(defAnalysis.Expression, "fff");
        }

        private static ExpressionAnalysis AnalyzeExpression(int location, string sourceCode) {
            if (location < 0) {
                location = sourceCode.Length + location;
            }

            var analyzer = new PythonAnalyzer(new MockDlrRuntimeHost(), new MockErrorProviderFactory());
            var buffer = new MockTextBuffer(sourceCode);
            var textView = new MockTextView(buffer);
            var item = analyzer.AnalyzeTextView(textView);
            while (item.IsAnalyzed) {
                Thread.Sleep(100);
            }
            
            var snapshot = (MockTextSnapshot)buffer.CurrentSnapshot;

            return analyzer.AnalyzeExpression(snapshot, buffer, new MockTrackingSpan(snapshot, location, 0));
        }

        private static void MemberCompletionTest(int location, string sourceCode, string expectedExpression) {
            if (location < 0) {
                location = sourceCode.Length + location;
            }

            var analyzer = new PythonAnalyzer(new MockDlrRuntimeHost(), new MockErrorProviderFactory());
            var buffer = new MockTextBuffer(sourceCode);
            var snapshot = (MockTextSnapshot)buffer.CurrentSnapshot;
            var context = analyzer.GetCompletions(snapshot, buffer, new MockTrackingSpan(snapshot, location, 1));
            AreEqual(context.Text, expectedExpression);            
        }

        private static void SignatureTest(int location, string sourceCode, string expectedExpression, int paramIndex) {
            if (location < 0) {
                location = sourceCode.Length + location;
            }

            var analyzer = new PythonAnalyzer(new MockDlrRuntimeHost(), new MockErrorProviderFactory());
            var buffer = new MockTextBuffer(sourceCode);
            var snapshot = (MockTextSnapshot)buffer.CurrentSnapshot;
            var context = analyzer.GetSignatures(snapshot, buffer, new MockTrackingSpan(snapshot, location, 1));
            AreEqual(context.Text, expectedExpression);
            AreEqual(context.ParameterIndex, paramIndex);
        }
    }
}
