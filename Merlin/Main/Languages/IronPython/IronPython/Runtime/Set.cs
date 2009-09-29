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

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    /// <summary>
    /// Common interface shared by both Set and FrozenSet
    /// </summary>
    public interface ISet : IEnumerable, IEnumerable<object> {
        int __len__();

        bool __contains__(object value);
        bool issubset(object set);
        bool issuperset(CodeContext context, object set);

        // private methods used for operations between set types.
        ISet PrivDifference(IEnumerable set);
        ISet PrivIntersection(IEnumerable set);
        ISet PrivSymmetricDifference(IEnumerable set);
        ISet PrivUnion(IEnumerable set);
        void PrivAdd(object adding);
        void PrivRemove(object removing);
        void PrivFreeze();
        void SetData(IEnumerable set);
    }

    /// <summary>
    /// Contains common set functionality between set and forzenSet
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
            foreach (KeyValuePair<object, object> o in items.GetItems()) {
                sb.Append(comma);
                sb.Append(PythonOps.Repr(context, o.Key));
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
            res.PrivFreeze();
            return res;
        }

        public static ISet Difference(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (res.__contains__(ie.Current)) {
                    res.PrivRemove(ie.Current);
                }
            }
            res.PrivFreeze();
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
            res.PrivFreeze();
            return res;
        }

        public static ISet Union(ISet x, object y) {
            ISet set = SetHelpers.MakeSet(x, x);
            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                set.PrivAdd(ie.Current);
            }
            set.PrivFreeze();
            return set;
        }

        public static bool IsSubset(ISet x, object y) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(y));
            foreach (object o in x) {
                if (!set.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static PythonTuple Reduce(CommonDictionaryStorage items, PythonType type) {
            object[] keys = new object[items.Count];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in items.GetItems()) {
                keys[i++] = kvp.Key;
            }
            return PythonTuple.MakeTuple(type, PythonTuple.MakeTuple(List.FromArrayNoCopy(keys)), null);
        }
    }

    /// <summary>
    /// Mutable set class
    /// </summary>
    [PythonType("set")]
    public class SetCollection : ISet, IValueEquality, ICodeFormattable, ICollection {
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

        #endregion

        #region ISet

        public int __len__() {
            return _items.Count;
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            if (_items.Count == 0) {
                PythonOps.Hash(DefaultContext.Default, value);    // make sure we have a hashable item
            }
            return _items.Contains(value);
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(CodeContext context, object set) {
            return this >= new SetCollection(PythonOps.GetEnumerator(set));
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)difference(set);
        }

        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)intersection(set);
        }

        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)symmetric_difference(set);
        }

        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)union(set);
        }

        void ISet.PrivAdd(object adding) {
            add(adding);
        }

        void ISet.PrivRemove(object removing) {
            remove(removing);
        }

        void ISet.PrivFreeze() {
            // nop for non-frozen sets.
        }

        void ISet.SetData(IEnumerable set) {
            _items = new CommonDictionaryStorage();
            foreach (object o in set) {
                _items.Add(o, o);
            }
        }

        #endregion

        #region NonOperator Operations

        public object union() {
            return SetHelpers.MakeSet(this, this);
        }

        public object union(object s) {
            return SetHelpers.Union(this, s);
        }

        public ISet union([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Union(res, s);
            }
            return res;
        }

        public object intersection() {
            return SetHelpers.MakeSet(this, this);
        }

        public object intersection(object s) {
            return SetHelpers.Intersection(this, s);
        }

        public ISet intersection([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Intersection(res, s);
            }
            return res;
        }

        public object difference() {
            return SetHelpers.MakeSet(this, this);
        }

        public object difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public ISet difference([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Difference(res, s);
            }
            return res;
        }

        public object symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        public bool isdisjoint(object s) {
            return SetHelpers.Intersection(this, s).__len__() == 0;
        }

        public SetCollection copy() {
            return new SetCollection(((IEnumerable)this).GetEnumerator());
        }

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(_items, DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection)));
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

        #endregion

        #region Mutating Members

        /// <summary>
        /// Appends one IEnumerable to an existing set
        /// </summary>
        /// <param name="s"></param>
        public void update(object s) {
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
            foreach (KeyValuePair<object, object> o in _items.GetItems()) {
                _items.Remove(o.Key);
                return o.Key;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), SpecialName]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public int __cmp__(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public static object operator &(ISet y, SetCollection x) {
            return x & y;
        }

        public static object operator |(ISet y, SetCollection x) {
            return x | y;
        }

        public static object operator ^(ISet y, SetCollection x) {
            return x ^ y;
        }

        public static object operator -(ISet y, SetCollection x) {
            return SetHelpers.MakeSet(x, y.PrivDifference(x));
        }

        public static object operator &(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.intersection(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.intersection(x);
            }

            return SetHelpers.MakeSet(x, y.PrivIntersection(x));
        }

        public static object operator |(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.union(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.union(x);
            }

            return SetHelpers.MakeSet(x, y.PrivUnion(x));
        }

        public static object operator ^(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.symmetric_difference(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.symmetric_difference(x);
            }

            return SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
        }

        public static object operator -(SetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IEnumerable Members

        [PythonHidden]
        public IEnumerator GetEnumerator() {
            return new SetIterator(_items, true);
        }

        #endregion

        #region IRichComparable

        public static bool operator >(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            if (s.__len__() >= self.__len__()) return false;

            foreach (object o in s) {
                if (!self.__contains__(o)) return false;
            }
            return true;
        }

        public static bool operator <(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            if (s.__len__() <= self.__len__()) return false;

            foreach (object o in self) {
                if (!s.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(SetCollection self, object other) {
            return self > other || ((IValueEquality)self).ValueEquals(other);
        }

        public static bool operator <=(SetCollection self, object other) {
            return self < other || ((IValueEquality)self).ValueEquals(other);
        }

        #endregion

        #region IValueEquality Members

        // default conversion of protocol methods only allow's our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("set objects are unhashable");
        }

        bool IValueEquality.ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.__len__() != __len__()) return false;
                return set.issubset(this) && this.issubset(set);
            }
            return false;
        }

        public bool __eq__(object other) {
            return ((IValueEquality)this).ValueEquals(other);
        }

        public bool __ne__(object other) {
            return !((IValueEquality)this).ValueEquals(other);
        }

        public const object __hash__ = null;


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
            throw new NotImplementedException();
        }

        public int Count {
            [PythonHidden]
            get { return this._items.Count; }
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
    /// Non-mutable set class
    /// </summary>
    [PythonType("frozenset")]
    public class FrozenSetCollection : ISet, IValueEquality, ICodeFormattable, ICollection {
        internal static readonly FrozenSetCollection EMPTY = new FrozenSetCollection();

        private CommonDictionaryStorage _items;
        private int _hashCode;
#if DEBUG
        private int _returnedHc;
#endif

        #region Set Construction

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
                    // constructing frozen set from set, we return the original frozen set.
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
                // constructing frozen set from set, we return the original frozen set.
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

        public FrozenSetCollection() {
            _items = new CommonDictionaryStorage();
            // hash code is 0 for empty set
            CalculateHashCode();
        }

        private FrozenSetCollection(CommonDictionaryStorage set) {
            _items = set;
            CalculateHashCode();
        }

        protected FrozenSetCollection(object set)
            : this(ListToDictionary(set)) {
        }

        internal FrozenSetCollection(ISet set)
            : this((object)set) {
        }

        #endregion

        #region ISet

        public int __len__() {
            return _items.Count;
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            PythonOps.Hash(DefaultContext.Default, value);// make sure we have a hashable item
            return _items.Contains(value);
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(CodeContext context, object set) {
            return this >= FrozenSetCollection.Make(set);
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)difference(set);
        }

        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)intersection(set);
        }

        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)symmetric_difference(set);
        }

        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)union(set);
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
            CalculateHashCode();
        }

        void ISet.PrivFreeze() {
            CalculateHashCode();
        }

        #endregion

        #region NonOperator operations

        public object union() {
            return SetHelpers.MakeSet(this, this);
        }

        public object union(object s) {
            return SetHelpers.Union(this, s);
        }

        public ISet union([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Union(res, s);
            }
            return res;
        }

        public object intersection() {
            return SetHelpers.MakeSet(this, this);
        }

        public object intersection(object s) {
            return (SetHelpers.Intersection(this, s));
        }

        public ISet intersection([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Intersection(res, s);
            }
            return res;
        }

        public object difference() {
            return SetHelpers.MakeSet(this, this);
        }

        public object difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public ISet difference([NotNull] params object[] ss) {
            ISet res = this;
            foreach (object s in ss) {
                res = SetHelpers.Difference(res, s);
            }
            return res;
        }

        public object symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        public bool isdisjoint(object s) {
            return SetHelpers.Intersection(this, s).__len__() == 0;
        }

        public object copy() {
            // Python behavior: If we're a non-derived frozen set, we return ourselves. 
            // If we're a derived frozen set we make a new set of our type that contains our
            // contents.
            if (this.GetType() == typeof(FrozenSetCollection)) {
                return (this);
            }
            ISet set = SetHelpers.MakeSet(this, this);
            set.PrivFreeze();
            return (set);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        public void __init__(params object[] o) {
            // nop
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

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(_items, DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection)));
        }

        #endregion

        #region Operators

        public static object operator &(ISet y, FrozenSetCollection x) {
            return x & y;
        }

        public static object operator |(ISet y, FrozenSetCollection x) {
            return x | y;
        }

        public static object operator ^(ISet y, FrozenSetCollection x) {
            return x ^ y;
        }

        public static object operator -(ISet y, FrozenSetCollection x) {
            return SetHelpers.MakeSet(x, y.PrivDifference(x));
        }

        public static object operator &(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.intersection(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.intersection(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivIntersection(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator |(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.union(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.union(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivUnion(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator ^(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.symmetric_difference(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.symmetric_difference(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator -(FrozenSetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IEnumerable Members

        [PythonHidden]
        public IEnumerator GetEnumerator() {
            return new SetIterator(_items, false);
        }

        #endregion

        private void CalculateHashCode() {
            // hash code needs be stable across collections (even if keys are
            // added in different order) and needs to be fairly collision free.
            _hashCode = 6551;

            int[] hash_codes = new int[_items.Count];

            int i = 0;
            foreach (KeyValuePair<object, object> o in _items.GetItems()) {
                hash_codes[i++] = PythonOps.Hash(DefaultContext.Default, o.Key);
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

            _hashCode = hash1 + (hash2 * 1566083941);
        }

        #region IRichComparable

        public static bool operator >(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            if (s.__len__() >= self.__len__()) return false;

            foreach (object o in s) {
                if (!self.__contains__(o)) return false;
            }
            return true;
        }

        public static bool operator <(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) {
                throw PythonOps.TypeError("can only compare to a set");
            }

            if (s.__len__() <= self.__len__()) return false;

            foreach (object o in self) {
                if (!s.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(FrozenSetCollection self, object other) {
            return self > other || ((IValueEquality)self).ValueEquals(other);
        }

        public static bool operator <=(FrozenSetCollection self, object other) {
            return self < other || ((IValueEquality)self).ValueEquals(other);
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
#if DEBUG
            // make sure we never change the hashcode we hand out in debug builds.
            // if we do then it means we somehow called PrivAdd/PrivRemove after
            // already using the hash code.
            Debug.Assert(_returnedHc == _hashCode || _returnedHc == 0);
            _returnedHc = _hashCode;
#endif
            return _hashCode;
        }
        
        // default conversion of protocol methods only allow's our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        bool IValueEquality.ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.__len__() != __len__()) return false;
                return set.issubset(this) && this.issubset(set);
            }
            return false;
        }

        public bool __eq__(object other) {
            return ((IValueEquality)this).ValueEquals(other);
        }

        public bool __ne__(object other) {
            return !((IValueEquality)this).ValueEquals(other);
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
            throw new NotImplementedException();
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
        private bool _mutable;
        private int _count;
        private CommonDictionaryStorage _items;
        private IEnumerator<object> _enumerator;

        internal SetIterator(CommonDictionaryStorage items, bool mutable) {
            _mutable = mutable;
            _items = items;
            if (mutable) {
                lock (items) {
                    _count = items.Count;
                    _enumerator = items.GetKeys().GetEnumerator();
                }
            } else {
                _count = items.Count;
                _enumerator = items.GetKeys().GetEnumerator();
            }
        }

        #region IEnumerator<object> Members

        public object Current {
            get {
                if (_mutable && (_count != _items.Count || !_items.Contains(_enumerator.Current))) {
                    throw PythonOps.RuntimeError("set changed during iteration");
                }

                return _enumerator.Current;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            _enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members


        public bool MoveNext() {
            return _enumerator.MoveNext();
        }

        public void Reset() {
            _enumerator.Reset();
        }

        #endregion

        #region IEnumerable Members

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
