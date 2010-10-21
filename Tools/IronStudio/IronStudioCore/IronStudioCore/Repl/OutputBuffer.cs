/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace Microsoft.IronStudio.Library.Repl {
    public class OutputBuffer : IDisposable {
        private readonly Timer _timer;
        private int _maxSize;
        private readonly object _lock;
        private readonly StringBuilder _buffer;
        private long _lastFlush;
        private static readonly Stopwatch _stopwatch;
        public event Action<string> OutputText;
        private const int _initialMaxSize = 1024;

        public OutputBuffer()
            : this(400) {
        }

        static OutputBuffer() {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public OutputBuffer(int timeout) {
            _maxSize = _initialMaxSize;
            _lock = new object();
            _buffer = new StringBuilder(_maxSize);
            _timer = new Timer();
            _timer.Elapsed += (sender, args) => Flush();
            _timer.Interval = timeout;
        }

        public void Write(string text) {
            bool needsFlush = false;
            lock (_lock) {
                _buffer.Append(text);
                needsFlush = (_buffer.Length > _maxSize);
                if (!needsFlush && !_timer.Enabled) {
                    _timer.Enabled = true;
                }
            }
            if (needsFlush) {
                Flush();
            }
        }

        public void Flush() {
            // if we're rapidly outputting grow the output buffer.
            long curTime = _stopwatch.ElapsedMilliseconds;
            if ((curTime - _lastFlush) < 1000) {
                if (_maxSize < (1024 * 1024)) {
                    _maxSize *= 2;
                }
            }
            _lastFlush = _stopwatch.ElapsedMilliseconds;

            string result;
            lock (_lock) {
                result = _buffer.ToString();
                _buffer.Length = 0;
                if (_buffer.Capacity > _maxSize) {
                    _buffer.Capacity = _maxSize;
                }
                _timer.Enabled = false;
            }
            if (result.Length > 0) {
                Action<string> evt = OutputText;
                if (evt != null) {
                    evt(result);
                }
            }
        }

        public void Dispose() {
            _timer.Enabled = false;
        }
    }
}
