/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using IronRuby;
using Microsoft.IronStudio.RemoteEvaluation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace Microsoft.IronRubyTools.Library.Repl {
    public class RemoteRubyEvaluator : RubyEvaluator {
        private RemoteScriptFactory _factory;
        private string _currentScopeName;

        static RemoteRubyEvaluator() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        // Constructed via reflection when deserialized from the registry.
        public RemoteRubyEvaluator() {
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
            _currentScopeName = "";
            base.Start();
        }

        public override void Restart() {
            WriteLine("Remote process has exited, restarting...");
            _factory = CreateFactory();
            Start();
            
            var changed = AvailableScopesChanged;
            if (changed != null) {
                changed(this, EventArgs.Empty);
            }
        }

        public override void Reset() {
            // TODO: strange text buffer behavior (race condition?)
            // WriteLine("Remote process has been reset...");            
            _factory.Shutdown();

            _factory = CreateFactory();
            Start();

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

        public override bool CanExecuteText(string text) {
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
            } else if (args.Name == typeof(Ruby).Assembly.FullName) {
                return typeof(Ruby).Assembly;
            }
            return null;
        }

        protected override ScriptRuntime/*!*/ CreateRuntime() {
            var setup = new ScriptRuntimeSetup();
            setup.AddRubySetup();
            return _factory.CreateRuntime(setup);
        }

        protected override void RedirectIO(Stream/*!*/ stream, TextWriter/*!*/ writer, TextReader/*!*/ reader) {
            _factory.SetConsoleOut(writer);
            _factory.SetConsoleError(writer);
            _factory.SetConsoleIn(reader);
        }

        public override void AbortCommand() {
            ThreadPool.QueueUserWorkItem(x => _factory.Abort());
        }


        public event EventHandler<EventArgs> AvailableScopesChanged;

        public IEnumerable<string> GetAvailableScopes() {
            return ArrayUtils.EmptyStrings;
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

        public override bool EnableMultipleScopes {
            get {
                return false;
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
