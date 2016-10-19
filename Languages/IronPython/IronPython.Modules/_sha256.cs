/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

//!!! This is pretty inefficient. We should probably use hasher.TransformBlock instead of
//!!! hanging onto all of the bytes.
//!!! Also, we could probably make a generic version of this that could then be specialized
//!!! for both md5 and sha.

[assembly: PythonModule("_sha256", typeof(IronPython.Modules.PythonSha256))]
namespace IronPython.Modules {
    [Documentation("SHA256 hash algorithm")]
    public static class PythonSha256 {
        [ThreadStatic]
        private static SHA256 _hasher256;

#if !NETSTANDARD
        [ThreadStatic]
        private static SHA224 _hasher224;
#endif

        private const int blockSize = 64;

        public const string __doc__ = "SHA256 hash algorithm";

        private static SHA256 GetHasherSHA256() {
            if (_hasher256 == null) {
#if SILVERLIGHT || WP75
                 _hasher256 = new SHA256Managed();
#else
                _hasher256 = SHA256.Create();
#endif
            }
            return _hasher256;
        }

# if !NETSTANDARD
        private static SHA224 GetHasherSHA224() {
            if (_hasher224 == null) {
                _hasher224 = new SHA224();
            }
            return _hasher224;
        }
#endif

        public static Sha256Object sha256(object data) {
            return new Sha256Object(data);
        }

        public static Sha256Object sha256(Bytes data) {
            return new Sha256Object((IList<byte>)data);
        }

        public static Sha256Object sha256(PythonBuffer data) {
            return new Sha256Object((IList<byte>)data);
        }

        public static Sha256Object sha256(ByteArray data) {
            return new Sha256Object((IList<byte>)data);
        }

        public static Sha256Object sha256() {
            return new Sha256Object();
        }

        [PythonHidden]
        public sealed class Sha256Object : HashBase
#if FEATURE_ICLONEABLE
            , ICloneable 
#endif
        {
            internal Sha256Object() : this(new byte[0]) { }

            internal Sha256Object(object initialData) {
                _bytes = new byte[0];
                update(initialData);
            }

            internal Sha256Object(IList<byte> initialBytes) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            internal override HashAlgorithm Hasher {
                get {
                    return GetHasherSHA256();
                }
            }


            [Documentation("copy() -> object (copy of this object)")]
            public Sha256Object copy() {
                return new Sha256Object(_bytes);
            }
#if FEATURE_ICLONEABLE
            object ICloneable.Clone() {
                return copy();
            }
#endif

            public const int block_size = 64;
            public const int digest_size = 32;
            public const int digestsize = 32;
            public const string name = "SHA256";
        }

#if NETSTANDARD
        public static Sha256Object sha224(object data) {
            throw new NotImplementedException();
        }

        public static Sha256Object sha224() {
            throw new NotImplementedException();
        }
#else
        public static Sha224Object sha224(object data) {
            return new Sha224Object(data);
        }

        public static Sha224Object sha224(Bytes data) {
            return new Sha224Object((IList<byte>)data);
        }

        public static Sha224Object sha224(PythonBuffer data) {
            return new Sha224Object((IList<byte>)data);
        }

        public static Sha224Object sha224(ByteArray data) {
            return new Sha224Object((IList<byte>)data);
        }

        public static Sha224Object sha224() {
            return new Sha224Object();
        }

        [PythonHidden]
        public sealed class Sha224Object : HashBase
#if FEATURE_ICLONEABLE
            , ICloneable
#endif
        {
            internal Sha224Object() : this(new byte[0]) { }

            internal Sha224Object(object initialData) {
                _bytes = new byte[0];
                update(initialData);
            }

            internal Sha224Object(IList<byte> initialBytes) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            internal override HashAlgorithm Hasher {
                get {
                    return GetHasherSHA224();
                }
            }


            [Documentation("copy() -> object (copy of this object)")]
            public Sha224Object copy() {
                return new Sha224Object(_bytes);
            }
#if FEATURE_ICLONEABLE
            object ICloneable.Clone() {
                return copy();
            }
#endif

            public const int block_size = 64;
            public const int digest_size = 28;
            public const int digestsize = 28;
            public const string name = "SHA224";
        }
#endif
    }

#if !NETSTANDARD
    [PythonHidden]
    public class SHA224 : HashAlgorithm {
        private const int BLOCK_SIZE_BYTES = 64;
        private const int HASH_SIZE_BITS = 224;
        private const int HASH_SIZE_BYTES = HASH_SIZE_BITS / 8;

        private uint[] _H = new uint[8];
        private byte[] _nextBlock = new byte[BLOCK_SIZE_BYTES];
        private int _nextBlockCount;
        private uint[] _workingBuffer = new uint[64];
        private ulong _totalCount;

        public SHA224() {
            Initialize();
        }

        private static readonly uint[] _K = new uint[] {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
           0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
           0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
           0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
           0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
           0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
           0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2 };

        public override void Initialize() {
            _nextBlockCount = 0;
            _totalCount = 0;

            // set initial values for hash
            _H[0] = 0xc1059ed8;
            _H[1] = 0x367cd507;
            _H[2] = 0x3070dd17;
            _H[3] = 0xf70e5939;
            _H[4] = 0xffc00b31;
            _H[5] = 0x68581511;
            _H[6] = 0x64f98fa7;
            _H[7] = 0xbefa4fa4;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            int i;
            State = 1;

            if (_nextBlockCount != 0) {
                if (cbSize < (BLOCK_SIZE_BYTES - _nextBlockCount)) {
                    Buffer.BlockCopy(array, ibStart, _nextBlock, _nextBlockCount, cbSize);
                    _nextBlockCount += cbSize;
                    return;
                } else {
                    i = (BLOCK_SIZE_BYTES - _nextBlockCount);
                    Buffer.BlockCopy(array, ibStart, _nextBlock, _nextBlockCount, i);
                    ProcessSingleBlock(_nextBlock, 0);
                    _nextBlockCount = 0;
                    ibStart += i;
                    cbSize -= i;
                }
            }

            for (i = 0; i < cbSize - cbSize % BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES) {
                ProcessSingleBlock(array, ibStart + i);
            }

            if (cbSize % BLOCK_SIZE_BYTES != 0) {
                Buffer.BlockCopy(array, cbSize - cbSize % BLOCK_SIZE_BYTES + ibStart, _nextBlock, 0, cbSize % BLOCK_SIZE_BYTES);
                _nextBlockCount = cbSize % BLOCK_SIZE_BYTES;
            }
        }

        protected override byte[] HashFinal() {
            byte[] hash = new byte[28];
            int i, j;

            ProcessFinalBlock(_nextBlock, 0, _nextBlockCount);

            for (i = 0; i < 7; i++) {
                for (j = 0; j < 4; j++) {
                    hash[i * 4 + j] = (byte)(_H[i] >> (24 - j * 8));
                }
            }

            State = 0;
            return hash;
        }

        private void ProcessSingleBlock(byte[] block, int offset) {
            uint a, b, c, d, e, f, g, h;
            uint t1, t2;
            int i;
            uint[] buff = _workingBuffer;

            for (i = 0; i < 16; i++) {
                buff[i] = (uint)(((block[offset+ 4 * i]) << 24)
                    | ((block[offset + 4 * i + 1]) << 16)
                    | ((block[offset + 4 * i + 2]) << 8)
                    | ((block[offset + 4 * i + 3])));
            }


            for (i = 16; i < 64; i++) {
                t1 = buff[i - 15];
                t1 = (((t1 >> 7) | (t1 << 25)) ^ ((t1 >> 18) | (t1 << 14)) ^ (t1 >> 3));

                t2 = buff[i - 2];
                t2 = (((t2 >> 17) | (t2 << 15)) ^ ((t2 >> 19) | (t2 << 13)) ^ (t2 >> 10));
                buff[i] = t2 + buff[i - 7] + t1 + buff[i - 16];
            }

            a = _H[0];
            b = _H[1];
            c = _H[2];
            d = _H[3];
            e = _H[4];
            f = _H[5];
            g = _H[6];
            h = _H[7];

            for (i = 0; i < 64; i++) {
                t1 = h + (((e >> 6) | (e << 26)) ^ ((e >> 11) | (e << 21)) ^ ((e >> 25) | (e << 7))) + ((e & f) ^ (~e & g)) + _K[i] + buff[i];

                t2 = (((a >> 2) | (a << 30)) ^ ((a >> 13) | (a << 19)) ^ ((a >> 22) | (a << 10)));
                t2 = t2 + ((a & b) ^ (a & c) ^ (b & c));
                h = g;
                g = f;
                f = e;
                e = d + t1;
                d = c;
                c = b;
                b = a;
                a = t1 + t2;
            }

            _H[0] += a;
            _H[1] += b;
            _H[2] += c;
            _H[3] += d;
            _H[4] += e;
            _H[5] += f;
            _H[6] += g;
            _H[7] += h;
        }

        private void ProcessFinalBlock(byte[] data, int offset, int count) {
            ulong total = _totalCount + (ulong)count;
            int padding = (56 - (int)(total % BLOCK_SIZE_BYTES));

            if (padding < 1)
                padding += BLOCK_SIZE_BYTES;

            byte[] tempBuf = new byte[count + padding + 8];

            for (int i = 0; i < count; i++) {
                tempBuf[i] = data[i + offset];
            }

            tempBuf[count] = 0x80;
            for (int i = count + 1; i < count + padding; i++) {
                tempBuf[i] = 0x00;
            }

            ulong size = total << 3;
            AddLength(size, tempBuf, count + padding);
            ProcessSingleBlock(tempBuf, 0);

            if (count + padding + 8 == 128) {
                ProcessSingleBlock(tempBuf, 64);
            }
        }

        void AddLength(ulong length, byte[] buf, int pos) {
            buf[pos++] = (byte)(length >> 56);
            buf[pos++] = (byte)(length >> 48);
            buf[pos++] = (byte)(length >> 40);
            buf[pos++] = (byte)(length >> 32);
            buf[pos++] = (byte)(length >> 24);
            buf[pos++] = (byte)(length >> 16);
            buf[pos++] = (byte)(length >> 8);
            buf[pos] = (byte)(length);
        }

        public override int HashSize {
            get {
                return HASH_SIZE_BITS;
            }
        }

        private uint Ch(uint u, uint v, uint w) {
            return (u & v) ^ (~u & w);
        }

        private uint Maj(uint u, uint v, uint w) {
            return (u & v) ^ (u & w) ^ (v & w);
        }

        private uint Ro0(uint x) {
            return ((x >> 7) | (x << 25))
                ^ ((x >> 18) | (x << 14))
                ^ (x >> 3);
        }

        private uint Ro1(uint x) {
            return ((x >> 17) | (x << 15))
                ^ ((x >> 19) | (x << 13))
                ^ (x >> 10);
        }

        private uint Sig0(uint x) {
            return ((x >> 2) | (x << 30))
                ^ ((x >> 13) | (x << 19))
                ^ ((x >> 22) | (x << 10));
        }

        private uint Sig1(uint x) {
            return ((x >> 6) | (x << 26))
                ^ ((x >> 11) | (x << 21))
                ^ ((x >> 25) | (x << 7));
        }
    }
#endif

    public class HashBase {
        internal byte[] _bytes;
        private byte[] _hash;

        internal HashBase() {
        }

        internal virtual HashAlgorithm Hasher {
            get {
                throw new NotImplementedException();
            }
        }

        public void update(Bytes newBytes) {
            update((IList<byte>)newBytes);
        }

        public void update(ByteArray newBytes) {
            update((IList<byte>)newBytes);
        }

        internal void update(IList<byte> newBytes) {
            byte[] updatedBytes = new byte[_bytes.Length + newBytes.Count];
            Array.Copy(_bytes, updatedBytes, _bytes.Length);
            newBytes.CopyTo(updatedBytes, _bytes.Length);
            _bytes = updatedBytes;
            _hash = Hasher.ComputeHash(_bytes);
        }

        [Documentation("update(string) -> None (update digest with string data)")]
        public void update(object newData) {
            update(Converter.ConvertToString(newData).MakeByteArray());
        }

        public void update(PythonBuffer buffer) {
            update((IList<byte>)buffer);
        }

        [Documentation("digest() -> int (current digest value)")]
        public string digest() {
            return _hash.MakeString();
        }

        [Documentation("hexdigest() -> string (current digest as hex digits)")]
        public string hexdigest() {
            StringBuilder result = new StringBuilder(2 * _hash.Length);
            for (int i = 0; i < _hash.Length; i++) {
                result.Append(_hash[i].ToString("x2"));
            }
            return result.ToString();
        }
    }
}
