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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace IronRuby.Builtins {
    /// <summary>
    /// Pipe for intra-process producer-consumer style message passing
    /// </summary>
    internal class RubyPipe : Stream {
        private readonly EventWaitHandle _dataAvailableEvent;
        private readonly EventWaitHandle _writerClosedEvent;
        private readonly WaitHandle[] _eventArray;
        private readonly Queue<byte> _queue;

        private const int WriterClosedEventIndex = 1;

        private RubyPipe() {
            _dataAvailableEvent = new AutoResetEvent(false);
            _writerClosedEvent = new ManualResetEvent(false);
            _eventArray = new WaitHandle[2];
            _queue = new Queue<byte>();

            _eventArray[0] = _dataAvailableEvent;
            _eventArray[1] = _writerClosedEvent;
            Debug.Assert(_eventArray[WriterClosedEventIndex] == _writerClosedEvent);
        }

        private RubyPipe(RubyPipe pipe) {
            _dataAvailableEvent = pipe._dataAvailableEvent;
            _writerClosedEvent = pipe._writerClosedEvent;
            _eventArray = pipe._eventArray;
            _queue = pipe._queue;
        }

        internal void CloseWriter() {
            _writerClosedEvent.Set();
        }

        public static void CreatePipe(out Stream reader, out Stream writer) {
            RubyPipe pipe = new RubyPipe();
            reader = pipe;
            writer = new PipeWriter(pipe);
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override long Length {
            get { throw new NotImplementedException(); }
        }

        public override long Position {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            // Wait until data is available, or if the writer has closed the pipe
            //
            // In the latter case, we do need to return any pending data, and so fall through.
            // Pending data will be returned the first time, and 0 will naturually be returned subsequent times 
            WaitHandle.WaitAny(_eventArray);

            lock (((ICollection)_queue).SyncRoot) {
                if (_queue.Count <= count) {
                    _queue.CopyTo(buffer, 0);
                    _queue.Clear();
                    return _queue.Count;
                } else {
                    for (int idx = 0; idx < count; idx++) {
                        buffer[idx] = _queue.Dequeue();
                    }
                    return count;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            lock (((ICollection)_queue).SyncRoot) {
                for (int idx = 0; idx < count; idx++) {
                    _queue.Enqueue(buffer[offset + idx]);
                }
                _dataAvailableEvent.Set();
            }
        }

        /// <summary>
        /// PipeWriter instance always exists as a sibling of a RubyPipe. Two objects are needed
        /// so that we can detect whether Close is being called on the reader end of a pipe,
        /// or on the writer end of a pipe.
        /// </summary>
        internal class PipeWriter : RubyPipe {
            internal PipeWriter(RubyPipe pipe)
                : base(pipe) {
            }

            public override void Close() {
                base.Close();
                CloseWriter();
            }
        }
    }
}
