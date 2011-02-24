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
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using IronPython.Hosting;
using Microsoft.IronStudio.RemoteEvaluation;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting.Hosting;

namespace Microsoft.IronPythonTools.Library.Repl {

    public class RemotePythonEvaluator : PythonEvaluator, IMultipleScopeEvaluator {
        private RemoteScriptFactory _factory;
        private string _currentScopeName;

        static RemotePythonEvaluator() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        // Constructed via reflection when deserialized from the registry.
        public RemotePythonEvaluator() {
            _factory = CreateFactory();

        }

        public static RemoteScriptFactory CreateFactory() {
            return new RemoteScriptFactory(ApartmentState.STA);
        }

        public RemoteScriptFactory RemoteScriptFactory {
            get {
                return _factory;
            }
        }

        public override void PublishScopeVariables(ScriptScope scope) {
        }

        public override void Start() {
            _currentScopeName = "__main__";
            base.Start();
        }

        public override void Restart() {
            WriteLine("Remote process has exited, restarting...");
            _factory = CreateFactory();
            Start();
            _factory.CommandDispatcher = _engine.GetService<PythonService>(_engine).GetLocalCommandDispatcher();
            
            var changed = AvailableScopesChanged;
            if (changed != null) {
                changed(this, EventArgs.Empty);
            }
        }

        public override void Reset() {
            WriteLine("Remote process has been reset...");            
            _factory.Shutdown();

            _factory = CreateFactory();
            Start();
            _factory.CommandDispatcher = _engine.GetService<PythonService>(_engine).GetLocalCommandDispatcher();

            var changed = AvailableScopesChanged;
            if (changed != null) {
                changed(this, EventArgs.Empty);
            }
        }

        public override bool ExecuteText(string text, Action<bool, ObjectHandle> completionFunction) {
            if (_factory.IsDisconnected) {
                Restart();
            }

            return base.ExecuteText(text, completionFunction);
        }

        public override string FormatException(ObjectHandle exception) {
            if (_factory.IsDisconnected) {
                Restart();
            }

            return base.FormatException(exception);
        }

        public override bool CanExecuteText(string/*!*/ text) {
            if (_factory.IsDisconnected) {
                Restart();
            }

            return base.CanExecuteText(text);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            // VS Loads us into the LoadFrom context, remoting needs to get the same assemblies
            // but uses Assembly.Load.  We return the correct assemblies here.
            if (args.Name == typeof(ScriptRuntime).Assembly.FullName) {
                return typeof(ScriptRuntime).Assembly;
            } else if (args.Name == typeof(Python).Assembly.FullName) {
                return typeof(Python).Assembly;
            }
            return null;
        }

        public virtual Dictionary<string, object> GetOptions() {
            return new Dictionary<string, object>();
        }

        protected override ScriptEngine MakeEngine(Stream stream, TextWriter writer, TextReader reader) {
            _factory.SetConsoleOut(writer);
            _factory.SetConsoleError(writer);
            _factory.SetConsoleIn(reader);
            
            var runtime = (ScriptRuntime)_factory.CreateRuntime(Python.CreateRuntimeSetup(GetOptions()));
            var res = runtime.GetEngine("Python");
            InitializeEngine(stream, writer, res);
            return res;
        }

        public override void AbortCommand() {
            ThreadPool.QueueUserWorkItem(x => _factory.Abort());
        }


        public event EventHandler<EventArgs> AvailableScopesChanged;
        public event EventHandler<EventArgs> CurrentScopeChanged;

        public IEnumerable<string> GetAvailableScopes() {
            return _engine.GetModuleFilenames();
        }

        public override ICollection<OverloadDoc> GetSignatureDocumentation(string expression) {
            if (_factory.IsDisconnected) {
                Restart();
            }

            return base.GetSignatureDocumentation(expression);
        }

        public override ICollection<MemberDoc> GetMemberNames(string expression) {
            if (_factory.IsDisconnected) {
                Restart();
            }

            return base.GetMemberNames(expression);
        }

        public void SetScope(string scopeName) {
            try {
                _currentScope = _engine.ImportModule(scopeName);
                _currentScopeName = scopeName;
                WriteLine(String.Format("Current scope changed to {0}", scopeName));

                var curScopeChanged = CurrentScopeChanged;
                if (curScopeChanged != null) {
                    curScopeChanged(this, EventArgs.Empty);
                }
            } catch {
                WriteLine(String.Format("Unknown module: {0}", scopeName));
            }
        }

        public string SetScope(ScriptScope scope) {
            _currentScope = scope;
            string scopeName;

            if (scope.TryGetVariable<string>("__name__", out scopeName)) {
                _currentScopeName = scopeName;
            } else {
                _currentScopeName = String.Empty;
            }
                
            var curScopeChanged = CurrentScopeChanged;
            if (curScopeChanged != null) {
                curScopeChanged(this, EventArgs.Empty);
            }

            return _currentScopeName;
        }

        public override bool EnableMultipleScopes {
            get {
                return true;
            }
        }

        public string CurrentScopeName {
            get {
                return _currentScopeName;
            }
        }

        public ScriptScope CurrentScope {
            get {
                return _currentScope;
            }
        }
    }
}
