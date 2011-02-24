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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Runtime;
using IronRuby.Builtins;

namespace IronRuby.StandardLibrary.Threading {
    // Synchronized queue.
    [RubyClass("Queue")]
    public class RubyQueue {
        protected readonly Queue<object>/*!*/ _queue;
        protected int _waiting;

        public RubyQueue() {
            _queue = new Queue<object>();
        }

        protected RubyQueue(int capacity) {
            _queue = new Queue<object>(capacity);
        }

        private void Enqueue(object value) {
            lock (_queue) {
                _queue.Enqueue(value);
                Monitor.PulseAll(_queue);
            }
        }

        protected object Dequeue() {
            object value;
            lock (_queue) {
                _waiting++;

                try {
                    while (_queue.Count == 0) {
                        Monitor.Wait(_queue);
                    }
                } finally {
                    _waiting--;
                }
                value = _queue.Dequeue();
                Monitor.PulseAll(_queue);
            }
            return value;
        }
        
        [RubyMethod("enq")]
        [RubyMethod("push")]
        [RubyMethod("<<")]
        public static RubyQueue/*!*/ Enqueue(RubyQueue/*!*/ self, object value) {
            self.Enqueue(value);
            return self;
        }

        [RubyMethod("deq")]
        [RubyMethod("pop")]
        [RubyMethod("shift")]
        public static object Dequeue(RubyQueue/*!*/ self, [Optional]bool nonBlocking) {
            if (nonBlocking) {
                lock (self._queue) {
                    if (self._queue.Count == 0) {
                        throw new ThreadError("queue empty");
                    }
                    return self._queue.Dequeue();
                }
            }
            return self.Dequeue();
        }

        [RubyMethod("size")]
        [RubyMethod("length")]
        public static int GetCount(RubyQueue/*!*/ self) {
            lock (self._queue) {
                return self._queue.Count;
            }
        }

        [RubyMethod("clear")]
        public static RubyQueue/*!*/ Clear(RubyQueue/*!*/ self) {
            lock (self._queue) {
                self._queue.Clear();
            }
            return self;
        }

        [RubyMethod("empty?")]
        public static bool IsEmpty(RubyQueue/*!*/ self) {
            return GetCount(self) == 0;
        }

        [RubyMethod("num_waiting")]
        public static int GetNumberOfWaitingThreads(RubyQueue/*!*/ self) {
            return self._waiting;
        }
        
        // TODO:
        // "marshal_load" 
        // "marshal_dump" 
    }
}
