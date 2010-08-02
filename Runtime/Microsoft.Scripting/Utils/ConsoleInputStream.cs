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
#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Console input stream (Console.OpenStandardInput) has a bug that manifests itself if reading small amounts of data.
    /// This class wraps the standard input stream with a buffer that ensures that enough data are read from the underlying stream.
    /// </summary>
    public sealed class ConsoleInputStream : Stream {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ConsoleInputStream Instance = new ConsoleInputStream();
        
        // we use 0x1000 to be safe (MSVCRT uses this value for stdin stream buffer).
        private const int MinimalBufferSize = 0x1000; 

        private readonly Stream _input;
        private readonly object _lock = new object();
        private readonly byte[] _buffer = new byte[MinimalBufferSize];
        private int _bufferPos;
        private int _bufferSize;
        
        private ConsoleInputStream() {
            _input = Console.OpenStandardInput();
        }

        public override bool CanRead {
            get { return true; }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int result;
            lock (_lock) {
                if (_bufferSize > 0) {
                    result = Math.Min(count, _bufferSize);
                    Buffer.BlockCopy(_buffer, _bufferPos, buffer, offset, result);
                    _bufferPos += result;
                    _bufferSize -= result;
                    offset += result;
                    count -= result;
                } else {
                    result = 0;
                }

                if (count > 0) {
                    Debug.Assert(_bufferSize == 0);
                    if (count < MinimalBufferSize) {
                        int bytesRead = _input.Read(_buffer, 0, MinimalBufferSize);
                        int bytesToReturn = Math.Min(bytesRead, count);
                        Buffer.BlockCopy(_buffer, 0, buffer, offset, bytesToReturn);

                        _bufferSize = bytesRead - bytesToReturn;
                        _bufferPos = bytesToReturn;
                        result += bytesToReturn;
                    } else {
                        result += _input.Read(buffer, offset, count);
                    }
                }
            }

            return result;
        }

        #region Stubs

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override void Flush() {
            throw new NotSupportedException();
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
#endif