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
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;

using Debugging = Microsoft.Scripting.Debugging;

namespace IronPython.Runtime {
    internal sealed class TraceThread {
        public List<TraceBackFrame> Frames;
        public bool InTraceback;
    }

    internal sealed class PythonTracebackListener : Debugging.ITraceCallback {
        private readonly PythonContext _pythonContext;
        [ThreadStatic] private static TraceThread _threads;
        [ThreadStatic] private static TracebackDelegate _globalTraceDispatch;
        [ThreadStatic] private static object _globalTraceObject;
        private bool _exceptionThrown;
        
#if PROFILE_SUPPORT
        private bool _profile;
#endif

        internal PythonTracebackListener(PythonContext pythonContext) {
            _pythonContext = pythonContext;
        }

        internal PythonContext PythonContext {
            get {
                return _pythonContext;
            }
        }

        internal bool ExceptionThrown {
            get {
                return _exceptionThrown;
            }
        }

        internal static void SetTrace(object function, TracebackDelegate traceDispatch) {
            _globalTraceDispatch = traceDispatch;
            _globalTraceObject = function;
        }

        internal static object GetTraceObject() {
            return _globalTraceObject;
        }

#if PROFILE_SUPPORT
        internal void SetProfile(TracebackDelegate traceDispatch) {
            _globalTraceDispatch = traceDispatch;
            _profile = true;
        }
#endif

        internal static TraceThread GetCurrentThread() {
            return _threads;
        }

        private static TraceThread GetOrCreateThread() {
            TraceThread thread = _threads;

            if (thread == null) {
                _threads = thread = new TraceThread();
                thread.Frames = new List<TraceBackFrame>();
            }

            return thread;
        }

        #region ITraceCallback Members

        public void OnTraceEvent(Debugging.TraceEventKind kind, string name, string sourceFileName, SourceSpan sourceSpan, Func<IAttributesCollection> scopeCallback, object payload, object customPayload) {        
            if (kind == Debugging.TraceEventKind.ThreadExit ||                  // We don't care about thread-exit events
#if PROFILER_SUPPORT
            (_profile && kind == Debugging.TraceEventKind.TracePoint) ||    // Ignore code execute tracebacks when in profile mode
#endif
                kind == Debugging.TraceEventKind.ExceptionUnwind) {  // and we always have a try/catch so we don't care about methods unwinding.
                return;
            }

            TracebackDelegate traceDispatch = null;
            object traceDispatchObject = null;
            TraceThread thread = GetOrCreateThread();
            TraceBackFrame pyFrame;

            if (thread.InTraceback) {
                return;
            }

            try {
                if (kind == Debugging.TraceEventKind.FrameEnter) {
                    traceDispatch = _globalTraceDispatch;
                    traceDispatchObject = _globalTraceObject;
                    /*
                    if (thread.Frames.Count == 1 && traceDispatch != null) {
                        // Dispatch "line" trace for <module> frame
                        DispatchTrace(thread, Debugging.TraceEventKind.FrameEnter, null, _globalTraceDispatch, thread.Frames.Peek());
                    }*/

                    var properties = (PythonDebuggingPayload)customPayload;

                    // push the new frame
                    pyFrame = new TraceBackFrame(
                        this,
                        properties.Code,
                        thread.Frames.Count == 0 ? null : thread.Frames[thread.Frames.Count - 1],
                        properties,
                        scopeCallback
                    );

                    thread.Frames.Add(pyFrame);

                    pyFrame.Setf_trace(traceDispatchObject);
                } else {
                    if (thread.Frames.Count == 0) {
                        return;
                    }
                    pyFrame = thread.Frames[thread.Frames.Count - 1];
                    traceDispatch = pyFrame.TraceDelegate;
                    traceDispatchObject = pyFrame.Getf_trace();
                }

                // Update the current line
                if (kind != Debugging.TraceEventKind.FrameExit) {
                    pyFrame._lineNo = sourceSpan.Start.Line;
                }

                if (traceDispatchObject != null && !_exceptionThrown) {
                    DispatchTrace(thread, kind, payload, traceDispatch, traceDispatchObject, pyFrame);
                }
            } finally {
                if (kind == Debugging.TraceEventKind.FrameExit && thread.Frames.Count > 0) {
                    thread.Frames.RemoveAt(thread.Frames.Count - 1);
                }
            }            
        }

        #endregion

        
        private void DispatchTrace(TraceThread thread, Debugging.TraceEventKind kind, object payload, TracebackDelegate traceDispatch, object traceDispatchObject, TraceBackFrame pyFrame) {
            object args = null;

            // Prepare the event
            string traceEvent = String.Empty;
            switch (kind) {
                case Debugging.TraceEventKind.FrameEnter: traceEvent = "call"; break;
                case Debugging.TraceEventKind.TracePoint: traceEvent = "line"; break;
                case Debugging.TraceEventKind.Exception:
                    traceEvent = "exception";
                    object pyException = PythonExceptions.ToPython((Exception)payload);
                    object pyType = ((IPythonObject)pyException).PythonType;
                    args = PythonTuple.MakeTuple(pyType, pyException, null);
                    break;
                case Debugging.TraceEventKind.FrameExit:
                    traceEvent = "return";
                    args = payload;
                    break;
            }

            bool traceDispatchThrew = true;
            thread.InTraceback = true;
            try {
                TracebackDelegate dlg = traceDispatch(pyFrame, traceEvent, args);
                traceDispatchThrew = false;
                pyFrame.Setf_trace(dlg);
            } finally {
                thread.InTraceback = false;
                if (traceDispatchThrew) {
                    // We're matching CPython's behavior here.  If the trace dispatch throws any exceptions
                    // we don't re-enable tracebacks.  We need to leave the trace callback in place though
                    // so that we can pop our frames.
                    _globalTraceObject = _globalTraceDispatch = null;
                    _exceptionThrown = true;
                }
            }
        }
    }
}
