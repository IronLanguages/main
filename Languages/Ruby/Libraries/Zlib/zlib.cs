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

namespace IronRuby.StandardLibrary.Zlib {

    [RubyModule("Zlib")]
    public static class Zlib {

        #region Constants

        [RubyConstant("NO_FLUSH")]
        public const int NO_FLUSH = 0;

        [RubyConstant("SYNC_FLUSH")]
        public const int SYNC_FLUSH = 2;

        [RubyConstant("FULL_FLUSH")]
        public const int FULL_FLUSH = 3;

        [RubyConstant("FINISH")]
        public const int FINISH = 4;

        [RubyConstant("ZLIB_VERSION")]
        public static string ZLIB_VERSION = "1.2.3";

        [RubyConstant("VERSION")]
        public static string VERSION = "0.6.0";

        [RubyConstant("MAXBITS")]
        public const int MAXBITS = 15;

        [RubyConstant("MAXLCODES")]
        public const int MAXLCODES = 286;

        [RubyConstant("MAXDCODES")]
        public const int MAXDCODES = 30;

        [RubyConstant("MAXCODES")]
        public const int MAXCODES = (MAXLCODES + MAXDCODES);

        [RubyConstant("FIXLCODES")]
        public const int FIXLCODES = 288;

        [RubyConstant("MAX_WBITS")]
        public const int MAX_WBITS = 15;

        [RubyConstant("Z_DEFLATED")]
        public const int Z_DEFLATED = 8;

        [RubyConstant("BINARY")]
        public const int BINARY = 0;

        [RubyConstant("ASCII")]
        public const int ASCII = 1;

        [RubyConstant("UNKNOWN")]
        public const int UNKNOWN = 2;

        [RubyConstant("NO_COMPRESSION")]
        public const int NO_COMPRESSION = 0;

        [RubyConstant("BEST_SPEED")]
        public const int BEST_SPEED = 1;

        [RubyConstant("BEST_COMPRESSION")]
        public const int BEST_COMPRESSION = 9;

        [RubyConstant("DEFAULT_COMPRESSION")]
        public const int DEFAULT_COMPRESSION = -1;

        [RubyConstant("FILTERED")]
        public const int FILTERED = 1;

        [RubyConstant("HUFFMAN_ONLY")]
        public const int HUFFMAN_ONLY = 2;

        [RubyConstant("DEFAULT_STRATEGY")]
        public const int DEFAULT_STRATEGY = 0;
     
        #endregion

#if !SILVERLIGHT
        [RubyMethod("crc32", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int GetCrc(RubyModule/*!*/ self) {
            return 0;
        }

        [RubyMethod("crc32", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object GetCrc(RubyModule/*!*/ self, [Optional, DefaultProtocol]MutableString str, [Optional]int initialCrc) {
            byte[] bytes;
            if (str == null) {
                bytes = new byte[0];
            } else {
                bytes = str.ToByteArray();
            }
            uint result = Deflate.ZDeflateStream.UpdateCrc(unchecked((uint)initialCrc), bytes, 0, bytes.Length);
            return Protocols.Normalize(result);
        }
#endif

        #region ZStream class

        [RubyClass("ZStream")]
        public class ZStream {
            protected readonly List<byte>/*!*/ _inputBuffer;
            protected readonly List<byte>/*!*/ _outputBuffer;
            protected int _outPos = -1;
            protected int _inPos = -1;
            protected byte _bitBucket = 0;
            protected byte _bitCount = 0;
            protected bool _closed = false;

            public ZStream() {
                _outPos = -1;
                _inPos = -1;
                _bitBucket = 0;
                _bitCount = 0;
                _inputBuffer = new List<byte>();
                _outputBuffer = new List<byte>();
            }

            #region instance methods
            public bool Close() {
                _closed = true;
                return _closed;
            }
            #endregion

            [RubyMethod("adler")]
            public static int Adler(ZStream/*!*/ self) {
                throw new NotImplementedError();
            }

            [RubyMethod("avail_in")]
            public static int AvailIn(ZStream/*!*/ self) {
                return self._inputBuffer.Count - self._inPos;
            }

            [RubyMethod("avail_out")]
            public static int GetAvailOut(ZStream/*!*/ self) {
                return self._outputBuffer.Count - self._outPos;
            }

            [RubyMethod("avail_out=")]
            public static int SetAvailOut(ZStream/*!*/ self, int size) {
                self._outputBuffer.Capacity = size;
                return self._outputBuffer.Count;
            }

            [RubyMethod("finish")]
            [RubyMethod("close")]
            public static bool Close(ZStream/*!*/ self) {
                return self.Close();
            }

            [RubyMethod("stream_end?")]
            [RubyMethod("finished?")]
            [RubyMethod("closed?")]
            public static bool IsClosed(ZStream/*!*/ self) {
                return self._closed;
            }

            [RubyMethod("data_type")]
            public static void DataType(ZStream/*!*/ self) {
                throw new NotImplementedException();
            }

            [RubyMethod("flush_next_in")]
            public static List<byte> FlushNextIn(ZStream/*!*/ self) {
                self._inPos = self._inputBuffer.Count;
                return self._inputBuffer;
            }

            [RubyMethod("flush_next_out")]
            public static List<byte> FlushNextOut(ZStream/*!*/ self) {
                self._outPos = self._outputBuffer.Count;
                return self._outputBuffer;
            }

            [RubyMethod("reset")]
            public static void Reset(ZStream/*!*/ self) {
                self._outPos = -1;
                self._inPos = -1;
                self._inputBuffer.Clear();
                self._outputBuffer.Clear();
            }

            [RubyMethod("total_in")]
            public static int TotalIn(ZStream/*!*/ self) {
                return self._inputBuffer.Count;
            }

            [RubyMethod("total_out")]
            public static int TotalOut(ZStream/*!*/ self) {
                return self._outputBuffer.Count;
            }

            protected int GetBits(int need) {
                int val = _bitBucket;
                while (_bitCount < need) {
                    val |= (int)(_inputBuffer[++_inPos] << _bitCount);
                    _bitCount += 8;
                }

                _bitBucket = (byte)(val >> need);
                _bitCount -= (byte)need;
                return (val & ((1 << need) - 1));
            }
        }

        #endregion

        #region Inflate class

        [RubyClass("Inflate")]
        public class Inflate : ZStream {
            private int _wBits;
            private bool _rawDeflate;
            private HuffmanTree _fixedLengthCodes;
            private HuffmanTree _fixedDistanceCodes;
            private HuffmanTree _dynamicLengthCodes;
            private HuffmanTree _dynamicDistanceCodes;

            public Inflate() 
                : this(MAX_WBITS) {
            }

            public Inflate(int windowBits) {
                _wBits = windowBits;
                if (_wBits < 0) {
                    _rawDeflate = true;
                    _wBits *= -1;
                }
            }

            #region Private Implementation Details

            private sealed class HuffmanTree {
                internal readonly List<int>/*!*/ Count;
                internal readonly List<int>/*!*/ Symbol;

                internal HuffmanTree() {
                    Count = new List<int>();
                    Symbol = new List<int>();
                }
            }

            private void DynamicCodes() {
                byte[] order = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
                int nlen = (int)GetBits(5) + 257;
                int ndist = (int)GetBits(5) + 1;
                int ncode = (int)GetBits(4) + 4;

                List<int> lengths = new List<int>();

                _dynamicLengthCodes = new HuffmanTree();
                _dynamicDistanceCodes = new HuffmanTree();

                if (nlen > MAXLCODES || ndist > MAXDCODES) {
                    throw new DataError("too many length or distance codes");
                }

                int idx = 0;

                while (idx < ncode) {
                    SetOrExpand(lengths, order[idx], GetBits(3));
                    idx++;
                }

                while (idx < 19) {
                    SetOrExpand(lengths, order[idx], 0);
                    idx++;
                }

                int err = ConstructTree(_dynamicLengthCodes, lengths, 18);
                if (err != 0) {
                    throw new DataError("code lengths codes incomplete");
                }

                idx = 0;

                while (idx < (nlen + ndist)) {
                    int symbol = Decode(_dynamicLengthCodes);
                    if (symbol < 16) {
                        SetOrExpand(lengths, idx, symbol);
                        idx++;
                    } else {
                        int len = 0;
                        if (symbol == 16) {
                            if (idx == 0) {
                                throw new DataError("repeat lengths with no first length");
                            }
                            len = lengths[idx - 1];
                            symbol = 3 + (int)GetBits(2);
                        } else if (symbol == 17) {
                            symbol = 3 + (int)GetBits(3);
                        } else if (symbol == 18) {
                            symbol = 11 + (int)GetBits(7);
                        } else {
                            throw new DataError("invalid repeat length code");
                        }

                        if ((idx + symbol) > (nlen + ndist)) {
                            throw new DataError("repeat more than specified lengths");
                        }

                        while (symbol != 0) {
                            SetOrExpand(lengths, idx, len);
                            idx++;
                            symbol--;
                        }
                    }
                }

                err = ConstructTree(_dynamicLengthCodes, lengths, nlen - 1);
                if (err < 0 || (err > 0 && (nlen - _dynamicLengthCodes.Count[0] != 1))) {
                    throw new DataError("invalid literal/length code lengths");
                }

                lengths.RemoveRange(0, nlen);

                err = ConstructTree(_dynamicDistanceCodes, lengths, ndist - 1);
                if (err < 0 || (err > 0 && (ndist - _dynamicDistanceCodes.Count[0] != 1))) {
                    throw new DataError("invalid distance code lengths");
                }

                Codes(_dynamicLengthCodes, _dynamicDistanceCodes);
            }

            private void NoCompression() {
                _bitBucket = 0;
                _bitCount = 0;

                if (_inPos + 4 > _inputBuffer.Count) {
                    throw new DataError("not enough input to read length code");
                }

                int length = (int)(_inputBuffer[++_inPos] | (_inputBuffer[++_inPos] << 8));
                int lengthComplement = (int)(_inputBuffer[++_inPos] | (_inputBuffer[++_inPos] << 8));
                if (unchecked((ushort)length) != unchecked((ushort)(~lengthComplement))) {
                    throw new DataError("invalid stored block lengths");
                }

                if (_inPos + length > _inputBuffer.Count) {
                    throw new DataError("ran out of input");
                }

                _outputBuffer.AddRange(_inputBuffer.GetRange(_inPos + 1, length));
                _inPos += length;
                _outPos += length;
            }

            private void FixedCodes() {
                if (_fixedLengthCodes == null && _fixedDistanceCodes == null) {
                    GenerateHuffmans();
                }
                Codes(_fixedLengthCodes, _fixedDistanceCodes);
            }

            private void GenerateHuffmans() {
                List<int> lengths = new List<int>(300);
                int x = 0;
                for (; x < 144; x++) {
                    lengths.Add(8);
                }
                for (; x < 256; x++) {
                    lengths.Add(9);
                }
                for (; x < 280; x++) {
                    lengths.Add(7);
                }
                for (; x < 288; x++) {
                    lengths.Add(8);
                }
                _fixedLengthCodes = new HuffmanTree();
                ConstructTree(_fixedLengthCodes, lengths, 287);

                lengths.Clear();

                for (int y = 0; y < 30; y++) {
                    lengths.Add(5);
                }

                _fixedDistanceCodes = new HuffmanTree();
                ConstructTree(_fixedDistanceCodes, lengths, 29);
            }

            private int ConstructTree(HuffmanTree/*!*/ tree, List<int>/*!*/ lengths, int symbols) {
                List<int> offs = new List<int>();

                for (int x = 0; x <= MAXBITS; x++) {
                    SetOrExpand(tree.Count, x, 0);
                }

                for (int y = 0; y <= symbols; y++) {
                    (tree.Count[lengths[y]])++;
                }

                if (tree.Count[0] == symbols) {
                    return 0;
                }

                int left = 1;
                for (int y = 1; y <= MAXBITS; y++) {
                    left <<= 1;
                    left -= tree.Count[y];
                    if (left < 0) {
                        return left;
                    }
                }

                offs.Add(0);
                offs.Add(0);

                for (int len = 1; len <= MAXBITS - 1; len++) {
                    offs.Add(0);
                    offs[len + 1] = offs[len] + tree.Count[len];
                }

                for (int symbol = 0; symbol <= symbols; symbol++) {
                    if (lengths[symbol] != 0) {
                        SetOrExpand(tree.Symbol, offs[lengths[symbol]], symbol);
                        offs[lengths[symbol]]++;
                    }
                }

                return left;
            }

            private void SetOrExpand<T>(List<T>/*!*/ list, int index, T item) {
                int minCount = index + 1;
                int expand = minCount - list.Count;
                while (expand > 0) {
                    list.Add(default(T));
                    expand--;
                }
                list[index] = item;
            }

            private int Codes(HuffmanTree/*!*/ lengthCodes, HuffmanTree/*!*/ distanceCodes) {
                int[] lens = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258 };
                int[] lext = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };
                int[] dists = { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };
                int[] dext = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

                int symbol = 0;
                while (symbol != 256) {
                    symbol = Decode(lengthCodes);
                    if (symbol < 0) {
                        return symbol;
                    }
                    if (symbol < 256) {
                        SetOrExpand(_outputBuffer, ++_outPos, (byte)symbol);
                    }
                    if (symbol > 256) {
                        symbol -= 257;
                        if (symbol >= 29) {
                            throw new DataError("invalid literal/length or distance code in fixed or dynamic block");
                        }
                        int len = lens[symbol] + GetBits((byte)lext[symbol]);
                        symbol = Decode(distanceCodes);
                        if (symbol < 0) {
                            return symbol;
                        }
                        int dist = dists[symbol] + GetBits((byte)dext[symbol]);
                        if (dist > _outputBuffer.Count) {
                            throw new DataError("distance is too far back in fixed or dynamic block");
                        }
                        while (len > 0) {
                            SetOrExpand(_outputBuffer, ++_outPos, _outputBuffer[_outPos - dist]);
                            len--;
                        }
                    }
                }

                return 0;
            }

            private int Decode(HuffmanTree/*!*/ tree) {
                int code = 0;
                int first = 0;
                int index = 0;
                for (int len = 1; len <= 15; len++) {
                    code |= GetBits(1);
                    int count = tree.Count[len];
                    if (code < (first + count)) {
                        return tree.Symbol[index + (code - first)];
                    }
                    index += count;
                    first += count;
                    first <<= 1;
                    code <<= 1;
                }

                return -9;
            }

            #endregion

            [RubyMethod("inflate")]
            public static MutableString/*!*/ InflateString(Inflate/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ zstring) {
                if (zstring.IsEmpty) {
                    throw new BufError("buffer error");
                }
                
                // TODO: hack
                if (zstring.GetByteCount() == 6 && zstring.GetByte(0) == (byte)'X' && zstring.GetByte(1) == 0x85 && 
                    zstring.GetByte(2) == 0 && zstring.GetByte(3) == 0 && zstring.GetByte(4) == 0 && zstring.GetByte(5) == 0) {
                    return MutableString.CreateEmpty();
                }

                self._inputBuffer.AddRange(zstring.ConvertToBytes());

                if (self._rawDeflate == false) {
                    byte compression_method_and_flags = self._inputBuffer[++(self._inPos)];
                    byte flags = self._inputBuffer[++(self._inPos)];
                    if (((compression_method_and_flags << (byte)0x08) + flags) % (byte)31 != 0) {
                        throw new DataError("incorrect header check");
                    }

                    byte compression_method = (byte)(compression_method_and_flags & (byte)0x0F);
                    if (compression_method != Z_DEFLATED) {
                        throw new DataError("unknown compression method");
                    }

                    byte compression_info = (byte)(compression_method_and_flags >> (byte)0x04);
                    if ((compression_info + 8) > self._wBits) {
                        throw new DataError("invalid window size");
                    }

                    bool preset_dictionary_flag = ((flags & 0x20) >> 0x05 == 1);
                    // TODO: ??? 
                    // byte compression_level = (byte)((flags & 0xC0) >> (byte)0x06);

                    //TODO: Add Preset Dictionary Support
                    if (preset_dictionary_flag) {
                        self._inPos += 4;
                    }
                }

                bool last_block = false;

                while (!last_block) {
                    last_block = (self.GetBits(1) == 1);
                    byte block_type = (byte)self.GetBits(2);
                    switch (block_type) {
                        case 0:
                            self.NoCompression();
                            break;
                        case 1:
                            self.FixedCodes();
                            break;
                        case 2:
                            self.DynamicCodes();
                            break;
                        case 3:
                            throw new DataError("invalid block type");
                    }
                }

                return Inflate.Close(self);
            }

            [RubyMethod("inflate", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ InflateString(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ zstring) {
                return InflateString(new Inflate(), zstring);
            }

            [RubyMethod("close")]
            public static MutableString/*!*/ Close(Inflate/*!*/ self) {
                return MutableString.CreateBinary(self._outputBuffer, RubyEncoding.Binary);
            }
        }

        #endregion

        #region GzipFile class

        [RubyClass("GzipFile")]
        public class GZipFile {
            protected IOWrapper/*!*/ _ioWrapper;
            protected List<byte>/*!*/ _inputBuffer;
            protected List<byte>/*!*/ _outputBuffer;
            protected int _outPos;
            protected int _inPos;
            protected bool _isClosed;
            protected MutableString _originalName;
            protected MutableString _comment;

            public GZipFile(IOWrapper/*!*/ ioWrapper) {
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

            [RubyMethod("xtra_field")]
            public static MutableString ExtraField(GZipReader/*!*/ self) {
                return self._xtraField;
            }

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

            private bool IsBitSet(byte b, byte bit) {
                return ((b & (1 << bit)) == (1 << bit));
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
                    return new GZipReader(stream, reader);
                }
            }

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

            private GZipReader(IOWrapper/*!*/ ioWrapper, BinaryReader/*!*/ reader)
                : base(ioWrapper) {

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

            [RubyMethod("read")]
            public static MutableString/*!*/ Read(GZipReader/*!*/ self) {
                Inflate z = new Inflate(-MAX_WBITS);
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

        #region Exceptions

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
            public BufError(string message, Exception inner) : base(message ?? "BufError", inner) { }

#if !SILVERLIGHT
            protected BufError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
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

        #region Deflate class
#if !SILVERLIGHT
        [RubyClass("Deflate", BuildConfig="!SILVERLIGHT")]
        public class Deflate : ZStream {
            /// <summary>
            /// Adds a 2 byte header, and a 4 byte adler checksum footer.
            /// </summary>
            internal class ZDeflateStream : DeflateStream {
                private long _size;
                private uint _crc;
                private bool _leaveOpen;
                private Stream _output;

                public ZDeflateStream(Stream output, bool leaveOpen)
                    : base(output, CompressionMode.Compress, true) {
                    _output = output;
                    _leaveOpen = leaveOpen;

                    // System.IO.Compression.DeflateStream uses a window size of 8K and FLEVEL is 2 (default algorithm).
                    byte[] header = { 0x58, 0x85 };
                    _output.Write(header, 0, header.Length);
                }

                public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState) {
                    IAsyncResult result = base.BeginWrite(array, offset, count, asyncCallback, asyncState);
                    _size += count;
                    _crc = UpdateCrc(_crc, array, offset, count);
                    return result;
                }

                public override void Write(byte[] array, int offset, int count) {
                    base.Write(array, offset, count);
                    _size += count;
                    _crc = UpdateCrc(_crc, array, offset, count);
                }

                protected override void Dispose(bool disposing) {
                    base.Dispose(disposing);
                    if (disposing && _output != null) {
                        _output.WriteByte((byte)(_crc & 0xff));
                        _output.WriteByte((byte)((_crc >> 8) & 0xff));
                        _output.WriteByte((byte)((_crc >> 16) & 0xff));
                        _output.WriteByte((byte)((_crc >> 24) & 0xff));
                        if (!_leaveOpen) _output.Close();
                        _output = null;
                    }
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
            }

            public Deflate()
                : this(-1, -1, -1, -1) {
            }

            public Deflate(int level)
                : this(level, -1, -1, -1) {
            }

            public Deflate(int level, int windowBits)
                : this(level, windowBits, -1, -1) {
            }

            public Deflate(int level, int windowBits, int memlevel)
                : this(level, windowBits, memlevel, -1) {
            }

            public Deflate(int level, int windowBits, int memlevel, int strategy) {
                // TODO: use parameters
            }

            [RubyMethod("deflate")]
            public static MutableString/*!*/ DeflateString(Deflate/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str, int flush) {
                if (flush != FINISH) {
                    throw new NotImplementedError("flush can only be FINISH");
                }

                MutableStringStream inputStream = new MutableStringStream(str);
                MutableStringStream outputStream = new MutableStringStream();
                ZDeflateStream compressedZipStream = new ZDeflateStream(outputStream, false);

                int remainingInputSize = str.Length;
                byte[] inputDataBlock = new byte[Math.Min(0x1000, remainingInputSize)];
                while (remainingInputSize > 0) {
                    int count = inputStream.Read(inputDataBlock, 0, inputDataBlock.Length);
                    compressedZipStream.Write(inputDataBlock, 0, count);
                    remainingInputSize -= count;
                }
                compressedZipStream.Close();
                return outputStream.String;
            }

            [RubyMethod("deflate", RubyMethodAttributes.PublicSingleton)]
            public static MutableString/*!*/ DeflateString(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {
                return DeflateString(new Deflate(), str, FINISH);
            }
        }
#endif
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

            private GzipWriter(RespondToStorage/*!*/ respondToStorage, RubyContext/*!*/ context, IOWrapper/*!*/ ioWrapper, int level, int strategy) 
                : base(ioWrapper) {
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
                
                return new GzipWriter(respondToStorage, self.Context, ioWrapper, level, strategy);
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
                switch (flush) {
                    case NO_FLUSH:
                    case SYNC_FLUSH:
                    case FULL_FLUSH:
                    case FINISH:
                        self._gzipStream.Flush();
                        self._ioWrapper.Flush(flushStorage, context);
                        break;

                    default:
                        throw new StreamError("stream error");
                }
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
    }
}
