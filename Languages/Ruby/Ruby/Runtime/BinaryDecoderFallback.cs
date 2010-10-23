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

using System;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.Runtime {
    /// <summary>
    /// Widens invalid bytes to 16 bits and uses them as characters.
    /// </summary>
    internal sealed class BinaryDecoderFallback : DecoderFallback {
        internal static readonly BinaryDecoderFallback Instance = new BinaryDecoderFallback();

        private BinaryDecoderFallback() {
        }

        public override DecoderFallbackBuffer/*!*/ CreateFallbackBuffer() {
            return new Buffer();
        }

        public override int MaxCharCount {
            get { return 1; }
        }

        internal sealed class Buffer : DecoderFallbackBuffer {
            private int _index;
            private byte[] _bytes;

            internal Buffer() {
            }

            public override bool Fallback(byte[]/*!*/ bytesUnknown, int index) {
                _bytes = bytesUnknown;
                _index = 0;
                return true;
            }

            public override char GetNextChar() {
                return (Remaining > 0) ? (char)_bytes[_index++] : '\0';
            }

            public override bool MovePrevious() {
                if (_index == 0) {
                    return false;
                }
                _index--;
                return true;
            }

            public override int Remaining {
                get { return _bytes.Length - _index; }
            }

            public override void Reset() {
                _index = 0;
            }
        }
    }     
}

#endif