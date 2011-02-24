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
using System.Threading;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
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
            bool lockTaken = false;
            try {
                MonitorUtils.TryEnter(self._mutex, ref lockTaken);
            } finally {
                if (lockTaken) {
                    self._isLocked = true;
                }
            }
            return lockTaken;
        }

        [RubyMethod("lock")]
        public static RubyMutex/*!*/ Lock(RubyMutex/*!*/ self) {
            bool lockTaken = false;
            try {
                MonitorUtils.Enter(self._mutex, ref lockTaken);
            } finally {
                if (lockTaken) {
                    self._isLocked = true;
                }
            }
            return self;
        }

        [RubyMethod("unlock")]
        public static RubyMutex/*!*/ Unlock(RubyMutex/*!*/ self) {
            bool lockTaken = true;
            try {
                MonitorUtils.Exit(self._mutex, ref lockTaken);
            } finally {
                if (!lockTaken) {
                    self._isLocked = false;
                }
            }
            return self;
        }

        [RubyMethod("synchronize")]
        public static object Synchronize(BlockParam criticalSection, RubyMutex/*!*/ self) {
            bool lockTaken = false;
            try {
                MonitorUtils.Enter(self._mutex, ref lockTaken);
                self._isLocked = lockTaken;
                object result;
                criticalSection.Yield(out result);
                return result;
            } finally {
                if (lockTaken) {
                    MonitorUtils.Exit(self._mutex, ref lockTaken);
                    self._isLocked = lockTaken;
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
