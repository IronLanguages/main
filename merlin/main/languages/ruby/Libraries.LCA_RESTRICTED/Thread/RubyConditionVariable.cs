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

using System.Threading;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading {
    [RubyClass("ConditionVariable")]
    public class RubyConditionVariable {
        private RubyMutex _mutex;

        public RubyConditionVariable() {
        }

        [RubyMethod("signal")]
        public static RubyConditionVariable/*!*/ Signal(RubyConditionVariable/*!*/ self) {
            RubyMutex m = self._mutex;
            if (m != null) {
                Monitor.Pulse(m);
            }
            return self;
        }

        [RubyMethod("broadcast")]
        public static RubyConditionVariable/*!*/ Broadcast(RubyConditionVariable/*!*/ self) {
            RubyMutex m = self._mutex;
            if (m != null) {
                Monitor.PulseAll(m);
            }
            return self;
        }

        [RubyMethod("wait")]
        public static RubyConditionVariable/*!*/ Wait(RubyConditionVariable/*!*/ self, [NotNull]RubyMutex/*!*/ mutex) {
            self._mutex = mutex;
            Monitor.Wait(mutex.Mutex);
            return self;
        }

        // TODO:
        // "marshal_load" 
        // "marshal_dump"
    }
}
