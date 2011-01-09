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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Math;

namespace IronRuby.Builtins {
    public static class RubyEncoder {
        #region UTF8

        private static void GetCodePointByteCount(int codepoint, out int count, out int mark) {
            if (codepoint < 0x80) {
                count = 1;
                mark = 0;
            } else if (codepoint < 0x800) {
                count = 2;
                mark = 0xc0;
            } else if (codepoint < 0x10000) {
                count = 3;
                mark = 0xe0;
            } else if (codepoint < 0x200000) {
                count = 4;
                mark = 0xf0;
            } else if (codepoint < 0x4000000) {
                count = 5;
                mark = 0xf8;
            } else {
                count = 6;
                mark = 0xfc;
            }
        }

        private static void WriteUtf8CodePoint(Stream/*!*/ stream, int codepoint) {
            //       0 -       7F  0xxxxxxx
            //      80 -      7FF  110yyyxx 10xxxxxx
            //     800 -     FFFF  1110yyyy 10yyyyxx 10xxxxxx
            //   10000 -   1FFFFF  11110zzz 10zzyyyy 10yyyyxx 10xxxxxx
            //  200000 -  3FFFFFF  111110vv 10vvvvzz 10zzzzyy 10yyyyyy 10xxxxxx
            // 4000000 - 7FFFFFFF  1111110v 10vvvvuu 10uuuuzz 10zzzzyy 10yyyyxx 10xxxxxx
            if (codepoint < 0) {
                throw RubyExceptions.CreateRangeError("pack(U): value out of range");
            }
            int count, mark;
            GetCodePointByteCount(codepoint, out count, out mark);
            
            stream.WriteByte((byte)((codepoint >> (6 * (count - 1))) | mark));
            switch (count) {
                case 6: stream.WriteByte((byte)((codepoint >> 24) & 0x3f | 0x80)); goto case 5;
                case 5: stream.WriteByte((byte)((codepoint >> 18) & 0x3f | 0x80)); goto case 4;
                case 4: stream.WriteByte((byte)((codepoint >> 12) & 0x3f | 0x80)); goto case 3;
                case 3: stream.WriteByte((byte)((codepoint >> 6) & 0x3f | 0x80)); goto case 2;
                case 2: stream.WriteByte((byte)(codepoint & 0x3f | 0x80)); break;
            }
        }

        private static int ReadUtf8CodePoint(MutableString/*!*/ data, ref int index) {
            int length = data.GetByteCount();
            if (index >= length) {
                return -1;
            }
            int b = data.GetByte(index++);
            int count, mask;
            if ((b & 0x80) == 0) {
                count = 1;
                mask = 0xff;
            } else if ((b & 0xe0) == 0xc0) {
                count = 2;
                mask = 0x1f;
            } else if ((b & 0xf0) == 0xe0) {
                count = 3;
                mask = 0x0f;
            } else if ((b & 0xf8) == 0xf0) {
                count = 4;
                mask = 0x07;
            } else if ((b & 0xfc) == 0xf8) {
                count = 5;
                mask = 0x03;
            } else if ((b & 0xfe) == 0xfc) {
                count = 6;
                mask = 0x01;
            } else {
                throw RubyExceptions.CreateArgumentError("malformed UTF-8 character");
            }

            int codepoint = b & mask;
            for (int i = 1; i < count; i++) {
                if (index >= length) {
                    throw RubyExceptions.CreateArgumentError(
                        "malformed UTF-8 character (expected {0} bytes, given {1} bytes)", count, i
                    );
                }
                b = data.GetByte(index++);
                if ((b & 0xc0) != 0x80) {
                    throw RubyExceptions.CreateArgumentError("malformed UTF-8 character");
                }

                codepoint = (codepoint << 6) | (b & 0x3f);
            }

            int requiredCount;
            int mark;
            GetCodePointByteCount(codepoint, out requiredCount, out mark);
            if (requiredCount != count) {
                throw RubyExceptions.CreateArgumentError("redundant UTF-8 sequence");
            }
            return codepoint;
        }

        #endregion

        #region BER

        private static void WriteBer(Stream/*!*/ stream, IntegerValue value) {
            if (value.IsFixnum) {
                if (value.Fixnum < 0) {
                    throw RubyExceptions.CreateArgumentError("pack(w): value out of range");
                }
                int f = value.Fixnum;
                bool write = false;
                for (int shift = 28; shift > 0; shift -= 7) {
                    int b = (f >> shift) & 0x7f;
                    if (b != 0 || write) {
                        stream.WriteByte((byte)(b | (1 << 7)));
                        write = true;
                    }
                }
                stream.WriteByte((byte)(f & 0x7f));
            } else {
                BigInteger bignum = value.Bignum;
                if (bignum.Sign < 0) {
                    throw RubyExceptions.CreateArgumentError("pack(w): value out of range");
                }

                // not very efficient but good enough:
                uint[] words = bignum.GetWords();
                int i = words.Length;
                uint carry = 0;
                bool write = false;
                int shift = (Ceil(32 * i, 7) - 7) % 32;
                uint w = words[--i];
                while (true) {
                    if (shift == 0 && i == 0) {
                        stream.WriteByte((byte)(w & 0x7f));
                        break;
                    } else {
                        uint b = carry | (w >> shift) & 0x7f;
                        carry = 0;

                        write |= (b != 0);
                        if (write) {
                            stream.WriteByte((byte)(b | (1 << 7)));
                        }

                        if (shift < 7) {
                            carry = (w & (uint)((1 << shift) - 1)) << (7 - shift);
                            w = words[--i];
                        }
                    }

                    // (shift -= 7) mod 32:
                    shift = (shift + 32 - 7) % 32;
                }
            }
        }

        private static object ReadBer(MutableString/*!*/ data, ref int index) {
            int i = index;
            try {
                // skip initial zeroes:
                while (data.GetByte(i) == 0x80) { i++; }
                index = i;
                while ((data.GetByte(i) & 0x80) != 0) { i++; }
            } catch (IndexOutOfRangeException) {
                index = i;
                return null;
            }
            int byteCount = i - index + 1;

            if (byteCount <= 9) { // 9 * 7 == 63 < sizeof(long) * 8
                long result = 0;
                i = index;
                for (int j = 0; j < byteCount; j++, i++) {
                    result = (result << 7) | ((long)data.GetByte(i) & 0x7f);
                }
                index = i;
                return Protocols.Normalize(result);
            }

            uint[] words = new uint[CeilDiv(byteCount * 7, 32)];
            int shift = 0;
            int windex = 0;
            long w = 0;
            for (int j = 0; j < byteCount; j++, i--) {
                w |= ((long)data.GetByte(i) & 0x7f) << shift;
                if (shift >= 32 - 7) {
                    words[windex++] = (uint)(w & 0xffffffff);
                    w = w >> 32;
                }

                shift = (shift + 7) % 32;
            }
            if (w > 0) {
                words[windex] |= (uint)w;
            }
            index += byteCount;
            return new BigInteger(+1, words);
        }

        #endregion

        #region Base64

        private static byte[] _Base64Table = new byte[] { 
            (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', 
            (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', 
            (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'a', (byte)'b', (byte)'c', (byte)'d', 
            (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', 
            (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', 
            (byte)'y', (byte)'z', (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', 
            (byte)'8', (byte)'9', (byte)'+', (byte)'/'
        };

        public static void WriteBase64(Stream/*!*/ stream, MutableString/*!*/ str, int bytesPerLine) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(str, "str");
            ContractUtils.Requires(bytesPerLine > 2, "bytesPerLine");

            bytesPerLine = bytesPerLine - bytesPerLine % 3;
            int a, b, c;
            int length = str.GetByteCount();
            int remainingBytes = length % 3;
            int triples = length - remainingBytes;
            byte[] table = _Base64Table;
            int lineLength = 0;
            for (int i = 0; i < triples; i += 3) {
                a = str.GetByte(i);
                b = str.GetByte(i + 1);
                c = str.GetByte(i + 2);

                stream.WriteByte(table[(a & 0xfc) >> 2]);
                stream.WriteByte(table[((a & 3) << 4) | ((b & 240) >> 4)]);
                stream.WriteByte(table[((b & 15) << 2) | ((c & 0xc0) >> 6)]);
                stream.WriteByte(table[c & 0x3f]);
                lineLength += 3;

                if (lineLength == bytesPerLine) {
                    stream.WriteByte((byte)'\n');
                    lineLength = 0;
                }
            }

            if (remainingBytes == 0) {
                if (lineLength != 0) {
                    stream.WriteByte((byte)'\n');
                }
                return;
            }

            a = str.GetByte(triples);
            stream.WriteByte(table[(a & 0xfc) >> 2]);
            switch (remainingBytes) {
                case 1:
                    stream.WriteByte(table[(a & 3) << 4]);
                    stream.WriteByte((byte)'=');
                    break;

                case 2:
                    b = str.GetByte(triples + 1);
                    stream.WriteByte(table[((a & 3) << 4) | ((b & 240) >> 4)]);
                    stream.WriteByte(table[(b & 15) << 2]);
                    break;
            }
            stream.WriteByte((byte)'=');
            stream.WriteByte((byte)'\n');
        }
        
        private static MutableString/*!*/ ReadBase64(MutableString/*!*/ data, ref int offset) {
            int length = data.GetByteCount();
            var result = MutableString.CreateBinary();

            while (true) {
                // The following should exactly match MRI handling of incomplete input:
                int a = DecodeBase64Byte(data, length, true, ref offset);
                int b = DecodeBase64Byte(data, length, true, ref offset);
                int c = DecodeBase64Byte(data, length, false, ref offset);
                int d = (c != -2) ? DecodeBase64Byte(data, length, false, ref offset) : -2;
                if (a == -1 || b == -1 || c == -1 || d == -1) {
                    break;
                }

                int buffer = (a << 18) | (b << 12);
                result.Append((byte)((buffer >> 16) & 0xff));

                if (c == -2) {
                    break;
                }
                buffer |= (c << 6);
                result.Append((byte)((buffer >> 8) & 0xff));

                if (d == -2) {
                    break;
                }
                buffer |= d;
                result.Append((byte)(buffer & 0xff));
            }

            return result;
        }

        private static int DecodeBase64Byte(MutableString/*!*/ data, int length, bool skipEquals, ref int offset) {
            while (true) {
                if (offset >= length) {
                    return -1;
                }

                int c = data.GetByte(offset++);
                if (unchecked((uint)c - 'A' <= (uint)'Z' - 'A')) {
                    return c - 'A';
                }

                if (unchecked((uint)c - 'a' <= (uint)'z' - 'a')) {
                    return c - 'a' + 'Z' - 'A' + 1;
                }

                if (unchecked((uint)c - '0' <= (uint)'9' - '0')) {
                    return c - '0' + 2 * ('Z' - 'A' + 1);
                }

                switch (c) {
                    case '+': return 62;
                    case '/': return 63;
                    case '=':
                        if (skipEquals) {
                            break;
                        }
                        offset--;
                        return -2;
                }
            }
        }

        #endregion

        #region Printed Quotable

        /// <summary>
        /// Printable characters: [33, 60], [62, 126], space (32), tab (9) but not at the end of line
        /// Escaped: others =XX
        /// Soft eolns (could be inserted anywhere, Ruby: after each count + 1 characters): =\n
        /// </summary>
        private static void WritePrintedQuotable(Stream/*!*/ stream, MutableString/*!*/ str, int bytesPerLine) {
            bytesPerLine++;
            int lineLength = 0;
            int length = str.GetByteCount();
            for (int i = 0; i < length; i++) {
                byte c = str.GetByte(i);
                if (c >= 33 && c <= 60 || c >= 62 && c <= 126 || c == 9 || c == 32) {
                    stream.WriteByte(c);
                    lineLength++;
                } else if (c == (byte)'\n') {
                    stream.WriteByte(c);
                    lineLength = 0;
                } else {
                    stream.WriteByte((byte)'=');
                    stream.WriteByte((byte)(c >> 4).ToUpperHexDigit());
                    stream.WriteByte((byte)(c & 0x0f).ToUpperHexDigit());
                    lineLength += 3;
                }

                if (lineLength >= bytesPerLine) {
                    stream.WriteByte((byte)'=');
                    stream.WriteByte((byte)'\n');
                    lineLength = 0;
                }
            }

            if (lineLength > 0) {
                stream.WriteByte((byte)'=');
                stream.WriteByte((byte)'\n');
            }
        }

        private static MutableString/*!*/ ReadQuotedPrintable(MutableString/*!*/ data, ref int index) {
            MutableString result = MutableString.CreateBinary();
            int length = data.GetByteCount();
            int i = index;
            while (i < length) {
                byte c = data.GetByte(i++);
                if (c == '=') {
                    if (i >= length) {
                        break;
                    }

                    c = data.GetByte(i);
                    if (c == '\n') {
                        i++;
                        continue;
                    } 
                    if (c == '\r' && i + 1 < length && data.GetByte(i + 1) == '\n') {
                        i += 2;
                        continue;
                    }

                    int hi = Tokenizer.ToDigit(c);
                    if (hi >= 16) {
                        break;
                    }
                    i++;

                    if (i >= length) {
                        break;
                    }

                    int lo = Tokenizer.ToDigit(data.GetByte(i));
                    if (lo >= 16) {
                        break;
                    }

                    i++;
                    result.Append((byte)((hi << 4) | lo));
                } else {
                    result.Append(c);
                }
            }
            index = i;
            return result;
        }

        #endregion

        #region UU (Unix-Unix)

        private const char UUEncodeZero = '`';

        private static void EncodeUU(byte[]/*!*/ input, int bytesPerLine, Stream/*!*/ output) {
            ContractUtils.RequiresNotNull(input, "input");
            ContractUtils.RequiresNotNull(output, "output");

            if (input.Length == 0) {
                return;
            }

            bytesPerLine = bytesPerLine - bytesPerLine % 3;

            int remains = input.Length % bytesPerLine;
            int lines = input.Length / bytesPerLine;
            int inputOffset = 0;

            // encode full lines:
            for (int i = 0; i < lines; i++) {
                output.WriteByte(EncodeUUByte(bytesPerLine));

                for (int j = 0; j < bytesPerLine / 3; j++) {
                    EncodeUUTriple(output, input[inputOffset], input[inputOffset + 1], input[inputOffset + 2]);
                    inputOffset += 3;
                }

                output.WriteByte((byte)'\n');
            }

            // encode remaining bytes (if any):
            if (remains > 0) {
                output.WriteByte(EncodeUUByte(remains));
                int triples = remains / 3;
                remains %= 3;

                // full triples:
                for (int i = 0; i < triples; i++) {
                    EncodeUUTriple(output, input[inputOffset], input[inputOffset + 1], input[inputOffset + 2]);
                    inputOffset += 3;
                }

                // remaining bytes:
                if (remains == 1) {
                    EncodeUUTriple(output, input[inputOffset], 0, 0);
                } else if (remains == 2) {
                    EncodeUUTriple(output, input[inputOffset], input[inputOffset + 1], 0);
                }

                output.WriteByte((byte)'\n');
            }
        }

        private static MutableString/*!*/ ReadUU(MutableString/*!*/ data, ref int position) {
            var input = new MutableStringStream(data);
            var output = new MutableStringStream();
            input.Position = position;
            ReadUU(input, output);
            position = (int)input.Position;
            output.Close();
            return output.String;
        }

        private static bool ReadUU(Stream/*!*/ input, Stream/*!*/ output) {
            while (true) {
                int lineLength = input.ReadByte();
                if (lineLength == -1) {
                    return true;
                }

                lineLength = DecodeUUByte(lineLength);

                int remains = lineLength % 3;
                int triples = lineLength / 3;

                for (int i = 0; i < triples; i++) {
                    int a = DecodeUUByte(input.ReadByte());
                    int b = DecodeUUByte(input.ReadByte());
                    int c = DecodeUUByte(input.ReadByte());
                    int d = input.ReadByte();
                    if (d == -1) {
                        return false;
                    }
                    d = DecodeUUByte(d);

                    output.WriteByte((byte)((a << 2 | b >> 4) & 0xff));
                    output.WriteByte((byte)((b << 4 | c >> 2) & 0xff));
                    output.WriteByte((byte)((c << 6 | d) & 0xff));
                }

                if (remains > 0) {
                    int a = DecodeUUByte(input.ReadByte());
                    int b = DecodeUUByte(input.ReadByte());
                    int c = DecodeUUByte(input.ReadByte());
                    int d = input.ReadByte();
                    if (d == -1) {
                        return false;
                    }
                    d = DecodeUUByte(d);

                    output.WriteByte((byte)(a << 2 | b >> 4));

                    if (remains == 2) {
                        output.WriteByte((byte)(b << 4 | c >> 2));
                    }
                }

                if (input.ReadByte() != '\n') {
                    return false;
                }
            }
        }

        private static void EncodeUUTriple(Stream/*!*/ output, int a, int b, int c) {
            output.WriteByte(EncodeUUByte(a >> 2));
            output.WriteByte(EncodeUUByte(((a << 4) | (b >> 4)) & 0x3f));
            output.WriteByte(EncodeUUByte(((b << 2) | (c >> 6)) & 0x3f));
            output.WriteByte(EncodeUUByte(c & 0x3f));
        }

        private static byte EncodeUUByte(int b) {
            Debug.Assert(b <= 0x3f);
            return (byte)((b == 0) ? '`' : (0x20 + b));
        }

        private static byte DecodeUUByte(int c) {
            return (byte)((c - 0x20) & 0x3f);
        }

        #endregion

        #region Binary

        private static void WriteBits(Stream/*!*/ stream, int? countDef, bool reverse, MutableString/*!*/ str) {
            int length = str.GetByteCount();
            int count = countDef ?? length;

            int bits = Math.Min(count, length);
            int paddingBits = (8 - bits % 8) % 8;

            long resultLength = stream.Length +
                (bits + paddingBits) / 8 +
                ((count - bits) + (count - bits) % 2) / 2;

            // estimate the total length:
            stream.SetLength(resultLength);

            int b = 0;
            int i = 0;
            int s = reverse ? 0 : 7;
            int ds = reverse ? +1 : -1;
            while (i < bits) {
                b |= (str.GetByte(i++) & 1) << s;
                s += ds;
                if (i % 8 == 0) {
                    stream.WriteByte((byte)b);
                    b = 0;
                    s = reverse ? 0 : 7;
                }
            }

            if (paddingBits > 0) {
                stream.WriteByte((byte)b);
            }

            stream.Position = resultLength;
        }

        private static MutableString/*!*/ ReadBits(MutableString/*!*/ data, int? bitCount, ref int offset, bool lowestFirst) {
            int inputSize = data.GetByteCount() - offset;
            int outputSize = inputSize * 8;
            if (bitCount.HasValue) {
                int c = CeilDiv(bitCount.Value, 8);
                if (c <= inputSize) {
                    inputSize = c;
                    outputSize = bitCount.Value;
                }
            }

            var result = MutableString.CreateBinary(outputSize);
            if (outputSize == 0) {
                return result;
            }

            while (true) {
                int b = data.GetByte(offset++);
                for (int i = 0; i < 8; i++) {
                    result.Append((byte)('0' + ((b >> (lowestFirst ? i : 7 - i)) & 1)));
                    if (--outputSize == 0) {
                        return result;
                    }
                }
            }
        }

        #endregion

        #region Numbers

        private static int Ceil(int n, int d) {
            return CeilDiv(n, d) * d;
        }

        private static int CeilDiv(int n, int d) {
            return (n + d - 1) / d;
        }

        // TODO: move to MutableString?

        private static ulong ReadUInt64(MutableString/*!*/ data, ref int index, bool swap) {
            int i = index;
            index += 8;
            if (swap) {
                return
                    ((ulong)data.GetByte(i + 0) << 56) |
                    ((ulong)data.GetByte(i + 1) << 48) |
                    ((ulong)data.GetByte(i + 2) << 40) |
                    ((ulong)data.GetByte(i + 3) << 32) |
                    ((ulong)data.GetByte(i + 4) << 24) |
                    ((ulong)data.GetByte(i + 5) << 16) |
                    ((ulong)data.GetByte(i + 6) << 8) |
                    ((ulong)data.GetByte(i + 7));
            } else {
                return
                    ((ulong)data.GetByte(i + 7) << 56) |
                    ((ulong)data.GetByte(i + 6) << 48) |
                    ((ulong)data.GetByte(i + 5) << 40) |
                    ((ulong)data.GetByte(i + 4) << 32) |
                    ((ulong)data.GetByte(i + 3) << 24) |
                    ((ulong)data.GetByte(i + 2) << 16) |
                    ((ulong)data.GetByte(i + 1) << 8) |
                    ((ulong)data.GetByte(i + 0));
            }
        }

        internal static void Write(Stream/*!*/ stream, ulong n, bool swap) {
            if (swap) {
                stream.WriteByte((byte)(n >> 56));
                stream.WriteByte((byte)((n >> 48) & 0xff));
                stream.WriteByte((byte)((n >> 40) & 0xff));
                stream.WriteByte((byte)((n >> 32) & 0xff));
                stream.WriteByte((byte)((n >> 24) & 0xff));
                stream.WriteByte((byte)((n >> 16) & 0xff));
                stream.WriteByte((byte)((n >> 8) & 0xff));
                stream.WriteByte((byte)(n & 0xff));
            } else {
                stream.WriteByte((byte)(n & 0xff));
                stream.WriteByte((byte)((n >> 8) & 0xff));
                stream.WriteByte((byte)((n >> 16) & 0xff));
                stream.WriteByte((byte)((n >> 24) & 0xff));
                stream.WriteByte((byte)((n >> 32) & 0xff));
                stream.WriteByte((byte)((n >> 40) & 0xff));
                stream.WriteByte((byte)((n >> 48) & 0xff));
                stream.WriteByte((byte)(n >> 56));
            }
        }

        private static uint ReadUInt32(MutableString/*!*/ data, ref int index, bool swap) {
            int i = index;
            index += 4;
            if (swap) {
                return
                    ((uint)data.GetByte(i + 0) << 24) |
                    ((uint)data.GetByte(i + 1) << 16) |
                    ((uint)data.GetByte(i + 2) << 8) |
                    ((uint)data.GetByte(i + 3));
            } else {
                return
                    ((uint)data.GetByte(i + 3) << 24) |
                    ((uint)data.GetByte(i + 2) << 16) |
                    ((uint)data.GetByte(i + 1) << 8) |
                    ((uint)data.GetByte(i + 0));
            }
        }

        internal static void Write(Stream/*!*/ stream, uint n, bool swap) {
            if (swap) {
                stream.WriteByte((byte)(n >> 24));
                stream.WriteByte((byte)((n >> 16) & 0xff));
                stream.WriteByte((byte)((n >> 8) & 0xff));
                stream.WriteByte((byte)(n & 0xff));
            } else {
                stream.WriteByte((byte)(n & 0xff));
                stream.WriteByte((byte)((n >> 8) & 0xff));
                stream.WriteByte((byte)((n >> 16) & 0xff));
                stream.WriteByte((byte)(n >> 24));
            }
        }

        private static ushort ReadUInt16(MutableString/*!*/ data, ref int index, bool swap) {
            int i = index;
            index += 2;
            if (swap) {
                return (ushort)((data.GetByte(i + 0) << 8) | data.GetByte(i + 1));
            } else {
                return (ushort)((data.GetByte(i + 1) << 8) | data.GetByte(i + 0));
            }
        }

        private static void Write(Stream/*!*/ stream, ushort n, bool swap) {
            if (swap) {
                stream.WriteByte((byte)((n >> 8) & 0xff));
                stream.WriteByte((byte)(n & 0xff));
            } else {
                stream.WriteByte((byte)(n & 0xff));
                stream.WriteByte((byte)((n >> 8) & 0xff));
            }
        }

        private static double Int64BitsToDouble(long value) {
#if SILVERLIGHT
            ulong u = unchecked((ulong)value);
            byte[] bytes = new byte[] { 
                (byte)(u & 0xff),
                (byte)((u >> 8) & 0xff), 
                (byte)((u >> 16) & 0xff), 
                (byte)((u >> 24) & 0xff), 
                (byte)((u >> 32) & 0xff), 
                (byte)((u >> 40) & 0xff), 
                (byte)((u >> 48) & 0xff), 
                (byte)(u >> 56),                 
            };

            return BitConverter.ToDouble(bytes, 0);
#else
            return BitConverter.Int64BitsToDouble(value);
#endif
        }

        private static long DoubleToInt64Bits(double value) {
#if SILVERLIGHT
            var bytes = BitConverter.GetBytes(value);
            return unchecked((long)(
                ((ulong)bytes[7] << 56) |
                ((ulong)bytes[6] << 48) |
                ((ulong)bytes[5] << 40) |
                ((ulong)bytes[4] << 32) |
                ((ulong)bytes[3] << 24) |
                ((ulong)bytes[2] << 16) |
                ((ulong)bytes[1] << 8) |
                (ulong)bytes[0]
            ));
#else
            return BitConverter.DoubleToInt64Bits(value);
#endif
        }

        private static double ReadDouble(MutableString/*!*/ data, ref int index, bool swap) {
            return Int64BitsToDouble(unchecked((long)ReadUInt64(data, ref index, swap)));
        }

        private static void WriteDouble(ConversionStorage<double>/*!*/ floatConversion,
            Stream/*!*/ stream, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                Write(stream, unchecked((ulong)DoubleToInt64Bits(Protocols.CastToFloat(floatConversion, GetPackArg(self, i + j)))), swap);
            }
        }

        private static float ReadSingle(MutableString/*!*/ data, ref int index, bool swap) {
            byte[] bytes = data.GetBinarySlice(index, sizeof(float));
            if (swap) {
                byte b = bytes[0];
                bytes[0] = bytes[3];
                bytes[3] = b;
                b = bytes[1];
                bytes[1] = bytes[2];
                bytes[2] = b;
            }
            index += 4;
            return BitConverter.ToSingle(bytes, 0);
        }

        private static void WriteSingle(ConversionStorage<double>/*!*/ floatConversion,
            Stream/*!*/ stream, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                byte[] bytes = BitConverter.GetBytes((float)Protocols.CastToFloat(floatConversion, GetPackArg(self, i + j)));
                if (swap) {
                    stream.WriteByte(bytes[3]);
                    stream.WriteByte(bytes[2]);
                    stream.WriteByte(bytes[1]);
                    stream.WriteByte(bytes[0]);
                } else {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static void WriteUInt64(ConversionStorage<IntegerValue>/*!*/ integerConversion,
            Stream/*!*/ stream, RubyArray/*!*/ self, int i, int count, bool swap) {

            for (int j = 0; j < count; j++) {
                object value = GetPackArg(self, i + j);
                if (value == null) {
                    throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
                }

                IntegerValue integer = Protocols.CastToInteger(integerConversion, value);
                ulong u;
                if (integer.IsFixnum) {
                    Write(stream, unchecked((ulong)integer.Fixnum), swap);
                } else if (integer.Bignum.Abs().AsUInt64(out u)) {
                    if (integer.Bignum.Sign < 0) {
                        u = unchecked(~u + 1);
                    }
                    Write(stream, u, swap);
                } else {
                    throw RubyExceptions.CreateRangeError("bignum out of range (-2**64, 2**64)");
                }
            }
        }

        private static void WriteUInt32(ConversionStorage<IntegerValue>/*!*/ integerConversion,
            Stream/*!*/ stream, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                long value = Protocols.CastToInt64Unchecked(integerConversion, GetPackArg(self, i + j));
                if (value <= -0x100000000L || value >= 0x100000000L) {
                    throw RubyExceptions.CreateRangeError("bignum out of range (-2**32, 2**32)");
                }
                Write(stream, unchecked((uint)value), swap);
            }
        }

        private static void WriteUInt16(ConversionStorage<IntegerValue>/*!*/ integerConversion,
            Stream/*!*/ stream, RubyArray/*!*/ self, int i, int count, bool swap) {
            for (int j = 0; j < count; j++) {
                long value = Protocols.CastToInt64Unchecked(integerConversion, GetPackArg(self, i + j));
                if (value <= -0x100000000L || value >= 0x100000000L) {
                    throw RubyExceptions.CreateRangeError("bignum out of range (-2**32, 2**32)");
                }
                Write(stream, unchecked((ushort)value), swap);
            }
        }

        private static void FromHex(Stream/*!*/ stream, MutableString/*!*/ str, int nibbleCount, bool swap) {
            int maxCount = Math.Min(nibbleCount, str.GetByteCount());
            for (int i = 0, j = 0; i < (nibbleCount + 1) / 2; i++, j += 2) {
                int hiNibble = (j < maxCount) ? FromHexDigit(str.GetByte(j)) : 0;
                int loNibble = (j + 1 < maxCount) ? FromHexDigit(str.GetByte(j + 1)) : 0;
                Debug.Assert(hiNibble >= 0 && hiNibble < 16 && loNibble >= 0 && loNibble < 16);

                int c = (swap) ? (loNibble << 4) | hiNibble : (hiNibble << 4) | loNibble;
                stream.WriteByte((byte)c);
            }
        }

        // hexa digits -> values
        private static int FromHexDigit(int c) {
            c = Tokenizer.ToDigit(c);
            if (c < 16) return c;

            // MRI does some magic here:
            throw new NotSupportedException("directives `H' and `h' expect hexadecimal digits in input string");
        }

        #endregion

        #region FormatDirective

        private struct FormatDirective {
            internal readonly char Directive;
            internal readonly int? Count; // null means *

            internal FormatDirective(char directive, int? count) {
                Directive = directive;
                Count = count;
            }

            private static char MapNative(char c, char modifier) {
                if (c == 's' || c == 'S' || c == 'i' || c == 'I') {
                    return c;
                } else if (c == 'l') {
                    return IntPtr.Size == 4 ? 'i' : 'q';
                } else if (c == 'L') {
                    return IntPtr.Size == 4 ? 'I' : 'Q';
                } else {
                    throw RubyExceptions.CreateArgumentError("'{0}' allowed only after types sSiIlL", modifier);
                }
            }

            internal static IEnumerable<FormatDirective>/*!*/ Enumerate(string/*!*/ format) {
                for (int i = 0; i < format.Length; i++) {
                    char c = format[i];
                    if (c == '%') {
                        throw RubyExceptions.CreateArgumentError("% is not supported");
                    }

                    if (c == '#') {
                        do { i++; } while (i < format.Length && format[i] != '\n');
                        continue;
                    }

                    if (!Tokenizer.IsLetter(c) && c != '@') {
                        continue;
                    }

                    i++;
                    int? count = 1;
                    char c2 = (i < format.Length) ? format[i] : '\0';
                    if (c2 == '_' || c2 == '!') {
                        char mapped = MapNative(c, c2);

                        // ignore !
                        if (c2 == '_') {
                            c = mapped;
                        }
                        i++;
                        c2 = (i < format.Length) ? format[i] : '\0';
                    }

                    if (Tokenizer.IsDecimalDigit(c2)) {
                        int pos1 = i;
                        i++;
                        while (i < format.Length && Tokenizer.IsDecimalDigit(format[i])) {
                            i++;
                        }
                        count = Int32.Parse(format.Substring(pos1, (i - pos1)));
                        i--;
                    } else if (c == '@' && c2 == '-') {
                        int pos1 = i;
                        i += 2;
                        while (i < format.Length && Tokenizer.IsDecimalDigit(format[i])) {
                            i++;
                        }
                        count = Int32.Parse(format.Substring(pos1, (i - pos1)));
                        i--;
                    } else if (c2 == '*') {
                        count = null;
                    } else {
                        i--;
                        if (c == '@') {
                            count = 0;
                        }
                    }

                    yield return new FormatDirective(c, count);
                }
            }
        }

        #endregion

        #region Pack

        public static MutableString/*!*/ Pack(
            ConversionStorage<IntegerValue>/*!*/ integerConversion,
            ConversionStorage<double>/*!*/ floatConversion,
            ConversionStorage<MutableString>/*!*/ stringCast,
            ConversionStorage<MutableString>/*!*/ tosConversion,
            RubyArray/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {

            // TODO: encodings

            using (MutableStringStream stream = new MutableStringStream()) {
                int i = 0;
                foreach (FormatDirective directive in FormatDirective.Enumerate(format.ConvertToString())) {
                    int count = directive.Count ?? self.Count - i;

                    MutableString str;
                    switch (directive.Directive) {
                        case '@':
                            count = 0;
                            stream.SetLength(stream.Position = directive.Count.HasValue ? directive.Count.Value : 1);
                            break;

                        case 'A':
                        case 'a':
                        case 'Z':
                            count = 1;
                            WriteString(stream, directive, ToMutableString(stringCast, stream, GetPackArg(self, i)));
                            break;

                        case 'B':
                        case 'b':
                            count = 1;
                            WriteBits(
                                stream,
                                directive.Count,
                                directive.Directive == 'b',
                                str = GetPackArg(self, i) != null ? ToMutableString(stringCast, stream, GetPackArg(self, i)) : MutableString.FrozenEmpty
                            );
                            break;

                        case 'c':
                        case 'C':
                            for (int j = 0; j < count; j++) {
                                stream.WriteByte(unchecked((byte)Protocols.CastToUInt32Unchecked(integerConversion, GetPackArg(self, i + j))));
                            }
                            break;

                        case 'd': // 8-byte native-endian
                        case 'D':
                            WriteDouble(floatConversion, stream, self, i, count, false);
                            break;

                        case 'e': // 4-byte little-endian
                            WriteSingle(floatConversion, stream, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 'E': // 8-byte little-endian
                            WriteDouble(floatConversion, stream, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 'f': // 4-byte native-endian
                        case 'F':
                            WriteSingle(floatConversion, stream, self, i, count, false);
                            break;

                        case 'g': // 4-byte big-endian
                            WriteSingle(floatConversion, stream, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'G': // 8-byte big-endian
                            WriteDouble(floatConversion, stream, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'h':
                        case 'H':
                            // MRI skips null, unlike in "m" directive:
                            if (GetPackArg(self, i) != null) {
                                str = ToMutableString(stringCast, stream, GetPackArg(self, i));
                                FromHex(stream, str, directive.Count ?? str.GetByteCount(), directive.Directive == 'h');
                            }
                            count = 1;
                            break;

                        case 'Q':
                        case 'q': // (un)signed 8-byte native-endian
                            WriteUInt64(integerConversion, stream, self, i, count, false);
                            break;

                        case 'l':
                        case 'i':
                        case 'L':
                        case 'I': // (un)signed 4-byte native-endian
                            WriteUInt32(integerConversion, stream, self, i, count, false);
                            break;

                        case 'N': // (un)signed 4-byte big-endian
                            WriteUInt32(integerConversion, stream, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'n': // (un)signed 2-byte big-endian
                            WriteUInt16(integerConversion, stream, self, i, count, BitConverter.IsLittleEndian);
                            break;

                        case 'V': // (un)signed 4-byte little-endian
                            WriteUInt32(integerConversion, stream, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 'v': // (un)signed 2-byte little-endian
                            WriteUInt16(integerConversion, stream, self, i, count, !BitConverter.IsLittleEndian);
                            break;

                        case 's': // (un)signed 2-byte native-endian
                        case 'S':
                            WriteUInt16(integerConversion, stream, self, i, count, false);
                            break;

                        case 'm': // Base64
                            if (GetPackArg(self, i) == null) {
                                throw RubyExceptions.CreateTypeConversionError("nil", "String");
                            }
                            WriteBase64(stream, ToMutableString(stringCast, stream, GetPackArg(self, i)),
                                (directive.Count.HasValue && directive.Count.Value > 2) ? directive.Count.Value : 45
                            );
                            count = 1;
                            break;

                        case 'M': // quoted-printable, MIME encoding
                            count = 1;
                            if (GetPackArg(self, i) != null) {
                                var site = tosConversion.GetSite(ConvertToSAction.Make(tosConversion.Context));
                                MutableString s = site.Target(site, GetPackArg(self, i));
                                stream.String.TaintBy(s);
                                WritePrintedQuotable(stream, s,
                                    (directive.Count.HasValue && directive.Count.Value >= 2) ? directive.Count.Value : 72
                                );
                            }
                            break;

                        case 'u': // UU-encoded
                            if (GetPackArg(self, i) == null) {
                                throw RubyExceptions.CreateTypeConversionError("nil", "String");
                            }
                            RubyEncoder.EncodeUU(ToMutableString(stringCast, stream, GetPackArg(self, i)).ToByteArray(), 
                                (directive.Count.HasValue && directive.Count.Value > 2) ? directive.Count.Value : 45, 
                                stream
                            );
                            count = 1;
                            break;

                        case 'w': // BER-encoded
                            for (int j = 0; j < count; j++) {
                                WriteBer(stream, Protocols.CastToInteger(integerConversion, GetPackArg(self, i + j)));
                            }
                            break;

                        case 'U': // UTF8 code point
                            for (int j = 0; j < count; j++) {
                                RubyEncoder.WriteUtf8CodePoint(stream, Protocols.CastToInteger(integerConversion, GetPackArg(self, i + j)).ToInt32());
                            }
                            break;

                        case 'X':
                            count = 0;
                            int len3 = directive.Count.HasValue ? directive.Count.Value : 0;
                            if (len3 > stream.Position) {
                                throw RubyExceptions.CreateArgumentError("X outside of string");
                            }
                            stream.String.Write((int)stream.Position - len3, 0, len3);
                            stream.Position -= len3;
                            break;

                        case 'x':
                            count = 0;
                            int len4 = directive.Count.HasValue ? directive.Count.Value : 0;
                            stream.String.Write((int)stream.Position, 0, len4);
                            stream.Position += len4;
                            break;

                        default:
                            count = 0;
                            break;
                    }

                    i += count;
                }
                stream.SetLength(stream.Position);
                return stream.String.TaintBy(format);
            }
        }

        private static MutableString ToMutableString(ConversionStorage<MutableString>/*!*/ stringCast, MutableStringStream/*!*/ stream, object value) {
            var site = stringCast.GetSite(ConvertToStrAction.Make(stringCast.Context));
            var result = site.Target(site, value);
            if (result != null) {
                stream.String.TaintBy(result);
            }
            return result;
        }

        private static object GetPackArg(RubyArray/*!*/ array, int index) {
            if (index >= array.Count) {
                throw RubyExceptions.CreateArgumentError("too few arguments");
            }
            return array[index];                    
        }

        private static void WriteString(Stream/*!*/ stream, FormatDirective directive, MutableString str) {
            // TODO (opt): unneccessary copy of byte[]
            byte[] bytes = str != null ? str.ToByteArray() : Utils.EmptyBytes;
            int dataLen;
            int paddedLen;
            if (directive.Count.HasValue) {
                paddedLen = directive.Count.Value;
                dataLen = Math.Min(bytes.Length, paddedLen);
            } else {
                paddedLen = bytes.Length;
                dataLen = bytes.Length;
            }
            stream.Write(bytes, 0, dataLen);
            if (paddedLen > dataLen) {
                byte fill = (directive.Directive == 'A') ? (byte)' ' : (byte)0;
                for (int j = 0; j < (paddedLen - dataLen); j++) {
                    stream.WriteByte(fill);
                }
            }
            if (directive.Directive == 'Z' && !directive.Count.HasValue) {
                stream.WriteByte((byte)0);
            }
        }

        #endregion

        #region Unpack

        public static RubyArray/*!*/ Unpack(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {
            RubyArray result = new RubyArray(1 + self.Length / 2);

            // TODO: encodings
            int position = 0;
            int length = self.GetByteCount();
            foreach (FormatDirective directive in FormatDirective.Enumerate(format.ToString())) {
                int count, maxCount;
                int nilCount = 0;
                switch (directive.Directive) {
                    case '@':
                        if (directive.Count.HasValue) {
                            if (directive.Count.Value > length) {
                                throw RubyExceptions.CreateArgumentError("@ outside of string");
                            }
                            position = directive.Count.Value > 0 ? directive.Count.Value : 0;
                        } else {
                            position = length;
                        }
                        break;

                    case 'A':
                    case 'a':
                    case 'Z':
                        result.Add(ReadString(self, directive.Directive, directive.Count, ref position));
                        break;

                    case 'B':
                    case 'b':
                        result.Add(ReadBits(self, directive.Count, ref position, directive.Directive == 'b'));
                        break;

                    case 'c':
                        count = CalculateCounts(length - position, directive.Count, sizeof(sbyte), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add((int)unchecked((sbyte)self.GetByte(position++)));
                        }
                        break;

                    case 'C':
                        count = CalculateCounts(length - position, directive.Count, sizeof(byte), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add((int)self.GetByte(position++));
                        }
                        break;

                    case 'd': // 8-byte native-endian
                    case 'D':
                        count = CalculateCounts(length - position, directive.Count, sizeof(double), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ReadDouble(self, ref position, false));
                        }
                        break;

                    case 'e': // 4-byte little-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(float), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add((double)ReadSingle(self, ref position, !BitConverter.IsLittleEndian));
                        }
                        break;

                    case 'E': // 8-byte little-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(double), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ReadDouble(self, ref position, !BitConverter.IsLittleEndian));
                        }
                        break;

                    case 'f': // 4-byte native-endian
                    case 'F':
                        count = CalculateCounts(length - position, directive.Count, sizeof(float), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add((double)ReadSingle(self, ref position, false));
                        }
                        break;

                    case 'g': // 4-byte big-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(float), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add((double)ReadSingle(self, ref position, BitConverter.IsLittleEndian));
                        }
                        break;

                    case 'G': // 8-byte big-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(double), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ReadDouble(self, ref position, BitConverter.IsLittleEndian));
                        }
                        break;

                    case 'i':
                    case 'l': // signed 4-byte native-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(int), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(unchecked((int)ReadUInt32(self, ref position, false)));
                        }
                        break;

                    case 'I':
                    case 'L': // unsigned 4-byte native-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(uint), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(Protocols.Normalize(ReadUInt32(self, ref position, false)));
                        }
                        break;

                    case 'N': // unsigned 4-byte big-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(uint), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(Protocols.Normalize(ReadUInt32(self, ref position, BitConverter.IsLittleEndian)));
                        }
                        break;

                    case 'n': // unsigned 2-byte big-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(ushort), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref position, BitConverter.IsLittleEndian)));
                        }
                        break;

                    case 'v': // unsigned 2-byte little-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(ushort), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref position, !BitConverter.IsLittleEndian)));
                        }
                        break;

                    case 'V': // unsigned 4-byte little-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(uint), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(Protocols.Normalize(ReadUInt32(self, ref position, !BitConverter.IsLittleEndian)));
                        }
                        break;

                    case 'q': // signed 8-byte native-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(long), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(Protocols.Normalize(unchecked((long)ReadUInt64(self, ref position, false))));
                        }
                        break;

                    case 'Q': // unsigned 8-byte native-endian
                        count = CalculateCounts(length - position, directive.Count, sizeof(ulong), out nilCount);
                        nilCount = 0;
                        for (int j = 0; j < count; j++) {
                            result.Add(Protocols.Normalize(ReadUInt64(self, ref position, false)));
                        }
                        break;

                    case 'm': // Base64
                        result.Add(ReadBase64(self, ref position));
                        break;

                    case 'M': // quoted-printable, MIME encoding
                        result.Add(ReadQuotedPrintable(self, ref position));
                        break;

                    case 's':
                        count = CalculateCounts(length - position, directive.Count, sizeof(short), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ScriptingRuntimeHelpers.Int32ToObject(unchecked((short)ReadUInt16(self, ref position, !BitConverter.IsLittleEndian))));
                        }
                        break;

                    case 'S':
                        count = CalculateCounts(length - position, directive.Count, sizeof(ushort), out nilCount);
                        for (int j = 0; j < count; j++) {
                            result.Add(ScriptingRuntimeHelpers.Int32ToObject(ReadUInt16(self, ref position, !BitConverter.IsLittleEndian)));
                        }
                        break;

                    case 'U':
                        count = directive.Count ?? Int32.MaxValue;                            
                        for (int i = 0; i < count; i++) {
                            int cp = ReadUtf8CodePoint(self, ref position);
                            if (cp == -1) {
                                break;
                            }
                            result.Add(cp);
                        }
                        break;

                    case 'w': // BER encoding
                        count = directive.Count ?? Int32.MaxValue;
                        for (int i = 0; i < count; i++) {
                            object value = ReadBer(self, ref position);

                            if (value == null) {
                                break;
                            }
                            result.Add(value);
                        }
                        break;

                    case 'u': // UU decoding                        
                        result.Add(ReadUU(self, ref position));
                        break;

                    case 'X':
                        int len3 = directive.Count.HasValue ? directive.Count.Value : (int)(length - position);
                        if (len3 > position) {
                            throw RubyExceptions.CreateArgumentError("X outside of string");
                        }
                        position -= len3;
                        break;

                    case 'x':
                        int newPos = directive.Count.HasValue ? (int)position + directive.Count.Value : (int)length;
                        if (newPos > length) {
                            throw RubyExceptions.CreateArgumentError("X outside of string");
                        }
                        position = newPos;
                        break;

                    case 'h':
                    case 'H':
                        maxCount = (int)(length - position) * 2;
                        result.Add(ToHex(self, ref position, Math.Min(directive.Count ?? maxCount, maxCount), directive.Directive == 'h'));
                        break;
                }
                for (int i = 0; i < nilCount; i++) {
                    result.Add(null);
                }
            }
            return result;
        }

        private static int CalculateCounts(int remaining, int? count, int size, out int leftover) {
            int maxCount = remaining / size;
            if (!count.HasValue) {
                leftover = 0;
                return maxCount;
            } else if (count.Value <= maxCount) {
                leftover = 0;
                return count.Value;
            } else {
                leftover = count.Value - maxCount;
                return maxCount;
            }
        }

        private static MutableString/*!*/ ToHex(MutableString/*!*/ data, ref int index, int nibbleCount, bool swap) {
            int wholeChars = nibbleCount / 2;
            MutableString hex = MutableString.CreateMutable(nibbleCount, RubyEncoding.Binary);

            for (int i = 0; i < wholeChars; i++) {
                byte b = data.GetByte(index++);
                char loNibble = (b & 0x0F).ToLowerHexDigit();
                char hiNibble = ((b & 0xF0) >> 4).ToLowerHexDigit();

                if (swap) {
                    hex.Append(loNibble);
                    hex.Append(hiNibble);
                } else {
                    hex.Append(hiNibble);
                    hex.Append(loNibble);
                }
            }

            // the last nibble:
            if ((nibbleCount & 1) != 0) {
                int b = data.GetByte(index++);
                if (swap) {
                    hex.Append((b & 0x0F).ToLowerHexDigit());
                } else {
                    hex.Append(((b & 0xF0) >> 4).ToLowerHexDigit());
                }
            }

            return hex;
        }

        private static MutableString/*!*/ ReadString(MutableString/*!*/ data, int trimMode, int? count, ref int offset) {
            int start = offset;
            int e = data.GetByteCount();
            if (count.HasValue) {
                int end = start + count.Value;
                if (end < e) {
                    e = end;
                }
            }

            offset = e;
            byte b;
            switch (trimMode) {
                case 'A':
                    while (--e >= start && ((b = data.GetByte(e)) == 0 || b == ' ')) { }
                    e++;
                    break;

                case 'Z':
                    int i = start;
                    while (i < e && data.GetByte(i) != 0) { i++; }
                    if (!count.HasValue) {
                        offset = i + 1;
                    }
                    e = i;
                    break;
            }

            return data.GetSlice(start, e - start);
        }

        #endregion
    }
}
