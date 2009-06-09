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

using System.Net.Sockets;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Runtime.InteropServices;
using System.Net;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("TCPSocket", BuildConfig = "!SILVERLIGHT")]
    public class TCPSocket : IPSocket {
        public TCPSocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }

        [RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetHostByName(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, object hostNameOrAddress) {
            return GetHostByName(ConvertToHostString(stringCast, hostNameOrAddress), false);
        }

        [RubyConstructor]
        public static TCPSocket/*!*/ CreateTCPSocket(
            ConversionStorage<MutableString>/*!*/ stringCast, 
            ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ remoteHost, 
            object remotePort) {

            int port = ConvertToPortNum(stringCast, fixnumCast, remotePort);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteHost.ConvertToString(), port);

            return new TCPSocket(self.Context, socket);
        }

        [RubyConstructor]
        public static TCPSocket/*!*/ CreateTCPSocket(
            ConversionStorage<MutableString>/*!*/ stringCast,
            ConversionStorage<int>/*!*/ fixnumCast,
            RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ remoteHost,
            object remotePort,
            int localPort) {

            if (localPort == 0) {
                // Not sure what the semantics should be in this case but we make sure not to blow up.
                // Real-world code (Server.connect_to in memcache.rb in the memcache-client gem) does do "TCPSocket.new(host, port, 0)"
                return CreateTCPSocket(stringCast, fixnumCast, self, remoteHost, remotePort);
            }

            throw new NotImplementedError();
        }

        [RubyConstructor]
        public static TCPSocket/*!*/ CreateTCPSocket(
            ConversionStorage<MutableString>/*!*/ stringCast,
            ConversionStorage<int>/*!*/ fixnumCast,
            RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ remoteHost,
            object remotePort,
            [DefaultProtocol, NotNull]MutableString/*!*/ localHost,
            [DefaultProtocol, NotNull]MutableString/*!*/ localPort) {

            TCPSocket/*!*/ socket = CreateTCPSocket(stringCast, fixnumCast, self, remoteHost, remotePort);

            IPAddress localIPAddress = GetHostAddress(localHost.ConvertToString());
            int localPortNum = ConvertToPortNum(stringCast, fixnumCast, localPort);
            IPEndPoint localEndPoint = new IPEndPoint(localIPAddress, localPortNum);
            socket.Socket.Bind(localEndPoint);

            return socket;
        }
    }
}
#endif
