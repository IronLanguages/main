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
using System.Text;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime {

    /// <summary>
    /// ASCII-8BIT encoding: 
    /// 0xmn -> \u00mn
    /// \u00mn -> 0xmn, error otherwise
    /// </summary>
    public sealed class BinaryEncoding : Encoding {
        public static readonly Encoding/*!*/ Instance = new BinaryEncoding();

#if !SILVERLIGHT
        private BinaryEncoding()
            : base(RubyEncoding.CodePageBinary) {
#else
        private BinaryEncoding() 
            : base() {
#endif
        }

        public override int GetByteCount(char[]/*!*/ chars, int index, int count) {
            for (int i = 0; i < count; i++) {
                if (chars[index + i] > 0xff) {
                    // TODO: we don't support fallbacks (we should add the result of the fallback to the count):
                    throw new EncoderFallbackException();
                }
            }
            return count;
        }

        public override int GetCharCount(byte[]/*!*/ bytes, int index, int count) {
            return count;
        }

        public override int GetMaxByteCount(int charCount) {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount) {
            return byteCount;
        }

        public override int GetBytes(char[]/*!*/ chars, int charIndex, int charCount, byte[]/*!*/ bytes, int byteIndex) {
            ContractUtils.RequiresArrayRange(chars, charIndex, charCount, "charIndex", "charCount");
            ContractUtils.RequiresArrayRange(bytes, byteIndex, charCount, "byteIndex", "charCount");

            try {
                for (int i = 0; i < charCount; i++) {
                    bytes[byteIndex + i] = checked((byte)chars[charIndex + i]);
                }
            } catch (OverflowException) {
                // TODO: we don't support fallbacks
                throw new EncoderFallbackException();
            }

            return charCount;
        }

        public override int GetChars(byte[]/*!*/ bytes, int byteIndex, int byteCount, char[]/*!*/ chars, int charIndex) {
            ContractUtils.RequiresArrayRange(bytes, byteIndex, byteCount, "byteIndex", "byteCount");
            ContractUtils.RequiresArrayRange(chars, charIndex, byteCount, "charIndex", "byteCount");

            for (int i = 0; i < byteCount; i++) {
                chars[charIndex + i] = (char)bytes[byteIndex + i];
            }

            return byteCount;
        }

#if !SILVERLIGHT
        public override string/*!*/ EncodingName {
            get { return "ASCII-8BIT"; }
        }

        public override bool IsSingleByte {
            get { return true; }
        }
#endif

        public override string/*!*/ WebName {
            get { return "ASCII-8BIT"; }
        }
    }
}
