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
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    /// <summary>
    /// Represents an object with members that can be dynamically added and removed at runtime.
    /// </summary>
    public sealed class ExpandoObject : IDynamicMetaObjectProvider, IDictionary<string, object> {
        internal readonly object LockObject;                          // the readonly field is used for locking the Expando object
        private ExpandoData _data;                                    // the data currently being held by the Expando object
        private int _count;                                           // the count of available members

        internal readonly static object Uninitialized = new object(); // A marker object used to identify that a value is uninitialized.

        internal const int AmbiguousMatchFound = -2;        // The value is used to indicate there exists ambiguous match in the Expando object
        internal const int NoMatch = -1;                    // The value is used to indicate there is no matching member

        /// <summary>
        /// Creates a new ExpandoObject with no members.
        /// </summary>
        public ExpandoObject() {
            _data = ExpandoData.Empty;
            LockObject = new object();
        }

        #region Get/Set/Delete Helpers

        /// <summary>
        /// Try to get the data stored for the specified class at the specified index.  If the
        /// class has changed a full lookup for the slot will be performed and the correct
        /// value will be retrieved.
        /// </summary>
        internal int TryGetValue(ExpandoClass klass, int index, bool caseInsensitive, string name, out object value) {
            // read the data now.  The data is immutable so we get a consistent view.
            // If there's a concurrent writer they will replace data and it just appears
            // that we won the race
            ExpandoData data = _data;
            if (data.Class != klass || caseInsensitive) {
                /* Re-search for the index matching the name here if
                 *  1) the class has changed, we need to get the correct index and return
                 *  the value there.
                 *  2) the search is case insensitive:
                 *      a. the member specified by index may be deleted, but there might be other
                 *      members matching the name if the binder is case insensitive.
                 *      b. the member that exactly matches the name didn't exist before and exists now,
                 *      need to find the exact match.
                 */
                index = data.Class.GetValueIndex(name, caseInsensitive, this);
            }

            if (index < 0) {
                value = null;
                return index;
            }

            if (data[index] == Uninitialized) {
                value = null;
                return NoMatch;
            }

            // index is now known to be correct
            value = data[index];
            return index;
        }
        
        /// <summary>
        /// Sets the data for the specified class at the specified index.  If the class has
        /// changed then a full look for the slot will be performed.  If the new class does
        /// not have the provided slot then the Expando's class will change. Only case sensitive
        /// setter is supported in ExpandoObject.
        /// </summary>
        internal int TrySetValue(ExpandoClass klass, int index, object value, bool caseInsensitive, string name) {
            lock (LockObject) {
                ExpandoData data = _data;

                if (data.Class != klass || caseInsensitive) {
                    //the class has changed or we are doing a case-insensitive search, 
                    //we need to get the correct index and set the value there.  If we 
                    //don't have the value then we need to promote the class - that 
                    //should only happen when we have multiple concurrent writers.
                    index = data.Class.GetValueIndex(name, caseInsensitive, this);
                    if (index == ExpandoObject.AmbiguousMatchFound) {
                        return index;
                    }
                    if (index == ExpandoObject.NoMatch) {
                        //Before creating a new class with the new member, need to check 
                        //if there is the exact same member but is deleted. We should reuse
                        //the class if there is such a member.
                        int exactMatch = caseInsensitive ? 
                            data.Class.GetValueIndexCaseSensitive(name) :
                            index;
                        if (exactMatch != ExpandoObject.NoMatch) {
                            Debug.Assert(data[exactMatch] == Uninitialized);
                            index = exactMatch;
                        } else {
                            ExpandoClass newClass = data.Class.FindNewClass(name);
                            data = PromoteClassWorker(data.Class, newClass);
                            //After the class promotion, there must be an exact match,
                            //so we can do case-sensitive search here.
                            index = data.Class.GetValueIndexCaseSensitive(name);
                            Debug.Assert(index != ExpandoObject.NoMatch);
                        }
                    }
                }

                //Setting an uninitialized member increases the count of available members
                if (data[index] == Uninitialized) {
                    _count++;
                }

                data[index] = value;
                return index;
            }           
        }              

        /// <summary>
        /// Deletes the data stored for the specified class at the specified index.
        /// </summary>
        internal int TryDeleteValue(ExpandoClass klass, int index, bool caseInsensitive, string name) {
            lock (LockObject) {
                ExpandoData data = _data;

                if (data.Class != klass || caseInsensitive) {
                    // the class has changed or we are doing a case-insensitive search,
                    // we need to get the correct index.  If there is no associated index
                    // we simply can't have the value and we return false.
                    index = data.Class.GetValueIndex(name, caseInsensitive, this);
                }
                if (index < 0) {
                    return index;
                }

                object oldValue = data[index];
                data[index] = Uninitialized;

                //Deleting an available member decreases the count of available members
                if (oldValue != Uninitialized) {
                    _count--;
                }

                return oldValue == Uninitialized ? ExpandoObject.NoMatch : index;
            }
        }

        /// <summary>
        /// Returns true if the member at the specified index has been deleted,
        /// otherwise false.
        /// </summary>
        internal bool IsDeletedMember(int index) {
            ExpandoData data = _data;
            Debug.Assert(index >= 0 && index <= data.Length);

            if (index == data.Length) {
                //the member is a newly added by SetMemberBinder and not in data yet
                return false;
            }

            return _data[index] == ExpandoObject.Uninitialized;
        }

        /// <summary>
        /// Exposes the ExpandoClass which we've associated with this 
        /// Expando object.  Used for type checks in rules.
        /// </summary>
        internal ExpandoClass Class {
            get {
                return _data.Class;
            }
        }

        /// <summary>
        /// Promotes the class from the old type to the new type and returns the new
        /// ExpandoData object.
        /// </summary>
        private ExpandoData PromoteClassWorker(ExpandoClass oldClass, ExpandoClass newClass) {
            Debug.Assert(oldClass != newClass);

            lock (LockObject) {
                if (_data.Class == oldClass) {
                    _data = _data.UpdateClass(newClass);
                }
                return _data;
            }
        }

        /// <summary>
        /// Internal helper to promote a class.  Called from our RuntimeOps helper.  This
        /// version simply doesn't expose the ExpandoData object which is a private
        /// data structure.
        /// </summary>
        internal void PromoteClass(ExpandoClass oldClass, ExpandoClass newClass) {
            PromoteClassWorker(oldClass, newClass);
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new MetaExpando(parameter, this);
        }
        #endregion

        #region Helper methods
        private void TryAddMember(string key, object value) {
            ContractUtils.RequiresNotNull(key, "key");
            lock (LockObject) {
                ExpandoData data = _data;
                int index = data.Class.GetValueIndexCaseSensitive(key);
                if (index >= 0 && data[index] != Uninitialized) {
                    throw Error.SameKeyExistsInExpando(key);
                } 
                if (index < 0) {
                    ExpandoClass newClass = data.Class.FindNewClass(key);
                    data = PromoteClassWorker(data.Class, newClass);
                    index = data.Class.GetValueIndexCaseSensitive(key);
                }
                TrySetValue(data.Class, index, value, false, key);
            }
        }

        private bool TryGetValueForKey(string key, out object value) {
            ExpandoData data = _data;
            int index = data.Class.GetValueIndexCaseSensitive(key);
            int result = TryGetValue(data.Class, index, false, key, out value);
            return result >= 0;
        }

        private bool TryGetValueAndIndexForKey(string key, out object value, out int index) {
            ExpandoData data = _data;
            index = data.Class.GetValueIndexCaseSensitive(key);
            int result = TryGetValue(data.Class, index, false, key, out value);
            return result >= 0;
        }

        private bool ExpandoContainsKey(string key) {
            ExpandoData data = _data;
            for (int i = 0; i < data.Class.Keys.Length; i++ ) {
                if (string.Equals(
                    data.Class.Keys[i],
                    key,
                    StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }

        private bool ExpandoContainsValue(object value) {
            ExpandoData data = _data;
            for (int i = 0; i < data.Class.Keys.Length; i++) {
                if (object.Equals(data[i], value)) {
                    return true;
                }
            }
            return false;
        }

        // We create a non-generic type for the debug view for each different collection type
        // that uses DebuggerTypeProxy, instead of defining a generic debug view type and
        // using different instantiations. The reason for this is that support for generics
        // with using DebuggerTypeProxy is limited. For C#, DebuggerTypeProxy supports only
        // open types (from MSDN http://msdn.microsoft.com/en-us/library/d8eyd8zc.aspx).
        private sealed class KeyCollectionDebugView {
            private ICollection<string> collection;
            public KeyCollectionDebugView(ICollection<string> collection) {
                Debug.Assert(collection != null);
                this.collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string[] Items {
                get {
                    string[] items = new string[collection.Count];
                    collection.CopyTo(items, 0);
                    return items;
                }
            }
        }

        [DebuggerTypeProxy(typeof(KeyCollectionDebugView))]
        [DebuggerDisplay("Count = {Count}")]
        private class KeyCollection : ICollection<string> {
            private readonly ExpandoObject _expando;
            private readonly int _expandoVersion;
            private readonly int _expandoCount;
            private readonly ExpandoData _expandoData;

            internal KeyCollection(ExpandoObject expando) {
                lock (expando.LockObject) {
                    _expando = expando;
                    _expandoVersion = expando._data.Version;
                    _expandoCount = expando._count;
                    _expandoData = expando._data;
                }
            }

            #region ICollection<string> Members

            public void Add(string item) {
                throw Error.CollectionReadOnly();
            }

            public void Clear() {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(string item) {
                lock (_expando.LockObject) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    return _expando.ExpandoContainsKey(item);
                }
            }

            public void CopyTo(string[] array, int arrayIndex) {
                lock (_expando.LockObject) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    ContractUtils.RequiresNotNull(array, "array");
                    ContractUtils.RequiresArrayRange(array, arrayIndex, _expandoCount, "arrayIndex", "Count");
                    ExpandoData data = _expando._data;
                    for (int i = 0; i < data.Class.Keys.Length; i++) {
                        if (data[i] != Uninitialized) {
                            array[arrayIndex++] = data.Class.Keys[i];
                        }
                    }
                }
            }

            public int Count {
                get {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    return _expandoCount;
                }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(string item) {
                throw Error.CollectionReadOnly();
            }

            #endregion

            #region IEnumerable<string> Members

            public IEnumerator<string> GetEnumerator() {
                return GetKeyEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetKeyEnumerator();
            }

            private IEnumerator<string> GetKeyEnumerator() {
                ExpandoData data = _expando._data;
                for (int i = 0; i < data.Class.Keys.Length; i++) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    if (data[i] != Uninitialized) {
                        yield return data.Class.Keys[i];
                    }
                }
            }
            #endregion
        }

        // We create a non-generic type for the debug view for each different collection type
        // that uses DebuggerTypeProxy, instead of defining a generic debug view type and
        // using different instantiations. The reason for this is that support for generics
        // with using DebuggerTypeProxy is limited. For C#, DebuggerTypeProxy supports only
        // open types (from MSDN http://msdn.microsoft.com/en-us/library/d8eyd8zc.aspx).
        private sealed class ValueCollectionDebugView {
            private ICollection<object> collection;
            public ValueCollectionDebugView(ICollection<object> collection) {
                Debug.Assert(collection != null);
                this.collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items {
                get {
                    object[] items = new object[collection.Count];
                    collection.CopyTo(items, 0);
                    return items;
                }
            }
        }

        [DebuggerTypeProxy(typeof(ValueCollectionDebugView))]
        [DebuggerDisplay("Count = {Count}")]
        private class ValueCollection : ICollection<object> {
            private readonly ExpandoObject _expando;
            private readonly int _expandoVersion;
            private readonly int _expandoCount;
            private readonly ExpandoData _expandoData;

            internal ValueCollection(ExpandoObject expando) {
                lock (expando.LockObject) {
                    _expando = expando;
                    _expandoVersion = expando._data.Version;
                    _expandoCount = expando._count;
                    _expandoData = expando._data;
                }
            }

            #region ICollection<string> Members

            public void Add(object item) {
                throw Error.CollectionReadOnly();
            }

            public void Clear() {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(object item) {
                lock (_expando.LockObject) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    return _expando.ExpandoContainsValue(item);
                }
            }

            public void CopyTo(object[] array, int arrayIndex) {
                lock (_expando.LockObject) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    ContractUtils.RequiresNotNull(array, "array");
                    ContractUtils.RequiresArrayRange(array, arrayIndex, _expandoCount, "arrayIndex", "Count");
                    ExpandoData data = _expando._data;
                    for (int i = 0; i < data.Class.Keys.Length; i++) {
                        if (data[i] != Uninitialized) {
                            array[arrayIndex++] = data[i];
                        }
                    }
                }
            }

            public int Count {
                get {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    return _expandoCount;
                }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(object item) {
                throw Error.CollectionReadOnly();
            }

            #endregion

            #region IEnumerable<string> Members

            public IEnumerator<object> GetEnumerator() {
                return GetValueEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetValueEnumerator();
            }

            private IEnumerator<object> GetValueEnumerator() {
                ExpandoData data = _expando._data;
                for (int i = 0; i < data.Class.Keys.Length; i++) {
                    if (_expando._data.Version != _expandoVersion || _expandoData != _expando._data) {
                        //the underlying expando object has changed
                        throw Error.CollectionModifiedWhileEnumerating();
                    }
                    if (data[i] != Uninitialized) {
                        yield return data[i];
                    }
                }
            }

            #endregion
        }

        #endregion

        #region IDictionary<string, object> Members
        ICollection<string> IDictionary<string, object>.Keys {
            get {
                return new KeyCollection(this);
            }
        }

        ICollection<object> IDictionary<string, object>.Values {
            get {
                return new ValueCollection(this);
            }
        }

        object IDictionary<string, object>.this[string key] {
            get {
                object value;
                if (!TryGetValueForKey(key, out value)) {
                    throw Error.KeyDoesNotExistInExpando(key);
                }
                return value;
            }
            set {
                ContractUtils.RequiresNotNull(key, "key");
                lock (LockObject) {
                    ExpandoData data = _data;
                    int index = data.Class.GetValueIndexCaseSensitive(key);
                    if (index < 0) {
                        throw Error.KeyDoesNotExistInExpando(key);
                    }
                    TrySetValue(data.Class, index, value, false, key);
                }
            }
        }

        void IDictionary<string, object>.Add(string key, object value) {
            this.TryAddMember(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key) {
            ContractUtils.RequiresNotNull(key, "key");
            ExpandoData data = _data;
            int index = data.Class.GetValueIndexCaseSensitive(key);
            return index >= 0 && data[index] != Uninitialized;
        }

        bool IDictionary<string, object>.Remove(string key) {
            ContractUtils.RequiresNotNull(key, "key");
            lock (LockObject) {
                ExpandoData data = _data;
                int index = data.Class.GetValueIndexCaseSensitive(key);
                int result = TryDeleteValue(data.Class, index, false, key);
                return result >= 0;
            }
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value) {
            return TryGetValueForKey(key, out value);
        }

        #endregion

        #region ICollection<KeyValuePair<string, object>> Members
        int ICollection<KeyValuePair<string, object>>.Count {
            get {
                return _count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get { return false; }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) {
            this.TryAddMember(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear() {
            //We remove both class and data!
            lock (LockObject) {
                _data = ExpandoData.Empty;
                _count = 0;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) {
            object value;
            if (!TryGetValueForKey(item.Key, out value))
                return false;

            return object.Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange(array, arrayIndex, _count, "arrayIndex", "Count");
            foreach (KeyValuePair<string, object> item in this)
                array[arrayIndex++] = item;
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) {
            object value;
            int index;
            lock (LockObject) {
                ExpandoData data = _data;
                if (!TryGetValueAndIndexForKey(item.Key, out value, out index))
                    return false;
                if (value != item.Value)
                    return false;
                return TryDeleteValue(data.Class, index, false, item.Key) >= 0;
            }
        }
        #endregion

        #region IEnumerable<KeyValuePair<string, object>> Member

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() {
            return GetExpandoEnumerator(_data.Version);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetExpandoEnumerator(_data.Version);
        }

        private IEnumerator<KeyValuePair<string, object>> GetExpandoEnumerator(int version) {
            ExpandoData data = _data;
            for (int i = 0; i < data.Class.Keys.Length; i++) {
                if (_data.Version != version || data != _data) {
                    //the underlying expando object has changed :
                    //1) the version of the expando data changed
                    //2) the data object is changed 
                    throw Error.CollectionModifiedWhileEnumerating();
                }
                if (data[i] != Uninitialized) {
                    yield return new KeyValuePair<string,object>(data.Class.Keys[i], data[i]);
                }
            }
        }
        #endregion

        #region MetaExpando

        private class MetaExpando : DynamicMetaObject {
            public MetaExpando(Expression expression, ExpandoObject value)
                : base(expression, BindingRestrictions.Empty, value) {
            }

            private DynamicMetaObject GetDynamicMetaObjectForMember(string name, bool ignoreCase, DynamicMetaObject fallback) {
                ExpandoClass klass = Value.Class;

                //try to find the member, including the deleted members
                int index = klass.GetValueIndex(name, ignoreCase, Value);
                string methodName = ignoreCase ? "ExpandoTryGetValueIgnoreCase" : "ExpandoTryGetValue";

                ParameterExpression value = Expression.Parameter(typeof(object), "value");

                Expression tryGetValue = Expression.Call(
                    typeof(RuntimeOps).GetMethod(methodName),
                    GetLimitedSelf(),
                    Expression.Constant(klass),
                    Expression.Constant(index),
                    Expression.Constant(name),
                    value
                );

                Expression memberValue = Expression.Block(
                    new ParameterExpression[] { value },
                    Expression.Condition(
                        Expression.IsTrue(tryGetValue),
                        value,
                        DynamicMetaObjectBinder.Convert(fallback.Expression, typeof(object))
                    )
                );

                return new DynamicMetaObject(
                        memberValue,
                        fallback.Restrictions
                );
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");
                DynamicMetaObject memberValue = GetDynamicMetaObjectForMember(
                    binder.Name, 
                    binder.IgnoreCase,
                    binder.FallbackGetMember(this)
                );

                return AddDynamicTestAndDefer(binder, Value.Class, null, memberValue);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                ContractUtils.RequiresNotNull(binder, "binder");
                DynamicMetaObject memberValue = GetDynamicMetaObjectForMember(
                    binder.Name, 
                    binder.IgnoreCase,
                    binder.FallbackInvokeMember(this, args)
                );
                //invoke the member value using the language's binder
                return AddDynamicTestAndDefer(
                    binder,
                    Value.Class,
                    null,
                    binder.FallbackInvoke(memberValue, args, null)
                );
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                ContractUtils.RequiresNotNull(binder, "binder");
                ContractUtils.RequiresNotNull(value, "value");

                ExpandoClass klass;
                int index;

                ExpandoClass originalClass = GetClassEnsureIndex(binder.Name, binder.IgnoreCase, Value, out klass, out index);
                string methodName = binder.IgnoreCase ? "ExpandoTrySetValueIgnoreCase" : "ExpandoTrySetValue";

                return AddDynamicTestAndDefer(
                    binder,
                    klass,
                    originalClass,
                    new DynamicMetaObject(
                        DynamicMetaObjectBinder.Convert(
                            Expression.Call(
                                typeof(RuntimeOps).GetMethod(methodName),
                                GetLimitedSelf(),
                                Expression.Constant(klass),
                                Expression.Constant(index),
                                Expression.Convert(value.Expression, typeof(object)),
                                Expression.Constant(binder.Name)
                            ),
                            typeof(object)
                        ),
                        BindingRestrictions.Empty
                    )
                );
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");

                string methodName = binder.IgnoreCase ? "ExpandoTryDeleteValueIgnoreCase" : "ExpandoTryDeleteValue";
                int index = Value.Class.GetValueIndex(binder.Name, binder.IgnoreCase, Value);

                Expression tryDelete = Expression.Call(
                    typeof(RuntimeOps).GetMethod(methodName),
                    GetLimitedSelf(),
                    Expression.Constant(Value.Class),
                    Expression.Constant(index),
                    Expression.Constant(binder.Name)
                );
                DynamicMetaObject fallback = binder.FallbackDeleteMember(this);

                DynamicMetaObject target = new DynamicMetaObject(
                    Expression.Condition(
                        Expression.IsFalse(tryDelete),
                        DynamicMetaObjectBinder.Convert(fallback.Expression, typeof(object)), //if fail to delete, fall back
                        Expression.Convert(Expression.Constant(true), typeof(object))
                    ),
                    fallback.Restrictions
                );

                return AddDynamicTestAndDefer(binder, Value.Class, null, target);
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                var expandoData = Value._data;
                var klass = expandoData.Class;
                for (int i = 0; i < klass.Keys.Length; i++) {
                    object val = expandoData[i];
                    if (val != ExpandoObject.Uninitialized) {
                        yield return klass.Keys[i];
                    }
                }
            }

            /// <summary>
            /// Adds a dynamic test which checks if the version has changed.  The test is only necessary for
            /// performance as the methods will do the correct thing if called with an incorrect version.
            /// </summary>
            private DynamicMetaObject AddDynamicTestAndDefer(DynamicMetaObjectBinder binder, ExpandoClass klass, ExpandoClass originalClass, DynamicMetaObject succeeds) {

                Expression ifTestSucceeds = succeeds.Expression;
                if (originalClass != null) {
                    // we are accessing a member which has not yet been defined on this class.
                    // We force a class promotion after the type check.  If the class changes the 
                    // promotion will fail and the set/delete will do a full lookup using the new
                    // class to discover the name.
                    Debug.Assert(originalClass != klass);

                    ifTestSucceeds = Expression.Block(
                        Expression.Call(
                            null,
                            typeof(RuntimeOps).GetMethod("ExpandoPromoteClass"),
                            GetLimitedSelf(),
                            Expression.Constant(originalClass),
                            Expression.Constant(klass)
                        ),
                        succeeds.Expression
                    );
                }

                return new DynamicMetaObject(
                    DynamicMetaObjectBinder.Convert(
                        Expression.Condition(
                            Expression.Call(
                                null,
                                typeof(RuntimeOps).GetMethod("ExpandoCheckVersion"),
                                GetLimitedSelf(),
                                Expression.Constant(originalClass ?? klass)
                            ),
                            ifTestSucceeds,
                            binder.GetUpdateExpression(ifTestSucceeds.Type)
                        ),
                        typeof(object)
                    ),
                    GetRestrictions().Merge(succeeds.Restrictions)
                );
            }

            /// <summary>
            /// Gets the class and the index associated with the given name.  Does not update the expando object.  Instead
            /// this returns both the original and desired new class.  A rule is created which includes the test for the
            /// original class, the promotion to the new class, and the set/delete based on the class post-promotion.
            /// </summary>
            private ExpandoClass GetClassEnsureIndex(string name, bool caseInsensitive, ExpandoObject obj, out ExpandoClass klass, out int index) {
                ExpandoClass originalClass = Value.Class;

                index = originalClass.GetValueIndex(name, caseInsensitive, obj) ;
                if (index == ExpandoObject.AmbiguousMatchFound) {
                    klass = originalClass;
                    return null;
                }
                if (index == ExpandoObject.NoMatch) {
                    // go ahead and find a new class now...
                    ExpandoClass newClass = originalClass.FindNewClass(name);

                    klass = newClass;
                    index = newClass.GetValueIndexCaseSensitive(name);

                    Debug.Assert(index != ExpandoObject.NoMatch);
                    return originalClass;
                } else {
                    klass = originalClass;
                    return null;
                }                
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                if (Expression.Type == LimitType) {
                    return Expression;
                }
                return Expression.Convert(Expression, LimitType);
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private BindingRestrictions GetRestrictions() {
                Debug.Assert(Restrictions == BindingRestrictions.Empty, "We don't merge, restrictions are always empty");

                return BindingRestrictions.GetTypeRestriction(this);
            }

            public new ExpandoObject Value {
                get {
                    return (ExpandoObject)base.Value;
                }
            }
        }

        #endregion

        #region ExpandoData
        
        /// <summary>
        /// Stores the class and the data associated with the class as one atomic
        /// pair.  This enables us to do a class check in a thread safe manner w/o
        /// requiring locks.
        /// </summary>
        private class ExpandoData {
            internal static ExpandoData Empty = new ExpandoData();

            /// <summary>
            /// the dynamically assigned class associated with the Expando object
            /// </summary>
            internal readonly ExpandoClass Class;

            /// <summary>
            /// data stored in the expando object, key names are stored in the class.
            /// 
            /// Expando._data must be locked when mutating the value.  Otherwise a copy of it 
            /// could be made and lose values.
            /// </summary>
            private readonly object[] _dataArray;

            /// <summary>
            /// Indexer for getting/setting the data
            /// </summary>
            internal object this[int index] {
                get {
                    return _dataArray[index];
                }
                set {
                    //when the array is updated, version increases, even the new value is the same
                    //as previous. Dictionary type has the same behavior.
                    _version++;
                    _dataArray[index] = value;
                }
            }

            internal int Version {
                get { return _version; }
            }

            internal int Length {
                get { return _dataArray.Length; }
            }

            /// <summary>
            /// Constructs an empty ExpandoData object with the empty class and no data.
            /// </summary>
            private ExpandoData() {
                Class = ExpandoClass.Empty;
                _dataArray = new object[0];
            }

            /// <summary>
            /// the version of the ExpandoObject that tracks set and delete operations
            /// </summary>
            private int _version;

            /// <summary>
            /// Constructs a new ExpandoData object with the specified class and data.
            /// </summary>
            internal ExpandoData(ExpandoClass klass, object[] data, int version) {
                Class = klass;
                _dataArray = data;
                _version = version;
            }

            /// <summary>
            /// Update the associated class and increases the storage for the data array if needed.
            /// </summary>
            /// <param name="newClass"></param>
            /// <returns></returns>
            internal ExpandoData UpdateClass(ExpandoClass newClass) {
                if (_dataArray.Length >= newClass.Keys.Length) {
                    // we have extra space in our buffer, just initialize it to Uninitialized.
                    this[newClass.Keys.Length - 1] = ExpandoObject.Uninitialized;
                    return new ExpandoData(newClass, this._dataArray, this._version);
                } else {
                    // we've grown too much - we need a new object array
                    int oldLength = _dataArray.Length;
                    object[] arr = new object[GetAlignedSize(newClass.Keys.Length)];
                    Array.Copy(_dataArray, arr, _dataArray.Length);
                    ExpandoData newData = new ExpandoData(newClass, arr, this._version);
                    newData[oldLength] = ExpandoObject.Uninitialized;
                    return newData;
                }
            }

            private static int GetAlignedSize(int len) {
                // the alignment of the array for storage of values (must be a power of two)
                const int DataArrayAlignment = 8;

                // round up and then mask off lower bits
                return (len + (DataArrayAlignment - 1)) & (~(DataArrayAlignment - 1));
            }
        }

        #endregion            
    }
}

namespace System.Runtime.CompilerServices {
    public static partial class RuntimeOps {
        /// <summary>
        /// Gets the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <param name="value">The out parameter containing the value of the member.</param>
        /// <returns>True if the member exists in the expando object, otherwise false.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryGetValue(ExpandoObject expando, object indexClass, int index, string name, out object value) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.TryGetValue((ExpandoClass)indexClass, index, false, name, out value) >= 0;
        }

        /// <summary>
        /// Gets the value of an item in an expando object, ignoring the case of the member name.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <param name="value">The out parameter containing the value of the member.</param>
        /// <returns>True if the member exists in the expando object, otherwise false.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryGetValueIgnoreCase(ExpandoObject expando, object indexClass, int index, string name, out object value) {
            ContractUtils.RequiresNotNull(expando, "expando");
            int result = expando.TryGetValue((ExpandoClass)indexClass, index, true, name, out value);
            if (result == ExpandoObject.AmbiguousMatchFound) {
                throw Error.AmbiguousMatchInExpandoObject();
            } else {
                return result >= 0;
            }
        }

        /// <summary>
        /// Sets the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="value">The value of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>
        /// Returns the index for the set member.
        /// </returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExpandoTrySetValue(ExpandoObject expando, object indexClass, int index, object value, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.TrySetValue((ExpandoClass)indexClass, index, value, false, name);
        }

        /// <summary>
        /// Sets the value of an item in an expando object, ignoring the case of the member name.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="value">The value of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>
        /// If there is ambiguous case-insensitive match, returns -2.
        /// Otherwise returns the index for the set member.
        /// </returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExpandoTrySetValueIgnoreCase(ExpandoObject expando, object indexClass, int index, object value, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            int result = expando.TrySetValue((ExpandoClass)indexClass, index, value, true, name);
            if (result == ExpandoObject.AmbiguousMatchFound) {
                throw Error.AmbiguousMatchInExpandoObject();
            }
        }

        /// <summary>
        /// Deletes the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>true if the item was successfully removed; otherwise, false.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryDeleteValue(ExpandoObject expando, object indexClass, int index, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.TryDeleteValue((ExpandoClass)indexClass, index, false, name) >= 0;
        }

        /// <summary>
        /// Deletes the value of an item in an expando object, ignoring the case of the member name.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>true if the item was successfully removed; otherwise, false.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoTryDeleteValueIgnoreCase(ExpandoObject expando, object indexClass, int index, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            int result = expando.TryDeleteValue((ExpandoClass)indexClass, index, true, name);
            if (result == ExpandoObject.AmbiguousMatchFound) {
                throw Error.AmbiguousMatchInExpandoObject();
            } else {
                return result >= 0;
            }
        }

        /// <summary>
        /// Checks the version of the expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="version">The version to check.</param>
        /// <returns>true if the version is equal; otherwise, false.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ExpandoCheckVersion(ExpandoObject expando, object version) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.Class == version;
        }

        /// <summary>
        /// Promotes an expando object from one class to a new class.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="oldClass">The old class of the expando object.</param>
        /// <param name="newClass">The new class of the expando object.</param>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void ExpandoPromoteClass(ExpandoObject expando, object oldClass, object newClass) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.PromoteClass((ExpandoClass)oldClass, (ExpandoClass)newClass);
        }
    }
}

