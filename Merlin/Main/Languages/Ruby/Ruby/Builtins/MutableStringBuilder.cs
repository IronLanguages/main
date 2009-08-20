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

using System.Diagnostics;
using System;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting.Utils;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    public sealed class MutableStringBuilder {
        private readonly RubyEncoding/*!*/ _encoding;

        // character cache: converted to bytes each time a byte is appended:
        private char[] _chars;

        // binary content: once a byte is appended the result is a binary string:
        private byte[] _bytes;

        private int _charCount;
        private int _byteCount;

        public MutableStringBuilder(RubyEncoding/*!*/ encoding) {
            Assert.NotNull(encoding);
            _encoding = encoding;
        }

        public MutableStringBuilder(string/*!*/ value, RubyEncoding/*!*/ encoding) 
            : this(encoding) {
            Assert.NotNull(value);
            _chars = value.ToCharArray();
            _charCount = _chars.Length;
        }

        public void Append(char c) {
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + 1);
            _chars[oldCount] = c;
        }

        public void Append(char c1, char c2) {
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + 2);
            _chars[oldCount] = c1;
            _chars[oldCount + 1] = c2;
        }

        public void Append(char c1, char c2, char c3) {
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + 3);
            _chars[oldCount] = c1;
            _chars[oldCount + 1] = c2;
            _chars[oldCount + 2] = c3;
        }

        public void Append(char[] chars, int start, int count) {
            int oldCount = _charCount;
            Ensure(ref _chars, _charCount = oldCount + count);
            Buffer.BlockCopy(chars, start << 1, _chars, oldCount << 1, count << 1);
        }

        public void Append(byte b) {
            // b is ASCII => we can treat b as char if there are any characters cached or if no bytes were appended.
            // otherwise => b needs to be appended to byte[].
            if (_encoding == RubyEncoding.Binary || b <= 0x7f && (_charCount != 0 || _byteCount == 0)) {
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
