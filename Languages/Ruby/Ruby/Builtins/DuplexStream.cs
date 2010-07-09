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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Text;
using System.Diagnostics;

namespace IronRuby.Builtins {
    // TODO: remove (RubyIO class should handle this functionality)
    internal sealed class DuplexStream : Stream {
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public DuplexStream(StreamReader reader, StreamWriter writer) {
            Debug.Assert(reader != null || writer != null);
            _reader = reader;
            _writer = writer;
        }

        public override void Close() {
            if (_reader != null) {
                _reader.Close();
            }
            if (_writer != null) {
                _writer.Close();
            }
        }

        public StreamReader Reader {
            get { return _reader; }
        }

        public StreamWriter Writer {
            get { return _writer; }
        }

        public override bool CanRead {
            get { return _reader != null; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return _writer != null; }
        }

        public override void Flush() {
            if (_reader != null) {
                _reader.BaseStream.Flush();
            }
            if (_writer != null) {
                _writer.Flush();
            }
        }

        public override long Length {
            get { throw new NotImplementedException(); }
        }

        public override long Position {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int ReadByte() {
            if (_reader == null) {
                throw new InvalidOperationException();
            }
            return _reader.Read();
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            if (_reader == null) {
                throw new InvalidOperationException();
            }

            // TODO:
            return _reader.BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (_writer == null) {
                throw new InvalidOperationException();
            }

            // TODO:
            Debug.Assert(_writer != null);
            _writer.Write(_writer.Encoding.GetString(buffer, offset, count));
        }
    }
}
