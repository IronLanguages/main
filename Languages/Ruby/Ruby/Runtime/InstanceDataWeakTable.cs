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

using System;
using System.Collections.Generic;
using System.Text;
using SRC = System.Runtime.CompilerServices;
using System.Reflection;
using System.Diagnostics;

namespace IronRuby.Runtime {

    // thread-unsafe
    internal class WeakTable<TKey, TValue> {
        private Dictionary<object, TValue>/*!*/ _dict;
        private static readonly IEqualityComparer<object> _Comparer = new Comparer();

        #region Comparer

        // WeakComparer treats WeakReference as transparent envelope
        // also, uses reference equality
        sealed class Comparer : IEqualityComparer<object> {
            bool IEqualityComparer<object>.Equals(object x, object y) {
                WeakReference wx = x as WeakReference;
                WeakReference wy = y as WeakReference;

                // Pull the value(s) out of the WeakReference for comparison
                // Empty WeakReference slots are only equal to themselves
                if (wx != null) {
                    x = wx.Target;
                    if (x == null) {
                        return wx == wy;
                    }
                }

                if (wy != null) {
                    y = wy.Target;
                    if (y == null) {
                        return wx == wy;
                    }
                }

                return x == y;
            }

            int IEqualityComparer<object>.GetHashCode(object obj) {
                WeakReference wobj = obj as WeakReference;
                if (wobj != null) {
                    obj = wobj.Target;
                    if (obj == null) {
                        // empty WeakReference slots are only equal to themselves
                        return wobj.GetHashCode();
                    }
                }

                return SRC.RuntimeHelpers.GetHashCode(obj);
            }
        }
        #endregion

        #region weak hashtable cleanup

        int _version, _cleanupVersion;

#if SILVERLIGHT // GC
        WeakReference _cleanupGC = new WeakReference(new object());
#else
        int _cleanupGC = 0;
#endif

        bool GarbageCollected() {
            // Determine if a GC has happened

            // WeakReferences can become zero only during the GC.
            bool garbage_collected;
#if SILVERLIGHT // GC.CollectionCount
            garbage_collected = !_cleanupGC.IsAlive;
            if (garbage_collected) _cleanupGC = new WeakReference(new object());
#else
            int currentGC = GC.CollectionCount(0);
            garbage_collected = currentGC != _cleanupGC;
            if (garbage_collected) _cleanupGC = currentGC;
#endif
            return garbage_collected;
        }

        void CheckCleanup() {
            _version++;

            long change = _version - _cleanupVersion;

            // Cleanup the table if it is a while since we have done it last time.
            // Take the size of the table into account.
            if (change > 1234 + _dict.Count / 2) {
                // It makes sense to do the cleanup only if a GC has happened in the meantime.
                if (GarbageCollected()) {
                    Cleanup();
                    _cleanupVersion = _version;
                } else {
                    _cleanupVersion += 1234;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")] // TODO
        void Cleanup() {
            int liveCount = 0;
            int emptyCount = 0;

            foreach (WeakReference w in _dict.Keys) {
                if (w.Target != null) {
                    liveCount++;
                } else {
                    emptyCount++;
                }
            }

            // Rehash the table if there is a significant number of empty slots
            if (emptyCount > liveCount / 4) {
                Dictionary<object, TValue> newtable = new Dictionary<object, TValue>(liveCount + liveCount / 4, _Comparer);

                foreach (WeakReference w in _dict.Keys) {
                    object target = w.Target;

                    if (target != null) {
                        newtable[w] = _dict[w];
                        GC.KeepAlive(target);
                    }
                }

                _dict = newtable;
            }
        }
        #endregion

        public WeakTable() {
            _dict = new Dictionary<object, TValue>(_Comparer);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return _dict.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value) {
            Cleanup();
            // _dict might be a new Dictionary after Cleanup(),
            // so use the field directly
            _dict.Add(new WeakReference(key, true), value);
        }
    }

#if DEV10

    // thread-safe
    internal class InstanceDataWeakTable {

        private delegate bool TryGetValueDelegate(object key, out RubyInstanceData value);
        private delegate void AddDelegate(object key, RubyInstanceData value);

        private static readonly MethodInfo _AddMethod, _TryGetValueMethod;
        private static readonly Type _TableType;

        private readonly object/*!*/ _dict;
        private readonly TryGetValueDelegate/*!*/ _tryGetValue;
        private readonly AddDelegate/*!*/ _add;

        static InstanceDataWeakTable() {
#if !SILVERLIGHT
            _TableType = typeof(object).Assembly.GetType("System.Runtime.CompilerServices.ConditionalWeakTable`2", false, false);
            if (_TableType != null) {
                _TableType = _TableType.MakeGenericType(typeof(object), typeof(RubyInstanceData));
            } else
#endif      
            _TableType = typeof(WeakTable<object, RubyInstanceData>);

            Utils.Log(_TableType.FullName, "WEAK_TABLE");

            _TryGetValueMethod = _TableType.GetMethod("TryGetValue", new Type[] { typeof(object), typeof(RubyInstanceData).MakeByRefType() });
            _AddMethod = _TableType.GetMethod("Add", new Type[] { typeof(object), typeof(RubyInstanceData) });
        }

        public InstanceDataWeakTable() {
            _dict = Activator.CreateInstance(_TableType);
            _tryGetValue = (TryGetValueDelegate)Delegate.CreateDelegate(typeof(TryGetValueDelegate), _dict, _TryGetValueMethod);
            _add = (AddDelegate)Delegate.CreateDelegate(typeof(AddDelegate), _dict, _AddMethod);
        }

        public bool TryGetValue(object key, out RubyInstanceData value) {
            lock (_dict) {
                return _tryGetValue(key, out value);
            }
        }

        public RubyInstanceData/*!*/ GetValue(object key) {
            lock (_dict) {
                RubyInstanceData value;
                if (_tryGetValue(key, out value)) {
                    return value;
                }

                value = new RubyInstanceData();
                _add(key, value);
                return value;
            }
        }
    }
#endif
}