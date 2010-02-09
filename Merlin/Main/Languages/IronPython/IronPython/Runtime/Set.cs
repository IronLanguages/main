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
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    /// <summary>
    /// Common interface shared by both Set and FrozenSet
    /// </summary>
    public interface ISet : IEnumerable, IEnumerable<object>, ICollection, IStructuralEquatable, ICodeFormattable
#if CLR2
        , IValueEquality
#endif
    {
        int __len__();
        bool __contains__(object value);
        PythonTuple __reduce__();

        // private methods used for operations between set types.
        void PrivAdd(object adding);
        void PrivRemove(object removing);
        void SetData(IEnumerable set);

        #region NonOperator Operations

        bool isdisjoint(object s); 
        bool issubset(object set);
        bool issuperset(object set);

        ISet union();
        ISet union(object s);
        ISet union([NotNull] params object[] ss);

        ISet intersection();
        ISet intersection(object s);
        ISet intersection([NotNull] params object[] ss);

        ISet difference();
        ISet difference(object s);
        ISet difference([NotNull] params object[] ss);

        ISet symmetric_difference(object s);

        #endregion
    }

    /// <summary>
    /// Contains common set functionality between set and frozenSet
    /// </summary>
    static class SetHelpers {
        public static string SetToString(CodeContext/*!*/ context, object set, CommonDictionaryStorage items) {
            string setTypeStr;
            Type setType = set.GetType();
            if (setType == typeof(SetCollection)) {
                setTypeStr = "set";
            } else if (setType == typeof(FrozenSetCollection)) {
                setTypeStr = "frozenset";
            } else {
                setTypeStr = PythonTypeOps.GetName(set);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(setTypeStr);
            sb.Append("([");
            string comma = "";
            foreach (object o in items.GetKeys()) {
                sb.Append(comma);
                sb.Append(PythonOps.Repr(context, o));
                comma = ", ";
            }
            sb.Append("])");

            return sb.ToString();
        }

        /// <summary>
        /// Creates a set that can be hashable.  If the set is currently a FrozenSet the
        /// set is returned.  If the set is a normal Set then a FrozenSet is returned
        /// with its contents.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object GetHashableSetIfSet(object o) {
            SetCollection asSet = o as SetCollection;
            if (asSet != null) {
                if (asSet.GetType() != typeof(SetCollection)) {
                    // subclass of set, need to check if it is hashable
                    if (IsHashable(asSet)) {
                        return o;
                    }
                }
                return FrozenSetCollection.Make(((IEnumerable)asSet).GetEnumerator());
            }
            return o;
        }

        private static bool IsHashable(SetCollection asSet) {
            PythonTypeSlot pts;
            PythonType pt = DynamicHelpers.GetPythonType(asSet);
            object slotValue;

            return pt.TryResolveSlot(DefaultContext.Default, "__hash__", out pts) &&
                   pts.TryGetValue(DefaultContext.Default, asSet, pt, out slotValue) && slotValue != null;
        }

        public static ISet MakeSet(object setObj) {
            Type t = setObj.GetType();
            if (t == typeof(SetCollection)) {
                return new SetCollection();
            } else if (t == typeof(FrozenSetCollection)) {
                return new FrozenSetCollection();
            } else {
                // subclass                
                PythonType dt = DynamicHelpers.GetPythonType(setObj);

                ISet set = PythonCalls.Call(dt) as ISet;
                Debug.Assert(set != null);

                return set;
            }
        }

        public static ISet MakeSet(object setObj, ISet set) {
            Type t = setObj.GetType();
            if (t == typeof(SetCollection)) {
                return new SetCollection(set);
            } else if (t == typeof(FrozenSetCollection)) {
                return new FrozenSetCollection(set);
            } else {
                // subclass                
                PythonType dt = DynamicHelpers.GetPythonType(setObj);

                ISet res = PythonCalls.Call(dt) as ISet;

                Debug.Assert(res != null);
                res.SetData(set);
                return res;
            }
        }

        public static ISet Intersection(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x);

            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (x.__contains__(ie.Current))
                    res.PrivAdd(ie.Current);
            }
            return res;
        }

        public static ISet Difference(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x, x);

            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (res.__contains__(ie.Current)) {
                    res.PrivRemove(ie.Current);
                }
            }
            return res;
        }

        public static ISet SymmetricDifference(ISet x, object y) {
            SetCollection otherSet = new SetCollection(PythonOps.GetEnumerator(y));       //make a set to deal w/ dups in the enumerator
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            foreach (object o in otherSet) {
                if (res.__contains__(o)) {
                    res.PrivRemove(o);
                } else {
                    res.PrivAdd(o);
                }
            }
            return res;
        }

        public static ISet Union(ISet x, object y) {
            ISet set = SetHelpers.MakeSet(x, x);
            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                set.PrivAdd(ie.Current);
            }
            return set;
        }

        public static bool IsSubset(ISet x, object y) {
            ISet set = y as ISet;
            if (set == null) {
                set = new SetCollection(PythonOps.GetEnumerator(y));
            }

            return IsSubset(x, set, false);
        }

        public static bool IsSuperset(ISet x, object y) {
            ISet set = y as ISet;
            if (set == null) {
                set = new SetCollection(PythonOps.GetEnumerator(y));
            }

            return IsSubset(set, x, false);
        }

        public static bool IsSubset(ISet x, ISet y, bool strict) {
            if (x.Count > y.Count || strict && x.Count == y.Count) {
                return false;
            }
            foreach (object o in x) {
                if (!y.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static PythonTuple Reduce(CommonDictionaryStorage items, PythonType type) {
            object[] keys = new object[items.Count];
            int i = 0;
            foreach (object key in items.GetKeys()) {
                keys[i++] = key;
            }
            return PythonTuple.MakeTuple(type, PythonTuple.MakeTuple(List.FromArrayNoCopy(keys)), null);
        }

        public static bool Equals(ISet x, ISet y, IEqualityComparer comparer) {
            if (x.Count != y.Count) {
                return false;
            }

            // optimization when we know the behavior of the comparer
            if (comparer is PythonContext.PythonEqualityComparer) {
                foreach (object o in x) {
                    if (!y.__contains__(o)) {
                        return false;
                    }
                }
                return true;
            }

            // slower comparison using comparer
            List yItems = new List(y.GetEnumerator());
            foreach (object o in x) {
                bool found = false;
                for (int i = 0; i < yItems.Count; i++) {
                    if (comparer.Equals(o, yItems[i])) {
                        found = true;
                        yItems.RemoveAt(i);
                        break;
                    }
                }
                if (!found) {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Mutable set class
    /// </summary>
    [PythonType("set"), DebuggerDisplay("set, {Count} items", TargetTypeName = "set"), DebuggerTypeProxy(typeof(CollectionDebugProxy))]
    public class SetCollection : ISet {
        private CommonDictionaryStorage _items;

        #region Set contruction

        public void __init__() {
            clear();
        }

        public void __init__(object setData) {
            CommonDictionaryStorage newStorage = new CommonDictionaryStorage();

            IEnumerator ie = PythonOps.GetEnumerator(setData);
            while (ie.MoveNext()) {
                object current = ie.Current;
                newStorage.AddNoLock(current, current);
            }
            _items = newStorage;
        }

        public static object __new__(CodeContext/*!*/ context, PythonType cls) {
            if (cls == TypeCache.Set) {
                return new SetCollection();
            }

            return cls.CreateInstance(context);
        }

        public static object __new__(CodeContext/*!*/ context, PythonType cls, object arg) {
            return __new__(context, cls);
        }

        public static object __new__(CodeContext/*!*/ context, PythonType cls, params object[] args\u00F8) {
            return __new__(context, cls);
        }

        public static object __new__(CodeContext/*!*/ context, PythonType cls, [ParamDictionary]IDictionary<object, object> kwArgs, params object[] args\u00F8) {
            return __new__(context, cls);
        }

        public SetCollection() {
            _items = new CommonDictionaryStorage();
        }

        internal SetCollection(object setData) {
            Init(setData);
        }

        internal SetCollection(IEnumerator setData) {
            _items = new CommonDictionaryStorage();
            while (setData.MoveNext()) {
                add(setData.Current);
            }
        }

        private void Init(params object[] o) {
            if (o.Length > 1) {
                throw PythonOps.TypeError("set expected at most 1 arguments, got {0}", o.Length);
            }

            _items = new CommonDictionaryStorage();
            if (o.Length != 0) {
                IEnumerator setData = PythonOps.GetEnumerator(o[0]);
                while (setData.MoveNext()) {
                    add(setData.Current);
                }
            }
        }

        public SetCollection copy() {
            return new SetCollection(((IEnumerable)this).GetEnumerator());
        }

        #endregion

        #region ISet

        public int __len__() {
            return Count;
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSets for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            if (_items.Count == 0) {
                PythonOps.Hash(DefaultContext.Default, value);    // make sure we have a hashable item
            }
            return _items.Contains(value);
        }

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(_items, DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection)));
        }

        void ISet.PrivAdd(object adding) {
            add(adding);
        }

        void ISet.PrivRemove(object removing) {
            remove(removing);
        }

        void ISet.SetData(IEnumerable set) {
            _items = new CommonDictionaryStorage();
            foreach (object o in set) {
                _items.Add(o, o);
            }
        }

        #endregion

        #region NonOperator Operations

        public bool isdisjoint(object s) {
            return SetHelpers.Intersection(this, s).Count == 0;
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(object set) {
            return SetHelpers.IsSuperset(this, set);
        }

        public ISet union() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet union(object s) {
            return SetHelpers.Union(this, s);
        }

        public ISet union([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Union(res, s);
            }
            return res;
        }

        public ISet intersection() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet intersection(object s) {
            return SetHelpers.Intersection(this, s);
        }

        public ISet intersection([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Intersection(res, s);
            }
            return res;
        }

        public ISet difference() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public ISet difference([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Difference(res, s);
            }
            return res;
        }

        public ISet symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        #endregion

        #region Mutating Members

        /// <summary>
        /// Appends one IEnumerable to an existing set
        /// </summary>
        /// <param name="s"></param>
        public void update(object s) {
            if (Object.ReferenceEquals(s, this)) {
                return;
            }

            IEnumerator ie = PythonOps.GetEnumerator(s);
            while (ie.MoveNext()) {
                add(ie.Current);
            }
        }

        /// <summary>
        /// Appends one or more IEnumerables to an existing set
        /// </summary>
        public void update([NotNull] params object[] ss) {
            foreach (object s in ss) {
                update(s);
            }
        }

        public void add(object o) {
            _items.Add(o, o);
        }

        public void intersection_update(object s) {
            SetCollection set = intersection(s) as SetCollection;
            _items = set._items;
        }

        public void intersection_update([NotNull] params object[] ss) {
            foreach (object s in ss) {
                intersection_update(s);
            }
        }

        public void difference_update(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (__contains__(o)) {
                    remove(o);
                }
            }
        }

        public void difference_update([NotNull] params object[] ss) {
            foreach (object s in ss) {
                difference_update(s);
            }
        }

        public void symmetric_difference_update(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (__contains__(o)) {
                    remove(o);
                } else {
                    add(o);
                }
            }
        }

        public void remove([NotNull]SetCollection o) {
            var set = SetHelpers.GetHashableSetIfSet(o);
            if (!_items.RemoveAlwaysHash(set)) {
                throw PythonOps.KeyError(o);
            }
        }

        public void remove(object o) {
            if (!_items.RemoveAlwaysHash(o)) {
                throw PythonOps.KeyError(o);
            }
        }

        public void discard(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            _items.Remove(o);
        }

        public object pop() {
            foreach (object o in _items.GetKeys()) {
                _items.Remove(o);
                return o;
            }
            throw PythonOps.KeyError("pop from an empty set");
        }

        public void clear() {
            _items.Clear();
        }

        #endregion

        #region Operators

        [SpecialName]
        public SetCollection InPlaceBitwiseAnd(object s) {
            ISet set = s as ISet;
            if (set == null) {
                throw PythonOps.TypeError("unsupported operand type(s) for &=: '{0}' and '{1}'", PythonTypeOps.GetName(s), PythonTypeOps.GetName(this));
            }

            intersection_update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceBitwiseOr(object s) {
            ISet set = s as ISet;
            if (set == null) {
                throw PythonOps.TypeError("unsupported operand type(s) for |=: '{0}' and '{1}'", PythonTypeOps.GetName(s), PythonTypeOps.GetName(this));
            }

            update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceSubtract(object s) {
            ISet set = s as ISet;
            if (set == null) {
                throw PythonOps.TypeError("unsupported operand type(s) for -=: '{0}' and '{1}'", PythonTypeOps.GetName(s), PythonTypeOps.GetName(this));
            }

            difference_update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceExclusiveOr(object s) {
            ISet set = s as ISet;
            if (set == null) {
                throw PythonOps.TypeError("unsupported operand type(s) for ^=: '{0}' and '{1}'", PythonTypeOps.GetName(s), PythonTypeOps.GetName(this));
            }

            symmetric_difference_update(set);
            return this;
        }

        public static object operator &(ISet y, SetCollection x) {
            return y.intersection(x);
        }

        public static object operator |(ISet y, SetCollection x) {
            return y.union(x);
        }

        public static object operator ^(ISet y, SetCollection x) {
            return y.symmetric_difference(x);
        }

        public static object operator -(ISet y, SetCollection x) {
            return y.difference(x);
        }

        public static ISet operator &(SetCollection x, ISet y) {
            return x.intersection(y);
        }

        public static ISet operator |(SetCollection x, ISet y) {
            return x.union(y);
        }

        public static ISet operator ^(SetCollection x, ISet y) {
            return x.symmetric_difference(y);
        }

        public static ISet operator -(SetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IValueEquality Members
#if CLR2
        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("set objects are unhashable");
        }

        bool IValueEquality.ValueEquals(object o) {
            return __eq__(o);
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
            CompareUtil.Push(this);
            try {
                res = ((IStructuralEquatable)new FrozenSetCollection(this)).GetHashCode(comparer);
            } finally {
                CompareUtil.Pop(this);
            }

            return res;
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
            ISet set = other as ISet;
            if (set != null) {
                return SetHelpers.Equals(this, set, comparer);
            }
            return false;
        }

        // default conversion of protocol methods only allows our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        public bool __eq__(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.Count != Count) {
                    return false;
                }
                return issubset(set);
            }
            return false;
        }

        public bool __ne__(object other) {
            return !__eq__(other);
        }

        #endregion

        #region IRichComparable

        public static bool operator >(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(s, self, true);
        }

        public static bool operator <(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(self, s, true);
        }

        public static bool operator >=(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(s, self, false);
        }

        public static bool operator <=(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(self, s, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        [SpecialName]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public int __cmp__(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new SetIterator(_items, true);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new SetIterator(_items, true);
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return SetHelpers.SetToString(context, this, _items);
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            int i = 0;
            foreach (object o in this) {
                array.SetValue(o, index + i++);
            }
        }

        public int Count {
            [PythonHidden]
            get { return _items.Count; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion      
    }


    /// <summary>
    /// Immutable set class
    /// </summary>
    [PythonType("frozenset"), DebuggerDisplay("frozenset, {Count} items", TargetTypeName = "frozenset"), DebuggerTypeProxy(typeof(CollectionDebugProxy))]
    public class FrozenSetCollection : ISet {
        internal static readonly FrozenSetCollection EMPTY = new FrozenSetCollection();

        private CommonDictionaryStorage _items;
        private HashCache _hashCache;

        #region Set Construction

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        public void __init__(params object[] o) {
            // nop
        }
        
        public static FrozenSetCollection __new__(CodeContext context, object cls) {
            if (cls == TypeCache.FrozenSet) {
                return EMPTY;
            } else {
                PythonType dt = cls as PythonType;
                object res = dt.CreateInstance(context);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) {
                    throw PythonOps.TypeError("{0} is not a subclass of frozenset", res);
                }
                return fs;
            }
        }

        public static FrozenSetCollection __new__(CodeContext context, object cls, object setData) {
            if (cls == TypeCache.FrozenSet) {
                FrozenSetCollection fs = setData as FrozenSetCollection;
                if (fs != null) {
                    // constructing frozen set from frozen set, we return the original frozen set.
                    return fs;
                }

                fs = FrozenSetCollection.Make(setData);
                return fs;
            } else {
                object res = ((PythonType)cls).CreateInstance(context, setData);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) {
                    throw PythonOps.TypeError("{0} is not a subclass of frozenset", res);
                }

                return fs;
            }
        }

        internal static FrozenSetCollection Make(object setData) {
            FrozenSetCollection fs = setData as FrozenSetCollection;
            if (fs != null) {
                // constructing frozen set from frozen set, we return the original frozen set.
                return fs;
            }

            CommonDictionaryStorage items = ListToDictionary(setData);

            if (items.Count == 0) {
                fs = EMPTY;
            } else {
                fs = new FrozenSetCollection(items);
            }

            return fs;
        }

        private static CommonDictionaryStorage ListToDictionary(object set) {
            IEnumerator setData = PythonOps.GetEnumerator(set);
            CommonDictionaryStorage items = new CommonDictionaryStorage();
            while (setData.MoveNext()) {
                object o = setData.Current;
                items.Add(o, o);
            }
            return items;
        }

        public FrozenSetCollection()
            : this(new CommonDictionaryStorage()) {
        }

        private FrozenSetCollection(CommonDictionaryStorage set) {
            _items = set;
        }

        protected FrozenSetCollection(object set)
            : this(ListToDictionary(set)) {
        }

        internal FrozenSetCollection(ISet set)
            : this((object)set) {
        }

        public FrozenSetCollection copy() {
            // Python behavior: If we're a non-derived frozen set, we return ourselves. 
            // If we're a derived frozen set we make a new set of our type that contains our
            // contents.
            if (this.GetType() == typeof(FrozenSetCollection)) {
                return this;
            }
            FrozenSetCollection set = (FrozenSetCollection)SetHelpers.MakeSet(this, this);
            return set;
        }

        #endregion

        #region ISet

        public int __len__() {
            return Count;
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSets for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            if (_items.Count == 0) {
                PythonOps.Hash(DefaultContext.Default, value);    // make sure we have a hashable item
            }
            return _items.Contains(value);
        }

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(_items, DynamicHelpers.GetPythonTypeFromType(typeof(FrozenSetCollection)));
        }

        void ISet.PrivAdd(object adding) {
            PythonOps.Hash(DefaultContext.Default, adding);// make sure we're hashable
            _items.Add(adding, adding);
        }

        void ISet.PrivRemove(object removing) {
            PythonOps.Hash(DefaultContext.Default, removing);// make sure we're hashable
            _items.Remove(removing);
        }

        void ISet.SetData(IEnumerable set) {
            _items = new CommonDictionaryStorage();
            foreach (object o in set) {
                _items.Add(o, o);
            }
        }

        #endregion

        #region NonOperator Operations

        public bool isdisjoint(object s) {
            return SetHelpers.Intersection(this, s).Count == 0;
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(object set) {
            return SetHelpers.IsSuperset(this, set);
        }

        public ISet union() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet union(object s) {
            return SetHelpers.Union(this, s);
        }

        public ISet union([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Union(res, s);
            }
            return res;
        }

        public ISet intersection() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet intersection(object s) {
            return SetHelpers.Intersection(this, s);
        }

        public ISet intersection([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Intersection(res, s);
            }
            return res;
        }

        public ISet difference() {
            return SetHelpers.MakeSet(this, this);
        }

        public ISet difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public ISet difference([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Difference(res, s);
            }
            return res;
        }

        public ISet symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        #endregion

        #region Operators

        public static object operator &(ISet y, FrozenSetCollection x) {
            return y.intersection(x);
        }

        public static object operator |(ISet y, FrozenSetCollection x) {
            return y.intersection(x);
        }

        public static object operator ^(ISet y, FrozenSetCollection x) {
            return y.intersection(x);
        }

        public static object operator -(ISet y, FrozenSetCollection x) {
            return y.difference(x);
        }

        public static ISet operator &(FrozenSetCollection x, ISet y) {
            return x.intersection(y);
        }

        public static ISet operator |(FrozenSetCollection x, ISet y) {
            return x.union(y);
        }

        public static ISet operator ^(FrozenSetCollection x, ISet y) {
            return x.symmetric_difference(y);
        }

        public static ISet operator -(FrozenSetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IStructuralEquatable Members

        private sealed class HashCache {
            internal readonly int HashCode;
            internal readonly IEqualityComparer Comparer;

            internal HashCache(int hashCode, IEqualityComparer comparer) {
                HashCode = hashCode;
                Comparer = comparer;
            }
        }

        private int CalculateHashCode(IEqualityComparer/*!*/ comparer) {
            Assert.NotNull(comparer);

            HashCache curHashCache = _hashCache;
            if (curHashCache != null && object.ReferenceEquals(comparer, curHashCache.Comparer)) {
                return curHashCache.HashCode;
            }

            // hash code needs be stable across collections (even if keys are
            // added in different order) and needs to be fairly collision free.

            int[] hash_codes = new int[_items.Count];

            int i = 0;
            foreach (object o in _items.GetKeys()) {
                hash_codes[i++] = comparer.GetHashCode(o);
            }

            Array.Sort(hash_codes);

            int hash1 = 6551;
            int hash2 = hash1;

            for (i = 0; i < hash_codes.Length; i += 2) {
                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hash_codes[i];

                if (i == hash_codes.Length - 1) {
                    break;
                }
                hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hash_codes[i + 1];
            }

            hash1 += hash2 * 1566083941;

            _hashCache = new HashCache(hash1, comparer);
            return hash1;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer/*!*/ comparer) {
            return CalculateHashCode(comparer);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
            ISet set = other as ISet;
            if (set != null) {
                return SetHelpers.Equals(this, set, comparer);
            }
            return false;
        }

        // default conversion of protocol methods only allows our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        public bool __eq__(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.Count != Count) {
                    return false;
                }
                return issubset(set);
            }
            return false;
        }

        public bool __ne__(object other) {
            return !__eq__(other);
        }

        #endregion

        #region IValueEquality Members

#if CLR2
        int IValueEquality.GetValueHashCode() {
            return CalculateHashCode(DefaultContext.DefaultPythonContext.EqualityComparerNonGeneric);
        }

        bool IValueEquality.ValueEquals(object o) {
            return __eq__(o);
        }
#endif
        #endregion

        #region IRichComparable

        public static bool operator >(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(s, self, true);
        }

        public static bool operator <(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(self, s, true);
        }

        public static bool operator >=(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(s, self, false);
        }

        public static bool operator <=(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            return SetHelpers.IsSubset(self, s, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        [SpecialName]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public int __cmp__(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new SetIterator(_items, false);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new SetIterator(_items, false);
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return SetHelpers.SetToString(context, this, _items);
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            int i = 0;
            foreach (object o in this) {
                array.SetValue(o, index + i++);
            }
        }

        public int Count {
            [PythonHidden]
            get { return _items.Count; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion
    }

    /// <summary>
    /// Iterator over sets
    /// </summary>
    [PythonType("setiterator")]
    public sealed class SetIterator : IEnumerable, IEnumerable<object>, IEnumerator, IEnumerator<object> {
        private readonly CommonDictionaryStorage _items;
        private readonly IEnumerator<object> _enumerator;
        private readonly int _version;

        internal SetIterator(CommonDictionaryStorage items, bool mutable) {
            _items = items;
            if (mutable) {
                lock (items) {
                    _version = _items.Version;
                    _enumerator = items.GetKeys().GetEnumerator();
                }
            } else {
                _version = _items.Version;
                _enumerator = items.GetKeys().GetEnumerator();
            }
        }

        #region IDisposable Members

        [PythonHidden]
        public void Dispose() {
            _enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        public object Current {
            [PythonHidden]
            get {
                if (_items.Version != _version) {
                    throw PythonOps.RuntimeError("set changed during iteration");
                }

                return _enumerator.Current;
            }
        }

        [PythonHidden]
        public bool MoveNext() {
            return _enumerator.MoveNext();
        }

        [PythonHidden]
        public void Reset() {
            _enumerator.Reset();
        }

        #endregion

        #region IEnumerable Members

        [PythonHidden]
        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion
    }
}
