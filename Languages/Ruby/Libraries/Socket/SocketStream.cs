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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Sockets {
    internal class SocketStream : Stream {
        internal readonly Socket/*!*/ _socket;

        public SocketStream(Socket/*!*/ s) {
            Assert.NotNull(s);
            _socket = s;
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Close() {
            base.Close();
            _socket.Close();
        }

        public override void Flush() {
        }

        public override long Length {
            get {
                throw new InvalidOperationException();
            }
        }

        public override long Position {
            get {
                throw new InvalidOperationException();
            }
            set {
                throw new InvalidOperationException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _socket.Receive(buffer, offset, count, SocketFlags.None);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value) {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _socket.Send(buffer, offset, count, SocketFlags.None);
        }
    }
}
#endif
