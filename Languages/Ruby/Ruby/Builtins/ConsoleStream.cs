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

namespace IronRuby.Builtins {
    /// <summary>
    /// A custom Stream class that forwards calls to Console In/Out/Error based on ConsoleType
    /// </summary>
    internal sealed class ConsoleStream : Stream {
        private ConsoleStreamType _consoleType;
        private readonly SharedIO _io;

        public ConsoleStream(SharedIO/*!*/ io, ConsoleStreamType consoleType) {
            Assert.NotNull(io);
            _consoleType = consoleType;
            _io = io;
        }

        public ConsoleStreamType StreamType {
            get { return _consoleType; }
        }

        public override bool CanRead {
            get { return _consoleType == ConsoleStreamType.Input ? true : false; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return _consoleType == ConsoleStreamType.Input ? false : true; }
        }

        public override void Flush() {
            switch (_consoleType) {
                case ConsoleStreamType.ErrorOutput:
                    _io.ErrorWriter.Flush();
                    break;

                case ConsoleStreamType.Output:
                    _io.OutputWriter.Flush();
                    break;

                case ConsoleStreamType.Input:
                    throw new NotSupportedException();
            }
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[]/*!*/ buffer, int offset, int count) {
            return _io.InputStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[]/*!*/ buffer, int offset, int count) {
            if (_consoleType == ConsoleStreamType.Output) {
                _io.OutputStream.Write(buffer, offset, count);
            } else {
                _io.ErrorStream.Write(buffer, offset, count);
            }
        }
    }
}
