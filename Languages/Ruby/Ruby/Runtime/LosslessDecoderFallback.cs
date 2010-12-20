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
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// Decodes bytes with no information loss. Provides access to invalid byte sequences encountered in the string.
    /// </summary>
    internal sealed class LosslessDecoderFallback : DecoderFallback {
        internal const char InvalidCharacterPlaceholder = '\uffff';
        private List<byte[]> _invalidCharacters;
        public bool Track { get; set; }
        
        internal LosslessDecoderFallback() {
        }

        public override DecoderFallbackBuffer/*!*/ CreateFallbackBuffer() {
            return new Buffer(this);
        }

        public List<byte[]> InvalidCharacters {
            get { return _invalidCharacters; }
        }

        public override int MaxCharCount {
            get { return 1; }
        }

        internal sealed class Buffer : DecoderFallbackBuffer {
            private readonly LosslessDecoderFallback/*!*/ _fallback;
            private int _index;

            internal Buffer(LosslessDecoderFallback/*!*/ fallback) {
                Assert.NotNull(fallback);
                _fallback = fallback;
            }

            public override bool Fallback(byte[]/*!*/ bytesUnknown, int index) {
                if (_fallback.Track) {
                    if (_fallback._invalidCharacters == null) {
                        _fallback._invalidCharacters = new List<byte[]>();
                    }
                    // bytesUnknown is reused for multiple calls, so we need to copy its content
                    _fallback._invalidCharacters.Add(ArrayUtils.Copy(bytesUnknown));
                }
                _index = 0;
                return true;
            }

            public override char GetNextChar() {
                if (Remaining > 0) {
                    _index++;
                    return InvalidCharacterPlaceholder;
                } else {
                    return '\0';
                }
            }

            public override bool MovePrevious() {
                if (_index == 0) {
                    return false;
                }
                _index--;
                return true;
            }

            public override int Remaining {
                get { return 1 - _index; }
            }

            public override void Reset() {
                _index = 0;
            }
        }
    }
}

#endif