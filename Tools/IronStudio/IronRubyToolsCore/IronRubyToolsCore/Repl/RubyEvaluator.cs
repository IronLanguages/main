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
using System.Runtime.InteropServices;
using Microsoft.IronStudio.Library.Repl;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using IronRuby;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools.Library.Repl {
    public class RubyEvaluator : DlrEvaluator {
        // Constructed via reflection when deserialized from the registry.
        public RubyEvaluator()
            : base("Ruby") {
        }

        public override string/*!*/ Prompt {
            get { return "»"; }
        }

        public override string/*!*/ CommandPrefix {
            get { return "."; }
        }

        public override bool DisplayPromptInMargin {
            get { return true; }
        }

        public override void Start() {
            base.Start();
            
            InitScope(MakeScope());
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

        protected virtual ScriptRuntime/*!*/ CreateRuntime() {
            ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
            setup.AddRubySetup();
            setup.DebugMode = true;
            return Ruby.CreateRuntime(setup);
        }

        protected virtual void RedirectIO(Stream/*!*/ stream, TextWriter/*!*/ writer, TextReader/*!*/ reader) {
            // nop
        }

        protected sealed override ScriptEngine/*!*/ MakeEngine(Stream/*!*/ stream, TextWriter/*!*/ writer, TextReader/*!*/ reader) {
            var runtime = CreateRuntime();

            runtime.IO.SetOutput(stream, writer);
            RedirectIO(stream, writer, reader);

            runtime.LoadAssembly(typeof(string).Assembly); // mscorlib.dll
            runtime.LoadAssembly(typeof(System.Uri).Assembly); // System.dll

            return runtime.GetRubyEngine();
        }

        protected virtual ScriptScope MakeScope() {
            return _engine.CreateScope();
        }

        protected override SourceCodeKind SourceCodeKind {
            get {
                return SourceCodeKind.InteractiveCode;
            }
        }

        protected override ScriptScope ScopeForLastResult {
            get {
                throw new NotImplementedException("TODO");
            }
        }

        public override bool EnableMultipleScopes {
            get {
                return false;
            }
        }
    }
}
