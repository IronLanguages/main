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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace IronRuby.StandardLibrary.Digest {

    [RubyModule("Digest")]
    public static class Digest {

        #region Module Methods

        [RubyMethod("const_missing", RubyMethodAttributes.PublicSingleton)]
        public static object ConstantMissing(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name) {
            // TODO:
            throw new NotImplementedException();
        }

        [RubyMethod("hexencode", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ HexEncode(RubyModule/*!*/ self, [NotNull]MutableString/*!*/ str) {
            // TODO:
            throw new NotImplementedException();
        }

        #endregion

        // TODO: MRI doesn't define MD5 constant here, it implements const_missing
#if !SILVERLIGHT
        [RubyClass("MD5", BuildConfig = "!SILVERLIGHT")]
        public class MD5 : Base {
            public MD5()
                : base(System.Security.Cryptography.MD5.Create()) {
            }
        }

        [RubyClass("SHA1", BuildConfig = "!SILVERLIGHT")]
        public class SHA1 : Base {
            public SHA1()
                : base(System.Security.Cryptography.SHA1.Create()) {
            }
        }

        [RubyClass("SHA256", BuildConfig = "!SILVERLIGHT")]
        public class SHA256 : Base {
            public SHA256()
                : base(System.Security.Cryptography.SHA256.Create()) {
            }
        }

        [RubyClass("SHA384", BuildConfig = "!SILVERLIGHT")]
        public class SHA384 : Base {
            public SHA384()
                : base(System.Security.Cryptography.SHA384.Create()) {
            }
        }

        [RubyClass("SHA512", BuildConfig = "!SILVERLIGHT")]
        public class SHA512 : Base {
            public SHA512()
                : base(System.Security.Cryptography.SHA512.Create()) {
            }
        }
#endif

        [RubyClass("Base")]
        public class Base : Class {
            private readonly HashAlgorithm/*!*/ _algorithm;
            private MutableString/*!*/ _buffer;

            protected Base(HashAlgorithm/*!*/ algorithm) {
                Assert.NotNull(algorithm);
                _algorithm = algorithm;
                _buffer = MutableString.CreateBinary();
            }

            [RubyMethod("<<")]
            [RubyMethod("update")]
            public static Base/*!*/ Update(RubyContext/*!*/ context, Base/*!*/ self, MutableString str) {
                self._buffer.Append(str);
                return self;
            }

            [RubyMethod("finish", RubyMethodAttributes.PrivateInstance)]
            public static MutableString/*!*/ Finish(RubyContext/*!*/ context, Base/*!*/ self) {
                byte[] input = self._buffer.ConvertToBytes();
                byte[] hash = self._algorithm.ComputeHash(input);
                return MutableString.CreateBinary(hash);
            }

            [RubyMethod("reset")]
            public static Base/*!*/ Reset(RubyContext/*!*/ context, Base/*!*/ self) {
                self._buffer = MutableString.CreateBinary();
                self._algorithm.Initialize();
                return self;
            }
        }

        [RubyClass("Class"), Includes(typeof(Instance))]
        public class Class {

            [RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString Digest(
                CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
                CallSiteStorage<Func<CallSite, object, MutableString, object>>/*!*/ digestStorage,
                RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {

                // TODO: new?
                var allocateSite = allocateStorage.GetCallSite("allocate", 0);
                object obj = allocateSite.Target(allocateSite, self);

                // TODO: check obj
                var site = digestStorage.GetCallSite("digest", 1);
                return (MutableString)site.Target(site, obj, str);
            }

            [RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString Digest(RubyClass/*!*/ self) {
                throw RubyExceptions.CreateArgumentError("no data given");
            }

            [RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ HexDigest(CallSiteStorage<Func<CallSite, object, MutableString, object>>/*!*/ storage, 
                RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {

                var site = storage.GetCallSite("digest", 1);
                MutableString result = (MutableString)site.Target(site, self, str);
                // TODO: check result != null
                return HexEncode(result);
            }

            [RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
            public static MutableString HexDigest(RubyClass/*!*/ self) {
                throw RubyExceptions.CreateArgumentError("no data given");
            }

            #region Helpers

            internal static MutableString/*!*/ Bytes2Hex(byte[]/*!*/ bytes) {
                // TODO (opt): see also OpenSSL
                return MutableString.CreateAscii(System.BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant());
            }

            internal static MutableString/*!*/ HexEncode(MutableString/*!*/ str) {
                return Bytes2Hex(str.ConvertToBytes());
            }

            #endregion
        }

        [RubyModule("Instance")]
        public class Instance {

            [RubyMethod("digest")]
            public static MutableString Digest(
                CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
                CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                object self) {

                object clone;
                if (!RubyUtils.TryDuplicateObject(initializeCopyStorage, allocateStorage, self, true, out clone)) {
                    throw RubyExceptions.CreateArgumentError("unable to copy object");
                }

                var finish = finishStorage.GetCallSite("finish", 0);
                // TODO: cast?
                return (MutableString)finish.Target(finish, clone);
            }

            [RubyMethod("digest")]
            public static MutableString Digest(
                CallSiteStorage<Func<CallSite, object, MutableString, object>>/*!*/ updateStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ resetStorage,
                object self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {

                var update = updateStorage.GetCallSite("update", 1);
                update.Target(update, self, str);

                var finish = finishStorage.GetCallSite("finish", 0);
                object value = finish.Target(finish, self);

                var reset = resetStorage.GetCallSite("reset", 0);
                reset.Target(reset, self);

                // TODO: cast?
                return (MutableString)value;
            }

            [RubyMethod("digest!")]
            public static MutableString DigestNew(
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ resetStorage,
                object self) {

                var finish = finishStorage.GetCallSite("finish", 0);
                object value = finish.Target(finish, self);

                var reset = resetStorage.GetCallSite("reset", 0);
                reset.Target(reset, self);

                // TODO: cast?
                return (MutableString)value;
            }

            [RubyMethod("hexdigest")]
            public static MutableString HexDigest(
                CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
                CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                object self) {

                return Class.HexEncode(Digest(initializeCopyStorage, allocateStorage, finishStorage, self));
            }

            [RubyMethod("hexdigest")]
            public static MutableString HexDigest(
                CallSiteStorage<Func<CallSite, object, MutableString, object>>/*!*/ updateStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ resetStorage,
                object self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {
                return Class.HexEncode(Digest(updateStorage, finishStorage, resetStorage, self, str));
            }

            [RubyMethod("hexdigest!")]
            public static MutableString HexDigestNew(
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ finishStorage,
                CallSiteStorage<Func<CallSite, object, object>>/*!*/ resetStorage,
                object self) {
                return Class.HexEncode(DigestNew(finishStorage, resetStorage, self));
            }
        }
    }
}
