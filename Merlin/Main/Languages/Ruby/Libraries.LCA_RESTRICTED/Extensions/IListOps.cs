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

namespace IronRuby.Builtins {
   
    [RubyModule(Extends = typeof(IList)), Includes(typeof(Enumerable))]
    public static class IListOps {
        
        #region Helpers

        internal static int NormalizeIndex(IList/*!*/ list, int index) {
            return NormalizeIndex(list.Count, index);
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

        private static IEnumerable<object>/*!*/ EnumerateRange(IList/*!*/ list, int index, int count) {
            int stop = index + count;
            for (int i = index; i < stop; i++) {
                yield return list[i];
            }
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

        private static void InsertRange(IList/*!*/ collection, int index, IEnumerable<object>/*!*/ items) {
            List<object> list = collection as List<object>;
            if (list != null) {
                list.InsertRange(index, items);
            } else {
                int i = index;
                foreach (object obj in items) {
                    collection.Insert(i++, obj);
                }
            }
        }

        private static void RemoveRange(IList/*!*/ collection, int index, int count) {
            List<object> list = collection as List<object>;
            if (list != null) {
                list.RemoveRange(index, count);
            } else {
                for (int i = index + count - 1; i >= index; i--) {
                    collection.RemoveAt(i);
                }
            }
        }

        internal static void AddRange(IList/*!*/ collection, IList/*!*/ items) {
            List<object> list = collection as List<object>;
            if (list != null) {
                list.Capacity += items.Count;
            }
            // note: "collection" could be the same as "items" so we can't use an enumerator
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                collection.Add(items[i]);
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

            throw RubyExceptions.CreateTypeError(String.Format("{0}#allocate should return IList", cls.Name));
        }

        #endregion

        #region initialize_copy, replace, clear, to_a, to_ary

        [RubyMethod("replace")]
        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static IList/*!*/ Replace(RubyContext/*!*/ context, IList/*!*/ self, [NotNull, DefaultProtocol]IList/*!*/ other) {
            RubyUtils.RequiresNotFrozen(context, self);

            self.Clear();
            AddRange(self, other);
            return self;
        }

        [RubyMethod("clear")]
        public static IList Clear(RubyContext/*!*/ context, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
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
        public static RubyArray/*!*/ Repetition(IList/*!*/ self, int repeat) {
            if (repeat < 0) {
                throw RubyExceptions.CreateArgumentError("negative argument");
            }
            RubyArray result = new RubyArray(self.Count * repeat);
            for (int i = 0; i < repeat; ++i) {
                AddRange(result, self);
            }
            return result;
        }

        [RubyMethod("*")]
        public static MutableString Repetition(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self, [NotNull]MutableString/*!*/ separator) {
            return Join(tosConversion, self, separator);
        }

        [RubyMethod("*")]
        public static object Repetition(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self, [DefaultProtocol]Union<MutableString, int> repeat) {

            if (repeat.IsFixnum()) {
                return Repetition(self, repeat.Fixnum());
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
        public static IList/*!*/ Concat(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {
            if (other.Count > 0) {
                RubyUtils.RequiresNotFrozen(context, self);
            }
            AddRange(self, other);
            return self;
        }

        [RubyMethod("-")]
        public static RubyArray/*!*/ Difference(BinaryOpStorage/*!*/ equals, IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {
            RubyArray result = new RubyArray();
        
            // TODO: optimize this
            foreach (object element in self) {
                if (!Include(equals, other, element)) {
                    result.Add(element);
                }
            }

            return result;
        }

        #endregion

        #region ==, <=>, eql?, hash

        [RubyMethod("==")]
        public static bool Equals(IList/*!*/ self, object other) {
            return false;
        }
        
        [RubyMethod("==")]
        public static bool Equals(BinaryOpStorage/*!*/ equals, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            Assert.NotNull(self, other);

            if (object.ReferenceEquals(self, other)) {
                return true;
            }

            if (self.Count != other.Count) {
                return false;
            }

            for (int i = 0; i < self.Count; ++i) {
                bool result = Protocols.IsEqual(equals, self[i], other[i]);
                if (!result) {
                    return false;
                }
            }
            return true;
        }

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, IList/*!*/ self, [DefaultProtocol, NotNull]IList/*!*/ other) {

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

        [RubyMethod("eql?")]
        public static bool HashEquals(IList/*!*/ self, object other) {
            return RubyArray.Equals(self, other);
        }

        [RubyMethod("hash")]
        public static int GetHashCode(IList/*!*/ self) {
            return RubyArray.GetHashCode(self);
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
                RemoveRange(list, index, length);
            }
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int index, object value) {
            RubyUtils.RequiresNotFrozen(context, self);

            index = NormalizeIndex(self, index);

            if (index < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of array", index));
            }

            if (index < self.Count) {
                // TODO: conversions
                self[index] = value;
            } else {
                ExpandList(self, index);
                self.Add(value);
            }
            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int index, [DefaultProtocol]int length, object value) {
            RubyUtils.RequiresNotFrozen(context, self);

            if (length < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("negative length ({0})", length));
            }

            index = NormalizeIndex(self, index);
            if (index < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of array", index));
            }

            IList valueAsList = value as IList;

            if (value == null || (valueAsList != null && valueAsList.Count == 0)) {
                DeleteItems(self, index, length);
            } else {
                if (valueAsList == null) {
                    Insert(context, self, index, value);
                    
                    if (length > 0) {
                        RemoveRange(self, index + 1, Math.Min(length, self.Count - index - 1));
                    }
                } else {
                    if (value == self) {
                        valueAsList = new object[self.Count];
                        self.CopyTo(valueAsList as object[], 0);
                    }

                    ExpandList(self, index);

                    int limit = length > valueAsList.Count ? valueAsList.Count : length;

                    for (int i = 0; i < limit; i++) {
                        OverwriteOrAdd(self, index + i, valueAsList[i]);
                    }

                    if (length < valueAsList.Count) {
                        InsertRange(self, index + limit, EnumerateRange(valueAsList, limit, valueAsList.Count - limit));
                    }
                    else {
                        RemoveRange(self, index + limit, Math.Min(length - valueAsList.Count, self.Count - (index + limit)));
                    }
                }
            }

            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self, [NotNull]Range/*!*/ range, object value) {
            RubyUtils.RequiresNotFrozen(fixnumCast.Context, self);
            
            int begin = Protocols.CastToFixnum(fixnumCast, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, range.End);

            begin = begin < 0 ? begin + self.Count : begin;
            if (begin < 0) {
                throw RubyExceptions.CreateRangeError(String.Format("{0}..{1} out of range", begin, end));
            }

            end = end < 0 ? end + self.Count : end;

            int count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return SetElement(fixnumCast.Context, self, begin, Math.Max(count, 0), value);
        }

        #endregion

        #region &, |

        [RubyMethod("&")]
        public static RubyArray/*!*/ Intersection(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]IList/*!*/ other) {
            Dictionary<object, bool> items = new Dictionary<object, bool>(context.EqualityComparer);
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

        [RubyMethod("|")]
        public static RubyArray/*!*/ Union(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]IList other) {
            var seen = new Dictionary<object, bool>(context.EqualityComparer);
            var result = new RubyArray();

            // Union merges the two arrays, removing duplicates
            foreach (object item in self) {
                if (!seen.ContainsKey(item)) {
                    seen.Add(item, true);
                    result.Add(item);
                }
            }

            foreach (object item in other) {
                if (!seen.ContainsKey(item)) {
                    seen.Add(item, true);
                    result.Add(item);
                }
            }

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
        public static object CollectInPlace(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            Assert.NotNull(context, self);

            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            RubyUtils.RequiresNotFrozen(context, self);

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

            return result;
        }

        [RubyMethod("compact!")]
        public static IList CompactInPlace(RubyContext/*!*/ context, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

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

        #region delete, delete_at

        public static bool Remove(BinaryOpStorage/*!*/ equals, IList/*!*/ self, object item) {
            int i = 0;
            bool removed = false;
            while (i < self.Count) {
                if (Protocols.IsEqual(equals, self[i], item)) {
                    RubyUtils.RequiresNotFrozen(equals.Context, self);
                    self.RemoveAt(i);
                    removed = true;
                } else {
                    ++i;
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

            if (block != null) {
                object result;
                block.Yield(out result);
                return result;
            }
            return removed ? item : null;
        }

        [RubyMethod("delete_at")]
        public static object DeleteAt(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int index) {
            RubyUtils.RequiresNotFrozen(context, self);

            index = index < 0 ? index + self.Count : index;
            if (index < 0 || index > self.Count) {
                return null;
            }

            object result = GetElement(self, index);
            self.RemoveAt(index);
            return result;
        }

        [RubyMethod("delete_if")]
        public static object DeleteIf(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            bool changed, jumped;
            DeleteIf(context, block, self, out changed, out jumped);
            return self;
        }

        private static object DeleteIf(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self, out bool changed, out bool jumped) {
            RubyUtils.RequiresNotFrozen(context, self);
            changed = false;
            jumped = false;

            if (block == null && self.Count > 0) {
                throw RubyExceptions.NoBlockGiven();
            }
            
            // TODO: if block jumpes the array is not modified:
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

        [RubyMethod("reject!")]
        public static object RejectInPlace(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            bool changed, jumped;
            object result = DeleteIf(context, block, self, out changed, out jumped);
            if (jumped) return result;
            if (changed) return self;
            return null;
        }

        #endregion

        #region each, each_index

        [RubyMethod("each")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            Assert.NotNull(context, self);

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
        public static object EachIndex(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            Assert.NotNull(context, self);

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
        public static object Fetch(RubyContext/*!*/ context, BlockParam outOfRangeValueProvider, IList/*!*/ list, [DefaultProtocol]int index, [Optional]object defaultValue) {
            int oldIndex = index;
            if (InRangeNormalized(list, ref index)) {
                return list[index];
            }

            if (outOfRangeValueProvider != null) {
                if (defaultValue != Missing.Value) {
                    context.ReportWarning("block supersedes default value argument");
                }

                object result;
                outOfRangeValueProvider.Yield(oldIndex, out result);
                return result;
            }
            
            if (defaultValue == Missing.Value) {
                throw RubyExceptions.CreateIndexError("index " + index + " out of array");
            }
            return defaultValue;
        }

        #endregion

        #region fill

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(RubyContext/*!*/ context, IList/*!*/ self, object obj, [DefaultParameterValue(0)]int start) {
            RubyUtils.RequiresNotFrozen(context, self);
            
            // Note: Array#fill(obj, start) is not equivalent to Array#fill(obj, start, 0)
            // (as per MRI behavior, the latter can expand the array if start > length, but the former doesn't)
            start = Math.Max(0, NormalizeIndex(self, start));

            for (int i = start; i < self.Count; i++) {
                self[i] = obj;
            }
            return self;
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(RubyContext/*!*/ context, IList/*!*/ self, object obj, int start, int length) {
            RubyUtils.RequiresNotFrozen(context, self);

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
                return Fill(fixnumCast.Context, self, obj, startFixnum);
            } else {
                return Fill(fixnumCast.Context, self, obj, startFixnum, Protocols.CastToFixnum(fixnumCast, length));
            }
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(ConversionStorage<int>/*!*/ fixnumCast, IList/*!*/ self, object obj, Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(fixnumCast.Context, self, obj, begin, length);
        }

        [RubyMethod("fill")]
        public static object Fill(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, [DefaultParameterValue(0)]int start) {
            RubyUtils.RequiresNotFrozen(context, self);

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
        public static object Fill(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, int start, int length) {
            RubyUtils.RequiresNotFrozen(context, self);

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
                return Fill(fixnumCast.Context, block, self, startFixnum);
            } else {
                return Fill(fixnumCast.Context, block, self, startFixnum, Protocols.CastToFixnum(fixnumCast, length));
            }
        }

        [RubyMethod("fill")]
        public static object Fill(ConversionStorage<int>/*!*/ fixnumCast, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(fixnumCast.Context, block, self, begin, length);
        }

        #endregion

        #region first, last

        [RubyMethod("first")]
        public static object First(IList/*!*/ self) {
            return self.Count == 0 ? null : self[0];
        }

        [RubyMethod("first")]
        public static IList/*!*/ First(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            count = count > self.Count ? self.Count : count;
            return GetResultRange(allocateStorage, self, 0, count);
        }

        [RubyMethod("last")]
        public static object Last(IList/*!*/ self) {
            return self.Count == 0 ? null : self[self.Count - 1];
        }

        [RubyMethod("last")]
        public static IList/*!*/ Last(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            IList/*!*/ self, [DefaultProtocol]int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            count = count > self.Count ? self.Count : count;
            return GetResultRange(allocateStorage, self, self.Count - count, count);
        }

        #endregion

        #region flatten, flatten!

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _infiniteFlattenTracker = new RubyUtils.RecursionTracker();

        public static bool TryFlattenArray(
            CallSiteStorage<Func<CallSite, IList, object>>/*!*/ flattenStorage, 
            ConversionStorage<IList>/*!*/ tryToAry, 
            IList list, out IList/*!*/ result) {

            // TODO: create correct subclass of RubyArray rather than RubyArray directly
            result = new RubyArray();

            using (IDisposable handle = _infiniteFlattenTracker.TrackObject(list)) {
                if (handle == null) {
                    throw RubyExceptions.CreateArgumentError("tried to flatten recursive array");
                }
                bool flattened = false;
                for (int i = 0; i < list.Count; i++) {
                    IList item = Protocols.TryCastToArray(tryToAry, list[i]);
                    if (item != null) {
                        flattened = true;
                        var flatten = flattenStorage.GetCallSite("flatten", 0);

                        object flattenedItem = flatten.Target(flatten, item);
                        IList flattenedList = Protocols.TryCastToArray(tryToAry, flattenedItem);
                        if (flattenedList != null) {
                            AddRange(result, flattenedList);
                        } else {
                            result.Add(flattenedItem);
                        }
                    } else {
                        result.Add(list[i]);
                    }
                }
                return flattened;
            }
        }

        [RubyMethod("flatten")]
        public static IList/*!*/ Flatten(
            CallSiteStorage<Func<CallSite, IList, object>>/*!*/ flattenStorage, 
            ConversionStorage<IList>/*!*/ tryToAry, 
            RubyContext/*!*/ context, IList/*!*/ self) {
            IList result;
            TryFlattenArray(flattenStorage, tryToAry, self, out result);
            return result;
        }

        [RubyMethod("flatten!")]
        public static IList FlattenInPlace(
            CallSiteStorage<Func<CallSite, IList, object>>/*!*/ flattenStorage, 
            ConversionStorage<IList>/*!*/ tryToAry, 
            RubyContext/*!*/ context, IList/*!*/ self) {
            IList result;
            if (!TryFlattenArray(flattenStorage, tryToAry, self, out result)) {
                return null;
            }

            RubyUtils.RequiresNotFrozen(context, self);
            self.Clear();
            AddRange(self, result);
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
            for (int i = self.Count - 1; i >= 0; i--) {
                if (Protocols.IsEqual(equals, self[i], item)) {
                    return i;
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
            IList/*!*/ self, [NotNull]params object[]/*!*/ values) {
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
             IList/*!*/ self, [NotNull]params object[]/*!*/ values) {
            RubyArray result = new RubyArray();

            for (int i = 0; i < values.Length; i++) {
                Range range = values[i] as Range;
                if (range != null) {
                    IList fragment = GetElement(fixnumCast, allocateStorage, self, range);
                    if (fragment != null) {
                        result.AddRange(fragment);
                    }
                } else {
                    result.Add(GetElement(self, Protocols.CastToFixnum(fixnumCast, values[i])));
                }
            }

            return result;
        }

        #endregion

        #region join, to_s, inspect

        public static void RecursiveJoin(ConversionStorage<MutableString>/*!*/ tosConversion, 
            IList/*!*/ list, MutableString/*!*/ separator, MutableString/*!*/ result, Dictionary<object, bool>/*!*/ seen) {

            Assert.NotNull(list, separator, result, seen);
            // TODO: can we get by only tracking List<> ?
            // (inspect needs to track everything)
            bool found;
            if (seen.TryGetValue(list, out found)) {
                result.Append("[...]");
                return;
            }

            seen.Add(list, true); // push

            for (int i = 0; i < list.Count; ++i) {
                object item = list[i];

                if (item is ValueType) {
                    result.Append(Protocols.ConvertToString(tosConversion, item));
                } else if (item == null) {
                    // append nothing
                } else {
                    IList listItem = item as IList;
                    if (listItem != null) {
                        RecursiveJoin(tosConversion, listItem, separator, result, seen);
                    } else {
                        result.Append(Protocols.ConvertToString(tosConversion, item));
                    }
                }

                if (i < list.Count - 1) {
                    result.Append(separator);
                }
            }

            seen.Remove(list);
        }

        [RubyMethod("join")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ Join(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self) {
            return Join(tosConversion, self, tosConversion.Context.ItemSeparator);
        }

        [RubyMethod("join")]
        public static MutableString/*!*/ Join(ConversionStorage<MutableString>/*!*/ tosConversion, IList/*!*/ self, MutableString separator) {
            MutableString result = MutableString.CreateMutable();
            RecursiveJoin(tosConversion, self, separator ?? MutableString.Empty, result, 
                new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance)
            );
            return result;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, IList/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.Create("[...]");
                }
                MutableString str = MutableString.CreateMutable();
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
        public static IList/*!*/ Insert(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int index, [NotNull]params object[]/*!*/ args) {
            if (args.Length > 0)
                RubyUtils.RequiresNotFrozen(context, self);

            if (args.Length == 0) {
                return self;
            }

            if (index == -1) {
                AddRange(self, args);
                return self;
            }

            index = index < 0 ? index + self.Count + 1 : index;
            if (index < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of array", index));
            }

            if (index >= self.Count) {
                ExpandList(self, index);
                AddRange(self, args);
                return self;
            }

            InsertRange(self, index, args);
            return self;
        }

        [RubyMethod("push")]
        public static IList/*!*/ Push(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]params object[]/*!*/ values) {
            if (values.Length > 0) {
                RubyUtils.RequiresNotFrozen(context, self);
            }
            AddRange(self, values);
            return self;
        }

        [RubyMethod("pop")]
        public static object Pop(RubyContext/*!*/ context, IList/*!*/ self) {
            if (self.Count == 0) {
                return null;
            }

            RubyUtils.RequiresNotFrozen(context, self);
            object result = self[self.Count - 1];
            self.RemoveAt(self.Count - 1);
            return result;
        }

        [RubyMethod("shift")]
        public static object Shift(RubyContext/*!*/ context, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

            if (self.Count == 0) {
                return null;
            }

            object result = self[0];
            self.RemoveAt(0);
            return result;
        }

        [RubyMethod("unshift")]
        public static IList/*!*/ Unshift(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]params object[]/*!*/ args) {
            if (args.Length > 0) {
                RubyUtils.RequiresNotFrozen(context, self);
            }

            InsertRange(self, 0, args);
            return self;
        }

        [RubyMethod("<<")]
        public static IList/*!*/ Append(RubyContext/*!*/ context, IList/*!*/ self, object value) {
            RubyUtils.RequiresNotFrozen(context, self);
            self.Add(value);
            return self;
        }

        #endregion

        #region slice!

        [RubyMethod("slice!")]
        public static object SliceInPlace(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int index) {
            RubyUtils.RequiresNotFrozen(context, self);
            index = index < 0 ? index + self.Count : index;
            if (index >= 0 && index < self.Count) {
                object result = self[index];
                SetElement(context, self, index, 1, null);
                return result;
            } else {
                return null;
            }
        }

        [RubyMethod("slice!")]
        public static object SliceInPlace(ConversionStorage<int>/*!*/ fixnumCast, 
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, [NotNull]Range/*!*/ range) {
            RubyUtils.RequiresNotFrozen(fixnumCast.Context, self);
            object result = GetElement(fixnumCast, allocateStorage, self, range);
            SetElement(fixnumCast, self, range, null);
            return result;
        }

        [RubyMethod("slice!")]
        public static IList/*!*/ SliceInPlace(CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            IList/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {
            RubyUtils.RequiresNotFrozen(allocateStorage.Context, self);
            IList result = GetElements(allocateStorage, self, start, length);
            SetElement(allocateStorage.Context, self, start, length, null);
            return result;
        }

        #endregion

        #region sort, sort!

        [RubyMethod("sort")]
        public static IList/*!*/ Sort(
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, IList/*!*/ self) {

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            IList result = CreateResultArray(allocateStorage, self);
            Replace(comparisonStorage.Context, result, ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, ToArray(self)));
            return result;
        }

        [RubyMethod("sort!")]
        public static IList/*!*/ SortInPlace(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BlockParam block, IList/*!*/ self) {

            RubyUtils.RequiresNotFrozen(comparisonStorage.Context, self);
            // this should always call ArrayOps.SortInPlace instead
            Debug.Assert(!(self is RubyArray));

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            Replace(comparisonStorage.Context, self, ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, ToArray(self)));
            return self;
        }

        #endregion

        #region reverse, reverse!, transpose, uniq, uniq!

        [RubyMethod("reverse")]
        public static RubyArray/*!*/ Reverse(IList/*!*/ self) {
            RubyArray reversedList = new RubyArray(self.Count);
            for (int i = 0; i < self.Count; i++) {
                reversedList.Add(self[self.Count - i - 1]);
            }
            return reversedList;
        }

        [RubyMethod("reverse!")]
        public static IList/*!*/ InPlaceReverse(RubyContext/*!*/ context, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

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
        public static RubyArray/*!*/ Transpose(ConversionStorage<IList>/*!*/ arrayCast, RubyContext/*!*/ context, IList/*!*/ self) {
            // Get the arrays. Note we need to check length as we go, so we call to_ary on all the
            // arrays we encounter before the error (if any).
            RubyArray result = new RubyArray();
            for (int i = 0; i < self.Count; i++) {
                IList list = Protocols.CastToArray(arrayCast, self[i]);

                if (i == 0) {
                    // initialize the result
                    result.Capacity = list.Count;
                    for (int j = 0; j < list.Count; j++) {
                        result.Add(new RubyArray());
                    }
                } else if (list.Count != result.Count) {
                    throw RubyExceptions.CreateIndexError(string.Format("element size differs ({0} should be {1})", list.Count, result.Count));
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
            foreach (object item in self) {
                if (!seen.ContainsKey(item)) {
                    result.Add(item);
                    seen.Add(item, true);
                }
            }

            return result;
        }

        [RubyMethod("uniq!")]
        public static IList UniqueSelf(RubyContext/*!*/ context, IList/*!*/ self) {
            var seen = new Dictionary<object, bool>(context.EqualityComparer);
            bool modified = false;
            int i = 0;
            while (i < self.Count) {
                object key = self[i];
                if (!seen.ContainsKey(key)) {
                    seen.Add(key, true);
                    i++;
                } else {
                    self.RemoveAt(i);
                    modified = true;
                }
            }

            return modified ? self : null;
        }

        #endregion
    }
}
