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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Debugging = Microsoft.Scripting.Debugging;

namespace IronPython.Runtime {
    internal sealed class TraceThread {
        public Stack<TraceBackFrame> Frames;
        public bool InTraceback;
    }

    internal sealed class PythonTracebackListener : Debugging.ITraceCallback {
#if PROFILE_SUPPORT
        private bool _profile;
#endif
        private TracebackDelegate _globalTraceDispatch;
        private object _globalTraceObject;
        private PythonContext _pythonContext;

        private ThreadLocal<TraceThread> _threads;

        internal PythonTracebackListener(PythonContext pythonContext) {
            _pythonContext = pythonContext;
            _threads = new ThreadLocal<TraceThread>();
        }

        internal PythonContext PythonContext {
            get {
                return _pythonContext;
            }
        }

        internal void SetTrace(object function, TracebackDelegate traceDispatch) {
            _globalTraceDispatch = traceDispatch;
            _globalTraceObject = function;
        }

        internal object GetTraceObject() {
            return _globalTraceObject;
        }

#if PROFILE_SUPPORT
        internal void SetProfile(TracebackDelegate traceDispatch) {
            _globalTraceDispatch = traceDispatch;
            _profile = true;
        }
#endif

        internal TraceThread GetCurrentThread() {
            return _threads.Value;
        }

        private TraceThread GetOrCreateThread() {
            TraceThread thread = _threads.Value;

            if (thread == null) {
                thread = new TraceThread();
                _threads.Value = thread;
                thread.Frames = new Stack<TraceBackFrame>();

                // Create the <module> (bottom) frame
                thread.Frames.Push(
                    new TraceBackFrame(_pythonContext.SharedContext, new PythonDictionary(), new PythonDictionary(), new FunctionCode(),
                    new TraceBackFrame(_pythonContext.SharedContext, new PythonDictionary(), new PythonDictionary(), new FunctionCode(), null)));
            }

            return thread;
        }

        #region ITraceCallback Members

        public void OnTraceEvent(Debugging.TraceEventKind kind, string name, string sourceFileName, SourceSpan sourceSpan, Func<Scope> scopeCallback, object payload, object customPayload) {
            if (kind == Debugging.TraceEventKind.ThreadExit ||                  // We don't care about thread-exit events
#if PROFILER_SUPPORT
                (_profile && kind == Debugging.TraceEventKind.TracePoint) ||    // Ignore code execute tracebacks when in profile mode
#endif
                kind == Debugging.TraceEventKind.ExceptionUnwind) {  // and we always have a try/catch so we don't care about methods unwinding.
                return;
            }

            TracebackDelegate traceDispatch = null;
            TraceThread thread = GetOrCreateThread();
            TraceBackFrame pyFrame;

            try {
                if (kind == Debugging.TraceEventKind.FrameEnter) {
                    traceDispatch = _globalTraceDispatch;
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
                        thread.Frames.Count == 0 ? null : thread.Frames.Peek(),
                        properties,
                        scopeCallback
                    );
                    thread.Frames.Push(pyFrame);

                    pyFrame.f_trace = traceDispatch;
                } else {
                    if (thread.Frames.Count == 0) {
                        return;
                    }
                    pyFrame = thread.Frames.Peek();
                    traceDispatch = pyFrame.f_trace;
                }

                // Update the current line
                if (kind != Debugging.TraceEventKind.FrameExit) {
                    pyFrame._lineNo = sourceSpan.Start.Line;
                }

                if (traceDispatch != null) {
                    DispatchTrace(thread, kind, payload, traceDispatch, pyFrame);
                }
            } finally {
                if (kind == Debugging.TraceEventKind.FrameExit && thread.Frames.Count > 0) {
                    thread.Frames.Pop();
                }
            }
        }

        #endregion

        
        private void DispatchTrace(TraceThread thread, Debugging.TraceEventKind kind, object payload, TracebackDelegate traceDispatch, TraceBackFrame pyFrame) {
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
                    args = new PythonTuple(new object[] { pyType, pyException, null });
                    break;
                case Debugging.TraceEventKind.FrameExit:
                    traceEvent = "return";
                    args = payload;
                    break;
            }

            bool traceDispatchThrew = false;
            _pythonContext.TracePipeline.TraceCallback = null;
            thread.InTraceback = true;
            try {
                traceDispatch = traceDispatch(pyFrame, traceEvent, args);
            } catch {
                // We're matching CPython's behavior here.  If the trace dispatch throws any exceptions
                // we don't re-enable tracebacks
                traceDispatchThrew = true;
                _globalTraceObject = _globalTraceDispatch = null;

                throw;
            } finally {
                thread.InTraceback = false;
                if (!traceDispatchThrew) {
                    // renable tracebacks
                    _pythonContext.TracePipeline.TraceCallback = this;

                    pyFrame.f_trace = traceDispatch;
                }
            }
        }
    }
}
