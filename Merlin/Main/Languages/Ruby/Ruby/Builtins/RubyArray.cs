/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    /// <summary>
    /// Implements Ruby array.
    /// Not thread safe (even when frozen).
    /// </summary>
    [DebuggerDisplay("{GetDebugView()}")]
    public partial class RubyArray : IList<object>, IList, IRubyObjectState, IDuplicable {
        private readonly List<object>/*!*/ _content;

        // The lowest bit is tainted flag.
        // The version is set to FrozenVersion when the string is frozen. FrozenVersion is the maximum version, so any update to the version 
        // triggers an OverflowException, which we convert to InvalidOperationException.
        private uint _versionAndFlags;

        private const uint IsTaintedFlag = 1;
        private const int FlagCount = 1;
        private const uint FlagsMask = (1U << FlagCount) - 1;
        private const uint VersionMask = ~FlagsMask;
        private const uint FrozenVersion = VersionMask;

        #region Construction

        public RubyArray() {
            _content = new List<object>();
        }

        public RubyArray(int capacity) {
            _content = new List<object>(capacity);
        }

        public RubyArray(IEnumerable<object>/*!*/ collection) {
            _content = new List<object>(collection);
        }

        public RubyArray(IEnumerable/*!*/ collection)
            : this(CollectionUtils.ToEnumerable<object>(collection)) {
        }

        public RubyArray(IList/*!*/ list, int start, int count) {
            ContractUtils.RequiresNotNull(list, "list");
            ContractUtils.RequiresArrayRange(list.Count, start, count, "start", "count");

            var data = new List<object>();
            for (int i = 0; i < count; i++) {
                data.Add(list[start + i]);
            }
            _content = data;
        }

        public static RubyArray/*!*/ Create(object item) {
            var result = new RubyArray();
            result._content.Add(item);
            return result;
        }

        /// <summary>
        /// Creates a blank instance of a RubyArray or its subclass given the Ruby class object.
        /// </summary>
        public static RubyArray/*!*/ CreateInstance(RubyClass/*!*/ rubyClass) {
            return (rubyClass.GetUnderlyingSystemType() == typeof(RubyArray)) ? new RubyArray() : new RubyArray.Subclass(rubyClass);
        }

        /// <summary>
        /// Creates an empty instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Array.
        /// </summary>
        public virtual RubyArray/*!*/ CreateInstance() {
            return new RubyArray();
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        #endregion

        #region Versioning, Flags

        private void Mutate() {
            try {
                checked { _versionAndFlags += (1 << FlagCount); }
            } catch (OverflowException) {
                throw RubyExceptions.CreateTypeError("can't modify frozen object");
            }
        }

        public bool IsTainted {
            get {
                return (_versionAndFlags & IsTaintedFlag) != 0;
            }
            set {
                Mutate();
                _versionAndFlags = (_versionAndFlags & ~IsTaintedFlag) | (value ? IsTaintedFlag : 0);
            }
        }

        public bool IsFrozen {
            get {
                return (_versionAndFlags & VersionMask) == FrozenVersion;
            }
        }

        void IRubyObjectState.Freeze() {
            Freeze();
        }

        public RubyArray/*!*/ Freeze() {
            _versionAndFlags |= FrozenVersion;
            return this;
        }

        #endregion

        #region HashCode, Equality

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _HashTracker = new RubyUtils.RecursionTracker();

        public static int GetHashCode(UnaryOpStorage/*!*/ hashStorage, ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self) {
            int hash = self.Count;
            using (IDisposable handle = _HashTracker.TrackObject(self)) {
                if (handle == null) {
                    // hashing of recursive array
                    return 0;
                }

                var hashSite = hashStorage.GetCallSite("hash");
                var toIntSite = fixnumCast.GetSite(ConvertToFixnumAction.Make(fixnumCast.Context));
                foreach (object item in self) {
                    hash = (hash << 1) ^ toIntSite.Target(toIntSite, hashSite.Target(hashSite, item));
                }
            }
            return hash;
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

        public static bool Equals(BinaryOpStorage/*!*/ eqlStorage, IList/*!*/ self, object obj) {
            if (ReferenceEquals(self, obj)) {
                return true;
            }

            IList other = obj as IList;
            if (other == null || self.Count != other.Count) {
                return false;
            }

            using (IDisposable handle = _EqualsTracker.TrackObject(self)) {
                if (handle == null) {
                    // hashing of recursive array
                    return false;
                }

                var site = eqlStorage.GetCallSite("eql?");
                for (int i = 0; i < self.Count; i++) {
                    if (!Protocols.IsTrue(site.Target(site, self[i], other[i]))) {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region IList<object> Members

        public int IndexOf(object item) {
            return _content.IndexOf(item);
        }

        public object this[int index] {
            get {
                return _content[index];
            }
            set {
                Mutate();
                _content[index] = value;
            }
        }

        public void Insert(int index, object item) {
            Mutate();
            _content.Insert(index, item);
        }

        public void RemoveAt(int index) {
            Mutate();
            _content.RemoveAt(index);
        }

        #endregion

        #region ICollection<object> Members

        public int Count {
            get { return _content.Count; }
        }

        public bool IsReadOnly {
            get { return IsFrozen; }
        }

        public bool Contains(object item) {
            return _content.Contains(item);
        }

        public void CopyTo(object[]/*!*/ array, int arrayIndex) {
            _content.CopyTo(array, arrayIndex);
        }

        public void Add(object item) {
            Mutate();
            _content.Add(item);
        }

        public void Clear() {
            Mutate();
            _content.Clear();
        }

        public bool Remove(object item) {
            Mutate();
            return _content.Remove(item);
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object>/*!*/ GetEnumerator() {
            return _content.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator/*!*/ IEnumerable.GetEnumerator() {
            return ((IEnumerable)_content).GetEnumerator();
        }

        #endregion

        #region IList Members

        bool IList.IsFixedSize {
            get { return IsReadOnly; }
        }

        void IList.Remove(object value) {
            Remove(value);
        }

        int IList.Add(object value) {
            Mutate();
            int result = _content.Count;
            _content.Add(value);
            return result;
        }

        #endregion

        #region ICollection Members (read-only)

        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)_content).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized {
            get { return ((ICollection)_content).IsSynchronized; }
        }

        object ICollection.SyncRoot {
            get { return ((ICollection)_content).SyncRoot; }
        }

        #endregion

        #region List specific

        // read-only //

        public int Capacity {
            get { return _content.Capacity; }
            // cannot remove items:
            set { _content.Capacity = value; }
        }

        public void CopyTo(object[]/*!*/ result) {
            _content.CopyTo(result);
        }

        public int FindIndex(Predicate<object> match) {
            return _content.FindIndex(match);
        }

#if !SILVERLIGHT
        public int FindIndex(int startIndex, Predicate<object> match) {
            return _content.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<object> match) {
            return _content.FindIndex(startIndex, count, match);
        }
#endif
        public int BinarySearch(object item) {
            return _content.BinarySearch(item);
        }

        public int BinarySearch(object item, IComparer<object> comparer) {
            return _content.BinarySearch(item, comparer);
        }

        public int BinarySearch(int index, int count, object item, IComparer<object> comparer) {
            return _content.BinarySearch(index, count, item, comparer);
        }

        // mutating //

        public void Reverse() {
            Mutate();
            _content.Reverse();
        }

        public RubyArray/*!*/ AddMultiple(int count, object value) {
            Mutate();
            _content.Capacity += count;
            for (int i = 0; i < count; i++) {
                _content.Add(value);
            }
            return this;
        }

        public RubyArray/*!*/ AddRange(IList/*!*/ items) {
            Mutate();

            // items could be equal to this => we need to capture the count before we iterate:
            int count = items.Count;

            _content.Capacity += count;
            for (int i = 0; i < count; i++) {
                _content.Add(items[i]);
            }
            return this;
        }

        public RubyArray/*!*/ AddRange(IEnumerable/*!*/ items) {
            Mutate();
            foreach (var item in items) {
                _content.Add(item);
            }
            return this;
        }

        public void InsertRange(int index, IEnumerable<object>/*!*/ collection) {
            Mutate();
            _content.InsertRange(index, collection);
        }

        public void RemoveRange(int index, int count) {
            Mutate();
            _content.RemoveRange(index, count);
        }

        public void Sort() {
            Mutate();
            _content.Sort();
        }

        public void Sort(Comparison<object>/*!*/ comparison) {
            Mutate();
            _content.Sort(comparison);
        }

        #endregion

        #region DebugView

        internal string/*!*/ GetDebugView() {
            return RubyContext._Default.Inspect(this).ToString();
        }

        #endregion
    }
}
