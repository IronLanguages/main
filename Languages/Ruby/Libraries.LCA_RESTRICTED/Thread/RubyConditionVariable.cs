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

using System.Threading;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading {
    [RubyClass("ConditionVariable")]
    public class RubyConditionVariable {
        private RubyMutex _mutex;
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly object _lock = new object();
        private int _waits;

        public RubyConditionVariable() {
        }

        [RubyMethod("signal")]
        public static RubyConditionVariable/*!*/ Signal(RubyConditionVariable/*!*/ self) {
            RubyMutex m = self._mutex;
            if (m != null) {
                self._signal.Set();
            }
            return self;
        }

        [RubyMethod("broadcast")]
        public static RubyConditionVariable/*!*/ Broadcast(RubyConditionVariable/*!*/ self) {
            RubyMutex m = self._mutex;
            if (m != null) {
                lock (self._lock) {
                    int waits = self._waits;
                    for (int i = 0; i < waits; i++) {
                        self._signal.Set();
                        //
                        // WARNING
                        //
                        // There is no guarantee that every call to the Set method will release a waiting thread.
                        // If two calls are too close together, so that the second call occurs before a thread 
                        // has been released, only one thread is released. 
                        // We add a sleep to increase the chance that all waiting threads will be released.
                        //
                        Thread.CurrentThread.Join(1);
                    }
                }
            }
            return self;
        }

        [RubyMethod("wait")]
        public static RubyConditionVariable/*!*/ Wait(RubyConditionVariable/*!*/ self, [NotNull]RubyMutex/*!*/ mutex) {
            self._mutex = mutex;
            RubyMutex.Unlock(mutex);
            lock (self._lock) { self._waits++; }

            self._signal.WaitOne();

            lock (self._lock) { self._waits--; }
            RubyMutex.Lock(mutex);
            return self;
        }

        // TODO:
        // "marshal_load" 
        // "marshal_dump"
    }
}
