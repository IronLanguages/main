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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    using MSC = MutableString.Character;

    public partial class Tests {
        private MutableString/*!*/ MS(string/*!*/ data) {
            return MutableString.CreateMutable(data.Length * 3, RubyEncoding.Binary).Append(data);
        }

        private MutableString/*!*/ MS(string/*!*/ data, RubyEncoding/*!*/ e) {
            return MutableString.CreateMutable(data.Length * 3, e).Append(data);
        }

        private MutableString/*!*/ MS(byte[]/*!*/ data) {
            return MutableString.CreateBinary(data.Length * 3).Append(data);
        }

        private MutableString/*!*/ MS(byte[]/*!*/ data, RubyEncoding/*!*/ e) {
            return MutableString.CreateBinary(data.Length * 3, e).Append(data);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Factories() {
            var x = MutableString.CreateAscii("");
            Assert(!x.IsBinary && x.IsEmpty);

            var y = MutableString.Create(x);
            Assert(!x.IsBinary && y.IsEmpty && !ReferenceEquals(x, y));

            x = MutableString.Create("a", RubyEncoding.UTF8);
            Assert(!x.IsBinary && x.ToString() == "a" && x.Encoding == RubyEncoding.UTF8);

            x = MutableString.CreateMutable(RubyEncoding.UTF8);
            Assert(!x.IsBinary && x.IsEmpty && x.Encoding == RubyEncoding.UTF8);

            x = MutableString.CreateMutable("hello", RubyEncoding.UTF8);
            Assert(!x.IsBinary && x.ToString() == "hello" && x.Encoding == RubyEncoding.UTF8);

            x = MutableString.CreateMutable(100, RubyEncoding.UTF8);
            Assert(!x.IsBinary && x.IsEmpty && x.Encoding == RubyEncoding.UTF8);

            x = MutableString.CreateBinary(new byte[] { (byte)'A' });
            Assert(x.IsBinary && x.ToString() == "A" && x.Encoding == RubyEncoding.Binary);

            x = MutableString.CreateBinary(100);
            Assert(x.IsBinary && x.IsEmpty && x.Encoding == RubyEncoding.Binary);

            x = MutableString.CreateBinary(100, RubyEncoding.UTF8);
            Assert(x.IsBinary && x.IsEmpty && x.Encoding == RubyEncoding.UTF8);

            x = MutableString.CreateBinary(new List<byte>(new byte[] { 1, 2, 3 }), RubyEncoding.UTF8);
            Assert(x.IsBinary && x.GetByteCount() == 3 && x.Encoding == RubyEncoding.UTF8);
        }

        [Options(NoRuntime = true)]
        public void MutableString_GetHashCode() {
            int h1, h2;
            MutableString s;

            // binary -> invalid string:
            s = MutableString.CreateBinary(new byte[] { 0xce }, RubyEncoding.UTF8);
            h1 = s.GetHashCode();
            AssertExceptionThrown<DecoderFallbackException>(() => s.GetCharCount());

            // binary -> string:
            s = MutableString.CreateBinary(new byte[] { 0xce, 0xa3 }, RubyEncoding.UTF8);
            h1 = s.GetHashCode();
            Assert(s.GetCharCount() == 1);
            h2 = s.GetHashCode();
            Assert(h1 == h2);

            // string -> binary:
            s = MutableString.Create("Σ", RubyEncoding.UTF8);
            h1 = s.GetHashCode();

            s.GetByteCount();

            // binary -> string:
            s = MutableString.CreateBinary(new byte[] { 0xce, 0xa3 }, RubyEncoding.UTF8);
            h1 = s.GetHashCode();
            Assert(s.GetCharCount() == 1);
            h2 = s.GetHashCode();
            Assert(h1 == h2);

            // string -> binary:
            s = MutableString.Create("Σ", RubyEncoding.UTF8);
            h1 = s.GetHashCode();

            s.GetByteCount();

#if TODO
            // hash(binary) == hash(string):
            s = MutableString.CreateBinary(new byte[] { 0xce, 0xa3 }, RubyEncoding.UTF8);
            h1 = s.GetHashCode();
            s = MutableString.Create("Σ", RubyEncoding.UTF8);
            h2 = s.GetHashCode();
            AssertEquals(h1, h2);
#endif

            // same content, different capacities:
            var a = MutableString.CreateMutable(10, RubyEncoding.UTF8).Append("hello");
            var b = MutableString.CreateMutable(20, RubyEncoding.UTF8).Append("hello");
            var c = MutableString.Create("hello", RubyEncoding.UTF8);
            Assert(a.GetHashCode() == b.GetHashCode());
            Assert(a.GetHashCode() == c.GetHashCode());

            // same content, different encodings:
            // ASCII characters only:
            a = MutableString.Create("hello", RubyEncoding.UTF8);
            b = MutableString.Create("hello", RubyEncoding.SJIS);
            c = MutableString.CreateAscii("hello");
            Assert(a.GetHashCode() == b.GetHashCode());
            Assert(a.GetHashCode() == c.GetHashCode());
            Assert(b.GetHashCode() == c.GetHashCode());

            // non-ASCII characters:
            a = MutableString.Create("α", RubyEncoding.UTF8);
            b = MutableString.Create("α", RubyEncoding.SJIS);
            c = MutableString.CreateBinary(Encoding.UTF8.GetBytes("α"), RubyEncoding.Binary);
            Assert(a.GetHashCode() != b.GetHashCode());
            Assert(a.GetHashCode() != c.GetHashCode());
            Assert(b.GetHashCode() != c.GetHashCode());
        }

        [Options(NoRuntime = true)]
        public void MutableString_IsAscii() {
            var a = MutableString.CreateBinary(new byte[] { 0x12, 0x34, 0x56 }, RubyEncoding.Binary);
            Assert(a.IsAscii());
            a.Remove(2);
            Assert(a.IsAscii());
            a.Append(0x56);
            Assert(a.IsAscii());
            a.Append(0x80);
            Assert(!a.IsAscii());
            a.ConvertToString();
            Assert(!a.IsAscii());
            a.Remove(2);
            Assert(a.IsAscii());
        }

        [Options(NoRuntime = true)]
        public void MutableString_Length() {
            MutableString x;
            x = MutableString.Create("a", RubyEncoding.Binary);
            Assert(MutableStringOps.GetCharCount(x) == 1);

            x = MutableString.Create("α", RubyEncoding.UTF8);
            Assert(MutableStringOps.GetCharCount(x) == 1);

            x = MutableString.Create("α", RubyEncoding.UTF8);
            Assert(MutableStringOps.GetByteCount(x) == 2);
        }

        [Options(NoRuntime = true)]
        public void MutableString_CompareTo() {
            MutableString x, y;
            RubyEncoding SJIS = RubyEncoding.SJIS;

            // invalid bytes <=> valid string:
            var invalid = new byte[] { 0xe2, 0x85, 0x9c, 0xef };
            var alpha = new byte[] { 0xce, 0xb1 };

            x = MS(invalid, RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(Math.Sign(x.CompareTo(y)) == Math.Sign(0xe2 - 0xce));

            x = MS(invalid, RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(Math.Sign(y.CompareTo(x)) == Math.Sign(0xce - 0xe2));

            x = MS(invalid, RubyEncoding.UTF8);
            y = MS(invalid, RubyEncoding.UTF8);
            Assert(x.CompareTo(y) == 0);

            x = MS("α", RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(x.CompareTo(y) == 0);

            // encodings orderd by code-page:
            Assert(Math.Sign(RubyEncoding.UTF8.CompareTo(SJIS)) == 1);

            // difference in encodings is ignored if both strings are ascii:
            x = MS("a", RubyEncoding.UTF8);
            y = MS(new byte[] { (byte)'b' }, SJIS);
            Assert(Math.Sign(x.CompareTo(y)) == -1);

            // difference in encodings is ignored if both strings are ascii:
            x = MS("α", RubyEncoding.UTF8);
            y = MS("α", SJIS);
            Assert(Math.Sign(x.CompareTo(y)) == Math.Sign(RubyEncoding.UTF8.CompareTo(SJIS)));

            x = MS(new byte[] { (byte)'a' }, RubyEncoding.UTF8);
            y = MS("α", SJIS);
            Assert(Math.Sign(x.CompareTo(y)) == Math.Sign(RubyEncoding.UTF8.CompareTo(SJIS)));

            x = MS("α", RubyEncoding.UTF8);
            y = MS("a", SJIS);
            Assert(Math.Sign(x.CompareTo(y)) == Math.Sign(RubyEncoding.UTF8.CompareTo(SJIS)));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Append_Byte() {
            MutableString x;
            x = MutableString.CreateBinary(new byte[] { 1, 2 });
            Assert(x.GetByteCount() == 2);

            x.Append(3);
            Assert(x.GetByteCount() == 3);

            x.Append(3, 0);
            Assert(x.GetByteCount() == 3);

            x.Append(new byte[] { 4, 5 });
            Assert(x.GetByteCount() == 5);

            x.Append(new byte[] { });
            Assert(x.GetByteCount() == 5);

            x.Append(MS(new byte[] { 6 }));
            Assert(x.GetByteCount() == 6);

            x.Append(MS(new byte[] { }));
            Assert(x.GetByteCount() == 6);

            x.Append(7, 8);
            Assert(x.GetByteCount() == 14);

            x.Append(new byte[] { 14, 15, 16, 17, 18, 19, 20 }, 1, 3);
            Assert(x.GetByteCount() == 17);

            x.Append(MS(new byte[] { 14, 15, 16, 17, 18, 19, 20 }), 4, 3);
            Assert(x.GetByteCount() == 20);

            Assert(x.Equals(MS(new byte[] { 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 15, 16, 17, 18, 19, 20 })));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Append_Char() {
            var e = RubyEncoding.UTF8;

            MutableString x;
            x = MutableString.CreateMutable(e);
            Assert(x.IsEmpty);

            x.Append("αβ");
            Assert(x.GetCharCount() == 2);

            x.Append('γ');
            Assert(x.GetCharCount() == 3);

            x.Append('x', 0);
            Assert(x.GetCharCount() == 3);

            x.Append(new char[] { 'δ', 'f' });
            Assert(x.GetCharCount() == 5);

            x.Append(new char[] { });
            Assert(x.GetCharCount() == 5);

            x.Append(MS("g", e));
            Assert(x.GetCharCount() == 6);

            x.Append(MS("", e));
            Assert(x.GetCharCount() == 6);

            x.Append('h', 8);
            Assert(x.GetCharCount() == 14);

            x.Append("hijκλμν", 1, 3);
            Assert(x.GetCharCount() == 17);

            x.Append(MS("hijκλμν", e), 4, 3);
            Assert(x.GetCharCount() == 20);

            x.Append("zzzz");
            Assert(x.GetCharCount() == 24);

            Assert(x.Equals(MS("αβγδfghhhhhhhhijκλμνzzzz", e)));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Append() {
            MutableString x, y;
            
            // Appending Unicode string literal to a string doesn't check correctness of the resulting string.
            // An exception is thrown during subssequent operation that needs to convert the content.
            x = MS(((char)250).ToString(), RubyEncoding.Binary);
            x.Append('α');
            AssertExceptionThrown<EncoderFallbackException>(() => x.ToByteArray());

            // invalid bytes + valid string -> invalid bytes, but not an error:
            var invalid_utf8 = new byte[] { 0xe2, 0x85, 0x9c, 0xef };
            var valid_utf8 = Encoding.UTF8.GetBytes("α");

            x = MS(invalid_utf8, RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(x.Append(y).ToByteArray().ValueEquals(Utils.Concatenate(invalid_utf8, valid_utf8)));

            x = MS(invalid_utf8, RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(y.Append(x).ToByteArray().ValueEquals(Utils.Concatenate(valid_utf8, invalid_utf8)));

            x = MS(invalid_utf8, RubyEncoding.UTF8);
            Assert(x.Append(x).ToByteArray().ValueEquals(Utils.Concatenate(invalid_utf8, invalid_utf8)));


            x = MS(invalid_utf8, RubyEncoding.UTF8);
            y = MS("βαγ", RubyEncoding.UTF8);
            Assert(x.Append(y, 1, 1).ToByteArray().ValueEquals(Utils.Concatenate(invalid_utf8, valid_utf8)));

            x = MS(invalid_utf8, RubyEncoding.UTF8);
            y = MS("α", RubyEncoding.UTF8);
            Assert(y.Append(x, 1, 2).ToByteArray().ValueEquals(Utils.Concatenate(valid_utf8, new byte[] { 0x85, 0x9c })));

            x = MS(invalid_utf8, RubyEncoding.UTF8);
            Assert(x.Append(x, 1, 2).ToByteArray().ValueEquals(Utils.Concatenate(invalid_utf8, new byte[] { 0x85, 0x9c })));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Insert_Byte() {
            MutableString x;
            x = MutableString.CreateBinary(new byte[] { 1, 2 });
            x.Insert(0, 3);
            Assert(x.CompareTo(MS(new byte[] { 3, 1, 2 })) == 0);
            x.Insert(3, 4);
            Assert(x.CompareTo(MS(new byte[] { 3, 1, 2, 4 })) == 0);
            x.Insert(2, new byte[] { });
            Assert(x.CompareTo(MS(new byte[] { 3, 1, 2, 4 })) == 0);
            x.Insert(1, new byte[] { 5, 6, 7 });
            Assert(x.CompareTo(MS(new byte[] { 3, 5, 6, 7, 1, 2, 4 })) == 0);
            x.Insert(0, new byte[] { 8 }, 0, 1);
            Assert(x.CompareTo(MS(new byte[] { 8, 3, 5, 6, 7, 1, 2, 4 })) == 0);
            x.Insert(0, MutableString.CreateBinary(new byte[] { }));
            Assert(x.CompareTo(MS(new byte[] { 8, 3, 5, 6, 7, 1, 2, 4 })) == 0);
            x.Insert(5, MutableString.CreateBinary(new byte[] { 9, 10 }));
            Assert(x.CompareTo(MS(new byte[] { 8, 3, 5, 6, 7, 9, 10, 1, 2, 4 })) == 0);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Insert_Char() {
            var e = RubyEncoding.UTF8;

            MutableString x;
            x = MutableString.CreateMutable("12", e);
            x.Insert(0, '3');
            Assert(x.CompareTo(MS("312", e)) == 0);
            x.Insert(3, '4');
            Assert(x.CompareTo(MS("3124", e)) == 0);
            x.Insert(2, "");
            Assert(x.CompareTo(MS("3124", e)) == 0);
            x.Insert(1, "567");
            Assert(x.CompareTo(MS("3567124", e)) == 0);
            x.Insert(0, "8", 0, 1);
            Assert(x.CompareTo(MS("83567124", e)) == 0);
            x.Insert(0, MS("", e));
            Assert(x.CompareTo(MS("83567124", e)) == 0);
            x.Insert(5, MS("9Ω", e));
            Assert(x.CompareTo(MS("835679Ω124", e)) == 0);

            // Inserting Unicode string literal to a string doesn't check correctness of the resulting string.
            // An exception is thrown during subssequent operation that needs to convert the content.
            x = MS(((char)250).ToString(), RubyEncoding.Binary);
            x.Insert(0, 'α');
            AssertExceptionThrown<EncoderFallbackException>(() => x.ToByteArray());
        }

        [Options(NoRuntime = true)]
        public void MutableString_Remove_Byte() {
            MutableString x;
            x = MutableString.CreateBinary(new byte[] { });
            for (int i = 0; i < 10; i++) {
                x.Append((byte)i, 10);
            }
            Assert(x.GetByteCount() == 100);
            x.Remove(20, 60);
            Assert(x.GetByteCount() == 40);
            Assert(x.GetByte(0) == 0);
            Assert(x.GetByte(10) == 1);
            Assert(x.GetByte(20) == 8);
            Assert(x.GetByte(30) == 9);

            x = MutableString.CreateBinary(new byte[] { 1, 2, 3 });
            Assert(x.Equals(MS(new byte[] { 1, 2, 3 })));
            x.Remove(0, 1);
            Assert(x.Equals(MS(new byte[] { 2, 3 })));
            x.Remove(1, 1);
            Assert(x.Equals(MS(new byte[] { 2, })));
            x.Remove(0, 1);
            Assert(x.Equals(MS(new byte[] { })));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Remove_Char() {
            var e = RubyEncoding.UTF8;

            MutableString x;
            x = MutableString.Create("", e);
            for (int i = 0; i < 10; i++) {
                x.Append((char)('0' + i), 10);
            }
            Assert(x.GetCharCount() == 100);
            x.Remove(20, 60);
            Assert(x.GetCharCount() == 40);
            Assert(x.GetChar(0) == '0');
            Assert(x.GetChar(10) == '1');
            Assert(x.GetChar(20) == '8');
            Assert(x.GetChar(30) == '9');

            x = MutableString.Create("123", e);
            Assert(x.Equals(MS("123", e)));
            x.Remove(0, 1);
            Assert(x.Equals(MS("23", e)));
            x.Remove(1, 1);
            Assert(x.Equals(MS("2", e)));
            x.Remove(0, 1);
            Assert(x.Equals(MS("", e)));
        }

        // ASCII <-> ASCII
        // non-ASCII -> 0x80 -> '\u0080'
        public class NonInvertibleEncoding : Encoding {
            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                for (int i = 0; i < charCount; i++) {
                    char c = chars[charIndex + i];
                    bytes[byteIndex + i] = c > '\u007f' ? (byte)0x80 : (byte)c;
                }
                return charCount;
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
                for (int i = 0; i < byteCount; i++) {
                    chars[charIndex + i] = (char)bytes[i + byteIndex];
                }
                return byteCount;
            }

            public override int GetByteCount(char[] chars, int index, int count) {
                return count;
            }

            public override int GetCharCount(byte[] bytes, int index, int count) {
                return count;
            }

            public override int GetMaxByteCount(int charCount) {
                return charCount;
            }

            public override int GetMaxCharCount(int byteCount) {
                return byteCount;
            }
        }

        [Options(NoRuntime = true)]
        public void MutableString_SwitchRepr() {
            // \u{12345} in UTF-8:
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 };
            var u215c = new byte[] { 0xE2, 0x85, 0x9C };

            var surrogate = MS(u12345, RubyEncoding.UTF8);
            surrogate.Append('x');
            Assert(surrogate.GetCharCount() == 3);
            Assert(surrogate.ToString() == Encoding.UTF8.GetString(u12345) + "x");

            var result = MutableString.AppendUnicodeRepresentation(new StringBuilder(), Encoding.UTF8.GetString(u12345), MutableString.Escape.NonAscii, -1, -1);
            Assert(result.ToString() == "\\u{12345}");

            result = MutableString.AppendUnicodeRepresentation(new StringBuilder(), Encoding.UTF8.GetString(u215c), MutableString.Escape.NonAscii, -1, -1);
            Assert(result.ToString() == "\\u{215c}");

            //var incompleteChar = MS(new byte[] { 0xF0, 0x92 }, RubyEncoding.UTF8);
            //AssertExceptionThrown<DecoderFallbackException>(() => incompleteChar.Append('x'));

            // TODO:
            // var e = RubyEncoding.GetRubyEncoding(new NonInvertibleEncoding());
        }
        
        [Options(NoRuntime = true)]
        public void MutableString_Concatenate() {
#if TODO
            var utf8 = new byte[] { 0xe2, 0x85, 0x9c };
            var sjis = new byte[] { 0x82, 0xA0 };
            var ascii = new byte[] { 0x20 };

            Test_Concatenate(utf8, RubyEncoding.Binary, utf8, RubyEncoding.Binary, RubyEncoding.Binary);
            Test_Concatenate(utf8, RubyEncoding.Binary, sjis, RubyEncoding.KCodeSJIS, RubyEncoding.Binary);
            Test_Concatenate(utf8, RubyEncoding.Binary, utf8, RubyEncoding.KCodeUTF8, RubyEncoding.Binary);

            Test_Concatenate(sjis, RubyEncoding.KCodeSJIS, utf8, RubyEncoding.Binary, RubyEncoding.Binary);
            Test_Concatenate(sjis, RubyEncoding.KCodeSJIS, sjis, RubyEncoding.KCodeSJIS, RubyEncoding.KCodeSJIS);
            Test_Concatenate(sjis, RubyEncoding.KCodeSJIS, utf8, RubyEncoding.KCodeUTF8, RubyEncoding.Binary);
            Test_Concatenate(sjis, RubyEncoding.KCodeSJIS, utf8, RubyEncoding.UTF8, RubyEncoding.Binary);

            Test_Concatenate(utf8, RubyEncoding.KCodeUTF8, utf8, RubyEncoding.Binary, RubyEncoding.Binary);
            Test_Concatenate(utf8, RubyEncoding.KCodeUTF8, sjis, RubyEncoding.KCodeSJIS, RubyEncoding.Binary);
            Test_Concatenate(utf8, RubyEncoding.KCodeUTF8, utf8, RubyEncoding.KCodeUTF8, RubyEncoding.KCodeUTF8);
            Test_Concatenate(utf8, RubyEncoding.KCodeUTF8, utf8, RubyEncoding.UTF8, RubyEncoding.KCodeUTF8);

            Test_Concatenate(utf8, RubyEncoding.UTF8, sjis, RubyEncoding.KCodeSJIS, RubyEncoding.Binary);
            Test_Concatenate(utf8, RubyEncoding.UTF8, utf8, RubyEncoding.KCodeUTF8, RubyEncoding.KCodeUTF8);
            Test_Concatenate(utf8, RubyEncoding.UTF8, utf8, RubyEncoding.UTF8, RubyEncoding.UTF8);

            Test_Concatenate(utf8, RubyEncoding.UTF8, ascii, RubyEncoding.Binary, RubyEncoding.UTF8);
            Test_Concatenate(ascii, RubyEncoding.Binary, ascii, RubyEncoding.UTF8, RubyEncoding.UTF8);

            AssertExceptionThrown<EncodingCompatibilityError>(
                () => Test_Concatenate(utf8, RubyEncoding.Binary, utf8, RubyEncoding.UTF8, RubyEncoding.Binary)
            );
#endif
        }

        private void Test_Concatenate(byte[]/*!*/ b1, RubyEncoding/*!*/ e1, byte[]/*!*/ b2, RubyEncoding/*!*/ e2, RubyEncoding/*!*/ resultEncoding) {
            var s1 = MutableString.CreateBinary(b1, e1).PrepareForCharacterRead();
            var s2 = MutableString.CreateBinary(b2, e2).PrepareForCharacterRead();

            var s = MutableStringOps.Concatenate(s1, s2);
            Assert(s.Encoding == resultEncoding);
            var b = s.ToByteArray();
            Assert(b.ValueCompareTo(b.Length, Utils.Concatenate(b1, b2)) == 0);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Reverse() {
            var SJIS = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageSJIS);
            var utf8 = new byte[] { 0xe2, 0x85, 0x9c };
            var rev_bin_utf8 = new byte[] { 0x9c, 0x85, 0xe2};
            var invalid_utf8 = new byte[] { 0xe2, 0x85, 0x9c, 0xef };
            var rev_invalid_utf8 = new byte[] { 0xef, 0xe2, 0x85, 0x9c };
            var sjis = new byte[] { 0x82, 0xA0 };
            var ascii = new byte[] { 0x20, 0x55 };
            var rev_ascii = new byte[] { 0x55, 0x20 };
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 }; // \u{12345} in UTF-8

            Test_Reverse(new byte[0], RubyEncoding.UTF8, new byte[0]);
            Test_Reverse(utf8, RubyEncoding.UTF8, utf8);
            Test_Reverse(utf8, RubyEncoding.Binary, rev_bin_utf8);
            Test_Reverse(sjis, SJIS, sjis);
            Test_Reverse(ascii, RubyEncoding.UTF8, rev_ascii);
            
            // TODO: surrogates
            AssertExceptionThrown<EncoderFallbackException>(
                () => Test_Reverse(u12345, RubyEncoding.UTF8, u12345)
            );

            // TODO: MRI allows incorrect byte sequences
            AssertExceptionThrown<ArgumentException>(
                () => Test_Reverse(invalid_utf8, RubyEncoding.UTF8, rev_invalid_utf8)
            );

            Assert(MutableStringOps.Reverse(MutableString.Create("αΣ", RubyEncoding.UTF8)).ToString() == "Σα");
        }

        public void Test_Reverse(byte[]/*!*/ b, RubyEncoding/*!*/ e, byte[]/*!*/ expected) {
            var s = MutableString.CreateBinary(b, e);
            MutableStringOps.Reverse(s);
            var actual = s.ToByteArray();
            Assert(actual.ValueEquals(expected));
        }

        private byte[] Utf8(string str) {
            return Encoding.UTF8.GetBytes(str);
        }

        private byte[] Sjis(string str) {
            return RubyEncoding.SJIS.StrictEncoding.GetBytes(str);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Translate1() {
            var SJIS = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageSJIS);
            var sjis = new byte[] { 0x82, 0xA0 };
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 }; // \u{12345} in UTF-8
            Test_Translate(
                Utf8("αβγδ"), RubyEncoding.UTF8,
                Utf8("α-γ"), RubyEncoding.UTF8, 
                Utf8("AB"), SJIS,
                Utf8("ABBδ"), RubyEncoding.UTF8
            );

            Test_Translate(
                Utf8("αaβzγcδ"), RubyEncoding.UTF8,
                Utf8("a-z"), RubyEncoding.Binary,
                Utf8("*"), SJIS,
                Utf8("α*β*γ*δ"), RubyEncoding.UTF8
            );

            Test_Translate(
                Utf8("αaβzγcδ"), RubyEncoding.UTF8,
                Utf8("^α-δ"), RubyEncoding.UTF8,
                Utf8("-"), SJIS,
                Utf8("α-β-γ-δ"), RubyEncoding.UTF8
            );
            
            Test_Translate(
                Utf8("-α-"), RubyEncoding.Binary,
                Utf8("α"), RubyEncoding.Binary,
                Utf8("AB"), RubyEncoding.Binary,
                Utf8("-AB-"), RubyEncoding.Binary
            );

            Test_Translate(
                Utf8("-a-"), SJIS,
                Utf8("a"), RubyEncoding.Binary,
                Utf8("A"), RubyEncoding.UTF8,
                Utf8("-A-"), SJIS
            );

            Test_Translate(
               Utf8("a"), RubyEncoding.UTF8,
               Utf8("a"), RubyEncoding.UTF8,
               Utf8("\0"), RubyEncoding.UTF8,
               Utf8("\0"), RubyEncoding.UTF8
           );

            AssertExceptionThrown<EncodingCompatibilityError>(
                () => Test_Translate(Utf8("α"), RubyEncoding.Binary, Utf8("α"), RubyEncoding.UTF8, Utf8("-"), SJIS, null, null)
            );

            AssertExceptionThrown<EncodingCompatibilityError>(
                () => Test_Translate(Utf8("α"), RubyEncoding.UTF8, Sjis("ﾎ"), SJIS, Utf8("-"), SJIS, null, null)
            );

            // correctly switches to char repr and invalidates hashcode:
            MutableString self, from, to, result;
            int h0, h1, h2;

            self = MutableString.CreateBinary(Utf8("aAaBa"), RubyEncoding.UTF8);
            from = MutableString.Create("a", RubyEncoding.UTF8);
            to = MutableString.Create("α", RubyEncoding.UTF8);
            result = MutableString.Create("αAαBα", RubyEncoding.UTF8);
            h0 = self.GetHashCode();

            MutableStringOps.Translate(self, from, to);

            h1 = self.GetHashCode();
            h2 = result.GetHashCode();
            Assert(h1 == h2);
            Assert(self.ToString() == "αAαBα");
        }

        private void Test_Translate(
            byte[]/*!*/ bself, RubyEncoding/*!*/ eself,
            byte[]/*!*/ bfrom, RubyEncoding/*!*/ efrom,
            byte[]/*!*/ bto, RubyEncoding/*!*/ eto, 
            byte[]/*!*/ expected, RubyEncoding/*!*/ expectedEncoding) {

            var self = MutableString.CreateBinary(bself, eself);
            var from = MutableString.CreateBinary(bfrom, efrom);
            var to = MutableString.CreateBinary(bto, eto);

            var result = MutableStringOps.GetTranslated(self, from, to);
            Assert(result.Encoding == expectedEncoding);
            var b = result.ToByteArray();
            Assert(b.ValueEquals(expected));
        }

        [Options(NoRuntime = true)]
        public void MutableString_StartsWith1() {
            MutableString s;
            byte[] alpha = Encoding.UTF8.GetBytes("α");

            // binary string, UTF8 encoding:
            s = MutableString.CreateBinary(alpha, RubyEncoding.UTF8);
            Assert(s.StartsWith('α'));

            s = MutableString.CreateBinary(alpha, RubyEncoding.UTF8).Remove(1, 1);
            Assert(!s.StartsWith('α'));

            s = MutableString.CreateBinary(alpha, RubyEncoding.UTF8).Remove(0, 2);
            Assert(!s.StartsWith('α'));

            // binary string:
            s = MutableString.CreateBinary(alpha, RubyEncoding.Binary);
            Assert(!s.StartsWith('α'));

            s = MutableString.CreateMutable(BinaryEncoding.Instance.GetString(alpha), RubyEncoding.Binary);
            Assert(!s.StartsWith('α'));
            
            // char array content:
            s = MutableString.CreateMutable("abc", RubyEncoding.UTF8).Remove(1, 2);
            Assert(s.StartsWith('a'));
            Assert(!s.StartsWith('α'));
        }

        [Options(NoRuntime = true)]
        public void MutableString_IndexOf1() {
            string s = "12123";

            var strs = new[] {
                MutableString.CreateBinary(Utils.Concatenate(BinaryEncoding.Instance.GetBytes(s), new byte[] { 0, 0 })).Remove(s.Length, 2),
                MutableString.CreateMutable(s + "α", RubyEncoding.UTF8),
                MutableString.CreateMutable(s, RubyEncoding.UTF8).Append('α'),
                MutableString.CreateMutable(s, RubyEncoding.Binary).Append(new byte[] { 0xff }),
            };

            for (int i = 0; i < strs.Length; i++) {
                var a = strs[i];

                Action<string> test1 = (value) => {
                    Assert(a.IndexOf(BinaryEncoding.Instance.GetBytes(value)) == s.IndexOf(value));
                };

                Action<string, int> test2 = (value, start) => {
                    Assert(a.IndexOf(BinaryEncoding.Instance.GetBytes(value), start) == s.IndexOf(value, start));
                };

                Action<string, int, int> test3 = (value, start, count) => {
                    Assert(a.IndexOf(BinaryEncoding.Instance.GetBytes(value), start, count) == s.IndexOf(value, start, count));
                };

                test1("");
                test1("0");
                test1("12");
                test1("3");
                test2("", 4);
                test2("", 5);
                test2("12", 0);
                test2("12", 1);
                test2("12", 2);
                test2("30", 4);
                test3("", 2, 0);
                test3("12", 2, 1);
                test3("12", 2, 2);

                Assert(a.IndexOf('3', 100) == -1);
                Assert(a.IndexOf('2', 2) == s.IndexOf('2', 2));
                Assert(a.IndexOf('\0') == -1);

                Assert(a.IndexOf((byte)'3', 100) == -1);
                Assert(a.IndexOf((byte)'2', 2) == s.IndexOf('2', 2));
                Assert(a.IndexOf(0) == -1);

                Assert(a.IndexOf("123", 100) == -1);
                Assert(a.IndexOf("123", 1) == s.IndexOf("123", 1));
            }

            AssertExceptionThrown<ArgumentNullException>(() => strs[0].IndexOf((byte[])null, 1, 2));
            AssertExceptionThrown<ArgumentNullException>(() => strs[0].IndexOf((string)null, 1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf((byte)6, -1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf((byte)6, 1, -2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf('6', -1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf('6', 1, -2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf(new byte[] { 6 }, -1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf(new byte[] { 6 }, 6, -1));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf("6", -1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].IndexOf("6", 6, -1));
        }

        [Options(NoRuntime = true)]
        public void MutableString_LastIndexOf1() {
           string s = "12123";

            var strs = new[] {
                MutableString.CreateBinary(Utils.Concatenate(BinaryEncoding.Instance.GetBytes(s), new byte[] { 0, 0 })).Remove(s.Length, 2),
                MutableString.CreateMutable(s + "α", RubyEncoding.UTF8),
                MutableString.CreateMutable(s, RubyEncoding.UTF8).Append('α'),
                MutableString.CreateMutable(s, RubyEncoding.Binary).Append(new byte[] { 0xff }),
            };

            for (int i = 0; i < strs.Length; i++) {
                var a = strs[i];

                a = MutableString.CreateBinary(Utils.Concatenate(BinaryEncoding.Instance.GetBytes(s), new byte[] { 0, 0 }));
                a.Remove(s.Length, 2);

                Action<string> test1 = (value) => {
                    Assert(a.LastIndexOf(BinaryEncoding.Instance.GetBytes(value)) == s.LastIndexOf(value));
                };

                Action<string, int> test2 = (value, start) => {
                    Assert(a.LastIndexOf(BinaryEncoding.Instance.GetBytes(value), start) == s.LastIndexOf(value, start));
                };

                Action<string, int, int> test3 = (value, start, count) => {
                    Assert(a.LastIndexOf(BinaryEncoding.Instance.GetBytes(value), start, count) == s.LastIndexOf(value, start, count));
                };

                test1("");
                test1("0");
                test1("12");
                test1("3");
                test2("12", 0);
                test2("12", 1);
                test2("12", 2);
                test3("12", 4, 2);
                test3("12", 4, 3);
                test3("12", 0, 1);
                test3("12", 0, 0);
                test3("", 2, 0);

                Assert(a.LastIndexOf('3', 9, 5) == (s + "-----").LastIndexOf('3', 9, 5));
                Assert(a.LastIndexOf('2', 2) == s.LastIndexOf('2', 2));
                Assert(a.LastIndexOf('\0') == -1);

                Assert(a.LastIndexOf((byte)'3', 9, 5) == (s + "-----").LastIndexOf('3', 9, 5));
                Assert(a.LastIndexOf((byte)'2', 2) == s.LastIndexOf('2', 2));
                Assert(a.LastIndexOf(0) == -1);

                Assert(a.LastIndexOf("123", 9, 5) == (s + "-----").LastIndexOf("123", 9, 5));
                Assert(a.LastIndexOf("123", 1) == s.LastIndexOf("123", 1));
            }

            AssertExceptionThrown<ArgumentNullException>(() => strs[0].LastIndexOf((byte[])null, 1, 2));
            AssertExceptionThrown<ArgumentNullException>(() => strs[0].LastIndexOf((string)null, 1, 2));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf((byte)6, -1, 0));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf((byte)6, 0, -1));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf((byte)6, 1, 3));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf('6', -1, 0));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf('6', 0, -1));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf(new byte[] { 6 }, -1, 0));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf(new byte[] { 6 }, 0, -1));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf("6", -1, 0));
            AssertExceptionThrown<ArgumentException>(() => strs[0].LastIndexOf("6", 0, -1));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Index1() {
            var SJIS = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageSJIS);
            var sjis = new byte[] { 0x82, 0xA0 };
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 }; // \u{12345} in UTF-8
            var invalid = MutableString.CreateBinary(new byte[] { 0x80 }, RubyEncoding.UTF8);

            // SJIS non-ASCII string:
            AssertExceptionThrown<InvalidOperationException>(
                () => MutableStringOps.Index(MutableString.CreateBinary(sjis, SJIS), 0xA0, 0)
            );

            // binary encoding:
            int i;
            i = (int)MutableStringOps.Index(MutableString.CreateBinary(sjis, RubyEncoding.Binary), 0xA0, 0);
            Assert(i == 1);

            // ASCII-only are ok:
            i = (int)MutableStringOps.Index(MutableString.CreateMutable("abc", RubyEncoding.UTF8), (int)'a', 0);
            Assert(i == 0);
            
            MutableString a, b;

            // incompatible encodings:
            a = MutableString.CreateBinary(sjis, SJIS);
            b = MutableString.CreateBinary(u12345, RubyEncoding.UTF8);
            AssertExceptionThrown<EncodingCompatibilityError>(() => MutableStringOps.Index(a, b, 0));

            // invalid character:
            AssertExceptionThrown<ArgumentException>(() => MutableStringOps.Index(invalid, MutableString.FrozenEmpty, 0));
            AssertExceptionThrown<ArgumentException>(() => MutableStringOps.Index(MutableString.FrozenEmpty, invalid, 0));
            
            // returns character index:
            i = (int)MutableStringOps.Index(
                MutableString.CreateMutable("aαb", RubyEncoding.UTF8),
                MutableString.CreateMutable("b", SJIS), 
                0
            );
            Assert(i == 2);

            // returns character index:
            i = (int)MutableStringOps.Index(
                MutableString.CreateMutable("αabbba", RubyEncoding.UTF8),
                MutableString.CreateAscii("a"),
                2
            );
            Assert(i == 5);
        }

        public void MutableString_IndexRegex1() {
            var SJIS = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageSJIS);
            var sjis = new byte[] { 0x82, 0xA0 };
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 }; // \u{12345} in UTF-8
            var invalid = MutableString.CreateBinary(new byte[] { 0x80 }, RubyEncoding.UTF8);
            int i;
            MutableString a;
            RubyRegex r;
            RubyScope scope = new RubyTopLevelScope(Context);

            // incompatible encodings:
            a = MutableString.CreateBinary(sjis, SJIS);
            r = new RubyRegex(MutableString.CreateBinary(u12345, RubyEncoding.UTF8));
            AssertExceptionThrown<EncodingCompatibilityError>(() => MutableStringOps.Index(scope, a, r, 0));

            // invalid character:
            AssertExceptionThrown<ArgumentException>(() => MutableStringOps.Index(scope, invalid, r, 0));

            // returns character index:
            i = (int)MutableStringOps.Index(
                scope,
                MutableString.CreateMutable("aαb", RubyEncoding.UTF8),
                new RubyRegex(MutableString.CreateMutable("b", SJIS)),
                0
            );
            Assert(i == 2);

            // "start at" counts chars in 1.9, returns character index
            i = (int)MutableStringOps.Index(
                scope,
                MutableString.CreateMutable("αabbba", RubyEncoding.UTF8),
                new RubyRegex(MutableString.CreateAscii("a")),
                2
            );
            Assert(i == 5);

            // "start at" counts bytes in 1.8, returns byte index (regardless of KCODE)
            i = (int)MutableStringOps.Index(
                scope,
                MutableString.CreateBinary(Encoding.UTF8.GetBytes("αa"), RubyEncoding.Binary),
                new RubyRegex(MutableString.CreateAscii("a"), RubyRegexOptions.UTF8),
                2
            );
            Assert(i == 2);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Characters1() {
            TestChars(MS("αβ", RubyEncoding.UTF8), "αβ");
            TestChars(MS(Encoding.UTF8.GetBytes("αβ"), RubyEncoding.UTF8), "αβ");
            TestChars(MS("α", RubyEncoding.UTF8).Append('β'), "αβ");
            TestChars(MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.UTF8), "ab");
            TestChars(MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.Binary), "ab");

            var sjis = RubyEncoding.SJIS.StrictEncoding.GetBytes("あ");
            var beta = Encoding.UTF8.GetBytes("β");
            var x = new byte[] { (byte)'x' };
            var uinvalid = new byte[] { 0xff };
            var u12345 = new byte[] { 0xF0, 0x92, 0x8D, 0x85 }; // \u{12345} in UTF-8

            var c_sjis = new MSC('あ');
            var c_beta = new MSC('β');
            var c_x = new MSC('x');
            var c_uinvalid = new MSC(uinvalid);
            var s_u12345 = Encoding.UTF8.GetString(u12345);

            Assert(beta.Length == 2);

            // binary:
            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(beta, x), RubyEncoding.Binary),
                new MSC((char)beta[0]), new MSC((char)beta[1]), new MSC('x') 
            );

            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(beta, beta, x, x), RubyEncoding.UTF8),
                c_beta, c_beta, c_x, c_x
            );

            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(beta, uinvalid, uinvalid, beta, x, x, uinvalid), RubyEncoding.UTF8),
                c_beta, c_uinvalid, c_uinvalid, c_beta, c_x, c_x, c_uinvalid
            );

            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(u12345, beta), RubyEncoding.UTF8),
                new MSC(s_u12345[0], s_u12345[1]), c_beta
            );

            // string:
            TestChars(
                MutableString.CreateMutable("α" + s_u12345 + "xβ", RubyEncoding.UTF8),
                new MSC('α'), new MSC(s_u12345[0], s_u12345[1]), c_x, c_beta
            );

            TestChars(
                MutableString.CreateMutable(BinaryEncoding.Instance.GetString(Encoding.UTF8.GetBytes("xβ")), RubyEncoding.Binary),
                c_x, new MSC((char)beta[0]), new MSC((char)beta[1])
            );

            // chars:
            TestChars(
                MutableString.CreateMutable("α" + s_u12345 + "xβ", RubyEncoding.UTF8).Remove(4, 1),
                new MSC('α'), new MSC(s_u12345[0], s_u12345[1]), c_x
            );

            // remaining:
            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(beta, uinvalid, uinvalid, beta, x, x, uinvalid), RubyEncoding.UTF8), 
                MutableString.CreateBinary(Utils.Concatenate(uinvalid, beta, x, x, uinvalid), RubyEncoding.UTF8),
                c_beta, c_uinvalid
            );

            TestChars(
                MutableString.CreateBinary(Utils.Concatenate(uinvalid, uinvalid, uinvalid), RubyEncoding.UTF8),
                MutableString.CreateBinary(uinvalid, RubyEncoding.UTF8),
                c_uinvalid, c_uinvalid
            );

            TestChars(
                MutableString.CreateBinary(uinvalid, RubyEncoding.UTF8),
                MutableString.CreateBinary(new byte[0], RubyEncoding.UTF8),
                c_uinvalid
            );

            TestChars(
                MutableString.CreateMutable("α" + s_u12345 + "xβ", RubyEncoding.UTF8),
                MutableString.CreateMutable("xβ", RubyEncoding.UTF8),
                new MSC('α'), new MSC(s_u12345[0], s_u12345[1])
            );

            TestChars(
                MutableString.CreateMutable("α" + s_u12345 + "xβ", RubyEncoding.UTF8).Remove(4, 1),
                MutableString.CreateMutable("x", RubyEncoding.UTF8),
                new MSC('α'), new MSC(s_u12345[0], s_u12345[1])
            );
        }

        private void TestChars(MutableString/*!*/ str, string/*!*/ expected) {
            var e = str.GetCharacters();
            int i = 0;
            while (e.MoveNext()) {
                Assert(i < expected.Length);
                Assert(e.Current.Value == expected[i++]);
            }
            Assert(i == expected.Length);
        }

        private void TestChars(MutableString/*!*/ str, params MutableString.Character[]/*!*/ expected) {
            var e = str.GetCharacters();
            int i = 0;
            while (e.MoveNext()) {
                Assert(i < expected.Length);
                Assert(e.Current.Equals(expected[i++]));
            }
            Assert(i == expected.Length);
        }

        private void TestChars(MutableString/*!*/ str, MutableString/*!*/ remaining, params MutableString.Character[]/*!*/ expected) {
            var e = str.GetCharacters();
            for (int i = 0; i < expected.Length; i++) {
                Assert(e.MoveNext());
                Assert(e.Current.Equals(expected[i]));
                if (!remaining.IsEmpty) {
                    Assert(e.HasMore);
                }
            }
            Assert(MutableString.CreateMutable(remaining.Encoding).AppendRemaining(e).Equals(remaining));
        }

        [Options(NoRuntime = true)]
        public void MutableString_Bytes1() {
            List<byte> bs;

            bs = new List<byte>();
            foreach (byte b in MS("αβ", RubyEncoding.UTF8).GetBytes()) {
                bs.Add(b);
            }
            Assert(bs.ToArray().ValueEquals(Encoding.UTF8.GetBytes("αβ")));

            bs = new List<byte>();
            foreach (byte b in MS(Encoding.UTF8.GetBytes("αβ"), RubyEncoding.UTF8).GetBytes()) {
                bs.Add(b);
            }
            Assert(bs.ToArray().ValueEquals(Encoding.UTF8.GetBytes("αβ")));

            bs = new List<byte>();
            foreach (byte b in MS("α", RubyEncoding.UTF8).Append('β').GetBytes()) {
                bs.Add(b);
            }
            Assert(bs.ToArray().ValueEquals(Encoding.UTF8.GetBytes("αβ")));

            bs = new List<byte>();
            foreach (byte b in MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.UTF8).GetBytes()) {
                bs.Add(b);
            }
            Assert(bs.ToArray().ValueEquals(new byte[] { (byte)'a', (byte)'b' }));

            bs = new List<byte>();
            foreach (byte b in MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.Binary).GetBytes()) {
                bs.Add(b);
            }
            Assert(bs.ToArray().ValueEquals(new byte[] { (byte)'a', (byte)'b' }));
        }

        [Options(NoRuntime = true)]
        public void MutableString_ChangeEncoding1() {
#if TODO
            var alpha = Encoding.UTF8.GetBytes("α");
            MutableString s = MutableString.CreateBinary(alpha, RubyEncoding.UTF8);

            var SJIS = RubyEncoding.SJIS;
            var hc2 = MutableString.Create("α", SJIS).GetHashCode();
            var hc3 = MutableString.CreateBinary(alpha, SJIS).GetHashCode();
            // TODO: hc2 should be equal to hc3
            s.ChangeEncoding(SJIS, true);
            Assert(s.ToString() == "ﾎｱ" && s.Encoding == SJIS && !s.IsBinaryEncoded && (s.GetHashCode() == hc2 || s.GetHashCode() == hc3));

            s = MutableString.CreateMutable("α", RubyEncoding.UTF8);
            s.ChangeEncoding(SJIS, true);
            Assert(s.ToByteArray().ValueEquals(SJIS.StrictEncoding.GetBytes("α")));

            hc = s.GetHashCode();
            hc2 = MutableString.Create("α", RubyEncoding.SJIS).GetHashCode();
            hc3 = MutableString.CreateBinary(alpha, RubyEncoding.SJIS).GetHashCode();
            
            s.ChangeEncoding(RubyEncoding.SJIS, true);
            Assert(s.GetHashCode() == hc2 || s.GetHashCode() == hc3);
#endif
        }

        [Options(NoRuntime = true)]
        public void MutableString_ValidEncoding1() {
            var str = MutableString.CreateBinary(Encoding.UTF8.GetBytes("\u0081"), RubyEncoding.SJIS);
            Assert(str.ContainsInvalidCharacters());

            str = MutableString.CreateMutable("\u0081", RubyEncoding.SJIS);
            Assert(str.ContainsInvalidCharacters());

            str = MutableString.CreateMutable("hello", RubyEncoding.SJIS);
            Assert(!str.ContainsInvalidCharacters());

            str = MutableString.CreateMutable("ﾎｱ", RubyEncoding.SJIS);
            Assert(!str.ContainsInvalidCharacters());
        }
    }
}
