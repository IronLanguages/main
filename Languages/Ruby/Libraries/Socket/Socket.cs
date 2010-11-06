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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime;
using System.IO;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("Socket", BuildConfig = "!SILVERLIGHT")]
    [Includes(typeof(SocketConstants), Copy = true)]
    public class RubySocket : RubyBasicSocket {

        #region Construction

        public RubySocket(RubyContext/*!*/ context, Socket/*!*/ socket)
            : base(context, socket) {
        }

        [RubyConstructor]
        public static RubySocket/*!*/ CreateSocket(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, [NotNull]object/*!*/ domain, [DefaultProtocol]int/*!*/ type, [DefaultProtocol]int protocol) {

            AddressFamily addressFamily = ConvertToAddressFamily(stringCast, fixnumCast, domain);
            return new RubySocket(self.Context, new Socket(addressFamily, (SocketType)type, (ProtocolType)protocol));
        }

        #endregion

        #region Public Singleton Methods

        [RubyMethod("getaddrinfo", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray GetAddressInfo(
            ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, object hostNameOrAddress, object port,
            [DefaultParameterValue(null)]object family,
            [DefaultParameterValue(0)]object socktype,
            [DefaultParameterValue(0)]object protocol,
            [DefaultParameterValue(null)]object flags) {

            RubyContext context = self.Context;

            IPHostEntry entry = (hostNameOrAddress != null) ?
                GetHostEntry(ConvertToHostString(stringCast, hostNameOrAddress), DoNotReverseLookup(context).Value) : 
                MakeEntry(IPAddress.Any, DoNotReverseLookup(context).Value);

            int iPort = ConvertToPortNum(stringCast, fixnumCast, port);

            // TODO: ignore family, the only supported families are InterNetwork and InterNetworkV6
            ConvertToAddressFamily(stringCast, fixnumCast, family);
            int socketType = Protocols.CastToFixnum(fixnumCast, socktype);
            int protocolType = Protocols.CastToFixnum(fixnumCast, protocol);

            RubyArray results = new RubyArray(entry.AddressList.Length);
            for (int i = 0; i < entry.AddressList.Length; ++i) {
                IPAddress address = entry.AddressList[i];

                RubyArray result = new RubyArray(9);
                result.Add(ToAddressFamilyString(address.AddressFamily));
                result.Add(iPort);
                result.Add(HostNameToMutableString(context, IPAddressToHostName(address, DoNotReverseLookup(context).Value)));
                result.Add(MutableString.CreateAscii(address.ToString()));
                result.Add((int)address.AddressFamily);
                result.Add(socketType);

                // TODO: protocol type:
                result.Add(protocolType);

                results.Add(result);
            }
            return results;
        }

        [RubyMethod("gethostbyaddr", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray GetHostByAddress(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ address, [DefaultParameterValue(null)]object type) {

            // TODO: ignore family, the only supported families are InterNetwork and InterNetworkV6
            ConvertToAddressFamily(stringCast, fixnumCast, type);
            IPHostEntry entry = GetHostEntry(new IPAddress(address.ConvertToBytes()), DoNotReverseLookup(self.Context).Value);

            return CreateHostEntryArray(self.Context, entry, true);
        }

        [RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetHostByName(RubyClass/*!*/ self, int address) {
            return GetHostByName(self.Context, ConvertToHostString(address), true);
        }

        [RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetHostByName(RubyClass/*!*/ self, [NotNull]BigInteger/*!*/ address) {
            return GetHostByName(self.Context, ConvertToHostString(address), true);
        }

        [RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetHostByName(RubyClass/*!*/ self, [DefaultProtocol]MutableString name) {
            return GetHostByName(self.Context, ConvertToHostString(name), true);
        }

        [RubyMethod("gethostname", RubyMethodAttributes.PublicSingleton)]
        public static MutableString GetHostname(RubyClass/*!*/ self) {
            return HostNameToMutableString(self.Context, Dns.GetHostName());
        }

        private static readonly MutableString/*!*/ _DefaultProtocol = MutableString.CreateAscii("tcp").Freeze();

        [RubyMethod("getservbyname", RubyMethodAttributes.PublicSingleton)]
        public static int GetServiceByName(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ name, 
            [DefaultProtocol, Optional]MutableString protocol) {
            
            if (protocol == null) {
                protocol = _DefaultProtocol;
            }

            ServiceName service = SearchForService(name, protocol);
            if (service != null) {
                return service.Port;
            } 

            // Cannot use: object port = Protocols.TryConvertToInteger(context, name);
            // Since the conversion process returns 0 if the string is not a valid number
            try {
                return ParseInteger(self.Context, name.ConvertToString());
            } catch (InvalidOperationException) {
                throw SocketErrorOps.Create(MutableString.FormatMessage("no such service {0} {1}", name, protocol));
            }
        }

        /// <summary>
        /// Returns a pair of connected sockets
        /// [Not Implemented]
        /// </summary>
        [RubyMethod("socketpair", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("pair", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ CreateSocketPair(RubyClass/*!*/ self, object domain, object type, object protocol) {
            throw new NotImplementedError();
        }

        [RubyMethod("getnameinfo", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetNameInfo(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, [NotNull]RubyArray/*!*/ hostInfo, [Optional]object flags) {
            if (hostInfo.Count < 3 || hostInfo.Count > 4) {
                throw RubyExceptions.CreateArgumentError("First parameter must be a 3 or 4 element array");
            }

            RubyContext context = self.Context;

            // We only support AF_INET (IP V4) family
            AddressFamily addressFamily = ConvertToAddressFamily(stringCast, fixnumCast, hostInfo[0]);
            if (addressFamily != AddressFamily.InterNetwork) {
                throw new SocketException((int)SocketError.AddressFamilyNotSupported);
            }

            // Lookup the service name for the given port.
            int port = ConvertToPortNum(stringCast, fixnumCast, hostInfo[1]);
            ServiceName service = SearchForService(port);

            // hostInfo[2] should have a host name
            // if it exists and is not null hostInfo[3] should have an IP address
            // in that case we use that rather than the host name.
            object hostName =  (hostInfo.Count > 3 && hostInfo[3] != null) ? hostInfo[3] : hostInfo[2];
            IPHostEntry entry = GetHostEntry(ConvertToHostString(stringCast, hostName), false);

            RubyArray result = new RubyArray(2);
            result.Add(HostNameToMutableString(context, entry.HostName));
            if (service != null) {
                result.Add(MutableString.Create(service.Name));
            } else {
                result.Add(port);
            }
            return result;
        }

        [RubyMethod("getnameinfo", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetNameInfo(RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ address, [Optional]object flags) {

            IPEndPoint ep = UnpackSockAddr(address);
            IPHostEntry entry = GetHostEntry(ep.Address, false);
            ServiceName service = SearchForService(ep.Port);

            RubyArray result = new RubyArray(2);
            result.Add(HostNameToMutableString(self.Context, entry.HostName));
            if (service != null) {
                result.Add(MutableString.Create(service.Name));
            } else {
                result.Add(ep.Port);
            }
            return result;
        }

        /// <summary>
        /// Returns the system dependent sockaddr structure packed into a string
        /// </summary>
        [RubyMethod("sockaddr_in", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("pack_sockaddr_in", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ PackInetSockAddr(ConversionStorage<MutableString>/*!*/ stringCast, ConversionStorage<int>/*!*/ fixnumCast, 
            RubyClass/*!*/ self, object port, object hostNameOrAddress) {
            int iPort = ConvertToPortNum(stringCast, fixnumCast,port);

            IPAddress address = (hostNameOrAddress != null) ?
                GetHostAddress(ConvertToHostString(stringCast, hostNameOrAddress)) : IPAddress.Loopback;

            SocketAddress socketAddress = new IPEndPoint(address, iPort).Serialize();
            var result = MutableString.CreateBinary(socketAddress.Size);
            for (int i = 0; i < socketAddress.Size; i++) {
                result.Append(socketAddress[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns the system dependent sockaddr structure packed into a string
        /// </summary>
        [RubyMethod("unpack_sockaddr_in", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ UnPackInetSockAddr(RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ address) {

            IPEndPoint ep = UnpackSockAddr(address);
            RubyArray result = new RubyArray(2);
            result.Add(ep.Port);
            result.Add(MutableString.CreateAscii(ep.Address.ToString()));
            return result;
        }

        internal static IPEndPoint/*!*/ UnpackSockAddr(MutableString/*!*/ stringAddress) {
            byte[] bytes = stringAddress.ConvertToBytes();
            SocketAddress addr = new SocketAddress(AddressFamily.InterNetwork, bytes.Length);
            for (int i = 0; i < bytes.Length; ++i) {
                addr[i] = bytes[i];
            }
            IPEndPoint ep = new IPEndPoint(0, 0);
            return (IPEndPoint)ep.Create(addr);
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("accept")]
        public static RubyArray/*!*/ Accept(RubyContext/*!*/ context, RubySocket/*!*/ self) {
            RubyArray result = new RubyArray(2);
            RubySocket s = new RubySocket(context, self.Socket.Accept());
            result.Add(s);
            SocketAddress addr = s.Socket.RemoteEndPoint.Serialize();
            result.Add(MutableString.CreateAscii(addr.ToString()));
            return result;
        }

        [RubyMethod("accept_nonblock")]
        public static RubyArray/*!*/ AcceptNonBlocking(RubyContext/*!*/ context, RubySocket/*!*/ self) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return Accept(context, self);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("bind")]
        public static int Bind(RubyContext/*!*/ context, RubySocket/*!*/ self, MutableString sockaddr) {
            IPEndPoint ep = UnpackSockAddr(sockaddr);
            self.Socket.Bind(ep);
            return 0;
        }

        [RubyMethod("connect")]
        public static int Connect(RubyContext/*!*/ context, RubySocket/*!*/ self, MutableString sockaddr) {
            IPEndPoint ep = UnpackSockAddr(sockaddr);
            self.Socket.Connect(ep);
            return 0;
        }

        [RubyMethod("connect_nonblock")]
        public static int ConnectNonBlocking(RubyContext/*!*/ context, RubySocket/*!*/ self, MutableString sockaddr) {
            bool blocking = self.Socket.Blocking;
            try {
                self.Socket.Blocking = false;
                return Connect(context, self, sockaddr);
            } finally {
                // Reset the blocking
                self.Socket.Blocking = blocking;
            }
        }

        [RubyMethod("listen")]
        public static int Listen(RubyContext/*!*/ context, RubySocket/*!*/ self, int backlog) {
            self.Socket.Listen(backlog);
            return 0;
        }

        [RubyMethod("recvfrom")]
        public static RubyArray/*!*/ ReceiveFrom(ConversionStorage<int>/*!*/ fixnumCast, RubySocket/*!*/ self, int length) {
            return ReceiveFrom(fixnumCast, self, length, null);
        }

        [RubyMethod("recvfrom")]
        public static RubyArray/*!*/ ReceiveFrom(ConversionStorage<int>/*!*/ fixnumCast, RubySocket/*!*/ self, 
            int length, object/*Numeric*/ flags) {
            SocketFlags sFlags = ConvertToSocketFlag(fixnumCast, flags);
            byte[] buffer = new byte[length];
            EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);
            int received = self.Socket.ReceiveFrom(buffer, sFlags, ref fromEP);
            MutableString str = MutableString.CreateBinary();
            str.Append(buffer, 0, received);
            str.IsTainted = true;
            return RubyOps.MakeArray2(str, self.GetAddressArray(fromEP));
        }


        [RubyMethod("sysaccept")]
        public static RubyArray/*!*/ SysAccept(RubyContext/*!*/ context, RubySocket/*!*/ self) {
            RubyArray result = new RubyArray(2);
            // TODO: Do we need some kind of strong reference to the socket
            // here to stop the RubySocket from being garbage collected?
            RubySocket s = new RubySocket(context, self.Socket.Accept());
            result.Add(s.GetFileDescriptor());
            SocketAddress addr = s.Socket.RemoteEndPoint.Serialize();
            result.Add(MutableString.CreateAscii(addr.ToString()));
            return result;
        }

        #endregion

        #region Constants

        [RubyModule("Constants", BuildConfig ="!SILVERLIGHT")]
        public class SocketConstants {
            #region Address Family

            [RubyConstant]
            public const int AF_APPLETALK = (int)AddressFamily.AppleTalk;
            [RubyConstant]
            public const int AF_ATM = (int)AddressFamily.Atm;
            [RubyConstant]
            public const int AF_CCITT = (int)AddressFamily.Ccitt;
            [RubyConstant]
            public const int AF_CHAOS = (int)AddressFamily.Chaos;
            [RubyConstant]
            public const int AF_DATAKIT = (int)AddressFamily.DataKit;
            [RubyConstant]
            public const int AF_DLI = (int)AddressFamily.DataLink;
            [RubyConstant]
            public const int AF_ECMA = (int)AddressFamily.Ecma;
            [RubyConstant]
            public const int AF_HYLINK = (int)AddressFamily.HyperChannel;
            [RubyConstant]
            public const int AF_IMPLINK = (int)AddressFamily.ImpLink;
            [RubyConstant]
            public const int AF_INET = (int)AddressFamily.InterNetwork;
            [RubyConstant]
            public const int AF_INET6 = (int)AddressFamily.InterNetworkV6;
            [RubyConstant]
            public const int AF_IPX = (int)AddressFamily.Ipx;
            [RubyConstant]
            public const int AF_ISO = (int)AddressFamily.Iso;
            [RubyConstant]
            public const int AF_LAT = (int)AddressFamily.Lat;
            [RubyConstant]
            public const int AF_MAX = (int)AddressFamily.Max;
            [RubyConstant]
            public const int AF_NETBIOS = (int)AddressFamily.NetBios;
            [RubyConstant]
            public const int AF_NS = (int)AddressFamily.NS;
            [RubyConstant]
            public const int AF_OSI = (int)AddressFamily.Osi;
            [RubyConstant]
            public const int AF_PUP = (int)AddressFamily.Pup;
            [RubyConstant]
            public const int AF_SNA = (int)AddressFamily.Sna;
            [RubyConstant]
            public const int AF_UNIX = (int)AddressFamily.Unix;
            [RubyConstant]
            public const int AF_UNSPEC = (int)AddressFamily.Unspecified;

            #endregion

            #region Flag Options for GetAddressInfo

            [RubyConstant]
            public const int AI_PASSIVE = 1;
            [RubyConstant]
            public const int AI_CANONNAME = 2;
            [RubyConstant]
            public const int AI_NUMERICHOST = 4;

            #endregion

            #region Error Return Codes from GetAddressInfo

            [RubyConstant]
            public const int EAI_AGAIN = 2;
            [RubyConstant]
            public const int EAI_BADFLAGS = 3;
            [RubyConstant]
            public const int EAI_FAIL = 4;
            [RubyConstant]
            public const int EAI_FAMILY = 5;
            [RubyConstant]
            public const int EAI_MEMORY = 6;
            [RubyConstant]
            public const int EAI_NODATA = 7;
            [RubyConstant]
            public const int EAI_NONAME = 8;
            [RubyConstant]
            public const int EAI_SERVICE = 9;
            [RubyConstant]
            public const int EAI_SOCKTYPE = 10;

            #endregion

            #region Addresses

            [RubyConstant]
            public const int IPPORT_RESERVED = 1024;
            [RubyConstant]
            public const int IPPORT_USERRESERVED = 5000;

            [RubyConstant]
            public const int INET_ADDRSTRLEN = 16;
            [RubyConstant]
            public const int INET6_ADDRSTRLEN = 46;

            [RubyConstant]
            public const uint INADDR_ALLHOSTS_GROUP = 0xe0000001;
            [RubyConstant]
            public const int INADDR_ANY = 0;
            [RubyConstant]
            public const uint INADDR_BROADCAST = 0xffffffff;
            [RubyConstant]
            public const int INADDR_LOOPBACK = 0x7F000001;
            [RubyConstant]
            public const uint INADDR_MAX_LOCAL_GROUP = 0xe00000ff;
            [RubyConstant]
            public const uint INADDR_NONE = 0xffffffff;
            [RubyConstant]
            public const uint INADDR_UNSPEC_GROUP = 0xe0000000;

            #endregion

            #region IP Protocol Constants

            [RubyConstant]
            public const int IP_DEFAULT_MULTICAST_TTL = 1;
            [RubyConstant]
            public const int IP_DEFAULT_MULTICAST_LOOP = 1;

            [RubyConstant]
            public const int IP_OPTIONS = 1;
            [RubyConstant]
            public const int IP_HDRINCL = 2;
            [RubyConstant]
            public const int IP_TOS = 3;
            [RubyConstant]
            public const int IP_TTL = 4;
            [RubyConstant]
            public const int IP_MULTICAST_IF = 9;
            [RubyConstant]
            public const int IP_MULTICAST_TTL = 10;
            [RubyConstant]
            public const int IP_MULTICAST_LOOP = 11;
            [RubyConstant]
            public const int IP_ADD_MEMBERSHIP = 12;
            [RubyConstant]
            public const int IP_DROP_MEMBERSHIP = 13;
            [RubyConstant]
            public const int IP_ADD_SOURCE_MEMBERSHIP = 15;
            [RubyConstant]
            public const int IP_DROP_SOURCE_MEMBERSHIP = 16;
            [RubyConstant]
            public const int IP_BLOCK_SOURCE = 17;
            [RubyConstant]
            public const int IP_UNBLOCK_SOURCE = 18;
            [RubyConstant]
            public const int IP_PKTINFO = 19;
            [RubyConstant]
            public const int IP_MAX_MEMBERSHIPS = 20;

            [RubyConstant]
            public const int IPPROTO_GGP = 3;
            [RubyConstant]
            public const int IPPROTO_ICMP = 1;
            [RubyConstant]
            public const int IPPROTO_IDP = 22;
            [RubyConstant]
            public const int IPPROTO_IGMP = 2;
            [RubyConstant]
            public const int IPPROTO_IP = 0;
            [RubyConstant]
            public const int IPPROTO_MAX = 256;
            [RubyConstant]
            public const int IPPROTO_ND = 77;
            [RubyConstant]
            public const int IPPROTO_PUP = 12;
            [RubyConstant]
            public const int IPPROTO_RAW = 255;
            [RubyConstant]
            public const int IPPROTO_TCP = 6;
            [RubyConstant]
            public const int IPPROTO_UDP = 17;
            [RubyConstant]
            public const int IPPROTO_AH = 51;
            [RubyConstant]
            public const int IPPROTO_DSTOPTS = 60;
            [RubyConstant]
            public const int IPPROTO_ESP = 50;
            [RubyConstant]
            public const int IPPROTO_FRAGMENT = 44;
            [RubyConstant]
            public const int IPPROTO_HOPOPTS = 0;
            [RubyConstant]
            public const int IPPROTO_ICMPV6 = 58;
            [RubyConstant]
            public const int IPPROTO_IPV6 = 41;
            [RubyConstant]
            public const int IPPROTO_NONE = 59;
            [RubyConstant]
            public const int IPPROTO_ROUTING = 43;

            [RubyConstant]
            public const int IPV6_JOIN_GROUP = 12;
            [RubyConstant]
            public const int IPV6_LEAVE_GROUP = 13;
            [RubyConstant]
            public const int IPV6_MULTICAST_HOPS = 10;
            [RubyConstant]
            public const int IPV6_MULTICAST_IF = 9;
            [RubyConstant]
            public const int IPV6_MULTICAST_LOOP = 11;
            [RubyConstant]
            public const int IPV6_UNICAST_HOPS = 4;
            [RubyConstant]
            public const int IPV6_PKTINFO = 19;



            #endregion

            #region Message Options
            [RubyConstant]
            public const int MSG_DONTROUTE = 4;
            [RubyConstant]
            public const int MSG_OOB = 1;
            [RubyConstant]
            public const int MSG_PEEK = 2;
            [RubyConstant]
            #endregion

            #region Name Info
            public const int NI_DGRAM = 16;
            [RubyConstant]
            public const int NI_MAXHOST = 1025;
            [RubyConstant]
            public const int NI_MAXSERV = 32;
            [RubyConstant]
            public const int NI_NAMEREQD = 4;
            [RubyConstant]
            public const int NI_NOFQDN = 1;
            [RubyConstant]
            public const int NI_NUMERICHOST = 2;
            [RubyConstant]
            public const int NI_NUMERICSERV = 8;
            #endregion

            #region Protocol Family

            [RubyConstant]
            public const int PF_APPLETALK = (int)ProtocolFamily.AppleTalk;
            [RubyConstant]
            public const int PF_ATM = (int)ProtocolFamily.Atm;
            [RubyConstant]
            public const int PF_CCITT = (int)ProtocolFamily.Ccitt;
            [RubyConstant]
            public const int PF_CHAOS = (int)ProtocolFamily.Chaos;
            [RubyConstant]
            public const int PF_DATAKIT = (int)ProtocolFamily.DataKit;
            [RubyConstant]
            public const int PF_DLI = (int)ProtocolFamily.DataLink;
            [RubyConstant]
            public const int PF_ECMA = (int)ProtocolFamily.Ecma;
            [RubyConstant]
            public const int PF_HYLINK = (int)ProtocolFamily.HyperChannel;
            [RubyConstant]
            public const int PF_IMPLINK = (int)ProtocolFamily.ImpLink;
            [RubyConstant]
            public const int PF_INET = (int)ProtocolFamily.InterNetwork;
            [RubyConstant]
            public const int PF_INET6 = (int)ProtocolFamily.InterNetworkV6;
            [RubyConstant]
            public const int PF_IPX = (int)ProtocolFamily.Ipx;
            [RubyConstant]
            public const int PF_ISO = (int)ProtocolFamily.Iso;
            [RubyConstant]
            public const int PF_LAT = (int)ProtocolFamily.Lat;
            [RubyConstant]
            public const int PF_MAX = (int)ProtocolFamily.Max;
            [RubyConstant]
            public const int PF_NS = (int)ProtocolFamily.NS;
            [RubyConstant]
            public const int PF_OSI = (int)ProtocolFamily.Osi;
            [RubyConstant]
            public const int PF_PUP = (int)ProtocolFamily.Pup;
            [RubyConstant]
            public const int PF_SNA = (int)ProtocolFamily.Sna;
            [RubyConstant]
            public const int PF_UNIX = (int)ProtocolFamily.Unix;
            [RubyConstant]
            public const int PF_UNSPEC = (int)ProtocolFamily.Unspecified;

            #endregion

            #region Socket Shutdown

            [RubyConstant]
            public const int SHUT_RD = (int)SocketShutdown.Receive;
            [RubyConstant]
            public const int SHUT_RDWR = (int)SocketShutdown.Both;
            [RubyConstant]
            public const int SHUT_WR = (int)SocketShutdown.Send;

            #endregion

            #region Socket Type

            [RubyConstant]
            public const int SOCK_DGRAM = (int)SocketType.Dgram;
            [RubyConstant]
            public const int SOCK_RAW = (int)SocketType.Raw;
            [RubyConstant]
            public const int SOCK_RDM = (int)SocketType.Rdm;
            [RubyConstant]
            public const int SOCK_SEQPACKET = (int)SocketType.Seqpacket;
            [RubyConstant]
            public const int SOCK_STREAM = (int)SocketType.Stream;

            #endregion

            #region Socket Option

            [RubyConstant]
            public const int SO_ACCEPTCONN = (int)SocketOptionName.AcceptConnection;
            [RubyConstant]
            public const int SO_BROADCAST = (int)SocketOptionName.Broadcast;
            [RubyConstant]
            public const int SO_DEBUG = (int)SocketOptionName.Debug;
            [RubyConstant]
            public const int SO_DONTROUTE = (int)SocketOptionName.DontRoute;
            [RubyConstant]
            public const int SO_ERROR = (int)SocketOptionName.Error;
            [RubyConstant]
            public const int SO_KEEPALIVE = (int)SocketOptionName.KeepAlive;
            [RubyConstant]
            public const int SO_LINGER = (int)SocketOptionName.Linger;
            [RubyConstant]
            public const int SO_OOBINLINE = (int)SocketOptionName.OutOfBandInline;
            [RubyConstant]
            public const int SO_RCVBUF = (int)SocketOptionName.ReceiveBuffer;
            [RubyConstant]
            public const int SO_RCVLOWAT = (int)SocketOptionName.ReceiveLowWater;
            [RubyConstant]
            public const int SO_RCVTIMEO = (int)SocketOptionName.ReceiveTimeout;
            [RubyConstant]
            public const int SO_REUSEADDR = (int)SocketOptionName.ReuseAddress;
            [RubyConstant]
            public const int SO_SNDBUF = (int)SocketOptionName.SendBuffer;
            [RubyConstant]
            public const int SO_SNDLOWAT = (int)SocketOptionName.SendLowWater;
            [RubyConstant]
            public const int SO_SNDTIMEO = (int)SocketOptionName.SendTimeout;
            [RubyConstant]
            public const int SO_TYPE = (int)SocketOptionName.Type;
            [RubyConstant]
            public const int SO_USELOOPBACK = (int)SocketOptionName.UseLoopback;

            #endregion

            [RubyConstant]
            public const int SOL_SOCKET = 65535;

            [RubyConstant]
            public const int SOMAXCONN = Int32.MaxValue;

            [RubyConstant]
            public const int TCP_NODELAY = 1;
        }

        #endregion

        #region Private Helpers

        private static int ParseInteger(RubyContext/*!*/ context, string/*!*/ str) {
            bool isNegative = false;
            if (str[0] == '-') {
                isNegative = true;
                str = str.Remove(0, 1);
            }

            Tokenizer tokenizer = new Tokenizer();
            tokenizer.Initialize(new StringReader(str));
            Tokens token = tokenizer.GetNextToken();
            TokenValue value = tokenizer.TokenValue;
            Tokens nextToken = tokenizer.GetNextToken();

            // We are only interested in the whole string being a valid Integer
            if (token == Tokens.Integer && nextToken == Tokens.Integer) {
                return isNegative ? -value.Integer1 : value.Integer1;
            } else {
                throw RubyExceptions.CreateTypeConversionError("String", "Integer");
            }
        }

        #endregion
    }
}
#endif
