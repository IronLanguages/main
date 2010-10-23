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
#if CLR2 && SILVERLIGHT
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Scripting.Utils {

    /// <summary>
    /// A simple hashset, built on Dictionary{K, V}
    /// </summary>
    public sealed class HashSet<T> : ICollection<T> {
        private readonly Dictionary<T, object> _data;

        public HashSet() {
            _data = new Dictionary<T, object>();
        }

        public HashSet(IEqualityComparer<T> comparer) {
            _data = new Dictionary<T, object>(comparer);
        }

        public HashSet(IList<T> list) {
            _data = new Dictionary<T, object>(list.Count);
            foreach (T t in list) {
                _data.Add(t, null);
            }
        }

        public HashSet(ICollection<T> list) {
            _data = new Dictionary<T, object>(list.Count);
            foreach (T t in list) {
                _data.Add(t, null);
            }
        }

        public void Add(T item) {
            _data[item] = null;
        }

        public void Clear() {
            _data.Clear();
        }

        public bool Contains(T item) {
            return _data.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _data.Keys.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _data.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool IsSupersetOf(HashSet<T> other) {
            if (Count < other.Count) {
                return false;
            }

            foreach (T t in other._data.Keys) {
                if (!_data.ContainsKey(t)) {
                    return false;
                }
            }

            return true;
        }

        public bool Remove(T item) {
            return _data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return _data.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _data.Keys.GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other) {
            foreach (T t in other) {
                Add(t);
            }
        }

    }
}
#endif
