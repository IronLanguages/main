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

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Crypto = System.Security.Cryptography;

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
                        throw RubyExceptions.CreateRuntimeError("Unsupported digest algorithm ({0}).", algorithmName);
                    }

                    self._algorithm = algorithm;
                    return self;
                }

                // new(string) -> digest

                [RubyMethod("reset")]
                public static Digest/*!*/ Reset(Digest/*!*/ self) {
                    self._algorithm.Clear();
                    return self;
                }

                // update(string) -> aString
                // finish -> aString

                [RubyMethod("name")]
                public static MutableString/*!*/ Name(Digest/*!*/ self) {
                    return MutableString.CreateAscii(self._algorithm.HashName);
                }

                [RubyMethod("digest_size")]
                public static int Seed(Digest/*!*/ self) {
                    return self._algorithm.OutputBlockSize;
                }

                //TODO: Properly disable this with BuildConfig
                [RubyMethod("digest")]
                public static MutableString/*!*/ BlankDigest(Digest/*!*/ self) {
#if !SILVERLIGHT
                    // TODO: This support only SHA1, It should use self._algorithm but It is not
                    byte[] blank_data = Encoding.UTF8.GetBytes("");
                    byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(blank_data);
                    return MutableString.CreateBinary(hash);
#else
            throw new NotSupportedException();
#endif
                }

                //TODO: Properly disable with BuildConfig
                [RubyMethod("hexdigest")]
                public static MutableString/*!*/ BlankHexDigest(Digest/*!*/ self) {
#if !SILVERLIGHT
                    byte[] blank_data = Encoding.UTF8.GetBytes("");
                    byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(blank_data);
                    return MutableString.CreateAscii(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
#else
            throw new NotSupportedException();
#endif
                }
            }
        }

        [RubyClass("HMAC")]
        public class HMAC {

            internal static byte[] Digest(DigestFactory.Digest digest, MutableString key, MutableString data) {
                // TODO: does MRI really modify the digest object?
                digest.Algorithm.Key = key.ConvertToBytes();
                byte[] hash = digest.Algorithm.ComputeHash(data.ConvertToBytes());
                return hash;
            }

            [RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ HexDigest(RubyClass/*!*/ self,
                [NotNull]DigestFactory.Digest/*!*/ digest,
                [NotNull]MutableString/*!*/ key,
                [NotNull]MutableString/*!*/ data) {

                byte[] hash = Digest(digest, key, data);

                // TODO (opt):
                return MutableString.CreateAscii(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
            }

            [RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ Digest(RubyClass/*!*/ self,
                [NotNull]DigestFactory.Digest/*!*/ digest,
                [NotNull]MutableString/*!*/ key,
                [NotNull]MutableString/*!*/ data) {

                byte[] hash = Digest(digest, key, data);

                return MutableString.CreateBinary(hash);
            }

            // HMAC.new(key, digest) -> hmac
            // update(string) -> self
            // digest -> aString
            // hexdigest -> aString
            // reset -> self
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
            public static MutableString/*!*/ RandomBytes(RubyModule/*!*/ self, [DefaultProtocol]int length) {
                if (length < 0) {
                    throw RubyExceptions.CreateArgumentError("negative string size");
                }

                if (length == 0) {
                    return MutableString.CreateEmpty();
                }

                byte[] data = new byte[length];
                var generator = new Crypto.RNGCryptoServiceProvider();
                generator.GetBytes(data);

                return MutableString.CreateBinary(data);
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

        [RubyModule("X509")]
        public static class X509 {

            [RubyClass("CertificateError", Extends = typeof(CryptographicException), Inherits = typeof(ExternalException))]
            public class CryptographicExceptionOps {
                [RubyConstructor]
                public static CryptographicException/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, DefaultParameterValue(null)]MutableString message) {
                    CryptographicException result = new CryptographicException(RubyExceptions.MakeMessage(ref message, "Not enought data."));
                    RubyExceptionData.InitializeException(result, message);
                    return result;
                }
            }

            // TODO: Constants

            [RubyClass("Certificate")]
            public class Certificate {
                private X509Certificate/*!*/ _certificate;

                [RubyConstructor]
                public static Certificate/*!*/ CreateCertificate(RubyClass/*!*/ self) {
                    return Initialize(new Certificate(), null);
                }

                [RubyConstructor]
                public static Certificate/*!*/ CreateCertificate(RubyClass/*!*/ self, MutableString/*!*/ data) {
                    return Initialize(new Certificate(), data);
                }

                [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
                public static Certificate/*!*/ Initialize(Certificate/*!*/ self, MutableString/*!*/ data) {
                    if (data == null) {
                        self._certificate = new X509Certificate();
                    } else {
                        self._certificate = new X509Certificate(data.ToByteArray());
                    }

                    return self;
                }

                // add_extension
                // check_private_key
                // extensions
                // extensions=

                private static string OpenSSLFormat(string x509String) {
                    string[] pairs = x509String.Split(',');
                    Array.Sort<string>(pairs);

                    StringBuilder sb = new StringBuilder();
                    foreach (var val in pairs) {
                        sb.AppendFormat("/{0}", val.Trim());
                    }

                    return sb.ToString();
                }

                // issuer=

                [RubyMethod("issuer")]
                public static MutableString Issuer(Certificate/*!*/ self) {
                    if (self._certificate.Handle == IntPtr.Zero) {
                        return null;
                    } else {
                        return MutableString.CreateAscii(OpenSSLFormat(self._certificate.Issuer));
                    }
                }

                // not_after => time
                // not_after=
                // not_before => time
                // not_before=

                [RubyMethod("public_key")]
                public static MutableString PublicKey(Certificate/*!*/ self) {
                    if (self._certificate.Handle == IntPtr.Zero) {
                        // TODO: Raise OpenSSL::X509::CertificateError
                        return MutableString.CreateEmpty();
                    } else {
                        return MutableString.CreateAscii(self._certificate.GetPublicKeyString());
                    }
                }
                // public_key=

                private int SerailNumber {
                    get {
                        if (_certificate.Handle == IntPtr.Zero) {
                            return 0;
                        } else {
                            return int.Parse(_certificate.GetSerialNumberString(), CultureInfo.InvariantCulture);
                        }
                    }
                }

                [RubyMethod("serial")]
                public static int Serial(Certificate/*!*/ self) {
                    return self.SerailNumber;
                }

                // serial=
                // sign(key, digest) => self
                // signature_algorithm

                [RubyMethod("subject")]
                public static MutableString Subject(Certificate/*!*/ self) {
                    if (self._certificate.Handle == IntPtr.Zero) {
                        return null;
                    } else {
                        return MutableString.CreateAscii(OpenSSLFormat(self._certificate.Subject));
                    }
                }

                // subject=
                // to_der
                // to_pem

                [RubyMethod("inspect")]
                [RubyMethod("to_s")]
                public static MutableString ToString(RubyContext/*!*/ context, Certificate/*!*/ self) {
                    using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                        // #<OpenSSL::X509::Certificate subject=, issuer=, serial=0, not_before=nil, not_after=nil>
                        var result = MutableString.CreateEmpty();
                        result.Append("#<");
                        result.Append(context.Inspect(context.GetClassOf(self)));

                        if (handle == null) {
                            return result.Append(":...>");
                        }
                        bool empty = self._certificate.Handle == IntPtr.Zero;
                        result.AppendFormat(" subject={0}, issuer={1}, serial={2}, not_before=nil, not_after=nil>", 
                            empty ? "" : OpenSSLFormat(self._certificate.Subject),
                            empty ? "" : OpenSSLFormat(self._certificate.Issuer),
                            empty ? 0 : self.SerailNumber
                        );
                        return result;
                    }
                }

                // to_text
                // verify

                [RubyMethod("version")]
                public static int Version(Certificate/*!*/ self) {
                    if (self._certificate.Handle == IntPtr.Zero) {
                        return 0;
                    } else {
                        return 2;
                    }
                }

                // version=
            }

            [RubyClass("Name")]
            public class Name {
                // new => name
                // new(string) => name
                // new(dn) => name
                // new(dn, template) => name
                // add_entry(oid, value [, type]) => self
                // to_s => string
                // to_s(integer) => string
                // to_a => [[name, data, type], ...]
                // hash => integer
                // to_der => string
                // parse(string) => name
            }
        }

        [RubyModule("PKey")]
        public static class PKey {

            [RubyClass("RSA")]
            public class RSA {
                // RSA.new([size | encoded_key] [, pass]) -> rsa
                // new(2048) -> rsa 
                // new(File.read("rsa.pem")) -> rsa
                // new(File.read("rsa.pem"), "mypassword") -> rsa
                // initialize
                // generate(size [, exponent]) -> rsa
                // public? -> true (The return value is always true since every private key is also a public key)
                // private? -> true | false
                // to_pem -> aString
                // to_pem(cipher, pass) -> aString
                // to_der -> aString
                // public_encrypt(string [, padding]) -> aString
                // public_decrypt(string [, padding]) -> aString
                // private_encrypt(string [, padding]) -> aString
                // private_decrypt(string [, padding]) -> aString
                // params -> hash
                // to_text -> aString
                // public_key -> aRSA
                // inspect
                // to_s
            }
        }

        [RubyClass("OpenSSLError"), Serializable]
        public class OpenSSLError : SystemException {
            private const string/*!*/ M = "OpenSSL error";

            public OpenSSLError() : this(null, null) { }
            public OpenSSLError(string message) : this(message, null) { }
            public OpenSSLError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
            public OpenSSLError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
            protected OpenSSLError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyModule("SSL")]
        public static class SSL {
            [RubyClass("SSLError"), Serializable]
            public class SSLError : OpenSSLError {
                private const string/*!*/ M = "SSL error";

                public SSLError() : this(null, null) { }
                public SSLError(string message) : this(message, null) { }
                public SSLError(string message, Exception inner) : base(RubyExceptions.MakeMessage(message, M), inner) { }
                public SSLError(MutableString message) : base(RubyExceptions.MakeMessage(ref message, M)) { RubyExceptionData.InitializeException(this, message); }

#if !SILVERLIGHT
                protected SSLError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                    : base(info, context) { }
#endif
            }
        }
    }
}
