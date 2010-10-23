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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using System.Threading;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading {
    // Synchronized queue.
    [RubyClass]
    public class SizedQueue : RubyQueue {
        private int _limit;

        public SizedQueue([DefaultProtocol]int limit) {
            _limit = limit;
        }

        public SizedQueue() {
        }

        private void Enqueue(object value) {
            lock (_queue) {
                _waiting++;
                try {
                    while (_queue.Count == _limit) {
                        Monitor.Wait(_queue);
                    }
                } finally {
                    _waiting--;
                }
                _queue.Enqueue(value);
                Debug.Assert(_queue.Count <= _limit);

                Monitor.PulseAll(_queue);
            }
        }
              

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static SizedQueue/*!*/ Reinitialize(SizedQueue/*!*/ self, [DefaultProtocol]int limit) {
            SetLimit(self, limit);
            return self;
        }

        [RubyMethod("max")]
        public static int GetLimit(SizedQueue/*!*/ self) {
            return self._limit;
        }

        [RubyMethod("max=")]
        public static void SetLimit(SizedQueue/*!*/ self, [DefaultProtocol]int limit) {
            self._limit = limit;
        }
        
        [RubyMethod("enq")]
        [RubyMethod("push")]
        [RubyMethod("<<")]
        public static SizedQueue/*!*/ Enqueue(SizedQueue/*!*/ self, object value) {
            self.Enqueue(value);
            return self;
        }

        [RubyMethod("deq")]
        [RubyMethod("pop")]
        [RubyMethod("shift")]
        public static object Dequeue(SizedQueue/*!*/ self, params object[]/*!*/ values) {
            // TODO: 
            if (values.Length != 0) {
                throw new NotImplementedException();
            }
            return self.Dequeue();
        }
    }
}
