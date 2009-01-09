/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Security;
using System.Security.Permissions;
using IronRuby.Builtins;
using Microsoft.Scripting.Interpretation;
using System.Linq.Expressions;
using System.Threading;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores extra instance data associated with Ruby exceptions
    /// </summary>
    [Serializable]
    public class RubyExceptionData {
        private static readonly object/*!*/ _DataKey = new object();
        internal const string TopLevelMethodName = "#top-level-method#";

        // owner exception, needed for lazy initialization of message, backtrace
        private Exception/*!*/ _exception;
        // For asynchronous exceptions (Thread#raise), the user exception is wrapped in a TheadAbortException
        private Exception/*!*/ _visibleException;
        private Thread/*!*/ _throwingThread;

        // if this is set to null we need to initialize it
        private object _message; 
        
        // can be set explicitly by the user (even to nil):
        private RubyArray _backtrace;

        // false if _backtrace needs to be initialized.
        private bool _backtraceInitialized; 

        // Compiled trace: contains frames above and including the first Ruby filter/catch site that the exception was caught by:
        private StackTrace _catchSiteTrace;

        // Compiled trace: contains frames starting with the throw site up to the first filter/catch that the exception was caught by:
        private StackTrace _throwSiteTrace; 

        private RubyExceptionData(Exception/*!*/ exception) {
            _exception = exception;
            _visibleException = exception;
            _throwingThread = Thread.CurrentThread;
        }

        // Called lazily to create a Ruby backtrace.
        private void CreateBacktrace() {
            RubyArray result;

            int skipFrames = 0;
            bool hasFileAccessPermissions = DetectFileAccessPermissions();

#if SILVERLIGHT // TODO: StackTrace.ctor(exception) security critical
            // throw-site trace is built already:
            result = _backtrace ?? new RubyArray();
#else
            result = new RubyArray();
            if (_throwSiteTrace == null) {
                SetCompiledTrace();
            }

            AddBacktrace(result, _throwSiteTrace.GetFrames(), hasFileAccessPermissions, skipFrames, false);
#endif
            if (_catchSiteTrace != null) {
                // skip one frame - the catch-site frame is already included
                AddBacktrace(result, _catchSiteTrace.GetFrames(), hasFileAccessPermissions, 1, false);
            }

            _backtrace = result;
            _backtraceInitialized = true;
        }

        internal void SetCompiledTrace() {
            if (_exception != _visibleException) {
                // Thread#raise uses Thread.Abort to raise an async exception. In such cases, a different instance of 
                // ThreadAbortException is thrown at the end of every catch block (as long as Thread.ResetAbort is not called). 
                // However, we only want to remember the first one as it will have the most complete stack trace.
                // So we ignore subsequent calls.
                if (_backtraceInitialized) {
                    return;
                }
            }

            Debug.Assert(!_backtraceInitialized);

#if SILVERLIGHT // TODO: StackTrace.ctor(exception) security critical
            _catchSiteTrace = new StackTrace();

            var result = new RubyArray();
            foreach (string line in _exception.StackTrace.Split('\n')) {
                string frame = line.Trim();
                if (frame.StartsWith("at ")) {
                    frame = frame.Substring("at ".Length);
                }

                if (frame.StartsWith("_stub_") ||
                    frame.StartsWith("Microsoft.Scripting") ||
                    frame.StartsWith("System.Runtime") ||
                    frame.StartsWith("IronRuby.Builtins.Kernel.RaiseException") ||
                    frame.StartsWith("IronRuby.Builtins.Kernel.MethodMissing")) {
                    continue;
                }

                int lineNumber = 0;
                string fileName = null;
                string methodName = frame;
                TryParseRubyMethodName(ref methodName, ref fileName, ref lineNumber);
                result.Add(FormatFrame(methodName, lineNumber, fileName));
            }

            // save partial trace:
            _backtrace = result; 
#else
            _catchSiteTrace = new StackTrace(true);
            _throwSiteTrace = new StackTrace(_exception, true);
#endif
        }

        internal void SetInterpretedTrace(InterpreterState/*!*/ state) {
            if (_exception != _visibleException) {
                // Thread#raise uses Thread.Abort to raise an async exception. In such cases, a different instance of 
                // ThreadAbortException is thrown at the end of every catch block (as long as Thread.ResetAbort is not called). 
                // However, we only want to remember the first one as it will have the most complete stack trace.
                // So we ignore subsequent calls.
                if (_backtraceInitialized) {
                    return;
                }
            }

            Debug.Assert(!_backtraceInitialized);

            // we need to copy the trace since the source locations in frames above catch site could be altered by further interpretation:
            _backtrace = AddBacktrace(new RubyArray(), state, 0);
            _backtraceInitialized = true;
        }

        internal static RubyArray/*!*/ CreateBacktrace(RubyContext/*!*/ context, IEnumerable<StackFrame>/*!*/ stackTrace, int skipFrames) {
            return AddBacktrace(new RubyArray(), stackTrace, DetectFileAccessPermissions(), skipFrames, context.Options.ExceptionDetail);
        }

        public static RubyArray/*!*/ CreateBacktrace(RubyContext/*!*/ context, int skipFrames) {
            if (context.Options.InterpretedMode) {
                var currentFrame = InterpreterState.Current.Value;
                Debug.Assert(currentFrame != null); 
                return AddBacktrace(new RubyArray(), currentFrame, skipFrames);
            } else {
#if SILVERLIGHT
                StackTrace trace = new StackTrace();
#else
                StackTrace trace = new StackTrace(true);
#endif
                return AddBacktrace(new RubyArray(), trace.GetFrames(), DetectFileAccessPermissions(), skipFrames, context.Options.ExceptionDetail);
            }
        }

        // TODO: partial trust
        private static bool DetectFileAccessPermissions() {
#if SILVERLIGHT
            return false;
#else
            try {
                new FileIOPermission(PermissionState.Unrestricted).Demand();
                return true;
            } catch (SecurityException) {
                return false;
            }
#endif
        }

        private static RubyArray/*!*/ AddBacktrace(RubyArray/*!*/ result, InterpreterState/*!*/ frame, int skipFrames) {
            do {
                if (skipFrames == 0) {
                    string methodName;

                    // TODO: generalize for all languages
                    if (frame.ScriptCode.LanguageContext is RubyContext) {
                        methodName = ParseRubyMethodName(frame.Lambda.Name);
                    } else {
                        methodName = frame.Lambda.Name;
                    }

                    result.Add(MutableString.Create(FormatFrame(
                        frame.ScriptCode.SourceUnit.Path,
                        frame.CurrentLocation.Line,
                        methodName
                    )));
                } else {
                    skipFrames--;
                }

                frame = frame.Caller;
            } while (frame != null);

            return result;
        }

        private static RubyArray/*!*/ AddBacktrace(RubyArray/*!*/ result, IEnumerable<StackFrame> stackTrace, bool hasFileAccessPermission, 
            int skipFrames, bool exceptionDetail) {

            if (stackTrace != null) {
                foreach (StackFrame frame in stackTrace) {
                    if (IsVisibleFrame(frame.GetMethod()) || exceptionDetail) {
                        if (skipFrames == 0) {
                            string methodName, file;
                            int line;
                            GetStackFrameInfo(frame, hasFileAccessPermission, out methodName, out file, out line);
                            result.Add(MutableString.Create(FormatFrame(file, line, methodName)));
                        } else {
                            skipFrames--;
                        }
                    }
                }
            }

            return result;
        }

        private static string/*!*/ FormatFrame(string file, int line, string methodName) {
            if (String.IsNullOrEmpty(methodName)) {
                return String.Format("{0}:{1}", file, line);
            } else {
                return String.Format("{0}:{1}:in `{2}'", file, line, methodName);
            }
        }

        private static void GetStackFrameInfo(StackFrame/*!*/ frame, bool hasFileAccessPermission, 
            out string/*!*/ methodName, out string/*!*/ fileName, out int line) {

            MethodBase method = frame.GetMethod();
            methodName = method.Name;

#if SILVERLIGHT
            fileName = null;
            line = 0;
#else
            fileName = (hasFileAccessPermission) ? frame.GetFileName() : null;
            line = frame.GetFileLineNumber();
#endif
            if (!TryParseRubyMethodName(ref methodName, ref fileName, ref line)) {
                object[] attrs = method.GetCustomAttributes(typeof(RubyMethodAttribute), false);
                if (attrs.Length > 0) {
                    // TODO: aliases
                    methodName = ((RubyMethodAttribute)attrs[0]).Name;
                }
            }

            if (methodName.StartsWith(TopLevelMethodName)) {
                methodName = null;
            }

#if !SILVERLIGHT
            if (String.IsNullOrEmpty(fileName)) {
                if (method.DeclaringType != null) {
                    fileName = (hasFileAccessPermission) ? method.DeclaringType.Assembly.GetName().Name : null;
                    line = 0;
                }
            }
#endif
        }

        // {method-name};{file-name};{line-number}${dlr-suffix}
        private static bool TryParseRubyMethodName(ref string methodName, ref string fileName, ref int line) {
            int dollar = methodName.IndexOf('$'); // TODO: no support in DLR
            if (dollar != -1) {
                string[] parts = methodName.Substring(0, dollar).Split(';');
                methodName = parts[0];
                if (parts.Length == 3) {
                    if (fileName == null) {
                        fileName = parts[1];
                    }
                    if (line == 0) {
                        line = Int32.Parse(parts[2]);
                    }
                }
                return true;
            } else {
                return false;
            }
        }

        private static string ParseRubyMethodName(string/*!*/ lambdaName) {
            if (lambdaName.StartsWith(TopLevelMethodName)) {
                return null;
            }

            int idx = lambdaName.IndexOf(';');
            if (idx < 0) {
                return lambdaName;
            }

            return lambdaName.Substring(0, idx);
        }

        // TODO: better filtering in DLR
        private static bool IsVisibleFrame(MethodBase/*!*/ method) {
            // filter out the _stub_ methods
            if (method.Name.StartsWith("_stub_")) {
                return false;
            }
                
            Type type = method.DeclaringType;

            if (type != null) {
                string typeName = type.FullName;
                if (typeName.StartsWith("System.Reflection.") ||
                    typeName.StartsWith("System.Runtime") ||
                    typeName.StartsWith("System.Dynamic") ||
                    typeName.StartsWith("Microsoft.Scripting")) {
                    return false;
                }

                // TODO: check loaded assemblies
                if (type.Assembly == typeof(RubyOps).Assembly) {
                    return false;
                }

                if (method.IsDefined(typeof(RubyStackTraceHiddenAttribute), false)) {
                    return false;
                }
                 
                if (type.Assembly.IsDefined(typeof(RubyLibraryAttribute), false)) {
                    return method.IsDefined(typeof(RubyMethodAttribute), false);
                }
            }
                
            return true;
        }

        /// <summary>
        /// Gets the instance data associated with the exception
        /// </summary>
        public static RubyExceptionData/*!*/ GetInstance(Exception/*!*/ e) {
            RubyExceptionData result = TryGetInstance(e);
            if (result == null) {
                result = AssociateInstance(e);                
            }
            return result;
        }

        internal static RubyExceptionData/*!*/ AssociateInstance(Exception/*!*/ e) {
            RubyExceptionData result;

            Exception visibleException = RubyOps.GetVisibleException(e);
            if (e == visibleException) {
                result = new RubyExceptionData(e);
            } else {
                // Async exception

                Debug.Assert(e is ThreadAbortException);
                result = GetInstance(visibleException);

                // Since visibleException was instantiated by the thread calling Thread#raise, we need to reset it here
                result._throwingThread = Thread.CurrentThread;

                if (result._exception == visibleException) {
                    // A different instance of ThreadAbortException is thrown at the end of every catch block (as long as
                    // Thread.ResetAbort is not called). However, we only want to remember the first one 
                    // as it will have the most complete stack trace.
                    result._exception = e;
                }
            }

            e.Data[_DataKey] = result;
            return result;
        }

        internal static RubyExceptionData TryGetInstance(Exception/*!*/ e) {
            return e.Data[_DataKey] as RubyExceptionData;
        }
        
        public object Message {
            get {
                if (_message == null) {
                    _message = MutableString.Create(_visibleException.Message);
                }
                return _message;
            }
            set { 
                ContractUtils.RequiresNotNull(value, "value"); 
                _message = value; 
            }
        }

        public RubyArray Backtrace {
            get {
                if (!_backtraceInitialized) {
                    CreateBacktrace();
                }
                return _backtrace;
            }
            set {
                _backtraceInitialized = true;
                _backtrace = value;
            }
        }

        /// <summary>
        /// Called from Kernel#raise, when throwing a user created Exception objects
        /// Clears out any backtrace set by the user
        /// This causes the new one to be lazily created the next time it is accessed
        /// </summary>
        public static void ClearBacktrace(Exception e) {
            RubyExceptionData result = TryGetInstance(e);
            if (result != null) {
                result._backtraceInitialized = false;
                result._backtrace = null;
                result._catchSiteTrace = null;
            }
        }

        public static string/*!*/ GetClrMessage(object message, string/*!*/ className) {
            // TODO: we can use to_s protocol conversion that doesn't throw an exception:
            var str = message as MutableString;
            return (str != null) ? str.ToString() : className;
        }

        public static string/*!*/ GetClrMessage(RubyClass/*!*/ exceptionClass, object message) {
            return GetClrMessage(message, exceptionClass.Name);
        }

        public static Exception/*!*/ InitializeException(Exception/*!*/ exception, object message) {
            RubyExceptionData data = RubyExceptionData.GetInstance(exception);
            // only set it if message is non-null. Otherwise, let lazy initialization create the default message from CLR exception message
            if (message != null) {
                data.Message = message;
            }

            return exception;
        }

#if SILVERLIGHT // Thread.ExceptionState
        public static void ActiveExceptionHandled(Exception visibleException) {}
#else
        /// <summary>
        /// This function calls Thread.ResetAbort. However, note that ResetAbort causes ThreadAbortException.ExceptionState 
        /// to be cleared, and we use that to squirrel away the Ruby exception that the user is expecting. Hence, ResetAbort
        /// should only be called when ThreadAbortException.ExceptionState no longer needs to be accessed.
        /// </summary>
        /// <param name="visibleException"></param>
        public static void ActiveExceptionHandled(Exception visibleException) {
            Debug.Assert(RubyOps.GetVisibleException(visibleException) == visibleException);

            RubyExceptionData data = RubyExceptionData.GetInstance(visibleException);
            if (data._exception != visibleException && data._throwingThread == Thread.CurrentThread) {
                Debug.Assert((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0);
                Thread.ResetAbort();
            }
        }
#endif
    }
}
