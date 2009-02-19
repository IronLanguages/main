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
        public static UDPSocket/*!*/ CreateUDPSocket(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, object family) {
            AddressFamily addressFamily = ConvertToAddressFamily(stringCast, fixnumCast, self.Context, family);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return new UDPSocket(self.Context, socket);
        }

        #region Public Instance Methods

        [RubyMethod("bind")]
        public static int Bind(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyContext/*!*/ context, UDPSocket/*!*/ self, object hostNameOrAddress, object port) {

            int iPort = ConvertToPortNum(stringCast, fixnumCast, context, port);
            IPAddress address = (hostNameOrAddress != null) ? 
                GetHostAddress(ConvertToHostString(stringCast, context, hostNameOrAddress)) : IPAddress.Loopback;

            IPEndPoint ep = new IPEndPoint(address, iPort);
            self.Socket.Bind(ep);
            return 0;
        }

        [RubyMethod("connect")]
        public static int Connect(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyContext/*!*/ context, UDPSocket/*!*/ self, object hostname, object port) {

            string strHostname = ConvertToHostString(stringCast, context, hostname);
            int iPort = ConvertToPortNum(stringCast, fixnumCast, context, port);
            self.Socket.Connect(strHostname, iPort);
            return 0;
        }

        [RubyMethod("recvfrom_nonblock")]
        public static RubyArray/*!*/ ReceiveFromNonBlocking(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, IPSocket/*!*/ self, int length) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return ReceiveFrom(fixnumCast, context, self, length, null);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("recvfrom_nonblock")]
        public static RubyArray/*!*/ ReceiveFromNonBlocking(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, IPSocket/*!*/ self, int length, object/*Numeric*/ flags) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return ReceiveFrom(fixnumCast, context, self, length, flags);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("send")]
        public static int Send(ConversionStorage<int>/*!*/ fixnumCast, ConversionStorage<MutableString>/*!*/ stringCast,
            RubyContext/*!*/ context, RubyBasicSocket/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ message,
            object flags, object hostNameOrAddress, object port) {
            
            Protocols.CheckSafeLevel(context, 4, "send");
            int iPort = ConvertToPortNum(stringCast, fixnumCast, context, port);
            SocketFlags sFlags = ConvertToSocketFlag(fixnumCast, context, flags);

            // Convert the parameters
            IPAddress address = (hostNameOrAddress != null) ?
                GetHostAddress(ConvertToHostString(stringCast, context, hostNameOrAddress)) : IPAddress.Loopback;

            EndPoint toEndPoint = new IPEndPoint(address, iPort);
            return self.Socket.SendTo(message.ConvertToBytes(), sFlags, toEndPoint);
        }

        // These overwritten methods have to be here because we kill the ones in RubyBasicSocket by creating the one above
        [RubyMethod("send")]
        public static new int Send(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, 
            RubyBasicSocket/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ message, object flags) {
            return RubyBasicSocket.Send(fixnumCast, context, self, message, flags);
        }

        [RubyMethod("send")]
        public static new int Send(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context,
            RubyBasicSocket/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ message, object flags, 
            [DefaultProtocol, NotNull]MutableString/*!*/ to) {

            return RubyBasicSocket.Send(fixnumCast, context, self, message, flags, to);
        }

        #endregion
    }
}
#endif
