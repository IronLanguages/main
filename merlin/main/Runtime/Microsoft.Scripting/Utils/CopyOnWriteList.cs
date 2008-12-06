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

using System.Collections.Generic;
using System.Threading;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// List optimized for few writes and multiple reads. It provides thread-safe read and write access. 
    /// Iteration is not thread-safe by default, but GetCopyForRead allows for iteration 
    /// without taking a lock.
    /// </summary>
    public class CopyOnWriteList<T> : IList<T> {
        List<T> _list = new List<T>();

        List<T> GetNewListForWrite() {
            List<T> oldList = _list;
            List<T> newList = new List<T>(oldList.Count + 1);
            newList.AddRange(oldList);
            return newList;
        }

        /// <summary>
        /// Gets a copy of the contents of the list. The copy will not change even if the original
        /// CopyOnWriteList object is modified. This method should be used to iterate the list in
        /// a thread-safe way if no lock is taken. Iterating on the original list is not guaranteed 
        /// to be thread-safe.
        /// </summary>
        /// <returns>The returned copy should not be modified by the caller.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        public List<T> GetCopyForRead() {
            // Just return the underlying list
            return _list;
        }

        #region IList<T> Members

        public int IndexOf(T item) {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            List<T> oldList, replacedList;
            do {
                oldList = _list;
                List<T> newList = GetNewListForWrite();
                newList.Insert(index, item);
                replacedList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (replacedList != oldList);
        }

        public void RemoveAt(int index) {
            List<T> oldList, replacedList;
            do {
                oldList = _list;
                List<T> newList = GetNewListForWrite();
                newList.RemoveAt(index);
                replacedList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (replacedList != oldList);
        }

        public T this[int index] {
            get {
                return _list[index];
            }

            set {
                List<T> oldList, replacedList;
                do {
                    oldList = _list;
                    List<T> newList = GetNewListForWrite();
                    newList[index] = value;
                    replacedList = Interlocked.CompareExchange(ref _list, newList, oldList);
                } while (replacedList != oldList);
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item) {
            List<T> oldList, replacedList;
            do {
                oldList = _list;
                List<T> newList = GetNewListForWrite();
                newList.Add(item);
                replacedList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (replacedList != oldList);
        }

        public void Clear() {
            _list = new List<T>();
        }

        [Confined]
        public bool Contains(T item) {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _list.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            List<T> oldList, replacedList;
            bool ret;
            do {
                oldList = _list;
                List<T> newList = GetNewListForWrite();
                ret = newList.Remove(item);
                replacedList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (replacedList != oldList);

            return ret;
        }

        #endregion

        #region IEnumerable<T> Members

        [Pure]
        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        [Pure]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((System.Collections.IEnumerable)_list).GetEnumerator();
        }

        #endregion
    }
}
