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

using System.IO;

using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    public class MutableStringStream : Stream {
        private MutableString/*!*/ _string;
        private int _position;

        public MutableStringStream() 
            : this(MutableString.CreateMutable()) {
        }

        public MutableStringStream(MutableString/*!*/ basis) {
            _string = basis;
            _position = 0;
        }

        public MutableString/*!*/ String {
            get { return _string; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _string = value;
                _position = 0;
            }
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return true; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Flush() {
        }

        public override long Length {
            get { return _string.ConvertToBytes().Length; }
        }

        public override long Position {
            get {
                return _position;
            }
            set {
                if (value < 0) {
                    throw RubyExceptions.CreateIOError("Invalid argument");
                }
                _position = (int)value;
            }
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            int maxReadLen = _string.GetByteCount() - _position;
            if (count > maxReadLen) {
                count = maxReadLen;
            }
            for (int i = 0; i < count; i++) {
                buffer[offset + i] = _string.GetByte(_position++);
            }
            return count;
        }

        public override int ReadByte() {
            if (_position >= _string.GetByteCount()) {
                return -1;
            }
            return _string.GetByte(_position++);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            int byteCount = _string.GetByteCount();

            int position = _position;
            switch (origin) {
                case SeekOrigin.Begin:
                    position = (int)offset;
                    break;
                case SeekOrigin.End:
                    position = byteCount + (int)offset;
                    break;
                case SeekOrigin.Current:
                    position += (int)offset;
                    break;
            }
            Position = position;
            return Position;
        }

        public override void SetLength(long value) {
            int byteCount = _string.GetByteCount();
            if (byteCount < value) {
                _string.Append(0, (int)(value - byteCount));
            } else if (byteCount > value) {
                _string.Remove((int)value, byteCount - (int)value);
            }
        }

        public override void Write(byte[]/*!*/ buffer, int offset, int count) {
            _string.Insert(_position, buffer, offset, count);
            _position += count;
        }

        public override void WriteByte(byte value) {
            _string.Insert(_position, value);
            _position++;
        }
    }
}
