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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.IO;

namespace IronRuby.Builtins {
    [RubyModule("Errno")]
    public static class Errno {
        // Errno.constants.sort

        internal static FileNotFoundException/*!*/ CreateENOENT() {
            return new FileNotFoundException();
        }

        internal static FileNotFoundException/*!*/ CreateENOENT(string message, Exception inner) {
            return new FileNotFoundException(message, inner);
        }

        internal static FileNotFoundException/*!*/ CreateENOENT(string message) {
            return new FileNotFoundException(message);
        }

        internal static UnauthorizedAccessException/*!*/ CreateEACCES(string message, Exception inner) {
            return new UnauthorizedAccessException(message, inner);
        }

        internal static string/*!*/ MakeMessage(string message, string/*!*/ baseMessage) {
            Assert.NotNull(baseMessage);
            return (message != null) ? String.Concat(baseMessage, " - ", message) : baseMessage;
        }

        internal static string/*!*/ MakeMessage(ref MutableString message, string/*!*/ baseMessage) {
            Assert.NotNull(baseMessage);
            string result = MakeMessage(message != null ? message.ConvertToString() : null, baseMessage);
            message = MutableString.Create(result);
            return result;
        }

        [RubyClass("EADDRINUSE"), Serializable]
        public class AddressInUseError : ExternalException {
            private const string/*!*/ M = "Only one usage of each socket address (protocol/network address/port) is normally permitted.";

            public AddressInUseError() : this(null, null) { }
            public AddressInUseError(string message) : this(message, null) { }
            public AddressInUseError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public AddressInUseError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected AddressInUseError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EDOM"), Serializable]
        public class DomainError : ExternalException {
            private const string/*!*/ M = "Domain error";

            public DomainError() : this(null, null) { }
            public DomainError(string message) : this(message, null) { }
            public DomainError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public DomainError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected DomainError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EINVAL"), Serializable]
        public class InvalidError : ExternalException {
            private const string/*!*/ M = "Invalid argument";

            public InvalidError() : this(null, null) { }
            public InvalidError(string message) : this(message, null) { }
            public InvalidError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public InvalidError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected InvalidError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ENOENT", Extends = typeof(FileNotFoundException), Inherits = typeof(ExternalException))]
        public class FileNotFoundExceptionOps {
            [RubyConstructor]
            public static FileNotFoundException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                FileNotFoundException result = new FileNotFoundException(Errno.MakeMessage(ref message, "No such file or directory"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("ENOTDIR", Extends = typeof(DirectoryNotFoundException), Inherits = typeof(ExternalException))]
        public class DirectoryNotFoundExceptionOps {
            [RubyConstructor]
            public static DirectoryNotFoundException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                DirectoryNotFoundException result = new DirectoryNotFoundException(Errno.MakeMessage(ref message, "Not a directory"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("EACCES", Extends=typeof(System.UnauthorizedAccessException), Inherits=typeof(ExternalException))]
        public class UnauthorizedAccessExceptionOps {
            [RubyConstructor]
            public static UnauthorizedAccessException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                UnauthorizedAccessException result = new UnauthorizedAccessException(Errno.MakeMessage(ref message, "Permission denied"));
                RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyClass("EEXIST"), Serializable]
        public class ExistError : ExternalException {
            private const string/*!*/ M = "File exists";

            public ExistError() : this(null, null) { }
            public ExistError(string message) : this(message, null) { }
            public ExistError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public ExistError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ExistError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EBADF"), Serializable]
        public class BadFileDescriptorError : ExternalException {
            private const string/*!*/ M = "Bad file descriptor";

            public BadFileDescriptorError() : this(null, null) { }
            public BadFileDescriptorError(string message) : this(message, null) { }
            public BadFileDescriptorError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public BadFileDescriptorError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected BadFileDescriptorError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EPIPE"), Serializable]
        public class PipeError : ExternalException {
            private const string/*!*/ M = "Broken pipe";

            public PipeError() : this(null, null) { }
            public PipeError(string message) : this(message, null) { }
            public PipeError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public PipeError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected PipeError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ENOTCONN"), Serializable]
        public class NotConnectedError : ExternalException {
            private const string/*!*/ M = "A request to send or receive data was disallowed because the socket is not connected and (when sending on a datagram socket using a sendto call) no address was supplied.";

            public NotConnectedError() : this(null, null) { }
            public NotConnectedError(string message) : this(message, null) { }
            public NotConnectedError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public NotConnectedError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected NotConnectedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNREFUSED"), Serializable]
        public class ConnectionRefusedError : ExternalException {
            private const string/*!*/ M = "No connection could be made because the target machine actively refused it.";

            public ConnectionRefusedError() : this(null, null) { }
            public ConnectionRefusedError(string message) : this(message, null) { }
            public ConnectionRefusedError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public ConnectionRefusedError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionRefusedError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNRESET"), Serializable]
        public class ConnectionResetError : ExternalException {
            private const string/*!*/ M = "An existing connection was forcibly closed by the remote host.";

            public ConnectionResetError() : this(null, null) { }
            public ConnectionResetError(string message) : this(message, null) { }
            public ConnectionResetError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public ConnectionResetError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionResetError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("ECONNABORTED"), Serializable]
        public class ConnectionAbortError : ExternalException {
            private const string/*!*/ M = "An established connection was aborted by the software in your host machine.";

            public ConnectionAbortError() : this(null, null) { }
            public ConnectionAbortError(string message) : this(message, null) { }
            public ConnectionAbortError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public ConnectionAbortError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ConnectionAbortError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyClass("EXDEV"), Serializable]
        public class ImproperLinkError : ExternalException {
            private const string/*!*/ M = "Improper link";

            public ImproperLinkError() : this(null, null) { }
            public ImproperLinkError(string message) : this(message, null) { }
            public ImproperLinkError(string message, Exception inner) : base(MakeMessage(message, M), inner) { }
            public ImproperLinkError(MutableString message) : base(MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected ImproperLinkError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }
    }
}
