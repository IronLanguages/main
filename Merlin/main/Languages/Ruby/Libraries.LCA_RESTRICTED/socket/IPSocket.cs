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
using System.Runtime.InteropServices;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("IPSocket", BuildConfig = "!SILVERLIGHT")]
    public abstract class IPSocket : RubyBasicSocket {

        public IPSocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }
        
        [RubyMethod("getaddress", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetAddress(RubyClass/*!*/ self, object hostname) {
            return GetAddressInternal(self.Context, hostname);
        }

        #region Public Instance Methods

        [RubyMethod("addr")]
        public static RubyArray/*!*/ GetLocalAddress(RubyContext/*!*/ context, IPSocket/*!*/ self) {
            return GetAddressArray(context, self.Socket.LocalEndPoint);
        }

        [RubyMethod("peeraddr")]
        public static object/*!*/ GetPeerAddress(RubyContext/*!*/ context, IPSocket/*!*/ self) {
            return GetAddressArray(context, self.Socket.RemoteEndPoint);
        }

        [RubyMethod("recvfrom")]
        public static RubyArray/*!*/ ReceiveFrom(ConversionStorage<int>/*!*/ conversionStorage, RubyContext/*!*/ context, IPSocket/*!*/ self, 
            int length, [DefaultParameterValue(null)]object/*Numeric*/ flags) {

            SocketFlags sFlags = ConvertToSocketFlag(conversionStorage, context, flags);
            byte[] buffer = new byte[length];
            EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);
            int received = self.Socket.ReceiveFrom(buffer, sFlags, ref fromEP);
            MutableString str = MutableString.CreateBinary();
            str.Append(buffer, 0, received);
            context.SetObjectTaint(str, true);
            return RubyOps.MakeArray2(str, GetAddressArray(context, fromEP));
        }
        #endregion
    }
}
#endif
