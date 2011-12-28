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
using System.IO;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    /// <summary>
    /// IO builtin class. Wraps a BCL Stream object. Implementation of Ruby methods is in IoOps.cs in IronRuby.Libraries assembly.
    /// </summary>
    public partial class RubyIO : IDisposable {
        private RubyContext/*!*/ _context;
        private RubyEncoding/*!*/ _externalEncoding;
        private RubyEncoding _internalEncoding;

        // -1 if uninitialized or closed:
        private int _fileDescriptor;

        // null if uninitialized or closed:
        private RubyBufferedStream _stream;

        private bool _autoFlush;
        private IOMode _mode;
        public int LineNumber { get; set; }

        #region Constants

        public const int SEEK_SET = 0;
        public const int SEEK_CUR = 1;
        public const int SEEK_END = 2;

        public static SeekOrigin ToSeekOrigin(int rubySeekOrigin) {
            switch (rubySeekOrigin) {
                case SEEK_SET: return SeekOrigin.Begin;
                case SEEK_END: return SeekOrigin.End;
                case SEEK_CUR: return SeekOrigin.Current;
                default: throw RubyExceptions.CreateArgumentError("Invalid argument");
            }
        }

        public static long GetSeekPosition(long length, long position, long seekOffset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin: return seekOffset;
                case SeekOrigin.End: return length + seekOffset;
                case SeekOrigin.Current: return position + seekOffset;
            }
            throw Assert.Unreachable;
        }

        #endregion

        #region Construction

        public RubyIO(RubyContext/*!*/ context) {
            ContractUtils.RequiresNotNull(context, "context");

            _context = context;
            _fileDescriptor = -1;
            _stream = null;
            _externalEncoding = context.DefaultExternalEncoding;
            _internalEncoding = context.DefaultInternalEncoding;
        }

        public RubyIO(RubyContext/*!*/ context, Stream/*!*/ stream, IOMode mode) 
            : this(context, stream, context.AllocateFileDescriptor(stream), mode) {
        }

        public RubyIO(RubyContext/*!*/ context, StreamReader reader, StreamWriter writer, IOMode mode)
            : this(context, new DuplexStream(reader, writer), mode) {
        }

        public RubyIO(RubyContext/*!*/ context, Stream/*!*/ stream, int descriptor, IOMode mode) 
            : this(context) {
            ContractUtils.RequiresNotNull(context, "context");
            ContractUtils.RequiresNotNull(stream, "stream");
            SetStream(stream);
            _mode = mode;
            _fileDescriptor = descriptor;
        }

        public void Reset(Stream/*!*/ stream, IOMode mode) {
            _mode = mode;
            SetStream(stream);
            SetFileDescriptor(Context.AllocateFileDescriptor(stream));
        }

        #endregion

        #region Descriptor, Encoding, Flags

        public RubyContext/*!*/ Context {
            get { return _context; }
        }

        public RubyEncoding ExternalEncoding {
            get { return _externalEncoding; }
            set { _externalEncoding = value; }
        }

        public RubyEncoding InternalEncoding {
            get { return _internalEncoding; }
            set { _internalEncoding = value; }
        }

        public int GetFileDescriptor() {
            RequireOpen();
            return _fileDescriptor;
        }

        public void SetFileDescriptor(int value) {
            ContractUtils.Requires(value >= 0);
            RequireOpen();
            _fileDescriptor = value; 
        }

        /// <summary>
        /// Returns true if the IO object represents stdin/stdout/stderr (no matter whether or not the actual streams are redirected).
        /// </summary>
        public ConsoleStreamType? ConsoleStreamType {
            get {
                var stream = GetStream();
                var console = stream.BaseStream as ConsoleStream;
                return console != null ? console.StreamType : (ConsoleStreamType?)null;
            } 
        }

        internal static bool IsConsoleDescriptor(int fileDescriptor) {
            return fileDescriptor >= 0 && fileDescriptor < 3;
        }

        public bool IsConsoleDescriptor() {
            return IsConsoleDescriptor(_fileDescriptor);
        }

        public bool Closed {
            get { return _mode.IsClosed(); }
        }

        public bool Initialized {
            get { return Closed || _stream != null; }
        }

        public bool PreserveEndOfLines {
            get { 
                return (_mode & IOMode.PreserveEndOfLines) != 0; 
            }
            set {
                if (value) {
                    _mode |= IOMode.PreserveEndOfLines;
                } else {
                    _mode &= ~IOMode.PreserveEndOfLines;
                }
            }
        }

        public bool AutoFlush {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        #endregion

        #region Basic Stream Operations

        public RubyBufferedStream/*!*/ GetStream() {
            if (Closed) {
                throw RubyExceptions.CreateIOError("closed stream");
            }

            RequireInitialized();
            return _stream;
        }

        public void SetStream(Stream/*!*/ stream) {
            ContractUtils.RequiresNotNull(stream, "stream");
            _stream = new RubyBufferedStream(stream, _context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19);
        }

        public void RequireInitialized() {
            if (!Closed && _stream == null) {
                throw RubyExceptions.CreateIOError("uninitialized stream");
            }
        }

        public void RequireOpen() {
            GetStream();
        }

        public void RequireWritable() {
            GetWritableStream();
        }

        public void RequireReadable() {
            GetReadableStream();
        }

        public RubyBufferedStream/*!*/ GetWritableStream() {
            var result = GetStream();
            if (!_mode.CanWrite()) {
                throw RubyExceptions.CreateIOError("not opened for writing");
            }
            if (!result.CanWrite) {
                throw RubyExceptions.CreateEBADF();
            }
            return result;
        }

        public RubyBufferedStream/*!*/ GetReadableStream() {
            var result = GetStream();
            if (!_mode.CanRead()) {
                throw RubyExceptions.CreateIOError("not opened for reading");
            }
            if (!result.CanRead) {
                throw RubyExceptions.CreateEBADF();
            }
            return result;
        }

        public long Position {
            get {
                var stream = GetStream();
                try {
                    return stream.Position;
                } catch (ObjectDisposedException) {
                    throw RubyExceptions.CreateEBADF();
                }
            }
            set {
                var stream = GetStream();
                try {
                    stream.Position = value;
                } catch (ObjectDisposedException) {
                    throw RubyExceptions.CreateEBADF();
                }
            }
        }

        public void Seek(long offset, SeekOrigin origin) {
            var stream = GetStream();
            try {
                stream.Seek(offset, origin);
            } catch (IOException) {
                throw RubyExceptions.CreateEINVAL();
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public void Flush() {
            var stream = GetStream();
            try {
                stream.Flush();
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public long Length {
            get {
                var stream = GetStream();
                try {
                    return stream.Length;
                } catch (ObjectDisposedException) {
                    throw RubyExceptions.CreateEBADF();
                }
            }

            set {
                var stream = GetStream();
                try {
                    stream.SetLength(value);
                } catch (ObjectDisposedException) {
                    throw RubyExceptions.CreateIOError("closed stream");
                } catch (NotSupportedException) {
                    throw RubyExceptions.CreateIOError("not opened for writing");
                }
            }
        }

        public int WriteBytes(byte[]/*!*/ buffer, int index, int count) {
            ContractUtils.RequiresNotNull(buffer, "buffer");
            return WriteBytes(buffer, null, index, count);
        }

        public int WriteBytes(MutableString/*!*/ buffer, int index, int count) {
            ContractUtils.RequiresNotNull(buffer, "buffer");
            return WriteBytes(null, buffer, index, count);
        }

        // TODO: transcoding
        private int WriteBytes(byte[] bytes, MutableString str, int index, int count) {
            var stream = GetWritableStream();

            if ((_mode & IOMode.WriteAppends) != 0 && stream.CanSeek) {
                stream.Seek(0, SeekOrigin.End);
            }

            try {
                if (bytes != null) {
                    return stream.WriteBytes(bytes, index, count, PreserveEndOfLines);
                } else {
                    return stream.WriteBytes(str, index, count, PreserveEndOfLines);
                }
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public void Dispose() {
            Close();
        }

        public void Close() {
            int fd = _fileDescriptor;
            _mode = _mode.Close();
            _fileDescriptor = -1;

            if (_stream != null) {
                _stream = null;
                _context.CloseStream(fd);
            }
        }

        public void CloseWriter() {
            var duplex = GetStream().BaseStream as DuplexStream;
            if (duplex == null && _mode.CanRead() || duplex != null && !_mode.CanWrite()) {
                throw RubyExceptions.CreateIOError("closing non-duplex IO for writing");
            }
            
            if (duplex != null) {
                duplex.Writer.Dispose();
            }

            _mode = _mode.CloseWrite();
            if (_mode.IsClosed()) {
                Close();
            }
        }

        public void CloseReader() {
            var duplex = GetStream().BaseStream as DuplexStream;
            if (duplex == null && _mode.CanWrite() || duplex != null && !_mode.CanRead()) {
                throw RubyExceptions.CreateIOError("closing non-duplex IO for reading");
            } 
            
            if (duplex != null) {
                duplex.Reader.Dispose();
            }

            _mode = _mode.CloseRead();
            if (_mode.IsClosed()) {
                Close();
            }
        }

        public IOMode Mode {
            get { return _mode; }
            set { _mode = value; }
        }

        #endregion

        #region Operations

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

        public virtual int SetReadTimeout(int timeout) {
            if (timeout > 0) {
                throw RubyExceptions.CreateEBADF();
            }
            return 0;
        }

        public virtual void NonBlockingOperation(Action operation, bool isRead) {
            throw RubyExceptions.CreateEBADF();
        }

        public virtual int FileControl(int commandId, int arg) {
            GetStream();

            // TODO:
            throw new NotSupportedException();
        }

        public virtual int FileControl(int commandId, byte[] arg) {
            GetStream();

            // TODO:
            throw new NotSupportedException();
        }

        public BinaryReader/*!*/ GetBinaryReader() {
            return new BinaryReader(GetReadableStream());
        }

        public BinaryWriter/*!*/ GetBinaryWriter() {
            return new BinaryWriter(GetWritableStream());
        }

        public bool IsEndOfStream() {
            return GetReadableStream().PeekByte() == -1;
        }

        // returns the number of bytes written to the stream:
        public int WriteBytes(char[]/*!*/ buffer, int index, int count) {
            byte[] bytes = _externalEncoding.StrictEncoding.GetBytes(buffer, index, count);
            return WriteBytes(bytes, 0, bytes.Length);
        }

        // returns the number of bytes written to the stream:
        public int WriteBytes(string/*!*/ value) {
            byte[] bytes = _externalEncoding.StrictEncoding.GetBytes(value);
            return WriteBytes(bytes, 0, bytes.Length);
        }

        public int AppendBytes(MutableString/*!*/ buffer, int count) {
            var stream = GetReadableStream();
            try {
                return stream.AppendBytes(buffer, count, PreserveEndOfLines);
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public MutableString ReadLineOrParagraph(MutableString separator, int limit) {
            var stream = GetReadableStream();
            try {
                return stream.ReadLineOrParagraph(separator, _externalEncoding, PreserveEndOfLines, limit >= 0 ? limit : Int32.MaxValue);
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public int ReadByteNormalizeEoln() {
            var stream = GetReadableStream();
            try {
                return stream.ReadByteNormalizeEoln(PreserveEndOfLines);
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public int PeekByteNormalizeEoln() {
            var stream = GetReadableStream();
            try {
                return stream.PeekByteNormalizeEoln(PreserveEndOfLines);
            } catch (ObjectDisposedException) {
                throw RubyExceptions.CreateEBADF();
            }
        }

        public void PushBack(byte b) {
            GetStream().PushBack(b);            
        }

        #endregion

        public override string/*!*/ ToString() {
            return RubyUtils.ObjectToMutableString(_context, this).ToString();
        }
    }
}
