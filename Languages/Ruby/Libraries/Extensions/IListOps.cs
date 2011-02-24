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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins {
    using EachSite = Func<CallSite, object, Proc, object>;

    [RubyModule(Extends = typeof(IList), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(Enumerable))]
    public static class IListOps {
        
        #region Helpers

        // MRI: Some operations check frozen flag even if they don't change the array content.
        private static void RequireNotFrozen(IList/*!*/ self) {
            RubyArray array = self as RubyArray;
            if (array != null && array.IsFrozen) {
                throw RubyExceptions.CreateObjectFrozenError();
            }
        }

        internal static int NormalizeIndex(IList/*!*/ list, int index) {
            return NormalizeIndex(list.Count, index);
        }

        internal static int NormalizeIndexThrowIfNegative(IList/*!*/ list, int index) {
            index = NormalizeIndex(list.Count, index);
            if (index < 0) {
                throw RubyExceptions.CreateIndexError("index {0} out of array", index);
            }
            return index;
        }

        internal static int NormalizeIndex(int count, int index) {
            return index < 0 ? index + count : index;
        }

        internal static bool NormalizeRange(int listCount, ref int start, ref int count) {
            start = NormalizeIndex(listCount, start);
            if (start < 0 || start > listCount || count < 0) {
                return false;
            }

            if (count != 0) {
                count = start + count > listCount ? listCount - start : count;
            }

            return true;
        }

        internal static bool NormalizeRange(ConversionStorage<int>/*!*/ fixnumCast, int listCount, Range/*!*/ range, out int begin, out int count) {
            begin = Protocols.CastToFixnum(fixnumCast, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, range.End);

            begin = NormalizeIndex(listCount, begin);

            if (begin < 0 || begin > listCount) {
                count = 0;
                return false;
            }

            end = NormalizeIndex(listCount, end);

            count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return true;
        }

        private static bool InRangeNormalized(IList/*!*/ list, ref int index) {
            if (index < 0) {
                index = index + list.Count;
            }
            return index >= 0 && index < list.Count;
        }

        private static IList/*!*/ GetResultRange(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ list, int index, int count) {
            IList result = CreateResultArray(allocateStorage, list);
            int stop = index + count;
            for (int i = index; i < stop; i++) {
                result.Add(list[i]);
            }
            return result;
        }

        private static void InsertRange(IList/*!*/ list, int index, IList/*!*/ items, int start, int count) {
            RubyArray array;
            List<object> listOfObject;
            ICollection<object> collection;
            if ((array = list as RubyArray) != null) {
                array.InsertRange(index, items, start, count);
            } else if ((listOfObject = list as List<object>) != null && ((collection = items as ICollection<object>) != null) && start == 0 && count == collection.Count) {
                listOfObject.InsertRange(index, collection);
            } else {
                for (int i = 0; i < count; i++) {
                    list.Insert(index + i, items[start + i]);
                }
            }
        }

        internal static void RemoveRange(IList/*!*/ collection, int index, int count) {
            if (count <= 1) {
                if (count > 0) {
                    collection.RemoveAt(index);
                }
                return;
            }

            List<object> list;
            RubyArray array;
            if ((array = collection as RubyArray) != null) {
                array.RemoveRange(index, count);
            } else if ((list = collection as List<object>) != null) {
                list.RemoveRange(index, count);
            } else {
                for (int i = index + count - 1; i >= index; i--) {
                    collection.RemoveAt(i);
                }
            }
        }

        internal static void AddRange(IList/*!*/ collection, IList/*!*/ items) {
            RubyArray array;

            int count = items.Count;
            if (count <= 1) {
                if (count > 0) {
                    collection.Add(items[0]);
                } else if ((array = collection as RubyArray) != null) {
                    array.RequireNotFrozen();
                }
                return;
            }

            array = collection as RubyArray;
            if (array != null) {
                array.AddRange(items);
            } else {
                for (int i = 0; i < count; i++) {
                    collection.Add(items[i]);
                }
            }
        }

        private static IList/*!*/ CreateResultArray(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ list) {
            // RubyArray:
            var array = list as RubyArray;
            if (array != null) {
                return array.CreateInstance();
            }
            
            // interop - call a default ctor to get an instance:
            var allocate = allocateStorage.GetCallSite("allocate", 0);
            var cls = allocateStorage.Context.GetClassOf(list);
            var result = allocate.Target(allocate, cls) as IList;
            if (result != null) {
                return result;
            }

            throw RubyExceptions.CreateTypeError("{0}#allocate should return IList", cls.Name);
        }

        internal static IEnumerable<Int32>/*!*/ ReverseEnumerateIndexes(IList/*!*/ collection) {
            for (int originalSize = collection.Count, i = originalSize - 1; i >= 0; i--) {
                yield return i;
                if (collection.Count < originalSize) {
                    i = originalSize - (originalSize - collection.Count);
                    originalSize = collection.Count;
                }
            }
        }

        #endregion

        #region initialize_copy, replace, clear, to_a, to_ary

        [RubyMethod("replace")]
        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static IList/*!*/ Replace(IList/*!*/ self, [NotNull, DefaultProtocol]IList/*!*/ other) {
            self.Clear();
            AddRange(self, other);
            return self;
        }

        [RubyMethod("clear")]
        public static IList Clear(IList/*!*/ self) {
            self.Clear();
            return self;
        }

        [RubyMethod("to_a")]
        [RubyMethod("to_ary")]
        public static RubyArray/*!*/ ToArray(IList/*!*/ self) {
            RubyArray list = new RubyArray(self.Count);
            foreach (object item in self) {
                list.Add(item);
            }
            return list;
        }
        
        #endregion

        #region *, +, concat, product

        [RubyMethod("*")]
        public static IList/*!*/ Repeat(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ self, int repeat) {
            if (repeat < 0) {
                throw RubyExceptions.CreateArgumentError("negative argument");
            }

            IList result = CreateResultArray(allocateStorage, self);
            RubyArray array = result as RubyArray;
            if (array != null) {
                array.AddCapacity(self.Count * repeat);
            }
            
            for (int i = 0; i < repeat; ++i) {
                AddRange(result, self);
            }

            allocateStorage.Context.TaintObjectBy(result, self);
            return result;
        }

        [RubyMethod("*")]
        public static MutableString Repeat(JoinConversionStorage/*!*/ conversions, IList/*!*/ self, [NotNull]MutableString/*!*/ separator) {
            return Join(conversions, self, separator);
        }

        [RubyMethod("*")]
        public static object Repeat(UnaryOpStorage/*!*/ allocateStorage, JoinConversionStorage/*!*/ conversions,
            IList/*!*/ self, [DefaultProtocol, NotNull]Union<MutableString, int> repeat) {

            if (repeat.IsFixnum()) {
                return Repeat(allocateStorage, self, repeat.Fixnum());
            } else {
                return Repeat(conversions, self, repeat.String());
            }
        }

        [RubyMethod("+")]
        public static RubyArray/*!*/ Concatenate(IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {
            RubyArray result = new RubyArray(self.Count + other.Count);
            AddRange(result, self);
            AddRange(result, other);
            return result;
        }

        [RubyMethod("concat")]
        public static IList/*!*/ Concat(IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {
            AddRange(self, other);
            return self;
        }

        [RubyMethod("-")]
        public static RubyArray/*!*/ Difference(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage, 
            IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {

            RubyArray result = new RubyArray();
            
            // cost: (|self| + |other|) * (hash + eql) + dict
            var remove = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
            bool removeNull = false;
            foreach (var item in other) {
                if (item != null) {
                    remove[item] = true;
                } else {
                    removeNull = true;
                }
            }

            foreach (var item in self) {
                if (!(item != null ? remove.ContainsKey(item) : removeNull)) {
                    result.Add(item);
                }
            }

            return result;
        }

        internal static int IndexOf(CallSite<Func<CallSite, object, object, object>>/*!*/ equalitySite, IList/*!*/ self, object item) {
            for (int i = 0; i < self.Count; i++) {
                if (Protocols.IsTrue(equalitySite.Target(equalitySite, item, self[i]))) {
                    return i;
                }
            }
            return -1;
        }

        [RubyMethod("product")]
        public static RubyArray/*!*/ Product(IList/*!*/ self, [DefaultProtocol, NotNullItems]params IList/*!*/[]/*!*/ arrays) {
            var result = new RubyArray();
            
            if (self.Count == 0) {
                return result;
            }
            for (int i = 0; i < arrays.Length; i++) {
                if (arrays[i].Count == 0) {
                    return result;
                }
            }
            
            int[] indices = new int[1 + arrays.Length];
            while (true) {
                var current = new RubyArray(indices.Length);
                for (int i = 0; i < indices.Length; i++) {
                    current[i] = GetNth(i, self, arrays)[indices[i]];
                }
                result.Add(current);

                // increment indices:
                for (int i = indices.Length - 1; i >= 0; i--) {
                    int newIndex = indices[i] + 1;
                    if (newIndex < GetNth(i, self, arrays).Count) {
                        indices[i] = newIndex;
                        break;
                    } else if (i > 0) {
                        indices[i] = 0;
                    } else {
                        return result;
                    }
                }
            }
        }

        private static IList GetNth(int n, IList first, IList[] items) {
            return (n == 0) ? first : items[n - 1];
        }

        #endregion

        #region ==, <=>, eql?, hash

        [RubyMethod("==")]
        public static bool Equals(RespondToStorage/*!*/ respondTo, BinaryOpStorage/*!*/ equals, IList/*!*/ self, object other) {
            return Protocols.RespondTo(respondTo, other, "to_ary") && Protocols.IsEqual(equals, other, self);
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

        [RubyMethod("==")]
        public static bool Equals(BinaryOpStorage/*!*/ equals, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            Assert.NotNull(self, other);

            if (ReferenceEquals(self, other)) {
                return true;
            }

            if (self.Count != other.Count) {
                return false;
            }

            using (IDisposable handleSelf = _EqualsTracker.TrackObject(self), handleOther = _EqualsTracker.TrackObject(other)) {
                if (handleSelf == null && handleOther == null) {
                    // both arrays went recursive:
                    return true;
                }

                var site = equals.GetCallSite("==");
                for (int i = 0; i < self.Count; ++i) {
                    if (!Protocols.IsEqual(site, self[i], other[i])) {
                        return false;
                    }
                }
            }

            return true;
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _ComparisonTracker = new RubyUtils.RecursionTracker();

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, ConversionStorage<IList>/*!*/ toAry, IList/*!*/ self, object other) {
            IList otherArray = Protocols.TryCastToArray(toAry, other);
            return (otherArray != null) ? Compare(comparisonStorage, self, otherArray) : null;
        }

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            using (IDisposable handleSelf = _ComparisonTracker.TrackObject(self), handleOther = _ComparisonTracker.TrackObject(other)) {
                if (handleSelf == null && handleOther == null) {
                    // both arrays went recursive:
                    return ScriptingRuntimeHelpers.Int32ToObject(0);
                }

                int limit = Math.Min(self.Count, other.Count);
                var compare = comparisonStorage.GetCallSite("<=>");

                for (int i = 0; i < limit; i++) {
                    object result = compare.Target(compare, self[i], other[i]);
                    if (!(result is int) || (int)result != 0) {
                        return result;
                    }
                }

                return ScriptingRuntimeHelpers.Int32ToObject(Math.Sign(self.Count - other.Count));
            }
        }

        [RubyMethod("eql?")]
        public static bool HashEquals(BinaryOpStorage/*!*/ eqlStorage, IList/*!*/ self, object other) {
            return RubyArray.Equals(eqlStorage, self, other);
        }

        [RubyMethod("hash")]
        public static int GetHashCode(UnaryOpStorage/*!*/ hashStorage, ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self) {
            return RubyArray.GetHashCode(hashStorage, fixnumCast, self);
        }

        #endregion

        #region slice, [], at

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static object GetElement(IList/*!*/ self, [DefaultProtocol]int index) {
            return InRangeNormalized(self, ref index) ? self[index] : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElements(UnaryOpStorage/*!*/ allocateStorage, 
            IList/*!*/ self, [DefaultProtocol]int index, [DefaultProtocol]int count) {

            if (!NormalizeRange(self.Count, ref index, ref count)) {
                return null;
            }

            return GetResultRange(allocateStorage, self, index, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElements(ConversionStorage<int>/*!*/ fixnumCast, UnaryOpStorage/*!*/ allocateStorage, 
            IList/*!*/ self, [NotNull]Range/*!*/ range) {

            int start, count;
            if (!NormalizeRange(fixnumCast, self.Count, range, out start, out count)) {
                return null;
            }

            return count < 0 ? CreateResultArray(allocateStorage, self) : GetElements(allocateStorage, self, start, count);
        }

        [RubyMethod("at")]
        public static object At(IList/*!*/ self, [DefaultProtocol]int index) {
            return GetElement(self, index);
        }

        #endregion

        #region []=

        public static void ExpandList(IList/*!*/ list, int index) {
            int diff = index - list.Count;
            for (int i = 0; i < diff; i++) {
                list.Add(null);
            }
        }

        public static void OverwriteOrAdd(IList/*!*/ list, int index, object value) {
            if (index < list.Count) {
                list[index] = value;
            } else {
                list.Add(value);
            }
        }

        public static void DeleteItems(IList/*!*/ list, int index, int length) {
            if (index >= list.Count) {
                ExpandList(list, index);
            } else {
                // normalize for max length
                if (index + length > list.Count) {
                    length = list.Count - index;
                }

                if (length == 0) {
                    RequireNotFrozen(list);
                } else {
                    RemoveRange(list, index, length);
                }
            }
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyArray/*!*/ self, [DefaultProtocol]int index, object value) {
            index = NormalizeIndexThrowIfNegative(self, index);

            if (index >= self.Count) {
                self.AddMultiple(index + 1 - self.Count, null);
            }

            return self[index] = value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(IList/*!*/ self, [DefaultProtocol]int index, object value) {
            index = NormalizeIndexThrowIfNegative(self, index);

            if (index < self.Count) {
                self[index] = value;
            } else {
                ExpandList(self, index);
                self.Add(value);
            }
            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(ConversionStorage<IList>/*!*/ arrayTryCast, IList/*!*/ self, 
            [DefaultProtocol]int index, [DefaultProtocol]int length, object value) {
            if (length < 0) {
                throw RubyExceptions.CreateIndexError("negative length ({0})", length);
            }

            index = NormalizeIndexThrowIfNegative(self, index);

            IList valueAsList = value as IList;
            if (valueAsList == null) {
                valueAsList = Protocols.TryCastToArray(arrayTryCast, value);
            }

            if (valueAsList != null && valueAsList.Count == 0) {
                DeleteItems(self, index, length);
            } else {
                if (valueAsList == null) {
                    Insert(self, index, value);
                    
                    if (length > 0) {
                        RemoveRange(self, index + 1, Math.Min(length, self.Count - index - 1));
                    }
                } else {
                    if (value == self) {
                        var newList = new object[self.Count];
                        self.CopyTo(newList, 0);
                        valueAsList = newList;
                    }

                    ExpandList(self, index);

                    int limit = length > valueAsList.Count ? valueAsList.Count : length;

                    for (int i = 0; i < limit; i++) {
                        OverwriteOrAdd(self, index + i, valueAsList[i]);
                    }

                    if (length < valueAsList.Count) {
                        InsertRange(self, index + limit, valueAsList, limit, valueAsList.Count - limit);
                    } else {
                        RemoveRange(self, index + limit, Math.Min(length - valueAsList.Count, self.Count - (index + limit)));
                    }
                }
            }

            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(ConversionStorage<IList>/*!*/ arrayTryCast, ConversionStorage<int>/*!*/ fixnumCast, 
            IList/*!*/ self, [NotNull]Range/*!*/ range, object value) {

            int start, count;
            RangeToStartAndCount(fixnumCast, range, self.Count, out start, out count);
            return SetElement(arrayTryCast, self, start, count, value);
        }

        private static void RangeToStartAndCount(ConversionStorage<int>/*!*/ fixnumCast, Range/*!*/ range, int length, out int start, out int count) {
            start = Protocols.CastToFixnum(fixnumCast, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, range.End);

            start = start < 0 ? start + length : start;
            if (start < 0) {
                throw RubyExceptions.CreateRangeError("{0}..{1} out of range", start, end);
            }

            end = end < 0 ? end + length : end;
            count = Math.Max(range.ExcludeEnd ? end - start : end - start + 1, 0);
        }

        #endregion

        #region &, |

        [RubyMethod("&")]
        public static RubyArray/*!*/ Intersection(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage, 
            IList/*!*/ self, [DefaultProtocol]IList/*!*/ other) {
            Dictionary<object, bool> items = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
            RubyArray result = new RubyArray();

            // first get the items in the RHS
            foreach (object item in other) {
                items[item] = true;
            }

            // now, go through the items in the LHS, adding ones that were also in the RHS
            // this ensures that we return the items in the correct order
            foreach (object item in self) {
                if (items.Remove(item)) {
                    result.Add(item);
                    if (items.Count == 0) {
                        break; // all done
                    }
                }
            }

            return result;
        }

        private static void AddUniqueItems(IList/*!*/ list, IList/*!*/ result, Dictionary<object, bool> seen, ref bool nilSeen) {
            foreach (object item in list) {
                if (item == null) {
                    if (!nilSeen) {
                        nilSeen = true;
                        result.Add(null);
                    }
                    continue;
                }

                if (!seen.ContainsKey(item)) {
                    seen.Add(item, true);
                    result.Add(item);
                }
            }
        }

        [RubyMethod("|")]
        public static RubyArray/*!*/ Union(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage, 
            IList/*!*/ self, [DefaultProtocol]IList other) {
            var seen = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
            bool nilSeen = false;
            var result = new RubyArray();

            // Union merges the two arrays, removing duplicates
            AddUniqueItems(self, result, seen, ref nilSeen);

            AddUniqueItems(other, result, seen, ref nilSeen);

            return result;
        }

        #endregion

        #region assoc, rassoc

        public static IList GetContainerOf(BinaryOpStorage/*!*/ equals, IList list, int index, object item) {
            foreach (object current in list) {
                IList subArray = current as IList;
                if (subArray != null && subArray.Count > index) {
                    if (Protocols.IsEqual(equals, subArray[index], item)) {
                        return subArray;
                    }
                }
            }
            return null;
        }

        [RubyMethod("assoc")]
        public static IList GetContainerOfFirstItem(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            return GetContainerOf(equals, self, 0, item);
        }

        [RubyMethod("rassoc")]
        public static IList/*!*/ GetContainerOfSecondItem(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            return GetContainerOf(equals, self, 1, item);
        }

        #endregion

        #region collect!, map!, compact, compact!

        [RubyMethod("collect!")]
        [RubyMethod("map!")]
        public static object CollectInPlace(BlockParam collector, IList/*!*/ self) {
            return (collector != null) ? CollectInPlaceImpl(collector, self) : new Enumerator((_, block) => CollectInPlaceImpl(block, self));
        }

        private static object CollectInPlaceImpl(BlockParam/*!*/ collector, IList/*!*/ self) {
            int i = 0;
            while (i < self.Count) {
                object result;
                if (collector.Yield(self[i], out result)) {
                    return result;
                }
                self[i] = result;
                i++;
            }

            return self;
        }

        [RubyMethod("compact")]
        public static IList/*!*/ Compact(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);

            foreach (object item in self) {
                if (item != null) {
                    result.Add(item);
                }
            }

            allocateStorage.Context.TaintObjectBy(result, self);

            return result;
        }

        [RubyMethod("compact!")]
        public static IList CompactInPlace(IList/*!*/ self) {
            RequireNotFrozen(self);

            bool changed = false;
            int i = 0;
            while (i < self.Count) {
                if (self[i] == null) {
                    changed = true;
                    self.RemoveAt(i);
                } else {
                    i++;
                }
            }
            return changed ? self : null;
        }

        #endregion

        #region delete, delete_at, reject, reject!

        public static bool Remove(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            int i = 0;
            bool removed = false;
            while (i < self.Count) {
                if (Protocols.IsEqual(equals, self[i], item)) {
                    self.RemoveAt(i);
                    removed = true;
                } else {
                    i++;
                }
            }
            return removed;
        }

        [RubyMethod("delete")]
        public static object Delete(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            return Remove(equals, self, item) ? item : null;
        }

        [RubyMethod("delete")]
        public static object Delete(BinaryOpStorage/*!*/ equals, BlockParam block, IList/*!*/ self, object item) {
            bool removed = Remove(equals, self, item);

            if (!removed && block != null) {
                object result;
                block.Yield(out result);
                return result;
            }
            return removed ? item : null;
        }

        [RubyMethod("delete_at")]
        public static object DeleteAt(IList/*!*/ self, [DefaultProtocol]int index) {
            index = index < 0 ? index + self.Count : index;
            if (index < 0 || index >= self.Count) {
                return null;
            }

            object result = GetElement(self, index);
            self.RemoveAt(index);
            return result;
        }

        [RubyMethod("delete_if")]
        public static object DeleteIf(BlockParam block, IList/*!*/ self) {
            return (block != null) ? DeleteIfImpl(block, self) : new Enumerator((_, innerBlock) => DeleteIfImpl(innerBlock, self));
        }

        private static object DeleteIfImpl(BlockParam/*!*/ block, IList/*!*/ self) {
            bool changed, jumped;
            DeleteIf(block, self, out changed, out jumped);
            return self;
        }

        [RubyMethod("reject!")]
        public static object RejectInPlace(BlockParam block, IList/*!*/ self) {
            return (block != null) ? RejectInPlaceImpl(block, self) : new Enumerator((_, innerBlock) => RejectInPlaceImpl(innerBlock, self));
        }

        private static object RejectInPlaceImpl(BlockParam/*!*/ block, IList/*!*/ self) {
            bool changed, jumped;
            object result = DeleteIf(block, self, out changed, out jumped);
            return jumped ? result : changed ? self : null;
        }

        [RubyMethod("reject")]
        public static object Reject(CallSiteStorage<EachSite>/*!*/ each, UnaryOpStorage/*!*/ allocate,
            BlockParam predicate, IList/*!*/ self) {
            return (predicate != null) ? RejectImpl(each, allocate, predicate, self) : new Enumerator((_, block) => RejectImpl(each, allocate, block, self));
        }

        private static object RejectImpl(CallSiteStorage<EachSite>/*!*/ each, UnaryOpStorage/*!*/ allocate, 
            BlockParam/*!*/ predicate, IList/*!*/ self) {

            IList result = CreateResultArray(allocate, self);

            for (int i = 0; i < self.Count; i++) {
                object item = self[i];
                object blockResult;
                if (predicate.Yield(item, out blockResult)) {
                    return blockResult;
                }

                if (RubyOps.IsFalse(blockResult)) {
                    result.Add(item);
                }
            }

            return result;
        }

        private static object DeleteIf(BlockParam/*!*/ block, IList/*!*/ self, out bool changed, out bool jumped) {
            Assert.NotNull(block, self);

            changed = false;
            jumped = false;

            RequireNotFrozen(self);
            
            // TODO: if block jumps the array is not modified:
            int i = 0;
            while (i < self.Count) {
                object result;
                if (block.Yield(self[i], out result)) {
                    jumped = true;
                    return result;
                }

                if (RubyOps.IsTrue(result)) {
                    changed = true;
                    self.RemoveAt(i);
                } else {
                    i++;
                }
            }
            return null;
        }

        #endregion

        #region each, each_index, reverse_each

        [RubyMethod("each")]
        public static Enumerator/*!*/ Each(IList/*!*/ self) {
            return new Enumerator((_, block) => Each(block, self));
        }

        [RubyMethod("each")]
        public static object Each([NotNull]BlockParam/*!*/ block, IList/*!*/ self) {
            for (int i = 0; i < self.Count; i++) {
                object result;
                if (block.Yield(self[i], out result)) {
                    return result;
                }
            }
            return self;
        }

        [RubyMethod("each_index")]
        public static Enumerator/*!*/ EachIndex(IList/*!*/ self) {
            return new Enumerator((_, block) => EachIndex(block, self));
        }
        
        [RubyMethod("each_index")]
        public static object EachIndex([NotNull]BlockParam/*!*/ block, IList/*!*/ self) {
            int i = 0;
            while (i < self.Count) {
                object result;
                if (block.Yield(ScriptingRuntimeHelpers.Int32ToObject(i), out result)) {
                    return result;
                }
                i++;
            }

            return self;
        }

        [RubyMethod("reverse_each")]
        public static Enumerator/*!*/ ReverseEach(RubyArray/*!*/ self) {
            return new Enumerator((_, block) => ReverseEach(block, self));
        }

        [RubyMethod("reverse_each")]
        public static object ReverseEach([NotNull]BlockParam/*!*/ block, RubyArray/*!*/ self) {
            foreach (int index in IListOps.ReverseEnumerateIndexes(self)) {
                object result;
                if (block.Yield(self[index], out result)) {
                    return result;
                }
            }
            return self;
        }

        #endregion

        #region fetch

        [RubyMethod("fetch")]
        public static object Fetch(
            ConversionStorage<int>/*!*/ fixnumCast, 
            BlockParam outOfRangeValueProvider, 
            IList/*!*/ list, 
            object/*!*/ index, 
            [Optional]object defaultValue) {

            int convertedIndex = Protocols.CastToFixnum(fixnumCast, index);

            if (InRangeNormalized(list, ref convertedIndex)) {
                return list[convertedIndex];
            }

            if (outOfRangeValueProvider != null) {
                if (defaultValue != Missing.Value) {
                    fixnumCast.Context.ReportWarning("block supersedes default value argument");
                }

                object result;
                outOfRangeValueProvider.Yield(index, out result);
                return result;
            }

            if (defaultValue == Missing.Value) {
                throw RubyExceptions.CreateIndexError("index {0} out of array", convertedIndex);
            }
            return defaultValue;
        }

        #endregion

        #region fill

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(IList/*!*/ self, object obj, [DefaultParameterValue(0)]int start) {
            // Note: Array#fill(obj, start) is not equivalent to Array#fill(obj, start, 0)
            // (as per MRI behavior, the latter can expand the array if start > length, but the former doesn't)
            start = Math.Max(0, NormalizeIndex(self, start));

            for (int i = start; i < self.Count; i++) {
                self[i] = obj;
            }
            return self;
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(IList/*!*/ self, object obj, int start, int length) {
            // Note: Array#fill(obj, start) is not equivalent to Array#fill(obj, start, 0)
            // (as per MRI behavior, the latter can expand the array if start > length, but the former doesn't)
            start = Math.Max(0, NormalizeIndex(self, start));

            ExpandList(self, Math.Min(start, start + length));

            for (int i = 0; i < length; i++) {
                OverwriteOrAdd(self, start + i, obj);
            }

            return self;
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self, object obj, object start, [DefaultParameterValue(null)]object length) {
            int startFixnum = (start == null) ? 0 : Protocols.CastToFixnum(fixnumCast, start);
            if (length == null) {
                return Fill(self, obj, startFixnum);
            } else {
                return Fill(self, obj, startFixnum, Protocols.CastToFixnum(fixnumCast, length));
            }
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self, object obj, [NotNull]Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(self, obj, begin, length);
        }

        [RubyMethod("fill")]
        public static object Fill([NotNull]BlockParam/*!*/ block, IList/*!*/ self, [DefaultParameterValue(0)]int start) {
            start = Math.Max(0, NormalizeIndex(self, start));

            for (int i = start; i < self.Count; i++) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                self[i] = result;
            }
            return self;
        }

        [RubyMethod("fill")]
        public static object Fill([NotNull]BlockParam/*!*/ block, IList/*!*/ self, int start, int length) {
            start = Math.Max(0, NormalizeIndex(self, start));

            ExpandList(self, Math.Min(start, start + length));

            for (int i = start; i < start + length; i++) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                OverwriteOrAdd(self, i, result);
            }

            return self;
        }

        [RubyMethod("fill")]
        public static object Fill(ConversionStorage<int>/*!*/ fixnumCast, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, object start, [DefaultParameterValue(null)]object length) {
            int startFixnum = (start == null) ? 0 : Protocols.CastToFixnum(fixnumCast, start);
            if (length == null) {
                return Fill(block, self, startFixnum);
            } else {
                return Fill(block, self, startFixnum, Protocols.CastToFixnum(fixnumCast, length));
            }
        }

        [RubyMethod("fill")]
        public static object Fill(ConversionStorage<int>/*!*/ fixnumCast, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, [NotNull]Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(block, self, begin, length);
        }

        #endregion

        #region first, last

        [RubyMethod("first")]
        public static object First(IList/*!*/ self) {
            return self.Count == 0 ? null : self[0];
        }

        [RubyMethod("first")]
        public static IList/*!*/ First(IList/*!*/ self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            if (count > self.Count) {
                count = self.Count;
            }
            return new RubyArray(self, 0, count);
        }

        [RubyMethod("last")]
        public static object Last(IList/*!*/ self) {
            return self.Count == 0 ? null : self[self.Count - 1];
        }

        [RubyMethod("last")]
        public static IList/*!*/ Last(IList/*!*/ self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            if (count > self.Count) {
                count = self.Count;
            }
            return new RubyArray(self, self.Count - count, count);
        }

        #endregion

        #region flatten, flatten!

        private static int IndexOfList(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ list, int start, out IList listItem) {
            for (int i = start; i < list.Count; i++) {
                listItem = Protocols.TryCastToArray(tryToAry, list[i]);
                if (listItem != null) {
                    return i;
                }
            }
            listItem = null;
            return -1;
        }

        /// <summary>
        /// Enumerates all items of the list recursively - if there are any items convertible to IList the items of that lists are enumerated as well.
        /// Returns null if there are no nested lists and so the list can be enumerated using a standard enumerator.
        /// </summary>
        public static IEnumerable<object> EnumerateRecursively(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ list, int maxDepth, Func<IList, object>/*!*/ loopDetected) {
            if (maxDepth == 0) {
                return null;
            }
            
            IList nested;
            int nestedIndex = IndexOfList(tryToAry, list, 0, out nested);

            if (nestedIndex == -1) {
                return null;
            }

            return EnumerateRecursively(tryToAry, list, list, nested, nestedIndex, maxDepth, loopDetected);
        }

        private static IEnumerable<object>/*!*/ EnumerateRecursively(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ root, 
            IList/*!*/ list, IList/*!*/ nested, int nestedIndex, int maxDepth, Func<IList, object>/*!*/ loopDetected) {

            Debug.Assert(nested != null);
            Debug.Assert(nestedIndex != -1);

            if (maxDepth < 0) {
                maxDepth = Int32.MaxValue;
            }

            var worklist = new Stack<KeyValuePair<IList, int>>();
            var recursionPath = new HashSet<object>(ReferenceEqualityComparer<object>.Instance);
            recursionPath.Add(root);
            int start = 0;

            while (true) {
                // "list" is the list being visited by the current work item (there might be more work items visiting the same list)
                // "nestedIndex" is the index of "nested" in the "list"

                if (nestedIndex >= 0) {
                    // push a work item that will process the items following the nested list:
                    worklist.Push(new KeyValuePair<IList, int>(list, nestedIndex + 1));
                    
                    // yield items preceding the nested list:
                    for (int i = start; i < nestedIndex; i++) {
                        yield return list[i];
                    }

                    // push a workitem for the nested list:
                    worklist.Push(new KeyValuePair<IList, int>(nested, 0));
                } else {
                    // there is no nested list => yield all remaining items:
                    for (int i = start; i < list.Count; i++) {
                        yield return list[i];
                    }
                }

            next:
                if (worklist.Count == 0) {
                    break;
                }

                var workitem = worklist.Pop();
                list = workitem.Key;
                start = workitem.Value;

                // finishing nested list:
                if (start == list.Count) {
                    recursionPath.Remove(list);
                    goto next;
                }

                // starting nested list:
                if (start == 0 && recursionPath.Contains(list)) {
                    yield return loopDetected(list);
                    goto next;
                }

                // set the index to -1 if we would go deeper then we should:
                nestedIndex = (recursionPath.Count < maxDepth) ? IndexOfList(tryToAry, list, start, out nested) : -1;

                // starting nested list:
                if (start == 0 && nestedIndex != -1) {
                    recursionPath.Add(list);
                }
            }
        }

        internal static IList/*!*/ Flatten(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ list, int maxDepth, IList/*!*/ result) {
            var recEnum = EnumerateRecursively(tryToAry, list, maxDepth, (_) => { 
                throw RubyExceptions.CreateArgumentError("tried to flatten recursive array"); 
            });

            if (recEnum != null) {
                foreach (var item in recEnum) {
                    result.Add(item);
                }
            } else {
                AddRange(result, list);
            }

            return result;
        }

        [RubyMethod("flatten")]
        public static IList/*!*/ Flatten(UnaryOpStorage/*!*/ allocateStorage, ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ self, 
            [DefaultProtocol, DefaultParameterValue(-1)] int maxDepth) {

            return Flatten(tryToAry, self, maxDepth, CreateResultArray(allocateStorage, self));
        }

        [RubyMethod("flatten!")]
        public static IList FlattenInPlace(ConversionStorage<IList>/*!*/ tryToAry, RubyArray/*!*/ self, [DefaultProtocol, DefaultParameterValue(-1)] int maxDepth) {
            self.RequireNotFrozen();
            return FlattenInPlace(tryToAry, (IList)self, maxDepth);
        }
        
        [RubyMethod("flatten!")]
        public static IList FlattenInPlace(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ self, [DefaultProtocol, DefaultParameterValue(-1)] int maxDepth) {
            if (maxDepth == 0) {
                return null;
            }
            
            IList nested;
            int nestedIndex = IndexOfList(tryToAry, self, 0, out nested);

            if (nestedIndex == -1) {
                return null;
            }

            var remaining = new object[self.Count - nestedIndex];
            for (int i = 0, j = nestedIndex; i < remaining.Length; i++) {
                remaining[i] = self[j++];
            }

            bool isRecursive = false;
            var recEnum = EnumerateRecursively(tryToAry, self, remaining, nested, 0, maxDepth, (rec) => {
                isRecursive = true;
                return rec;
            });

            // rewrite items following the first nested list (including the list):
            int itemCount = nestedIndex;
            foreach (var item in recEnum) {
                if (itemCount < self.Count) {
                    self[itemCount] = item;
                } else {
                    self.Add(item);
                }
                itemCount++;
            }

            // empty arrays can make the list shrink:
            while (self.Count > itemCount) {
                self.RemoveAt(self.Count - 1);
            }

            if (isRecursive) {
                throw RubyExceptions.CreateArgumentError("tried to flatten recursive array");
            }
            return self;
        }

        #endregion

        #region include?, find_index/index, rindex

        [RubyMethod("include?")]
        public static bool Include(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            return FindIndex(equals, null, self, item) != null;
        }

        [RubyMethod("find_index")]
        [RubyMethod("index")]
        public static Enumerator/*!*/ GetFindIndexEnumerator(BlockParam predicate, IList/*!*/ self) {
            Debug.Assert(predicate == null);
            throw new NotImplementedError("TODO: find_index enumerator");
        }

        [RubyMethod("find_index")]
        [RubyMethod("index")]
        public static object FindIndex([NotNull]BlockParam/*!*/ predicate, IList/*!*/ self) {
            for (int i = 0; i < self.Count; i++) {
                object blockResult;
                if (predicate.Yield(self[i], out blockResult)) {
                    return blockResult;
                }
                
                if (Protocols.IsTrue(blockResult)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(i);
                }
            }
            return null;
        }

        [RubyMethod("find_index")]
        [RubyMethod("index")]
        public static object FindIndex(BinaryOpStorage/*!*/ equals, BlockParam predicate, IList/*!*/ self, object value) {
            if (predicate != null) {
                equals.Context.ReportWarning("given block not used");
            }

            for (int i = 0; i < self.Count; i++) {
                if (Protocols.IsEqual(equals, self[i], value)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(i);
                }
            }

            return null;
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex([NotNull]BlockParam/*!*/ predicate, IList/*!*/ self) {
            foreach (int i in IListOps.ReverseEnumerateIndexes(self)) {
                object blockResult;
                if (predicate.Yield(self[i], out blockResult)) {
                    return blockResult;
                }

                if (Protocols.IsTrue(blockResult)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(i);
                }
            }
            return null;
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(BinaryOpStorage/*!*/ equals, BlockParam predicate, IList/*!*/ self, object item) {
            if (predicate != null) {
                equals.Context.ReportWarning("given block not used");
            } 
            
            foreach (int i in IListOps.ReverseEnumerateIndexes(self)) {
                if (Protocols.IsEqual(equals, self[i], item)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(i);
                }
            }
            return null;
        }

        #endregion

        #region indexes, indices, values_at

        [RubyMethod("indexes")]
        [RubyMethod("indices")]
        public static object Indexes(ConversionStorage<int>/*!*/ fixnumCast,
            UnaryOpStorage/*!*/ allocateStorage,
            IList/*!*/ self, params object[]/*!*/ values) {
            fixnumCast.Context.ReportWarning("Array#indexes and Array#indices are deprecated; use Array#values_at");

            RubyArray result = new RubyArray();

            for (int i = 0; i < values.Length; ++i) {
                Range range = values[i] as Range;
                if (range != null) {
                    IList fragment = GetElements(fixnumCast, allocateStorage, self, range);
                    if (fragment != null) {
                        result.Add(fragment);
                    }
                } else {
                    result.Add(GetElement(self, Protocols.CastToFixnum(fixnumCast, values[i])));
                }
            }

            return result;

        }

        [RubyMethod("values_at")]
        public static RubyArray/*!*/ ValuesAt(ConversionStorage<int>/*!*/ fixnumCast,
            UnaryOpStorage/*!*/ allocateStorage,
            IList/*!*/ self, params object[]/*!*/ values) {
            RubyArray result = new RubyArray();

            for (int i = 0; i < values.Length; i++) {
                Range range = values[i] as Range;
                if (range != null) {
                    int start, count;
                    if (!NormalizeRange(fixnumCast, self.Count, range, out start, out count)) {
                        continue;
                    }

                    if (count > 0) {
                        result.AddRange(GetElements(allocateStorage, self, start, count));
                        if (start + count >= self.Count) {
                            result.Add(null);
                        }
                    }
                } else {
                    result.Add(GetElement(self, Protocols.CastToFixnum(fixnumCast, values[i])));
                }
            }

            return result;
        }

        #endregion

        #region join, to_s, inspect
        
        private static void JoinRecursive(JoinConversionStorage/*!*/ conversions, IList/*!*/ list, List<MutableString/*!*/>/*!*/ parts, 
            ref bool? isBinary, ref Dictionary<object, bool> seen) {

            foreach (object item in list) {
                if (item == null) {
                    parts.Add(null);
                    continue;
                }

                IList listItem = conversions.ToAry.Target(conversions.ToAry, item);
                if (listItem != null) {
                    bool _;
                    if (ReferenceEquals(listItem, list) || seen != null && seen.TryGetValue(listItem, out _)) {
                        throw RubyExceptions.CreateArgumentError("recursive array join");
                    }

                    if (seen == null) {
                        seen = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
                    }

                    seen.Add(listItem, true);
                    JoinRecursive(conversions, listItem, parts, ref isBinary, ref seen);
                    seen.Remove(listItem);
                    continue;
                }

                // try to_str first, then to_s:
                MutableString strItem = conversions.ToStr.Target(conversions.ToStr, item) ?? conversions.ToS.Target(conversions.ToS, item);
                parts.Add(strItem);
                isBinary = isBinary.HasValue ? (isBinary | strItem.IsBinary) : strItem.IsBinary;
            }
        }

        public static MutableString/*!*/ Join(JoinConversionStorage/*!*/ conversions, IList/*!*/ self, MutableString separator) {
            var parts = new List<MutableString>(self.Count);
            bool? isBinary = (separator != null) ? separator.IsBinary : (bool?)null;
            Dictionary<object, bool> seen = null;
            
            // build a list of strings to join:
            JoinRecursive(conversions, self, parts, ref isBinary, ref seen);
            if (parts.Count == 0) {
                return MutableString.CreateEmpty();
            }

            if (separator != null && separator.IsBinary != isBinary && !separator.IsAscii()) {
                isBinary = true;
            }

            // calculate length:
            MutableString any = separator;
            int length = (separator != null) ? (isBinary.HasValue && isBinary.Value ? separator.GetByteCount() : separator.GetCharCount()) * (parts.Count - 1) : 0;
            for (int i = 0, n = parts.Count; i < n; i++) {
                var part = parts[i];
                if (part != null) {
                    length += (isBinary.HasValue && isBinary.Value) ? part.GetByteCount() : part.GetCharCount();
                    if (any == null) {
                        any = part;
                    }
                }
            }

            if (any == null) {
                return MutableString.CreateEmpty();
            }

            var result = isBinary.HasValue && isBinary.Value ? 
                MutableString.CreateBinary(length, any.Encoding) :
                MutableString.CreateMutable(length, any.Encoding);

            for (int i = 0, n = parts.Count; i < n; i++) {
                var part = parts[i];

                if (separator != null && i > 0) {
                    result.Append(separator);
                }

                if (part != null) {
                    result.Append(part);
                    result.TaintBy(part);
                }
            }

            if (separator != null) {
                result.TaintBy(separator);
            }
            if (!result.IsTainted || !result.IsUntrusted) {
                result.TaintBy(self, conversions.Context);
            }
            return result;
        }

        [RubyMethod("join")]
        public static MutableString/*!*/ Join(JoinConversionStorage/*!*/ conversions, IList/*!*/ self) {
            return Join(conversions, self, conversions.Context.ItemSeparator);
        }

        [RubyMethod("join")]
        public static MutableString/*!*/ JoinWithLazySeparatorConversion(
            JoinConversionStorage/*!*/ conversions, 
            ConversionStorage<MutableString>/*!*/ toStr,
            IList/*!*/ self, object separator) {

            if (self.Count == 0) {
                return MutableString.CreateEmpty();
            }

            return Join(conversions, self, separator != null ? Protocols.CastToString(toStr, separator) : null);
        }

        [RubyMethod("to_s")]
        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, IList/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.CreateAscii("[...]");
                }
                MutableString str = MutableString.CreateMutable(RubyEncoding.Binary);
                str.Append('[');
                bool first = true;
                foreach (object obj in self) {
                    if (first) {
                        first = false;
                    } else {
                        str.Append(", ");
                    }
                    str.Append(context.Inspect(obj));
                }
                str.Append(']');
                return str;
            }
        }

        #endregion

        #region length, size, empty?, nitems

        [RubyMethod("length")]
        [RubyMethod("size")]
        [RubyMethod("count")]
        public static int Length(IList/*!*/ self) {
            return self.Count;
        }

        [RubyMethod("empty?")]
        [RubyMethod("none?")]
        public static bool Empty(IList/*!*/ self) {
            return self.Count == 0;
        }

        [RubyMethod("nitems")]
        public static int NumberOfNonNilItems(IList/*!*/ self) {
            int count = 0;
            foreach (object obj in self) {
                if (obj != null) {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region insert, push, pop, shift, unshift, <<

        [RubyMethod("insert")]
        public static IList/*!*/ Insert(IList/*!*/ self, [DefaultProtocol]int index, params object[]/*!*/ args) {
            if (args.Length == 0) {
                var array = self as RubyArray;
                if (array != null) {
                    array.RequireNotFrozen();
                }
                return self;
            }

            if (index == -1) {
                AddRange(self, args);
                return self;
            }

            index = index < 0 ? index + self.Count + 1 : index;
            if (index < 0) {
                throw RubyExceptions.CreateIndexError("index {0} out of array", index);
            }

            if (index >= self.Count) {
                ExpandList(self, index);
                AddRange(self, args);
                return self;
            }

            InsertRange(self, index, args, 0, args.Length);
            return self;
        }

        [RubyMethod("push")]
        public static IList/*!*/ Push(IList/*!*/ self, params object[]/*!*/ values) {
            AddRange(self, values);
            return self;
        }

        [RubyMethod("pop")]
        public static object Pop(IList/*!*/ self) {
            if (self.Count == 0) {
                return null;
            }

            object result = self[self.Count - 1];
            self.RemoveAt(self.Count - 1);
            return result;
        }

        [RubyMethod("pop")]
        public static object Pop(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int count) {
            RequireNotFrozen(self);

            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size");
            }

            if (count == 0 || self.Count == 0) {
                return new RubyArray();
            }

            var normalizedCount = count <= self.Count ? count : self.Count;
            var index = self.Count - normalizedCount;

            var result = new RubyArray(self, index, normalizedCount);
            IListOps.RemoveRange(self, index, normalizedCount);
            return result;
        }

        [RubyMethod("shift")]
        public static object Shift(IList/*!*/ self) {
            if (self.Count == 0) {
                return null;
            }

            object result = self[0];
            self.RemoveAt(0);
            return result;
        }

        [RubyMethod("unshift")]
        public static IList/*!*/ Unshift(IList/*!*/ self, object/*!*/ arg) {
            self.Insert(0, arg);
            return self;
        }

        [RubyMethod("unshift")]
        public static IList/*!*/ Unshift(IList/*!*/ self, params object[]/*!*/ args) {
            InsertRange(self, 0, args, 0, args.Length);
            return self;
        }

        [RubyMethod("<<")]
        public static IList/*!*/ Append(IList/*!*/ self, object value) {
            self.Add(value);
            return self;
        }

        #endregion

        #region slice!

        [RubyMethod("slice!")]
        public static object SliceInPlace(IList/*!*/ self, [DefaultProtocol]int index) {
            index = index < 0 ? index + self.Count : index;
            if (index >= 0 && index < self.Count) {
                object result = self[index];
                DeleteElements(self, index, 1);
                return result;
            } else {
                return null;
            }
        }

        [RubyMethod("slice!")]
        public static IList SliceInPlace(ConversionStorage<int>/*!*/ fixnumCast, UnaryOpStorage/*!*/ allocateStorage, 
            IList/*!*/ self, [NotNull]Range/*!*/ range) {

            IList result = GetElements(fixnumCast, allocateStorage, self, range);
            int start, count;
            RangeToStartAndCount(fixnumCast, range, self.Count, out start, out count);
            DeleteElements(self, start, count);
            return result;
        }

        [RubyMethod("slice!")]
        public static IList SliceInPlace(UnaryOpStorage/*!*/ allocateStorage, 
            IList/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {

            IList result = GetElements(allocateStorage, self, start, length);
            DeleteElements(self, start, length);
            return result;
        }

        private static void DeleteElements(IList/*!*/ self, int start, int count) {
            if (count < 0) {
                throw RubyExceptions.CreateIndexError("negative length ({0})", count);
            }

            DeleteItems(self, NormalizeIndexThrowIfNegative(self, start), count);
        }

        #endregion

        #region sort, sort!

        [RubyMethod("sort")]
        public static object Sort(UnaryOpStorage/*!*/ allocateStorage, ComparisonStorage/*!*/ comparisonStorage, BlockParam block, IList/*!*/ self) {
            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            IList resultList = CreateResultArray(allocateStorage, self);
            StrongBox<object> breakResult;
            RubyArray result = ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self), out breakResult);
            if (breakResult == null) {
                Replace(resultList, result);
                return resultList;
            } else {
                return breakResult.Value;
            }
        }

        [RubyMethod("sort!")]
        public static object SortInPlace(ComparisonStorage/*!*/ comparisonStorage, BlockParam block, IList/*!*/ self) {
            // this should always call ArrayOps.SortInPlace instead
            Debug.Assert(!(self is RubyArray));

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            StrongBox<object> breakResult;
            RubyArray result = ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self), out breakResult);
            if (breakResult == null) {
                Replace(self, result);
                return self;
            } else {
                return breakResult.Value;
            }
        }

        #endregion

        #region shuffle, shuffle!

        [RubyMethod("shuffle")]
        public static IList/*!*/ Shuffle(UnaryOpStorage/*!*/ allocateStorage, RubyArray/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);
            if (self.Count == 0) {
                return result;
            }

            RubyArray array = result as RubyArray;
            if (array != null && array.Count < self.Count) {
                array.AddCapacity(self.Count - array.Count);
            }

            var generator = allocateStorage.Context.RandomNumberGenerator;

            result.Add(self[0]);
            for (int i = 1; i < self.Count; i++) {
                int j = generator.Next(i + 1);
                result.Add((j < result.Count) ? result[j] : null);
                result[j] = self[i];
            }

            return result;
        }

        [RubyMethod("shuffle!")]
        public static RubyArray/*!*/ ShuffleInPlace(RubyContext/*!*/ context, RubyArray/*!*/ self) {
            var generator = context.RandomNumberGenerator;
            for (int i = self.Count - 1; i >= 0; i--) {
                int j = generator.Next(i + 1);
                object value = self[i];
                self[i] = self[j];
                self[j] = value;
            }
            return self;
        }

        #endregion

        #region reverse, reverse!, transpose, uniq, uniq!

        [RubyMethod("reverse")]
        public static IList/*!*/ Reverse(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ self) {
            IList reversedList = CreateResultArray(allocateStorage, self);
            if (reversedList is RubyArray) {
                (reversedList as RubyArray).AddCapacity(self.Count);
            }
            for (int i = 0; i < self.Count; i++) {
                reversedList.Add(self[self.Count - i - 1]);
            }
            return reversedList;
        }

        [RubyMethod("reverse!")]
        public static IList/*!*/ InPlaceReverse(IList/*!*/ self) {
            int stop = self.Count / 2;
            int last = self.Count - 1;
            for (int i = 0; i < stop; i++) {
                int swap = last - i;
                object t = self[i];
                self[i] = self[swap];
                self[swap] = t;
            }
            return self;
        }

        [RubyMethod("transpose")]
        public static RubyArray/*!*/ Transpose(ConversionStorage<IList>/*!*/ arrayCast, IList/*!*/ self) {
            // Get the arrays. Note we need to check length as we go, so we call to_ary on all the
            // arrays we encounter before the error (if any).
            RubyArray result = new RubyArray();
            for (int i = 0; i < self.Count; i++) {
                IList list = Protocols.CastToArray(arrayCast, self[i]);

                if (i == 0) {
                    // initialize the result
                    result.AddCapacity(list.Count);
                    for (int j = 0; j < list.Count; j++) {
                        result.Add(new RubyArray());
                    }
                } else if (list.Count != result.Count) {
                    throw RubyExceptions.CreateIndexError("element size differs ({0} should be {1})", list.Count, result.Count);
                }

                // add items
                Debug.Assert(list.Count == result.Count);
                for (int j = 0; j < result.Count; j++) {
                    ((RubyArray)result[j]).Add(list[j]);
                }
            }

            return result;
        }

        [RubyMethod("uniq")]
        public static IList/*!*/ Unique(UnaryOpStorage/*!*/ allocateStorage, IList/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);

            var seen = new Dictionary<object, bool>(allocateStorage.Context.EqualityComparer);
            bool nilSeen = false;
            
            AddUniqueItems(self, result, seen, ref nilSeen);
            return result;
        }

        [RubyMethod("uniq!")]
        public static IList UniqueSelf(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage, RubyArray/*!*/ self) {
            self.RequireNotFrozen();
            return UniqueSelf(hashStorage, eqlStorage, (IList)self);
        }

        [RubyMethod("uniq!")]
        public static IList UniqueSelf(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage, IList/*!*/ self) {
            var seen = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
            bool nilSeen = false;
            bool modified = false;
            int i = 0;
            while (i < self.Count) {
                object key = self[i];
                if (key != null && !seen.ContainsKey(key)) {
                    seen.Add(key, true);
                    i++;
                } else if (key == null && !nilSeen) {
                    nilSeen = true;
                    i++;
                } else {
                    self.RemoveAt(i);
                    modified = true;
                }
            }

            return modified ? self : null;
        }

        #endregion

        #region permutation, combination

        internal sealed class PermutationEnumerator : IEnumerator {
            private struct State {
                public readonly int i, j;
                public State(int i, int j) { this.i = i; this.j = j; }
            }

            private readonly IList/*!*/ _list;
            private readonly int? _size;

            public PermutationEnumerator(IList/*!*/ list, int? size) {
                _size = size;
                _list = list;
            }

            //
            // rec = lambda do |i|
            //   # State "j < -1"
            //   if i == result.length
            //     yield result.dup
            //     return
            //   end
            //
            //   j = i
            //   while j < values.length
            //     values[j], values[i] = values[i], values[j]
            //     result[i] = values[i]      
            //     rec.(i + 1)
            //     # State "j >= 0"
            //     j += 1
            //   end
            //
            //   while j > i
            //     j -= 1
            //     values[j], values[i] = values[i], values[j] 
            //   end
            // end
            //
            // rec.(0)
            //
            public object Each(RubyScope/*!*/ scope, BlockParam/*!*/ block) {
                int size = _size ?? _list.Count;
                if (size < 0 || size > _list.Count) {
                    return _list;
                }

                var result = new object[size];
                var values = new object[_list.Count];
                _list.CopyTo(values, 0);
                var stack = new Stack<State>();
                stack.Push(new State(0, -1));

                while (stack.Count > 0) {
                    var entry = stack.Pop();
                    int i = entry.i;
                    int j = entry.j;

                    if (j < 0) {
                        if (i == result.Length) {
                            object blockResult;
                            if (block.Yield(RubyOps.MakeArrayN(result), out blockResult)) {
                                return blockResult;
                            }
                        } else {
                            result[i] = values[i];
                            stack.Push(new State(i, i));
                            stack.Push(new State(i + 1, -1));
                        }
                    } else {
                        j++;
                        if (j == values.Length) {
                            while (j > i) {
                                j--;
                                Xchg(values, i, j);
                            }
                        } else {
                            Xchg(values, i, j);
                            result[i] = values[i];
                            stack.Push(new State(i, j));
                            stack.Push(new State(i + 1, -1));
                        }
                    }
                }
                return _list;
            }

            private static void Xchg(object[]/*!*/ values, int i, int j) {
                object item = values[j];
                values[j] = values[i];
                values[i] = item;
            }
        }
        
        internal sealed class CombinationEnumerator : IEnumerator {
            private struct State {
                public readonly int i, j;
                public readonly bool init;
                public State(int i, int j, bool init) { this.i = i; this.j = j; this.init = init; }
            }

            private readonly IList/*!*/ _list;
            private readonly int? _size;

            public CombinationEnumerator(IList/*!*/ list, int? size) {
                _size = size;
                _list = list;
            }

            //   
            // rec = lambda do |i,j|
            //   # State "init"
            //   if j == result.length
            //     yield result.dup
            //     return
            //   end
            //   
            //   while i <= values.length - result.length + j
            //     result[j] = values[i]
            //     rec.(i + 1, j + 1)
            //     # State "!init"
            //     i += 1      
            //   end
            // end
            //   
            // rec.(0, 0)
            // 
            public object Each(RubyScope/*!*/ scope, BlockParam/*!*/ block) {
                int size = _size ?? _list.Count;
                if (size < 0 || size > _list.Count) {
                    return _list;
                }
                var result = new object[size];
                var values = new object[_list.Count];
                _list.CopyTo(values, 0);
                var stack = new Stack<State>();
                stack.Push(new State(0, 0, true));

                while (stack.Count > 0) {
                    var entry = stack.Pop();
                    int i = entry.i;
                    int j = entry.j;

                    if (entry.init && j == result.Length) {
                        object blockResult;
                        if (block.Yield(RubyOps.MakeArrayN(result), out blockResult)) {
                            return blockResult;
                        }
                    } else {
                        if (!entry.init) {
                            i++;
                        }
                        if (i <= values.Length - result.Length + j) {
                            result[j] = values[i];
                            stack.Push(new State(i, j, false));
                            stack.Push(new State(i + 1, j + 1, true));
                        }
                    }
                }
                return _list;
            }
        }

        [RubyMethod("permutation")]
        public static object GetPermutations(BlockParam block, IList/*!*/ self, [DefaultProtocol, Optional]int? size) {
            var enumerator = new PermutationEnumerator(self, size);
            if (block == null) {
                return new Enumerator(enumerator);
            }

            return enumerator.Each(null, block);
        }
        
        [RubyMethod("combination")]
        public static object GetCombinations(BlockParam block, IList/*!*/ self, [DefaultProtocol, Optional]int? size) {
            var enumerator = new CombinationEnumerator(self, size);
            if (block == null) {
                return new Enumerator(enumerator);
            }

            return enumerator.Each(null, block);
        }

        #endregion
    }
}
