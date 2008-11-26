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

#if !SILVERLIGHT

using System.Net;
using System.Net.Sockets;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("UDPSocket", BuildConfig = "!SILVERLIGHT")]
    public class UDPSocket : IPSocket {
        public UDPSocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }

        [RubyConstructor]
        public static UDPSocket/*!*/ CreateUDPSocket(RubyClass/*!*/ self) {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return new UDPSocket(self.Context, socket);
        }

        [RubyConstructor]
        public static UDPSocket/*!*/ CreateUDPSocket(RubyClass/*!*/ self, object family) {
            AddressFamily addressFamily = ConvertToAddressFamily(self.Context, family);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return new UDPSocket(self.Context, socket);
        }

        #region Public Instance Methods

        [RubyMethod("bind")]
        public static int Bind(RubyContext/*!*/ context, UDPSocket/*!*/ self, object hostname, object port) {
            int iPort = ConvertToPortNum(context, port);
            if (hostname == null) {
                hostname = MutableString.Create("localhost");
            }
            MutableString address = GetAddressInternal(context, hostname);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(address.ConvertToString()), iPort);
            self.Socket.Bind(ep);
            return 0;
        }

        [RubyMethod("connect")]
        public static int Connect(RubyContext/*!*/ context, UDPSocket/*!*/ self, object hostname, object port) {
            MutableString strHostname = ConvertToHostString(context, hostname);
            int iPort = ConvertToPortNum(context, port);
            self.Socket.Connect(strHostname.ConvertToString(), iPort);
            return 0;
        }

        [RubyMethod("recvfrom_nonblock")]
        public static RubyArray/*!*/ ReceiveFromNonBlocking(RubyContext/*!*/ context, IPSocket/*!*/ self, int length) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return ReceiveFrom(context, self, length, null);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("recvfrom_nonblock")]
        public static RubyArray/*!*/ ReceiveFromNonBlocking(RubyContext/*!*/ context, IPSocket/*!*/ self, int length, object/*Numeric*/ flags) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return ReceiveFrom(context, self, length, flags);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("send")]
        public static int Send(RubyContext/*!*/ context, RubyBasicSocket/*!*/ self, object message, object flags, object hostname, object port) {
            Protocols.CheckSafeLevel(context, 4, "send");
            // Convert the parameters
            SocketFlags sFlags = ConvertToSocketFlag(context, flags);
            MutableString strMessage = Protocols.CastToString(context, message);
            MutableString address = GetAddressInternal(context, hostname);
            int iPort = ConvertToPortNum(context, port);
            EndPoint toEndPoint = new IPEndPoint(IPAddress.Parse(address.ConvertToString()), iPort);
            return self.Socket.SendTo(strMessage.ConvertToBytes(), sFlags, toEndPoint);
        }

        // These overwritten methods have to be here because we kill the ones in RubyBasicSocket by creating the one above
        [RubyMethod("send")]
        public static new int Send(RubyContext/*!*/ context, RubyBasicSocket/*!*/ self, object message, object flags) {
            return RubyBasicSocket.Send(context, self, message, flags);
        }

        [RubyMethod("send")]
        public static new int Send(RubyContext/*!*/ context, RubyBasicSocket/*!*/ self, object message, object flags, object to) {
            return RubyBasicSocket.Send(context, self, message, flags, to);
        }

        #endregion
    }
}
#endif
