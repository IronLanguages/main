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
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime {
    using UnaryOpSite = CallSite<Func<CallSite, object, object>>;
    using BinaryOpSite = CallSite<Func<CallSite, object, object, object>>;
    using TernaryOpSite = CallSite<Func<CallSite, object, object, object, object>>;

    public sealed class IOWrapper : Stream {
        private readonly RubyContext _context;

        #region Call Sites

        private UnaryOpSite _closeSite;
        private UnaryOpSite _flushSite;
        private BinaryOpSite _writeSite;
        private BinaryOpSite _readSite;
        private TernaryOpSite _seekSite;
        private UnaryOpSite _tellSite;

        private UnaryOpSite CloseSite {
            get {
                if (_closeSite == null) {
                    _closeSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "close", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _closeSite;
            }
        }

        private UnaryOpSite FlushSite {
            get {
                if (_flushSite == null) {
                    _flushSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "flush", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _flushSite;
            }
        }

        private BinaryOpSite WriteSite {
            get {
                if (_writeSite == null) {
                    _writeSite = BinaryOpSite.Create(RubyCallAction.Make(_context, "write", RubyCallSignature.WithImplicitSelf(1)));
                }

                return _writeSite;
            }
        }

        private BinaryOpSite ReadSite {
            get {
                if (_readSite == null) {
                    _readSite = BinaryOpSite.Create(RubyCallAction.Make(_context, "read", RubyCallSignature.WithImplicitSelf(1)));
                }

                return _readSite;
            }
        }

        private TernaryOpSite SeekSite {
            get {
                if (_seekSite == null) {
                    _seekSite = TernaryOpSite.Create(RubyCallAction.Make(_context, "seek", RubyCallSignature.WithImplicitSelf(2)));
                }

                return _seekSite;
            }
        }

        private UnaryOpSite TellSite {
            get {
                if (_tellSite == null) {
                    _tellSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "tell", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _tellSite;
            }
        }

        private UnaryOpSite PosSite {
            get {
                if (_posSite == null) {
                    _posSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "pos", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _posSite;
            }
        }

        private UnaryOpSite EofQSite {
            get {
                if (_eofQSite == null) {
                    _eofQSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "eof?", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _eofQSite;
            }
        }

        private UnaryOpSite EofSite {
            get {
                if (_eofSite == null) {
                    _eofSite = UnaryOpSite.Create(RubyCallAction.Make(_context, "eof", RubyCallSignature.WithImplicitSelf(0)));
                }

                return _eofSite;
            }
        }

        #endregion

        private readonly object _io;
        private readonly bool _canRead;
        private readonly bool _canWrite;
        private readonly bool _canSeek;
        private readonly bool _canFlush;
        private readonly bool _canClose;
        private readonly byte[]/*!*/ _buffer;
        private int _writePos;
        private int _readPos;
        private int _readLen;
        private UnaryOpSite _posSite;
        private UnaryOpSite _eofQSite;
        private UnaryOpSite _eofSite;
        private RespondToStorage _respondToStorage;

        public IOWrapper(RubyContext/*!*/ context, object io, bool canRead, bool canWrite, bool canSeek, bool canFlush, bool canClose, int bufferSize) {
            Assert.NotNull(context);

            _context = context;
            _respondToStorage = new RespondToStorage(_context);

            _io = io;
            _canRead = canRead;
            _canWrite = canWrite;
            _canSeek = canSeek;
            _canFlush = canFlush;
            _canClose = canClose;
            _buffer = new byte[bufferSize];
            _writePos = 0;
            _readPos = 0;
            _readLen = 0;
        }

        public object UnderlyingObject {
            get { return _io; }
        }

        public override bool CanRead {
            get { return _canRead; }
        }

        public override bool CanSeek {
            get { return _canSeek; }
        }

        public override bool CanWrite {
            get { return _canWrite; }
        }

        public override long Length {
            get {
                if (_io is Stream) {
                    return ((Stream)_io).Length;
                } else {
                    long currentPos = this.Position;
                    Seek(0, SeekOrigin.End);
                    long result = this.Position;
                    this.Position = currentPos;
                    return result;
                }
            }
        }

        public override long Position {
            get {
                if (_io is Stream) {
                    return ((Stream)_io).Position;
                }
                // TODO: conversion
                if (Protocols.RespondTo(_respondToStorage, _io, "pos")) {
                    return (long)this.PosSite.Target(this.PosSite, _io);
                } else {
                    if (!_canSeek) {
                        throw new NotSupportedException();
                    }
                    return (long)this.TellSite.Target(this.TellSite, _io);
                }
            }
            set {
                if (!_canSeek) {
                    throw new NotSupportedException();
                }
                Seek(value, SeekOrigin.Begin);
            }
        }

        public bool Eof {
            get {
                if (Protocols.RespondTo(_respondToStorage, _io, "eof?")) {
                    return (bool)this.EofQSite.Target(this.EofQSite, _io);
                } else if (Protocols.RespondTo(_respondToStorage, _io, "eof")) {
                    return (bool)this.EofSite.Target(this.EofSite, _io);
                } else {
                    return this.Position >= this.Length;
                }
            }
        }

        public override void Flush() {
            FlushWrite();
            FlushRead();

            if (_canFlush) {
                try {
                    FlushSite.Target(FlushSite, _io);
                } catch (MissingMethodException) {
                    // nop
                }
            }
        }

        private void FlushWrite() {
            if (_writePos > 0) {
                WriteToObject();
            }
        }

        private void FlushRead() {
            if (_canSeek && (_readPos < _readLen)) {
                Seek(this._readPos - this._readLen, SeekOrigin.Current);
            }
            _readPos = 0;
            _readLen = 0;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Flush();

                base.Dispose(disposing);

                if (_canClose) {
                    try {
                        CloseSite.Target(CloseSite, _io);
                    } catch (MissingMethodException) {
                        // nop
                    }
                }
            } else {
                base.Dispose(disposing);
            }
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            if (!_canRead) {
                throw new NotSupportedException();
            }

            int size = _readLen - _readPos;
            if (size == 0) {
                FlushWrite();
                if (count > _buffer.Length) {
                    size = ReadFromObject(buffer, offset, count);
                    _readPos = 0;
                    _readLen = 0;
                    return size;
                }
                size = ReadFromObject(_buffer, 0, _buffer.Length);
                if (size == 0) {
                    return 0;
                }
                _readPos = 0;
                _readLen = size;
            }

            if (size > count) {
                size = count;
            }

            Buffer.BlockCopy(_buffer, _readPos, buffer, offset, size);
            _readPos += size;
            if (size < count) {
                int additionalSize = ReadFromObject(buffer, offset + size, count - size);
                size += additionalSize;
                _readPos = 0;
                _readLen = 0;
            }
            return size;
        }

        public override int ReadByte() {
            if (!_canRead) {
                throw new NotSupportedException();
            }
            if (_readPos == _readLen) {
                FlushWrite();

                _readLen = ReadFromObject(_buffer, 0, _buffer.Length);
                _readPos = 0;
                if (_readLen == 0) {
                    return -1;
                }

            }
            return _buffer[_readPos++];
        }

        private int ReadFromObject(byte[]/*!*/ buffer, int offset, int count) {
            // TODO: readpartial is called if available
            // TODO: conversion
            MutableString result = (MutableString)ReadSite.Target(ReadSite, _io, count);
            if (result == null) {
                return 0;
            } else {
                byte[] readdata = result.ConvertToBytes();
                Buffer.BlockCopy(readdata, 0, buffer, offset, readdata.Length);
                return readdata.Length;
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (!_canSeek) {
                throw new NotSupportedException();
            }

            int rubyOrigin = 0;
            switch (origin) {
                case SeekOrigin.Begin:
                    rubyOrigin = RubyIO.SEEK_SET;
                    break;
                case SeekOrigin.Current:
                    rubyOrigin = RubyIO.SEEK_CUR;
                    break;
                case SeekOrigin.End:
                    rubyOrigin = RubyIO.SEEK_END;
                    break;
            }

            SeekSite.Target(SeekSite, _io, offset, rubyOrigin);
            return Position;
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (!_canWrite) {
                throw new NotSupportedException();
            }

            if (_writePos == 0) {
                FlushRead();
            } else {
                int size = _buffer.Length - _writePos;
                if (size > 0) {
                    if (size > count) {
                        size = count;
                    }
                    Buffer.BlockCopy(buffer, offset, _buffer, _writePos, size);
                    _writePos += size;
                    if (size == count) {
                        return;
                    }
                    offset += size;
                    count -= size;
                }
                WriteToObject();
            }
            if (count >= _buffer.Length) {
                WriteToObject(buffer, offset, count);
            } else if (count > 0) {
                Buffer.BlockCopy(buffer, offset, _buffer, 0, count);
                _writePos = count;
            }
        }

        public override void WriteByte(byte value) {
            if (!_canWrite) {
                throw new NotSupportedException();
            }

            if (_writePos == 0) {
                FlushRead();
            }
            if (_writePos == _buffer.Length) {
                WriteToObject();
            }
            _buffer[_writePos++] = value;
        }

        private void WriteToObject() {
            WriteToObject(_buffer, 0, _writePos);
            _writePos = 0;
        }

        private void WriteToObject(byte[]/*!*/ buffer, int offset, int count) {
            MutableString argument = MutableString.CreateBinary(count);
            argument.Append(buffer, offset, count);
            WriteSite.Target(WriteSite, _io, argument);
        }
    }
}
