/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Collections;
using System;

namespace IronRuby.Builtins {

    /// <summary>
    /// TODO: ordered dictionary
    /// TODO: all operations should check frozen state!
    /// 
    /// Dictionary inherits from Object, mixes in Enumerable.
    /// Ruby hash is a Dictionary{object, object}, but it adds default value/proc
    /// </summary>
    public partial class Hash : IDictionary, IDictionary<object, object>, IRubyObjectState, IDuplicable {
        private Dictionary<object, object> _dictionary;

        // The default value can be a Proc that we should *return*, and that is different
        // from the default value being a Proc that we should *call*, hence two variables
        private Proc _defaultProc;
        private object _defaultValue;

        private uint _flags;
        private const uint IsFrozenFlag = 1;
        private const uint IsTaintedFlag = 2;
        private const uint IsUntrustedFlag = 4;

        #region Construction

        public Hash(RubyContext/*!*/ context)
            : this(new Dictionary<object, object>(context.EqualityComparer)) {
        }

        public Hash(IEqualityComparer<object>/*!*/ comparer)
            : this(new Dictionary<object, object>(comparer)) {
        }

        public Hash(EqualityComparer/*!*/ comparer, Proc defaultProc, object defaultValue)
            : this(new Dictionary<object, object>(comparer)) {
            _defaultValue = defaultValue;
            _defaultProc = defaultProc;
        }

        public Hash(EqualityComparer/*!*/ comparer, int capacity)
            : this(new Dictionary<object, object>(capacity, comparer)) {
        }

        public Hash(IDictionary<object, object>/*!*/ dictionary)
            : this(new Dictionary<object, object>(dictionary)) {
        }
        
        public Hash(IDictionary<object, object>/*!*/ dictionary, EqualityComparer/*!*/ comparer) 
            : this(new Dictionary<object, object>(dictionary, comparer)) {
        }

        public Hash(Hash/*!*/ hash)
            : this(new Dictionary<object, object>(hash, hash._dictionary.Comparer)) {
            _defaultProc = hash._defaultProc;
            _defaultValue = hash.DefaultValue;
        }

        private Hash(Dictionary<object, object> dictionary) {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Creates a blank instance of a RubyArray or its subclass given the Ruby class object.
        /// </summary>
        public static Hash/*!*/ CreateInstance(RubyClass/*!*/ rubyClass) {
            return (rubyClass.GetUnderlyingSystemType() == typeof(Hash)) ? new Hash(rubyClass.Context) : new Hash.Subclass(rubyClass);
        }

        /// <summary>
        /// Creates an empty instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Hash.
        /// </summary>
        protected virtual Hash/*!*/ CreateInstance() {
            return new Hash(_dictionary.Comparer);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        #endregion

        #region Flags

        public void RequireNotFrozen() {
            if ((_flags & IsFrozenFlag) != 0) {
                throw RubyExceptions.CreateObjectFrozenError();
            }
        }

        private void Mutate() {
            RequireNotFrozen();
        }

        public bool IsTainted {
            get {
                return (_flags & IsTaintedFlag) != 0;
            }
            set {
                Mutate();
                _flags = (_flags & ~IsTaintedFlag) | (value ? IsTaintedFlag : 0);
            }
        }

        public bool IsUntrusted {
            get {
                return (_flags & IsUntrustedFlag) != 0;
            }
            set {
                Mutate();
                _flags = (_flags & ~IsUntrustedFlag) | (value ? IsUntrustedFlag : 0);
            }
        }

        public bool IsFrozen {
            get {
                return (_flags & IsFrozenFlag) != 0;
            }
        }

        void IRubyObjectState.Freeze() {
            Freeze();
        }

        public Hash/*!*/ Freeze() {
            _flags |= IsFrozenFlag;
            return this;
        }

        #endregion

        #region Ruby-specific features

        public Proc DefaultProc {
            get { return _defaultProc; }
            set {
                Mutate();
                _defaultProc = value;
            }
        }

        public object DefaultValue {
            get { return _defaultValue; }
            set {
                Mutate();
                _defaultValue = value;
            }
        }

        public IEqualityComparer<object> Comparer {
            get { 
                return _dictionary.Comparer; 
            }
        }

        public void SetComparer(IEqualityComparer<object> comparer) {
            var newDictionary = new Dictionary<object, object>(_dictionary.Count, comparer);
            foreach (var entry in _dictionary) {
                newDictionary[entry.Key] = entry.Value;
            }
            _dictionary = newDictionary;
        }

        #endregion

        #region IDictionary<object, object>

        public void Add(object key, object value) {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(object key) {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<object> Keys {
            get { return _dictionary.Keys; }
        }

        public bool Remove(object key) {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(object key, out object value) {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<object> Values {
            get { return _dictionary.Values; }
        }

        public object this[object key] {
            get {
                return _dictionary[key];
            }
            set {
                _dictionary[key] = value;
            }
        }

        public void Add(KeyValuePair<object, object> item) {
            ((ICollection<KeyValuePair<object, object>>)_dictionary).Add(item);
        }

        public void Clear() {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item) {
            return ((ICollection<KeyValuePair<object, object>>)_dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<object, object>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly {
            get { return ((ICollection<KeyValuePair<object, object>>)_dictionary).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<object, object> item) {
            return ((ICollection<KeyValuePair<object, object>>)_dictionary).Remove(item);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<object, object>>)_dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        #endregion

        #region IDictionary

        bool IDictionary.Contains(object key) {
            return _dictionary.ContainsKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return CollectionUtils.ToDictionaryEnumerator(_dictionary.GetEnumerator());
        }

        void IDictionary.Add(object key, object value) {
            _dictionary.Add(key, value);
        }

        bool IDictionary.IsFixedSize {
            get { return false; }
        }

        bool IDictionary.IsReadOnly {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return _dictionary.Keys; }
        }

        void IDictionary.Remove(object key) {
            _dictionary.Remove(key);
        }

        ICollection IDictionary.Values {
            get { return _dictionary.Values; }
        }

        object IDictionary.this[object key] {
            get {
                object result;
                _dictionary.TryGetValue(key, out result);
                return result;
            }
            set {
                _dictionary[key] = value;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            foreach (DictionaryEntry entry in ((IDictionary)this)) {
                array.SetValue(entry, index++);
            }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion
    }
}
