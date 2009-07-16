/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.TraceBackFrame..ctor(System.Object,System.Object,System.Object)", MessageId = "0#globals")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "IronPython.Runtime.Exceptions.TraceBackFrame.Globals", MessageId = "Globals")]

namespace IronPython.Runtime.Exceptions {
    [PythonType("traceback")]
    [Serializable]
    public class TraceBack {
        private readonly TraceBack _next;
        private readonly TraceBackFrame _frame;
        private int _line;

        public TraceBack(TraceBack nextTraceBack, TraceBackFrame fromFrame) {
            _next = nextTraceBack;
            _frame = fromFrame;
        }

        public TraceBack tb_next {
            get {
                return _next;
            }
        }

        public object tb_frame {
            get {
                return _frame;
            }
        }

        public int tb_lineno {
            get {
                return _line;
            }
        }

        public int tb_lasti {
            get {
                return 0;   // not presently tracked
            }
        }

        internal void SetLine(int lineNumber) {
            _line = lineNumber;
        }
    }

    [PythonType("frame")]
    [Serializable]
    public class TraceBackFrame {
        private readonly PythonTracebackListener _traceAdapter;
        private TracebackDelegate _trace;
        internal int _lineNo;
        private readonly PythonDebuggingPayload _debugProperties;
        private readonly Func<Scope> _scopeCallback;

        private readonly object _globals;
        private readonly object _locals;
        private readonly object _code;
        private readonly CodeContext/*!*/ _context;
        private readonly TraceBackFrame _back;

        internal TraceBackFrame(CodeContext/*!*/ context, object globals, object locals, object code) {
            _globals = globals;
            _locals = locals;
            _code = code;
            _context = context;
        }

        internal TraceBackFrame(CodeContext/*!*/ context, object globals, object locals, object code, TraceBackFrame back) {
            _globals = globals;
            _locals = locals;
            _code = code;
            _context = context;
            _back = back;
        }

        internal TraceBackFrame(PythonTracebackListener traceAdapter, object code, TraceBackFrame back, PythonDebuggingPayload debugProperties, Func<Scope> scopeCallback) {
            this._traceAdapter = traceAdapter;
            this._code = code;
            this._back = back;
            this._debugProperties = debugProperties;
            this._scopeCallback = scopeCallback;
        }
        
        public TracebackDelegate f_trace {
            get {
                if (_traceAdapter != null) {
                    return _trace;
                } else {
                    return null;
                }
            }
            set {
                if (_traceAdapter != null) {
                    _trace = value;
                }
            }
        }

        [SpecialName]
        public void Deletef_trace() {
            f_trace = null;
        }

        public object f_globals {
            get {
                if (_traceAdapter != null && _scopeCallback != null) {
                    return new PythonDictionary(new GlobalScopeDictionaryStorage(_scopeCallback()));
                } else {
                    return _globals;
                }
            }
        }

        public object f_locals {
            get {
                if (_traceAdapter != null && _scopeCallback != null) {
                    return new PythonDictionary(new GlobalScopeDictionaryStorage(_scopeCallback()));
                } else {
                    return _locals;
                }
            }
        }

        public object f_code {
            get {
                return _code;
            }
        }

        public object f_builtins {
            get {
                return PythonContext.GetContext(_context).BuiltinModuleInstance.Dict;
            }
        }

        public TraceBackFrame f_back {
            get {
                return _back;
            }
        }

        public object f_exc_traceback {
            get {
                return null;
            }
        }

        public object f_exc_type {
            get {
                return null;
            }
        }

        public bool f_restricted {
            get {
                return false;
            }
        }

        public object f_lineno {
            get {
                if (_traceAdapter != null) {
                    return _lineNo;
                } else {
                    return 1;
                }
            }
            set {
                if (!(value is int)) {
                    throw PythonOps.ValueError("lineno must be an integer");
                }

                int newLineNum = (int)value;

                if (_traceAdapter != null) {
                    SetLineNumber(newLineNum);
                } else {
                    throw PythonOps.ValueError("f_lineno can only be set by a trace function");
                }
            }
        }

        private void SetLineNumber(int newLineNum) {
            var pyThread = _traceAdapter.GetCurrentThread();
            if (pyThread == null || !Type.ReferenceEquals(this, pyThread.Frames.Peek())) {
                throw PythonOps.ValueError("f_lineno can only be set by a trace function");
            }

            FunctionCode funcCode = _debugProperties.Code;
            Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations = _debugProperties.LoopAndFinallyLocations;
            Dictionary<int, bool> handlerLocations = _debugProperties.HandlerLocations;

            Dictionary<int, bool> currentLoopIds = null;
            bool inForLoopOrFinally = loopAndFinallyLocations != null && loopAndFinallyLocations.TryGetValue(_lineNo, out currentLoopIds);
            
            int originalNewLine = newLineNum;

            if (newLineNum < funcCode.Span.Start.Line) {
                throw PythonOps.ValueError("line {0} comes before the current code block", newLineNum);
            } else if (newLineNum > funcCode.Span.End.Line) {
                throw PythonOps.ValueError("line {0} comes after the current code block", newLineNum);
            }


            while (newLineNum <= funcCode.Span.End.Line) {
                var span = new SourceSpan(new SourceLocation(0, newLineNum, 1), new SourceLocation(0, newLineNum, Int32.MaxValue));

                // Check if we're jumping onto a handler
                bool handlerIsFinally;
                if (handlerLocations != null && handlerLocations.TryGetValue(newLineNum, out handlerIsFinally)) {
                    throw PythonOps.ValueError("can't jump to 'except' line");                    
                }

                // Check if we're jumping into a for-loop
                Dictionary<int, bool> jumpIntoLoopIds;
                if (loopAndFinallyLocations != null && loopAndFinallyLocations.TryGetValue(newLineNum, out jumpIntoLoopIds)) {
                    // If we're not in any loop already - then we can't jump into a loop
                    if (!inForLoopOrFinally) {
                        throw BadForOrFinallyJump(newLineNum, jumpIntoLoopIds);
                    }

                    // If we're in loops - we can only jump if we're not entering a new loop
                    foreach (int jumpIntoLoopId in jumpIntoLoopIds.Keys) {
                        if (!currentLoopIds.ContainsKey(jumpIntoLoopId)) {
                            throw BadForOrFinallyJump(newLineNum, currentLoopIds);
                        }
                    }
                } else if (currentLoopIds != null) {
                    foreach (bool isFinally in currentLoopIds.Values) {
                        if (isFinally) {
                            throw PythonOps.ValueError("can't jump out of 'finally block'");
                        }
                    }
                }

                if (_traceAdapter.PythonContext.TracePipeline.CanSetNextStatement((string)((FunctionCode)_code).co_filename, span)) {
                    _traceAdapter.PythonContext.TracePipeline.SetNextStatement((string)((FunctionCode)_code).co_filename, span);
                    _lineNo = newLineNum;
                    return;
                }

                ++newLineNum;
            }

            throw PythonOps.ValueError("line {0} is invalid jump location ({1} - {2} are valid)", originalNewLine, funcCode.Span.Start.Line, funcCode.Span.End.Line);
        }

        private static Exception BadForOrFinallyJump(int newLineNum, Dictionary<int, bool> jumpIntoLoopIds) {
            foreach (bool isFinally in jumpIntoLoopIds.Values) {
                if (isFinally) {
                    return PythonOps.ValueError("can't jump into 'finally block'", newLineNum);
                }
            }
            return PythonOps.ValueError("can't jump into 'for loop'", newLineNum);
        }
    }

    public delegate TracebackDelegate TracebackDelegate(TraceBackFrame frame, string result, object payload);
}
