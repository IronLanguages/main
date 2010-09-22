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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Compiler;

namespace IronRuby.Builtins {

    /// <summary>
    /// All encodings in Ruby are represented by instances of a single class. Therefore we need to wrap .NET Encoding class in RubyEncoding.
    /// Instances of this class are app-domain singletons. That's all right as far as the class is readonly and doesn't implement IRubyObject.
    /// Taint, frozen flags and instance variables need to be stored in per-runtime lookaside table.
    /// </summary>
    [Serializable]
    public class RubyEncoding : ISerializable, IExpressionSerializable {
        #region Singletons

        public const int CodePageBinary = 0;
        public const int CodePageAscii = 20127;
        public const int CodePageEUC = 51932;
        public const int CodePageUTF7 = 65000;
        public const int CodePageUTF8 = 65001;
        public const int CodePageUTF16BE = 1201;
        public const int CodePageUTF16LE = 1200;
        public const int CodePageUTF32BE = 12001;
        public const int CodePageUTF32LE = 12000;
        public const int CodePageSJIS = 932;

        // TODO: how does MRI sort encodings?
        public static readonly RubyEncoding/*!*/ Binary = new RubyEncoding(BinaryEncoding.Instance, BinaryEncoding.Instance, -4);
        public static readonly RubyEncoding/*!*/ UTF8 = new RubyEncoding(CreateEncoding(CodePageUTF8, false), CreateEncoding(CodePageUTF8, true), -3);
        public static readonly RubyEncoding/*!*/ Ascii = new RubyEncoding(CreateEncoding(CodePageAscii, false), CreateEncoding(CodePageAscii, true), -2);
        public static readonly RubyEncoding/*!*/ EUC = new RubyEncoding(CreateEncoding(CodePageEUC, false), CreateEncoding(CodePageEUC, true), -1);
        public static readonly RubyEncoding/*!*/ SJIS = new RubyEncoding(CreateEncoding(CodePageSJIS, false), CreateEncoding(CodePageSJIS, true), 0);

        #endregion

        // TODO: use encoders/decoders?
        private readonly Encoding/*!*/ _encoding;
        private readonly Encoding/*!*/ _strictEncoding;
        private readonly int _maxBytesPerChar;
        private readonly int _ordinal;
        private readonly bool _isAsciiIdentity;
        private Expression _expression;

        private RubyEncoding(Encoding/*!*/ encoding, Encoding/*!*/ strictEncoding, int ordinal) {
            Assert.NotNull(encoding, strictEncoding);
            _ordinal = ordinal;
            _encoding = encoding;
            _strictEncoding = strictEncoding;
            _maxBytesPerChar = strictEncoding.GetMaxByteCount(1);
            _isAsciiIdentity = AsciiIdentity(encoding);
        }

        internal Expression/*!*/ Expression {
            get { return _expression ?? (_expression = Expression.Constant(this)); }
        }

        public bool IsAsciiIdentity {
            get { return _isAsciiIdentity; }
        }

        private static Encoding/*!*/ CreateEncoding(int codepage, bool throwOnError) {
#if SILVERLIGHT
            return new UTF8Encoding(false, throwOnError);
#else
            if (throwOnError) {
                return Encoding.GetEncoding(codepage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            } else {
                return Encoding.GetEncoding(codepage, EncoderFallback.ReplacementFallback, BinaryDecoderFallback.Instance);
            }
#endif
        }

        #region Serialization
#if !SILVERLIGHT
        private RubyEncoding(SerializationInfo/*!*/ info, StreamingContext context) {
            throw Assert.Unreachable;
        }

        [Serializable]
        internal sealed class Deserializer : ISerializable, IObjectReference {
            private readonly int _codePage;
            
            private Deserializer(SerializationInfo/*!*/ info, StreamingContext context) {
                _codePage = info.GetInt32("CodePage");
            }

            public object GetRealObject(StreamingContext context) {
                return GetRubyEncoding(_codePage);
            }

            void ISerializable.GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
                throw Assert.Unreachable;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            info.AddValue("CodePage", _encoding.CodePage);
            info.SetType(typeof(Deserializer));
        }
#endif
        #endregion

        public int MaxBytesPerChar {
            get { return _maxBytesPerChar; }
        }

        public Encoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public Encoding/*!*/ StrictEncoding {
            get { return _strictEncoding; }
        }

        public string/*!*/ Name {
            get { return _encoding.WebName; }
        }

        public int CodePage {
            get { return GetCodePage(_encoding); }
        }

        public override string/*!*/ ToString() {
            return Name;
        }

        public int CompareTo(RubyEncoding/*!*/ other) {
            return _ordinal - other._ordinal;
        }

        public static RubyRegexOptions ToRegexOption(RubyEncoding encoding) {
            if (encoding == RubyEncoding.Binary) {
                return RubyRegexOptions.FIXED;
            }
            
            if (encoding == null) {
                return RubyRegexOptions.NONE;
            }

            switch (GetCodePage(encoding._encoding)) {
#if !SILVERLIGHT
                case RubyEncoding.CodePageSJIS: return RubyRegexOptions.SJIS;
                case RubyEncoding.CodePageEUC: return RubyRegexOptions.EUC;
#endif
                case RubyEncoding.CodePageUTF8: return RubyRegexOptions.UTF8;
            }

            throw Assert.Unreachable;
        }

        public static RubyEncoding GetRegexEncoding(RubyRegexOptions options) {
            switch (options & RubyRegexOptions.EncodingMask) {
#if !SILVERLIGHT
                case RubyRegexOptions.EUC: return RubyEncoding.EUC;
                case RubyRegexOptions.SJIS: return RubyEncoding.SJIS;
#endif
                case RubyRegexOptions.UTF8: return RubyEncoding.UTF8;
                case RubyRegexOptions.FIXED: return RubyEncoding.Binary;
                default: return null;
            }
        }


        internal static int GetCodePage(int firstChar) {
            switch (firstChar) {
#if !SILVERLIGHT
                case 'E':
                case 'e': return CodePageEUC;
                case 'S':
                case 's': return CodePageSJIS;
#endif
                case 'U':
                case 'u': return CodePageUTF8;
                default:
                    return -1;
            }
        }

        public static RubyEncoding GetEncodingByNameInitial(int initial) {
            int codepage = GetCodePage(initial);
            return codepage > 0 ? GetRubyEncoding(codepage) : null;
        }

        public void RequireAsciiIdentity() {
            if (!_isAsciiIdentity) {
                throw new NotSupportedException(String.Format("Encoding {0} (code page {1}) is not supported", _encoding, GetCodePage(_encoding)));
            }
        }

#if !SILVERLIGHT
        private static Dictionary<int, RubyEncoding> _Encodings;
        
        public static RubyEncoding/*!*/ GetRubyEncoding(Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(encoding, "encoding");
            
            if (encoding.CodePage == 0) {
                if (encoding == BinaryEncoding.Instance) {
                    return Binary;
                }

                // TODO: allow custom encodings (without codepage)
            }

            return GetRubyEncoding(encoding.CodePage);
        }

        public static RubyEncoding/*!*/ GetRubyEncoding(int codepage) {
            switch (codepage) {
                case CodePageBinary: return Binary;
                case CodePageAscii: return Ascii;
                case CodePageUTF8: return UTF8;
                case CodePageSJIS: return SJIS;
                case CodePageEUC: return EUC;
            }

            if (_Encodings == null) {
                Interlocked.CompareExchange(ref _Encodings, new Dictionary<int, RubyEncoding>(), null); 
            }

            RubyEncoding result;
            lock (_Encodings) {
                if (!_Encodings.TryGetValue(codepage, out result)) {
                    result = new RubyEncoding(
                        CreateEncoding(codepage, false),
                        CreateEncoding(codepage, true),
                        codepage
                    );

                    _Encodings.Add(codepage, result);
                }
            }

            return result;
        }

        private static int GetCodePage(Encoding/*!*/ encoding) {
            return encoding.CodePage;
        }

        public static bool AsciiIdentity(Encoding/*!*/ encoding) {
            if (encoding == BinaryEncoding.Instance) {
                return true;
            }

            switch (encoding.CodePage) {
                case 437: // OEM United States
                case 708: // Arabic (ASMO 708)
                case 720: // Arabic (DOS)
                case 737: // Greek (DOS)
                case 775: // Baltic (DOS)
                case 850: // Western European (DOS)
                case 852: // Central European (DOS)
                case 855: // OEM Cyrillic
                case 857: // Turkish (DOS)
                case 858: // OEM Multilingual Latin I
                case 860: // Portuguese (DOS)
                case 861: // Icelandic (DOS)
                case 862: // Hebrew (DOS)
                case 863: // French Canadian (DOS)
                case 864: // Arabic (864)
                case 865: // Nordic (DOS)
                case 866: // Cyrillic (DOS)
                case 869: // Greek, Modern (DOS)
                case 874: // Thai (Windows)
                case 932: // Japanese (Shift-JIS)
                case 936: // Chinese Simplified (GB2312)
                case 949: // Korean
                case 950: // Chinese Traditional (Big5)
                case 1250: // Central European (Windows)
                case 1251: // Cyrillic (Windows)
                case 1252: // Western European (Windows)
                case 1253: // Greek (Windows)
                case 1254: // Turkish (Windows)
                case 1255: // Hebrew (Windows)
                case 1256: // Arabic (Windows)
                case 1257: // Baltic (Windows)
                case 1258: // Vietnamese (Windows)
                case 1361: // Korean (Johab)
                case 10000: // Western European (Mac)
                case 10001: // Japanese (Mac)
                case 10002: // Chinese Traditional (Mac)
                case 10003: // Korean (Mac)
                case 10004: // Arabic (Mac)
                case 10005: // Hebrew (Mac)
                case 10006: // Greek (Mac)
                case 10007: // Cyrillic (Mac)
                case 10008: // Chinese Simplified (Mac)
                case 10010: // Romanian (Mac)
                case 10017: // Ukrainian (Mac)
                case 10029: // Central European (Mac)
                case 10079: // Icelandic (Mac)
                case 10081: // Turkish (Mac)
                case 10082: // Croatian (Mac)
                case 20000: // Chinese Traditional (CNS)
                case 20001: // TCA Taiwan
                case 20002: // Chinese Traditional (Eten)
                case 20003: // IBM5550 Taiwan
                case 20004: // TeleText Taiwan
                case 20005: // Wang Taiwan
                case 20127: // US-ASCII
                case 20866: // Cyrillic (KOI8-R)
                case 20932: // Japanese (JIS 0208-1990 and 0212-1990)
                case 20936: // Chinese Simplified (GB2312-80)
                case 20949: // Korean Wansung
                case 21866: // Cyrillic (KOI8-U)
                case 28591: // Western European (ISO)
                case 28592: // Central European (ISO)
                case 28593: // Latin 3 (ISO)
                case 28594: // Baltic (ISO)
                case 28595: // Cyrillic (ISO)
                case 28596: // Arabic (ISO)
                case 28597: // Greek (ISO)
                case 28598: // Hebrew (ISO-Visual)
                case 28599: // Turkish (ISO)
                case 28603: // Estonian (ISO)
                case 28605: // Latin 9 (ISO)
                case 38598: // Hebrew (ISO-Logical)
                case 50220: // Japanese (JIS)
                case 50221: // Japanese (JIS-Allow 1 byte Kana)
                case 50222: // Japanese (JIS-Allow 1 byte Kana - SO/SI)
                case 50225: // Korean (ISO)
                case 50227: // Chinese Simplified (ISO-2022)
                case 51932: // Japanese (EUC)
                case 51936: // Chinese Simplified (EUC)
                case 51949: // Korean (EUC)
                case 54936: // Chinese Simplified (GB18030)
                case 57002: // ISCII Devanagari
                case 57003: // ISCII Bengali
                case 57004: // ISCII Tamil
                case 57005: // ISCII Telugu
                case 57006: // ISCII Assamese
                case 57007: // ISCII Oriya
                case 57008: // ISCII Kannada
                case 57009: // ISCII Malayalam
                case 57010: // ISCII Gujarati
                case 57011: // ISCII Punjabi
                case 65001: // Unicode (UTF-8)
                    Debug.Assert(IsAsciiIdentityFallback(encoding));
                    return true;

                default: 
                    return IsAsciiIdentityFallback(encoding);
            }
        }

        private static string _AllAscii;

        private static bool IsAsciiIdentityFallback(Encoding/*!*/ encoding) {
            if (_AllAscii == null) {
                // all ASCII characters:
                var sb = new StringBuilder(0x80);
                for (int i = 0; i < 0x80; i++) {
                    sb.Append((char)i);
                }
                _AllAscii = sb.ToString();
            }

            var bytes = encoding.GetBytes(_AllAscii);
            if (bytes.Length != _AllAscii.Length) {
                return false;
            }

            for (int i = 0; i < _AllAscii.Length; i++) {
                if ((int)_AllAscii[i] != (int)bytes[i]) {
                    return false;
                }
            }

            return true;
        }

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            // TODO: use static fields, deal with KCODEs
            return Methods.CreateEncoding.OpCall(Expression.Constant(_encoding.CodePage));
        }
#else
        public static bool AsciiIdentity(Encoding/*!*/ encoding) {
            switch (GetCodePage(encoding)) {
                case CodePageBinary:
                case CodePageUTF8: 
                    return true;
            }

            return false;
        }

        public static RubyEncoding/*!*/ GetRubyEncoding(Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(encoding, "encoding");
            if (encoding == BinaryEncoding.Instance) {
                return Binary;
            } else if (encoding.ToString() == Encoding.UTF8.ToString()) {
                return UTF8;
            } else {
                throw new ArgumentException(String.Format("Unknown encoding: '{0}'", encoding));
            }
        }

        internal static RubyEncoding/*!*/ GetRubyEncoding(int codepage) {
            switch (codepage) {
                case CodePageBinary: return Binary;
                case CodePageUTF8: return UTF8;
                default: throw new ArgumentException(String.Format("Unknown encoding codepage: {0}", codepage));
            }
        }

        private static int GetCodePage(Encoding/*!*/ encoding) {
            Debug.Assert(encoding != null);

            if (encoding == BinaryEncoding.Instance) {
                return CodePageBinary;
            }

            switch (encoding.WebName.ToUpperInvariant()) {
                case "UTF-8": return CodePageUTF8;
                case "UTF-16": return CodePageUTF16LE;
                case "UTF-16BE": return CodePageUTF16BE;
            }
            
            throw new ArgumentException(String.Format("Unknown encoding: {0}", encoding));
        }

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            // TODO: use a static fields, deal with KCODEs
            return Expression.Constant(UTF8);
        }
#endif
    }

}
