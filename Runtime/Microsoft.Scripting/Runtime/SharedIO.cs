/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    public sealed class SharedIO {
        // prevents this object from transitions to an inconsistent state, doesn't sync output or input:
        private readonly object _mutex = new object();

        #region Proxies

        private sealed class StreamProxy : Stream {
            private readonly ConsoleStreamType _type;
            private readonly SharedIO _io;

            public StreamProxy(SharedIO io, ConsoleStreamType type) {
                Assert.NotNull(io);
                _io = io;
                _type = type;
            }

            public override bool CanRead {
                get { return _type == ConsoleStreamType.Input; }
            }

            public override bool CanSeek {
                get { return false; }
            }

            public override bool CanWrite {
                get { return !CanRead; }
            }

            public override void Flush() {
                _io.GetStream(_type).Flush();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                return _io.GetStream(_type).Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count) {
                _io.GetStream(_type).Write(buffer, offset, count);
            }

            public override long Length {
                get {
                    throw new NotSupportedException();
                }
            }

            public override long Position {
                get {
                    throw new NotSupportedException();
                }
                set {
                    throw new NotSupportedException();
                }
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotSupportedException();
            }

            public override void SetLength(long value) {
                throw new NotSupportedException();
            }
        }

        #endregion

        // lazily initialized to Console by default:
        private Stream _inputStream;
        private Stream _outputStream;
        private Stream _errorStream;

        private TextReader _inputReader;
        private TextWriter _outputWriter;
        private TextWriter _errorWriter;

        private Encoding _inputEncoding;

        public Stream InputStream { get { InitializeInput(); return _inputStream; } }
        public Stream OutputStream { get { InitializeOutput(); return _outputStream; } }
        public Stream ErrorStream { get { InitializeErrorOutput(); return _errorStream; } }

        public TextReader InputReader { get { InitializeInput(); return _inputReader; } }
        public TextWriter OutputWriter { get { InitializeOutput(); return _outputWriter; } }
        public TextWriter ErrorWriter { get { InitializeErrorOutput(); return _errorWriter; } }

        public Encoding InputEncoding { get { InitializeInput(); return _inputEncoding; } }
        public Encoding OutputEncoding { get { InitializeOutput(); return _outputWriter.Encoding; } }
        public Encoding ErrorEncoding { get { InitializeErrorOutput(); return _errorWriter.Encoding; } }

        internal SharedIO() {
        }

        private void InitializeInput() {
            if (_inputStream == null) {
                lock (_mutex) {
                    if (_inputStream == null) {
#if SILVERLIGHT
                        _inputEncoding = StringUtils.DefaultEncoding;
                        _inputStream = new TextStream(Console.In, _inputEncoding);
#else
                        _inputStream = ConsoleInputStream.Instance;
                        _inputEncoding = Console.InputEncoding;
#endif
                        _inputReader = Console.In;
                    }
                }
            }
        }

        private void InitializeOutput() {
            if (_outputStream == null) {
                lock (_mutex) {
                    if (_outputStream == null) {
#if SILVERLIGHT
                        _outputStream = new TextStream(Console.Out);
#else
                        _outputStream = Console.OpenStandardOutput();
#endif
                        _outputWriter = Console.Out;
                    }
                }
            }
        }

        private void InitializeErrorOutput() {
            if (_errorStream == null) {
#if SILVERLIGHT
                Stream errorStream = new TextStream(Console.Error);
#else
                Stream errorStream = Console.OpenStandardError();
#endif
                Interlocked.CompareExchange(ref _errorStream, errorStream, null);
                Interlocked.CompareExchange(ref _errorWriter, Console.Error, null);
            }
        }

        /// <summary>
        /// Only host should redirect I/O.
        /// </summary>
        public void SetOutput(Stream stream, TextWriter writer) {
            Assert.NotNull(stream, writer);
            lock (_mutex) {
                _outputStream = stream;
                _outputWriter = writer;
            }
        }

        public void SetErrorOutput(Stream stream, TextWriter writer) {
            Assert.NotNull(stream, writer);
            lock (_mutex) {
                _errorStream = stream;
                _errorWriter = writer;
            }
        }

        public void SetInput(Stream stream, TextReader reader, Encoding encoding) {
            Assert.NotNull(stream, reader, encoding);
            lock (_mutex) {
                _inputStream = stream;
                _inputReader = reader;
                _inputEncoding = encoding;
            }
        }

        public void RedirectToConsole() {
            lock (_mutex) {
                _inputEncoding = null;
                _inputStream = null;
                _outputStream = null;
                _errorStream = null;
                _inputReader = null;
                _outputWriter = null;
                _errorWriter = null;
            }
        }

        public Stream GetStream(ConsoleStreamType type) {
            switch (type) {
                case ConsoleStreamType.Input: return InputStream;
                case ConsoleStreamType.Output: return OutputStream;
                case ConsoleStreamType.ErrorOutput: return ErrorStream;
            }
            throw Error.InvalidStreamType(type);
        }

        public TextWriter GetWriter(ConsoleStreamType type) {
            switch (type) {
                case ConsoleStreamType.Output: return OutputWriter;
                case ConsoleStreamType.ErrorOutput: return ErrorWriter;
            }
            throw Error.InvalidStreamType(type);
        }

        public Encoding GetEncoding(ConsoleStreamType type) {
            switch (type) {
                case ConsoleStreamType.Input: return InputEncoding;
                case ConsoleStreamType.Output: return OutputEncoding;
                case ConsoleStreamType.ErrorOutput: return ErrorEncoding;
            }
            throw Error.InvalidStreamType(type);
        }

        public TextReader GetReader(out Encoding encoding) {
            TextReader reader;
            lock (_mutex) {
                reader = InputReader;
                encoding = InputEncoding;
            }
            return reader;
        }

        public Stream GetStreamProxy(ConsoleStreamType type) {
            return new StreamProxy(this, type);
        }
    }
}
