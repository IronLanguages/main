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

        public object f_globals {
            get {
                return _globals;
            }
        }

        public object f_locals {
            get {
                return _locals;
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

        public int f_lineno {
            get {
                // we don't track line numbers yet, this matches the line number CPython's warning
                // module uses when getframe isn't available.
                return 1;
            }
        }

        public object f_trace {
            get {
                return null;
            }
        }

        public bool f_restricted {
            get {
                return false;
            }
        }
    }
}
