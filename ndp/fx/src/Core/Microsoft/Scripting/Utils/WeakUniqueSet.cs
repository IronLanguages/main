/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Dynamic.Utils {
    internal sealed class WeakUniqueSet<T> where T : class {
        private readonly Dictionary<int, WeakLinkedList> hashTable = new Dictionary<int, WeakLinkedList>();

        internal T GetUniqueFor(T obj) {
            CheckCleanup();

            int hash = obj.GetHashCode();
            WeakLinkedList list;

            // LOCK table to get/set the list
            lock (hashTable) {
                if (!hashTable.TryGetValue(hash, out list)) {
                    hashTable[hash] = list = new WeakLinkedList();
                    list.Add(obj);
                    return obj;
                }
            }

            var objType = obj.GetType();

            // LOCK list to iterate
            lock (list) {
                foreach (var current in list) {
                    Debug.Assert(current != null);
                    if (current.GetType() == objType && current.Equals(obj)) {
                        return current as T;
                    }
                }
                list.Add(obj);
            }
            return obj;
        }



        #region TableCleanup

        int sinceLastChange;

#if SILVERLIGHT // GC
        WeakReference cleanupGC = new WeakReference(new object());
#else
        int cleanupGC = 0;
#endif

        /// <summary>
        /// Check if any of the keys have gotten collected
        /// </summary>
        private void CheckCleanup() {
            sinceLastChange++;

            // Cleanup the table if it is a while since we have done it last time.
            // Take the size of the table into account.
            if (sinceLastChange > 1234 + hashTable.Count / 2) {
                if (Interlocked.Exchange(ref sinceLastChange, 0) < 1234) {
                    return; // someone is already cleaning
                };

                bool HasCollectionSinceLastCleanup;
#if SILVERLIGHT // GC.CollectionCount
                HasCollectionSinceLastCleanup = !cleanupGC.IsAlive;
                if (HasCollectionSinceLastCleanup) cleanupGC = new WeakReference(new object());
#else
                int currentGC = GC.CollectionCount(2);
                HasCollectionSinceLastCleanup = currentGC != cleanupGC;
                if (HasCollectionSinceLastCleanup) cleanupGC = currentGC;
#endif
                if (HasCollectionSinceLastCleanup) {
                    CleanTable();
                }
            }
        }

        // remove empty lists.
        internal void CleanTable() {
            var keys = hashTable.Keys.ToReadOnly();
            // LOCK table to get lists.
            lock (hashTable) {
                foreach (int hash in keys) {
                    WeakLinkedList list = hashTable[hash];
                    int cnt;

                    // LOCK list to iterate.
                    lock (list) {
                        cnt = list.Count();
                    }

                    if (cnt == 0) {
                        hashTable.Remove(hash);
                    }
                }
            }
        }

        #endregion
    }


    // self-compacting list of weak values
    internal class WeakLinkedList : IEnumerable {
        private class Node {
            internal Node next;
            internal readonly WeakReference value;

            internal Node(object item) {
                value = new WeakReference(item);
            }
        }

        private Node _head;

        internal void Add(object item) {
            Node newNode = new Node(item);
            newNode.next = _head;
            _head = newNode;
        }

        // O(n) operation. will drop dead nodes and report number of live ones.
        internal int Count() {
            int i = 0;
            foreach (var cur in this) {
                i++;
            }
            return i;
        }

        // enumerator for the list. Skips and drops dead nodes.
        IEnumerator IEnumerable.GetEnumerator() {
            Node cur1 = _head;
            object value = null;

            while (cur1 != null && (value = cur1.value.Target) == null) {
                cur1 = cur1.next;
            }
            // Invariant: cur1 ponts to null or the first live node and value has its Target.

            if (cur1 == null) {
                _head = cur1;
                yield break;
            }

            yield return value;

            Node cur2;
            do {
                // Invariant: cur1 ponts to last known live node, 
                cur2 = cur1.next;
                while (cur2 != null && (value = cur2.value.Target) == null) {
                    cur2 = cur2.next;
                }
                // Invariant: cur2 ponts to null or the next live node and value has its Target.
                if (cur2 == null) {
                    cur1.next = cur2;
                    yield break;
                }

                yield return value;
                cur1 = cur2;
            } while (cur1 != null);
        }
    }
}
