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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime {
    /// <summary>
    /// General purpose storage used for most PythonDictionarys.
    /// 
    /// This dictionary storage is thread safe for multiple readers or writers.
    /// 
    /// Mutations to the dictionary involves a simple locking strategy of
    /// locking on the DictionaryStorage object to ensure that only one
    /// mutation happens at a time.
    /// 
    /// Reads against the dictionary happen lock free.  When the dictionary is mutated
    /// it is either adding or removing buckets in a thread-safe manner so that the readers
    /// will either see a consistent picture as if the read occured before or after the mutation.
    /// 
    /// When resizing the dictionary the buckets are replaced atomically so that the reader
    /// sees the new buckets or the old buckets.  When reading the reader first reads
    /// the buckets and then calls a static helper function to do the read from the bucket
    /// array to ensure that readers are not seeing multiple bucket arrays.
    /// </summary>
    [Serializable]
    internal sealed class CommonDictionaryStorage : DictionaryStorage
#if !SILVERLIGHT
, ISerializable, IDeserializationCallback
#endif
 {
        private Bucket[] _buckets;
        private int _count;
        private Func<object, int> _hashFunc;
        private Func<object, object, bool> _eqFunc;
        private Type _keyType;

        private const int InitialBucketSize = 7;
        private const int ResizeMultiplier = 3;

        // pre-created delegate instances shared by all homogeneous dictionaries for primitive types.
        private static readonly Func<object, int> _primitiveHash = PrimitiveHash, _doubleHash = DoubleHash, _intHash = IntHash, _tupleHash = TupleHash, _genericHash = GenericHash;
        private static readonly Func<object, object, bool> _intEquals = IntEquals, _doubleEquals = DoubleEquals, _stringEquals = StringEquals, _tupleEquals = TupleEquals, _genericEquals = GenericEquals, _objectEq = System.Object.ReferenceEquals;

        // marker type used to indicate we've gone megamorphic
        private static readonly Type HeterogeneousType = typeof(CommonDictionaryStorage);   // a type we can never see here.

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public CommonDictionaryStorage() {
        }

        /// <summary>
        /// Creates a new dictionary storage with no buckets
        /// </summary>
        public CommonDictionaryStorage(int count) {
            _buckets = new Bucket[count + 1];
        }

        /// <summary>
        /// Creates a new dictionary geting values/keys from the
        /// items arary
        /// </summary>
        public CommonDictionaryStorage(object[] items, bool isHomogeneous)
            : this(Math.Max(items.Length / 2, InitialBucketSize)) {
            // always called w/ items, and items should be even (key/value pairs)
            Debug.Assert(items.Length > 0 && (items.Length & 0x01) == 0);

            PythonType t = DynamicHelpers.GetPythonType(items[1]);

            if (!isHomogeneous) {
                for (int i = 1; i < items.Length / 2; i++) {
                    if (DynamicHelpers.GetPythonType(items[i * 2 + 1]) != t) {
                        SetHeterogeneousSites();
                        t = null;
                        break;
                    }
                }
            }

            if (t != null) {
                // homogeneous collection
                UpdateHelperFunctions(t, items[1]);
            }

            for (int i = 0; i < items.Length / 2; i++) {
                AddOne(items[i * 2 + 1], items[i * 2]);
            }
        }

        private void AddItems(object[] items) {
            for (int i = 0; i < items.Length / 2; i++) {
                AddNoLock(items[i * 2 + 1], items[i * 2]);
            }
        }

        /// <summary>
        /// Creates a new dictionary storage with the given set of buckets
        /// and size.  Used when cloning the dictionary storage.
        /// </summary>
        private CommonDictionaryStorage(Bucket[] buckets, int count, Type keyType, Func<object, int> hashFunc, Func<object, object, bool> eqFunc) {
            _buckets = buckets;
            _count = count;
            _keyType = keyType;
            _hashFunc = hashFunc;
            _eqFunc = eqFunc;
        }

#if !SILVERLIGHT
        private CommonDictionaryStorage(SerializationInfo info, StreamingContext context) {
            // remember the serialization info, we'll deserialize when we get the callback.  This
            // enables special types like DBNull.Value to successfully be deserialized inside of us.  We
            // store the serialization info in a special bucket so we don't have an extra field just for
            // serialization
            _buckets = new Bucket[] { new DeserializationBucket(info) };
        }
#endif

        /// <summary>
        /// Adds a new item to the dictionary, replacing an existing one if it already exists.
        /// </summary>
        public override void Add(object key, object value) {
            lock (this) {
                AddNoLock(key, value);
            }
        }

        public override void AddNoLock(object key, object value) {
            if (_buckets == null) {
                Initialize();
            }

            Type t = CompilerHelpers.GetType(key);
            if (t != _keyType && _keyType != HeterogeneousType) {
                UpdateHelperFunctions(t, key);
            }

            AddOne(key, value);
        }

        private void AddOne(object key, object value) {
            if (Add(_buckets, key, value)) {
                _count++;

                if (_count >= _buckets.Length) {
                    // grow the hash table
                    EnsureSize(_buckets.Length * ResizeMultiplier);
                }
            }
        }

        private void UpdateHelperFunctions(Type t, object key) {
            if (_keyType == null) {
                // first time through, get the sites for this specific type...
                if (t == typeof(int)) {
                    _hashFunc = _intHash;
                    _eqFunc = _intEquals;
                } else if (t == typeof(string)) {
                    _hashFunc = _primitiveHash;
                    _eqFunc = _stringEquals;
                } else if (t == typeof(double)) {
                    _hashFunc = _doubleHash;
                    _eqFunc = _doubleEquals;
                } else if (t == typeof(PythonTuple)) {
                    _hashFunc = _tupleHash;
                    _eqFunc = _tupleEquals;
                } else if(t == typeof(Type).GetType()) {    // this odd check checks for RuntimeType.
                    _hashFunc = _primitiveHash;
                    _eqFunc = _objectEq;
                } else {
                    // random type, but still homogeneous... get a shared site for this type.
                    PythonType pt = DynamicHelpers.GetPythonType(key);
                    var hashSite = DefaultContext.DefaultPythonContext.GetHashSite(pt);
                    var equalSite = DefaultContext.DefaultPythonContext.GetEqualSite(pt);

                    AssignSiteDelegates(hashSite, equalSite);
                }

                _keyType = t;
            } else if (_keyType != HeterogeneousType) {
                // 2nd time through, we're adding a new type so we have mutliple types now, 
                // make a new site for this storage

                SetHeterogeneousSites();

                // we need to clone the buckets so any lock-free readers will only see
                // the old buckets which are homogeneous
                _buckets = (Bucket[])_buckets.Clone();
            }
            // else we have already created a new site this dictionary
        }

        private void SetHeterogeneousSites() {
            var hashSite = DefaultContext.DefaultPythonContext.MakeHashSite();
            var equalSite = DefaultContext.DefaultPythonContext.MakeEqualSite();

            AssignSiteDelegates(hashSite, equalSite);

            _keyType = HeterogeneousType;
        }

        private void AssignSiteDelegates(CallSite<Func<CallSite, object, int>> hashSite, CallSite<Func<CallSite, object, object, bool>> equalSite) {
            _hashFunc = (o) => hashSite.Target(hashSite, o);
            _eqFunc = (o1, o2) => equalSite.Target(equalSite, o1, o2);
        }

        private void EnsureSize(int newSize) {
            if (_buckets.Length >= newSize) {
                return;
            }

            Bucket[] newBuckets = new Bucket[newSize];

            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    Bucket next = curBucket.Next;

                    AddWorker(newBuckets, curBucket.Key, curBucket.Value, curBucket.HashCode);

                    curBucket = next;
                }
            }

            _buckets = newBuckets;
        }

        /// <summary>
        /// Initializes the buckets to their initial capacity, the caller
        /// must check if the buckets are empty first.
        /// </summary>
        private void Initialize() {
            _buckets = new Bucket[InitialBucketSize];
        }

        /// <summary>
        /// Static add helper that works over a single set of buckets.  Used for
        /// both the normal add case as well as the resize case.
        /// </summary>
        private bool Add(Bucket[] buckets, object key, object value) {
            int hc = Hash(key);

            return AddWorker(buckets, key, value, hc);
        }

        private bool AddWorker(Bucket[] buckets, object key, object value, int hc) {
            int index = hc % buckets.Length;
            Bucket prev = buckets[index];
            Bucket cur = prev;

            while (cur != null) {
                if (cur.HashCode == hc && _eqFunc(key, cur.Key)) {
                    cur.Value = value;
                    return false;
                }

                prev = cur;
                cur = cur.Next;
            }

            if (prev != null) {
                Debug.Assert(prev.Next == null);
                prev.Next = new Bucket(hc, key, value, null);
            } else {
                buckets[index] = new Bucket(hc, key, value, null);
            }

            return true;
        }

        /// <summary>
        /// Removes an entry from the dictionary and returns true if the
        /// entry was removed or false.
        /// </summary>
        public override bool Remove(object key) {
            object dummy;
            return TryRemoveValue(key, out dummy);
        }

        /// <summary>
        /// Removes an entry from the dictionary and returns true if the
        /// entry was removed or false.  The key will always be hashed
        /// so if it is unhashable an exception will be thrown - even
        /// if the dictionary has no buckets.
        /// </summary>
        internal bool RemoveAlwaysHash(object key) {
            lock (this) {
                object dummy;
                return TryRemoveNoLock(key, out dummy);
            }
        }
        
        public override bool TryRemoveValue(object key, out object value) {
            lock (this) {
                if (!HasAnyValues(_buckets)) {
                    value = null;
                    return false;
                }

                return TryRemoveNoLock(key, out value);
            }
        }

        private bool TryRemoveNoLock(object key, out object value) {
            Func<object, int> hashFunc;
            Func<object, object, bool> eqFunc;
            if (CompilerHelpers.GetType(key) == _keyType || _keyType == HeterogeneousType) {
                hashFunc = _hashFunc;
                eqFunc = _eqFunc;
            } else {
                hashFunc = _genericHash;
                eqFunc = _genericEquals;
            }

            int hc = hashFunc(key) & Int32.MaxValue;
            
            if (_buckets == null) {
                value = null;
                return false;
            }

            int index = hc % _buckets.Length;
            Bucket bucket = _buckets[index];
            Bucket prev = bucket;
            while (bucket != null) {
                if (bucket.HashCode == hc && eqFunc(key, bucket.Key)) {
                    value = bucket.Value;
                    if (prev == bucket) {
                        _buckets[index] = bucket.Next;
                    } else {
                        prev.Next = bucket.Next;
                    }
                    _count--;

                    return true;
                }
                prev = bucket;
                bucket = bucket.Next;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Checks to see if the key exists in the dictionary.
        /// </summary>
        public override bool Contains(object key) {
            return Contains(_buckets, key);
        }

        /// <summary>
        /// Static helper to see if the key exists in the provided bucket array.
        /// 
        /// Used so the contains check can run against a buckets while a writer
        /// replaces the buckets.
        /// </summary>
        private bool Contains(Bucket[] buckets, object key) {
            object res;
            return TryGetValue(buckets, key, out res);
        }

        private bool HasAnyValues(Bucket[] buckets) {
            return buckets != null && _hashFunc != null;
        }

        /// <summary>
        /// Trys to get the value associated with the given key and returns true
        /// if it's found or false if it's not present.
        /// </summary>
        public override bool TryGetValue(object key, out object value) {
            return TryGetValue(_buckets, key, out value);
        }

        /// <summary>
        /// Static helper to try and get the value from the dictionary.
        /// 
        /// Used so the value lookup can run against a buckets while a writer
        /// replaces the buckets.
        /// </summary>
        private bool TryGetValue(Bucket[] buckets, object key, out object value) {
            if (HasAnyValues(buckets)) {
                int hc;
                Func<object, object, bool> eqFunc;
                if (CompilerHelpers.GetType(key) == _keyType || _keyType == HeterogeneousType) {
                    hc = _hashFunc(key) & Int32.MaxValue;
                    eqFunc = _eqFunc;
                } else {
                    hc = _genericHash(key) & Int32.MaxValue;
                    eqFunc = _genericEquals;
                }

                Bucket bucket = buckets[hc % buckets.Length];
                while (bucket != null) {
                    if (bucket.HashCode == hc && eqFunc(key, bucket.Key)) {
                        value = bucket.Value;
                        return true;
                    }
                    bucket = bucket.Next;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the number of key/value pairs currently in the dictionary.
        /// </summary>
        public override int Count {
            get { return _count; }
        }

        /// <summary>
        /// Clears the contents of the dictionary.
        /// </summary>
        public override void Clear() {
            lock (this) {
                if (_buckets != null) {
                    _buckets = new Bucket[8];
                    _count = 0;
                }
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            lock (this) {
                List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>(Count);
                if (_buckets != null) {
                    for (int i = 0; i < _buckets.Length; i++) {
                        Bucket curBucket = _buckets[i];
                        while (curBucket != null) {
                            res.Add(new KeyValuePair<object, object>(curBucket.Key, curBucket.Value));

                            curBucket = curBucket.Next;
                        }
                    }
                }
                return res;
            }
        }

        public override bool HasNonStringAttributes() {
            lock (this) {
                if (_keyType != typeof(string) && _buckets != null) {
                    for (int i = 0; i < _buckets.Length; i++) {
                        Bucket curBucket = _buckets[i];
                        while (curBucket != null) {
                            if (!(curBucket.Key is string)) {
                                return true;
                            }

                            curBucket = curBucket.Next;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Clones the storage returning a new DictionaryStorage object.
        /// </summary>
        public override DictionaryStorage Clone() {
            lock (this) {
                if (_buckets == null) {
                    return new CommonDictionaryStorage();
                }

                Bucket[] resBuckets = new Bucket[_buckets.Length];
                for (int i = 0; i < _buckets.Length; i++) {
                    if (_buckets[i] != null) {
                        resBuckets[i] = _buckets[i].Clone();
                    }
                }

                return new CommonDictionaryStorage(resBuckets, Count, _keyType, _hashFunc, _eqFunc);
            }
        }

        public override void CopyTo(DictionaryStorage/*!*/ into) {
            Debug.Assert(into != null);

            if (_buckets != null) {
                using (new OrderedLocker(this, into)) {
                    CommonDictionaryStorage commonInto = into as CommonDictionaryStorage;
                    if (commonInto != null) {
                        CommonCopyTo(commonInto);
                    } else {
                        UncommonCopyTo(into);
                    }
                }
            }
        }

        private void CommonCopyTo(CommonDictionaryStorage into) {
            if (into._buckets == null) {
                into._buckets = new Bucket[_buckets.Length];
            } else {
                int curSize = into._buckets.Length;
                while (curSize < _count + into._count) {
                    curSize *= ResizeMultiplier;
                }
                into.EnsureSize(curSize);
            }

            if (into._keyType == null) {
                into._keyType = _keyType;
                into._hashFunc = _hashFunc;
                into._eqFunc = _eqFunc;
            } else if (into._keyType != _keyType) {
                SetHeterogeneousSites();
            }

            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    if (AddWorker(into._buckets, curBucket.Key, curBucket.Value, curBucket.HashCode)) {
                        into._count++;
                    }
                    curBucket = curBucket.Next;
                }
            }
        }

        private void UncommonCopyTo(DictionaryStorage into) {
            for (int i = 0; i < _buckets.Length; i++) {
                Bucket curBucket = _buckets[i];
                while (curBucket != null) {
                    into.AddNoLock(curBucket.Key, curBucket.Value);

                    curBucket = curBucket.Next;
                }
            }
        }

        /// <summary>
        /// Helper to hash the given key w/ support for null.
        /// </summary>
        private int Hash(object key) {
            if (key is string) return key.GetHashCode() & Int32.MaxValue;

            return _hashFunc(key) & Int32.MaxValue;
        }

        /// <summary>
        /// Used to store a single hashed key/value and a linked list of
        /// collisions.
        /// 
        /// Bucket is not serializable because it stores the computed hash
        /// code which could change between serialization and deserialization.
        /// </summary>
        private class Bucket {
            public object Key;          // the key to be hashed
            public object Value;        // the value associated with the key
            public Bucket Next;         // the next chained bucket when there's a collision
            public int HashCode;        // the hash code of the contained key.

            public Bucket() {
            }

            public Bucket(int hashCode, object key, object value, Bucket next) {
                HashCode = hashCode;
                Key = key;
                Value = value;
                Next = next;
            }

            public Bucket Clone() {
                return new Bucket(HashCode, Key, Value, CloneNext());
            }

            private Bucket CloneNext() {
                if (Next == null) return null;
                return Next.Clone();
            }
        }

        #region Hash/Equality Delegates

        private static int PrimitiveHash(object o) {
            return o.GetHashCode();
        }

        private static int IntHash(object o) {
            return (int)o;
        }

        private static int DoubleHash(object o) {
            return DoubleOps.__hash__((double)o);
        }

        private static int GenericHash(object o) {
            return PythonOps.Hash(DefaultContext.Default, o);
        }

        private static int TupleHash(object o) {
            return ((IValueEquality)o).GetValueHashCode();
        }

        private static bool StringEquals(object o1, object o2) {
            return (string)o1 == (string)o2;
        }

        private static bool IntEquals(object o1, object o2) {
            return (int)o1 == (int)o2;
        }

        private static bool DoubleEquals(object o1, object o2) {
            return (double)o1 == (double)o2;
        }

        private static bool TupleEquals(object o1, object o2) {
            return ((IValueEquality)o1).ValueEquals(o2);
        }

        private static bool GenericEquals(object o1, object o2) {
            return PythonOps.EqualRetBool(o1, o2);
        }

        #endregion

#if !SILVERLIGHT

        /// <summary>
        /// Special marker bucket used during deserialization to not add
        /// an extra field to the dictionary storage type.
        /// </summary>
        private class DeserializationBucket : Bucket {
            public readonly SerializationInfo/*!*/ SerializationInfo;

            public DeserializationBucket(SerializationInfo info) {
                SerializationInfo = info;
            }
        }

        private DeserializationBucket GetDeserializationBucket() {
            if (_buckets == null) {
                return null;
            }

            if (_buckets.Length != 1) {
                return null;
            }

            return _buckets[0] as DeserializationBucket;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("buckets", GetItems());
        }

        #endregion

        #region IDeserializationCallback Members

        void IDeserializationCallback.OnDeserialization(object sender) {
            DeserializationBucket bucket = GetDeserializationBucket();
            if (bucket == null) {
                // we've received multiple OnDeserialization callbacks, only 
                // deserialize after the 1st one
                return;
            }

            SerializationInfo info = bucket.SerializationInfo;
            _buckets = null;

            var buckets = (List<KeyValuePair<object, object>>)info.GetValue("buckets", typeof(List<KeyValuePair<object, object>>));

            foreach (KeyValuePair<object, object> kvp in buckets) {
                Add(kvp.Key, kvp.Value);
            }
        }

        #endregion
#endif
    }

}
