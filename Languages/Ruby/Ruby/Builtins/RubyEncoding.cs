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
        public const int CodePageSJIS = 932;
        public const int CodePageBig5 = 950;
        public const int CodePageAscii = 20127;

        // Windows returns 2 EUC-JP encodings (CP 20932 and CP 51932). Mono implements EUC-JP as 51932 and doesn't support 20932.
        public const int CodePageEUCJP = 51932;

        public const int CodePageUTF7 = 65000;
        public const int CodePageUTF8 = 65001;
        public const int CodePageUTF16BE = 1201;
        public const int CodePageUTF16LE = 1200;
        public const int CodePageUTF32BE = 12001;
        public const int CodePageUTF32LE = 12000;
        
        // TODO: how does MRI sort encodings?

        public static readonly RubyEncoding/*!*/ Binary = new RubyEncoding(BinaryEncoding.Instance, BinaryEncoding.Instance, -4);
        public static readonly RubyEncoding/*!*/ UTF8 = new RubyEncoding(CreateEncoding(CodePageUTF8, false), CreateEncoding(CodePageUTF8, true), -3);

#if SILVERLIGHT
        public static readonly RubyEncoding/*!*/ Ascii = UTF8;
#else
        public static readonly RubyEncoding/*!*/ Ascii = new RubyEncoding(CreateEncoding(CodePageAscii, false), CreateEncoding(CodePageAscii, true), -2);
        public static readonly RubyEncoding/*!*/ EUCJP = new RubyEncoding(CreateEncoding(CodePageEUCJP, false), CreateEncoding(CodePageEUCJP, true), -1);
        public static readonly RubyEncoding/*!*/ SJIS = new RubyEncoding(CreateEncoding(CodePageSJIS, false), CreateEncoding(CodePageSJIS, true), 0);
#endif

        #endregion

        private readonly Encoding/*!*/ _encoding;
        private readonly Encoding/*!*/ _strictEncoding;
        private Expression _expression;
        private readonly int _ordinal;

        // TODO: combine into a single integer (tables could be merged)
        private readonly int _maxBytesPerChar;
        private readonly bool _isAsciiIdentity;
#if !SILVERLIGHT
        private bool? _isSingleByteCharacterSet;
        private bool? _isDoubleByteCharacterSet;
#endif

        private RubyEncoding(Encoding/*!*/ encoding, Encoding/*!*/ strictEncoding, int ordinal) {
            Assert.NotNull(encoding, strictEncoding);
            _ordinal = ordinal;
            _encoding = encoding;
            _strictEncoding = strictEncoding;
            _maxBytesPerChar = strictEncoding.GetMaxByteCount(1);
            _isAsciiIdentity = AsciiIdentity(encoding);
        }

        public override int GetHashCode() {
            return _ordinal;
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
            info.AddValue("CodePage", CodePage);
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

        /// <summary>
        /// Name as displayed by MRI.
        /// </summary>
        public string/*!*/ Name {
            get {
                return GetRubySpecificName(CodePage) ?? _encoding.WebName;
            }
        }

        public static string GetRubySpecificName(int codepage) {
            switch (codepage) {
                case RubyEncoding.CodePageUTF8: return "UTF-8";
#if !SILVERLIGHT
                case RubyEncoding.CodePageUTF7: return "UTF-7";
                case RubyEncoding.CodePageUTF16BE: return "UTF-16BE";
                case RubyEncoding.CodePageUTF16LE: return "UTF-16LE";
                case RubyEncoding.CodePageUTF32BE: return "UTF-32BE";
                case RubyEncoding.CodePageUTF32LE: return "UTF-32LE";
                case RubyEncoding.CodePageSJIS: return "Shift_JIS";
                case RubyEncoding.CodePageAscii: return "US-ASCII";

                // disambiguates CP 20932 and CP 51932:
                case RubyEncoding.CodePageEUCJP: return "EUC-JP";
                case 20932: return "CP20932";

                case 50220: return "ISO-2022-JP";
                case 50222: return "CP50222";
#endif
                default: return null;
            }
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

            switch (encoding.CodePage) {
#if !SILVERLIGHT
                case RubyEncoding.CodePageSJIS: return RubyRegexOptions.SJIS;
                case RubyEncoding.CodePageEUCJP: return RubyRegexOptions.EUC;
#endif
                case RubyEncoding.CodePageUTF8: return RubyRegexOptions.UTF8;
            }

            throw Assert.Unreachable;
        }

        public static RubyEncoding GetRegexEncoding(RubyRegexOptions options) {
            switch (options & RubyRegexOptions.EncodingMask) {
#if !SILVERLIGHT
                case RubyRegexOptions.EUC: return RubyEncoding.EUCJP;
                case RubyRegexOptions.SJIS: return RubyEncoding.SJIS;
#endif
                case RubyRegexOptions.UTF8: return RubyEncoding.UTF8;
                case RubyRegexOptions.FIXED: return RubyEncoding.Binary;
                default: return null;
            }
        }

        internal static int GetCodePage(int nameInitial) {
            switch (nameInitial) {
#if !SILVERLIGHT
                case 'E':
                case 'e': return CodePageEUCJP;
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
                throw new NotSupportedException(String.Format("Encoding {0} (code page {1}) is not supported", Name, CodePage));
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
                case CodePageEUCJP: return EUCJP;
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

        public bool IsSingleByteCharacterSet {
            get {
                if (!_isSingleByteCharacterSet.HasValue) {
                    _isSingleByteCharacterSet = IsSBCS(CodePage);
                }

                return _isSingleByteCharacterSet.Value;
            }
        }

        public bool IsDoubleByteCharacterSet {
            get {
                if (!_isDoubleByteCharacterSet.HasValue) {
                    _isDoubleByteCharacterSet = IsDBCS(CodePage);
                }

                return _isDoubleByteCharacterSet.Value;
            }
        }
        
        private static int[] _sbsc;
        private static int[] _dbsc; 

        private static bool IsSBCS(int codepage) {
            if (_sbsc == null) {
                _sbsc = new int[] {
                    0, 37, 437, 500, 708, 720, 737, 775, 850, 852, 855, 857, 858, 860, 861, 862, 863, 864, 865, 866, 869, 870, 874, 875, 1026, 
                    1047, 1140, 1141, 1142, 1143, 1144, 1145, 1146, 1147, 1148, 1149, 1250, 1251, 1252, 1253, 1254, 1255, 1256, 1257, 1258, 
                    10000, 10004, 10005, 10006, 10007, 10010, 10017, 10021, 10029, 10079, 10081, 10082, 20105, 20106, 20107, 20108, 20127, 
                    20269, 20273, 20277, 20278, 20280, 20284, 20285, 20290, 20297, 20420, 20423, 20424, 20833, 20838, 20866, 20871, 20880, 
                    20905, 20924, 21025, 21866, 28592, 28593, 28594, 28595, 28596, 28597, 28598, 28599, 28603, 28605, 29001, 38598
                };
            }

            return Array.BinarySearch(_sbsc, codepage) >= 0;
        }

        private static bool IsDBCS(int codepage) {
            if (_dbsc == null) {
                _dbsc = new int[] {
                    932, 936, 949, 950, 1361, 10001, 10002, 10003, 10008, 20000, 20001, 20002, 20003, 20004, 20005, 20261, 20932, 20936, 
                    20949, 50227, 51936, 51949
                };
            }

            return Array.BinarySearch(_dbsc, codepage) >= 0;
        }

        public bool InUnicodeBasicPlane {
            get {
                // TODO: others
                return this == Ascii || this == Binary;
            }
        }

        public bool IsUnicodeEncoding {
            get {
                switch (CodePage) {
                    case CodePageUTF7:
                    case CodePageUTF8:
                    case CodePageUTF16BE:
                    case CodePageUTF16LE:
                    case CodePageUTF32BE:
                    case CodePageUTF32LE:
                        return true;
                }
                return false;
            }
        }

        private static ReadOnlyDictionary<string, string> _aliases;

        public static ReadOnlyDictionary<string, string> Aliases {
            get { return _aliases ?? (_aliases = CreateAliases()); } 
        }

        private static ReadOnlyDictionary<string, string> CreateAliases() {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                { "646", "US-ASCII" }, 
                { "ASCII", "US-ASCII" }, 
                { "ANSI_X3.4-1968", "US-ASCII" }, 
                { "BINARY", "ASCII-8BIT" }, 
                { "CP437", "IBM437" }, 
                { "CP737", "IBM737" }, 
                { "CP775", "IBM775" }, 
                { "CP857", "IBM857" }, 
                { "CP860", "IBM860" }, 
                { "CP861", "IBM861" }, 
                { "CP862", "IBM862" }, 
                { "CP863", "IBM863" }, 
                { "CP864", "IBM864" },
                { "CP865", "IBM865" }, 
                { "CP866", "IBM866" }, 
                { "CP869", "IBM869" }, 
                { "CP874", "Windows-874" }, 
                { "CP878", "KOI8-R" }, 
                { "CP932", "Windows-31J" }, 
                { "CP936", "GBK" }, 
                { "CP950", "Big5" }, 
                { "CP951", "Big5-HKSCS" }, 
                { "CP1258", "Windows-1258" },
                { "CP1252", "Windows-1252" }, 
                { "CP1250", "Windows-1250" }, 
                { "CP1256", "Windows-1256" }, 
                { "CP1251", "Windows-1251" },
                { "CP1253", "Windows-1253" }, 
                { "CP1255", "Windows-1255" }, 
                { "CP1254", "Windows-1254" }, 
                { "CP1257", "Windows-1257" }, 
                { "CP65000", "UTF-7" }, 
                { "CP65001", "UTF-8" }, 
                { "IBM850", "CP850" }, 
                { "eucJP", "EUC-JP" }, 
                { "eucKR", "EUC-KR" }, 
                // { "eucTW", "EUC-TW" }, 
                { "ISO2022-JP", "ISO-2022-JP" }, 
                // { "ISO2022-JP2", "ISO-2022-JP-2" }, 
                { "ISO8859-1", "ISO-8859-1" }, 
                { "ISO8859-2", "ISO-8859-2" }, 
                { "ISO8859-3", "ISO-8859-3" }, 
                { "ISO8859-4", "ISO-8859-4" }, 
                { "ISO8859-5", "ISO-8859-5" }, 
                { "ISO8859-6", "ISO-8859-6" }, 
                { "ISO8859-7", "ISO-8859-7" }, 
                { "ISO8859-8", "ISO-8859-8" }, 
                { "ISO8859-9", "ISO-8859-9" }, 
                // { "ISO8859-10", "ISO-8859-10" }, 
                { "ISO8859-11", "ISO-8859-11" }, 
                { "ISO8859-13", "ISO-8859-13" }, 
                // { "ISO8859-14", "ISO-8859-14" }, 
                { "ISO8859-15", "ISO-8859-15" }, 
                // { "ISO8859-16", "ISO-8859-16" }, 
                { "SJIS", "Shift_JIS" }, 
                { "csWindows31J", "Windows-31J" }, 
                // { "MacJapan", "MacJapanese" }, 
                // { "UTF-8-MAC", "UTF8-MAC" }, 
                // { "UTF-8-HFS", "UTF8-MAC" }, 
                { "UCS-2BE", "UTF-16BE" }, 
                { "UCS-4BE", "UTF-32BE" }, 
                { "UCS-4LE", "UTF-32LE" },  
            });
        }

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            // TODO: use static fields
            return Methods.CreateEncoding.OpCall(Expression.Constant(CodePage));
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

        public bool IsSingleByteCharacterSet {
            get {
                return this == Binary;
            }
        }

        public bool IsDoubleByteCharacterSet {
            get {
                return false;
            }
        }

        public bool InUnicodeBasicPlane {
            get {
                return this == Binary;
            }
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
