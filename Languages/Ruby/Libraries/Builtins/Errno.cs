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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.IO;

namespace IronRuby.Builtins {
    [RubyModule("Errno")]
    public static class Errno {
        // Errno.constants.sort

        internal static UnauthorizedAccessException/*!*/ CreateEACCES() {
            return new UnauthorizedAccessException();
        }

        internal static UnauthorizedAccessException/*!*/ CreateEACCES(string message) {
            return new UnauthorizedAccessException(message);
        }

        internal static UnauthorizedAccessException/*!*/ CreateEACCES(string message, Exception inner) {
            return new UnauthorizedAccessException(message, inner);
        }

        [RubyClass("EAGAIN"), Serializable]
        public class ResourceTemporarilyUnavailableError : ExternalException {
            private const string/*!*/ M = "Resource temporarily unavailable";

            public ResourceTemporarilyUnavailableError() : this(null, null) { }
            public ResourceTemporarilyUnavailableError(string message) : this(message, null) { }
            public ResourceTemporarilyUnavailableError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ResourceTemporarilyUnavailableError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ResourceTemporarilyUnavailableError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EINTR"), Serializable]
        public class InterruptedError : ExternalException {
            private const string/*!*/ M = "Interrupted function call";

            public InterruptedError() : this(null, null) { }
            public InterruptedError(string message) : this(message, null) { }
            public InterruptedError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public InterruptedError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected InterruptedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EDOM"), Serializable]
        public class DomainError : ExternalException {
            private const string/*!*/ M = "Domain error";

            public DomainError() : this(null, null) { }
            public DomainError(string message) : this(message, null) { }
            public DomainError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public DomainError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected DomainError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EINVAL", Extends = typeof(InvalidError), Inherits = typeof(ExternalException))]
        public class InvalidErrorOps {
            [RubyConstructor]
            public static InvalidError/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                InvalidError result = new InvalidError(RubyExceptions.MakeMessage(ref message, "Invalid Argument"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("ENOENT", Extends = typeof(FileNotFoundException), Inherits = typeof(ExternalException))]
        public class FileNotFoundExceptionOps {
            [RubyConstructor]
            public static FileNotFoundException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                FileNotFoundException result = new FileNotFoundException(RubyExceptions.MakeMessage(ref message, "No such file or directory"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("ENOTDIR", Extends = typeof(DirectoryNotFoundException), Inherits = typeof(ExternalException))]
        public class DirectoryNotFoundExceptionOps {
            [RubyConstructor]
            public static DirectoryNotFoundException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                DirectoryNotFoundException result = new DirectoryNotFoundException(RubyExceptions.MakeMessage(ref message, "Not a directory"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("EACCES", Extends=typeof(System.UnauthorizedAccessException), Inherits=typeof(ExternalException))]
        public class UnauthorizedAccessExceptionOps {
            [RubyConstructor]
            public static UnauthorizedAccessException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                UnauthorizedAccessException result = new UnauthorizedAccessException(RubyExceptions.MakeMessage(ref message, "Permission denied"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("ECHILD"), Serializable]
        public class ChildError : ExternalException {
            private const string/*!*/ M = "No child processes";

            public ChildError() : this(null, null) { }
            public ChildError(string message) : this(message, null) { }
            public ChildError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ChildError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ChildError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EEXIST", Extends=typeof(ExistError), Inherits=typeof(ExternalException))]
        public class ExistErrorOps {
            [RubyConstructor]
            public static ExistError/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                var result = new ExistError(RubyExceptions.MakeMessage(ref message, "File exists"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("EBADF", Extends = typeof(BadFileDescriptorError), Inherits = typeof(ExternalException))]
        public class BadFileDescriptorErrorOps {
            [RubyConstructor]
            public static BadFileDescriptorError/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                var result = new BadFileDescriptorError(RubyExceptions.MakeMessage(ref message, "Bad file descriptor"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("ENOEXEC", Extends = typeof(ExecFormatError), Inherits = typeof(ExternalException))]
        public class ExecFormatErrorOps {
            [RubyConstructor]
            public static BadFileDescriptorError/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                var result = new BadFileDescriptorError(RubyExceptions.MakeMessage(ref message, "Exec format error"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("EPIPE"), Serializable]
        public class PipeError : ExternalException {
            private const string/*!*/ M = "Broken pipe";

            public PipeError() : this(null, null) { }
            public PipeError(string message) : this(message, null) { }
            public PipeError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public PipeError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected PipeError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }


        [RubyClass("EXDEV"), Serializable]
        public class ImproperLinkError : ExternalException {
            private const string/*!*/ M = "Improper link";

            public ImproperLinkError() : this(null, null) { }
            public ImproperLinkError(string message) : this(message, null) { }
            public ImproperLinkError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ImproperLinkError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ImproperLinkError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ESPIPE"), Serializable]
        public class InvalidSeekError : ExternalException {
            private const string/*!*/ M = "Invalid seek";

            public InvalidSeekError() : this(null, null) { }
            public InvalidSeekError(string message) : this(message, null) { }
            public InvalidSeekError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public InvalidSeekError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected InvalidSeekError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        #region Socket Errors

        // TODO: generate 

        [RubyClass("EWOULDBLOCK"), Serializable]
        public class WouldBlockError : ExternalException {
            private const string/*!*/ M = "A non-blocking socket operation could not be completed immediately.";
            public override int ErrorCode { get { return 10035; } }

            public WouldBlockError() : this(null, null) { }
            public WouldBlockError(string message) : this(message, null) { }
            public WouldBlockError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public WouldBlockError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected WouldBlockError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EADDRINUSE"), Serializable]
        public class AddressInUseError : ExternalException {
            private const string/*!*/ M = "Only one usage of each socket address (protocol/network address/port) is normally permitted.";
            public override int ErrorCode { get { return 10048; } }

            public AddressInUseError() : this(null, null) { }
            public AddressInUseError(string message) : this(message, null) { }
            public AddressInUseError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public AddressInUseError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected AddressInUseError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNABORTED"), Serializable]
        public class ConnectionAbortedError : ExternalException {
            private const string/*!*/ M = "An established connection was aborted by the software in your host machine.";
            public override int ErrorCode { get { return 10053; } }

            public ConnectionAbortedError() : this(null, null) { }
            public ConnectionAbortedError(string message) : this(message, null) { }
            public ConnectionAbortedError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ConnectionAbortedError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionAbortedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNRESET"), Serializable]
        public class ConnectionResetError : ExternalException {
            private const string/*!*/ M = "An existing connection was forcibly closed by the remote host.";
            public override int ErrorCode { get { return 10054; } }

            public ConnectionResetError() : this(null, null) { }
            public ConnectionResetError(string message) : this(message, null) { }
            public ConnectionResetError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ConnectionResetError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionResetError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ENOTCONN"), Serializable]
        public class NotConnectedError : ExternalException {
            private const string/*!*/ M = "A request to send or receive data was disallowed because the socket is not connected and (when sending on a datagram socket using a sendto call) no address was supplied.";
            public override int ErrorCode { get { return 10057; } }

            public NotConnectedError() : this(null, null) { }
            public NotConnectedError(string message) : this(message, null) { }
            public NotConnectedError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public NotConnectedError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected NotConnectedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNREFUSED"), Serializable]
        public class ConnectionRefusedError : ExternalException {
            private const string/*!*/ M = "No connection could be made because the target machine actively refused it.";
            public override int ErrorCode { get { return 10061; } }

            public ConnectionRefusedError() : this(null, null) { }
            public ConnectionRefusedError(string message) : this(message, null) { }
            public ConnectionRefusedError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public ConnectionRefusedError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionRefusedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }
        
        [RubyClass("EHOSTDOWN"), Serializable]
        public class HostDownError : ExternalException {
            private const string/*!*/ M = "A socket operation failed because the destination host was down.";
            public override int ErrorCode { get { return 10064; } }

            public HostDownError() : this(null, null) { }
            public HostDownError(string message) : this(message, null) { }
            public HostDownError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public HostDownError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected HostDownError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        #endregion
    }
}
