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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores extra instance data associated with Ruby exceptions
    /// </summary>
    [Serializable]
    public sealed class RubyExceptionData {
        internal static readonly Microsoft.Scripting.Utils.ThreadLocal<InterpretedFrame> CurrentInterpretedFrame = new Microsoft.Scripting.Utils.ThreadLocal<InterpretedFrame>();

        private static readonly object/*!*/ _DataKey = new object();
        internal const string TopLevelMethodName = "#";

#if SILVERLIGHT
        private static readonly bool DebugInfoAvailable = false;
#else
        private static readonly bool DebugInfoAvailable = true;
#endif

        // Real exception begin propagated by the CLR. Needed for lazy initialization of message, backtrace
        private Exception/*!*/ _exception;
        // For asynchronous exceptions (Thread#raise), the user-visible exception (accessible via _visibleException)
        // is wrapped in a TheadAbortException (accessible via _exception)
        private Exception/*!*/ _visibleException;
#if DEBUG
        // For asynchronous exceptions, this is useful to figure out which thread raised the exception
        private Thread/*!*/ _throwingThread;
#endif

        // if this is set to null we need to initialize it
        private object _message; 
        
        // can be set explicitly by the user (even to nil):
        private RubyArray _backtrace;

        private CallSite<Func<CallSite, RubyContext, Exception, RubyArray, object>>/*!*/ _setBacktraceCallSite;

        private RubyExceptionData(Exception/*!*/ exception) {
            _exception = exception;
            _visibleException = exception;
#if DEBUG
            _throwingThread = Thread.CurrentThread;
#endif
        }

        private RubyArray CreateBacktrace(RubyContext/*!*/ context, InterpretedFrame handlerFrame, StackTrace catchSiteTrace) {
            Assert.NotNull(context);

            bool hasFileAccessPermissions = DetectFileAccessPermissions();

            var result = new RubyArray();
            
            // Compiled trace: contains frames starting with the throw site up to the first filter/catch that the exception was caught by:
            StackTrace throwSiteTrace = DebugInfoAvailable ? new StackTrace(_exception, true) : new StackTrace(_exception);

            var interpretedFrame = handlerFrame ?? CurrentInterpretedFrame.Value;
            AddBacktrace(result, throwSiteTrace.GetFrames(), ref interpretedFrame, handlerFrame, hasFileAccessPermissions, 0, context.Options.ExceptionDetail);

            // Compiled trace: contains frames above and including the first Ruby filter/catch site that the exception was caught by:
            if (catchSiteTrace != null) {
                // skip one frame - the catch-site frame is already included
                AddBacktrace(result, catchSiteTrace.GetFrames(), ref interpretedFrame, handlerFrame, hasFileAccessPermissions,
                    handlerFrame != null ? 0 : 1, false
                );
            }

            return result;            
        }

        /// <summary>
        /// Builds backtrace for the exception if it wasn't built yet. 
        /// Captures a full stack trace starting with the current frame and combines it with the trace of the exception.
        /// Called from compiled code.
        /// </summary>
        internal void CaptureExceptionTrace(RubyScope/*!*/ scope) {
            if (_backtrace == null) {
                // If we are in an interpreted method, the CurrentInterpretedFrame is the first Ruby frame that the exception passes thru.
                // (if it was not the first one _backtrace would already been set
                StackTrace catchSiteTrace = DebugInfoAvailable ? new StackTrace(true) : new StackTrace();
                _backtrace = CreateBacktrace(scope.RubyContext, scope.InterpretedFrame, catchSiteTrace);
                DynamicSetBacktrace(scope.RubyContext, _backtrace);
            }
        }

        /// <summary>
        /// This is called by the IronRuby runtime to set the backtrace for an exception that has being raised. 
        /// Note that the backtrace may be set directly by user code as well. However, that uses a different code path.
        /// </summary>
        private void DynamicSetBacktrace(RubyContext/*!*/ context, RubyArray backtrace) {
            if (_setBacktraceCallSite == null) {
                Interlocked.CompareExchange(ref _setBacktraceCallSite, CallSite<Func<CallSite, RubyContext, Exception, RubyArray, object>>.
                    Create(RubyCallAction.MakeShared("set_backtrace", RubyCallSignature.WithImplicitSelf(1))), null);
            }
            _setBacktraceCallSite.Target(_setBacktraceCallSite, context, _exception, backtrace);
        }

        public static RubyArray/*!*/ CreateBacktrace(RubyContext/*!*/ context, int skipFrames) {
            var trace = DebugInfoAvailable ? new StackTrace(true) : new StackTrace();
            var interpretedFrame = CurrentInterpretedFrame.Value;
            return AddBacktrace(
                new RubyArray(), trace.GetFrames(), ref interpretedFrame, null, DetectFileAccessPermissions(), 
                skipFrames, context.Options.ExceptionDetail
            );
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

        private static RubyArray/*!*/ AddBacktrace(RubyArray/*!*/ result, IEnumerable<StackFrame> stackTrace,
            ref InterpretedFrame interpretedFrame, InterpretedFrame handlerFrame,
            bool hasFileAccessPermission, int skipFrames, bool exceptionDetail) {

            if (stackTrace != null) {
                foreach (StackFrame frame in stackTrace) {
                    string methodName, file;
                    int line;

                    if (InterpretedFrame.IsInterpretedFrame(frame.GetMethod())) {
                        // TODO: get language context, ask for method name?
                        // TODO: the trace can get corrupted if Python frame are in the middle - we need to move frame tracing to the interpreter
                        if (interpretedFrame == null) {
                            continue;
                        }

                        var debugInfo = interpretedFrame.GetDebugInfo(
                            (interpretedFrame == handlerFrame) ? interpretedFrame.FaultingInstruction : interpretedFrame.InstructionIndex
                        );

                        if (debugInfo != null) {
                            file = debugInfo.FileName;
                            line = debugInfo.StartLine;
                        } else {
                            file = null;
                            line = 0;
                        }
                        methodName = interpretedFrame.Lambda.Name;
                        TryParseRubyMethodName(ref methodName, ref file, ref line);
                        
                        interpretedFrame = interpretedFrame.Parent;                        
                    } else if (!TryGetStackFrameInfo(frame, hasFileAccessPermission, exceptionDetail, out methodName, out file, out line)) {
                        continue;
                    }

                    if (skipFrames == 0) {
                        result.Add(MutableString.Create(FormatFrame(file, line, methodName)));
                    } else {
                        skipFrames--;
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
            var sourceLine = line = frame.GetFileLineNumber();

            if (TryParseRubyMethodName(ref methodName, ref fileName, ref line)) {
                if (sourceLine == 0) {
                    RubyMethodDebugInfo debugInfo;
                    if (RubyMethodDebugInfo.TryGet(method, out debugInfo)) {
                        var ilOffset = frame.GetILOffset();
                        if (ilOffset >= 0) {
                            var mappedLine = debugInfo.Map(ilOffset);
                            if (mappedLine != 0) {
                                line = mappedLine;
                            }
                        }
                    }
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
#if !DEBUG
                    if (!exceptionDetail) {
                        fileName = null;
                        line = 0;
                    }
#endif                    
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
        private static int _Id = 0;

        internal static string/*!*/ EncodeMethodName(string/*!*/ methodName, string sourcePath, SourceSpan location) {
            // encodes line number, file name into the method name
            string fileName = sourcePath != null ? Path.GetFileName(sourcePath) : null;
            return String.Format(RubyMethodPrefix + "{0};{1};{2};{3}", methodName, fileName, location.IsValid ? location.Start.Line : 0,
                Interlocked.Increment(ref _Id));
        }

        // \u2111\u211c;{method-name};{file-name};{line-number};{dlr-suffix}
        private static bool TryParseRubyMethodName(ref string methodName, ref string fileName, ref int line) {
            if (methodName.StartsWith(RubyMethodPrefix)) {
                string[] parts = methodName.Split(';');
                if (parts.Length > 4) {
                    methodName = parts[1];
                    if (methodName == TopLevelMethodName) {
                        methodName = null;
                    }
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
        public static void ActiveExceptionHandled(Exception visibleException) {
            Debug.Assert(RubyUtils.GetVisibleException(visibleException) == visibleException);

            RubyExceptionData data = RubyExceptionData.GetInstance(visibleException);
            if (data._exception != visibleException) {
                // The exception was raised asynchronously with Thread.Abort. We can not just catch and ignore 
                // the ThreadAbortException as the CLR keeps trying to re-raise it unless ResetAbort is called.
                //
                // Note that ResetAbort can cause ThreadAbortException.ExceptionState to be cleared (though it may 
                // not be cleared under some circustances), and we use that to squirrel away the Ruby exception 
                // that the user is expecting. Hence, ResetAbort should only be called when 
                // ThreadAbortException.ExceptionState no longer needs to be accessed. 
                if ((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0) {
                    Thread.ResetAbort();
                }
            }
        }
#endif
    }
}
