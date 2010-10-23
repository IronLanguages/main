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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Similar to Dictionary[TKey,TValue], but it also ensures that the keys will not be kept alive
    /// if the only reference is from this collection. The value will be kept alive as long as the key
    /// is alive.
    /// 
    /// This currently has a limitation that the caller is responsible for ensuring that an object used as 
    /// a key is not also used as a value in *any* instance of a WeakHash. Otherwise, it will result in the
    /// object being kept alive forever. This effectively means that the owner of the WeakHash should be the
    /// only one who has access to the object used as a value.
    /// 
    /// Currently, there is also no guarantee of how long the values will be kept alive even after the keys
    /// get collected. This could be fixed by triggerring CheckCleanup() to be called on every garbage-collection
    /// by having a dummy watch-dog object with a finalizer which calls CheckCleanup().
    /// </summary>
    public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        // The one and only comparer instance.
        static readonly IEqualityComparer<object> comparer = new WeakComparer<object>();

        IDictionary<object, TValue> dict = new Dictionary<object, TValue>(comparer);
        int version, cleanupVersion;

#if SILVERLIGHT // GC
        WeakReference cleanupGC = new WeakReference(new object());
#else
        int cleanupGC = 0;
#endif

        public WeakDictionary() {
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) {
            CheckCleanup();

            // If the WeakHash already holds this value as a key, it will lead to a circular-reference and result
            // in the objects being kept alive forever. The caller needs to ensure that this cannot happen.
            Debug.Assert(!dict.ContainsKey(value));

            dict.Add(new WeakObject(key), value);
        }

        [Confined]
        public bool ContainsKey(TKey key) {
            // We dont have to worry about creating "new WeakObject(key)" since the comparer
            // can compare raw objects with WeakObject.
            return dict.ContainsKey(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public ICollection<TKey> Keys {
            get {
                // TODO:
                throw new NotImplementedException();
            }
        }

        public bool Remove(TKey key) {
            return dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return dict.TryGetValue(key, out value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public ICollection<TValue> Values {
            get {
                // TODO:
                throw new NotImplementedException();
            }
        }

        public TValue this[TKey key] {
            get {
                return dict[key];
            }
            set {
                // If the WeakHash already holds this value as a key, it will lead to a circular-reference and result
                // in the objects being kept alive forever. The caller needs to ensure that this cannot happen.
                Debug.Assert(!dict.ContainsKey(value));

                CheckCleanup();

                dict[new WeakObject(key)] = value;
            }
        }

        /// <summary>
        /// Check if any of the keys have gotten collected
        /// 
        /// Currently, there is also no guarantee of how long the values will be kept alive even after the keys
        /// get collected. This could be fixed by triggerring CheckCleanup() to be called on every garbage-collection
        /// by having a dummy watch-dog object with a finalizer which calls CheckCleanup().
        /// </summary>
        void CheckCleanup() {
            version++;

            long change = version - cleanupVersion;

            // Cleanup the table if it is a while since we have done it last time.
            // Take the size of the table into account.
            if (change > 1234 + dict.Count / 2) {
                // It makes sense to do the cleanup only if a GC has happened in the meantime.
                // WeakReferences can become zero only during the GC.

                bool garbage_collected;
#if SILVERLIGHT // GC.CollectionCount
                garbage_collected = !cleanupGC.IsAlive;
                if (garbage_collected) cleanupGC = new WeakReference(new object());
#else
                int currentGC = GC.CollectionCount(0);
                garbage_collected = currentGC != cleanupGC;
                if (garbage_collected) cleanupGC = currentGC;
#endif
                if (garbage_collected) {
                    Cleanup();
                    cleanupVersion = version;
                } else {
                    cleanupVersion += 1234;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
        private void Cleanup() {

            int liveCount = 0;
            int emptyCount = 0;

            foreach (WeakObject w in dict.Keys) {
                if (w.Target != null)
                    liveCount++;
                else
                    emptyCount++;
            }

            // Rehash the table if there is a significant number of empty slots
            if (emptyCount > liveCount / 4) {
                Dictionary<object, TValue> newtable = new Dictionary<object, TValue>(liveCount + liveCount / 4, comparer);

                foreach (WeakObject w in dict.Keys) {
                    object target = w.Target;

                    if (target != null)
                        newtable[w] = dict[w];

                    GC.KeepAlive(target);
                }

                dict = newtable;
            }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item) {
            // TODO:
            throw new NotImplementedException();
        }

        public void Clear() {
            // TODO:
            throw new NotImplementedException();
        }

        [Confined]
        public bool Contains(KeyValuePair<TKey, TValue> item) {
            // TODO:
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            // TODO:
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public int Count {
            get {
                // TODO:
                throw new NotImplementedException();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public bool IsReadOnly {
            get {
                // TODO:
                throw new NotImplementedException();
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            // TODO:
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        [Pure]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            // TODO:
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        [Pure]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            // TODO:
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class WeakObject {
        WeakReference weakReference;
        int hashCode;

        public WeakObject(object obj) {
            weakReference = new WeakReference(obj, true);
            hashCode = (obj == null) ? 0 : RuntimeHelpers.GetHashCode(obj);
        }

        public object Target {
            get {
                return weakReference.Target;
            }
        }

        [Confined]
        public override int GetHashCode() {
            return hashCode;
        }

        [Confined]
        public override bool Equals(object obj) {
            object target = weakReference.Target;
            if (target == null) {
                return false;
            }

            return target.Equals(obj);
        }
    }

    // WeakComparer treats WeakObject as transparent envelope
    sealed class WeakComparer<T> : IEqualityComparer<T> {
        bool IEqualityComparer<T>.Equals(T x, T y) {
            WeakObject wx = x as WeakObject;
            if (wx != null)
                x = (T)wx.Target;

            WeakObject wy = y as WeakObject;
            if (wy != null)
                y = (T)wy.Target;

            return Object.Equals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj) {
            WeakObject wobj = obj as WeakObject;
            if (wobj != null)
                return wobj.GetHashCode();

            return (obj == null) ? 0 : RuntimeHelpers.GetHashCode(obj);
        }
    }

    public sealed class HybridMapping<T> {
        private Dictionary<int, object> _dict = new Dictionary<int, object>();
        private readonly Object _synchObject = new Object();
        private readonly int _offset;
        private int _current;

        private const int SIZE = 4096;
        private const int MIN_RANGE = SIZE / 2;

        public HybridMapping()
            : this(0) {
        }

        public HybridMapping(int offset) {
            if (offset < 0 || (SIZE - offset) < MIN_RANGE) {
                throw new InvalidOperationException("HybridMapping is full");
            }
            _offset = offset;
            _current = offset;
        }

        private void NextKey() {
            if (++_current >= SIZE) {
                _current = _offset;
            }
        }

        public int WeakAdd(T value) {
            lock (_synchObject) {
                int saved = _current;
                while (_dict.ContainsKey(_current)) {
                    NextKey();
                    if (_current == saved)
                        throw new InvalidOperationException("HybridMapping is full");
                }
                _dict.Add(_current, new WeakObject(value));
                return _current;
            }
        }

        public int StrongAdd(T value) {
            lock (_synchObject) {
                int saved = _current;
                while (_dict.ContainsKey(_current)) {
                    NextKey();
                    if (_current == saved)
                        throw new InvalidOperationException("HybridMapping is full");
                }
                _dict.Add(_current, value);
                return _current;
            }
        }

        public T GetObjectFromId(int id) {
            object ret;
            if (_dict.TryGetValue(id, out ret)) {
                WeakObject weakObj = ret as WeakObject;
                if (weakObj != null) {
                    return (T)weakObj.Target;
                }
                if (ret is T) {
                    return (T)ret;
                }

                throw new InvalidOperationException("Unexpected dictionary content: type " + ret.GetType());
            } else
                return default(T);
        }

        public int GetIdFromObject(T value) {
            lock (_synchObject) {
                foreach (KeyValuePair<int, object> kv in _dict) {
                    if (kv.Value is WeakObject) {
                        object target = ((WeakObject)kv.Value).Target;
                        if (target != null && target.Equals(value))
                            return kv.Key;
                    } else if (kv.Value is T) {
                        object target = (T)(kv.Value);
                        if (target.Equals(value))
                            return kv.Key;
                    }
                }
            }
            return -1;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")] // TODO: fix (rename?)
        public void RemoveOnId(int id) {
            lock (_synchObject) {
                _dict.Remove(id);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")] // TODO: fix (rename?)
        public void RemoveOnObject(T value) {
            try {
                int id = GetIdFromObject(value);
                RemoveOnId(id);
            } catch { }
        }
    }
}
