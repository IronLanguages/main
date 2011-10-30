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
using System.IO.Compression;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using zlib = ComponentAce.Compression.Libs.ZLib;

namespace IronRuby.StandardLibrary.Zlib {

    [RubyModule("Zlib")]
    public static class Zlib {

        #region Constants

        [RubyConstant]
        public const int NO_FLUSH = (int)zlib.FlushStrategy.Z_NO_FLUSH;

        [RubyConstant]
        public const int SYNC_FLUSH = (int)zlib.FlushStrategy.Z_SYNC_FLUSH;

        [RubyConstant]
        public const int FULL_FLUSH = (int)zlib.FlushStrategy.Z_FULL_FLUSH;

        [RubyConstant]
        public const int FINISH = (int)zlib.FlushStrategy.Z_FINISH;

        [RubyConstant]
        public static string ZLIB_VERSION = "1.2.3";

        [RubyConstant]
        public static string VERSION = "0.6.0";

        [RubyConstant]
        public const int MAX_WBITS = zlib.ZLibUtil.MAX_WBITS;

        [RubyConstant]
        public const int BINARY = (int)zlib.BlockType.Z_BINARY;

        [RubyConstant]
        public const int ASCII = (int)zlib.BlockType.Z_ASCII;

        [RubyConstant]
        public const int UNKNOWN = (int)zlib.BlockType.Z_UNKNOWN;

        [RubyConstant]
        public const int NO_COMPRESSION = 0;

        [RubyConstant]
        public const int BEST_SPEED = 1;

        [RubyConstant]
        public const int BEST_COMPRESSION = 9;

        [RubyConstant]
        public const int DEFAULT_COMPRESSION = zlib.Deflate.Z_DEFAULT_COMPRESSION;

        [RubyConstant]
        public const int FILTERED = (int)zlib.CompressionStrategy.Z_FILTERED;

        [RubyConstant]
        public const int HUFFMAN_ONLY = (int)zlib.CompressionStrategy.Z_HUFFMAN_ONLY;

        [RubyConstant]
        public const int DEFAULT_STRATEGY = (int)zlib.CompressionStrategy.Z_DEFAULT_STRATEGY;

        [RubyConstant]
        public const int DEF_MEM_LEVEL = zlib.Deflate.DEF_MEM_LEVEL;

        internal const int DEFAULTALLOC = 1024;
        private const int Z_OK = (int)zlib.ZLibResultCode.Z_OK;
        private const int Z_STREAM_END = (int)zlib.ZLibResultCode.Z_STREAM_END;
        private const int Z_BUF_ERROR = (int)zlib.ZLibResultCode.Z_BUF_ERROR;
        private const int Z_STREAM_ERROR = (int)zlib.ZLibResultCode.Z_STREAM_ERROR;
        
        #endregion

        #region CRC32

        [RubyMethod("crc_table", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetCrcTable(RubyModule/*!*/ self) {
            var result = new RubyArray(crcTable.Length);
            for (int i = 0; i < crcTable.Length; i++) {
                result.Add(Protocols.Normalize(crcTable[i]));
            }

            return result;
        }

        [RubyMethod("crc32", RubyMethodAttributes.PublicSingleton)]
        public static int GetCrc(RubyModule/*!*/ self) {
            return 0;
        }

        [RubyMethod("crc32", RubyMethodAttributes.PublicSingleton)]
        public static object GetCrc(RubyModule/*!*/ self, [Optional, DefaultProtocol]MutableString str, [Optional]int initialCrc) {
            byte[] bytes;
            if (str == null) {
                bytes = new byte[0];
            } else {
                bytes = str.ToByteArray();
            }
            uint result = UpdateCrc(unchecked((uint)initialCrc), bytes, 0, bytes.Length);
            return Protocols.Normalize(result);
        }

        // See RFC1950 for details. http://www.faqs.org/rfcs/rfc1950.html
        internal static uint UpdateCrc(uint crc, byte[] buffer, int offset, int length) {
            crc ^= 0xffffffffU;
            while (--length >= 0) {
                crc = crcTable[(crc ^ buffer[offset++]) & 0xFF] ^ (crc >> 8);
            }
            crc ^= 0xffffffffU;
            return crc;
        }

        private static readonly uint[] crcTable = new uint[] {  
            0x00000000u, 0x77073096u, 0xee0e612cu, 0x990951bau, 0x076dc419u,
            0x706af48fu, 0xe963a535u, 0x9e6495a3u, 0x0edb8832u, 0x79dcb8a4u,
            0xe0d5e91eu, 0x97d2d988u, 0x09b64c2bu, 0x7eb17cbdu, 0xe7b82d07u,
            0x90bf1d91u, 0x1db71064u, 0x6ab020f2u, 0xf3b97148u, 0x84be41deu,
            0x1adad47du, 0x6ddde4ebu, 0xf4d4b551u, 0x83d385c7u, 0x136c9856u,
            0x646ba8c0u, 0xfd62f97au, 0x8a65c9ecu, 0x14015c4fu, 0x63066cd9u,
            0xfa0f3d63u, 0x8d080df5u, 0x3b6e20c8u, 0x4c69105eu, 0xd56041e4u,
            0xa2677172u, 0x3c03e4d1u, 0x4b04d447u, 0xd20d85fdu, 0xa50ab56bu,
            0x35b5a8fau, 0x42b2986cu, 0xdbbbc9d6u, 0xacbcf940u, 0x32d86ce3u,
            0x45df5c75u, 0xdcd60dcfu, 0xabd13d59u, 0x26d930acu, 0x51de003au,
            0xc8d75180u, 0xbfd06116u, 0x21b4f4b5u, 0x56b3c423u, 0xcfba9599u,
            0xb8bda50fu, 0x2802b89eu, 0x5f058808u, 0xc60cd9b2u, 0xb10be924u,
            0x2f6f7c87u, 0x58684c11u, 0xc1611dabu, 0xb6662d3du, 0x76dc4190u,
            0x01db7106u, 0x98d220bcu, 0xefd5102au, 0x71b18589u, 0x06b6b51fu,
            0x9fbfe4a5u, 0xe8b8d433u, 0x7807c9a2u, 0x0f00f934u, 0x9609a88eu,
            0xe10e9818u, 0x7f6a0dbbu, 0x086d3d2du, 0x91646c97u, 0xe6635c01u,
            0x6b6b51f4u, 0x1c6c6162u, 0x856530d8u, 0xf262004eu, 0x6c0695edu,
            0x1b01a57bu, 0x8208f4c1u, 0xf50fc457u, 0x65b0d9c6u, 0x12b7e950u,
            0x8bbeb8eau, 0xfcb9887cu, 0x62dd1ddfu, 0x15da2d49u, 0x8cd37cf3u,
            0xfbd44c65u, 0x4db26158u, 0x3ab551ceu, 0xa3bc0074u, 0xd4bb30e2u,
            0x4adfa541u, 0x3dd895d7u, 0xa4d1c46du, 0xd3d6f4fbu, 0x4369e96au,
            0x346ed9fcu, 0xad678846u, 0xda60b8d0u, 0x44042d73u, 0x33031de5u,
            0xaa0a4c5fu, 0xdd0d7cc9u, 0x5005713cu, 0x270241aau, 0xbe0b1010u,
            0xc90c2086u, 0x5768b525u, 0x206f85b3u, 0xb966d409u, 0xce61e49fu,
            0x5edef90eu, 0x29d9c998u, 0xb0d09822u, 0xc7d7a8b4u, 0x59b33d17u,
            0x2eb40d81u, 0xb7bd5c3bu, 0xc0ba6cadu, 0xedb88320u, 0x9abfb3b6u,
            0x03b6e20cu, 0x74b1d29au, 0xead54739u, 0x9dd277afu, 0x04db2615u,
            0x73dc1683u, 0xe3630b12u, 0x94643b84u, 0x0d6d6a3eu, 0x7a6a5aa8u,
            0xe40ecf0bu, 0x9309ff9du, 0x0a00ae27u, 0x7d079eb1u, 0xf00f9344u,
            0x8708a3d2u, 0x1e01f268u, 0x6906c2feu, 0xf762575du, 0x806567cbu,
            0x196c3671u, 0x6e6b06e7u, 0xfed41b76u, 0x89d32be0u, 0x10da7a5au,
            0x67dd4accu, 0xf9b9df6fu, 0x8ebeeff9u, 0x17b7be43u, 0x60b08ed5u,
            0xd6d6a3e8u, 0xa1d1937eu, 0x38d8c2c4u, 0x4fdff252u, 0xd1bb67f1u,
            0xa6bc5767u, 0x3fb506ddu, 0x48b2364bu, 0xd80d2bdau, 0xaf0a1b4cu,
            0x36034af6u, 0x41047a60u, 0xdf60efc3u, 0xa867df55u, 0x316e8eefu,
            0x4669be79u, 0xcb61b38cu, 0xbc66831au, 0x256fd2a0u, 0x5268e236u,
            0xcc0c7795u, 0xbb0b4703u, 0x220216b9u, 0x5505262fu, 0xc5ba3bbeu,
            0xb2bd0b28u, 0x2bb45a92u, 0x5cb36a04u, 0xc2d7ffa7u, 0xb5d0cf31u,
            0x2cd99e8bu, 0x5bdeae1du, 0x9b64c2b0u, 0xec63f226u, 0x756aa39cu,
            0x026d930au, 0x9c0906a9u, 0xeb0e363fu, 0x72076785u, 0x05005713u,
            0x95bf4a82u, 0xe2b87a14u, 0x7bb12baeu, 0x0cb61b38u, 0x92d28e9bu,
            0xe5d5be0du, 0x7cdcefb7u, 0x0bdbdf21u, 0x86d3d2d4u, 0xf1d4e242u,
            0x68ddb3f8u, 0x1fda836eu, 0x81be16cdu, 0xf6b9265bu, 0x6fb077e1u,
            0x18b74777u, 0x88085ae6u, 0xff0f6a70u, 0x66063bcau, 0x11010b5cu,
            0x8f659effu, 0xf862ae69u, 0x616bffd3u, 0x166ccf45u, 0xa00ae278u,
            0xd70dd2eeu, 0x4e048354u, 0x3903b3c2u, 0xa7672661u, 0xd06016f7u,
            0x4969474du, 0x3e6e77dbu, 0xaed16a4au, 0xd9d65adcu, 0x40df0b66u,
            0x37d83bf0u, 0xa9bcae53u, 0xdebb9ec5u, 0x47b2cf7fu, 0x30b5ffe9u,
            0xbdbdf21cu, 0xcabac28au, 0x53b39330u, 0x24b4a3a6u, 0xbad03605u,
            0xcdd70693u, 0x54de5729u, 0x23d967bfu, 0xb3667a2eu, 0xc4614ab8u,
            0x5d681b02u, 0x2a6f2b94u, 0xb40bbe37u, 0xc30c8ea1u, 0x5a05df1bu,
            0x2d02ef8du
        };

        #endregion

        #region Adler32

        [RubyMethod("adler32", RubyMethodAttributes.PublicSingleton)]
        public static object Adler32(RubyModule/*!*/ self, [Optional]MutableString str, [DefaultParameterValue(1)]int baseValue) {
            if (MutableString.IsNullOrEmpty(str)) {
                return baseValue;
            }

            byte[] buffer = str.ToByteArray();
            return Protocols.Normalize(zlib.Adler32.GetAdler32Checksum(baseValue, buffer, 0, buffer.Length));
        }

        #endregion

        #region ZStream class

        [RubyClass("ZStream")]
        public abstract class ZStream : RubyObject {
            protected readonly zlib.ZStream _stream;

            protected ZStream(RubyClass/*!*/ cls, zlib.ZStream/*!*/ stream) 
                : base(cls) {
                Debug.Assert(stream != null);
                _stream = stream;
            }

            internal abstract MutableString/*!*/ Close();

            [RubyMethod("adler")]
            public static object Adler(ZStream/*!*/ self) {
                return Protocols.Normalize(self._stream.adler);
            }

            [RubyMethod("avail_in")]
            public static int AvailIn(ZStream/*!*/ self) {
                return self._stream.avail_in;
            }

            [RubyMethod("avail_out")]
            public static int GetAvailOut(ZStream/*!*/ self) {
                return self._stream.avail_out;
            }

            [RubyMethod("avail_out=")]
            public static int SetAvailOut(ZStream/*!*/ self, int size) {
                long newBufferSize;
                var zst = self._stream;
                if (size < 0 || (newBufferSize = zst.next_out_index + size) > Int32.MaxValue) {
                    throw RubyExceptions.CreateArgumentError("negative string size (or size too big)");
                }

                int old = self._stream.avail_out;

                // Make sure we have enough space in the buffer.
                // We could keep the buffer larger but since users are calling 
                // this API explicitly they probably want to resize the buffer.
                var output = zst.next_out;
                Array.Resize(ref output, (int)newBufferSize);
                zst.next_out = output;
                self._stream.avail_out = size;
                return old;
            }

            [RubyMethod("finish")]
            [RubyMethod("close")]
            public static MutableString/*!*/ Close(ZStream/*!*/ self) {
                return self._stream.IsInitialized ? self.Close() : MutableString.CreateEmpty();
            }

            [RubyMethod("stream_end?")]
            [RubyMethod("finished?")]
            [RubyMethod("closed?")]
            public static bool IsClosed(ZStream/*!*/ self) {
                return !self._stream.IsInitialized;
            }

            [RubyMethod("data_type")]
            public static int DataType(ZStream/*!*/ self) {
                return (int)self._stream.Data_type;
            }

            [RubyMethod("flush_next_in")]
            public static MutableString/*!*/ FlushNextIn(ZStream/*!*/ self) {
                throw new NotImplementedError();
            }

            [RubyMethod("flush_next_out")]
            public static MutableString/*!*/ FlushNextOut(ZStream/*!*/ self) {
                throw new NotImplementedError();
            }

            [RubyMethod("reset")]
            public static void Reset(ZStream/*!*/ self) {
                if (self._stream.IsInitialized) {
                    int err = self._stream.reset();
                    Debug.Assert(err == Z_OK);
                }
            }

            [RubyMethod("total_in")]
            public static object TotalIn(ZStream/*!*/ self) {
                return Protocols.Normalize(self._stream.total_in);
            }

            [RubyMethod("total_out")]
            public static object TotalOut(ZStream/*!*/ self) {
                return Protocols.Normalize(self._stream.total_out);
            }
        }

        #endregion

        #region Deflate class

        [RubyClass("Deflate")]
        public class Deflate : ZStream {
            public Deflate(
                RubyClass/*!*/ cls,
                [DefaultParameterValue(DEFAULT_COMPRESSION)]int level,
                [DefaultParameterValue(MAX_WBITS)]int windowBits,
                [DefaultParameterValue(DEF_MEM_LEVEL)]int memlevel,
                [DefaultParameterValue(DEFAULT_STRATEGY)]int strategy)
                : base(cls, CreateDeflateStream(level, windowBits, memlevel, strategy)) {
            }

            private Deflate(RubyClass/*!*/ cls, int level)
                : this(cls, level, MAX_WBITS, DEF_MEM_LEVEL, DEFAULT_STRATEGY) {
            }

            private static zlib.ZStream CreateDeflateStream(int level, int windowBits, int memLevel, int strategy) {
                var stream = new zlib.ZStream();
                int result = stream.deflateInit(level, windowBits, memLevel, (zlib.CompressionStrategy)strategy);
                if (result != Z_OK) {
                    throw MakeError(result);
                }

                return stream;
            }

            [RubyMethod("<<")]
            public static Deflate/*!*/ AppendCompressed(Deflate/*!*/ self, [DefaultProtocol]MutableString str) {
                var zst = self._stream;
                int err = CompressInternal(self, str, NO_FLUSH);

                if (err != Z_OK) {
                    throw MakeError(err);
                }

                return self;
            }

            [RubyMethod("deflate")]
            public static MutableString/*!*/ Compress(Deflate/*!*/ self, [DefaultProtocol]MutableString str, [DefaultParameterValue(NO_FLUSH)]int flush) {
                var zst = self._stream;

                var result = MutableString.CreateBinary();

                // add previously compressed data to the output:
                if (zst.next_out != null) {
                    result.Append(zst.next_out, 0, zst.next_out_index);
                }

                int err;
                try {
                    int bufferStart = zst.next_out_index;
                    err = CompressInternal(self, str, flush);
                    result.Append(zst.next_out, bufferStart, zst.next_out_index - bufferStart);

                    if (err == Z_STREAM_END && flush == FINISH) {
                        err = zst.deflateEnd();
                    }
                    
                    if (err != Z_OK) {
                        throw MakeError(err);
                    }

                } finally {
                    zst.next_out = null;
                    zst.next_out_index = 0;
                    zst.avail_out = 0;
                }

                return result;
            }

            private static int CompressInternal(Deflate/*!*/ self, MutableString str, int flush) {
                var zst = self._stream;

                if (str == null) {
                    str = MutableString.FrozenEmpty;
                    flush = FINISH;
                } else if (str.Length == 0 && flush == NO_FLUSH) {
                    return Z_OK;
                } 
                
                if (!MutableString.IsNullOrEmpty(str)) {
                    byte[] input = str.ToByteArray();
                    zst.next_in = input;
                    zst.next_in_index = 0;
                    zst.avail_in = input.Length;
                } else {
                    zst.avail_in = 0;
                }

                if (zst.next_out == null) {
                    zst.next_out = new byte[DEFAULTALLOC];
                    zst.next_out_index = 0;
                    zst.avail_out = zst.next_out.Length;
                }

                int err = zst.deflate((zlib.FlushStrategy)flush);

                // double flush:
                if (flush != FINISH && err == (int)zlib.ZLibResultCode.Z_DATA_ERROR) {
                    return Z_OK;
                }

                while (err == Z_OK && zst.avail_out == 0) {
                    byte[] output = zst.next_out;
                    int oldLength = output.Length;

                    Array.Resize(ref output, oldLength * 2);

                    zst.next_out = output;
                    zst.avail_out = oldLength;

                    err = zst.deflate((zlib.FlushStrategy)flush);
                }

                return err;
            }

            internal override MutableString Close() {
                return Compress(this, null, FINISH);
            }

            [RubyMethod("flush")]
            public static MutableString/*!*/ Flush(Deflate/*!*/ self, [DefaultParameterValue(SYNC_FLUSH)]int flush) {
                if (flush == NO_FLUSH) {
                    return MutableString.CreateEmpty();
                }

                return Compress(self, MutableString.FrozenEmpty, flush);
            }

            [RubyMethod("params")]
            public static void SetParams(
                Deflate/*!*/ self, 
                [DefaultParameterValue(DEFAULT_COMPRESSION)]int level, 
                [DefaultParameterValue(DEFAULT_STRATEGY)]int strategy) {
                
                int err = self._stream.deflateParams(level, (zlib.CompressionStrategy)strategy);
                if (err != Z_OK) {
                    throw MakeError(err);
                }
            }

            [RubyMethod("set_dictionary")]
            public static void SetParams(Deflate/*!*/ self, [NotNull]MutableString/*!*/ dictionary) {
                byte[] buffer = dictionary.ToByteArray();
                int err = self._stream.deflateSetDictionary(buffer, buffer.Length);
                if (err != Z_OK) {
                    throw MakeError(err);
                }
            }

            [RubyMethod("deflate", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ DeflateString(RubyClass/*!*/ self, 
                [DefaultProtocol, NotNull]MutableString/*!*/ str, 
                [DefaultParameterValue(DEFAULT_COMPRESSION)]int level) {

                return Compress(new Deflate(self, level), str, FINISH);
            }
        }

        #endregion

        #region Inflate class

        [RubyClass("Inflate")]
        public class Inflate : ZStream {
            public Inflate(RubyClass/*!*/ cls, [DefaultParameterValue(MAX_WBITS)]int windowBits)
                : base(cls, CreateInflateStream(windowBits)) {
            }

            private static zlib.ZStream CreateInflateStream(int windowBits) {
                var zst = new zlib.ZStream();
                int result = zst.inflateInit(windowBits);
                if (result != Z_OK) {
                    throw MakeError(result);
                }

                return zst;
            }

            [RubyMethod("inflate")]
            public static MutableString/*!*/ InflateString(Inflate/*!*/ self, [DefaultProtocol]MutableString str) {
                var zst = self._stream;
                byte[] output = new byte[DEFAULTALLOC];

                zlib.FlushStrategy flush;
                if (str != null) {
                    byte[] input = str.ToByteArray();

                    zst.next_in = input;
                    zst.next_in_index = 0;
                    zst.avail_in = input.Length;
                    zst.next_out = output;
                    zst.next_out_index = 0;
                    zst.avail_out = output.Length;
                    flush = zlib.FlushStrategy.Z_SYNC_FLUSH;
                } else {
                    zst.next_out = output;
                    zst.next_out_index = 0;
                    zst.avail_out = output.Length;
                    flush = zlib.FlushStrategy.Z_FINISH;
                }

                long start_total_out = zst.total_out;
                int err = zst.inflate(flush);

                while (err == Z_OK && zst.avail_out == 0) {
                    int old_length = output.Length;
                    Array.Resize(ref output, output.Length * 2);
                    zst.next_out = output;
                    zst.avail_out = old_length;

                    err = zst.inflate(flush);
                }

                if (str == null) {
                    if (err != Z_STREAM_END && err != Z_OK && err != Z_BUF_ERROR) {
                        throw MakeError(err);
                    }
                } else if (err == Z_STREAM_END) {
                    err = zst.inflateEnd();
                    if (err != Z_OK) {
                        throw MakeError(err);
                    }
                }

                return MutableString.CreateBinary().Append(output, 0, (int)(zst.total_out - start_total_out));
            }

            internal override MutableString Close() {
                return InflateString(this, null);
            }

            [RubyMethod("flush")]
            public static MutableString/*!*/ Flush(Inflate/*!*/ self) {
                return InflateString(self, null);
            }

            [RubyMethod("inflate", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ InflateString(RubyClass/*!*/ self, 
                [DefaultProtocol, NotNull]MutableString/*!*/ str) {

                return InflateString(new Inflate(self, MAX_WBITS), str);
            }
        }

        #endregion

        #region GzipFile class

        [RubyClass("GzipFile")]
        public class GZipFile : RubyObject {
            protected IOWrapper/*!*/ _ioWrapper;
            protected List<byte>/*!*/ _inputBuffer;
            protected List<byte>/*!*/ _outputBuffer;
            protected int _outPos;
            protected int _inPos;
            protected bool _isClosed;
            protected MutableString _originalName;
            protected MutableString _comment;

            public GZipFile(RubyClass/*!*/ cls, IOWrapper/*!*/ ioWrapper) 
                : base(cls) {
                Debug.Assert(ioWrapper != null);
                _ioWrapper = ioWrapper;
                _inputBuffer = new List<byte>();
                _outputBuffer = new List<byte>();
                _outPos = -1;
                _inPos = -1;
            }

            [RubyClass("Error")]
            public class Error : RuntimeError {
                public Error(string message)
                    : base(message) {
                }
            }

            // TODO: missing NoFooter, LengthError, CRCError constants

            [RubyMethod("wrap", RubyMethodAttributes.PublicSingleton)]
            public static object Wrap(BinaryOpStorage/*!*/ newStorage, UnaryOpStorage/*!*/ closedStorage, UnaryOpStorage/*!*/ closeStorage, 
                BlockParam block, RubyClass/*!*/ self, object io) {

                var newSite = newStorage.GetCallSite("new");
                GZipFile gzipFile = (GZipFile)newSite.Target(newSite, self, io);

                if (block == null) {
                    return gzipFile;
                }

                try {
                    object blockResult;
                    block.Yield(gzipFile, out blockResult);
                    return blockResult;
                } finally {
                    CloseFile(closedStorage, closeStorage, self, gzipFile);
                }
            }

            private static void CloseFile(UnaryOpStorage/*!*/ closedStorage, UnaryOpStorage/*!*/ closeStorage, RubyClass self, GZipFile gzipFile) {
                var closedSite = closedStorage.GetCallSite("closed?");
                bool isClosed = Protocols.IsTrue(closedSite.Target(closedSite, gzipFile));

                if (!isClosed) {
                    var closeSite = closeStorage.GetCallSite("close");
                    closeSite.Target(closeSite, gzipFile);
                }
            }

            internal static void Close(UnaryOpStorage/*!*/ closeStorage, GZipFile/*!*/ self, bool closeIO) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }

                if (closeIO && self._ioWrapper.CanBeClosed) {
                    var site = closeStorage.GetCallSite("close");
                    site.Target(site, self._ioWrapper.UnderlyingObject);
                }

                self._isClosed = true;
            }


            [RubyMethod("closed?")]
            public static bool IsClosed(GZipFile/*!*/ self) {
                return self._isClosed;
            }

            [RubyMethod("comment")]
            public static MutableString Comment(GZipFile/*!*/ self) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }

                return self._comment;
            }

            // crc() 
            // level() 
            // mtime() 

            [RubyMethod("orig_name")]
            [RubyMethod("original_name")]
            public static MutableString OriginalName(GZipFile/*!*/ self) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }

                return self._originalName;
            }

            // os_code() 
            // sync() 
            // sync = flag
            // to_io
        }

        #endregion

        #region GzipReader class

        /* [RubyClass("GzipReader"), Includes(typeof(Enumerable))] */
        [RubyClass("GzipReader")]
        public class GZipReader : GZipFile {

            protected MutableString _xtraField;
            protected MutableString/*!*/ _contents;
            protected ushort _headerCrc;

            private GZipReader(RubyClass/*!*/ cls, IOWrapper/*!*/ ioWrapper, BinaryReader/*!*/ reader)
                : base(cls, ioWrapper) {

                // TODO: should all of this code be moved to open()?
                if (ReadUInt16LE(reader) != 0x8b1f) {
                    throw new Error("not in gzip format");
                }
                if (reader.ReadByte() != 0x08) {
                    throw new Error("unknown compression method");
                }

#pragma warning disable 168,219 // TODO: mcs: unused locals
                byte flg = reader.ReadByte();
                bool ftext = IsBitSet(flg, 0);
                bool fhcrc = IsBitSet(flg, 1);
                bool fextra = IsBitSet(flg, 2);
                bool fname = IsBitSet(flg, 3);
                bool fcomment = IsBitSet(flg, 4);

                uint secondsSince1970 = ReadUInt32LE(reader);
                DateTime mtime = RubyTime.Epoch.AddSeconds((double)secondsSince1970);
                byte xfl = reader.ReadByte();
                string os = GZipReader.OSES[reader.ReadByte()];
                if (fextra) {
                    int xlen = ReadUInt16LE(reader);
                    _xtraField = MutableString.CreateBinary(reader.ReadBytes(xlen));
                }
#pragma warning restore 168,219

                if (fname) {
                    _originalName = ReadStringZ(reader);
                } else {
                    _originalName = MutableString.CreateBinary();
                }

                if (fcomment) {
                    _comment = ReadStringZ(reader);
                } else {
                    _comment = MutableString.CreateBinary();
                }

                if (fhcrc) {
                    _headerCrc = ReadUInt16LE(reader);
                }

                _contents = ReadToEnd(reader);
            }

            [RubyConstructor]
            public static GZipReader/*!*/ Create(RespondToStorage/*!*/ respondToStorage, RubyClass/*!*/ self, object io) {
                IOWrapper stream = null;
                if (io != null) {
                    stream = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Read);
                }
                if (stream == null || !stream.CanRead) {
                    throw RubyExceptions.CreateMethodMissing(self.Context, io, "read");
                }

                using (BinaryReader reader = new BinaryReader(stream)) {
                    return new GZipReader(self, stream, reader);
                }
            }

            private bool IsBitSet(byte b, byte bit) {
                return ((b & (1 << bit)) == (1 << bit));
            }

            [RubyMethod("xtra_field")]
            public static MutableString ExtraField(GZipReader/*!*/ self) {
                return self._xtraField;
            }

            // TODO: mutable!
            [RubyConstant("OSES")]
            public static string[] OSES = {
                "FAT filesystem", 
			    "Amiga", 
			    "VMS (or OpenVMS)", 
			    "Unix", 
			    "VM/CMS", 
			    "Atari TOS", 
			    "HPFS fileystem (OS/2, NT)", 
			    "Macintosh", 
			    "Z-System",
			    "CP/M",
			    "TOPS-20",
			    "NTFS filesystem (NT)",
			    "QDOS",
			    "Acorn RISCOS",
			    "unknown"
            };

            private static ushort ReadUInt16LE(BinaryReader/*!*/ reader) {
                return (ushort)(
                    (ushort)(reader.ReadByte()) |
                    (((ushort)(reader.ReadByte())) << 8)
                    );
            }

            private static uint ReadUInt32LE(BinaryReader/*!*/ reader) {
                return (uint)(
                    (uint)(reader.ReadByte()) |
                    (((uint)(reader.ReadByte())) << 8) |
                    (((uint)(reader.ReadByte())) << 16) |
                    (((uint)(reader.ReadByte())) << 24)
                    );
            }

            private static MutableString/*!*/ ReadStringZ(BinaryReader/*!*/ reader) {
                List<byte> result = new List<byte>();
                byte c;
                while ((c = reader.ReadByte()) != 0) {
                    result.Add(c);
                }
                return MutableString.CreateBinary(result, RubyEncoding.Binary);
            }

            private static MutableString/*!*/ ReadToEnd(BinaryReader/*!*/ reader) {
                List<byte> result = new List<byte>();
                try {
                    while (true) {
                        result.Add(reader.ReadByte());
                    }
                } catch (EndOfStreamException) {
                }
                return MutableString.CreateBinary(result, RubyEncoding.Binary);
            }

            [RubyMethod("read")]
            public static MutableString/*!*/ Read(GZipReader/*!*/ self) {
                // TODO:
                Inflate z = new Inflate(self.ImmediateClass, MAX_WBITS);
                return Inflate.InflateString(z, self._contents);
            }

            [RubyMethod("open", RubyMethodAttributes.PrivateInstance)]
            public static GZipReader/*!*/ Open(GZipReader/*!*/ self) {
                // TODO: Open as an private instance method probably doesn't create a new GzipReader, right?
                // it probably returns nothing and is used internally to do all initialization
                return self;
            }

            [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
            public static GZipReader/*!*/ Open(RespondToStorage/*!*/ respondToStorage, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
                return Create(respondToStorage, self, new RubyFile(self.Context, path.ConvertToString(), IOMode.ReadOnly));
            }

            [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
            public static object Open(RespondToStorage/*!*/ respondToStorage, [NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
                GZipReader reader = Open(respondToStorage, self, path);
                object blockResult;
                block.Yield(reader, out blockResult);
                return blockResult;
            }

            // pos() 

            [RubyMethod("close")]
            public static object/*!*/ Close(UnaryOpStorage/*!*/ closeStorage, RubyContext/*!*/ context, GZipReader/*!*/ self) {
                GZipFile.Close(closeStorage, self, true);
                return self._ioWrapper.UnderlyingObject;
            }

            [RubyMethod("finish")]
            public static object/*!*/ Finish(UnaryOpStorage/*!*/ closeStorage, RubyContext/*!*/ context, GZipReader/*!*/ self) {
                GZipFile.Close(closeStorage, self, false);
                return self._ioWrapper.UnderlyingObject;
            }
        }

        #endregion

        #region GzipWriter class

#if !SILVERLIGHT
        [RubyClass("GzipWriter", BuildConfig="!SILVERLIGHT")]
        public class GzipWriter : GZipFile {
            private readonly GZipStream/*!*/ _gzipStream;

            // TODO:
#pragma warning disable 414 // mcs: unused field
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private int _level, _strategy;
#pragma warning restore 414

            private GzipWriter(RubyClass/*!*/ cls, RespondToStorage/*!*/ respondToStorage, RubyContext/*!*/ context, IOWrapper/*!*/ ioWrapper, int level, int strategy) 
                : base(cls, ioWrapper) {
                _level = level;
                _strategy = strategy;
                _gzipStream = new GZipStream(ioWrapper, CompressionMode.Compress, true);
            }

            [RubyConstructor]
            public static GzipWriter/*!*/ Create(
                RespondToStorage/*!*/ respondToStorage,
                RubyClass/*!*/ self,
                object io,
                [DefaultParameterValue(0)]int level,
                [DefaultParameterValue(DEFAULT_STRATEGY)]int strategy) {

                IOWrapper ioWrapper = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Write);
                if (ioWrapper == null || !ioWrapper.CanWrite) {
                    throw RubyExceptions.CreateMethodMissing(self.Context, io, "write");
                }
                
                return new GzipWriter(self, respondToStorage, self.Context, ioWrapper, level, strategy);
            }

            // Zlib::GzipWriter.open(filename, level=nil, strategy=nil) { |gz| ... }

            [RubyMethod("<<")]
            public static GzipWriter Output(ConversionStorage<MutableString>/*!*/ tosConversion, RubyContext/*!*/ context, GzipWriter/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {
                Write(tosConversion, context, self, str);
                return self;
            }

            [RubyMethod("close")]
            public static object/*!*/ Close(UnaryOpStorage/*!*/ closeStorage, RubyContext/*!*/ context, GzipWriter/*!*/ self) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }
                self._gzipStream.Close();
                self._ioWrapper.Flush();
                GZipFile.Close(closeStorage, self, true);
 				return self._ioWrapper.UnderlyingObject;
            }

            [RubyMethod("finish")]
            public static object/*!*/ Finish(UnaryOpStorage/*!*/ closeStorage, RubyContext/*!*/ context, GzipWriter/*!*/ self) {
                self._gzipStream.Close();
                self._ioWrapper.Flush(closeStorage, context);
                GZipFile.Close(closeStorage, self, false);
                return self._ioWrapper.UnderlyingObject;
            }

            [RubyMethod("comment=")]
            public static MutableString/*!*/ Comment(GzipWriter/*!*/ self, [NotNull]MutableString/*!*/ comment) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }
                self._comment = comment;

                return comment;
            }

            [RubyMethod("flush")]
            public static GzipWriter Flush(UnaryOpStorage/*!*/ flushStorage, RubyContext/*!*/ context, GzipWriter/*!*/ self, object flush) {
                if (flush != null) {
                    throw RubyExceptions.CreateUnexpectedTypeError(context, flush, "Fixnum");
                }

                return Flush(flushStorage, context, self, SYNC_FLUSH);
            }

            [RubyMethod("flush")]
            public static GzipWriter Flush(UnaryOpStorage/*!*/ flushStorage, RubyContext/*!*/ context, GzipWriter/*!*/ self, [DefaultParameterValue(SYNC_FLUSH)]int flush) {
                self._gzipStream.Flush();
                self._ioWrapper.Flush(flushStorage, context);
                return self;
            }

            // mtime=(p1) 

            [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
            public static object Open(
                RespondToStorage/*!*/ respondToStorage, 
                UnaryOpStorage/*!*/ closeStorage, 
                BlockParam block, 
                RubyClass/*!*/ self, 
                [NotNull]MutableString filename, 
                [DefaultParameterValue(0)]int level, 
                [DefaultParameterValue(DEFAULT_STRATEGY)]int strategy) {

                RubyFile file = new RubyFile(self.Context, filename.ConvertToString(), IOMode.CreateIfNotExists | IOMode.Truncate | IOMode.WriteOnly | IOMode.PreserveEndOfLines);
                GzipWriter gzipFile = Create(respondToStorage, self, file, level, strategy);

                if (block == null) {
                    return gzipFile;
                }

                try {
                    object blockResult;
                    block.Yield(gzipFile, out blockResult);
                    return blockResult;
                } finally {
                    Close(closeStorage, self.Context, gzipFile);
                }
            }

            [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
            public static object Open(
                RespondToStorage/*!*/ respondToStorage,
                UnaryOpStorage/*!*/ closeStorage,
                BlockParam block,
                RubyClass/*!*/ self,
                [NotNull]MutableString filename,
                object level,
                object strategy) {

                if (level != null) {
                    throw RubyExceptions.CreateUnexpectedTypeError(self.Context, level, "Fixnum");
                }
                if (strategy != null) {
                    throw RubyExceptions.CreateUnexpectedTypeError(self.Context, strategy, "Fixnum");
                }

                return Open(respondToStorage, closeStorage, block, self, filename, 0, 0);
            }

            [RubyMethod("orig_name=")]
            public static MutableString/*!*/ OriginalName(GzipWriter/*!*/ self, [NotNull]MutableString/*!*/ originalName) {
                if (self._isClosed) {
                    throw new Error("closed gzip stream");
                }

                self._originalName = originalName;

                return originalName;
            }

            // pos() 
            // print(...) 
            // printf(...) 
            // putc(p1) 
            // puts(...) 
            // tell() 
            [RubyMethod("write")]
            public static int Write(ConversionStorage<MutableString>/*!*/ tosConversion, RubyContext/*!*/ context, GzipWriter/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {
                byte[] bytes = str.ToByteArray();
                self._gzipStream.Write(bytes, 0, bytes.Length);
                return bytes.Length;
            }
        }
#endif
        #endregion

        #region Exceptions

        private static Exception/*!*/ MakeError(int result) {
            switch ((zlib.ZLibResultCode)result) {
                case zlib.ZLibResultCode.Z_NEED_DICT:
                    return new NeedDict();

                case zlib.ZLibResultCode.Z_MEM_ERROR:
                    return new MemError();

                case zlib.ZLibResultCode.Z_DATA_ERROR:
                    return new DataError();

                case zlib.ZLibResultCode.Z_BUF_ERROR:
                    return new BufError();

                case zlib.ZLibResultCode.Z_STREAM_ERROR:
                default:
                    return new StreamError();
            }
        }

        [RubyException("Error"), Serializable]
        public class Error : SystemException {
            public Error() : this(null, null) { }
            public Error(string message) : this(message, null) { }
            public Error(string message, Exception inner) : base(message ?? "Error", inner) { }

#if !SILVERLIGHT
            protected Error(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyException("DataError"), Serializable]
        public class DataError : Error {
            public DataError() : this(null, null) { }
            public DataError(string message) : this(message, null) { }
            public DataError(string message, Exception inner) : base(message ?? "DataError", inner) { }

#if !SILVERLIGHT
            protected DataError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyException("BufError"), Serializable]
        public class BufError : Error {
            public BufError() : this(null, null) { }
            public BufError(string message) : this(message, null) { }
            public BufError(string message, Exception inner) : base(message ?? "buffer error", inner) { }

#if !SILVERLIGHT
            protected BufError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyException("MemError"), Serializable]
        public class MemError : Error {
            public MemError() : this(null, null) { }
            public MemError(string message) : this(message, null) { }
            public MemError(string message, Exception inner) : base(message ?? "MemError", inner) { }

#if !SILVERLIGHT
            protected MemError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyException("NeedDict"), Serializable]
        public class NeedDict : Error {
            public NeedDict() : this(null, null) { }
            public NeedDict(string message) : this(message, null) { }
            public NeedDict(string message, Exception inner) : base(message ?? "need dictionary", inner) { }

#if !SILVERLIGHT
            protected NeedDict(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        [RubyException("StreamError"), Serializable]
        public class StreamError : Error {
            public StreamError() : this(null, null) { }
            public StreamError(string message) : this(message, null) { }
            public StreamError(string message, Exception inner) : base(message ?? "StreamError", inner) { }

#if !SILVERLIGHT
            protected StreamError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif
        }

        #endregion
    }
}
