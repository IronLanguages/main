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

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Scripting.Utils {

    /// <summary>
    /// Dictionary[TKey, TValue] is not thread-safe in the face of concurrent reads and writes. SynchronizedDictionary
    /// provides a thread-safe implementation. It holds onto a Dictionary[TKey, TValue] instead of inheriting from
    /// it so that users who need to do manual synchronization can access the underlying Dictionary[TKey, TValue].
    /// </summary>
    public class SynchronizedDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>> {

        Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// This returns the raw unsynchronized Dictionary[TKey, TValue]. Users are responsible for locking
        /// on it before accessing it. Also, it should not be arbitrarily handed out to other code since deadlocks
        /// can be caused if other code incorrectly locks on it.
        /// </summary>
        public Dictionary<TKey, TValue> UnderlyingDictionary {
            get { return _dictionary; }
        }

        public SynchronizedDictionary() 
            : this(new Dictionary<TKey, TValue>()) {
        }

        public SynchronizedDictionary(Dictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) {
            lock (_dictionary) {
                _dictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key) {
            lock (_dictionary) {
                return _dictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys {
            get {
                lock (_dictionary) {
                    return _dictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key) {
            lock (_dictionary) {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value) {
            lock (_dictionary) {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values {
            get {
                lock (_dictionary) {
                    return _dictionary.Values;
                }
            }
        }

        public TValue this[TKey key] {
            get {
                lock (_dictionary) {
                    return _dictionary[key];
                }
            }
            set {
                lock (_dictionary) {
                    _dictionary[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        private ICollection<KeyValuePair<TKey, TValue>> AsICollection() {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                AsICollection().Add(item);
            }
        }

        public void Clear() {
            lock (_dictionary) {
                AsICollection().Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                return AsICollection().Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            lock (_dictionary) {
                AsICollection().CopyTo(array, arrayIndex);
            }
        }

        public int Count {
            get {
                lock (_dictionary) {
                    return AsICollection().Count;
                }
            }
        }

        public bool IsReadOnly {
            get {
                lock (_dictionary) {
                    return AsICollection().IsReadOnly;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            lock (_dictionary) {
                return AsICollection().Remove(item);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            lock (_dictionary) {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            lock (_dictionary) {
                return _dictionary.GetEnumerator();
            }
        }

        #endregion
    }
}
