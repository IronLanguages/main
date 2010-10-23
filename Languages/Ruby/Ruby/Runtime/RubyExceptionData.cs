/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Globalization;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores extra instance data associated with Ruby exceptions
    /// </summary>
    [Serializable]
    public sealed class RubyExceptionData {
        private static readonly object/*!*/ _DataKey = typeof(RubyExceptionData);

        // An exception class can implement singleton method "new" that returns an arbitrary instance of an exception.
        // This mapping needs to be applied on exceptions created in libraries as well (they should be created "dynamically").
        // That would however need to pass RubyContext to every method that might throw an exception. Instead, we call
        // "new" on the exception's class as soon as it gets to the first Ruby EH handler (rescue/ensure/else).
        //
        // True if the exception has already been handled by Ruby EH clause or if it was constructed "dynamically" via Class#new.
        internal bool Handled { get; set; }

        // Real exception begin propagated by the CLR. Needed for lazy initialization of message, backtrace
        private Exception/*!*/ _exception;
        // For asynchronous exceptions (Thread#raise), the user-visible exception (accessible via _visibleException)
        // is wrapped in a TheadAbortException (accessible via _exception)
        private Exception/*!*/ _visibleException;

#if DEBUG
#pragma warning disable 414 // msc: unused field
        // For asynchronous exceptions, this is useful to figure out which thread raised the exception
        [NonSerialized]
        private Thread/*!*/ _throwingThread;
#pragma warning restore 414
#endif

        // if this is set to null we need to initialize it
        private object _message; 
        
        // can be set explicitly by the user (even to nil):
        private RubyArray _backtrace;

        [NonSerialized]
        private CallSite<Func<CallSite, RubyContext, Exception, RubyArray, object>> _setBacktraceCallSite;

        private RubyExceptionData(Exception/*!*/ exception) {
            _exception = exception;
            _visibleException = exception;
#if DEBUG
            _throwingThread = Thread.CurrentThread;
#endif
        }

        public static RubyArray/*!*/ CreateBacktrace(RubyContext/*!*/ context, int skipFrames) {
            return new RubyStackTraceBuilder(context, skipFrames).RubyTrace;
        }

        /// <summary>
        /// Builds backtrace for the exception if it wasn't built yet. 
        /// Captures a full stack trace starting with the current frame and combines it with the trace of the exception.
        /// Called from compiled code.
        /// </summary>
        internal void CaptureExceptionTrace(RubyScope/*!*/ scope) {
            if (_backtrace == null) {
                StackTrace catchSiteTrace = RubyStackTraceBuilder.GetClrStackTrace(null);
                _backtrace = new RubyStackTraceBuilder(scope.RubyContext, _exception, catchSiteTrace, scope.InterpretedFrame != null).RubyTrace;
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
                    _message = MutableString.Create(_visibleException.Message, RubyEncoding.UTF8);
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

        public static string/*!*/ GetClrMessage(RubyContext/*!*/ context, object message) {
            return Protocols.ToClrStringNoThrow(context, message);
        }

        public static string/*!*/ GetClrMessage(RubyClass/*!*/ exceptionClass, object message) {
            return GetClrMessage(exceptionClass.Context, message ?? exceptionClass.Name);
        }

        public static Exception/*!*/ InitializeException(Exception/*!*/ exception, object message) {
            RubyExceptionData data = RubyExceptionData.GetInstance(exception);
            // only set it if message is non-null. Otherwise, let lazy initialization create the default message from CLR exception message
            if (message != null) {
                data.Message = message;
            }

            return exception;
        }

        internal static Exception/*!*/ HandleException(RubyContext/*!*/ context, Exception/*!*/ exception) {
            // already handled:
            var instanceData = GetInstance(exception);
            if (instanceData.Handled) {
                return exception;
            }

            RubyClass exceptionClass = context.GetClass(exception.GetType());

            // new resolves to Class#new built-in method:
            var newMethod = exceptionClass.ImmediateClass.ResolveMethod("new", VisibilityContext.AllVisible);
            if (newMethod.Found && newMethod.Info.DeclaringModule == context.ClassClass && newMethod.Info is RubyCustomMethodInfo) {
                // initialize resolves to a built-in method:
                var initializeMethod = exceptionClass.ResolveMethod("initialize", VisibilityContext.AllVisible);
                if (initializeMethod.Found && initializeMethod.Info is RubyLibraryMethodInfo) {
                    instanceData.Handled = true;
                    return exception;
                }
            }

            var site = exceptionClass.NewSite;
            Exception newException;
            try {
                newException = site.Target(site, exceptionClass, instanceData.Message) as Exception;
            } catch (Exception e) {
                // MRI: this can lead to stack overflow:
                return HandleException(context, e);
            }

            // MRI doesn't handle this correctly, see http://redmine.ruby-lang.org/issues/show/1886:
            if (newException == null) {
                newException = RubyExceptions.CreateTypeError("exception object expected");
            }

            var newInstanceData = GetInstance(newException);
            
            newInstanceData.Handled = true;
            newInstanceData._backtrace = instanceData._backtrace;
            return newException;
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
