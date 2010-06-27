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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyException("NoMemoryError"), Serializable]
    public class NoMemoryError : Exception {
        public NoMemoryError() : this(null, null) { }
        public NoMemoryError(string message): this(message, null) { }
        public NoMemoryError(string message, Exception inner) : base(message ?? "NoMemoryError", inner) { }

#if !SILVERLIGHT
        protected NoMemoryError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("EOFError"), Serializable]
    public class EOFError : IOException {
        public EOFError() : this(null, null) { }
        public EOFError(string message): this(message, null) { }
        public EOFError(string message, Exception inner) : base(message ?? "EOFError", inner) { }

#if !SILVERLIGHT
        protected EOFError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("FloatDomainError"), Serializable]
    public class FloatDomainError : ArgumentOutOfRangeException {
        public FloatDomainError() : this(null, null) { }
        public FloatDomainError(string message) : this(message, null) { }
        public FloatDomainError(string message, Exception inner) : base(message ?? "FloatDomainError", inner) { }

#if !SILVERLIGHT
        protected FloatDomainError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("ThreadError"), Serializable]
    public class ThreadError : SystemException {
        public ThreadError() : this(null, null) { }
        public ThreadError(string message): this(message, null) { }
        public ThreadError(string message, Exception inner) : base(message ?? "ThreadError", inner) { }

#if !SILVERLIGHT
        protected ThreadError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("SystemExit", Extends = typeof(SystemExit))]
    public class SystemExitOps : Exception {
        [RubyMethod("status")]
        public static int GetStatus(SystemExit/*!*/ self) {
            return self.Status;
        }

        [RubyMethod("success?")]
        public static bool IsSuccessful(SystemExit/*!*/ self) {
            return self.Status == 0;
        }

        [RubyConstructor]
        public static SystemExit/*!*/ Factory(RubyClass/*!*/ self, object message) {
            return Factory(self, 0, message);
        }

        [RubyConstructor]
        public static SystemExit/*!*/ Factory(RubyClass/*!*/ self, [Optional]int status, [DefaultParameterValue(null)]object message) {
            SystemExit result = new SystemExit(status, RubyExceptionData.GetClrMessage(self, message ?? "SystemExit"));
            RubyExceptionData.InitializeException(result, message);
            return result;
        }
    }

    [RubyException("SignalException"), Serializable]
    public class SignalException : Exception {
        public SignalException() : this(null, null) { }
        public SignalException(string message): this(message, null) { }
        public SignalException(string message, Exception inner) : base(message ?? "SignalException", inner) { }

#if !SILVERLIGHT
        protected SignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("Interrupt", Inherits = typeof(SignalException)), Serializable]
    public class Interrupt : Exception { 
        public Interrupt() : this(null, null) { }
        public Interrupt(string message): this(message, null) { }
        public Interrupt(string message, Exception inner) : base(message ?? "Interrupt", inner) { }

#if !SILVERLIGHT
        protected Interrupt(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
#endif
    }

    [RubyException("LocalJumpError", Extends = typeof(LocalJumpError))]
    public static class LocalJumpErrorOps {
    }

    [RubyException("ScriptError", Extends = typeof(ScriptError))]
    public static class ScriptErrorOps {
    }

    [RubyException("NotImplementedError", Extends = typeof(NotImplementedError))]
    public static class NotImplementedErrorOps {
    }

    [RubyException("LoadError", Extends = typeof(LoadError))]
    public static class LoadErrorOps {
    }

    [RubyException("RegexpError", Extends = typeof(RegexpError))]
    public static class RegexpErrorOps {
    }

    [RubyException("SyntaxError", Extends = typeof(SyntaxError))]
    public static class SyntaxErrorOps {
    }

    [RubyException("SystemStackError", Extends = typeof(SystemStackError), Inherits = typeof(SystemException))]
    public static class SystemStackErrorOps {
    }

    [RubyException("StandardError", Extends = typeof(SystemException), Inherits = typeof(Exception))]
    public static class SystemExceptionOps {
    }

    [RubyException("ArgumentError", Extends = typeof(ArgumentException), Inherits = typeof(SystemException))]
    [HideMethod("message")]
    public static class ArgumentErrorOps {
    }

    [RubyException("IOError", Extends = typeof(IOException), Inherits = typeof(SystemException))]
    public static class IOErrorOps {
    }

    [RubyException("IndexError", Extends = typeof(IndexOutOfRangeException), Inherits = typeof(SystemException))]
    public static class IndexErrorOps {
    }

    [RubyException("RangeError", Extends = typeof(ArgumentOutOfRangeException), Inherits = typeof(SystemException))]
    [HideMethod("message")]
    public static class RangeErrorOps {
    }

    [RubyException("NameError", Extends = typeof(MemberAccessException), Inherits = typeof(SystemException))]
    public static class NameErrorOps {
    }

    [RubyException("NoMethodError", Extends = typeof(MissingMethodException), Inherits = typeof(MemberAccessException))]
    [HideMethod("message")]
    public static class NoMethodErrorOps {        
        [RubyConstructor]
        public static MissingMethodException/*!*/ Factory(
            RubyClass/*!*/ self,
            [DefaultParameterValue(null)]object message, 
            [DefaultParameterValue(null)]object name,
            [DefaultParameterValue(null)]object args) {
            MissingMethodException result = new MissingMethodException(RubyExceptionData.GetClrMessage(self, message ?? "NoMethodError"));
            RubyExceptionData.InitializeException(result, message);
            // Exception.Data requires the value to be Serializable. We workaround this using an array
            // of size 1 since System.Array is serializable. This will allow the exception to be marshalled.
            // If the value cannot actually be marshalled, it will fail only if the value is later accessed.
#if SILVERLIGHT
            result.Data[typeof(NoMethodErrorOps)] = new object[1] { args };
#else
            result.Data[typeof(NoMethodErrorOps)] = new ObjectHandle[1] { new ObjectHandle(args) };
#endif
            return result;
        }

        [RubyMethod("args")]
        public static object GetArguments(MissingMethodException/*!*/ self) {
#if SILVERLIGHT
            object[] args = self.Data[typeof(NoMethodErrorOps)] as object[];
            if (args == null) {
                return null;
            }
            return args[0];
#else
            ObjectHandle[] args = self.Data[typeof(NoMethodErrorOps)] as ObjectHandle[];
            if (args == null) {
                return null;
            }
            return args[0].Unwrap();
#endif
        }
    }

    [RubyException("SecurityError", Extends = typeof(SecurityException), Inherits = typeof(SystemException))]
    public static class SecurityErrorOps {
    }

    [RubyException("TypeError", Extends = typeof(InvalidOperationException), Inherits = typeof(SystemException))]
    public static class TypeErrorOps {
    }

    [RubyException("ZeroDivisionError", Extends = typeof(DivideByZeroException), Inherits = typeof(SystemException))]
    public static class ZeroDivisionErrorOps {
    }

    [RubyException("EncodingError", Extends = typeof(EncodingError), Inherits = typeof(SystemException))]
    public static class EncodingErrorOps {
    }

    [RubyException("RuntimeError", Extends = typeof(RuntimeError), Inherits = typeof(SystemException))]
    public static class RuntimeErrorOps {
    }

    // special one:
    [RubyException("SystemCallError", Extends = typeof(ExternalException), Inherits = typeof(SystemException))]
    public static class SystemCallErrorOps {
        [RubyConstructor]
        public static ExternalException/*!*/ Factory(RubyClass/*!*/ self, [DefaultProtocol]MutableString message) {
            ExternalException result = new ExternalException(RubyExceptions.MakeMessage(ref message, "unknown error"));
            RubyExceptionData.InitializeException(result, message);
            return result;
        }

        [RubyConstructor]
        public static ExternalException/*!*/ Factory(RubyClass/*!*/ self, int errorCode) {
            // TODO:
            var message = MutableString.Create("system error #" + errorCode, RubyEncoding.UTF8);

            ExternalException result = new ExternalException(RubyExceptions.MakeMessage(ref message, "unknown error"));
            RubyExceptionData.InitializeException(result, message);
            return result;
        }
    }
}
