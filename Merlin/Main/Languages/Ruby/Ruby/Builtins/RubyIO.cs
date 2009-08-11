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
using System.IO;
using System.Threading;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Text;
using System.Diagnostics;

namespace IronRuby.Builtins {
    public enum IOMode {
        ReadOnlyFromStart,
        ReadWriteFromStart,
        WriteOnlyTruncate,
        ReadWriteTruncate,
        WriteOnlyAppend,
        ReadWriteAppend,
        Closed,
    }

    /// <summary>
    /// IO builtin class. Wraps a BCL Stream object. Implementation of Ruby methods is in IoOps.cs in IronRuby.Libraries assembly.
    /// </summary>
    public class RubyIO : IDisposable {
        private RubyContext/*!*/ _context;
        private Encoding/*!*/ _externalEncoding;
        private Encoding _internalEncoding;

        private Stream _stream;
        private bool _preserveEndOfLines;

        private IOMode _mode;
        private int _fileDescriptor;
        private bool _disposed;
        private bool _closed;
        private bool _autoFlush;
        private int _peekAhead;

        public const int SEEK_SET = 0;
        public const int SEEK_CUR = 1;
        public const int SEEK_END = 2;

        private const byte CR = (byte)'\r';
        private const byte LF = (byte)'\n';

        #region Construction

        public RubyIO(RubyContext/*!*/ context) {
            _context = context;
            _fileDescriptor = context.AddDescriptor(this);
            _disposed = false;
            _closed = false;
            _peekAhead = -1;
            _stream = Stream.Null;

            // TODO: enable setting
            _externalEncoding = BinaryEncoding.Instance;
            _internalEncoding = null;
        }

        public RubyIO(RubyContext/*!*/ context, Stream/*!*/ stream, string/*!*/ modeString)
            : this(context) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(modeString, "modeString");

            _mode = ParseIOMode(modeString, out _preserveEndOfLines);
            _stream = stream;

            ResetLineNumbersForReadOnlyFiles(context);
        }

        // TODO: hack
        public RubyIO(RubyContext/*!*/ context, StreamReader reader, StreamWriter writer, string/*!*/ modeString)
            : this(context) {
            _mode = ParseIOMode(modeString, out _preserveEndOfLines);
            _stream = new DuplexStream(reader, writer);

            ResetLineNumbersForReadOnlyFiles(context);
        }


        public RubyIO(RubyContext/*!*/ context, Stream/*!*/ stream, RubyFileMode mode)
            : this(context) {
            ContractUtils.RequiresNotNull(stream, "stream");

            _mode = ParseIOMode(mode, out _preserveEndOfLines);
            _stream = stream;

            ResetLineNumbersForReadOnlyFiles(context);
        }

        private void ResetLineNumbersForReadOnlyFiles(RubyContext/*!*/ context) {
            if (RubyIO.IsReadable(_mode)) {
                context.InputProvider.LastInputLineNumber = 0;
            }
        }

        #endregion

        public Encoding ExternalEncoding {
            get { return _externalEncoding; }
        }

        public Encoding InternalEncoding {
            get { return _internalEncoding; }
        }

        public int FileDescriptor {
            get { return _fileDescriptor; }
        }

        public IOMode Mode {
            get { return _mode; }
        }

        protected Stream/*!*/ Stream {
            get {
                if (_stream == null) {
                    throw RubyExceptions.CreateIOError("uninitialized stream");
                }

                return _stream; 
            }
        }

        public bool IsConsole {
            get { return _stream is ConsoleStream; } 
        }

        internal static bool IsConsoleDescriptor(int fileDescriptor) {
            return (fileDescriptor < 3);
        }

        public bool IsConsoleDescriptor() {
            return IsConsoleDescriptor(_fileDescriptor);
        }

        public bool Closed {
            get { return _closed; }
            set { _closed = value; }
        }

        public bool PreserveEndOfLines {
            get { return _preserveEndOfLines; }
            set { _preserveEndOfLines = value; }
        }

        public bool HasBufferedReadData { get; set; }
        public bool HasBufferedWriteData { get; set; }

        #region Mode

        public static IOMode ParseIOMode(RubyFileMode mode, out bool preserveEndOfLines) {
            preserveEndOfLines = ((mode & RubyFileMode.BINARY) != 0);

            IOMode io;
            RubyFileMode readWriteMode = mode & RubyFileMode.ReadWriteMask;

            if (readWriteMode == RubyFileMode.WRONLY) {
                io = ((mode & RubyFileMode.APPEND) != 0) ? IOMode.WriteOnlyAppend : IOMode.WriteOnlyTruncate;
            } else if (readWriteMode == RubyFileMode.RDONLY) {
                io = ((mode & RubyFileMode.APPEND) != 0) ? IOMode.ReadWriteFromStart : IOMode.ReadOnlyFromStart;
            } else if (readWriteMode == RubyFileMode.RDWR) {
                io = ((mode & RubyFileMode.APPEND) != 0) ? IOMode.ReadWriteAppend : IOMode.ReadWriteFromStart;
            } else {
                throw new ArgumentException("file mode must be one of WRONLY, RDONLY, RDWR");
            }

            return io;
        }

        public static IOMode ParseIOMode(string/*!*/ mode, out bool preserveEndOfLines) {
            int i = mode.Length - 1;
            if (i < 0) {
                // empty:
                preserveEndOfLines = false;
                return IOMode.ReadOnlyFromStart;
            }

            bool plus = (mode[i] == '+');
            if (plus) {
                i--;
            }

            if (i < 0) {
                throw IllegalMode(mode);
            }

            preserveEndOfLines = (mode[i] == 'b');
            if (preserveEndOfLines) {
                i--;
            }

            if (i != 0) {
                throw IllegalMode(mode);
            }

            switch (mode[0]) {
                case 'r':
                    return plus ? IOMode.ReadWriteFromStart : IOMode.ReadOnlyFromStart;

                case 'w':
                    return plus ? IOMode.ReadWriteTruncate : IOMode.WriteOnlyTruncate;

                case 'a':
                    return plus ? IOMode.ReadWriteAppend : IOMode.WriteOnlyAppend;

                default:
                    throw IllegalMode(mode);
            }
        }

        internal static ArgumentException/*!*/ IllegalMode(string modeString) {
            return new ArgumentException(String.Format("illegal access mode {0}", modeString));
        }

        public static bool IsReadable(IOMode mode) {
            return (mode == IOMode.ReadOnlyFromStart || 
                mode == IOMode.ReadWriteAppend || 
                mode == IOMode.ReadWriteFromStart || 
                mode == IOMode.ReadWriteTruncate);
        }

        #endregion
        
        #region IDisposable Members

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    if (_stream != null) {
                        _stream.Dispose();
                        _stream = null;
                    }
                }
                _disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RubyIO() {
            Dispose(false);
        }

        #endregion

        #region Instance methods

        public virtual WaitHandle/*!*/ CreateReadWaitHandle() {
            // TODO:
            throw new NotSupportedException();
        }

        public virtual WaitHandle/*!*/ CreateWriteWaitHandle() {
            // TODO:
            throw new NotSupportedException();
        }

        public virtual WaitHandle/*!*/ CreateErrorWaitHandle() {
            // TODO:
            throw new NotSupportedException();
        }

        public virtual int FileControl(int commandId, int arg) {
            // TODO:
            throw new NotSupportedException();
        }

        public virtual int FileControl(int commandId, byte[] arg) {
            // TODO:
            throw new NotSupportedException();
        }
        
        public void ResetIOMode(string/*!*/ modeString) {
            _mode = ParseIOMode(modeString, out _preserveEndOfLines);
        }

        public long Position {
            get {
                Assert.NotNull(_stream);
                return _stream.Position;
            }
        }

        public bool AutoFlush {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        public void AssertNotClosed() {
            if (_closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }
        }

        public void AssertOpenedForWriting() {
            AssertNotClosed();
            if (_mode == IOMode.ReadOnlyFromStart) {
                throw RubyExceptions.CreateIOError("not opened for writing");
            }
        }

        public void AssertOpenedForReading() {
            AssertNotClosed();
            if (_mode == IOMode.WriteOnlyAppend || _mode == IOMode.WriteOnlyTruncate) {
                throw RubyExceptions.CreateIOError("not opened for reading");
            }
        }

        public BinaryReader/*!*/ GetBinaryReader() {
            AssertOpenedForReading();
            return new BinaryReader(_stream);
        }

        public BinaryWriter/*!*/ GetBinaryWriter() {
            AssertOpenedForWriting();
            return new BinaryWriter(_stream);
        }

        public bool IsEndOfStream() {
            return PeekByte() == -1;
        }

        public void Close() {
            if (_stream != null) {
                _stream.Close();
            }
            _closed = true;
        }

        // TODO:
        public void CloseWriter() {
            var duplex = _stream as DuplexStream;
            if (duplex == null) {
                throw RubyExceptions.CreateIOError("closing non-duplex IO for writing");
            }
            duplex.Writer.Close();
        }

        // TODO:
        public void CloseReader() {
            var duplex = _stream as DuplexStream;
            if (duplex == null) {
                throw RubyExceptions.CreateIOError("closing non-duplex IO for reading");
            }
            duplex.Reader.Close();
        }

        public long Seek(long offset, SeekOrigin origin) {
            return _stream.Seek(offset, origin);
        }

        public void Flush() {
            _stream.Flush();
            HasBufferedReadData = false;
            HasBufferedWriteData = false;
        }

        public long Length {
            get { return _stream.Length; }
            set { _stream.SetLength(value); }
        }

        // returns the number of bytes written to the stream:
        public int Write(char[]/*!*/ buffer, int index, int count) {
            byte[] bytes = _externalEncoding.GetBytes(buffer, index, count);
            Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        // returns the number of bytes written to the stream:
        public int Write(string/*!*/ value) {
            byte[] bytes = _externalEncoding.GetBytes(value);
            Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        // returns the number of bytes written to the stream:
        public int Write(MutableString/*!*/ value) {
            byte[] bytes = value.ToByteArray();
            return Write(bytes, 0, bytes.Length);
        }

        public int Write(byte[]/*!*/ buffer, int index, int count) {
            HasBufferedWriteData = true;
            HasBufferedReadData = false;
            if (_preserveEndOfLines) {
                _stream.Write(buffer, index, count);
                return buffer.Length;
            } else {
                int bytesWritten = 0;
                int i = index;
                while (i < count) {
                    int j = i;
                    while (j < buffer.Length && buffer[j] != LF) {
                        j++;
                    }
                    _stream.Write(buffer, i, j - i);
                    bytesWritten += j - i;

                    if (j < buffer.Length) {
                        _stream.WriteByte(CR);
                        _stream.WriteByte(LF);
                        bytesWritten += 2;
                    }

                    i = j + 1;
                }

                return bytesWritten;
            }
        }

        public int PeekByte() {
            int result;
            if (_peekAhead != -1) {
                result = _peekAhead;
            } else if (!_stream.CanSeek) {
                result = _stream.ReadByte();
                _peekAhead = result;
            } else {
                long pos = _stream.Position;
                result = _stream.ReadByte();
                _stream.Position = pos;
            }
            return result;
        }

        public int ReadByte() {
            HasBufferedReadData = true;
            HasBufferedWriteData = false;
            if (_peekAhead != -1) {
                int result = _peekAhead;
                _peekAhead = -1;
                return result;
            }
            return _stream.ReadByte();
        }

        public int ReadBytes(byte[]/*!*/ buffer, int offset, int count) {
            HasBufferedReadData = true;
            HasBufferedWriteData = false;
            return _stream.Read(buffer, offset, count);
        }

        public int AppendBytes(MutableString/*!*/ buffer, int count) {
            ContractUtils.RequiresNotNull(buffer, "buffer");
            ContractUtils.Requires(count >= 0, "count");

            if (count == 0) {
                return 0;
            }

            buffer.SwitchToBytes();
            int initialBufferSize = buffer.GetByteCount();
            if (_preserveEndOfLines) {
                AppendRawBytes(buffer, count);
            } else {
                // allocate 3 more bytes at the end for a backstop and possible LF:
                buffer.EnsureCapacity(initialBufferSize + count + 3);
                byte[] bytes = buffer.GetByteArray();

                int done = initialBufferSize;
                bool eof;
                do {
                    AppendRawBytes(buffer, count);
                    int end = buffer.GetByteCount();
                    int bytesRead = end - done;
                    if (bytesRead == 0) {
                        break;
                    }
                    eof = bytesRead < count;

                    if (bytes[end - 1] == CR && PeekByte() == LF) {
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
                } while (count > 0 && !eof);
            }

            return buffer.GetByteCount() - initialBufferSize;
        }

        private void AppendRawBytes(MutableString/*!*/ buffer, int count) {
            Debug.Assert(count > 0);

            HasBufferedReadData = true;
            HasBufferedWriteData = false;

            if (_peekAhead != -1) {
                buffer.Append((byte)_peekAhead);
                _peekAhead = -1;
                count--;
            }
            buffer.Append(_stream, count);
        }

        private static int IndexOfCrLf(byte[]/*!*/ array, int i) {
            while (true) {
                if (array[i++] == CR && array[i] == LF) {
                    return i - 1;
                }
            }
        }

        public int ReadByteNormalizeEoln() {
            // TODO: encoding
            int first = ReadByte();
            if (first == '\r' && !_preserveEndOfLines) {
                int second = PeekByte();
                if (second == '\n') {
                    return ReadByte();
                }
            }

            return first;
        }

        public int PeekByteNormalizeEoln() {
            // TODO: encoding
            long position = _stream.Position;

            int first = PeekByte();
            if (first == -1) {
                return -1;
            }

            if (first == '\r' && !_preserveEndOfLines) {
                first = ReadByte();
                int second = PeekByte();
                if (second == '\n') {
                    return second;
                }

                _stream.Position = position;
            }

            return first;
        }

        public MutableString ReadLineOrParagraph(MutableString separator) {
            if (separator != null && separator.Length == 0) {
                return ReadParagraph();
            } else {
                return ReadLine(separator);
            }
        }

        public MutableString ReadLine(MutableString separator) {
            AssertOpenedForReading();

            int c = ReadByteNormalizeEoln();
            if (c == -1) {
                return null;
            }

            int separatorOffset = 0;
            MutableString result = MutableString.CreateMutable();

            do {
                result.Append((char)c);

                if (separator != null && c == separator.GetChar(separatorOffset)) {
                    if (separatorOffset == separator.Length - 1) {
                        break;
                    }
                    separatorOffset++;
                } else if (separatorOffset > 0) {
                    separatorOffset = 0;
                }

                c = ReadByteNormalizeEoln();
            } while (c != -1);

            return result;
        }

        public MutableString ReadParagraph() {
            var result = ReadLine(MutableString.Create("\n\n"));

            int c;
            while ((c = PeekByteNormalizeEoln()) != -1) {
                if (c != '\n') break;
                ReadByteNormalizeEoln();
            }

            return result;
        }

        #endregion

        public override string/*!*/ ToString() {
            return RubyUtils.ObjectToMutableString(_context, this).ToString();
        }
    }
}
