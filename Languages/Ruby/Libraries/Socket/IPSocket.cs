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

using System.Net;
using System.Net.Sockets;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("IPSocket", BuildConfig = "!SILVERLIGHT")]
    public abstract class IPSocket : RubyBasicSocket {

        /// <summary>
        /// Creates an uninitialized socket.
        /// </summary>
        protected IPSocket(RubyContext/*!*/ context)
            : base(context) {
        }

        protected IPSocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }
        
        [RubyMethod("getaddress", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetAddress(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, object hostNameOrAddress) {
            return MutableString.CreateAscii(GetHostAddress(ConvertToHostString(stringCast, hostNameOrAddress)).ToString());
        }

        #region Public Instance Methods

        [RubyMethod("addr")]
        public static RubyArray/*!*/ GetLocalAddress(RubyContext/*!*/ context, IPSocket/*!*/ self) {
            return self.GetAddressArray(self.Socket.LocalEndPoint);
        }

        [RubyMethod("peeraddr")]
        public static object/*!*/ GetPeerAddress(RubyContext/*!*/ context, IPSocket/*!*/ self) {
            return self.GetAddressArray(self.Socket.RemoteEndPoint);
        }

        [RubyMethod("recvfrom")]
        public static RubyArray/*!*/ ReceiveFrom(ConversionStorage<int>/*!*/ conversionStorage, IPSocket/*!*/ self, 
            int length, [DefaultParameterValue(null)]object/*Numeric*/ flags) {

            SocketFlags sFlags = ConvertToSocketFlag(conversionStorage, flags);
            byte[] buffer = new byte[length];
            EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);
            int received = self.Socket.ReceiveFrom(buffer, sFlags, ref fromEP);
            MutableString str = MutableString.CreateBinary();
            str.Append(buffer, 0, received);

            str.IsTainted = true;
            return RubyOps.MakeArray2(str, self.GetAddressArray(fromEP));
        }
        #endregion
    }
}
#endif
