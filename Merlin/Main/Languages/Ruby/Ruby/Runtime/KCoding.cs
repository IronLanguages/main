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
#if SILVERLIGHT
        private readonly Encoding _encoding = null;
        private KCoding() { }
#else
        private readonly Encoding/*!*/ _encoding;

        private KCoding(Encoding/*!*/ encoding)
            : base(encoding.CodePage) {
            _encoding = encoding;
        }

        internal static int GetCodePage(int firstChar) {
            switch (firstChar) {
                case 'E':
                case 'e': return RubyEncoding.CodePageEUC;
                case 'S':
                case 's': return RubyEncoding.CodePageSJIS;
                case 'U':
                case 'u': return RubyEncoding.CodePageUTF8;
                default: 
                    return -1;
            }
        }

        private static Encoding CreateEncoding(int codepage, EncoderFallback/*!*/ encoderFallback, DecoderFallback/*!*/ decoderFallback) {
            return new KCoding(Encoding.GetEncoding(codepage, encoderFallback, decoderFallback));
        }

        public static KCoding Create(int codepage, bool throwOnError) {
            if (throwOnError) {
                return Create(codepage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            } else {
                return Create(codepage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
            }
        }

        public static KCoding Create(int codepage, EncoderFallback/*!*/ encoderFallback, DecoderFallback/*!*/ decoderFallback) {
            var encoding = CreateEncoding(codepage, encoderFallback, decoderFallback);
            return encoding != null ? new KCoding(encoding) : null;
        }

        public string/*!*/ KCodeName {
            get {
                switch (_encoding.CodePage) {
                    case RubyEncoding.CodePageEUC: return "EUC";
                    case RubyEncoding.CodePageSJIS: return "SJIS";
                    case RubyEncoding.CodePageUTF8: return "UTF8";
                    default: throw Assert.Unreachable;
                }
            }
        }

        internal static string/*!*/ GetKCodeName(RubyEncoding encoding) {
            return (encoding == null) ? "NONE" : ((KCoding)encoding.Encoding).KCodeName;
        }

        public override string/*!*/ ToString() {
            return "KCODE (" + KCodeName + ")";
        }

        public override bool IsAlwaysNormalized(NormalizationForm form) {
            return _encoding.IsAlwaysNormalized(form);
        }
#endif

        #region Encoding delegation

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

