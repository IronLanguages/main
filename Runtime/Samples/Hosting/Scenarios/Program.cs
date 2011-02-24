using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Security;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

// TODO:
//if (ops.IsInvokable(obj, memberName)) {
//    foreach (string signature in ops.GetCallSignatures(obj, memberName)) {
//        Console.WriteLine("  {0}", signature);
//    }
//}

namespace Scenarios {
    public class Scenarios {
        #region Helpers

        private static IEnumerable<string> ConsoleLines() {
            string line;
            while (!String.IsNullOrEmpty(line = Console.ReadLine())) {
                yield return line;
            }
        }

        private static IEnumerable<string> TypeLines(IEnumerable<string> lines) {
            foreach (var line in lines) {
                Console.WriteLine(line);
                yield return line;
            }
        }

        #endregion

        static void Main(string[] args) {
            //Scenario_REPL(ConsoleLines());

            // level 1
            Scenario_ExecuteFile_Push();
            Scenario_ExecuteFile_Pull();

            //// level 2:
            Scenario_REPL(TypeLines(new[] {
                "command_1",
                "command_2",
                ".py",
                "dir()",
                "x = (1,2,3)",
                ".rb",
                "p x"
            }));

            Scenario_CompiledCode();

            //// level 3:
            Scenario_Introspection();
            Scenario_RemoteIntrospection(true);
            Scenario_RemoteIntrospection(false);
            Scenario_RemoteEvaluation();
            Scenario_RemoteEvaluationInSandbox();
        }

        #region Level 1

        public class MyHostObjectModel {
            public Dictionary<string, Action> UserCommands = new Dictionary<string, Action>();
        }

        /// <summary>
        /// Shows setting Host OM on globals so that dynamic languages
        /// can import, require, etc., to access the host's OM.
        /// </summary>
        public static void Scenario_ExecuteFile_Push() {
            var hostOM = new MyHostObjectModel();
            var runtime = ScriptRuntime.CreateFromConfiguration();

            runtime.Globals.SetVariable("App", hostOM);
            runtime.ExecuteFile("register_user_commands.py");
            
            hostOM.UserCommands["foo"]();
        }

        /// <summary>
        /// Shows discovering command implementations from globals in scope.
        /// Above user code explicitly added commands, but this code finds
        /// commands matching a delegate type.
        /// </summary>
        public static void Scenario_ExecuteFile_Pull() {
            var runtime = ScriptRuntime.CreateFromConfiguration();
            var scope = runtime.ExecuteFile("user_commands.rb");

            var commands = new Dictionary<string, Action>();
            foreach (string name in scope.GetVariableNames()) {
                try {
                    commands[name] = (Action)scope.GetVariable(name);
                } catch (RuntimeBinderException) {
                }
            }

            foreach (var command in commands.Values) {
                command();
            }
        }

        #endregion

        #region Level 2

        private static void Scenario_REPL(IEnumerable<string> lines) {
            var runtime = ScriptRuntime.CreateFromConfiguration();

            // Assume user has chosen to execute a file, and you want to set the REPL's context to that file's scope.
            dynamic scope = runtime.ExecuteFile("user_commands.rb");
            ScriptEngine engine = scope.Engine;

            Console.Write("{0}> ", engine.Setup.DisplayName);
            foreach (string line in lines) {
                if (line.StartsWith(".")) {
                    // change the current language:
                    engine = runtime.GetEngine(line.Substring(1).Trim());
                } else {
                    // Use interactive script source to enable interactive code specific language features:
                    ScriptSource input = engine.CreateScriptSourceFromString(line, SourceCodeKind.InteractiveCode);

                    try {
                        // execute the command:
                        scope._ = input.Execute(scope);
                    } catch (Exception exception) {
                        // display the exception in a format of the current language:
                        Console.WriteLine(engine.GetService<ExceptionOperations>().FormatException(exception));
                    }

                }

                Console.Write("{0}> ", engine.Setup.DisplayName);
            }
        }

        // MerlinWeb:
        public static void Scenario_CompiledCode() {
            var runtime = ScriptRuntime.CreateFromConfiguration();

            // code behind linked from .aspx file:
            string url = "merlin_web_page_code_behind.py";

            var engine = runtime.GetEngineByFileExtension(Path.GetExtension(url));
            CompiledCode compiledCode = engine.CreateScriptSourceFromFile(url).Compile();

            // 5 requests for same page:
            for (int i = 0; i < 5; i++) {
                // on each request, create new scope with a custom dictionary for latebound look up of elements on page. 
                // This uses a derived type of DynamicObject for convenience.
                var page = new DynamicPage();

                ScriptScope scope = engine.CreateScope(page);
                compiledCode.Execute(scope);

                Action onLoad;
                if (scope.TryGetVariable<Action>("OnLoad", out onLoad)) {
                    onLoad();
                }

                Func<string> render = (scope as dynamic).Render;
                Console.WriteLine(render());
            }
        }

        public class DynamicPage : DynamicObject {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                if (_values.TryGetValue(binder.Name, out result)) {
                    return true;
                }

                // finds an element of given name:
                switch (binder.Name) {
                    case "element1": result = "e1"; return true;
                    case "element2": result = "e2"; return true;
                }

                result = null;
                return false;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value) {
                _values[binder.Name] = value;
                return true;
            }

            #region ...
            // Python bug workaround
            public string __doc__ { get; set; }
            #endregion
        }

        #endregion

        #region Level 3

        /// <summary>
        /// Shows reflecting over object members for tool support.
        /// This host is IronPython specific.
        /// </summary>
        public static void Scenario_Introspection() {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.ImportModule("datetime");
            object obj = scope.GetVariable("datetime");

            var ops = engine.GetService<DocumentationOperations>();
            foreach (MemberDoc doc in ops.GetMembers(obj)) {
                Console.WriteLine(doc.Name);
                
                object member = engine.Operations.GetMember(obj, doc.Name);
                PrintSignatures(ops.GetOverloads(member));
            }
        }

        private static void PrintSignatures(IEnumerable<OverloadDoc> overloads) {
            foreach (OverloadDoc overloadDoc in overloads) {
                Console.Write("  {0}(", overloadDoc.Name);
                int i = 0;
                foreach (ParameterDoc paramDoc in overloadDoc.Parameters) {
                    if (i > 0) {
                        Console.Write(", ");
                    }
                    Console.Write("{0} : {1}", paramDoc.Name, paramDoc.TypeName);
                    i++;
                }

                Console.Write(")");
                if (overloadDoc.ReturnParameter.TypeName.Length > 0) {
                    Console.Write(" : {0}", overloadDoc.ReturnParameter.TypeName);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Shows reflection for tools using a remote ScriptRuntime.
        /// This host is IronPython specific.
        /// </summary>
        public static void Scenario_RemoteIntrospection(bool isolate) {
            ScriptEngine engine;
            if (isolate) {
                engine = Python.CreateEngine(AppDomain.CreateDomain("remote domain"));
            } else {
                engine = Python.CreateEngine();
            }

            ScriptScope scope = engine.ImportModule("datetime");

            // use handle, the object might not be remotable:
            ObjectHandle obj = scope.GetVariableHandle("datetime");

            var ops = engine.GetService<DocumentationOperations>();
            foreach (MemberDoc doc in ops.GetMembers(obj)) {
                Console.WriteLine(doc.Name);

                ObjectHandle member = engine.Operations.GetMember(obj, doc.Name);
                PrintSignatures(ops.GetOverloads(member));
            }
        }

        public static void Scenario_RemoteEvaluation() {
            ScriptRuntime runtime = ScriptRuntime.CreateRemote(
                AppDomain.CreateDomain("remote domain"),
                ScriptRuntimeSetup.ReadConfiguration()
            );

            ScriptEngine engine = runtime.GetEngine("python");
            ObjectOperations ops = engine.Operations;

            ObjectHandle classC = engine.ExecuteAndWrap(@"
class C(object):
  def __init__(self, value):
    self.value = value
    
  def __int__(self):
    return self.value

C
");

            ObjectHandle result = ops.CreateInstance(classC, 17);
            int intResult = ops.Unwrap<int>(result);
            Console.WriteLine(intResult);
        }

        public static void Scenario_RemoteEvaluationInSandbox() {
            // creates a sandbox:
            Evidence e = new Evidence();
            e.AddHostEvidence(new Zone(SecurityZone.Internet));
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var ps = SecurityManager.GetStandardSandbox(e);
            AppDomain sandbox = AppDomain.CreateDomain("Tests", null, setup, ps);

            // creates a remote runtime:
            var runtime = ScriptRuntime.CreateRemote(sandbox, ScriptRuntimeSetup.ReadConfiguration());
            var engine = runtime.GetEngine("python");

            ObjectHandle exception;
            engine.CreateScriptSourceFromString(@"raise TypeError('this is wrong')").ExecuteAndWrap(out exception);

            if (exception != null) {
                string message, typeName;
                engine.GetService<ExceptionOperations>().GetExceptionMessage(exception, out message, out typeName);
                Console.WriteLine(typeName);
                Console.WriteLine(message);
            }
        }

        #endregion

    }
}
