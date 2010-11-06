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

using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("TCPServer", BuildConfig = "!SILVERLIGHT")]
    public class TCPServer : TCPSocket {
        private object _mutex = new object();
        private IAsyncResult _acceptResult;

        /// <summary>
        /// Creates an uninitialized socket.
        /// </summary>
        public TCPServer(RubyContext/*!*/ context)
            : base(context) {
        }

        public TCPServer(RubyContext/*!*/ context, Socket/*!*/ socket) 
            : base(context, socket) {
        }

        public override WaitHandle/*!*/ CreateReadWaitHandle() {
            return GetAcceptResult().AsyncWaitHandle;
        }

        private IAsyncResult/*!*/ GetAcceptResult() {
            if (_acceptResult == null) {
                lock (_mutex) {
                    if (_acceptResult == null) {
                        _acceptResult = Socket.BeginAccept(null, null);
                    }
                }
            }
            return _acceptResult;
        }

        private Socket/*!*/ Accept() {
            // acquire the result and replace it by null, so that no other thread can acquire it:
            IAsyncResult result = Interlocked.Exchange(ref _acceptResult, null);

            if (result == null) {
                ThreadOps.RubyThreadInfo info = ThreadOps.RubyThreadInfo.FromThread(Thread.CurrentThread);
                info.Blocked = true;
                try {
                    return Socket.Accept();
                } finally {
                    info.Blocked = false;
                }
            }

            // wait until accept finishes:
            return Socket.EndAccept(result);
        }

        [RubyConstructor]
        public static TCPServer/*!*/ CreateTCPServer(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, [DefaultProtocol]MutableString hostname, [DefaultParameterValue(null)]object port) {
            return new TCPServer(self.Context, CreateSocket(stringCast, fixnumCast, hostname, port));
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static TCPServer/*!*/ Reinitialize(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast,
            TCPServer/*!*/ self, [DefaultProtocol]MutableString hostname, [DefaultParameterValue(null)]object port) {
            self.Socket = CreateSocket(stringCast, fixnumCast, hostname, port);
            return self;
        }

        private static Socket CreateSocket(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast,
            [DefaultProtocol]MutableString hostname, [DefaultParameterValue(null)]object port) {

            IPAddress listeningInterface = null;
            if (hostname == null) {
                listeningInterface = new IPAddress(0);
            } else if (hostname.IsEmpty) {
                listeningInterface = IPAddress.Any;
            } else {
                string hostnameStr = hostname.ConvertToString();
                if (hostnameStr == IPAddress.Any.ToString()) {
                    listeningInterface = IPAddress.Any;
                } else if (hostnameStr == IPAddress.Loopback.ToString()) {
                    listeningInterface = IPAddress.Loopback;
                } else if (!IPAddress.TryParse(hostnameStr, out listeningInterface)) {

                    // look up the host IP from DNS
                    IPHostEntry hostEntry = Dns.GetHostEntry(hostnameStr);
                    foreach (IPAddress address in hostEntry.AddressList) {
                        if (address.AddressFamily == AddressFamily.InterNetwork) {
                            listeningInterface = address;
                            break;
                        }
                    }
                    if (listeningInterface == null) {
                        // TODO: do we need to support any other address family types?
                        // (presumably should support at least IPv6)
                        throw new NotImplementedException("TODO: non-inet addresses");
                    }
                }
                Assert.NotNull(listeningInterface);
            }

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                socket.Bind(new IPEndPoint(listeningInterface, ConvertToPortNum(stringCast, fixnumCast, port)));
                socket.Listen(10);            
            } catch (SocketException e) {
                switch (e.SocketErrorCode) {
                    case SocketError.AddressAlreadyInUse:
                        throw new Errno.AddressInUseError();
                    default: 
                        throw;
                }
            }
            return socket;
        }

        [RubyMethod("accept")]
        public static TCPSocket/*!*/ Accept(RubyContext/*!*/ context, TCPServer/*!*/ self) {
            return new TCPSocket(context, self.Accept());
        }

        [RubyMethod("accept_nonblock")]
        public static TCPSocket/*!*/ AcceptNonBlocking(RubyContext/*!*/ context, TCPServer/*!*/ self) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return Accept(context, self);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("sysaccept")]
        public static int SysAccept(RubyContext/*!*/ context, TCPServer/*!*/ self) {
            return Accept(context, self).GetFileDescriptor();
        }

        [RubyMethod("listen")]
        public static void Listen(TCPServer/*!*/ self, int backlog) {
            self.Socket.Listen(backlog);
        }
    }
}
#endif
