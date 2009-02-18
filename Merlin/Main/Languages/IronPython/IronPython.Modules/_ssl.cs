/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT // System.NET

using System;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using System.Security.Cryptography.X509Certificates;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using System.Runtime.CompilerServices;

/* [assembly: PythonModule("_ssl", typeof(IronPython.Modules.PythonSsl))] */
namespace IronPython.Modules {
    public static partial class PythonSocket { // PythonSsl
        // public const string __doc__ = "Implementation module for SSL socket operations.";

        #region Stubs for RAND functions

        // The RAND_ functions are effectively no-ops, as the BCL draws on system sources
        // for cryptographically-strong randomness and doesn't need (or accept) user input

        public static void RAND_add(string buf, double entropy) {
        }

        public static void RAND_egd(string source) {
        }

        public static int RAND_status() {
            return 1; // always ready
        }

        #endregion

        public static PythonType SSLType = DynamicHelpers.GetPythonTypeFromType(typeof(ssl));

        public class ssl {
            private readonly SslStream _sslStream;

            internal bool CertValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                return true; // Mimics behavior of CPython 2.5 SSL 
            }

            public ssl(PythonSocket.socket sock, [DefaultParameterValue(null)] string keyfile, [DefaultParameterValue(null)] string certfile) {
                _sslStream = new SslStream(new NetworkStream(sock._socket, false), true, CertValidationCallback);
                _sslStream.AuthenticateAsClient(sock._hostName);
            }

            [Documentation("issuer() -> issuer_certificate\n\n"
                + "Returns a string that describes the issuer of the server's certificate. Only useful for debugging purposes."
                )]
            public string issuer() {
                X509Certificate remoteCertificate = _sslStream.RemoteCertificate;
                if (remoteCertificate != null) {
                    return remoteCertificate.Issuer;
                } else {
                    return String.Empty;
                }
            }

            [Documentation("read([n]) -> buffer_read\n\n"
                + "If n is present, reads up to n bytes from the SSL connection. Otherwise, reads to EOF."
                )]
            public string read(CodeContext/*!*/ context, [DefaultParameterValue(Int32.MaxValue)] int n) {
                try {
                    byte[] buffer = new byte[2048];
                    MemoryStream result = new MemoryStream(n);
                    while (true) {
                        int readLength = (n < buffer.Length) ? n : buffer.Length;
                        int bytes = _sslStream.Read(buffer, 0, readLength);
                        if (bytes > 0) {
                            result.Write(buffer, 0, bytes);
                            n -= bytes;
                        }
                        if (bytes == 0 || n == 0) {
                            return result.ToArray().MakeString();
                        }
                    }
                } catch (Exception e) {
                    throw PythonSocket.MakeException(context, e);
                }
            }

            [Documentation("server() -> server_certificate\n\n"
                + "Returns a string that describes the server's certificate. Only useful for debugging purposes."
                )]
            public string server() {
                X509Certificate remoteCertificate = _sslStream.RemoteCertificate;
                if (remoteCertificate != null) {
                    return remoteCertificate.Subject;
                } else {
                    return String.Empty;
                }
            }

            [Documentation("write(s) -> bytes_sent\n\n"
                + "Writes the string s through the SSL connection."
                )]
            public int write(CodeContext/*!*/ context, string data) {
                byte[] buffer = data.MakeByteArray();
                try {
                    _sslStream.Write(buffer);
                    return buffer.Length;
                } catch (Exception e) {
                    throw PythonSocket.MakeException(context, e);
                }
            }
        }

        #region Exported constants

        public const int SSL_ERROR_SSL = 1;
        public const int SSL_ERROR_WANT_READ = 2;
        public const int SSL_ERROR_WANT_WRITE = 3;
        public const int SSL_ERROR_WANT_X509_LOOKUP = 4;
        public const int SSL_ERROR_SYSCALL = 5;
        public const int SSL_ERROR_ZERO_RETURN = 6;
        public const int SSL_ERROR_WANT_CONNECT = 7;
        public const int SSL_ERROR_EOF = 8;
        public const int SSL_ERROR_INVALID_ERROR_CODE = 9;

        #endregion
    }
}
#endif
