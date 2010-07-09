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
    /// Abstract base class used for optimized thread-safe dictionaries which have a set
    /// of pre-defined string keys.
    /// 
    /// Implementers derive from this class and override the GetExtraKeys, TrySetExtraValue, 
    /// and TryGetExtraValue methods. When looking up a value first the extra keys will be 
    /// searched using the optimized Try*ExtraValue functions.  If the value isn't found there
    /// then the value is stored in the underlying .NET dictionary.
    /// 
    /// This dictionary can store object values in addition to string values.  It also supports
    /// null keys.
    /// </summary>
    public abstract class CustomStringDictionary : 
#if CLR2
        IValueEquality,
#endif
        IDictionary, IDictionary<object, object> {

        private Dictionary<object, object> _data;
        private static readonly object _nullObject = new object();

        protected CustomStringDictionary() {
        }

        /// <summary>
        /// Gets a list of the extra keys that are cached by the the optimized implementation
        /// of the module.
        /// </summary>
        public abstract string[] GetExtraKeys();

        /// <summary>
        /// Try to set the extra value and return true if the specified key was found in the 
        /// list of extra values.
        /// </summary>
        protected internal abstract bool TrySetExtraValue(string key, object value);

        /// <summary>
        /// Try to get the extra value and returns true if the specified key was found in the
        /// list of extra values.  Returns true even if the value is Uninitialized.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        protected internal abstract bool TryGetExtraValue(string key, out object value);

        private void InitializeData() {
            Debug.Assert(_data == null);

            _data = new Dictionary<object, object>();
        }

        #region IDictionary<object, object> Members

        void IDictionary<object, object>.Add(object key, object value) {
            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    if (_data == null) InitializeData();
                    if (TrySetExtraValue(strKey, value))
                        return;
                    _data.Add(strKey, value);
                }
            } else {
                AddObjectKey(key, value);
            }
        }

        private void AddObjectKey(object key, object value) {
            if (_data == null) {
                InitializeData();
            }
            _data[key] = value;
        }

        [Confined]
        bool IDictionary<object, object>.ContainsKey(object key) {
            lock (this) {
                if (_data == null) {
                    return false;
                }
                object dummy;
                return _data.TryGetValue(key, out dummy);
            }
        }

        public ICollection<object> Keys {
            get {
                List<object> res = new List<object>();
                lock (this) {
                    if (_data != null) {
                        res.AddRange(_data.Keys);
                    }
                }

                foreach (var key in GetExtraKeys()) {
                    object dummy;
                    if (TryGetExtraValue(key, out dummy) && dummy != Uninitialized.Instance) {
                        res.Add(key);
                    }
                }

                return res;
            }
        }

        bool IDictionary<object, object>.Remove(object key) {
            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    if (TrySetExtraValue(strKey, Uninitialized.Instance)) return true;

                    if (_data == null) return false;
                    return _data.Remove(strKey);
                }
            }

            return RemoveObjectKey(key);
        }

        private bool RemoveObjectKey(object key) {
            return _data.Remove(key);
        }

        public bool TryGetValue(object key, out object value) {
            string strKey = key as string;
            if (strKey != null) {
                lock (this) {
                    if (TryGetExtraValue(strKey, out value) && value != Uninitialized.Instance) return true;

                    if (_data == null) return false;
                    return _data.TryGetValue(strKey, out value);
                }
            }

            return TryGetObjectValue(key, out value);
        }

        private bool TryGetObjectValue(object key, out object value) {
            if (_data == null) {
                value = null;
                return false;
            }
            return _data.TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<object, object>.Values {
            get {
                List<object> res = new List<object>();
                lock (this) {
                    if (_data != null) {
                        res.AddRange(_data.Values);
                    }
                }

                foreach (var key in GetExtraKeys()) {
                    object value;
                    if (TryGetExtraValue(key, out value) && value != Uninitialized.Instance) {
                        res.Add(value);
                    }
                }

                return res;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public object this[object key] {
            get {
                string strKey = key as string;
                object res;
                if (strKey != null) {
                    lock (this) {
                        if (TryGetExtraValue(strKey, out res) && !(res is Uninitialized)) return res;

                        if (_data == null) {
                            throw new KeyNotFoundException(strKey);
                        }

                        return _data[strKey];
                    }
                }

                if (TryGetObjectValue(key, out res))
                    return res;

                throw new KeyNotFoundException(key.ToString());
            }
            set {
                string strKey = key as string;
                if (strKey != null) {
                    lock (this) {
                        if (TrySetExtraValue(strKey, value)) return;

                        if (_data == null) InitializeData();
                        _data[strKey] = value;
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
                foreach (var key in GetExtraKeys()) {
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
                int count = _data != null ? _data.Count : 0;

                lock (this) {
                    foreach (var key in GetExtraKeys()) {
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

        public bool Remove(object key) {
            string strKey = key as string;
            if (strKey != null) {
                if (TrySetExtraValue(strKey, Uninitialized.Instance)) {
                    return true;
                }

                lock (this) {
                    if (_data != null) {
                        return _data.Remove(strKey);
                    }
                    return false;
                }
            } else {
                return RemoveObjectKey(key);
            }

        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [Pure]
        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator() {
            if (_data != null) {
                foreach (KeyValuePair<object, object> o in _data) {
                    yield return o;
                }
            }

            foreach (var o in GetExtraKeys()) {
                object val;
                if (TryGetExtraValue(o, out val) && val != Uninitialized.Instance) {
                    yield return new KeyValuePair<object, object>(o, val);
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
                object nullVal = l[i] = CustomStringDictionary.ObjToNull(l[i]);
                if (baseVal != nullVal) {
                    // we've transformed null, stop looking
                    break;
                }
            }
            return l.GetEnumerator();
        }

        #endregion

        #region IDictionary Members

        [Pure]
        void IDictionary.Add(object key, object value) {
            ((IDictionary<object, object>)this).Add(key, value);
        }

        [Pure]
        public bool Contains(object key) {
            object dummy;
            return ((IDictionary<object, object>)this).TryGetValue(key, out dummy);
        }

        [Pure]
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            List<IDictionaryEnumerator> enums = new List<IDictionaryEnumerator>();

            enums.Add(new ExtraKeyEnumerator(this));

            if (_data != null) enums.Add(((IDictionary)_data).GetEnumerator());

            return new DictionaryUnionEnumerator(enums);
        }

        public bool IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get {
                return new List<object>(((IDictionary<object, object>)this).Keys);
            }
        }

        void IDictionary.Remove(object key) {
            Remove(key);
        }

        ICollection IDictionary.Values {
            get {
                return new List<object>(((IDictionary<object, object>)this).Values);
            }
        }

        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
            throw Error.DictionaryNotHashable();
        }

        public virtual bool ValueEquals(object other) {
            if (Object.ReferenceEquals(this, other)) return true;

            IDictionary<object, object> oth = other as IDictionary<object, object>;
            IDictionary<object, object> ths = this as IDictionary<object, object>;
            if (oth == null) return false;

            if (oth.Count != ths.Count) return false;

            foreach (KeyValuePair<object, object> o in ths) {
                object res;
                if (!oth.TryGetValue(o.Key, out res))
                    return false;
#if CLR2
                IValueEquality ve = res as IValueEquality;
                if (ve != null) {
                    if (!ve.ValueEquals(o.Value)) return false;
                } else if ((ve = (o.Value as IValueEquality)) != null) {
                    if (!ve.Equals(res)) return false;
                } else
#endif
                    if (res != null) {
                        if (!res.Equals(o.Value)) return false;
                    } else if (o.Value != null) {
                        if (!o.Value.Equals(res)) return false;
                    } // else both null and are equal
            }
            return true;
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

        public static object NullToObj(object o) {
            if (o == null) return _nullObject;
            return o;
        }

        public static object ObjToNull(object o) {
            if (o == _nullObject) return null;
            return o;
        }

        public static bool IsNullObject(object o) {
            return o == _nullObject;
        }
    }

    [Obsolete("Derive directly from CustomStringDictionary instead")]
    public abstract class CustomSymbolDictionary : CustomStringDictionary {
    }
}
