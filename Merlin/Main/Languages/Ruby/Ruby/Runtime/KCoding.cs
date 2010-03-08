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
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime {
    internal sealed class KCoding : Encoding {
        private readonly Encoding/*!*/ _encoding;

        private KCoding(Encoding/*!*/ encoding)
#if !SILVERLIGHT
            : base(encoding.CodePage)
#endif  
        {
            _encoding = encoding;
        }

        public static KCoding/*!*/ Create(int codepage, bool throwOnError) {
#if SILVERLIGHT
            Debug.Assert(codepage == RubyEncoding.CodePageUTF8);
            return new KCoding(new UTF8Encoding(false, throwOnError));
#else
            if (throwOnError) {
                return Create(codepage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            } else {
                return Create(codepage, EncoderFallback.ReplacementFallback, BinaryDecoderFallback.Instance);
            }
#endif
        }

#if !SILVERLIGHT
        private static Encoding CreateEncoding(int codepage, EncoderFallback/*!*/ encoderFallback, DecoderFallback/*!*/ decoderFallback) {
            return Create(Encoding.GetEncoding(codepage, encoderFallback, decoderFallback));
        }

        public static KCoding Create(int codepage, EncoderFallback/*!*/ encoderFallback, DecoderFallback/*!*/ decoderFallback) {
            var encoding = CreateEncoding(codepage, encoderFallback, decoderFallback);
            return encoding != null ? Create(encoding) : null;
        }

        private static KCoding/*!*/ Create(Encoding/*!*/ encoding) {
            var result = (KCoding)new KCoding(encoding).Clone();
            result.DecoderFallback = encoding.DecoderFallback;
            result.EncoderFallback = encoding.EncoderFallback;
            return result;
        }
#endif

        internal static int GetCodePage(int firstChar) {
            switch (firstChar) {
#if !SILVERLIGHT
                case 'E':
                case 'e': return RubyEncoding.CodePageEUC;
                case 'S':
                case 's': return RubyEncoding.CodePageSJIS;
#endif
                case 'U':
                case 'u': return RubyEncoding.CodePageUTF8;
                default: 
                    return -1;
            }
        }

        public string/*!*/ KCodeName {
            get {
#if SILVERLIGHT
                return "UTF8";
#else
                switch (_encoding.CodePage) {
                    case RubyEncoding.CodePageEUC: return "EUC";
                    case RubyEncoding.CodePageSJIS: return "SJIS";
                    case RubyEncoding.CodePageUTF8: return "UTF8";
                    default: throw Assert.Unreachable;
                }
#endif
            }
        }

        internal static string/*!*/ GetKCodeName(RubyEncoding encoding) {
            return (encoding == null) ? "NONE" : ((KCoding)encoding.Encoding).KCodeName;
        }

        public override string/*!*/ ToString() {
            return "KCODE (" + KCodeName + ")";
        }

        #region Encoding delegation

#if !SILVERLIGHT
        public override bool IsAlwaysNormalized(NormalizationForm form) {
            return _encoding.IsAlwaysNormalized(form);
        }
#endif

        public override int GetByteCount(char[]/*!*/ chars, int index, int count) {
            return _encoding.GetByteCount(chars, index, count);
        }

        public override int GetBytes(char[]/*!*/ chars, int charIndex, int charCount, byte[]/*!*/ bytes, int byteIndex) {
            return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override int GetCharCount(byte[]/*!*/ bytes, int index, int count) {
            return _encoding.GetCharCount(bytes, index, count);
        }

        public override int GetChars(byte[]/*!*/ bytes, int byteIndex, int byteCount, char[]/*!*/ chars, int charIndex) {
            return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount) {
            return _encoding.GetMaxByteCount(charCount);
        }

        public override int GetMaxCharCount(int byteCount) {
            return _encoding.GetMaxCharCount(byteCount);
        }

        public override Decoder/*!*/ GetDecoder() {
            return _encoding.GetDecoder();
        }

        public override Encoder/*!*/ GetEncoder() {
            return _encoding.GetEncoder();
        }

        #endregion
    }
}

