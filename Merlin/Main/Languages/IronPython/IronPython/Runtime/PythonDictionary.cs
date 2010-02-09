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
using System.Security;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {

    [PythonType("dict"), Serializable, DebuggerTypeProxy(typeof(PythonDictionary.DebugProxy)), DebuggerDisplay("dict, {Count} items")]
    public class PythonDictionary : IDictionary<object, object>, IDictionary, IAttributesCollection,
#if CLR2
        IValueEquality,
#endif
        ICodeFormattable, IStructuralEquatable {
        [MultiRuntimeAware]
        private static object DefaultGetItem;   // our cached __getitem__ method
        internal readonly DictionaryStorage _storage;

        internal static object MakeDict(CodeContext/*!*/ context, PythonType cls) {
            if (cls == TypeCache.Dict) {
                return new PythonDictionary();
            }
            return PythonCalls.Call(context, cls);
        }

        #region Constructors

        public PythonDictionary() {
            _storage = new CommonDictionaryStorage();
        }

        internal PythonDictionary(DictionaryStorage storage) {
            _storage = storage;
        }

        internal PythonDictionary(IDictionary dict) {
            _storage = new CommonDictionaryStorage();
            lock (_storage) {
                foreach (DictionaryEntry de in dict) {
                    _storage.AddNoLock(de.Key, de.Value);
                }
            }
        }

        internal PythonDictionary(PythonDictionary dict) {
            _storage = dict._storage.Clone();
        }

        internal PythonDictionary(CodeContext/*!*/ context, object o)
            : this() {
            update(context, o);
        }

        internal PythonDictionary(int size) {
            _storage = new CommonDictionaryStorage();
        }

        internal static PythonDictionary FromIAC(CodeContext context, PythonDictionary iac) {
            return iac.GetType() == typeof(PythonDictionary) ? (PythonDictionary)iac : MakeDictFromIAC(context, iac);
        }

        private static PythonDictionary MakeDictFromIAC(CodeContext context, PythonDictionary iac) {
            return new PythonDictionary(new ObjectAttributesAdapter(context, iac));
        }
        
        internal static PythonDictionary MakeSymbolDictionary() {
            return new PythonDictionary(new StringDictionaryStorage());
        }

        internal static PythonDictionary MakeSymbolDictionary(int count) {
            return new PythonDictionary(new StringDictionaryStorage(count));
        }

        public void __init__(CodeContext/*!*/ context, object o, [ParamDictionary]IDictionary<object, object> kwArgs) {
            update(context, o);
            update(context, kwArgs);
        }

        public void __init__(CodeContext/*!*/ context, [ParamDictionary]IDictionary<object, object> kwArgs) {
            update(context, kwArgs);
        }

        public void __init__(CodeContext/*!*/ context, object o) {
            update(context, o);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void __init__() {
        }

        #endregion

        #region IDictionary<object,object> Members

        [PythonHidden]
        public void Add(object key, object value) {
            _storage.Add(key, value);
        }

        [PythonHidden]
        public bool ContainsKey(object key) {
            return _storage.Contains(key);
        }

        public ICollection<object> Keys {
            [PythonHidden]
            get { return keys(); }
        }

        [PythonHidden]
        public bool Remove(object key) {
            try {
                __delitem__(key);
                return true;
            } catch (KeyNotFoundException) {
                ExceptionHelpers.DynamicStackFrames = null;
                return false;
            }
        }

        [PythonHidden]
        public bool TryGetValue(object key, out object value) {
            if (_storage.TryGetValue(key, out value)) {
                return true;
            }

            // we need to manually look up a slot to get the correct behavior when
            // the __missing__ function is declared on a sub-type which is an old-class
            if (GetType() != typeof(PythonDictionary) &&
                PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default,
                this,
                key,
                "__missing__",
                out value)) {
                return true;
            }

            return false;
        }
        
        internal bool TryGetValueNoMissing(object key, out object value) {
            return _storage.TryGetValue(key, out value);
        }

        public ICollection<object> Values {
            [PythonHidden]
            get { return values(); }
        }

        #endregion

        #region ICollection<KeyValuePair<object,object>> Members

        [PythonHidden]
        public void Add(KeyValuePair<object, object> item) {
            _storage.Add(item.Key, item.Value);
        }

        [PythonHidden]
        public void Clear() {
            _storage.Clear();
        }

        [PythonHidden]
        public bool Contains(KeyValuePair<object, object> item) {
            return _storage.Contains(item.Key);
        }

        [PythonHidden]
        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            _storage.GetItems().CopyTo(array, arrayIndex);
        }

        public int Count {
            [PythonHidden]
            get { return _storage.Count; }
        }

        bool ICollection<KeyValuePair<object, object>>.IsReadOnly {
            get { return false; }
        }

        [PythonHidden]
        public bool Remove(KeyValuePair<object, object> item) {
            return _storage.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [PythonHidden]
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                yield return kvp;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return Converter.ConvertToIEnumerator(__iter__());
        }

        public virtual object __iter__() {
            return new DictionaryKeyEnumerator(_storage);
        }

        #endregion

        #region IMapping Members

        public object get(object key) {
            return DictionaryOps.get(this, key);
        }

        public object get(object key, object defaultValue) {
            return DictionaryOps.get(this, key, defaultValue);
        }

        public virtual object this[params object[] key] {
            get {
                if (key == null) {
                    return GetItem(null);
                }

                if (key.Length == 0) {
                    throw PythonOps.TypeError("__getitem__() takes exactly one argument (0 given)");
                }

                return this[PythonTuple.MakeTuple(key)];
            }
            set {
                if (key == null) {
                    SetItem(null, value);
                    return;
                }

                if (key.Length == 0) {
                    throw PythonOps.TypeError("__setitem__() takes exactly two argument (1 given)");
                }

                this[PythonTuple.MakeTuple(key)] = value;
            }
        }

        public virtual object this[object key] {
            get {
                return GetItem(key);
            }
            set {
                SetItem(key, value);
            }
        }

        private void SetItem(object key, object value) {
            _storage.Add(key, value);
        }

        private object GetItem(object key) {
            object ret;
            if (TryGetValue(key, out ret)) {
                return ret;
            }

            throw PythonOps.KeyError(key);
        }


        public virtual void __delitem__(object key) {
            if (!_storage.Remove(key)) {
                throw PythonOps.KeyError(key);
            }
        }

        public virtual void __delitem__(params object[] key) {
            if (key == null) {
                __delitem__((object)null);
            } else if (key.Length > 0) {
                __delitem__(PythonTuple.MakeTuple(key));
            } else {
                throw PythonOps.TypeError("__delitem__() takes exactly one argument (0 given)");
            }
        }

        #endregion

        #region IPythonContainer Members

        public virtual int __len__() {
            return Count;
        }

        #endregion

        #region Python dict implementation

        public void clear() {
            _storage.Clear();
        }

        public bool has_key(object key) {
            return DictionaryOps.has_key(this, key);
        }

        public object pop(object key) {
            return DictionaryOps.pop(this, key);
        }

        public object pop(object key, object defaultValue) {
            return DictionaryOps.pop(this, key, defaultValue);
        }

        public PythonTuple popitem() {
            return DictionaryOps.popitem(this);
        }

        public object setdefault(object key) {
            return DictionaryOps.setdefault(this, key);
        }

        public object setdefault(object key, object defaultValue) {
            return DictionaryOps.setdefault(this, key, defaultValue);
        }

        public virtual List keys() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(kvp.Key);
            }
            return res;
        }

        public virtual List values() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(kvp.Value);
            }
            return res;
        }

        public virtual List items() {
            List res = new List();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                res.append(PythonTuple.MakeTuple(kvp.Key, kvp.Value));
            }
            return res;
        }

        public IEnumerator iteritems() {
            return new DictionaryItemEnumerator(_storage);
        }

        public IEnumerator iterkeys() {
            return new DictionaryKeyEnumerator(_storage);
        }

        public IEnumerator itervalues() {
            return new DictionaryValueEnumerator(_storage);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void update() {
        }

        public void update(CodeContext/*!*/ context, [ParamDictionary]IDictionary<object, object> b) {
            DictionaryOps.update(context, this, b);
        }

        public void update(CodeContext/*!*/ context, object b) {
            DictionaryOps.update(context, this, b);
        }

        public void update(CodeContext/*!*/ context, object b, [ParamDictionary]IDictionary<object, object> f) {
            DictionaryOps.update(context, this, b);
            DictionaryOps.update(context, this, f);
        }

        private static object fromkeysAny(CodeContext/*!*/ context, PythonType cls, object o, object value) {
            PythonDictionary pyDict;
            object dict;

            if (cls == TypeCache.Dict) {
                string str;
                ICollection ic = o as ICollection;

                // creating our own dict, try and get the ideal size and add w/o locks
                if (ic != null) {
                    pyDict = new PythonDictionary(new CommonDictionaryStorage(ic.Count));
                } else if ((str = o as string) != null) {
                    pyDict = new PythonDictionary(str.Length);
                } else {
                    pyDict = new PythonDictionary();
                }

                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    pyDict._storage.AddNoLock(i.Current, value);
                }

                return pyDict;
            } else {
                // call the user type constructor
                dict = MakeDict(context, cls);
                pyDict = dict as PythonDictionary;
            }

            if (pyDict != null) {
                // then store all the keys with their associated value
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    pyDict[i.Current] = value;
                }
            } else {
                // slow path, cls.__new__ returned a user defined dictionary instead of a PythonDictionary.
                PythonContext pc = PythonContext.GetContext(context);
                IEnumerator i = PythonOps.GetEnumerator(o);
                while (i.MoveNext()) {
                    pc.SetIndex(dict, i.Current, value);
                }
            }

            return dict;
        }

        [ClassMethod]
        public static object fromkeys(CodeContext context, PythonType cls, object seq) {
            return fromkeys(context, cls, seq, null);
        }

        [ClassMethod]
        public static object fromkeys(CodeContext context, PythonType cls, object seq, object value) {
            XRange xr = seq as XRange;
            if (xr != null) {
                int n = xr.__len__();
                object ret = PythonContext.GetContext(context).CallSplat(cls);
                if (ret.GetType() == typeof(PythonDictionary)) {
                    PythonDictionary dr = ret as PythonDictionary;
                    for (int i = 0; i < n; i++) {
                        dr[xr[i]] = value;
                    }
                } else {
                    // slow path, user defined dict
                    PythonContext pc = PythonContext.GetContext(context);
                    for (int i = 0; i < n; i++) {
                        pc.SetIndex(ret, xr[i], value);
                    }
                }
                return ret;
            }
            return fromkeysAny(context, cls, seq, value);
        }

        public virtual PythonDictionary copy(CodeContext/*!*/ context) {
            return new PythonDictionary(_storage.Clone());
        }

        public virtual bool __contains__(object key) {
            return _storage.Contains(key);
        }

        // Dictionary has an odd not-implemented check to support custom dictionaries and therefore
        // needs a custom __eq__ / __ne__ implementation.

        [return: MaybeNotImplemented]
        public object __eq__(CodeContext/*!*/ context, object other) {
            if (!(other is PythonDictionary || other is IDictionary<object, object>))
                return NotImplementedType.Value;

            return ScriptingRuntimeHelpers.BooleanToObject(
                ((IStructuralEquatable)this).Equals(other, PythonContext.GetContext(context).EqualityComparerNonGeneric)
            );
        }

        [return: MaybeNotImplemented]
        public object __ne__(CodeContext/*!*/ context, object other) {
            if (!(other is PythonDictionary || other is IDictionary<object, object>))
                return NotImplementedType.Value;

            return ScriptingRuntimeHelpers.BooleanToObject(
                !((IStructuralEquatable)this).Equals(other, PythonContext.GetContext(context).EqualityComparerNonGeneric)
            );
        }

        [return: MaybeNotImplemented]
        public object __cmp__(CodeContext context, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if (!PythonOps.TryGetBoundAttr(context, other, "__len__", out len) ||
                    !PythonOps.TryGetBoundAttr(context, other, "iteritems", out iteritems)) {
                    return NotImplementedType.Value;
                }

                // user-defined dictionary...
                int lcnt = Count;
                int rcnt = PythonContext.GetContext(context).ConvertToInt32(PythonOps.CallWithContext(context, len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

                return DictionaryOps.CompareToWorker(context, this, new List(PythonOps.CallWithContext(context, iteritems)));
            }

            CompareUtil.Push(this, oth);
            try {
                return DictionaryOps.CompareTo(context, this, oth);
            } finally {
                CompareUtil.Pop(this, oth);
            }
        }

        public int __cmp__(CodeContext/*!*/ context, [NotNull]PythonDictionary/*!*/ other) {
            CompareUtil.Push(this, other);
            try {
                return DictionaryOps.CompareTo(context, this, other);
            } finally {
                CompareUtil.Pop(this, other);
            }
        }

        // these are present in CPython but always return NotImplemented.
        [return: MaybeNotImplemented]
        [Python3Warning("dict inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator > (PythonDictionary self, PythonDictionary other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("dict inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator <(PythonDictionary self, PythonDictionary other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("dict inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator >=(PythonDictionary self, PythonDictionary other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("dict inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator <=(PythonDictionary self, PythonDictionary other) {
            return PythonOps.NotImplemented;
        }

        #endregion

        #region IValueEquality Members
#if CLR2
        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeErrorForUnhashableType("dict");
        }

        bool IValueEquality.ValueEquals(object other) {
            return EqualsWorker(other, null);
        }
#endif
        #endregion

        #region IStructuralEquatable Members

        public const object __hash__ = null;

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
            if (CompareUtil.Check(this)) {
                return 0;
            }

            int res;
            ISet pairs = new FrozenSetCollection();
            foreach (KeyValuePair<object, object> kvp in _storage.GetItems()) {
                pairs.PrivAdd(PythonTuple.MakeTuple(kvp.Key, kvp.Value));
            }

            CompareUtil.Push(this);
            try {
                res = pairs.GetHashCode(comparer);
            } finally {
                CompareUtil.Pop(this);
            }

            return res;
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
            return EqualsWorker(other, comparer);
        }

        private bool EqualsWorker(object other, IEqualityComparer comparer) {
            if (Object.ReferenceEquals(this, other)) return true;

            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;
            if (oth.Count != Count) return false;

            PythonDictionary pd = other as PythonDictionary;
            if (pd != null) {
                return ValueEqualsPythonDict(pd, comparer);
            }
            // we cannot call Compare here and compare against zero because Python defines
            // value equality even if the keys/values are unordered.
            List myKeys = keys();

            foreach (object o in myKeys) {
                object res;
                if (!oth.TryGetValue(o, out res)) return false;

                CompareUtil.Push(res);
                try {
                    if (comparer == null) {
                        if (!PythonOps.EqualRetBool(res, this[o])) return false;
                    } else {
                        if (!comparer.Equals(res, this[o])) return false;
                    }
                } finally {
                    CompareUtil.Pop(res);
                }
            }
            return true;
        }

        private bool ValueEqualsPythonDict(PythonDictionary pd, IEqualityComparer comparer) {
            List myKeys = keys();

            foreach (object o in myKeys) {
                object res;
                if (!pd.TryGetValueNoMissing(o, out res)) return false;

                CompareUtil.Push(res);
                try {
                    if (comparer == null) {
                        if (!PythonOps.EqualRetBool(res, this[o])) return false;
                    } else {
                        if (!comparer.Equals(res, this[o])) return false;
                    }
                } finally {
                    CompareUtil.Pop(res);
                }
            }
            return true;
        }

        #endregion

        #region IDictionary Members

        [PythonHidden]
        public bool Contains(object key) {
            return __contains__(key);
        }

        internal class DictEnumerator : IDictionaryEnumerator {
            private IEnumerator<KeyValuePair<object, object>> _enumerator;
            private bool _moved;

            public DictEnumerator(IEnumerator<KeyValuePair<object, object>> enumerator) {
                _enumerator = enumerator;
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry {
                get {
                    // List<T> enumerator doesn't throw, so we need to.
                    if (!_moved) throw new InvalidOperationException();

                    return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);
                }
            }

            public object Key {
                get { return Entry.Key; }
            }

            public object Value {
                get { return Entry.Value; }
            }

            #endregion

            #region IEnumerator Members

            public object Current {
                get { return Entry; }
            }

            public bool MoveNext() {
                if (_enumerator.MoveNext()) {
                    _moved = true;
                    return true;
                }

                _moved = false;
                return false;
            }

            public void Reset() {
                _enumerator.Reset();
                _moved = false;
            }

            #endregion
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new DictEnumerator(_storage.GetItems().GetEnumerator());
        }

        bool IDictionary.IsFixedSize {
            get { return false; }
        }

        bool IDictionary.IsReadOnly {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return this.keys(); }
        }

        ICollection IDictionary.Values {
            get { return values(); }
        }

        void IDictionary.Remove(object key) {
            ((IDictionary<object, object>)this).Remove(key);
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return null; }
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return DictionaryOps.__repr__(context, this);
        }

        #endregion

        #region Fast Attribute Access Support
        /* IAttributesDictionary is implemented on our built-in
         * dictionaries to allow users to assign dictionaries into
         * classes.  These dictionaries will resolve their key via
         * the field table, but only get used when the user does
         * explicit dictionary assignment.
         *
         */

        #region IAttributesCollection Members

        void IAttributesCollection.Add(SymbolId name, object value) {
            this[SymbolTable.IdToString(name)] = value;
        }

        bool IAttributesCollection.ContainsKey(SymbolId name) {
            return __contains__(SymbolTable.IdToString(name));
        }

        bool IAttributesCollection.Remove(SymbolId name) {
            return ((IDictionary<object, object>)this).Remove(SymbolTable.IdToString(name));
        }

        ICollection<object> IAttributesCollection.Keys {
            get { return keys(); }
        }

        int IAttributesCollection.Count {
            get {
                return Count;
            }
        }

        bool IAttributesCollection.TryGetValue(SymbolId name, out object value) {
            if (GetType() != typeof(PythonDictionary) &&
                DictionaryOps.TryGetValueVirtual(DefaultContext.Default, this, SymbolTable.IdToString(name), ref DefaultGetItem, out value)) {
                return true;
            }

            // call Dict.TryGetValue to get the real value.
            return _storage.TryGetValue(SymbolTable.IdToString(name), out value);
        }

        object IAttributesCollection.this[SymbolId name] {
            get {
                return this[name];
            }
            set {
                if (GetType() == typeof(PythonDictionary)) {
                    // no need to call virtual version
                    _storage.Add(SymbolTable.IdToString(name), value);
                } else {
                    this[SymbolTable.IdToString(name)] = value;
                }
            }
        }

        IDictionary<SymbolId, object> IAttributesCollection.SymbolAttributes {
            get {
                Dictionary<SymbolId, object> d = new Dictionary<SymbolId, object>();
                foreach (KeyValuePair<object, object> name in _storage.GetItems()) {
                    string stringKey = name.Key as string;
                    if (stringKey == null) continue;
                    d.Add(SymbolTable.StringToId(stringKey), name.Value);
                }
                return d;
            }
        }

        void IAttributesCollection.AddObjectKey(object name, object value) { this[name] = value; }
        bool IAttributesCollection.TryGetObjectValue(object name, out object value) { return ((IDictionary<object, object>)this).TryGetValue(name, out value); }
        bool IAttributesCollection.RemoveObjectKey(object name) { return ((IDictionary<object, object>)this).Remove(name); }
        bool IAttributesCollection.ContainsObjectKey(object name) { return __contains__(name); }
        IDictionary<object, object> IAttributesCollection.AsObjectKeyedDictionary() { return this; }

        #endregion

        internal bool TryRemoveValue(object key, out object value) {
            return _storage.TryRemoveValue(key, out value);
        }

        #endregion

        #region Debugger View

        internal class DebugProxy {
            private readonly PythonDictionary _dict;

            public DebugProxy(PythonDictionary dict) {
                _dict = dict;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<KeyValueDebugView> Members {
                get {
                    var res = new List<KeyValueDebugView>();
                    foreach (var v in _dict) {
                        res.Add(new KeyValueDebugView(v.Key, v.Value));
                    }
                    return res;
                }
            }
        }

        [DebuggerDisplay("{Value}", Name = "{Key,nq}", Type = "{TypeInfo,nq}")]
        internal class KeyValueDebugView {
            public readonly object Key;
            public readonly object Value;

            public KeyValueDebugView(object key, object value) {
                Key = key;
                Value = value;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public string TypeInfo {
                get {
                    return "Key: " + PythonTypeOps.GetName(Key) + ", " + "Value: " + PythonTypeOps.GetName(Value);
                }
            }
        }

        #endregion
    }

#if !SILVERLIGHT // environment variables not available
    [Serializable]
    internal sealed class EnvironmentDictionaryStorage : DictionaryStorage {
        private readonly CommonDictionaryStorage/*!*/ _storage = new CommonDictionaryStorage();

        public EnvironmentDictionaryStorage() {
            AddEnvironmentVars();
        }

        private void AddEnvironmentVars() {
            try {
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                    Add(de.Key, de.Value);
                }
            } catch (SecurityException) {
                // environment isn't available under partial trust
            }
        }

        public override void Add(object key, object value) {
            _storage.Add(key, value);

            string s1 = key as string;
            string s2 = value as string;
            if (s1 != null && s2 != null) {
                Environment.SetEnvironmentVariable(s1, s2);
            }
        }

        public override bool Remove(object key) {
            bool res = _storage.Remove(key);

            string s = key as string;
            if (s != null) {
                Environment.SetEnvironmentVariable(s, string.Empty);
            }

            return res;
        }

        public override bool Contains(object key) {
            return _storage.Contains(key);
        }

        public override bool TryGetValue(object key, out object value) {
            return _storage.TryGetValue(key, out value);
        }

        public override int Count {
            get { return _storage.Count; }
        }

        public override void Clear() {
            foreach (var x in GetItems()) {
                string key = x.Key as string;
                if (key != null) {
                    Environment.SetEnvironmentVariable(key, string.Empty);
                }
            }

            _storage.Clear();
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            return _storage.GetItems();
        }
    }
#endif

    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonType("dictionary-keyiterator")]
    public sealed class DictionaryKeyEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        private readonly DictionaryStorage _dict;
        private readonly IEnumerator<object> _keys;
        private int _pos;

        internal DictionaryKeyEnumerator(DictionaryStorage dict) {
            _dict = dict;
            _size = dict.Count;
            _keys = dict.GetKeys().GetEnumerator();
            _pos = -1;
        }

        bool IEnumerator.MoveNext() {
            if (_size != _dict.Count) {
                _pos = _size - 1; // make the length 0
                throw PythonOps.RuntimeError("dictionary changed size during iteration");
            }
            if (_keys.MoveNext()) {
                _pos++;
                return true;
            } else {
                return false;
            }
        }

        void IEnumerator.Reset() {
            _keys.Reset();
            _pos = -1;
        }

        object IEnumerator.Current {
            get {
                return _keys.Current;
            }
        }

        object IEnumerator<object>.Current {
            get {
                return _keys.Current;
            }
        }

        void IDisposable.Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __length_hint__() {
            return _size - _pos - 1;
        }
    }

    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonType("dictionary-valueiterator")]
    public sealed class DictionaryValueEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        DictionaryStorage _dict;
        private readonly object[] _values;
        private int _pos;

        internal DictionaryValueEnumerator(DictionaryStorage dict) {
            _dict = dict;
            _size = dict.Count;
            _values = new object[_size];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in dict.GetItems()) {
                _values[i++] = kvp.Value;
            }
            _pos = -1;
        }

        public bool MoveNext() {
            if (_size != _dict.Count) {
                _pos = _size - 1; // make the length 0
                throw PythonOps.RuntimeError("dictionary changed size during iteration");
            }
            if (_pos + 1 < _size) {
                _pos++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            _pos = -1;
        }

        public object Current {
            get {
                return _values[_pos];
            }
        }

        public void Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __len__() {
            return _size - _pos - 1;
        }
    }

    /// <summary>
    /// Note: 
    ///   IEnumerator innerEnum = Dictionary&lt;K,V&gt;.KeysCollections.GetEnumerator();
    ///   innerEnum.MoveNext() will throw InvalidOperation even if the values get changed,
    ///   which is supported in python
    /// </summary>
    [PythonType("dictionary-itemiterator")]
    public sealed class DictionaryItemEnumerator : IEnumerator, IEnumerator<object> {
        private readonly int _size;
        private readonly DictionaryStorage _dict;
        private readonly List<object> _keys;
        private readonly List<object> _values;
        private int _pos;

        internal DictionaryItemEnumerator(DictionaryStorage dict) {
            _dict = dict;            
            _keys = new List<object>(dict.Count);
            _values = new List<object>(dict.Count);
            foreach (KeyValuePair<object, object> kvp in dict.GetItems()) {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
            _size = _values.Count;
            _pos = -1;
        }

        public bool MoveNext() {
            if (_size != _dict.Count) {
                _pos = _size - 1; // make the length 0
                throw PythonOps.RuntimeError("dictionary changed size during iteration");
            }
            if (_pos + 1 < _size) {
                _pos++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            _pos = -1;
        }

        public object Current {
            get {
                return PythonOps.MakeTuple(_keys[_pos], _values[_pos]);
            }
        }

        public void Dispose() {
        }

        public object __iter__() {
            return this;
        }

        public int __len__() {
            return _size - _pos - 1;
        }
    }

}
