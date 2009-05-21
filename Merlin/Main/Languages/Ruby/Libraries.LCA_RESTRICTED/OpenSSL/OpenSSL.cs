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
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Crypto = System.Security.Cryptography;
using System.Text;

namespace IronRuby.StandardLibrary.OpenSsl {

    [RubyModule("OpenSSL")]
    public static class OpenSsl {
        // TODO: constants
        // Config,HMACError,PKCS12,Random,OPENSSL_VERSION,PKCS7,BN,ConfigError,PKey,Engine,BNError,Netscape,OCSP
        // OpenSSLError,CipherError,SSL,VERSION,X509,ASN1,OPENSSL_VERSION_NUMBER,Cipher

        [RubyConstant]
        public const string OPENSSL_VERSION = "OpenSSL 0.9.8d 28 Sep 2006";

        [RubyConstant]
        public const double OPENSSL_VERSION_NUMBER = 9470031;

        [RubyConstant]
        public const string VERSION = "1.0.0";

        [RubyModule("Digest")]
        public static class DigestFactory {

            // TODO: constants:
            // SHA224,MDC2,DSS1,SHA512,SHA1,MD5,DSS,SHA384,SHA,MD4,SHA256,DigestError,RIPEMD160,MD2

            [RubyClass("Digest")]
            public class Digest {
                private Crypto.HMAC _algorithm;

                public Crypto.HMAC Algorithm {
                    get { return _algorithm; }
                }

                protected Digest() {
                }

                [RubyConstructor]
                public static Digest/*!*/ CreateDigest(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ algorithmName) {
                    return Initialize(new Digest(), algorithmName);
                }
                
                // Reinitialization. Not called when a factory/non-default ctor is called.
                [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
                public static Digest/*!*/ Initialize(Digest/*!*/ self, [NotNull]MutableString/*!*/ algorithmName) {
                    Crypto.HMAC algorithm;

#if SILVERLIGHT
                    switch (algorithmName.ToString()) {
                        case "SHA1": algorithm = new Crypto.HMACSHA1(); break;
                        case "SHA256": algorithm = new Crypto.HMACSHA256(); break;
                        default: algorithm = null; break;
                    }
#else
                    algorithm = Crypto.HMAC.Create("HMAC" + algorithmName.ConvertToString());
#endif

                    if (algorithm == null) {
                        throw new RuntimeError(String.Format("Unsupported digest algorithm ({0}).", algorithmName));
                    }

                    self._algorithm = algorithm;
                    return self;
                }
            }
        }

        [RubyClass("HMAC")]
        public class HMAC {
            [RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ HexDigest(RubyClass/*!*/ self, [NotNull]DigestFactory.Digest/*!*/ digest,
                [NotNull]MutableString/*!*/ key, [NotNull]MutableString/*!*/ data) {
                
                // TODO: does MRI really modify the digest object?
                digest.Algorithm.Key = key.ConvertToBytes();
                byte[] hash = digest.Algorithm.ComputeHash(data.ConvertToBytes());

                return MutableString.Create(BitConverter.ToString(hash).Replace("-", "").ToLower());
            }
        }

        [RubyModule("Random")]
        public static class RandomModule {

            // This is a no-op method since our random number generator uses the .NET crypto random number generator
            // that gets its seed values from the OS

            [RubyMethod("seed", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ Seed(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ seed) {
                return seed;
            }

            [RubyMethod("pseudo_bytes", RubyMethodAttributes.PublicSingleton)]
            [RubyMethod("random_bytes", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ RandomBytes(RubyModule/*!*/ self, [DefaultProtocol, NotNull]int/*!*/ length) {
                if (length < 0) {
                    throw RubyExceptions.CreateArgumentError("negative string size");
                }

                if (length == 0) {
                    return MutableString.Create("");
                }

                var result = new StringBuilder(length);

                byte[] data = new byte[length];
                var generator = new Crypto.RNGCryptoServiceProvider();
                generator.GetBytes(data);

                for (int i = 0; i < length; i++) {
                    result.Append(Convert.ToChar(data[i]));
                }

                return MutableString.Create(result.ToString());
            }

            // add(str, entropy) -> self
            // load_random_file(filename) -> true
        }

        [RubyClass("BN")]
        public class BN {

            // new => aBN
            // new(bn) => aBN
            // new(string) => aBN
            // new(string, 0 | 2 | 10 | 16) => aBN

            [RubyMethod("rand", RubyMethodAttributes.PublicSingleton)]
            public static BigInteger/*!*/ Rand(RubyClass/*!*/ self, [DefaultProtocol]int bits, [DefaultProtocol, Optional]int someFlag, [Optional]bool otherFlag) { // TODO: figure out someFlag and otherFlag
                byte[] data = new byte[bits >> 3];
                var generator = new Crypto.RNGCryptoServiceProvider();
                generator.GetBytes(data);

                uint[] transformed = new uint[data.Length >> 2];
                int j = 0;
                for (int i = 0; i < transformed.Length; ++i) {
                    transformed[i] = data[j] + (uint)(data[j + 1] << 8) + (uint)(data[j + 2] << 16) + (uint)(data[j + 3] << 24);
                    j += 4;
                }

                return new BigInteger(1, transformed);
            }
        }
    }
}