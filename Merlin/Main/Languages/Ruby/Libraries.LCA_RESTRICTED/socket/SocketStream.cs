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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace IronRuby.StandardLibrary.Sockets {
    internal class SocketStream : System.IO.Stream {
        public SocketStream(Socket/*!*/ s) {
            _socket = s;
        }

        private long _pos = 0;
        private byte _lastByteRead;
        private bool _peeked = false;
        private List<byte> _internalWriteBuffer = new List<byte>();
        internal readonly Socket/*!*/ _socket;

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
            if (_internalWriteBuffer.Count == 0) {
                return;
            }

            byte[] bufferedData = _internalWriteBuffer.ToArray();
            int bytesSent = _socket.Send(bufferedData);
            if (bytesSent < bufferedData.Length) {
                // TODO: Resend the rest
                System.Diagnostics.Debug.Assert(false, "Partial data sent");
            }
            _internalWriteBuffer.Clear();
        }

        public override long Length {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override long Position {
            get {
                return _pos;
            }
            set {
                long diff = _pos - value;
                if (diff == 1) {
                    _peeked = true;
                }

                _pos = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesToRead = _peeked ? count - 1 : count;
            byte[] readBuffer = new byte[bytesToRead];
            long oldPos = _pos;

            if (bytesToRead > 0) {
                int bytesRead = _socket.Receive(readBuffer, bytesToRead, SocketFlags.None);
                _pos += bytesRead;
            }

            if (_peeked) {
                // Put the byte we've already peeked at the beginning of the buffer
                buffer[offset] = _lastByteRead;
                // Put the rest of the data afterwards
                Array.Copy(readBuffer, 0, buffer, offset + 1, count - 1);
                _pos += 1;
                _peeked = false;
            } else {
                Array.Copy(readBuffer, 0, buffer, offset, count);
            }

            int totalBytesRead = (int)(_pos - oldPos);
            if (totalBytesRead > 0) {
                _lastByteRead = buffer[totalBytesRead - 1];
            }

            return totalBytesRead;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin) {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override void SetLength(long value) {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override void Write(byte[] buffer, int offset, int count) {
            for (int i = offset; i < offset + count; i++) {
                _internalWriteBuffer.Add(buffer[i]);
                if (buffer[i] == '\n') {
                    Flush();
                }
            }
            Flush();
        }
    }
}
#endif
