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
using System.IO;
using Microsoft.Scripting;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores extra instance data associated with Ruby exceptions
    /// </summary>
    [Serializable]
    public class RubyExceptionData {
        private static readonly object/*!*/ _DataKey = new object();
        internal const string TopLevelMethodName = "#";

#if SILVERLIGHT
        private static readonly bool DebugInfoAvailable = false;
#else
        private static readonly bool DebugInfoAvailable = true;
#endif

        // owner exception, needed for lazy initialization of message, backtrace
        private Exception/*!*/ _exception;
        // For asynchronous exceptions (Thread#raise), the user exception is wrapped in a TheadAbortException
        private Exception/*!*/ _visibleException;
        private Thread/*!*/ _throwingThread;

        // if this is set to null we need to initialize it
        private object _message; 
        
        // can be set explicitly by the user (even to nil):
        private RubyArray _backtrace;

        private CallSite<Action<CallSite, RubyContext, Exception, RubyArray>>/*!*/ _setBacktraceCallSite = CallSite<Action<CallSite, RubyContext, Exception, RubyArray>>.Create(RubyCallAction.Make("set_backtrace", 1));

        private RubyExceptionData(Exception/*!*/ exception) {
            _exception = exception;
            _visibleException = exception;
            _throwingThread = Thread.CurrentThread;
        }

        private void CreateBacktrace(RubyContext/*!*/ context, StackTrace catchSiteTrace) {
            int skipFrames = 0;
            bool hasFileAccessPermissions = DetectFileAccessPermissions();

            var result = new RubyArray();
            // Compiled trace: contains frames starting with the throw site up to the first filter/catch that the exception was caught by:
            StackTrace throwSiteTrace = DebugInfoAvailable ? new StackTrace(_exception, true) : new StackTrace(_exception);
            AddBacktrace(result, throwSiteTrace.GetFrames(), hasFileAccessPermissions, skipFrames, context.Options.ExceptionDetail);

            if (catchSiteTrace != null) {
                // skip one frame - the catch-site frame is already included
                AddBacktrace(result, catchSiteTrace.GetFrames(), hasFileAccessPermissions, 1, false);
            }

            _backtrace = result;
            SetBacktraceForRaise(_setBacktraceCallSite, context, _backtrace);
        }

        internal void SetCompiledTrace(RubyContext/*!*/ context) {
            if (_backtrace != null) {
                return;
            }

            // Compiled trace: contains frames above and including the first Ruby filter/catch site that the exception was caught by:
            StackTrace catchSiteTrace = DebugInfoAvailable ? new StackTrace(true) : new StackTrace();

            CreateBacktrace(context, catchSiteTrace);
        }

        internal void SetInterpretedTrace(InterpreterState/*!*/ state) {
            if (_backtrace != null) {
                return;
            }

            // we need to copy the trace since the source locations in frames above catch site could be altered by further interpretation:
            _backtrace = AddBacktrace(new RubyArray(), state, 0);
            SetBacktraceForRaise(_setBacktraceCallSite, state.ScriptCode.LanguageContext as RubyContext, _backtrace);
        }

        /// <summary>
        /// This is called by the IronRuby runtime to set the backtrace for an exception that has being raised. 
        /// Note that the backtrace may be set directly by user code as well. However, that uses a different code path.
        /// </summary>
        public void SetBacktraceForRaise(CallSite<Action<CallSite, RubyContext, Exception, RubyArray>> setBacktraceCallSite, RubyContext/*!*/ context, RubyArray backtrace) {
            setBacktraceCallSite.Target(setBacktraceCallSite, context, _exception, backtrace);
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
                var trace = DebugInfoAvailable ? new StackTrace(true) : new StackTrace();
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
                    string methodName, file;
                    int line;
                    if (TryGetStackFrameInfo(frame, hasFileAccessPermission, exceptionDetail, out methodName, out file, out line)) {
                        if (skipFrames == 0) {
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

        private static bool TryGetStackFrameInfo(StackFrame/*!*/ frame, bool hasFileAccessPermission, bool exceptionDetail,
            out string/*!*/ methodName, out string/*!*/ fileName, out int line) {

            MethodBase method = frame.GetMethod();
            methodName = method.Name;

            fileName = (hasFileAccessPermission) ? frame.GetFileName() : null;
            line = frame.GetFileLineNumber();

            if (TryParseRubyMethodName(ref methodName, ref fileName, ref line)) {
                // Ruby method:
                if (methodName == TopLevelMethodName) {
                    methodName = null;
                }
                return true;
            } else if (method.IsDefined(typeof(RubyStackTraceHiddenAttribute), false)) {
                return false;
            } else {
                object[] attrs = method.GetCustomAttributes(typeof(RubyMethodAttribute), false);
                if (attrs.Length > 0) {
                    // Ruby library method:
                    // TODO: aliases
                    methodName = ((RubyMethodAttribute)attrs[0]).Name;
                    fileName = null;
                    line = 0;
                    return true;
                } else if (exceptionDetail || IsVisibleClrFrame(method)) {
                    // Visible CLR method:
                    if (String.IsNullOrEmpty(fileName)) {
                        if (method.DeclaringType != null) {
                            fileName = (hasFileAccessPermission) ? method.DeclaringType.Assembly.GetName().Name : null;
                            line = 0;
                        }
                    }
                    return true;
                } else {
                    // Invisible CLR method:
                    return false;
                }
            }
        }

        private static bool IsVisibleClrFrame(MethodBase/*!*/ method) {
            if (Microsoft.Scripting.Actions.DynamicSiteHelpers.IsInvisibleDlrStackFrame(method)) {
                return false;
            }

            Type type = method.DeclaringType;
            if (type != null) {
                if (type.Assembly == typeof(RubyOps).Assembly) {
                    return false;
                }
            }
            // TODO: check loaded assemblies?
            return true;
        }

        private const string RubyMethodPrefix = "\u2111\u211c;";

        internal static string/*!*/ EncodeMethodName(SourceUnit/*!*/ sourceUnit, string/*!*/ methodName, SourceSpan location) {
            // encodes line number, file name into the method name
            string fileName = sourceUnit.HasPath ? Path.GetFileName(sourceUnit.Path) : String.Empty;
            return String.Format(RubyMethodPrefix + "{0};{1};{2};", methodName, fileName, location.IsValid ? location.Start.Line : 0);
        }

        // \u2111\u211c;{method-name};{file-name};{line-number};{dlr-suffix}
        private static bool TryParseRubyMethodName(ref string methodName, ref string fileName, ref int line) {
            if (methodName.StartsWith(RubyMethodPrefix)) {
                string[] parts = methodName.Split(';');
                if (parts.Length > 4) {
                    methodName = parts[1];
                    if (fileName == null) {
                        fileName = parts[2];
                    }
                    if (line == 0) {
                        line = Int32.Parse(parts[3]);
                    }
                    return true;
                }
            }
            return false;
        }

        private static string ParseRubyMethodName(string/*!*/ lambdaName) {
            if (!lambdaName.StartsWith(RubyMethodPrefix)) {
                return lambdaName;
            }

            int nameEnd = lambdaName.IndexOf(';', RubyMethodPrefix.Length);
            string name = lambdaName.Substring(RubyMethodPrefix.Length, nameEnd - RubyMethodPrefix.Length);
            return (name != TopLevelMethodName) ? name : null;
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

            Exception visibleException = RubyUtils.GetVisibleException(e);
            if (e == visibleException || visibleException == null) {
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
                return _backtrace;
            }
            set {
                _backtrace = value;
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
            Debug.Assert(RubyUtils.GetVisibleException(visibleException) == visibleException);

            RubyExceptionData data = RubyExceptionData.GetInstance(visibleException);
            if (data._exception != visibleException && data._throwingThread == Thread.CurrentThread) {
                Debug.Assert((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0);
                Thread.ResetAbort();
            }
        }
#endif
    }
}
