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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Provides fast strongly typed thread local storage.  This is significantly faster than
    /// Thread.GetData/SetData.
    /// </summary>
    public class ThreadLocal<T> {
        private StorageInfo[] _stores;                                         // array of storage indexed by managed thread ID
        private static readonly StorageInfo[] Updating = new StorageInfo[0];   // a marker used when updating the array
        private readonly bool _refCounted;

        
        public ThreadLocal() {
        }

        /// <summary>
        /// True if the caller will guarantee that all cleanup happens as the thread
        /// unwinds.
        /// 
        /// This is typically used in a case where the thread local is surrounded by
        /// a try/finally block.  The try block pushes some state, the finally block
        /// restores the previous state.  Therefore when the thread exits the thread
        /// local is back to it's original state.  This allows the ThreadLocal object
        /// to not check the current owning thread on retrieval.
        /// </summary>
        public ThreadLocal(bool refCounted) {
            _refCounted = refCounted;
        }

        #region Public API

        /// <summary>
        /// Gets or sets the value for the current thread.
        /// </summary>
        public T Value {
            get {
                return GetStorageInfo().Value;
            }
            set {
                GetStorageInfo().Value = value;
            }
        }

        /// <summary>
        /// Gets the current value if its not == null or calls the provided function
        /// to create a new value.
        /// </summary>
        public T GetOrCreate(Func<T> func) {
            Assert.NotNull(func);

            StorageInfo si = GetStorageInfo();
            T res = si.Value;
            if (res == null) {
                si.Value = res = func();
            }

            return res;
        }

        /// <summary>
        /// Calls the provided update function with the current value and
        /// replaces the current value with the result of the function.
        /// </summary>
        public T Update(Func<T, T> updater) {
            Assert.NotNull(updater);

            StorageInfo si = GetStorageInfo();
            return si.Value = updater(si.Value);
        }

        /// <summary>
        /// Replaces the current value with a new one and returns the old value.
        /// </summary>
        public T Update(T newValue) {
            StorageInfo si = GetStorageInfo();
            var oldValue = si.Value;
            si.Value = newValue;
            return oldValue;
        }

        #endregion

        #region Storage implementation

#if SILVERLIGHT
        private static int _cfThreadIdDispenser = 1;

        [ThreadStatic]
        private static int _cfThreadId;

        private static int GetCurrentThreadId() {
            if (PlatformAdaptationLayer.IsCompactFramework) {
                // CF doesn't index threads by small integers, so we need to do the indexing ourselves:
                int id = _cfThreadId;
                if (id == 0) {
                    _cfThreadId = id = Interlocked.Increment(ref _cfThreadIdDispenser);
                }
                return id;
            } else {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }
#else
        private static int GetCurrentThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }
#endif

        /// <summary>
        /// Gets the StorageInfo for the current thread.
        /// </summary>
        public StorageInfo GetStorageInfo() {
            return GetStorageInfo(_stores);
        }

        private StorageInfo GetStorageInfo(StorageInfo[] curStorage) {
            int threadId = GetCurrentThreadId();

            // fast path if we already have a value in the array
            if (curStorage != null && curStorage.Length > threadId) {
                StorageInfo res = curStorage[threadId];

                if (res != null && (_refCounted || res.Thread == Thread.CurrentThread)) {
                    return res;
                }
            }

            return RetryOrCreateStorageInfo(curStorage);
        }

        /// <summary>
        /// Called when the fast path storage lookup fails. if we encountered the Empty storage 
        /// during the initial fast check then spin until we hit non-empty storage and try the fast 
        /// path again.
        /// </summary>
        private StorageInfo RetryOrCreateStorageInfo(StorageInfo[] curStorage) {
            if (curStorage == Updating) {
                // we need to retry
                while ((curStorage = _stores) == Updating) {
                    Thread.Sleep(0);
                }

                // we now have a non-empty storage info to retry with
                return GetStorageInfo(curStorage);
            }

            // we need to mutate the StorageInfo[] array or create a new StorageInfo
            return CreateStorageInfo();
        }

        /// <summary>
        /// Creates the StorageInfo for the thread when one isn't already present.
        /// </summary>
        private StorageInfo CreateStorageInfo() {
            // we do our own locking, tell hosts this is a bad time to interrupt us.
#if !SILVERLIGHT
            Thread.BeginCriticalRegion();
#endif
            StorageInfo[] curStorage = Updating;
            try {
                int threadId = GetCurrentThreadId();
                StorageInfo newInfo = new StorageInfo(Thread.CurrentThread);

                // set to updating while potentially resizing/mutating, then we'll
                // set back to the current value.                                        
                while ((curStorage = Interlocked.Exchange(ref _stores, Updating)) == Updating) {
                    // another thread is already updating...
                    Thread.Sleep(0);
                }

                // check and make sure we have a space in the array for our value
                if (curStorage == null) {
                    curStorage = new StorageInfo[threadId + 1];
                } else if (curStorage.Length <= threadId) {
                    StorageInfo[] newStorage = new StorageInfo[threadId + 1];
                    for (int i = 0; i < curStorage.Length; i++) {
                        // leave out the threads that have exited
                        if (curStorage[i] != null && curStorage[i].Thread.IsAlive) {
                            newStorage[i] = curStorage[i];
                        }
                    }
                    curStorage = newStorage;
                }

                // create our StorageInfo in the array, the empty check ensures we're only here
                // when we need to create.
                Debug.Assert(curStorage[threadId] == null || curStorage[threadId].Thread != Thread.CurrentThread);

                return curStorage[threadId] = newInfo;
            } finally {
                if (curStorage != Updating) {
                    // let others access the storage again
                    Interlocked.Exchange(ref _stores, curStorage);
                }
#if !SILVERLIGHT
                Thread.EndCriticalRegion();
#endif
            }
        }

        /// <summary>
        /// Helper class for storing the value.  We need to track if a ManagedThreadId
        /// has been re-used so we also store the thread which owns the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO
        public sealed class StorageInfo {
            internal readonly Thread Thread;                 // the thread that owns the StorageInfo
            public T Value;                                // the current value for the owning thread

            internal StorageInfo(Thread curThread) {
                Assert.NotNull(curThread);

                Thread = curThread;
            }
        }

        #endregion
    }
}
