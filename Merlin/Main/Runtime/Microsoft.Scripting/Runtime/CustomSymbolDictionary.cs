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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Abstract base class used for optimized thread-safe SymbolDictionaries. 
    /// 
    /// Implementers derive from this class and override the GetExtraKeys, TrySetExtraValue, 
    /// and TryGetExtraValue methods. When looking up a value first the extra keys will be 
    /// searched using the optimized Try*ExtraValue functions.  If the value isn't found there
    /// then the value is stored in the underlying .NET dictionary.
    /// 
    /// Implementors can optionally override the object key functionality to store object keys
    /// using their own mechanism.  By default object keys are stored in their own dictionary
    /// which is stored in the primary SymbolId dictionary under an invalid symbol id.
    /// </summary>
    public abstract class CustomSymbolDictionary : BaseSymbolDictionary, IDictionary, IDictionary<object, object>, IAttributesCollection {
        private Dictionary<SymbolId, object> _data;

        protected CustomSymbolDictionary() {
        }

        /// <summary>
        /// Gets a list of the extra keys that are cached by the the optimized implementation
        /// of the module.
        /// </summary>
        public abstract SymbolId[] GetExtraKeys();

        /// <summary>
        /// Try to set the extra value and return true if the specified key was found in the 
        /// list of extra values.
        /// </summary>
        protected internal abstract bool TrySetExtraValue(SymbolId key, object value);

        /// <summary>
        /// Try to get the extra value and returns true if the specified key was found in the
        /// list of extra values.  Returns true even if the value is Uninitialized.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        protected internal abstract bool TryGetExtraValue(SymbolId key, out object value);

        private void InitializeData() {
            Debug.Assert(_data == null);

            _data = new Dictionary<SymbolId, object>();
        }

        /// <summary>
        /// Field dictionaries are usually indexed using literal strings, which is handled using the Symbols.
        /// However, Python does allow non-string keys too. We handle this case by lazily creating an object-keyed dictionary,
        /// and keeping it in the symbol-indexed dictionary. Such access is slower, which is acceptable.
        /// </summary>
        private Dictionary<object, object> GetObjectKeysDictionary() {
            Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
            if (objData == null) {
                if (_data == null) InitializeData();
                objData = new Dictionary<object, object>();
                _data.Add(ObjectKeys, objData);
            }
            return objData;
        }

        private Dictionary<object, object> GetObjectKeysDictionaryIfExists() {
            if (_data == null) return null;

            object objData;
            if (_data.TryGetValue(ObjectKeys, out objData))
                return (Dictionary<object, object>)objData;
            return null;
        }

        #region IDictionary<object, object> Members

        void IDictionary<object, object>.Add(object key, object value) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    if (_data == null) InitializeData();
                    SymbolId keyId = SymbolTable.StringToId(strKey);
                    if (TrySetExtraValue(keyId, value))
                        return;
                    _data.Add(keyId, value);
                }
            } else {
                AddObjectKey(key, value);
            }
        }

        [Confined]
        bool IDictionary<object, object>.ContainsKey(object key) {
            Debug.Assert(!(key is SymbolId));
            lock (this) {
                object dummy;
                return AsObjectKeyedDictionary().TryGetValue(key, out dummy);
            }
        }

        ICollection<object> IDictionary<object, object>.Keys {
            get {
                List<object> res = new List<object>();
                lock (this) if (_data != null) {
                        foreach (SymbolId x in _data.Keys) {
                            if (x == ObjectKeys) continue;
                            res.Add(SymbolTable.IdToString(x));
                        }
                    }

                foreach (SymbolId key in GetExtraKeys()) {
                    if (key.Id < 0) break;

                    object dummy;
                    if (TryGetExtraValue(key, out dummy) && dummy != Uninitialized.Instance) {
                        res.Add(SymbolTable.IdToString(key));
                    }
                }

                GetObjectKeys(res);
                return res;
            }
        }

        bool IDictionary<object, object>.Remove(object key) {
            Debug.Assert(!(key is SymbolId));

            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    SymbolId fieldId = SymbolTable.StringToId(strKey);
                    if (TrySetExtraValue(fieldId, Uninitialized.Instance)) return true;

                    if (_data == null) return false;
                    return _data.Remove(fieldId);
                }
            }

            return RemoveObjectKey(key);
        }

        bool IDictionary<object, object>.TryGetValue(object key, out object value) {
            Debug.Assert(!(key is SymbolId));

            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    SymbolId fieldId = SymbolTable.StringToId(strKey);

                    if (TryGetExtraValue(fieldId, out value) && value != Uninitialized.Instance) return true;

                    if (_data == null) return false;
                    return _data.TryGetValue(fieldId, out value);
                }
            }

            return TryGetObjectValue(key, out value);
        }

        ICollection<object> IDictionary<object, object>.Values {
            get {
                List<object> res = new List<object>();
                lock (this) {
                    if (_data != null) {
                        foreach (SymbolId x in _data.Keys) {
                            if (x == ObjectKeys) continue;
                            res.Add(_data[x]);
                        }
                    }
                }

                foreach (SymbolId key in GetExtraKeys()) {
                    if (key.Id < 0) break;

                    object value;
                    if (TryGetExtraValue(key, out value) && value != Uninitialized.Instance) {
                        res.Add(value);
                    }
                }

                GetObjectValues(res);
                return res;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public object this[object key] {
            get {
                Debug.Assert(!(key is SymbolId));

                string strKey = key as string;
                object res;
                if (strKey != null) {
                    lock (this) {
                        SymbolId id = SymbolTable.StringToId(strKey);

                        if (TryGetExtraValue(id, out res) && !(res is Uninitialized)) return res;

                        if (_data == null) {
                            throw new KeyNotFoundException(key.ToString());
                        }

                        return _data[id];
                    }
                }

                if (TryGetObjectValue(key, out res))
                    return res;

                throw new KeyNotFoundException(key.ToString());
            }
            set {
                Debug.Assert(!(key is SymbolId));

                string strKey = key as string;
                if (strKey != null) {
                    lock (this) {
                        SymbolId id = SymbolTable.StringToId(strKey);
                        if (TrySetExtraValue(id, value)) return;

                        if (_data == null) InitializeData();
                        _data[id] = value;
                    }
                } else {
                    AddObjectKey(key, value);
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<object, object> item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            lock (this) {
                foreach (SymbolId key in GetExtraKeys()) {
                    if (key.Id < 0) break;

                    TrySetExtraValue(key, Uninitialized.Instance);
                }
                _data = null;
            }
        }

        [Confined]
        public bool Contains(KeyValuePair<object, object> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange(array, arrayIndex, Count, "araryIndex", "Count");

            foreach (KeyValuePair<object, object> kvp in ((IEnumerable<KeyValuePair<object, object>>)this)) {
                array[arrayIndex++] = kvp;
            }
        }

        public int Count {
            get {
                int count = GetObjectKeyCount();

                lock (this) {
                    if (_data != null) {
                        foreach (KeyValuePair<SymbolId, object> o in _data) {
                            if (o.Key == SymbolId.Invalid) break;
                            if (o.Key != ObjectKeys) count++;
                        }
                    }

                    foreach (SymbolId key in GetExtraKeys()) {
                        if (key.Id < 0) break;

                        object dummy;
                        if (TryGetExtraValue(key, out dummy) && dummy != Uninitialized.Instance) count++;
                    }
                }

                return count;
            }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [Pure]
        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            if (_data != null) {
                foreach (KeyValuePair<SymbolId, object> o in _data) {
                    if (o.Key == SymbolId.Invalid) break;
                    if (o.Key == ObjectKeys) continue;
                    yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o.Key), o.Value);
                }
            }

            foreach (SymbolId o in GetExtraKeys()) {
                if (o.Id < 0) break;

                object val;
                if (TryGetExtraValue(o, out val) && val != Uninitialized.Instance) {
                    yield return new KeyValuePair<object, object>(SymbolTable.IdToString(o), val);
                }
            }

            IDictionaryEnumerator objItems = GetObjectItems();
            if (objItems != null) {
                while (objItems.MoveNext()) {
                    yield return new KeyValuePair<object, object>(objItems.Key, objItems.Value);
                }
            }
        }

        #endregion

        #region IEnumerable Members

        [Pure]
        public System.Collections.IEnumerator GetEnumerator() {
            List<object> l = new List<object>(this.Keys);
            for (int i = 0; i < l.Count; i++) {
                object baseVal = l[i];
                object nullVal = l[i] = BaseSymbolDictionary.ObjToNull(l[i]);
                if (baseVal != nullVal) {
                    // we've transformed null, stop looking
                    break;
                }
            }
            return l.GetEnumerator();
        }

        #endregion

        #region IAttributesDictionary Members

        public void Add(SymbolId name, object value) {
            lock (this) {
                if (TrySetExtraValue(name, value)) return;

                if (_data == null) InitializeData();
                _data.Add(name, value);
            }
        }

        public bool ContainsKey(SymbolId name) {
            object value;
            if (TryGetExtraValue(name, out value) && value != Uninitialized.Instance) return true;
            if (_data == null) return false;

            lock (this) return _data.ContainsKey(name);
        }

        public bool Remove(SymbolId name) {
            object value;
            if (TryGetExtraValue(name, out value)) {
                if (value == Uninitialized.Instance) return false;
                if (TrySetExtraValue(name, Uninitialized.Instance)) return true;
            }

            if (_data == null) return false;

            lock (this) return _data.Remove(name);
        }

        public bool TryGetValue(SymbolId name, out object value) {
            if (TryGetExtraValue(name, out value) && value != Uninitialized.Instance) return true;

            if (_data == null) return false;

            lock (this) return _data.TryGetValue(name, out value);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                object res;
                if (TryGetExtraValue(name, out res) && res != Uninitialized.Instance) return res;

                lock (this) {
                    if (_data == null) throw new KeyNotFoundException(SymbolTable.IdToString(name));
                    return _data[name];
                }
            }
            set {
                if (TrySetExtraValue(name, value)) return;

                lock (this) {
                    if (_data == null) InitializeData();
                    _data[name] = value;
                }
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                lock (this) {
                    if (_data != null) {
                        foreach (KeyValuePair<SymbolId, object> name in _data) {
                            if (name.Key == ObjectKeys) continue;
                            d.Add(name.Key, name.Value);
                        }
                    }
                    foreach (SymbolId extraKey in GetExtraKeys()) {
                        object value;
                        if (TryGetExtraValue(extraKey, out value) && !(value is Uninitialized))
                            d.Add(extraKey, value);
                    }
                }
                return d;
            }
        }

        /// <summary>
        /// Appends the object keys to the provided list.
        /// </summary>
        protected virtual void GetObjectKeys(List<object> res) {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) res.AddRange(objData.Keys);
            }
        }

        /// <summary>
        /// Appends the values stored under object keys to the provided list.
        /// </summary>
        protected virtual void GetObjectValues(List<object> res) {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) res.AddRange(objData.Values);
            }
        }

        /// <summary>
        /// Gets the count of object keys.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        protected virtual int GetObjectKeyCount() {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) {
                    return objData.Count;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets an IDictionaryEnumerator for all of the object key/value pairs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        public virtual IDictionaryEnumerator GetObjectItems() {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null) {
                    return objData.GetEnumerator();
                }
            }
            return null;
        }

        /// <summary>
        /// Stores the specified value under the specified object key.
        /// </summary>
        public virtual void AddObjectKey(object name, object value) {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionary();
                objData[name] = value;
            }
        }

        public bool ContainsObjectKey(object name) {
            return AsObjectKeyedDictionary().ContainsKey(name);
        }

        /// <summary>
        /// Removes the specified object key from the dictionary.
        /// </summary>
        public virtual bool RemoveObjectKey(object name) {
            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData == null) return false;
                return objData.Remove(name);
            }
        }

        /// <summary>
        /// Attemps to get the value stored under the specified object key.
        /// 
        /// Returns true if the key was found, false if not found.
        /// </summary>
        public virtual bool TryGetObjectValue(object name, out object value) {
            string strKey = name as string;
            if (strKey != null) {
                return ((IAttributesCollection)this).TryGetValue(SymbolTable.StringToId(strKey), out value);
            }

            lock (this) {
                Dictionary<object, object> objData = GetObjectKeysDictionaryIfExists();
                if (objData != null)
                    return objData.TryGetValue(name, out value);
            }
            value = null;
            return false;
        }

        public ICollection<object> Keys { get { return AsObjectKeyedDictionary().Keys; } }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            return this;
        }

        #endregion

        #region IDictionary Members

        [Pure]
        void IDictionary.Add(object key, object value) {
            AsObjectKeyedDictionary().Add(key, value);
        }

        [Pure]
        public bool Contains(object key) {
            return AsObjectKeyedDictionary().ContainsKey(key);
        }

        [Pure]
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            List<IDictionaryEnumerator> enums = new List<IDictionaryEnumerator>();

            enums.Add(new ExtraKeyEnumerator(this));

            if (_data != null) enums.Add(new TransformDictionaryEnumerator(_data));

            IDictionaryEnumerator objItems = GetObjectItems();
            if (objItems != null) {
                enums.Add(objItems);
            }

            return new DictionaryUnionEnumerator(enums);
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return new List<object>(AsObjectKeyedDictionary().Keys); }
        }

        void IDictionary.Remove(object key) {
            Debug.Assert(!(key is SymbolId));
            string strKey = key as string;
            if (strKey != null) {
                SymbolId id = SymbolTable.StringToId(strKey);
                if (TrySetExtraValue(id, Uninitialized.Instance)) return;

                lock (this) if (_data != null) _data.Remove(id);
            } else {
                RemoveObjectKey(key);
            }
        }

        ICollection IDictionary.Values {
            get {
                return new List<object>(AsObjectKeyedDictionary().Values);
            }
        }

        object IDictionary.this[object key] {
            get { return AsObjectKeyedDictionary()[key]; }
            set { AsObjectKeyedDictionary()[key] = value; }
        }

        #endregion

        public void CopyTo(Array array, int index) {
            throw Error.MethodOrOperatorNotImplemented();
        }

        public bool IsSynchronized {
            get {
                return true;
            }
        }

        public object SyncRoot {
            get {
                // TODO: Sync root shouldn't be this, it should be data.
                return this;
            }
        }
    }
}
