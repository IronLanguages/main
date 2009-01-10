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

using System;
using Microsoft.Scripting.Runtime;
using System.Threading;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.StandardLibrary.Threading {
    // TODO:
    // Ruby mutex is not recursive.
    // It can be unlocked from non-owning thread.
    [RubyClass("Mutex")]
    public class RubyMutex {
        private readonly object/*!*/ _mutex;

        // TODO: this is not precise.
        private bool _isLocked;

        internal object Mutex { get { return _mutex; } }

        public RubyMutex() {
            _mutex = new object();
        }

        [RubyMethod("locked?")]
        public static bool IsLocked(RubyMutex/*!*/ self) {
            return self._isLocked;
        }

        [RubyMethod("try_lock")]
        public static bool TryLock(RubyMutex/*!*/ self) {
            return self._isLocked = Monitor.TryEnter(self._mutex);
        }

        [RubyMethod("lock")]
        public static RubyMutex/*!*/ Lock(RubyMutex/*!*/ self) {
            Monitor.Enter(self._mutex);
            self._isLocked = true;
            return self;
        }

        [RubyMethod("unlock")]
        public static RubyMutex/*!*/ Unlock(RubyMutex/*!*/ self) {
            self._isLocked = false;
            Monitor.Exit(self._mutex);
            return self;
        }

        [RubyMethod("synchronize")]
        public static object Synchronize(BlockParam criticalSection, RubyMutex/*!*/ self) {
            lock (self._mutex) {
                self._isLocked = true;
                try {
                    object result;
                    criticalSection.Yield(out result);
                    return result;
                } finally {
                    self._isLocked = false;
                }
            }
        }

        [RubyMethod("exclusive_unlock")]
        public static bool ExclusiveUnlock(BlockParam criticalSection, RubyMutex/*!*/ self) {
            // TODO:
            throw new NotImplementedException();            
        }
        
        // TODO:
        // "marshal_load"
        // "marshal_dump"
    }
}
