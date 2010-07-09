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

#if !SILVERLIGHT

using System.Net.Sockets;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Runtime.InteropServices;
using System.Net;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("TCPSocket", BuildConfig = "!SILVERLIGHT")]
    public class TCPSocket : IPSocket {
        /// <summary>
        /// Creates an uninitialized socket.
        /// </summary>
        public TCPSocket(RubyContext/*!*/ context)
            : base(context) {
        }
        
        public TCPSocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }

        [RubyConstructor]
        public static TCPSocket/*!*/ CreateTCPSocket(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, [DefaultProtocol]MutableString remoteHost, object remotePort, [Optional]int localPort) {

            // Not sure what the semantics should be in this case but we make sure not to blow up.
            // Real-world code (Server.connect_to in memcache.rb in the memcache-client gem) does do "TCPSocket.new(host, port, 0)"
            if (localPort != 0) {
                throw new NotImplementedError();
            }

            return new TCPSocket(self.Context, CreateSocket(remoteHost, ConvertToPortNum(stringCast, fixnumCast, remotePort)));
        }

        [RubyConstructor]
        public static TCPSocket/*!*/ CreateTCPSocket(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, 
            [DefaultProtocol]MutableString remoteHost, object remotePort,
            [DefaultProtocol]MutableString localHost, object localPort) {

            return BindLocalEndPoint(
                CreateTCPSocket(stringCast, fixnumCast, self, remoteHost, remotePort, 0),
                localHost,
                ConvertToPortNum(stringCast, fixnumCast, localPort)
            );
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static TCPServer/*!*/ Reinitialize(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast,
            TCPServer/*!*/ self, [DefaultProtocol]MutableString remoteHost, object remotePort, [Optional]int localPort) {

            // Not sure what the semantics should be in this case but we make sure not to blow up.
            // Real-world code (Server.connect_to in memcache.rb in the memcache-client gem) does do "TCPSocket.new(host, port, 0)"
            if (localPort != 0) {
                throw new NotImplementedError();
            }

            self.Socket = CreateSocket(remoteHost, ConvertToPortNum(stringCast, fixnumCast, remotePort));
            return self;
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static TCPServer/*!*/ Reinitialize(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast,
            TCPServer/*!*/ self,
            [DefaultProtocol]MutableString remoteHost, object remotePort,
            [DefaultProtocol]MutableString localHost, object localPort) {

            self.Socket = CreateSocket(remoteHost, ConvertToPortNum(stringCast, fixnumCast, remotePort));
            BindLocalEndPoint(self, localHost, ConvertToPortNum(stringCast, fixnumCast, localPort));
            return self;
        }

        private static Socket/*!*/ CreateSocket(MutableString remoteHost, int port) {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                if (remoteHost != null) {
                    socket.Connect(remoteHost.ConvertToString(), port);
                } else {
                    socket.Connect(IPAddress.Loopback, port);
                }
            } catch (SocketException e) {
                switch (e.SocketErrorCode) {
                    case SocketError.ConnectionRefused:
                        throw new Errno.ConnectionRefusedError();
                    default:
                        throw;
                }
            }
            return socket;
        }

        private static TCPSocket/*!*/ BindLocalEndPoint(TCPSocket/*!*/ socket, MutableString localHost, int localPort) {
            IPAddress localIPAddress = localHost != null ? GetHostAddress(localHost.ConvertToString()) : IPAddress.Loopback;
            IPEndPoint localEndPoint = new IPEndPoint(localIPAddress, localPort);
            socket.Socket.Bind(localEndPoint);
            return socket;
        }

        [RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetHostByName(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, object hostNameOrAddress) {
            return GetHostByName(self.Context, ConvertToHostString(stringCast, hostNameOrAddress), false);
        }
    }
}
#endif
