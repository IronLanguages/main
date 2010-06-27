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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Simple thread-safe SymbolDictionary used for storing collections of members.
    /// 
    /// Like all SymbolDictionaries this supports both indexing using SymbolId's (IAttributesCollection)
    /// and via object keys (IDictionary&lt;object, object&gt;).
    /// </summary>
    public sealed class SymbolDictionary : BaseSymbolDictionary, IDictionary, IDictionary<object, object>, IAttributesCollection {
        private Dictionary<SymbolId, object> _data = new Dictionary<SymbolId, object>();

        public SymbolDictionary() {
        }

        public SymbolDictionary(IAttributesCollection from) {
            // enumeration of a dictionary requires locking
            // the target dictionary.
            lock (from) {
                foreach (KeyValuePair<object, object> kvp in from) {
                    AsObjectKeyedDictionary().Add(kvp.Key, kvp.Value);
                }
            }

        }

        /// <summary>
        /// Symbol dictionaries are usually indexed using literal strings, which is handled using the Symbols.
        /// However, some languages allow non-string keys too. We handle this case by lazily creating an object-keyed dictionary,
        /// and keeping it in the symbol-indexed dictionary. Such access is slower, which is acceptable.
        /// </summary>
        private Dictionary<object, object> GetObjectKeysDictionary() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) {
                objData = new Dictionary<object, object>();
                _data.Add(BaseSymbolDictionary.ObjectKeys, objData);
            }
            return objData;
        }

        private Dictionary<object, object> GetObjectKeysDictionaryIfExists() {
            object objData;
            if (_data.TryGetValue(BaseSymbolDictionary.ObjectKeys, out objData))
                return (Dictionary<object, object>)objData;
            return null;
        }

        #region IDictionary<object, object> Members

        void IDictionary<object, object>.Add(object key, object value) {
            Debug.Assert(!(key is SymbolId));

            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    _data.Add(SymbolTable.StringToId(strKey), value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionary();
                    objData[key] = value;
                }
            }
        }

        [Confined]
        bool IDictionary<object, object>.ContainsKey(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    if (!SymbolTable.StringHasId(strKey)) {
                        // Avoid creating a SymbolID if this string does not already have one
                        return false;
                    }
                    return _data.ContainsKey(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.ContainsKey(key);
                }
            }
        }

        ICollection<object> IDictionary<object, object>.Keys {
            get {
                // data.Keys is typed as ICollection<SymbolId>. Hence, we cannot return as a ICollection<object>.
                // Instead, we need to copy the data to a List<object>
                List<object> res = new List<object>();

                lock (this) {
                    foreach (SymbolId x in _data.Keys) {
                        if (x == BaseSymbolDictionary.ObjectKeys) continue;
                        res.Add(SymbolTable.IdToString(x));
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Keys);
                }

                return res;
            }
        }

        bool IDictionary<object, object>.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    return _data.Remove(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.Remove(key);
                }
            }
        }

        bool IDictionary<object, object>.TryGetValue(object key, out object value) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    return _data.TryGetValue(SymbolTable.StringToId(strKey), out value);
                } else {
                    value = null;
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null) return false;
                    return objData.TryGetValue(key, out value);
                }
            }
        }

        ICollection<object> IDictionary<object, object>.Values {
            get {
                // Are there any object-keys? If not we can use a fast-path
                lock (this) {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData == null)
                        return _data.Values;

                    // There any object-keys. We need to flatten out all the values
                    List<object> res = new List<object>();

                    foreach (KeyValuePair<SymbolId, object> x in _data) {
                        if (x.Key == BaseSymbolDictionary.ObjectKeys) continue;
                        res.Add(x.Value);
                    }

                    foreach (object o in objData.Values) {
                        res.Add(o);
                    }

                    return res;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public object this[object key] {
            get {
                Debug.Assert(!(key is SymbolId));
                string strKey = key as string;
                lock (this) {
                    if (strKey != null) {
                        object value;
                        if (_data.TryGetValue(SymbolTable.StringToId(strKey), out value))
                            return value;
                    } else {
                        Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                        if (objData != null)
                            return objData[key];
                    }
                }
                throw new KeyNotFoundException(String.Format("'{0}'", key));
            }
            set {
                Debug.Assert(!(key is SymbolId));
                string strKey = key as string;
                lock (this) {
                    if (strKey != null) {
                        _data[SymbolTable.StringToId(strKey)] = value;
                    } else {
                        Dictionary<object, object> objData = GetObjectKeysDictionary();
                        objData[key] = value;
                    }
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<object, object>> Members

        public void Add(KeyValuePair<object, object> item) {
            string strKey = item.Key as string;
            lock (this) {
                if (strKey != null) {
                    _data.Add(SymbolTable.StringToId(strKey), item.Value);
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionary();
                    objData[item.Key] = item.Value;
                }
            }
        }

        public void Clear() {
            lock (this) _data.Clear();
        }

        [Confined]
        public bool Contains(KeyValuePair<object, object> item) {
            object value;
            if (AsObjectKeyedDictionary().TryGetValue(item.Key, out value) && value == item.Value) return true;
            return false;
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            ContractUtils.RequiresNotNull(array, "array");

            lock (this) {
                ContractUtils.RequiresArrayRange(array, arrayIndex, Count, "arrayIndex", "array");
                foreach (KeyValuePair<object, object> o in ((IEnumerable<KeyValuePair<object, object>>)this)) {
                    array[arrayIndex++] = o;
                }
            }
        }

        public int Count {
            get {
                lock (this) {
                    int count = _data.Count;
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) {
                        // -1 is because data contains objData
                        count += objData.Count - 1;
                    }
                    return count;
                }
            }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            lock (this) {
                string strKey = item.Key as string;
                if (strKey != null) {
                    object value;
                    if (AsObjectKeyedDictionary().TryGetValue(strKey, out value) && value == item.Value) {
                        _data.Remove(SymbolTable.StringToId(strKey));
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [Pure]
        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            lock (this) {
                foreach (KeyValuePair<SymbolId, object> o in _data) {
                    if (o.Key == BaseSymbolDictionary.ObjectKeys) continue;
                    yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o.Key), o.Value);
                }

                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) {
                    foreach (KeyValuePair<object, object> o in objData) {
                        yield return o;
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Members

        [Pure]
        public System.Collections.IEnumerator GetEnumerator() {
            foreach (KeyValuePair<SymbolId, object> o in _data) {
                if (o.Key == BaseSymbolDictionary.ObjectKeys) continue;
                yield return SymbolTable.IdToString(o.Key);
            }

            IDictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData != null) {
                foreach (object o in objData.Keys)
                    yield return o;
            }
        }

        #endregion

        #region IAttributesDictionary Members

        public void Add(SymbolId name, object value) {
            lock (this) _data.Add(name, value);
        }

        public bool ContainsKey(SymbolId name) {
            lock (this) return _data.ContainsKey(name);
        }

        public bool Remove(SymbolId name) {
            lock (this) return _data.Remove(name);
        }

        public bool TryGetValue(SymbolId name, out object value) {
            lock (this) return _data.TryGetValue(name, out value);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                lock (this) return _data[name];
            }
            set {
                lock (this) _data[name] = value;
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                lock (this) {
                    if (GetObjectKeysDictionaryIfExists() == null) return _data;

                    Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                    foreach (KeyValuePair<SymbolId, object> name in _data) {
                        if (name.Key == BaseSymbolDictionary.ObjectKeys) continue;
                        d.Add(name.Key, name.Value);
                    }
                    return d;
                }
            }
        }

        public void AddObjectKey(object name, object value) {
            AsObjectKeyedDictionary().Add(name, value);
        }

        public bool ContainsObjectKey(object name) {
            return AsObjectKeyedDictionary().ContainsKey(name);
        }

        public bool RemoveObjectKey(object name) {
            return AsObjectKeyedDictionary().Remove(name);
        }

        public bool TryGetObjectValue(object name, out object value) {
            return AsObjectKeyedDictionary().TryGetValue(name, out value);
        }

        public ICollection<object> Keys { get { return AsObjectKeyedDictionary().Keys; } }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            return this;
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        [Pure]
        public bool Contains(object key) {
            lock (this) return AsObjectKeyedDictionary().ContainsKey(key);
        }

        [Pure]
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) return new TransformDictionaryEnumerator(_data);

            List<IDictionaryEnumerator> enums = new List<IDictionaryEnumerator>();
            enums.Add(new TransformDictionaryEnumerator(_data));

            Dictionary<object, object>.Enumerator objDataEnumerator = objData.GetEnumerator();
            enums.Add(objDataEnumerator);

            return new DictionaryUnionEnumerator(enums);
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get {
                // data.Keys is typed as ICollection<SymbolId>. Hence, we cannot return as a ICollection.
                // Instead, we need to copy the data to a List.
                List<object> res = new List<object>();

                lock (this) {
                    foreach (SymbolId x in _data.Keys) {
                        if (x == BaseSymbolDictionary.ObjectKeys) continue;
                        res.Add(SymbolTable.IdToString(x));
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Keys);
                }

                return res;
            }
        }

        void IDictionary.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            lock (this) {
                if (strKey != null) {
                    _data.Remove(SymbolTable.StringToId(strKey));
                } else {
                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null)
                        objData.Remove(key);
                }
            }
        }

        ICollection IDictionary.Values {
            get {
                List<object> res = new List<object>();

                lock (this) {
                    foreach (KeyValuePair<SymbolId, object> x in _data) {
                        if (x.Key == BaseSymbolDictionary.ObjectKeys) continue;
                        res.Add(x.Value);
                    }

                    Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                    if (objData != null) res.AddRange(objData.Values);
                }

                return res;
            }
        }

        object IDictionary.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        #endregion

        public void CopyTo(Array array, int index) {
            ContractUtils.RequiresNotNull(array, "array");

            lock (this) {
                ContractUtils.RequiresListRange(array, index, Count, "index", "array");
                foreach (object o in this) {
                    array.SetValue(o, index++);
                }
            }
        }

        public bool IsSynchronized {
            get {
                return true;
            }
        }

        public object SyncRoot {
            get {
                // TODO: We should really lock on something else...
                return this;
            }
        }
    }

}
