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
using System.Diagnostics;
using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    public class RubyBufferedStream : Stream {
        private const int MaxBufferSize = sizeof(uint);
        private readonly Stream/*!*/ _stream;

        // Read buffer: if bufferSize > 0 then the next byte to read is the first (lowest) byte in the buffer:
        private uint _buffer;
        private int _bufferSize;

        private const byte CR = (byte)'\r';
        private const byte LF = (byte)'\n';

        public RubyBufferedStream(Stream/*!*/ stream) {
            _stream = stream;
        }

        public Stream/*!*/ BaseStream {
            get { return _stream; }
        }

        public bool DataBuffered {
            get { return _bufferSize > 0; }
        }

        private byte PeekBufferByte(int i) {
            Debug.Assert(i < _bufferSize);
            return (byte)((_buffer >> (i * 8)) & 0xff);
        }

        private void LoadBufferByte(byte b) {
            Debug.Assert(_bufferSize < MaxBufferSize);
            _buffer |= (uint)b << (_bufferSize * 8);
            _bufferSize++;
        }

        private byte ReadBufferByte() {
            Debug.Assert(_bufferSize > 0);
            byte result = (byte)(_buffer & 0xff);
            _buffer >>= 8;
            _bufferSize--;
            return result;
        }

        public void PushBack(byte b) {
            if (_bufferSize == MaxBufferSize) {
                throw RubyExceptions.CreateIOError("ungetc failed: not enough space in buffer");
            }

            _buffer = (_buffer << 8) | b;
            _bufferSize++;
        }

        public override long Position {
            get {
                // TODO: this seems to be bug in MRI: you can read(0); ungetc(x); at the beginning of a stream, yet
                // you can't do ungetc(x) at the beginning of the stream.
                // (see http://redmine.ruby-lang.org/issues/show/1909)
                return Math.Max(_stream.Position - _bufferSize, 0);
            }
            set {
                // seek clears any buffer content (including ungetc):
                FlushRead();
                _stream.Position = value;
            }
        }

        public override long Seek(long pos, SeekOrigin origin) {
            FlushRead();
            return _stream.Seek(pos, origin);
        }

        private void FlushRead() {
            _buffer = 0;
            _bufferSize = 0;
        }

        public override void Write(byte[]/*!*/ buffer, int offset, int count) {
            FlushRead();

            _stream.Write(buffer, offset, count);
        }

        public int WriteBytes(MutableString/*!*/ buffer, int offset, int count, bool preserveEndOfLines) {
            // TODO: this is not safe, we are passing an internal pointer to the byte[] content of MutableString to the Stream:
            return WriteBytes(buffer.SwitchToBytes().GetByteArray(), offset, count, preserveEndOfLines);
        }

        public int WriteBytes(byte[]/*!*/ buffer, int offset, int count, bool preserveEndOfLines) {
            ContractUtils.RequiresArrayRange(buffer.Length, offset, count, "offset", "count");
            FlushRead();

            if (preserveEndOfLines) {
                _stream.Write(buffer, offset, count);
                return count;
            } else {
                int bytesWritten = 0;
                int i = offset;
                int end = offset + count;
                while (i < end) {
                    int j = i;
                    while (j < end && buffer[j] != LF) {
                        j++;
                    }
                    _stream.Write(buffer, i, j - i);
                    bytesWritten += j - i;

                    if (j < end) {
                        _stream.WriteByte(CR);
                        _stream.WriteByte(LF);
                        bytesWritten += 2;
                    }

                    i = j + 1;
                }

                return bytesWritten;
            }
        }

        public int PeekByte(int i) {
            Debug.Assert(i < MaxBufferSize);
            if (i < _bufferSize) {
                return PeekBufferByte(i);
            }

            int result;
            if (_stream.CanSeek) {
                long oldPos = _stream.Position;
                _stream.Position = Position + i;
                result = _stream.ReadByte();
                _stream.Position = oldPos;
                return result;
            }

            while (true) {
                result = _stream.ReadByte();
                if (result == -1) {
                    return result;
                }
                LoadBufferByte((byte)result);
                if (i < _bufferSize) {
                    return result;
                }
            }
        }

        public override int ReadByte() {
            return (_bufferSize > 0) ? ReadBufferByte() : _stream.ReadByte();
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            while (count > 0 && _bufferSize > 0) {
                buffer[offset++] = ReadBufferByte();
                count--;
            }

            return _stream.Read(buffer, offset, count);
        }

        // count == Int32.MaxValue means means no bound
        public int AppendBytes(MutableString/*!*/ buffer, int count, bool preserveEndOfLines) {
            ContractUtils.RequiresNotNull(buffer, "buffer");
            ContractUtils.Requires(count >= 0, "count");

            if (count == 0) {
                return 0;
            }

            bool readAll = count == Int32.MaxValue;

            buffer.SwitchToBytes();
            int initialBufferSize = buffer.GetByteCount();
            if (preserveEndOfLines) {
                AppendRawBytes(buffer, count);
            } else {
                // allocate 3 more bytes at the end for a backstop and possible LF:
                byte[] bytes = Utils.EmptyBytes;

                int done = initialBufferSize;
                bool eof;
                do {
                    AppendRawBytes(buffer, readAll ? 1024 : count);
                    int end = buffer.GetByteCount();
                    int bytesRead = end - done;
                    if (bytesRead == 0) {
                        break;
                    }

                    eof = bytesRead < count;

                    buffer.EnsureCapacity(end + 3);
                    bytes = buffer.GetByteArray();

                    if (bytes[end - 1] == CR && PeekByte(0) == LF) {
                        ReadByte();
                        bytes[end++] = LF;
                    }

                    // insert backstop:
                    bytes[end] = CR;
                    bytes[end + 1] = LF;

                    int last = IndexOfCrLf(bytes, done);
                    count -= last - done;
                    done = last;
                    while (last < end) {
                        int next = IndexOfCrLf(bytes, last + 2);
                        int chunk = next - last - 1;
                        Buffer.BlockCopy(bytes, last + 1, bytes, done, chunk);
                        done += chunk;
                        count -= chunk;
                        last = next;
                    }
                    buffer.Remove(done);
                } while (readAll || count > 0 && !eof);
            }

            if (readAll) {
                buffer.TrimExcess();
            }

            return buffer.GetByteCount() - initialBufferSize;
        }

        private void AppendRawBytes(MutableString/*!*/ buffer, int count) {
            Debug.Assert(count > 0);

            int remaining = count;

            while (_bufferSize > 0) {
                buffer.Append(ReadBufferByte());
                remaining--;
            }

            if (count == Int32.MaxValue) {
                const int chunk = 1024;

                int done = buffer.GetByteCount();
                int bytesRead;
                do {
                    buffer.Append(_stream, chunk);
                    bytesRead = buffer.GetByteCount() - done;
                    done += bytesRead;
                } while (bytesRead == chunk);
            } else {
                buffer.Append(_stream, remaining);
            }
        }

        private static int IndexOfCrLf(byte[]/*!*/ array, int i) {
            while (true) {
                if (array[i++] == CR && array[i] == LF) {
                    return i - 1;
                }
            }
        }

        public int ReadByteNormalizeEoln(bool preserveEndOfLines) {
            int first = ReadByte();
            if (first == '\r' && !preserveEndOfLines) {
                int second = PeekByte(0);
                if (second == '\n') {
                    return ReadByte();
                }
            }

            return first;
        }

        public int PeekByteNormalizeEoln(bool preserveEndOfLines) {
            int first = PeekByte(0);
            if (first == -1) {
                return -1;
            }

            if (first == '\r' && !preserveEndOfLines && PeekByte(1) == '\n') {
                return '\n';
            }

            return first;
        }

        public MutableString ReadLineOrParagraph(MutableString separator, RubyEncoding/*!*/ encoding, bool preserveEndOfLines) {
            if (separator == null) {
                var result = MutableString.CreateBinary();
                return AppendBytes(result, Int32.MaxValue, preserveEndOfLines) == 0 ? null : result;
            } else if (separator.IsEmpty) {
                return ReadParagraph(encoding, preserveEndOfLines);
            } else {
                return ReadLine(separator, encoding, preserveEndOfLines);
            }
        }

        public MutableString ReadParagraph(RubyEncoding/*!*/ encoding, bool preserveEndOfLines) {
            var result = ReadLine(MutableString.CreateAscii("\n\n"), encoding, preserveEndOfLines);

            int c;
            while ((c = PeekByteNormalizeEoln(preserveEndOfLines)) != -1) {
                if (c != '\n') {
                    break;
                }
                ReadByteNormalizeEoln(preserveEndOfLines);
            }

            return result;
        }

        public MutableString ReadLine(MutableString/*!*/ separator, RubyEncoding/*!*/ encoding, bool preserveEndOfLines) {
            int b = ReadByteNormalizeEoln(preserveEndOfLines);
            if (b == -1) {
                return null;
            }

            int separatorOffset = 0;
            MutableString result = MutableString.CreateBinary(encoding);

            do {
                result.Append((byte)b);

                if (b == separator.GetByte(separatorOffset)) {
                    if (separatorOffset == separator.Length - 1) {
                        break;
                    }
                    separatorOffset++;
                } else if (separatorOffset > 0) {
                    separatorOffset = 0;
                }

                b = ReadByteNormalizeEoln(preserveEndOfLines);
            } while (b != -1);

            return result;
        }

        public override void Close() {
            FlushRead();
            _stream.Close();
        }

        public override bool CanRead {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite {
            get { return _stream.CanWrite; }
        }

        public override void Flush() {
            FlushRead();
            _stream.Flush();
        }

        public override long Length {
            get { return _stream.Length; }
        }

        public override void SetLength(long value) {
            _stream.SetLength(value);
        }
    }
}
