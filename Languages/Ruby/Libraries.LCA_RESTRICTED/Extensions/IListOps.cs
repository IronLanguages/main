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

        private static IList/*!*/ GetResultRange(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ list, int index, int count) {

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
            } else if ((listOfObject = list as List<object>) != null && ((collection = items as ICollection<object>) != null)) {
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
            int count = items.Count;
            if (count <= 1) {
                if (count > 0) {
                    collection.Add(items[0]);
                }
                return;
            }

            RubyArray array = collection as RubyArray;
            if (array != null) {
                array.AddRange(items);
            } else {
                for (int i = 0; i < count; i++) {
                    collection.Add(items[i]);
                }
            }
        }

        private static IList/*!*/ CreateResultArray(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, IList/*!*/ list) {
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

        #region *, +, concat

        [RubyMethod("*")]
        public static IList/*!*/ Repetition(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, int repeat) {

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

            allocateStorage.Context.TaintObjectBy<IList>(result, self);
            return result;
        }

        [RubyMethod("*")]
        public static MutableString Repetition(ConversionStorage<MutableString>/*!*/ tosConversion, 
            IList/*!*/ self, [NotNull]MutableString/*!*/ separator) {
            return Join(tosConversion, self, separator);
        }

        [RubyMethod("*")]
        public static object Repetition(
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            ConversionStorage<MutableString>/*!*/ tosConversion, 
            IList/*!*/ self, [DefaultProtocol, NotNull]Union<MutableString, int> repeat) {

            if (repeat.IsFixnum()) {
                return Repetition(allocateStorage, self, repeat.Fixnum());
            } else {
                return Repetition(tosConversion, self, repeat.String());
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

        #endregion

        #region ==, <=>, eql?, hash

        [RubyMethod("==")]
        public static bool Equals(ConversionStorage<IList>/*!*/ arrayTryConvert, BinaryOpStorage/*!*/ equals, IList/*!*/ self, object other) {
            IList otherAsArray = Protocols.TryConvertToArray(arrayTryConvert, other);
            return otherAsArray != null ? Equals(equals, self, otherAsArray) : false;
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

        [RubyMethod("==")]
        public static bool Equals(BinaryOpStorage/*!*/ equals, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            Assert.NotNull(self, other);

            if (object.ReferenceEquals(self, other)) {
                return true;
            }

            if (self.Count != other.Count) {
                return false;
            }

            using (IDisposable handle = _EqualsTracker.TrackObject(self)) {
                if (handle == null) {
                    // hashing of recursive array
                    return false;
                }

                for (int i = 0; i < self.Count; ++i) {
                    bool result = Protocols.IsEqual(equals, self[i], other[i]);
                    if (!result) {
                        return false;
                    }
                }
            }

            return true;
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _infiniteComparisonTracker = new RubyUtils.RecursionTracker();

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {
            using (IDisposable handle = _infiniteComparisonTracker.TrackObject(self)) {
                if (handle == null) {
                    return 0;
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
        public static object GetElement(IList list, [DefaultProtocol]int index) {
            return InRangeNormalized(list, ref index) ? list[index] : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElements(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ list, [DefaultProtocol]int index, [DefaultProtocol]int count) {
            if (!NormalizeRange(list.Count, ref index, ref count)) {
                return null;
            }

            return GetResultRange(allocateStorage, list, index, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElement(ConversionStorage<int>/*!*/ fixnumCast, 
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList array, [NotNull]Range/*!*/ range) {
            int start, count;
            if (!NormalizeRange(fixnumCast, array.Count, range, out start, out count)) {
                return null;
            }

            return count < 0 ? CreateResultArray(allocateStorage, array) : GetElements(allocateStorage, array, start, count);
        }

        [RubyMethod("at")]
        public static object At(IList/*!*/ self, [DefaultProtocol]int index) {
            return GetElement(self, index);
        }

        #endregion

        #region []=

        public static void ExpandList(IList list, int index) {
            int diff = index - list.Count;
            for (int i = 0; i < diff; i++) {
                list.Add(null);
            }
        }

        public static void OverwriteOrAdd(IList list, int index, object value) {
            if (index < list.Count) {
                list[index] = value;
            } else {
                list.Add(value);
            }
        }

        public static void DeleteItems(IList list, int index, int length) {
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

            if (value == null) {
                DeleteItems(self, index, length);
                return null;
            }

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

            int begin = Protocols.CastToFixnum(fixnumCast, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, range.End);

            begin = begin < 0 ? begin + self.Count : begin;
            if (begin < 0) {
                throw RubyExceptions.CreateRangeError("{0}..{1} out of range", begin, end);
            }

            end = end < 0 ? end + self.Count : end;

            int count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return SetElement(arrayTryCast, self, begin, Math.Max(count, 0), value);
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
        public static object CollectInPlace(BlockParam block, IList/*!*/ self) {
            Assert.NotNull(self);

            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int i = 0;
            while (i < self.Count) {
                object result;
                if (block.Yield(self[i], out result)) {
                    return result;
                }
                self[i] = result;
                i++;
            }

            return self;
        }

        [RubyMethod("compact")]
        public static IList/*!*/ Compact(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, IList/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);

            foreach (object item in self) {
                if (item != null) {
                    result.Add(item);
                }
            }

            allocateStorage.Context.TaintObjectBy<IList>(result, self);

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
            bool changed, jumped;
            DeleteIf(block, self, out changed, out jumped);
            return self;
        }

        [RubyMethod("reject!")]
        public static object RejectInPlace(BlockParam block, IList/*!*/ self) {
            bool changed, jumped;
            object result = DeleteIf(block, self, out changed, out jumped);
            return jumped ? result : changed ? self : null;
        }

        [RubyMethod("reject")]
        public static object Reject(CallSiteStorage<EachSite>/*!*/ each, CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            BlockParam predicate, IList/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);

            if (predicate == null && self.Count > 0) {
                throw RubyExceptions.NoBlockGiven();
            }

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

        private static object DeleteIf(BlockParam block, IList/*!*/ self, out bool changed, out bool jumped) {
            changed = false;
            jumped = false;

            if (block == null && self.Count > 0) {
                throw RubyExceptions.NoBlockGiven();
            }

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

        #region each, each_index

        [RubyMethod("each")]
        public static object Each(BlockParam block, IList/*!*/ self) {
            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            for (int i = 0; i < self.Count; i++) {
                object result;
                if (block.Yield(self[i], out result)) {
                    return result;
                }
            }
            return self;
        }

        [RubyMethod("each_index")]
        public static object EachIndex(BlockParam block, IList/*!*/ self) {
            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int i = 0;
            while (i < self.Count) {
                object result;
                if (block.Yield(i, out result)) {
                    return result;
                }
                i++;
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
        public static IList/*!*/ Fill(ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self, object obj, Range/*!*/ range) {
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
        public static object Fill(ConversionStorage<int>/*!*/ fixnumCast, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, Range/*!*/ range) {
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
        public static IEnumerable<object> EnumerateRecursively(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ list, Func<IList, object>/*!*/ loopDetected) {
            IList nested;
            int nestedIndex = IndexOfList(tryToAry, list, 0, out nested);

            if (nestedIndex == -1) {
                return null;
            }

            return EnumerateRecursively(tryToAry, list, list, nested, nestedIndex, loopDetected);
        }

        private static IEnumerable<object>/*!*/ EnumerateRecursively(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ root, 
            IList/*!*/ list, IList nested, int nestedIndex, Func<IList, object>/*!*/ loopDetected) {

            var worklist = new Stack<KeyValuePair<IList, int>>();
            var recursionPath = new Dictionary<object, bool>(ReferenceEqualityComparer.Instance);
            int start = 0;

            while (true) {
                if (nestedIndex >= 0) {
                    // push a workitem for the items following the nested list:
                    if (nestedIndex < list.Count - 1) {
                        worklist.Push(new KeyValuePair<IList, int>(list, nestedIndex + 1));
                    }

                    // yield items preceding the nested list:
                    for (int i = start; i < nestedIndex; i++) {
                        yield return list[i];
                    }

                    // push a workitem for the nested list:
                    if (nestedIndex != -1) {
                        worklist.Push(new KeyValuePair<IList, int>(nested, 0));                        
                    }
                } else {
                    // there is no nested list => yield all remaining items:
                    for (int i = start; i < list.Count; i++) {
                        yield return list[i];
                    }
                }

                // finished nested list workitem:
                if (start == 0) {
                    recursionPath.Remove(list);
                }

            next:
                if (worklist.Count == 0) {
                    break;
                }

                var workitem = worklist.Pop();
                list = workitem.Key;
                start = workitem.Value;

                // starting nested workitem:
                if (start == 0) {
                    if (ReferenceEquals(nested, root) || recursionPath.ContainsKey(nested)) {
                        yield return loopDetected(nested);
                        goto next;
                    } else {
                        recursionPath.Add(nested, true);
                    }
                }
                
                nestedIndex = IndexOfList(tryToAry, list, start, out nested);
            }
        }

        [RubyMethod("flatten")]
        public static IList/*!*/ Flatten(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, ConversionStorage<IList>/*!*/ tryToAry, 
            IList/*!*/ self) {

            IList result = CreateResultArray(allocateStorage, self);
            var recEnum = EnumerateRecursively(tryToAry, self, (_) => { throw RubyExceptions.CreateArgumentError("tried to flatten recursive array"); });

            if (recEnum != null) {
                foreach (var item in recEnum) {
                    result.Add(item);
                }
            } else {
                AddRange(result, self);
            }

            return result;
        }

        [RubyMethod("flatten!")]
        public static IList FlattenInPlace(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ self) {
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
            var recEnum = EnumerateRecursively(tryToAry, self, remaining, nested, 0, (rec) => {
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

        #region include?, index, rindex

        [RubyMethod("include?")]
        public static bool Include(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            return Index(equals, self, item) != null;
        }

        [RubyMethod("index")]
        public static object Index(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            for (int i = 0; i < self.Count; ++i) {
                if (Protocols.IsEqual(equals, self[i], item)) {
                    return i;
                }
            }
            return null;
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            foreach (int index in IListOps.ReverseEnumerateIndexes(self)) {
                if (Protocols.IsEqual(equals, self[index], item)) {
                    return index;
                }
            }
            return null;
        }

        #endregion

        #region indexes, indices, values_at

        [RubyMethod("indexes")]
        [RubyMethod("indices")]
        public static object Indexes(ConversionStorage<int>/*!*/ fixnumCast, 
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            IList/*!*/ self, params object[]/*!*/ values) {
            fixnumCast.Context.ReportWarning("Array#indexes and Array#indices are deprecated; use Array#values_at");

            RubyArray result = new RubyArray();

            for (int i = 0; i < values.Length; ++i) {
                Range range = values[i] as Range;
                if (range != null) {
                    IList fragment = GetElement(fixnumCast, allocateStorage, self, range);
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
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
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
        
        private static void JoinRecursive(ConversionStorage<MutableString>/*!*/ tosConversion, 
            IList/*!*/ list, List<MutableString/*!*/>/*!*/ parts, 
            ref bool? isBinary, ref bool taint, ref Dictionary<object, bool> seen) {

            foreach (object item in list) {
                IList listItem;
                if ((listItem = item as IList) != null) {
                    bool _;
                    if (ReferenceEquals(listItem, list) || seen != null && seen.TryGetValue(listItem, out _)) {
                        parts.Add(RubyUtils.InfiniteRecursionMarker);
                    } else {
                        if (seen == null) {
                            seen = new Dictionary<object, bool>(ReferenceEqualityComparer.Instance);
                        }

                        seen.Add(listItem, true);
                        JoinRecursive(tosConversion, listItem, parts, ref isBinary, ref taint, ref seen);
                        seen.Remove(listItem);

                        taint |= tosConversion.Context.IsObjectTainted(listItem);
                    }
                } else if (item != null) {
                    var tosSite = tosConversion.Site;
                    if (tosSite == null) {
                        tosSite = tosConversion.GetSite(ConvertToSAction.Make(tosConversion.Context));
                    }
                    var strItem = tosSite.Target(tosSite, item);
                    parts.Add(strItem);
                    taint |= strItem.IsTainted;
                    isBinary = isBinary.HasValue ? (isBinary | strItem.IsBinary) : strItem.IsBinary;
                } else {
                    parts.Add(null);
               }
            }
        }

        public static MutableString/*!*/ Join(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self, MutableString/*!*/ separator) {
            var parts = new List<MutableString>(self.Count);
            bool partTainted = false;
            bool? isBinary = (separator != null) ? separator.IsBinary : (bool?)null;
            Dictionary<object, bool> seen = null;
            
            JoinRecursive(tosConversion, self, parts, ref isBinary, ref partTainted, ref seen);
            if (parts.Count == 0) {
                return MutableString.CreateEmpty();
            }

            if (separator != null && separator.IsBinary != isBinary && !separator.IsAscii()) {
                isBinary = true;
            }

            MutableString any = separator;
            int length = (separator != null) ? (isBinary.HasValue && isBinary.Value ? separator.GetByteCount() : separator.GetCharCount()) * (parts.Count - 1) : 0;
            foreach (MutableString part in parts) {
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

            for (int i = 0; i < parts.Count; i++) {
                if (separator != null && i > 0) {
                    result.Append(separator);
                }
                result.Append(parts[i]);
            }

            result.IsTainted |= partTainted;
            if (!result.IsTainted && (separator != null && separator.IsTainted || tosConversion.Context.IsObjectTainted(self))) {
                result.IsTainted = true;
            }
            return result;
        }

        [RubyMethod("join")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ Join(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self) {
            return Join(tosConversion, self, tosConversion.Context.ItemSeparator);
        }

        [RubyMethod("join")]
        public static MutableString/*!*/ Join(ConversionStorage<MutableString>/*!*/ tosConversion, ConversionStorage<MutableString>/*!*/ tostrConversion, 
            IList/*!*/ self, object separator) {
            if (self.Count == 0) {
                return MutableString.CreateEmpty();
            }
            return Join(tosConversion, self, separator != null ? Protocols.CastToString(tostrConversion, separator) : MutableString.FrozenEmpty);
        }

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
        public static int Length(IList/*!*/ self) {
            return self.Count;
        }

        [RubyMethod("empty?")]
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
            if (args.Length > 0) {
                InsertRange(self, 0, args, 0, args.Length);
            }
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
        public static object SliceInPlace(ConversionStorage<IList>/*!*/ arrayTryCast, IList/*!*/ self, [DefaultProtocol]int index) {
            index = index < 0 ? index + self.Count : index;
            if (index >= 0 && index < self.Count) {
                object result = self[index];
                SetElement(arrayTryCast, self, index, 1, null);
                return result;
            } else {
                return null;
            }
        }

        [RubyMethod("slice!")]
        public static object SliceInPlace(
            ConversionStorage<IList>/*!*/ arrayTryCast, 
            ConversionStorage<int>/*!*/ fixnumCast, 
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, [NotNull]Range/*!*/ range) {

            object result = GetElement(fixnumCast, allocateStorage, self, range);
            SetElement(arrayTryCast, fixnumCast, self, range, null);
            return result;
        }

        [RubyMethod("slice!")]
        public static IList/*!*/ SliceInPlace(
            ConversionStorage<IList>/*!*/ arrayTryCast, 
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {

            IList result = GetElements(allocateStorage, self, start, length);
            SetElement(arrayTryCast, self, start, length, null);
            return result;
        }

        #endregion

        #region sort, sort!

        [RubyMethod("sort")]
        public static object Sort(
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, IList/*!*/ self) {

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            IList resultList = CreateResultArray(allocateStorage, self);
            StrongBox<object> breakResult;
            RubyArray result = ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, ToArray(self), out breakResult);
            if (breakResult == null) {
                Replace(resultList, result);
                return resultList;
            } else {
                return breakResult.Value;
            }
        }

        [RubyMethod("sort!")]
        public static object SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, IList/*!*/ self) {

            // this should always call ArrayOps.SortInPlace instead
            Debug.Assert(!(self is RubyArray));

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            StrongBox<object> breakResult;
            RubyArray result = ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, ToArray(self), out breakResult);
            if (breakResult == null) {
                Replace(self, result);
                return self;
            } else {
                return breakResult.Value;
            }
        }

        #endregion

        #region reverse, reverse!, transpose, uniq, uniq!

        [RubyMethod("reverse")]
        public static IList/*!*/ Reverse(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, IList/*!*/ self) {
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
        public static IList/*!*/ Unique(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, IList/*!*/ self) {
            IList result = CreateResultArray(allocateStorage, self);

            var seen = new Dictionary<object, bool>(allocateStorage.Context.EqualityComparer);
            bool nilSeen = false;
            
            AddUniqueItems(self, result, seen, ref nilSeen);
            return result;
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

        #region zip 

        [RubyMethod("zip")]
        public static object Zip(CallSiteStorage<EachSite>/*!*/ each, ConversionStorage<IList>/*!*/ tryToAry, BlockParam block,
            object self, [DefaultProtocol, NotNullItems]params IList/*!*/[]/*!*/ args) {

            return Enumerable.Zip(each, tryToAry, block, self, args);
        }

        #endregion
    }
}
