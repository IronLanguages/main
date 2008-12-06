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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// All encodings in Ruby are represented by instances of a single class. Therefore we need to wrap .NET Encoding class in RubyEncoding.
    /// Instances of this class are app-domain singletons. That's all right as far as the class is readonly and doesn't implement IRubyObject.
    /// Taint, frozen flags and instance variables need to be stored in per-runtime lookaside table.
    /// </summary>
    public class RubyEncoding {
        public static readonly RubyEncoding/*!*/ Binary = new RubyEncoding(BinaryEncoding.Instance);
        public static readonly RubyEncoding/*!*/ UTF8 = new RubyEncoding(Encoding.UTF8);
        public static readonly RubyEncoding/*!*/ Default = new RubyEncoding(StringUtils.DefaultEncoding);

        private static Dictionary<int, RubyEncoding> _Encodings;

        private readonly Encoding/*!*/ _encoding;

        private RubyEncoding(Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(encoding, "encoding");
            _encoding = encoding;
        }

        public Encoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public string/*!*/ Name {
            get { return _encoding.WebName; }
        }

        /// <exception cref="ArgumentException">Unknown encoding.</exception>
        public static Encoding/*!*/ GetEncodingByRubyName(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");

            switch (name.ToLower()) {
                case "binary":
                case "ascii":
                case "ascii-8bit": return BinaryEncoding.Instance;
#if SILVERLIGHT
                case "utf-8": return Encoding.UTF8;
                default: throw new ArgumentException(String.Format("Unknown encoding: '{0}'", name));
#else
                default: return Encoding.GetEncoding(name);
#endif
            }
        }

        public static RubyEncoding/*!*/ GetRubyEncoding(string/*!*/ name) {
            return GetRubyEncoding(GetEncodingByRubyName(name));
        }
        
#if SILVERLIGHT
        public static RubyEncoding/*!*/ GetRubyEncoding(Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(encoding, "encoding");
            if (encoding == BinaryEncoding.Instance) {
                return Binary;
            } else if (encoding == Encoding.UTF8) {
                return UTF8;
            } else {
                throw new ArgumentException(String.Format("Unknown encoding: '{0}'", encoding));
            }
        }

        internal static RubyEncoding/*!*/ GetRubyEncoding(int codepage) {
            switch (codepage) {
                case 0: return Binary;
                case 65001: return UTF8;
                default: throw new ArgumentException(String.Format("Unknown encoding codepage: {0}", codepage));
            }
        }
#else
        internal static RubyEncoding/*!*/ GetRubyEncoding(int codepage) {
            return GetRubyEncoding(null, codepage);
        }

        public static RubyEncoding/*!*/ GetRubyEncoding(Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(encoding, "encoding");
            return GetRubyEncoding(encoding, encoding.CodePage);
        }

        private static RubyEncoding/*!*/ GetRubyEncoding(Encoding encoding, int codepage) {
            switch (codepage) {
                case 0: return Binary;
                case 65001: return UTF8;
            }

            if (_Encodings == null) {
                Interlocked.CompareExchange(ref _Encodings, new Dictionary<int, RubyEncoding>(), null); 
            }

            RubyEncoding result;
            lock (_Encodings) {
                if (!_Encodings.TryGetValue(codepage, out result)) {
                    _Encodings.Add(codepage, result = new RubyEncoding(encoding ?? Encoding.GetEncoding(codepage)));
                }
            }

            return result;
        }
#endif

        internal static Encoding/*!*/ GetEncoding(int codepage) {
            switch (codepage) {
                case 0: return BinaryEncoding.Instance;
#if SILVERLIGHT
                default: throw Assert.Unreachable;
#else
                default: return Encoding.GetEncoding(codepage);
#endif
            }
        }

        internal static int GetCodePage(Encoding/*!*/ encoding) {
            Debug.Assert(encoding != null);

            if (encoding == BinaryEncoding.Instance) {
                return 0;
            }

#if SILVERLIGHT
            if (encoding == BinaryEncoding.UTF8) {
                return 65001;
            }

            throw Assert.Unreachable;
#else
            return encoding.CodePage;
#endif
        }

        internal static Encoding/*!*/ GetDefaultHostEncoding(RubyCompatibility compatibility) {
            return (compatibility >= RubyCompatibility.Ruby19) ? Encoding.UTF8 : BinaryEncoding.Instance;
        }

#if !SILVERLIGHT
        public static bool IsAsciiIdentity(Encoding/*!*/ encoding) {
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
#endif
    }
}
