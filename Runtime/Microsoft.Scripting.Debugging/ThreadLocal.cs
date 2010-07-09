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

namespace Microsoft.Scripting.Debugging {
    internal class ThreadLocal<T> {
        private StorageInfo[] _stores;                                         // array of storage indexed by managed thread ID
        private static readonly StorageInfo[] Updating = new StorageInfo[0];   // a marker used when updating the array

        internal T Value {
            get {
                return GetStorageInfo().Value;
            }
            set {
                GetStorageInfo().Value = value;
            }
        }

        internal T[] AllValues {
            get {
                List<T> allValues = new List<T>(_stores.Length);
                foreach (StorageInfo si in _stores) {
                    if (si != null && si.Thread.IsAlive)
                        allValues.Add(si.Value);
                }

                return allValues.ToArray();
            }
        }

        #region Storage implementation

        private StorageInfo GetStorageInfo() {
            return GetStorageInfo(_stores);
        }

        private StorageInfo GetStorageInfo(StorageInfo[] curStorage) {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            // fast path if we already have a value in the array
            if (curStorage != null && curStorage.Length > threadId) {
                StorageInfo res = curStorage[threadId];

                if (res != null && res.Thread == Thread.CurrentThread) {
                    return res;
                }
            }

            return RetryOrCreateStorageInfo(curStorage);
        }

        private StorageInfo RetryOrCreateStorageInfo(StorageInfo[] curStorage) {
            if (curStorage == Updating) {
                // we need to retry
                while ((curStorage = _stores) == Updating) {
                    Thread.Sleep(0);
                }

                // we now have a non-empty storage info to retry with
                return GetStorageInfo(curStorage);
            }

            // we need to mutator the StorageInfo[] array or create a new StorageInfo
            return CreateStorageInfo();
        }

        private StorageInfo CreateStorageInfo() {
            // we do our own locking, tell hosts this is a bad time to interrupt us.
#if !SILVERLIGHT
            Thread.BeginCriticalRegion();
#endif
            StorageInfo[] curStorage = Updating;
            try {
                int threadId = Thread.CurrentThread.ManagedThreadId;
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

        private class StorageInfo {
            public readonly Thread Thread;                 // the thread that owns the StorageInfo
            public T Value;                                // the current value for the owning thread

            public StorageInfo(Thread curThread) {
                Debug.Assert(curThread != null);
                Thread = curThread;
            }
        }

        #endregion
    }
}
