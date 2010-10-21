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
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.IronStudio.Library.Repl;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Runtime;

namespace Microsoft.IronPythonTools.Library.Repl {
    public class PythonEvaluator : DlrEvaluator {
        private static ScriptEngine _localEngine = Python.CreateEngine();       // local engine for checking syntax

        // Constructed via reflection when deserialized from the registry.
        public PythonEvaluator()
            : base("python") {
        }

        public override void Start() {
            base.Start();
            
            InitScope(MakeScope("__main__"));
            InitThread();
        }

        public virtual void PublishScopeVariables(ScriptScope scope) {
        }

        protected override void InitScope(ScriptScope scope) {
            base.InitScope(scope);
            PublishScopeVariables(scope);
        }

        public override void Reset() {
        }

        protected override ScriptEngine MakeEngine(Stream stream, TextWriter writer, TextReader reader) {
#if true
            // Debuggable mode. 
            Dictionary<string, object> opts = new Dictionary<string, object>();

            opts["Debug"] = true; // enable to emit PDBs for py code 
            opts["Frames"] = opts["FullFrames"] = true; // Set these to gen $localContext.Scope            

            var result = Python.CreateEngine(opts);
#else
            // Non-debuggable mode
            var result = Python.CreateEngine();
#endif
            InitializeEngine(stream, writer, result);
            return result;
        }

        protected static void InitializeEngine(Stream stream, TextWriter writer, ScriptEngine result) {
            result.Runtime.IO.SetOutput(stream, writer);
            result.Runtime.LoadAssembly(typeof(string).Assembly); // mscorlib.dll
            result.Runtime.LoadAssembly(typeof(System.Uri).Assembly); // System.dll
        }

        public override bool CanExecuteText(string/*!*/ text) {
            // Multi-line: when you leave two blank lines in a row, your thought must be done
            // TODO: This feels unsatisfactory, even if it largely matches Python behavior under cmd.exe
            int newLines = 0;
            for (int i = text.Length - 1; i >= 0; i--) {
                if (text[i] == '\n') {
                    if (++newLines == 2) {
                        return true;
                    }
                } else if (Char.IsWhiteSpace(text[i])) {
                    continue;
                } else {
                    break;
                }
            }

            // If this a partially-formed thought (for instance, open brace or block), don't execute
            if (!base.CanExecuteText(text)) {
                return false;
            }

            // Single-line: if it's executable, then execute
            if (text.IndexOf('\n') == text.LastIndexOf('\n')) {
                return true;
            }

            return false;
        }

        protected override bool ShouldEvaluateForCompletion(string source) {
            var scriptSrc = _localEngine.CreateScriptSource(new StringTextContentProvider(source), "", SourceCodeKind.Expression);
            var context = new CompilerContext(HostingHelpers.GetSourceUnit(scriptSrc), HostingHelpers.GetLanguageContext(_localEngine).GetCompilerOptions(), ErrorSink.Null);
            var parser = Parser.CreateParser(context, new PythonOptions());
            
            var stmt = parser.ParseSingleStatement();
            var exprWalker = new ExprWalker();
            stmt.Walk(exprWalker);
            return exprWalker.ShouldExecute;
        }

        class ExprWalker : PythonWalker {
            public bool ShouldExecute = true;

            public override bool Walk(CallExpression node) {
                ShouldExecute = false;
                return base.Walk(node);
            }
        }

        protected virtual ScriptScope MakeScope(string name) {
            return _engine.CreateModule(name);
        }

        private Scope GetModule(string name) {
            var sysmodule = _engine.GetSysModule();
            var modules = sysmodule.GetVariable<IDictionary<object, object>>("modules");
            object result = null;
            modules.TryGetValue(name, out result);
            return result as Scope;
        }

        //[ReplCommand("reload")
        public void ReloadModule(string name) {
            if (GetModule(name) == null) {
                throw new Exception(String.Format("Module '{0}' not found", name));
            }

#if CAN_USE_PYTHON
        from project_analyzer import GetAnalysisEngine
        engine = GetAnalysisEngine()
        if engine is None:
            raise RuntimeError("Analysis engine is not enabled")
        
        if self._project is None:
            raise RuntimeError("Not attached to a project")
       
        analysis = engine.GetProjectAnalysisState(self._project)
        if analysis is None:
            msg = "Unable to get analysis for project %r" % (self._project,)
            raise RuntimeError(msg)
        
        import dependency_import
        d = dependency_import.get_dependencies(analysis, name)
        m = []
        
        def test_loaded(n):
            if self._GetModule(n) is None:
                return False
            m.append(n)
            return True
        
        d.WalkTree(test_loaded)
        
        // TODO: Run on engine thread
        b = IronPython.Hosting.Python.GetBuiltinModule(self._engine)
        for m in [self._GetModule(n) for n in m]:
            self.WriteLine('Reloading ' + m.__name__)
            self._engine.Operations.InvokeMember(b, 'reload', m)
#endif
        }

        protected override SourceCodeKind SourceCodeKind {
            get {
                return SourceCodeKind.InteractiveCode;
            }
        }

        protected override ScriptScope ScopeForLastResult {
            get {
                return _engine.GetBuiltinModule();
            }
        }

        protected override string[] FilterNames(IList<string> names, string startsWith) {
            string[] n = base.FilterNames(names, startsWith);
            for (int i = 0; i < n.Length; i++) {
                if (n[i].StartsWith("__") && n[i].EndsWith("__")) {
                    continue;
                }

                string[] n2 = new string[n.Length];
                int pivot = n.Length - i;
                for (int j = 0; j < pivot; j++) {
                    n2[j] = n[i + j];
                }
                for (int j = 0; j < i; j++) {
                    n2[pivot + j] = n[j];
                }
                n = n2;
                break;
            }
            return n;
        }
#if FALSE
        public override Member[] GetModules() {
            var sysmodule = _engine.GetSysModule();
            var path = sysmodule.GetVariable<List>("path");
            var mods = new List<Member>();
            foreach (string dir in path) {
                if (!Directory.Exists(dir)) {
                    continue;
                }

                foreach (string filename in Directory.GetFiles(dir)) {
                    mods.Add(new Member { Name = Path.GetFileNameWithoutExtension(filename) });
                }
                foreach (string dirname in Directory.GetDirectories(dir)) {
                    if (File.Exists(Path.Combine(dirname, "__init__.py"))) {
                        mods.Add(new Member { Name = Path.GetFileName(dir) });
                    }
                }
            }

            return mods.ToArray();
        }
#endif
        private static bool IsObjectNew(object obj) {
            BuiltinFunction newFn = (obj as BuiltinFunction);
            if (newFn == null) {
                return false;
            }
            return (newFn.DeclaringType == typeof(object) && newFn.__name__ == "__new__");
        }
#if FALSE
        public override ReplOverloadResult[] GetSignatures(string text) {
            var obj = GetRootObject();
            if (obj == null) {
                return new ReplOverloadResult[0];
            }

            var t = text.Remove(text.Length - 1);
            foreach (var symbol in t.Split('.')) {
                obj = GetObjectMember(obj, symbol);
                if (obj == null) {
                    return new ReplOverloadResult[0];
                }
            }

            var sigs = new List<ReplOverloadResult>();

            if (obj is BuiltinFunction) {
                // TODO: Deal with "clr" visibility
                BuiltinFunction bf = (obj as BuiltinFunction);
                foreach (object overload in bf.Overloads.Functions) {
                    sigs.Add(new BuiltinFunctionOverloadResult(this, overload));
                }
            } else if (obj is PythonFunction || obj is Method) {
                sigs.Add(new FunctionOverloadResult(this, obj));
            } else if (obj is PythonType || obj is OldClass) {
                var name = GetObjectMember(obj, "__name__") as string;
                var docstring = GetObjectMember(obj, "__doc__") as string;
                var newFunc = GetObjectMember(obj, "__new__");
                if (!IsObjectNew(newFunc)) {
                    if (newFunc is PythonFunction || newFunc is Method) {
                        sigs.Add(new FunctionOverloadResult(this, newFunc, name, docstring, true));
                    } else {
                        // TODO: Deal with "clr" visibility
                        BuiltinFunction bf = (obj as BuiltinFunction);
                        foreach (object overload in bf.Overloads.Functions) {
                            sigs.Add(new BuiltinFunctionOverloadResult(this, overload, name));
                        }
                    }
                } else {
                    var init = GetObjectMember(obj, "__init__");
                    var constructor = GetObjectMember(init, "im_func");
                    sigs.Add(new FunctionOverloadResult(this, constructor, name, docstring, true));
                }
            }
            return sigs.ToArray();
        }
#endif

        public override bool EnableMultipleScopes {
            get {
                return false;
            }
        }
    }
}
