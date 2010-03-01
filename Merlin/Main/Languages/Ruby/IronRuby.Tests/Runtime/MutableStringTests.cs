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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
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

            h2 = s.GetHashCode();
            AssertEquals(h1, h2);

            // same content, different capacities:
            var a = MutableString.CreateMutable(10, RubyEncoding.UTF8).Append("hello");
            var b = MutableString.CreateMutable(20, RubyEncoding.UTF8).Append("hello");
            var c = MutableString.Create("hello", RubyEncoding.UTF8);
            Assert(a.GetHashCode() == b.GetHashCode());
            Assert(a.GetHashCode() == c.GetHashCode());

            // same content, different encodings:
            // ASCII characters only:
            a = MutableString.Create("hello", RubyEncoding.UTF8);
            b = MutableString.Create("hello", RubyEncoding.KCodeSJIS.RealEncoding);
            c = MutableString.CreateAscii("hello");
            Assert(a.GetHashCode() == b.GetHashCode());
            Assert(a.GetHashCode() == c.GetHashCode());
            Assert(b.GetHashCode() == c.GetHashCode());

            // non-ASCII characters:
            a = MutableString.Create("α", RubyEncoding.UTF8);
            b = MutableString.Create("α", RubyEncoding.KCodeSJIS.RealEncoding);
            c = MutableString.CreateBinary(Encoding.UTF8.GetBytes("α"), RubyEncoding.Binary);
            Assert(a.GetHashCode() != b.GetHashCode());
            Assert(a.GetHashCode() != c.GetHashCode());
            Assert(b.GetHashCode() != c.GetHashCode());

            // same content, different k-codings:
            // 1.8 doesn't know about encodings => if the strings are binary equivalent they have the same hash:
            a = MutableString.Create("hello", RubyEncoding.KCodeUTF8);
            b = MutableString.Create("hello", RubyEncoding.KCodeSJIS);
            Assert(a.GetHashCode() == b.GetHashCode());

            a = MutableString.Create("α", RubyEncoding.KCodeUTF8);
            b = MutableString.Create("α", RubyEncoding.KCodeSJIS);
            c = MutableString.CreateBinary(Encoding.UTF8.GetBytes("α"), RubyEncoding.Binary);
            Assert(a.GetHashCode() != b.GetHashCode()); // the binary content is different
            Assert(a.GetHashCode() == c.GetHashCode()); // the binary contant is the same
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
            Assert(MutableStringOps.GetLength(x) == 1);

            x = MutableString.Create("α", RubyEncoding.UTF8);
            Assert(MutableStringOps.GetLength(x) == 1);

            x = MutableString.Create("α", RubyEncoding.KCodeUTF8);
            Assert(MutableStringOps.GetLength(x) == 2);
        }

        [Options(NoRuntime = true)]
        public void MutableString_CompareTo() {
            MutableString x, y;
            RubyEncoding SJIS = RubyEncoding.KCodeSJIS.RealEncoding;

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

            // k-codings (the string represnetation might be the same, but their binary repr is different):
            x = MS("α", RubyEncoding.KCodeUTF8);
            y = MS("α", RubyEncoding.KCodeSJIS);
            Assert(Math.Sign(x.CompareTo(y)) != 0);

            // same binary repr, different string repr:
            x = MS(alpha, RubyEncoding.KCodeUTF8);
            y = MS(alpha, RubyEncoding.KCodeSJIS);
            Assert(Math.Sign(x.CompareTo(y)) == 0);
            Assert(x.ToString() == "α");
            Assert(y.ToString() == "ﾎｱ");

            x = MS("α", RubyEncoding.KCodeUTF8);
            y = MS(BinaryEncoding.Instance.GetString(alpha), RubyEncoding.Binary); 
            Assert(Math.Sign(x.CompareTo(y)) == 0);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Equals() {
            byte[] bytes = Encoding.UTF8.GetBytes("α");

            MutableString a, b, c, d;
            a = MutableString.CreateBinary(bytes, RubyEncoding.Binary);
            b = MutableString.CreateBinary(bytes, RubyEncoding.KCodeSJIS);
            c = MutableString.CreateBinary(bytes, RubyEncoding.KCodeUTF8);
            d = MutableString.Create("α", RubyEncoding.KCodeUTF8);

            Assert(a.Equals(b));
            Assert(a.Equals(c));
            Assert(a.Equals(d));
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
        }

        private void Test_Concatenate(byte[]/*!*/ b1, RubyEncoding/*!*/ e1, byte[]/*!*/ b2, RubyEncoding/*!*/ e2, RubyEncoding/*!*/ resultEncoding) {
            var s1 = MutableString.CreateBinary(b1, e1).SwitchToCharacters();
            var s2 = MutableString.CreateBinary(b2, e2).SwitchToCharacters();

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

            // TODO: KCODE
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
            return RubyEncoding.KCodeSJIS.StrictEncoding.GetBytes(str);
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

            // TODO: KCODE

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
        public void MutableString_IndexOf1() {
            MutableString a;
            string s = "12123";

            a = MutableString.CreateBinary(Utils.Concatenate(BinaryEncoding.Instance.GetBytes(s), new byte[] { 0, 0 }));
            a.Remove(s.Length, 2);

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
            test2("12", 0);
            test2("12", 1);
            test2("12", 2);
            test2("30", 4);
            test3("", 2, 0);
            test3("12", 2, 1);
            test3("12", 2, 2);
        }

        [Options(NoRuntime = true)]
        public void MutableString_LastIndexOf1() {
            MutableString a;
            string s = "12123";

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

            AssertExceptionThrown<ArgumentOutOfRangeException>(() => a.LastIndexOf(new byte[] { 6 }, 0, 2));
            AssertExceptionThrown<ArgumentOutOfRangeException>(() => a.LastIndexOf(new byte[] { 6 }, 6, 2));
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

            // k-coded are ok:
            i = (int)MutableStringOps.Index(MutableString.CreateBinary(u12345, RubyEncoding.KCodeUTF8), 0x85, 0);
            Assert(i == 3);

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

            // returns byte index for k-coded strings:
            i = (int)MutableStringOps.Index(
                MutableString.CreateMutable("αaβb", RubyEncoding.KCodeUTF8),
                MutableString.CreateMutable("a", RubyEncoding.KCodeSJIS),
                0
            );
            Assert(i == 2);

            // returns byte index for k-coded strings:
            i = (int)MutableStringOps.Index(
                MutableString.CreateMutable("αabbba", RubyEncoding.KCodeUTF8),
                MutableString.CreateAscii("a"),
                2
            );
            Assert(i == 2);
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

            // returns byte index for k-coded strings:
            i = (int)MutableStringOps.Index(
                scope,
                MutableString.CreateMutable("αaβb", RubyEncoding.KCodeUTF8),
                new RubyRegex(MutableString.CreateMutable("a", RubyEncoding.KCodeSJIS)),
                0
            );
            Assert(i == 2);

            // returns byte index for k-coded strings:
            i = (int)MutableStringOps.Index(
                scope,
                MutableString.CreateMutable("αabbba", RubyEncoding.KCodeUTF8),
                new RubyRegex(MutableString.CreateAscii("a")),
                2
            );
            Assert(i == 2);

            // uses the current KCODE for match:
            a = MutableString.CreateBinary(new byte[] { 0x82, 0xa1, 0x82, 0xa0, 0x82, 0xa0 }, RubyEncoding.Binary);
            r = new RubyRegex(MutableString.CreateBinary(new byte[] { 0x82, 0xa0, (byte)'{', (byte)'2', (byte)'}' }, RubyEncoding.Binary));

            Context.KCode = RubyEncoding.KCodeSJIS;
            Assert((int)MutableStringOps.Index(scope, a, r, 0) == 2);

            Context.KCode = null;
            Assert(MutableStringOps.Index(scope, a, r, 0) == null);

            // invalid characters:
            a = MutableString.CreateBinary(new byte[] { 0x82, 0x82, 0xa0, 0xa0, 0x82 }, RubyEncoding.Binary);
            r = new RubyRegex(MutableString.CreateBinary(new byte[] { 0x82, 0xa0, (byte)'{', (byte)'2', (byte)'}' }, RubyEncoding.Binary));

            // TODO:
            // We throw an exception here since we don't exactly know how MRI handles invalid characters.
            Context.KCode = RubyEncoding.KCodeSJIS;
            AssertExceptionThrown<ArgumentException>(() => MutableStringOps.Index(scope, a, r, 0));

            Context.KCode = null;
            Assert((int)MutableStringOps.Index(scope, a, r, 0) == 1);
        }

        [Options(NoRuntime = true)]
        public void MutableString_Characters1() {
            StringBuilder cs;
            
            cs = new StringBuilder();
            foreach (char c in MS("αβ", RubyEncoding.UTF8).GetCharacters()) {
                cs.Append(c);
            }
            Assert(cs.ToString() == "αβ");

            cs = new StringBuilder();
            foreach (char c in MS(Encoding.UTF8.GetBytes("αβ"), RubyEncoding.UTF8).GetCharacters()) {
                cs.Append(c);
            }
            Assert(cs.ToString() == "αβ");

            cs = new StringBuilder();
            foreach (char c in MS("α", RubyEncoding.UTF8).Append('β').GetCharacters()) {
                cs.Append(c);
            }
            Assert(cs.ToString() == "αβ");

            cs = new StringBuilder();
            foreach (char c in MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.UTF8).GetCharacters()) {
                cs.Append(c);
            }
            Assert(cs.ToString() == "ab");

            cs = new StringBuilder();
            foreach (char c in MS(Encoding.UTF8.GetBytes("ab"), RubyEncoding.Binary).GetCharacters()) {
                cs.Append(c);
            }
            Assert(cs.ToString() == "ab");
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
    }
}
