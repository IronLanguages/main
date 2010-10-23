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
using Microsoft.IronStudio;
using IronPython.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronStudio.RemoteEvaluation;

namespace UnitTests {
    [TestClass]
    public partial class Program {
        public static void Main(string[] args) {
            var inst = new Program();
            foreach (var method in typeof(Program).GetMethods()) {
                if (method.Name.StartsWith("Scenario_")) {
                    if (args.Length > 0 && !args.Contains(method.Name)) {
                        Console.Write("Skipping Scenario {0} ", method.Name);
                        continue;
                    }
                    Console.Write("Scenario {0} ", method.Name);
                    try {
                        method.Invoke(inst, new object[0]);
                        Console.WriteLine("PASSED");
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine("FAILED");
                    }
                }
            }
        }

        [TestMethod]
        public void Scenario_RemoteScriptFactory() {
            using (var factory = RemotePythonEvaluator.CreateFactory()) {
                var runtime = (ScriptRuntime)factory.CreateRuntime(Python.CreateRuntimeSetup(new Dictionary<string, object>()));
                StringWriter writer = new StringWriter();
                StringWriter errWriter = new StringWriter();

                runtime.IO.SetOutput(Stream.Null, writer);
                factory.SetConsoleOut(writer);
                factory.SetConsoleError(errWriter);

                // verify print goes to the correct output
                var engine = runtime.GetEngine("Python");
                engine.Execute("print 'hello'");
                var builder = writer.GetStringBuilder();

                AreEqual(builder.ToString(), "hello\r\n");
                builder.Clear();

                // verify Console.WriteLine is redirected
                engine.Execute("import System\nSystem.Console.WriteLine('hello')\n");
                AreEqual(builder.ToString(), "hello\r\n");
                builder.Clear();

                // verify Console.Error.WriteLine is redirected to stderr
                var errBuilder = errWriter.GetStringBuilder();
                engine.Execute("import System\nSystem.Console.Error.WriteLine('hello')\n");
                AreEqual(errBuilder.ToString(), "hello\r\n");
                errBuilder.Clear();

                // raise an exception, should be propagated back
                try {
                    engine.Execute("import System\nraise System.ArgumentException()\n");
                    AreEqual(true, false);
                } catch (ArgumentException) {
                }

                /*
                 // verify that all code runs on the same thread
                var scope = engine.CreateScope();
                engine.Execute("import System");

            
                List<object> res = new List<object>();
                for (int i = 0; i < 100; i++) {
                    ThreadPool.QueueUserWorkItem(
                        (x) => {
                            object value = engine.Execute("System.Threading.Thread.CurrentThread.ManagedThreadId", scope);
                            lock (res) {
                                res.Add(value);
                            }
                    });
                }

                while (res.Count != 100) {
                    Thread.Sleep(100);
                }

                for (int i = 1; i < res.Count; i++) {
                    if (!res[i - 1].Equals(res[i])) {
                        throw new Exception("running on multiple threads");
                    }
                }*/

                // create a long running work item, execute it, and then make sure we can continue to execute work items.
                ThreadPool.QueueUserWorkItem(x => {
                    engine.Execute("while True: pass");
                });
                Thread.Sleep(1000);
                factory.Abort();

                AreEqual(engine.Execute("42"), 42);
            }

            // check starting on an MTA thread
            using (var factory = new RemoteScriptFactory(ApartmentState.MTA)) {
                var runtime = (ScriptRuntime)factory.CreateRuntime(Python.CreateRuntimeSetup(new Dictionary<string, object>()));
                var engine = runtime.GetEngine("Python");
                AreEqual(engine.Execute("import System\nSystem.Threading.Thread.CurrentThread.ApartmentState == System.Threading.ApartmentState.MTA"), true);
            }

            // check starting on an STA thread
            using (var factory = new RemoteScriptFactory(ApartmentState.STA)) {
                var runtime = (ScriptRuntime)factory.CreateRuntime(Python.CreateRuntimeSetup(new Dictionary<string, object>()));
                var engine = runtime.GetEngine("Python");
                AreEqual(engine.Execute("import System\nSystem.Threading.Thread.CurrentThread.ApartmentState == System.Threading.ApartmentState.STA"), true);
            }
        }

        private static void AreEqual(object x, object y) {
            if (!x.Equals(y)) {
                throw new Exception(String.Format("Expected {0}, got {1}", y, x));
            }
        }
    }
}
