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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Threading;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.IronStudio.Library.Repl {
    public abstract class DlrEvaluator : ReplEvaluator, IDlrEvaluator {
        protected ScriptEngine _engine;
        protected ScriptScope _currentScope;
        protected Thread _thread;
        protected Dispatcher _currentDispatcher;
        private int _varCounter;

        // Concrete subclasses constructed via reflection when deserialized from the registry.
        protected DlrEvaluator(string/*!*/ language)
            : base(language) {
        }

        public ScriptEngine Engine {
            get {
                return _engine;
            }
        }

        public override void Start() {
            Action<string> writer = _output.Write;
            _engine = MakeEngine(new OutputStream(writer), new Writer(writer), new Reader(ReadInput));
        }

        public override void Reset() {
            InitScope(Engine.CreateScope());
        }

        protected virtual ScriptEngine MakeEngine(System.IO.Stream stream, System.IO.TextWriter writer, System.IO.TextReader reader) {
            throw new NotImplementedException();
        }

        protected virtual void InitScope(ScriptScope scope) {
            _currentScope = scope ?? _engine.CreateScope();
        }

        protected void InitThread() {
            _thread = new Thread(Execute);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Priority = ThreadPriority.Normal;
            _thread.Name = String.Format("Execution Engine ({0})", Language);
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected void OutputResult(object result, ObjectHandle exception) {
            if (exception != null) {
                WriteException(exception);
            } else if (result != null) {
                ScopeForLastResult.SetVariable("_", result);
                WriteObject(result, _engine.Operations.Format(result));
            }
        }

        public override string FormatException(ObjectHandle exception) {
            return _engine.GetService<ExceptionOperations>().FormatException(exception);
        }

        protected virtual ScriptScope ScopeForLastResult {
            get { return _currentScope; }
        }

        protected CompilerOptions CompilerOptions {
            get { return _engine.GetCompilerOptions(_currentScope); }
        }

        protected virtual SourceCodeKind SourceCodeKind {
            get { return SourceCodeKind.Statements; }
        }

        protected Dispatcher Dispatcher {
            get {
                if (_currentDispatcher == null) {
                    _currentDispatcher = Dispatcher.FromThread(_thread);
                }
                return _currentDispatcher;
            }
        }

        public override bool CanExecuteText(string/*!*/ text) {
            var source = _engine.CreateScriptSourceFromString(text, SourceCodeKind);
            var result = source.GetCodeProperties(CompilerOptions);
            return (result == ScriptCodeParseResult.Empty ||
                result == ScriptCodeParseResult.Complete ||
                result == ScriptCodeParseResult.Invalid);
        }

        private void FinishExecution(ObjectHandle result, ObjectHandle exception, Action<bool, ObjectHandle> completionFunction) {
            _output.Flush();
            if (exception != null) {
                OutputResult(null, exception);
            }

            if (completionFunction != null) {
                completionFunction(exception == null, exception);
            }
        }

        public override bool ExecuteText(string text, Action<bool, ObjectHandle> completionFunction) {
            return ExecuteTextInScopeWorker(text, _currentScope, SourceCodeKind, (r, e) => FinishExecution(r, e, completionFunction));
        }

        private bool ExecuteTextInScopeWorker(string text, ScriptScope scope, SourceCodeKind kind, Action<ObjectHandle, ObjectHandle> completionFunction) {
            var source = _engine.CreateScriptSourceFromString(text, kind);
            var errors = new DlrErrorListener();
            var command = source.Compile(CompilerOptions, errors);
            if (command == null) {
                if (errors.Errors.Count > 0) {
                    WriteException(new ObjectHandle(errors.Errors[0]));
                }
                return false;
            }
            // Allow re-entrant execution.

            Dispatcher.BeginInvoke(new Action(() => {
                ObjectHandle result = null;
                ObjectHandle exception = null;
                try {
                    result = command.ExecuteAndWrap(scope, out exception);
                } catch (ThreadAbortException e) {
                    if (e.ExceptionState != null) {
                        exception = new ObjectHandle(e.ExceptionState);
                    } else {
                        exception = new ObjectHandle(e);
                    }
                    if ((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0) {
                        Thread.ResetAbort();
                    }
                } catch (RemotingException) {
                    WriteLine("Communication with the remote process has been disconnected.");
                } catch (Exception e) {
                    exception = new ObjectHandle(e);
                }
                if (completionFunction != null) {
                    completionFunction(result, exception);
                }
            }));

            return true;
        }

        public virtual ICollection<MemberDoc> GetMemberNames(string expression) {
            var source = _engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);
            var errors = new DlrErrorListener();

            if (ShouldEvaluateForCompletion(expression)) {
                var command = source.Compile(CompilerOptions, errors);
                if (command == null) {
                    return new MemberDoc[0];
                }

                ObjectHandle exception, obj = command.ExecuteAndWrap(_currentScope, out exception);
                if (exception == null) {
                    var docOps = _engine.GetService<DocumentationOperations>();
                    if (docOps != null) {
                        return docOps.GetMembers(obj);
                    }

                    return new List<MemberDoc>(
                        _engine.Operations.GetMemberNames(obj).Select(
                            x => new MemberDoc(x, MemberKind.None)
                        )
                    );
                }
            }

            return new MemberDoc[0];
        }

        protected virtual bool ShouldEvaluateForCompletion(string source) {
            return true;
        }

        public virtual ICollection<OverloadDoc> GetSignatureDocumentation(string expression) {
            var source = _engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);
            if (ShouldEvaluateForCompletion(expression)) {
                var errors = new DlrErrorListener();
                var command = source.Compile(CompilerOptions, errors);
                if (command == null) {
                    return new OverloadDoc[0];
                }

                ObjectHandle exception, obj = command.ExecuteAndWrap(_currentScope, out exception);
                if (exception == null) {
                    var docOps = _engine.GetService<DocumentationOperations>();
                    if (docOps != null) {
                        return docOps.GetOverloads(obj);
                    }

                    return new[] { 
                        new OverloadDoc(
                            "",
                            _engine.Operations.GetDocumentation(obj),
                            new ParameterDoc[0]
                        )
                    };
                }
            }

            return new OverloadDoc[0];
        }

        public override bool CheckSyntax(string text, SourceCodeKind kind) {
            var source = _engine.CreateScriptSourceFromString(text, SourceCodeKind);
            var errors = new DlrErrorListener();
            var command = source.Compile(CompilerOptions, errors);
            if (command == null) {
                if (errors.Errors.Count > 0) {
                    WriteException(new ObjectHandle(errors.Errors[0]));
                }
                return false;
            }
            return true;
        }

        public override void AbortCommand() {
            // TODO: Support for in-proc REPLs
        }

        public new void Dispose() {
            base.Dispose();
            var dispatcher = Dispatcher.FromThread(_thread);
            if (dispatcher != null) {
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }
        }

        private void Execute() {
            while (!Dispatcher.CurrentDispatcher.HasShutdownStarted) {
                try {
                    Dispatcher.Run();
                } catch (Exception exception) {
                    try {
                        OutputResult(null, new ObjectHandle(exception));
                    } catch (Exception) {
                        Restart();
                    }
                }
            }
            _engine.Runtime.Shutdown();
        }

        public virtual void Restart() {
        }

        public override string InsertData(object data, string prefix) {
            if (prefix == null || prefix.Length == 0) {
                prefix = "__data";
            }

            while (true) {
                var varname = prefix + _varCounter.ToString();
                if (!_currentScope.ContainsVariable(varname)) {
                    // TODO: Race condition
                    _currentScope.SetVariable(varname, data);
                    return varname;
                }
                _varCounter++;
            }
        }

        protected virtual string[] FilterNames(IList<string> names, string startsWith) {
            // TODO: LINQify once targeting CLR4?
            var n = new List<string>(names);
            if (startsWith != null && startsWith.Length != 0) {
                n.RemoveAll((s) => !s.StartsWith(startsWith));
            }
            n.Sort((a, b) => {
                int cmp1 = a.ToLowerInvariant().CompareTo(b.ToLowerInvariant());
                if (cmp1 != 0) {
                    return cmp1;
                }
                return a.CompareTo(b);
            });
            return n.ToArray();
        }

        protected override object GetRootObject() {
            return _currentScope;
        }

        protected override string[] GetObjectMemberNames(ObjectHandle obj, string startsWith) {
            try {
                return FilterNames(_engine.Operations.GetMemberNames(obj), startsWith);
            } catch {
                // TODO: Log error?
                return new string[0];
            }
        }

        public virtual bool EnableMultipleScopes {
            get {
                return true;
            }
        }

        class DlrErrorListener : ErrorListener {
            private readonly List<Exception> _errors = new List<Exception>();

            internal List<Exception> Errors {
                get { return _errors; }
            }

            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
                _errors.Add(new SyntaxErrorException(
                    message,
                    source.Path,
                    source.GetCode(),
                    source.GetCodeLine(span.Start.Line),
                    span,
                    errorCode,
                    severity));
            }
        }
    }
}
    