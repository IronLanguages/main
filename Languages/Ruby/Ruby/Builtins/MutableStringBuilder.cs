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

using System.Diagnostics;
using System;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting.Utils;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    internal sealed class MutableStringBuilder {
        private RubyEncoding/*!*/ _encoding;
        private bool _isAscii;

        // character cache: converted to bytes each time a byte is appended:
        private char[] _chars;

        // binary content: once a byte is appended the result is a binary string:
        private byte[] _bytes;

        private int _charCount;
        private int _byteCount;

        public MutableStringBuilder(RubyEncoding/*!*/ encoding) {
            Assert.NotNull(encoding);
            _encoding = encoding;
            _isAscii = true;
        }

        public bool IsAscii {
            get { return _isAscii; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { 
                return _encoding; 
            }
            set {
                Debug.Assert(value != null);
                _encoding = value; 
            }
        }

        public void Append(char c) {
            if (c >= 0x80) {
                _isAscii = false;
            }
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + 1);
            _chars[oldCount] = c;
        }

        public void AppendAscii(char c) {
            Debug.Assert(c < 0x80);
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + 1);
            _chars[oldCount] = c;
        }

        public void AppendUnicodeCodepoint(int codepoint) {
            if (codepoint < 0x10000) {
                // code-points [0xd800 .. 0xdffff] are not treated as invalid
                Append((char)codepoint);
            } else {
                int oldCount = _charCount;
                Ensure(ref _chars, _charCount = oldCount + 2);

                codepoint -= 0x10000;
                _chars[oldCount] = (char)((codepoint / 0x400) + 0xd800);
                _chars[oldCount + 1] = (char)((codepoint % 0x400) + 0xdc00);
                _isAscii = false;
            }
        }

        public void AppendAscii(char[] chars, int start, int count) {
            Debug.Assert(Utils.IsAscii(chars, start, count));
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + count);
            Buffer.BlockCopy(chars, start << 1, _chars, oldCount << 1, count << 1);
        }

        public void Append(byte b) {
            // b is ASCII => we can treat b as char if there are any characters cached or if no bytes were appended.
            // otherwise => b needs to be appended to byte[].
            if (b < 0x80 && (_charCount != 0 || _byteCount == 0)) {
                AppendAscii((char)b);
            } else if (_encoding == RubyEncoding.Binary) {
                Append((char)b);
            } else {
                AppendByte(b);
            }
        }

        private void AppendByte(byte b) {
            int oldCount;
            if (_charCount > 0) {
                oldCount = _byteCount;
                Ensure(ref _bytes, _byteCount = oldCount + _encoding.Encoding.GetByteCount(_chars, 0, _charCount) + 1);

                oldCount += _encoding.Encoding.GetBytes(_chars, 0, _charCount, _bytes, oldCount);
                _bytes[oldCount++] = b;

                Debug.Assert(oldCount == _byteCount);
                ClearChars();
                _charCount = 0;
            } else {
                oldCount = _byteCount;
                Ensure(ref _bytes, _byteCount = oldCount + 1);
                _bytes[oldCount] = b;
            }

            if (b >= 0x80) {
                _isAscii = false;
            }
        }

        private void Ensure<T>(ref T[] array, int size) {
            if (array == null) {
                array = new T[Math.Max(16, size)];
            } else {
                Utils.Resize(ref array, size);
            }
        }

        // returns string or byte[]
        public object ToValue() {
            object result;
            if (_byteCount > 0) {
                if (_charCount > 0) {
                    Array.Resize(ref _bytes, _byteCount + _encoding.Encoding.GetByteCount(_chars, 0, _charCount));
                    _encoding.Encoding.GetBytes(_chars, 0, _charCount, _bytes, _byteCount);
                } else {
                    Array.Resize(ref _bytes, _byteCount);
                }

                result = _bytes;
            } else if (_charCount > 0) {
                result = new String(_chars, 0, _charCount);
            } else {
                result = String.Empty;
            }
            _bytes = null;
            _chars = null;
            _charCount = _byteCount = 0;
            return result;
        }

        public MutableString/*!*/ ToMutableString() {
            object value = ToValue();
            string str = value as string;
            if (str != null) {
                return MutableString.Create(str, _encoding);
            } else {
                return MutableString.CreateBinary((byte[])value, _encoding);
            }
        }

        [Conditional("DEBUG")]
        private void ClearChars() {
            _chars = null;
        }
    }


}
