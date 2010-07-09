/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging {
    /// <summary>
    /// Used to wrap a lambda that was already a generator prior to transform.
    /// </summary>
    internal sealed class DebugGenerator<T> : IEnumerator<T>, IDisposable {
        private DebugFrame _frame;

        internal DebugGenerator(DebugFrame frame) {
            _frame = frame;
            _frame.RemapToGenerator(frame.FunctionInfo.Version);
        }

        #region IEnumerator<T> Members

        public T Current {
            get { return (T)((IEnumerator)this).Current; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
            IDisposable innerDisposable = _frame.Generator as IDisposable;
            if (innerDisposable != null)
                innerDisposable.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return ((IEnumerator)_frame.Generator).Current; }
        }

        public bool MoveNext() {
            _frame.Thread.PushExistingFrame(_frame);

            if (_frame.FunctionInfo.SequencePoints[_frame.CurrentLocationCookie].SourceFile.DebugMode == DebugMode.FullyEnabled ||
                _frame.FunctionInfo.SequencePoints[_frame.CurrentLocationCookie].SourceFile.DebugMode == DebugMode.TracePoints && _frame.FunctionInfo.GetTraceLocations()[_frame.CurrentLocationCookie]) {
                try {
                    _frame.DebugContext.DispatchDebugEvent(_frame.Thread, _frame.CurrentLocationCookie, TraceEventKind.FrameEnter, null);
                } catch (ForceToGeneratorLoopException) {
                    // We don't explicitely do anything here because we're about to enter the generator loop
                }
            }

            try {
                bool moveNext;
                _frame.DebugContext.GeneratorLoopProc(_frame, out moveNext);
                return moveNext;
            } finally {
                if (_frame.FunctionInfo.SequencePoints[0].SourceFile.DebugMode == DebugMode.FullyEnabled ||
                    _frame.FunctionInfo.SequencePoints[_frame.CurrentLocationCookie].SourceFile.DebugMode == DebugMode.TracePoints && _frame.FunctionInfo.GetTraceLocations()[_frame.CurrentLocationCookie]) {
                    _frame.DebugContext.DispatchDebugEvent(_frame.Thread, _frame.CurrentLocationCookie, TraceEventKind.FrameExit, Current);
                }

                bool threadExit = _frame.Thread.PopFrame();
                if (threadExit && _frame.DebugContext.DebugMode == DebugMode.FullyEnabled) {
                    // Fire thread-exit event
                    _frame.DebugContext.DispatchDebugEvent(_frame.Thread, Int32.MaxValue, TraceEventKind.ThreadExit, null);
                }
            }
        }

        public void Reset() {
            ((IEnumerator)_frame.Generator).Reset();
        }

        #endregion
    }
}
