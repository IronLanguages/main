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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace IronRuby.Builtins {

    /// <summary>
    /// Implements Ruby array.
    /// Not thread safe (even when frozen).
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{GetDebugView()}")]
    public partial class RubyArray : IList<object>, IList, IRubyObjectState, IDuplicable {
        private object[]/*!*/ _content;
        
        private int _start;
        private int _count;

        private uint _flags;
        private const uint IsFrozenFlag = 1;
        private const uint IsTaintedFlag = 2;
        private const uint IsUntrustedFlag = 4;

        [Conditional("DEBUG")]
        private void ObjectInvariant() {
            Debug.Assert(_start >= 0 && _count >= 0);
            Debug.Assert(_start + _count <= _content.Length);
            Debug.Assert(_content.Length == 0 || _content.Length >= Utils.MinListSize);
        }

        [Conditional("DEBUG")]
        internal void RequireNullEmptySlots() {
            for (int i = 0; i < _content.Length; i++) {
                if (i < _start || i >= _start + _count) {
                    Debug.Assert(_content[i] == null);
                }
            }
        }

        #region Construction

        internal RubyArray(object[]/*!*/ content, int start, int count) {
            _start = start;
            _content = content;
            _count = count;
            ObjectInvariant();
        }

        public RubyArray() 
            : this(ArrayUtils.EmptyObjects, 0, 0) {
        }

        public RubyArray(int capacity)
            : this(new object[Math.Max(capacity, Utils.MinListSize)], 0, 0) {
        }

        public RubyArray(RubyArray/*!*/ items)
            : this(items, 0, items.Count) {
        }

        public RubyArray(RubyArray/*!*/ items, int start, int count)
            : this(count) {
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");

            AddVector(items._content, items._start + start, count);
        }

        public RubyArray(IList/*!*/ items)
            : this(items, 0, items.Count) {
        }

        public RubyArray(IList/*!*/ items, int start, int count)
            : this(count) {
            AddRange(items, start, count);
        }

        public RubyArray(ICollection/*!*/ items)
            : this(items.Count) {
            AddCollection(items);
        }

        public RubyArray(IEnumerable/*!*/ items) 
            : this() {
            AddRange(items);
        }

        public static RubyArray/*!*/ Create(object item) {
            var content = new object[Utils.MinListSize];
            content[0] = item;
            return new RubyArray(content, 0, 1);
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

        #region Flags

        public void RequireNotFrozen() {
            if ((_flags & IsFrozenFlag) != 0) {
                // throw in a separate method to allow inlining of the current one
                ThrowObjectFrozenException();
            }
        }

        private static void ThrowObjectFrozenException() {
            throw RubyExceptions.CreateObjectFrozenError();
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

        public RubyArray/*!*/ Freeze() {
            _flags |= IsFrozenFlag;
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

            using (IDisposable handleSelf = _EqualsTracker.TrackObject(self), handleOther = _EqualsTracker.TrackObject(other)) {
                if (handleSelf == null && handleOther == null) {
                    // both arrays went recursive:
                    return true;
                }

                var site = eqlStorage.GetCallSite("eql?");
                for (int i = 0; i < self.Count; i++) {
                    if (!Protocols.IsEqual(site, self[i], other[i])) {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region Add, AddRange

        /// <summary>
        /// Adds count to _count and resizes the storage to make space for count elements if necessary.
        /// </summary>
        /// <param name="additionalCount">The number of elements that will be appended.</param>
        /// <param name="clear">
        /// True if the slots should be cleared (null initialized). 
        /// The caller isn't filling the slots with non-null values.
        /// </param>
        /// <returns>The index of the first slot that will contain the new elements.</returns>
        private int ResizeForAppend(int additionalCount, bool clear) {
            int oldCount = _count;
            _count += additionalCount;

            if (_start + oldCount > _content.Length - additionalCount) {
                // not enough space for additional items:
                object[] newContent;
                if (_count > _content.Length) {
                    newContent = new object[Utils.GetExpandedSize(_content, _count)];
                } else {
                    newContent = _content;
                }
                Array.Copy(_content, _start, newContent, 0, oldCount);
                if (newContent == _content && (additionalCount < _start || clear)) {
                    if (_start < oldCount) {
                        Utils.Fill(newContent, oldCount, null, _start);
                    } else {
                        Utils.Fill(newContent, _start, null, oldCount);
                    }
                }
                _content = newContent;
                _start = 0;
            }
            ObjectInvariant();
            return _start + oldCount;
        }

        public void Add(object item) {
            Mutate();
            int index = ResizeForAppend(1, false);
            _content[index] = item;
        }

        int IList.Add(object value) {
            int result = _count;
            Add(value);
            return result;
        }

        /// <summary>
        /// Ensures that the underlying storage is prepared to store at least the current number plus capacity items.
        /// </summary>
        /// <param name="capacity">Additional capacity.</param>
        /// <returns>Self.</returns>
        /// <remarks>
        /// Use to avoid reallocation of the underlying storage if the number of elements that will eventually by added to this array is known.
        /// Doesn't increase the number of elements in the array.
        /// Call <see cref="AddMultiple"/> to add multiple (possibly null) elements into the array.
        /// </remarks>
        public RubyArray/*!*/ AddCapacity(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity");
            }

            Mutate();
            int oldCount = _count;
            // We're not writing to the slots, so require them to be cleared so that we don't 
            // accidentally keep objects alive whose references might have been in the slots.
            // (the resize could've just copied the content left to make space for new elements).
            ResizeForAppend(capacity, true);
            _count = oldCount;
            return this;
        }

        public RubyArray/*!*/ AddMultiple(int count, object value) {
            Mutate();
            if (value != null) {
                int start = ResizeForAppend(count, false);
                int end = start + count;
                for (int i = start; i < end; i++) {
                    _content[i] = value;
                }
            } else {
                ResizeForAppend(count, true);
            }
            return this;
        }

        public RubyArray/*!*/ AddRange(IList/*!*/ items, int start, int count) {
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");
            Mutate();

            RubyArray array;
            object[] vector;
            if ((array = items as RubyArray) != null) {
                AddVector(array._content, array._start + start, count);
            } else if ((vector = items as object[]) != null) {
                AddVector(vector, start, count);
            } else {
                AddList(items, start, count);
            }

            return this;
        }

        public RubyArray/*!*/ AddRange(IEnumerable/*!*/ items) {
            ContractUtils.RequiresNotNull(items, "items");
            Mutate();

            RubyArray array;
            ICollection collection;
            object[] vector;

            if ((array = items as RubyArray) != null) {
                AddVector(array._content, array._start, array._count);
            } else if ((vector = items as object[]) != null) {
                AddVector(vector, 0, vector.Length);
            } else if ((collection = items as ICollection) != null) {
                AddCollection(collection);
            } else {
                AddSequence(items);
            }

            return this;
        }

        private void AddList(IList/*!*/ items, int start, int count) {
            int s = ResizeForAppend(count, false);
            for (int i = 0; i < count; i++) {
                _content[s + i] = items[start + i];
            }
        }

        internal void AddVector(object[]/*!*/ items, int start, int count) {
            int s = ResizeForAppend(count, false);
            Array.Copy(items, start, _content, s, count);
        }

        private void AddCollection(ICollection/*!*/ items) {
            int i = ResizeForAppend(items.Count, false);
            foreach (object item in items) {
                _content[i++] = item;
            }
        }

        private void AddSequence(IEnumerable/*!*/ items) {
            foreach (var item in items) {
                Add(item);
            }
        }

        #endregion

        #region Insert, InsertRange

        private int ResizeForInsertion(int index, int size) {
            if (_count + size > _content.Length) {
                object[] newContent = new object[Utils.GetExpandedSize(_content, _count + size)];
                Array.Copy(_content, _start, newContent, 0, index);
                Array.Copy(_content, _start + index, newContent, index + size, _count - index);
                _count += size;
                _content = newContent;
                return index;
            }

            int rindex = _start + index;
            int result = rindex;
            int spaceRight = _content.Length - _start - _count;
            int shiftLeft = 0, shiftRight = 0;
            if (_start >= size) {
                if (spaceRight >= size) {
                    if (index < _count / 2) {
                        shiftLeft = size;
                        result -= size;
                    } else {
                        shiftRight = size;
                    }
                } else {
                    shiftLeft = size;
                    result -= size;
                }
            } else if (spaceRight >= size) {
                shiftRight = size;
            } else {
                shiftLeft = _start;
                shiftRight = size - shiftLeft;
                result -= shiftLeft;
            }
            
            if (shiftLeft > 0) {
                var newStart = _start - shiftLeft;
                Array.Copy(_content, _start, _content, newStart, index);
                _start = newStart;
            } 

            if (shiftRight > 0) {
                Array.Copy(_content, rindex, _content, rindex + shiftRight, _count - index);
            }

            _count += size;
            return result;
        }

        public void Insert(int index, object item) {
            ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
            Mutate();
            int i = ResizeForInsertion(index, 1);
            _content[i] = item;
            ObjectInvariant();
        }

        public void InsertRange(int index, IList/*!*/ items, int start, int count) {
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
            ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");
            Mutate();

            RubyArray array;
            object[] vector;

            if ((array = items as RubyArray) != null) {
                InsertVector(index, array._content, start, count);
            } else if ((vector = items as object[]) != null) {
                InsertVector(index, vector, start, count);
            } else {
                InsertList(index, items, start, count);
            }
        }

        public void InsertRange(int index, IEnumerable/*!*/ items) {
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
            Mutate();
            
            RubyArray array;
            IList list;
            object[] vector;

            if ((array = items as RubyArray) != null) {
                InsertArray(index, array);
            } else if ((vector = items as object[]) != null) {
                InsertVector(index, vector, 0, vector.Length);
            } else if ((list = items as IList) != null) {
                InsertList(index, list, 0, list.Count);
            } else {
                InsertArray(index, new RubyArray(items));
            }
        }

        private void InsertArray(int index, RubyArray/*!*/ array) {
            InsertVector(index, array._content, array._start, array._count);
        }

        private void InsertVector(int index, object[]/*!*/ items, int start, int count) {
            int s = ResizeForInsertion(index, count);
            Array.Copy(items, start, _content, s, count);
        }

        private void InsertList(int index, IList/*!*/ items, int start, int count) {
            int s = ResizeForInsertion(index, count);
            for (int i = 0; i < count; i++) {
                _content[s + i] = items[start + i];
            }
        }

        #endregion

        #region RemoveAt, RemoveRange, Remove, Clear

        public void RemoveRange(int index, int size) {
            ContractUtils.RequiresArrayRange(_count, index, size, "index", "size");
            Mutate();

            int newCount = _count - size;
            int length = _content.Length;
            
            int newLength = 2 * newCount - 1;
            if (newLength <= length / 2) {
                // resize:
                object[] newContent = new object[Math.Max(Utils.MinListSize, newLength)];
                Array.Copy(_content, _start, newContent, 0, index);
                Array.Copy(_content, _start + index + size, newContent, index, newCount - index);
                _content = newContent;
                _start = 0;
            } else if (index <= newCount / 2) {
                // shift right:
                int newStart = _start + size;
                Array.Copy(_content, _start, _content, newStart, index);
                Utils.Fill(_content, _start, null, size);
                _start = newStart;
            } else {
                // shift left:
                Array.Copy(_content, _start + index + size, _content, _start + index, newCount - index);
                Utils.Fill(_content, _start + newCount, null, size);
            }
            _count = newCount;
            ObjectInvariant();
        }

        public void RemoveAt(int index) {
            RemoveRange(index, 1);
        }

        public bool Remove(object item) {
            int index = IndexOf(item);
            if (index >= 0) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        void IList.Remove(object value) {
            Remove(value);
        }

        public void Clear() {
            Mutate();
            _content = ArrayUtils.EmptyObjects;
            _start = _count = 0;
        }

        #endregion

        #region CopyTo, ToArray

        public void CopyTo(object[]/*!*/ array, int index) {
            Array.Copy(_content, _start, array, index, _count);
        }

        void ICollection.CopyTo(Array array, int index) {
            Array.Copy(_content, _start, array, index, _count);
        }

        public object[]/*!*/ ToArray() {
            object[] result = new object[_count];
            CopyTo(result, 0);
            return result;
        }

        #endregion

        #region IndexOf, Contains, FindIndex, GetEnumerator

        public int IndexOf(object item) {
            return IndexOf(item, 0, _count);
        }

        public int IndexOf(object item, int startIndex) {
            return Array.IndexOf(_content, item, startIndex, _count - startIndex);
        }
        
        public int IndexOf(object item, int startIndex, int count) {
            return Array.IndexOf(_content, item, _start + startIndex, count);
        }

#if !SILVERLIGHT // Array.FindIndex
        public int FindIndex(Predicate<object> match) {
            return FindIndex(0, _count, match);
        }

        public int FindIndex(int startIndex, Predicate<object>/*!*/ match) {
            return FindIndex(startIndex, _count - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<object>/*!*/ match) {
            return Array.FindIndex(_content, _start + startIndex, count, match);
        }
#endif

        public bool Contains(object item) {
            return IndexOf(item) >= 0;
        }

        public IEnumerator<object>/*!*/ GetEnumerator() {
            for (int i = 0, start = _start, count = _count; i < count; i++) {
                yield return _content[start + i];
            }
        }

        IEnumerator/*!*/ IEnumerable.GetEnumerator() {
            return ((IEnumerable<object>)this).GetEnumerator();
        }

        #endregion

        #region Misc

        public int Count {
            get { return _count; }
        }

        public bool IsReadOnly {
            get { return IsFrozen; }
        }

        bool IList.IsFixedSize {
            get { return IsReadOnly; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        public int Capacity {
            get { return _content.Length; }
        }

        public object this[int index] {
            get {
                if (index < 0 || index >= _count) {
                    throw new IndexOutOfRangeException();
                }
                return _content[_start + index];
            }
            set {
                Mutate();
                int delta = index - _count;
                if (delta >= 0) {
                    ResizeForAppend(delta + 1, true);
                }
                _content[_start + index] = value;
            }
        }

        public void Reverse() {
            Mutate();
            Array.Reverse(_content, _start, _count);
        }

        public void Sort() {
            Mutate();
            Array.Sort(_content, _start, _count);
        }

        public void Sort(Comparison<object>/*!*/ comparison) {
            Mutate();
            Array.Sort(_content, _start, _count, ArrayUtils.ToComparer(comparison));
        }

        #endregion

        #region DebugView

        internal string/*!*/ GetDebugView() {
            return RubyContext._Default != null ? RubyContext._Default.Inspect(this).ToString() : ToString();
        }

        #endregion
    }
}
