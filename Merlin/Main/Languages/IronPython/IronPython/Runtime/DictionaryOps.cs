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
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Text;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    /// <summary>
    /// Provides both helpers for implementing Python dictionaries as well
    /// as providing public methods that should be exposed on all dictionary types.
    /// 
    /// Currently these are published on IDictionary&lt;object, object&gt;
    /// </summary>
    public static class DictionaryOps {
        #region Dictionary Public API Surface

        [SpecialName]
        public static bool __contains__(IDictionary<object, object> self, object value) {
            return self.ContainsKey(value);
        }

        [SpecialName]
        [return: MaybeNotImplemented]
        public static object __cmp__(CodeContext/*!*/ context, IDictionary<object, object> self, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if (!PythonOps.TryGetBoundAttr(DefaultContext.Default, other, Symbols.Length, out len) ||
                    !PythonOps.TryGetBoundAttr(DefaultContext.Default, other, SymbolTable.StringToId("iteritems"), out iteritems)) {
                    return NotImplementedType.Value;
                }

                // user-defined dictionary...
                int lcnt = self.Count;
                int rcnt = Converter.ConvertToInt32(PythonOps.CallWithContext(DefaultContext.Default, len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

                return DictionaryOps.CompareToWorker(context, self, new List(PythonOps.CallWithContext(context, iteritems)));
            }

            CompareUtil.Push(self, oth);
            try {
                return DictionaryOps.CompareTo(context, self, oth);
            } finally {
                CompareUtil.Pop(self, oth);
            }
        }

        // Dictionary has an odd not-implemented check to support custom dictionaries and therefore
        // needs a custom __eq__ / __ne__ implementation.

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Equal(IDictionary<object, object> self, object other) {
            if (!(other is PythonDictionary || other is IDictionary<object, object>))
                return NotImplementedType.Value;

            return EqualsHelper(self, other);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object GreaterThanOrEqual(CodeContext/*!*/ context, IDictionary<object, object> self, object other) {
            object res = __cmp__(context, self, other);
            if (res == NotImplementedType.Value) return res;

            return ((int)res) >= 0;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object GreaterThan(CodeContext/*!*/ context, IDictionary<object, object> self, object other) {
            object res = __cmp__(context, self, other);
            if (res == NotImplementedType.Value) return res;

            return ((int)res) > 0;
        }

        [SpecialName]
        public static void __delitem__(IDictionary<object, object> self, object key) {
            if (!self.Remove(key)) {
                throw PythonOps.KeyError(key);
            }
        }

        public static IEnumerator __iter__(PythonDictionary self) {
            return new DictionaryKeyEnumerator(self._storage);
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LessThanOrEqual(CodeContext/*!*/ context, IDictionary<object, object> self, object other) {
            object res = __cmp__(context, self, other);
            if (res == NotImplementedType.Value) return res;

            return ((int)res) <= 0;
        }

        [SpecialName]
        public static int __len__(IDictionary<object, object> self) {
            return self.Count;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object LessThan(CodeContext/*!*/ context, IDictionary<object, object> self, object other) {
            object res = __cmp__(context, self, other);
            if (res == NotImplementedType.Value) return res;

            return ((int)res) < 0;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object NotEqual(IDictionary<object, object> self, object other) {
            object res = Equal(self, other);
            if (res != NotImplementedType.Value) return PythonOps.Not(res);

            return res;
        }

        public static string/*!*/ __repr__(CodeContext/*!*/ context, IDictionary<object, object> self) {
            List<object> infinite = PythonOps.GetAndCheckInfinite(self);
            if (infinite == null) {
                return "{...}";
            }

            int index = infinite.Count;
            infinite.Add(self);
            try {
                StringBuilder buf = new StringBuilder();
                buf.Append("{");
                bool first = true;
                foreach (KeyValuePair<object, object> kv in self) {
                    if (first) first = false;
                    else buf.Append(", ");

                    if (BaseSymbolDictionary.IsNullObject(kv.Key))
                        buf.Append("None");
                    else
                        buf.Append(PythonOps.Repr(context, kv.Key));
                    buf.Append(": ");

                    buf.Append(PythonOps.Repr(context, kv.Value));
                }
                buf.Append("}");
                return buf.ToString();
            } finally {
                System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                infinite.RemoveAt(index);
            }
        }

        public static void clear(IDictionary<object, object> self) {
            self.Clear();
        }

        public static object copy(CodeContext/*!*/ context, IDictionary<object, object> self) {
            return new PythonDictionary(context, new Dictionary<object, object>(self));
        }

        public static object get(IDictionary<object, object> self, object key) {
            return get(self, key, null);
        }

        public static object get(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            return defaultValue;
        }

        public static bool has_key(IDictionary<object, object> self, object key) {
            return self.ContainsKey(key);
        }

        public static List items(IDictionary<object, object> self) {
            List ret = PythonOps.MakeEmptyList(self.Count);
            foreach (KeyValuePair<object, object> kv in self) {
                ret.AddNoLock(PythonTuple.MakeTuple(kv.Key, kv.Value));
            }
            return ret;
        }

        public static IEnumerator iteritems(IDictionary<object, object> self) {
            return ((IEnumerable)items(self)).GetEnumerator();
        }

        public static IEnumerator iterkeys(IDictionary<object, object> self) {
            return ((IEnumerable)keys(self)).GetEnumerator();
        }

        public static IEnumerator itervalues(IDictionary<object, object> self) {
            return ((IEnumerable)values(self)).GetEnumerator();
        }

        public static List keys(IDictionary<object, object> self) {
            return PythonOps.MakeListFromSequence(self.Keys);
        }

        public static object pop(IDictionary<object, object> self, object key) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                throw PythonOps.KeyError(key);
            }
        }

        public static object pop(IDictionary<object, object> self, object key, object defaultValue) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                return defaultValue;
            }
        }

        public static PythonTuple popitem(IDictionary<object, object> self) {
            IEnumerator<KeyValuePair<object, object>> ie = self.GetEnumerator();
            if (ie.MoveNext()) {
                object key = ie.Current.Key;
                object val = ie.Current.Value;
                self.Remove(key);
                return PythonTuple.MakeTuple(key, val);
            }
            throw PythonOps.KeyError("dictionary is empty");
        }

        public static object setdefault(IDictionary<object, object> self, object key) {
            return setdefault(self, key, null);
        }

        public static object setdefault(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            self[key] = defaultValue;
            return defaultValue;
        }

        public static List values(IDictionary<object, object> self) {
            return PythonOps.MakeListFromSequence(self.Values);
        }

        public static void update(CodeContext/*!*/ context, PythonDictionary/*!*/ self, object b) {
            PythonDictionary pyDict;

            if ((pyDict = b as PythonDictionary) != null) {
                pyDict._storage.CopyTo(self._storage);
            } else {
                SlowUpdate(context, self, b);
            }
        }

        private static void SlowUpdate(CodeContext/*!*/ context, PythonDictionary/*!*/ self, object b) {
            object keysFunc;
            DictProxy dictProxy;
            IDictionary dict;
            if ((dictProxy = b as DictProxy) != null) {
                update(context, self, dictProxy.Type.GetMemberDictionary(context, false));
            } else if ((dict = b as IDictionary) != null) {
                IDictionaryEnumerator e = dict.GetEnumerator();
                while (e.MoveNext()) {
                    self._storage.Add(e.Key, e.Value);
                }
            } else if (PythonOps.TryGetBoundAttr(b, Symbols.Keys, out keysFunc)) {
                // user defined dictionary
                IEnumerator i = PythonOps.GetEnumerator(PythonCalls.Call(context, keysFunc));
                while (i.MoveNext()) {
                    self._storage.Add(i.Current, PythonOps.GetIndex(context, b, i.Current));
                }
            } else {
                // list of lists (key/value pairs), list of tuples,
                // tuple of tuples, etc...
                IEnumerator i = PythonOps.GetEnumerator(b);
                int index = 0;
                while (i.MoveNext()) {
                    if (!AddKeyValue(self, i.Current)) {
                        throw PythonOps.ValueError("dictionary update sequence element #{0} has bad length; 2 is required", index);
                    }
                    index++;
                }
            }
        }

        #endregion

        #region Dictionary Helper APIs

        internal static bool TryGetValueVirtual(CodeContext context, PythonDictionary self, object key, ref object DefaultGetItem, out object value) {
            IPythonObject sdo = self as IPythonObject;
            if (sdo != null) {
                Debug.Assert(sdo != null);
                PythonType myType = sdo.PythonType;
                object ret;
                PythonTypeSlot dts;

                if (DefaultGetItem == null) {
                    // lazy init our cached DefaultGetItem
                    TypeCache.Dict.TryLookupSlot(context, Symbols.GetItem, out dts);
                    bool res = dts.TryGetValue(context, self, TypeCache.Dict, out DefaultGetItem);
                    Debug.Assert(res);
                }

                // check and see if it's overridden
                if (myType.TryLookupSlot(context, Symbols.GetItem, out dts)) {
                    dts.TryGetValue(context, self, myType, out ret);

                    if (ret != DefaultGetItem) {
                        // subtype of dict that has overridden __getitem__
                        // we need to call the user's versions, and handle
                        // any exceptions.
                        try {
                            value = self[key];
                            return true;
                        } catch (KeyNotFoundException) {
                            value = null;
                            return false;
                        }
                    }
                }
            }

            value = null;
            return false;
        }

        internal static bool AddKeyValue(PythonDictionary self, object o) {
            IEnumerator i = PythonOps.GetEnumerator(o); //c.GetEnumerator();
            if (i.MoveNext()) {
                object key = i.Current;
                if (i.MoveNext()) {
                    object value = i.Current;
                    self._storage.Add(key, value);

                    return !i.MoveNext();
                }
            }
            return false;
        }
       
        internal static int CompareTo(CodeContext/*!*/ context, IDictionary<object, object> left, IDictionary<object, object> right) {
            int lcnt = left.Count;
            int rcnt = right.Count;

            if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

            List ritems = DictionaryOps.items(right);
            return CompareToWorker(context, left, ritems);
        }

        internal static int CompareToWorker(CodeContext/*!*/ context, IDictionary<object, object> left, List ritems) {
            List litems = DictionaryOps.items(left);

            litems.sort(context);
            ritems.sort(context);

            return litems.CompareToWorker(ritems);
        }

        internal static bool EqualsHelper(IDictionary<object, object> self, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;

            if (oth.Count != self.Count) return false;

            // we cannot call Compare here and compare against zero because Python defines
            // value equality as working even if the keys/values are unordered.
            List myKeys = keys(self);

            foreach (object o in myKeys) {
                object res;
                if (!oth.TryGetValue(o, out res)) return false;

                CompareUtil.Push(res);
                try {
                    if (!PythonOps.EqualRetBool(res, self[o])) return false;
                } finally {
                    CompareUtil.Pop(res);
                }
            }
            return true;
        }
        #endregion
    }
}
