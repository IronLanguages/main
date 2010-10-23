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

namespace Microsoft.Scripting.Utils {

    // Like ReadOnlyCollection<T>: wraps an IDictionary<TKey, TValue> in a read-only wrapper
    [Serializable]
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        // For wrapping non-readonly Keys, Values collections
        // Not used for standard dictionaries, which return read-only Keys and Values
        private sealed class ReadOnlyWrapper<T> : ICollection<T> {
            // no idea why this warning is here
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly ICollection<T> _collection;
            
            internal ReadOnlyWrapper(ICollection<T> collection) {
                _collection = collection;
            }

            #region ICollection<T> Members

            public void Add(T item) {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Clear() {
                throw new NotSupportedException("Collection is read-only.");
            }

            public bool Contains(T item) {
                return _collection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                _collection.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return _collection.Count; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(T item) {
                throw new NotSupportedException("Collection is read-only.");
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator() {
                return _collection.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return _collection.GetEnumerator();
            }

            #endregion
        }

        private readonly IDictionary<TKey, TValue> _dict;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict) {
            ReadOnlyDictionary<TKey, TValue> rodict = dict as ReadOnlyDictionary<TKey, TValue>;
            _dict = (rodict != null) ? rodict._dict : dict;
        }

        #region IDictionary<K,V> Members

        public bool ContainsKey(TKey key) {
            return _dict.ContainsKey(key);
        }

        public ICollection<TKey> Keys {
            get {
                ICollection<TKey> keys = _dict.Keys;
                if (!keys.IsReadOnly) {
                    return new ReadOnlyWrapper<TKey>(keys);
                }
                return keys;
            }
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return _dict.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values {
            get {
                ICollection<TValue> values = _dict.Values;
                if (!values.IsReadOnly) {
                    return new ReadOnlyWrapper<TValue>(values);
                }
                return values;
            }
        }

        public TValue this[TKey key] {
            get {
                return _dict[key];
            }
        }


        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
            throw new NotSupportedException("Collection is read-only.");
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key) {
            throw new NotSupportedException("Collection is read-only.");
        }

        TValue IDictionary<TKey, TValue>.this[TKey key] {
            get {
                return _dict[key];
            }
            set {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            _dict.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _dict.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException("Collection is read-only.");
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
            throw new NotSupportedException("Collection is read-only.");
        }

        bool ICollection<KeyValuePair<TKey,TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException("Collection is read-only.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return _dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _dict.GetEnumerator();
        }

        #endregion
    }
}
