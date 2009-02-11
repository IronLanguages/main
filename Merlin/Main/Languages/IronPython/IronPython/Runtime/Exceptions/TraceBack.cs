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
        private object _globals;
        private object _locals;
        private object _code;

        public TraceBackFrame(object globals, object locals, object code) {
            this._globals = globals;
            this._locals = locals;
            this._code = code;
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
    }
}
