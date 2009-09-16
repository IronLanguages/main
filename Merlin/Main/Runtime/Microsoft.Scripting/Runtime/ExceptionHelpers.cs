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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// These are some generally useful helper methods for handling exceptions.
    /// </summary>
    public static class ExceptionHelpers {
        private const string prevStackTraces = "PreviousStackTraces";

        /// <summary>
        /// Keeps track of exceptions being handled in interpreted mode (so we can support rethrow statements).
        /// </summary>
        [ThreadStatic]
        internal static List<Exception> _caughtExceptions;

        [ThreadStatic]
        private static List<Exception> _currentExceptions;

        [ThreadStatic]
        private static List<DynamicStackFrame> _stackFrames;

        /// <summary>
        /// Gets the list of exceptions that are currently being handled by the user. 
        /// 
        /// These represent active catch blocks on the stack.
        /// </summary>
        [Obsolete("will be removed soon")]
        public static List<Exception> CurrentExceptions {
            get {
                return _currentExceptions;
            }
        }

        public static Exception LastException {
            get {
                if (_caughtExceptions != null && _caughtExceptions.Count > 0) {
                    return _caughtExceptions[_caughtExceptions.Count - 1];
                } else {
                    throw Error.NoException();
                }
            }
        }

        /// <summary>
        /// Updates an exception before it's getting re-thrown so
        /// we can present a reasonable stack trace to the user.
        /// </summary>
        public static Exception UpdateForRethrow(Exception rethrow) {
#if !SILVERLIGHT
            List<StackTrace> prev;

            if (rethrow.Data.Contains(typeof(DynamicStackFrame))) {
                // we've saved the stack trace data in the exception, just continue
                // appending to it.n2
                _stackFrames = (List<DynamicStackFrame>)rethrow.Data[typeof(DynamicStackFrame)];
                rethrow.Data.Remove(typeof(DynamicStackFrame));
            }
            // we don't have any dynamic stack trace data, capture the data we can
            // from the raw exception object.
            StackTrace st = new StackTrace(rethrow, true);

            if (!TryGetAssociatedStackTraces(rethrow, out prev)) {
                prev = new List<StackTrace>();
                AssociateStackTraces(rethrow, prev);
            }

            prev.Add(st);
            
#endif
            return rethrow;
        }

        public static void ClearDynamicStackFrames(Exception e) {
            e.Data.Remove(typeof(DynamicStackFrame));
        }

        private static void AssociateStackTraces(Exception e, List<StackTrace> traces) {
            e.Data[prevStackTraces] = traces;
        }

        private static bool TryGetAssociatedStackTraces(Exception e, out List<StackTrace> traces) {
            traces = e.Data[prevStackTraces] as List<StackTrace>;
            return traces != null;
        }

        /// <summary>
        /// Returns all the stack traces associates with an exception
        /// </summary>
        public static IList<StackTrace> GetExceptionStackTraces(Exception rethrow) {
            List<StackTrace> result;
            return TryGetAssociatedStackTraces(rethrow, out result) ? result : null;
        }

        
        public static List<DynamicStackFrame> AssociateDynamicStackFrames(Exception clrException) {
            if (_stackFrames != null) {
                clrException.Data[typeof(DynamicStackFrame)] = _stackFrames;
            }
            return _stackFrames;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public static List<DynamicStackFrame> DynamicStackFrames {
            get {
                return _stackFrames;
            }
            set {
                _stackFrames = value;
            }
        }
        
        [Obsolete("will be removed soon")]
        public static void PushExceptionHandler(Exception clrException) {
            // _currentExceptions is thread static
            if (_currentExceptions == null) {
                _currentExceptions = new List<Exception>();
            }
            _currentExceptions.Add(clrException);

            AssociateDynamicStackFrames(clrException);
        }

        [Obsolete("will be removed soon")]
        public static void PopExceptionHandler() {
            // _currentExceptions is thread static
            Debug.Assert(_currentExceptions != null);
            Debug.Assert(_currentExceptions.Count != 0);

#if !SILVERLIGHT
            ThreadAbortException tae = _currentExceptions[_currentExceptions.Count - 1] as ThreadAbortException;
            if (tae != null && tae.ExceptionState is KeyboardInterruptException) {
                Thread.ResetAbort();
            }
#endif
            _currentExceptions.RemoveAt(_currentExceptions.Count - 1);
        }


        #region stack frame filtering

        /// <summary>
        /// Walks all stack frames, filtering out DLR frames
        /// Does not walk the frames in the InnerException, if any
        /// Frames are returned in CLR order (inner to outer)
        /// </summary>
        public static IEnumerable<DynamicStackFrame> GetStackFrames(Exception e) {
            return GetStackFrames(e, false);
        }

        /// <summary>
        /// Walks all stack frames, filtering out DLR frames
        /// Does not walk the frames in the InnerException, if any
        /// Frames are returned in CLR order (inner to outer), unless reverse is set
        /// </summary>
        public static IEnumerable<DynamicStackFrame> GetStackFrames(Exception e, bool reverseOrder) {
            IList<StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);
            if (traces == null) {
                traces = new[] { GetStackTrace(e) };
            } else {
                traces.Add(GetStackTrace(e));
            }

            List<DynamicStackFrame> dynamicFrames = new List<DynamicStackFrame>(ScriptingRuntimeHelpers.GetDynamicStackFrames(e));
            // dynamicFrames is stored in the opposite order that we are walking,
            // so we can always pop them from the back of the List<T>, which is O(1)
            if (!reverseOrder) {
                dynamicFrames.Reverse();
            }

            foreach (StackTrace trace in WalkList(traces, reverseOrder)) {
                foreach (DynamicStackFrame result in GetStackFrames(trace, dynamicFrames, reverseOrder)) {
                    yield return result;
                }
            }

            //TODO: we would like to be able to assert this;
            // right now, we cannot, because we are not using dynamic frames for non-interpreted dynamic methods.
            // (we create the frames, but we do not consume them in FormatStackTrace.)
            //Debug.Assert(dynamicFrames.Count == 0);
        }

        private static StackTrace GetStackTrace(Exception e) {
#if SILVERLIGHT
            return new StackTrace(e);
#else
            return new StackTrace(e, true);
#endif
        }

        private static IEnumerable<T> WalkList<T>(IList<T> list, bool reverseOrder) {
            if (reverseOrder) {
                for (int i = list.Count - 1; i >= 0; i--) {
                    yield return list[i];
                }
            } else {
                for (int i = 0; i < list.Count; i++) {
                    yield return list[i];
                }
            }
        }

        private static IEnumerable<DynamicStackFrame> GetStackFrames(StackTrace trace, List<DynamicStackFrame> dynamicFrames, bool reverseOrder) {
            StackFrame[] frames = trace.GetFrames();
            if (frames == null) {
                yield break;
            }

            foreach (StackFrame frame in WalkList(frames, reverseOrder)) {
                MethodBase method = frame.GetMethod();
                Type parentType = method.DeclaringType;

                if (dynamicFrames.Count > 0 && frame.GetMethod() == dynamicFrames[dynamicFrames.Count - 1].GetMethod()) {
                    yield return dynamicFrames[dynamicFrames.Count - 1];
                    dynamicFrames.RemoveAt(dynamicFrames.Count - 1);
                    continue;
                } 

                if (parentType != null) {
                    if (parentType == typeof(LambdaExpression) && method.Name == "DoExecute") {
                        // Evaluated frame -- Replace with dynamic frame
                        Debug.Assert(dynamicFrames.Count > 0);
                        //if (dynamicFrames.Count == 0) continue;
                        yield return dynamicFrames[dynamicFrames.Count - 1];

                        dynamicFrames.RemoveAt(dynamicFrames.Count - 1);
                        continue;
                    }
                }

                if (DynamicSiteHelpers.IsInvisibleDlrStackFrame(method)) {
                    continue;
                }

                if (method.DeclaringType != null && Snippets.Shared.IsSnippetsAssembly(method.DeclaringType.Assembly)) {
                    yield return GetStackFrame(frame);
                }
            }
        }

        private static DynamicStackFrame GetStackFrame(StackFrame frame) {
            MethodBase method = frame.GetMethod();
            string methodName = method.Name;
            string filename = frame.GetFileName();
            int line = frame.GetFileLineNumber();

            int dollar = method.Name.IndexOf('$');
            if (dollar != -1) {
                methodName = methodName.Substring(0, dollar);
            }

            if (String.IsNullOrEmpty(filename)) {
                if (method.DeclaringType != null) {
                    filename = method.DeclaringType.Assembly.GetName().Name;
                    line = 0;
                }
            }

            return new DynamicStackFrame(method, methodName, filename, line);
        }

        #endregion
    }
}
