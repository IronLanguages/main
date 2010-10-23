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

    public class IOWrapper : Stream {
        private readonly CallSite<Func<CallSite, object, object, object>> _writeSite;
        private readonly CallSite<Func<CallSite, object, object, object>> _readSite;
        private readonly CallSite<Func<CallSite, object, object, object, object>> _seekSite;
        private readonly CallSite<Func<CallSite, object, object>> _tellSite;
            

        private readonly object _obj;
        private readonly bool _canRead;
        private readonly bool _canWrite;
        private readonly bool _canSeek;
        private readonly bool _canFlush;
        private readonly bool _canBeClosed;
        private readonly byte[]/*!*/ _buffer;
        private int _writePos;
        private int _readPos;
        private int _readLen;

        public IOWrapper(RubyContext/*!*/ context, object io, bool canRead, bool canWrite, bool canSeek, bool canFlush, bool canBeClosed, int bufferSize) {
            Assert.NotNull(context);

            _writeSite = CallSite<Func<CallSite, object, object, object>>.Create(
                RubyCallAction.Make(context, "write", RubyCallSignature.WithImplicitSelf(1))
            );
            _readSite = CallSite<Func<CallSite, object, object, object>>.Create(
                RubyCallAction.Make(context, "read", RubyCallSignature.WithImplicitSelf(1))
            );
            _seekSite = CallSite<Func<CallSite, object, object, object, object>>.Create(
                RubyCallAction.Make(context, "seek", RubyCallSignature.WithImplicitSelf(2))
            );
            _tellSite = CallSite<Func<CallSite, object, object>>.Create(
                RubyCallAction.Make(context, "tell", RubyCallSignature.WithImplicitSelf(0))
            );

            _obj = io;

            _canRead = canRead;
            _canWrite = canWrite;
            _canSeek = canSeek;
            _canFlush = canFlush;
            _canBeClosed = canBeClosed;
            _buffer = new byte[bufferSize];
            _writePos = 0;
            _readPos = 0;
            _readLen = 0;
        }

        public object UnderlyingObject {
            get { return _obj; }
        }

        public override bool CanRead {
            get { return _canRead; }
        }

        public override bool CanSeek {
            get { return _canSeek; }
        }

        public bool CanBeClosed {
            get { return _canBeClosed; }
        }

        public override bool CanWrite {
            get { return _canWrite; }
        }

        public override long Length {
            get {
                long currentPos = Position;
                Seek(0, SeekOrigin.End);
                long result = Position;
                Position = currentPos;
                return result;
            }
        }

        public override long Position {
            get {
                if (!_canSeek) {
                    throw new NotSupportedException();
                }
                // TODO: conversion
                return (long)_tellSite.Target(_tellSite, _obj);
            }
            set {
                if (!_canSeek) {
                    throw new NotSupportedException();
                }
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush() {
            FlushWrite();
            FlushRead();
        }

        public void Flush(UnaryOpStorage/*!*/ flushStorage, RubyContext/*!*/ context) {
            Flush();

            if (_canFlush) {
                var site = flushStorage.GetCallSite("flush");
                site.Target(site, _obj);
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
            // TODO: conversion
            MutableString result = (MutableString)_readSite.Target(_readSite, _obj, count);
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

            _seekSite.Target(_seekSite, _obj, offset, rubyOrigin);
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
            _writeSite.Target(_writeSite, _obj, argument);
        }
    }
}
