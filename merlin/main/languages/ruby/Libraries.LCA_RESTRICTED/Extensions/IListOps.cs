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
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    using Math = System.Math;

    // TODO: Should this be IList<T> instead of IList?
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

        internal static bool NormalizeRange(RubyContext/*!*/ context, int listCount, Range/*!*/ range, out int begin, out int count) {
            bool excludeEnd;
            int end;
            Protocols.ConvertToIntegerRange(context, range, out begin, out end, out excludeEnd);
            
            begin = NormalizeIndex(listCount, begin);

            if (begin < 0 || begin > listCount) {
                count = 0;
                return false;
            }

            end = NormalizeIndex(listCount, end);

            count = excludeEnd ? end - begin : end - begin + 1;
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

        private static IList/*!*/ GetResultRange(RubyContext/*!*/ context, IList/*!*/ list, int index, int count) {
            IList result = CreateResultArray(context, list);
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

        private static readonly CallSite<Func<CallSite, RubyContext, RubyClass, IList>>/*!*/
            _CreateArraySite = CallSite<Func<CallSite, RubyContext, RubyClass, IList>>.Create(RubySites.InstanceCallAction("new"));

        private static IList/*!*/ CreateResultArray(RubyContext/*!*/ context, IList/*!*/ self) {
            return _CreateArraySite.Target(_CreateArraySite, context, context.GetClassOf(self));
        }

        #endregion

        #region *, +, <<, <=>

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
        public static MutableString Repetition(RubyContext/*!*/ context, IList/*!*/ self, string separator) {
            return Join(context, self, separator);
        }

        [RubyMethod("*")]
        public static MutableString Repetition(RubyContext/*!*/ context, IList/*!*/ self, MutableString separator) {
            return Join(context, self, separator);
        }

        [RubyMethod("*")]
        public static object Repetition(RubyContext/*!*/ context, IList/*!*/ self, object repeat) {
            MutableString str = Protocols.AsString(context, repeat);
            if (str != null)
                return Repetition(context, self, str);
            else
                return Repetition(self, Protocols.CastToFixnum(context, repeat));
        }

        [RubyMethod("+")]
        public static RubyArray/*!*/ Concatenate(IList/*!*/ self, IList/*!*/ other) {
            RubyArray result = new RubyArray(self.Count + other.Count);
            AddRange(result, self);
            AddRange(result, other);
            return result;
        }

        [RubyMethod("+")]
        public static IList Concatenate(RubyContext/*!*/ context, IList/*!*/ self, object other) {
            return Concatenate(self, Protocols.CastToArray(context, other));
        }

        [RubyMethod("-")]
        public static RubyArray/*!*/ Difference(RubyContext/*!*/ context, IList/*!*/ self, IList/*!*/ other) {
            RubyArray result = new RubyArray();
        
            // TODO: optimize this
            foreach (object element in self) {
                if (!Include(context, other, element)) {
                    result.Add(element);
                }
            }

            return result;
        }

        [RubyMethod("-")]
        public static RubyArray/*!*/ Difference(RubyContext/*!*/ context, IList/*!*/ self, object other) {
            return Difference(context, self, Protocols.CastToArray(context, other));
        }

        [RubyMethod("<<")]
        public static IList/*!*/ Append(RubyContext/*!*/ context, IList/*!*/ self, object value) {
            RubyUtils.RequiresNotFrozen(context, self);
            self.Add(value);
            return self;
        }

        [RubyMethod("<=>")]
        public static int Compare(RubyContext/*!*/ context, IList/*!*/ self, IList other) {
            int limit = System.Math.Min(self.Count, other.Count);

            for (int i = 0; i < limit; ++i) {
                int result = Protocols.Compare(context, self[i], other[i]);
                if (result != 0)
                    return result;
            }

            if (self.Count < other.Count)
                return -1;
            else if (self.Count == other.Count)
                return 0;
            else
                return 1;
        }

        [RubyMethod("<=>")]
        public static int Compare(RubyContext/*!*/ context, IList/*!*/ self, object other) {
            return Compare(context, self, Protocols.CastToArray(context, other));
        }
        #endregion

        #region ==, eql?, hash

        [RubyMethod("==")]
        public static bool Equals(IList/*!*/ self, object other) {
            return false;
        }
        
        [RubyMethod("==")]
        public static bool Equals(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            Assert.NotNull(self, other);

            if (object.ReferenceEquals(self, other)) {
                return true;
            }

            if (self.Count != other.Count) {
                return false;
            }

            for (int i = 0; i < self.Count; ++i) {
                bool result = Protocols.IsEqual(context, self[i], other[i]);
                if (!result) {
                    return false;
                }
            }
            return true;
        }

        // eql?, hash
        [RubyMethod("eql?")]
        public static bool HashEquals(IList/*!*/ self, object other) {
            return RubyArray.Equals(self, other);
        }

        [RubyMethod("hash")]
        public static int GetHashCode(IList/*!*/ self) {
            return RubyArray.GetHashCode(self);
        }
        #endregion

        #region slice, []

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static object GetElement(IList list, [DefaultProtocol]int index) {
            return InRangeNormalized(list, ref index) ? list[index] : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElements(RubyContext/*!*/ context, IList/*!*/ list, [DefaultProtocol]int index, [DefaultProtocol]int count) {
            if (!NormalizeRange(list.Count, ref index, ref count)) {
                return null;
            }

            return GetResultRange(context, list, index, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static IList GetElement(RubyContext/*!*/ context, IList array, [NotNull]Range/*!*/ range) {
            int start, count;
            if (!NormalizeRange(context, array.Count, range, out start, out count)) {
                return null;
            }

            return count < 0 ? CreateResultArray(context, array) : GetElements(context, array, start, count);
        }

        #endregion

        #region []=

        public static void ExpandList(IList list, int index) {
            int diff = index - list.Count;
            for (int i = 0; i < diff; ++i)
                list.Add(null);
        }

        public static void OverwriteOrAdd(IList list, int index, object value) {
            if (index < list.Count)
                list[index] = value;
            else
                list.Add(value);
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
        public static object SetElement(RubyContext/*!*/ context, IList list, int index, object value) {
            RubyUtils.RequiresNotFrozen(context, list);

            index = NormalizeIndex(list, index);

            if (index < 0)
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of array", index));

            if (index < list.Count)
                list[index] = value;
            else {
                ExpandList(list, index);
                list.Add(value);
            }
            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList list, object index, object value) {
            return SetElement(context, list, Protocols.CastToFixnum(context, index), value);
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList/*!*/ list, int index, int length, object value) {
            RubyUtils.RequiresNotFrozen(context, list);

            if (length < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("negative length ({0})", length));
            }

            index = NormalizeIndex(list, index);
            if (index < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of array", index));
            }

            IList valueAsList = value as IList;

            if (value == null || (valueAsList != null && valueAsList.Count == 0)) {
                DeleteItems(list, index, length);
            } else {
                if (valueAsList == null) {
                    SetElement(context, list, index, value);
                } else {
                    ExpandList(list, index);

                    int limit = length > valueAsList.Count ? valueAsList.Count : length;

                    for (int i = 0; i < limit; ++i) {
                        OverwriteOrAdd(list, index + i, valueAsList[i]);
                    }

                    if (length < valueAsList.Count) {
                        InsertRange(list, index + limit, EnumerateRange(valueAsList, limit, valueAsList.Count - limit));
                    } else {
                        RemoveRange(list, index + limit, Math.Min(length - valueAsList.Count, list.Count - (index + limit)));
                    }
                }
            }

            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList list, int index, object length, object value) {
            return SetElement(context, list, index, Protocols.CastToFixnum(context, length), value);
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList list, object index, int length, object value) {
            return SetElement(context, list, Protocols.CastToFixnum(context, index), length, value);
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList list, object index, object length, object value) {
            return SetElement(context, list, Protocols.CastToFixnum(context, index), Protocols.CastToFixnum(context, length), value);
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, IList list, Range range, object value) {
            RubyUtils.RequiresNotFrozen(context, list);
            bool excludeEnd;
            int begin, end;
            Protocols.ConvertToIntegerRange(context, range, out begin, out end, out excludeEnd);

            begin = begin < 0 ? begin + list.Count : begin;
            if (begin < 0)
                throw RubyExceptions.CreateRangeError(String.Format("{0}..{1} out of range", begin, end));

            end = end < 0 ? end + list.Count : end;

            int count = excludeEnd ? end - begin : end - begin + 1;
            return SetElement(context, list, begin, Math.Max(count, 0), value);
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

        public static IList GetContainerOf(RubyContext/*!*/ context, IList list, int index, object item) {
            foreach (object current in list) {
                IList subArray = current as IList;
                if (subArray != null && subArray.Count > index) {
                    if (Protocols.IsEqual(context, subArray[index], item))
                        return subArray;
                }
            }
            return null;
        }

        [RubyMethod("assoc")]
        public static IList GetContainerOfFirstItem(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            return GetContainerOf(context, self, 0, item);
        }

        [RubyMethod("at")]
        public static object At(IList/*!*/ self, int index) {
            return GetElement(self, index);
        }

        [RubyMethod("at")]
        public static object At(RubyContext/*!*/ context, IList/*!*/ self, object index) {
            return GetElement(self, Protocols.CastToFixnum(context, index));
        }

        [RubyMethod("clear")]
        public static IList Clear(RubyContext/*!*/ context, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
            self.Clear();
            return self;
        }

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
        public static IList/*!*/ Compact(RubyContext/*!*/ context, IList/*!*/ self) {
            IList result = CreateResultArray(context, self);

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
                } else
                    ++i;
            }
            return changed ? self : null;
        }

        [RubyMethod("concat")]
        public static IList/*!*/ Concat(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]IList/*!*/ other) {
            if (other.Count > 0)
                RubyUtils.RequiresNotFrozen(context, self);
            AddRange(self, other);
            return self;
        }

        [RubyMethod("concat")]
        public static IList Concat(RubyContext/*!*/ context, IList/*!*/ self, object other) {
            if (other == null) {
                throw RubyExceptions.CreateTypeError("cannot convert nil into Array");
            }

            return Concat(context, self, Protocols.CastToArray(context, other));
        }

        #region delete, delete_at

        public static bool Remove(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            int i = 0;
            bool removed = false;
            while (i < self.Count) {
                if (Protocols.IsEqual(context, self[i], item)) {
                    RubyUtils.RequiresNotFrozen(context, self);
                    self.RemoveAt(i);
                    removed = true;
                } else {
                    ++i;
                }
            }
            return removed;
        }

        [RubyMethod("delete")]
        public static object Delete(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            return Remove(context, self, item) ? item : null;
        }

        [RubyMethod("delete")]
        public static object Delete(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self, object item) {
            bool removed = Remove(context, self, item);

            if (block != null) {
                object result;
                block.Yield(out result);
                return result;
            }
            return removed ? item : null;
        }

        [RubyMethod("delete_at")]
        public static object DeleteAt(RubyContext/*!*/ context, IList/*!*/ self, object indexValue) {
            RubyUtils.RequiresNotFrozen(context, self);

            int index = Protocols.CastToFixnum(context, indexValue);
            index = index < 0 ? index + self.Count : index;
            if (index < 0 || index > self.Count)
                return null;

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

        [RubyMethod("empty?")]
        public static bool Empty(IList/*!*/ self) {
            return self.Count == 0;
        }

        #region fetch

        [RubyMethod("fetch")]
        public static object Fetch(RubyContext/*!*/ context, BlockParam outOfRangeValueProvider, IList/*!*/ list, object index, [Optional]object defaultValue) {
            return Fetch(context, outOfRangeValueProvider, list, Protocols.CastToFixnum(context, index), defaultValue);
        }

        [RubyMethod("fetch")]
        public static object Fetch(RubyContext/*!*/ context, BlockParam outOfRangeValueProvider, IList/*!*/ list, int fixnumIndex, [Optional]object defaultValue) {
            int oldIndex = fixnumIndex;
            if (InRangeNormalized(list, ref fixnumIndex)) {
                return list[fixnumIndex];
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
                throw RubyExceptions.CreateIndexError("index " + fixnumIndex + " out of array");
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
        public static IList/*!*/ Fill(RubyContext/*!*/ context, IList/*!*/ self, object obj, object start, [DefaultParameterValue(null)]object length) {
            int startFixnum = (start == null) ? 0 : Protocols.CastToFixnum(context, start);
            if (length == null) {
                return Fill(context, self, obj, startFixnum);
            } else {
                return Fill(context, self, obj, startFixnum, Protocols.CastToFixnum(context, length));
            }
        }

        [RubyMethod("fill")]
        public static IList/*!*/ Fill(RubyContext/*!*/ context, IList/*!*/ self, object obj, Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(context, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(context, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(context, self, obj, begin, length);
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
        public static object Fill(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, object start, [DefaultParameterValue(null)]object length) {
            int startFixnum = (start == null) ? 0 : Protocols.CastToFixnum(context, start);
            if (length == null) {
                return Fill(context, block, self, startFixnum);
            } else {
                return Fill(context, block, self, startFixnum, Protocols.CastToFixnum(context, length));
            }
        }

        [RubyMethod("fill")]
        public static object Fill(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IList/*!*/ self, Range/*!*/ range) {
            int begin = NormalizeIndex(self, Protocols.CastToFixnum(context, range.Begin));
            int end = NormalizeIndex(self, Protocols.CastToFixnum(context, range.End));
            int length = Math.Max(0, end - begin + (range.ExcludeEnd ? 0 : 1));

            return Fill(context, block, self, begin, length);
        }

        #endregion

        #region first

        [RubyMethod("first")]
        public static object First(IList/*!*/ self) {
            return self.Count == 0 ? null : self[0];
        }

        [RubyMethod("first")]
        public static IList/*!*/ First(RubyContext/*!*/ context, IList/*!*/ self, int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            count = count > self.Count ? self.Count : count;
            return GetResultRange(context, self, 0, count);
        }

        [RubyMethod("first")]
        public static object First(RubyContext/*!*/ context, IList/*!*/ self, object count) {
            return First(context, self, Protocols.CastToFixnum(context, count));
        }

        #endregion

        #region flatten, flatten!

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _infiniteFlattenTracker = new RubyUtils.RecursionTracker();

        public static bool TryFlattenArray(RubyContext/*!*/ context, IList list, out IList/*!*/ result) {
            // TODO: create correct subclass of RubyArray rather than RubyArray directly
            result = new RubyArray();

            using (IDisposable handle = _infiniteFlattenTracker.TrackObject(list)) {
                if (handle == null) {
                    throw RubyExceptions.CreateArgumentError("tried to flatten recursive array");
                }
                bool flattened = false;
                for (int i = 0; i < list.Count; ++i) {
                    IList item = list[i] as IList;
                    if (item == null) {
                        item = Protocols.AsArray(context, list[i]);
                    }

                    if (item == null) {
                        result.Add(list[i]);
                    } else {
                        flattened = true;
                        IList flattenedItem = LibrarySites.InvokeFlatten(context, item);
                        if (flattenedItem != null) {
                            AddRange(result, flattenedItem);
                        }
                    }
                }
                return flattened;
            }
        }

        [RubyMethod("flatten")]
        public static IList/*!*/ Flatten(RubyContext/*!*/ context, IList/*!*/ self) {
            IList result;
            TryFlattenArray(context, self, out result);
            return result;
        }

        [RubyMethod("flatten!")]
        public static IList FlattenInPlace(RubyContext/*!*/ context, IList/*!*/ self) {
            IList result;
            if (!TryFlattenArray(context, self, out result)) {
                return null;
            }

            RubyUtils.RequiresNotFrozen(context, self);
            self.Clear();
            AddRange(self, result);
            return self;
        }

        #endregion

        #region include?, index, indexes, indices

        [RubyMethod("include?")]
        public static bool Include(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            return Index(context, self, item) != null;
        }

        [RubyMethod("index")]
        public static object Index(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            for (int i = 0; i < self.Count; ++i) {
                if (Protocols.IsEqual(context, self[i], item))
                    return i;
            }
            return null;
        }

        public static RubyArray/*!*/ GetIndicesAsNestedArrays(RubyContext/*!*/ context, IList list, object[] parameters) {
            RubyArray result = new RubyArray();

            for (int i = 0; i < parameters.Length; ++i) {
                Range range = parameters[i] as Range;
                if (range != null) {
                    IList fragment = GetElement(context, list, range);
                    if (fragment != null) {
                        result.Add(fragment);
                    }
                } else {
                    result.Add(GetElement(list, Protocols.CastToFixnum(context, parameters[i])));
                }
            }

            return result;
        }

        // BUG? parameters are not splatted into object[], but instead are being passed in as IList
        [RubyMethod("indexes")]
        [RubyMethod("indices")]
        public static object Indexes(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]params object[] args) {
            context.ReportWarning("Array#indexes and Array#indices are deprecated; use Array#values_at");
            return GetIndicesAsNestedArrays(context, self, args);
        }

        #endregion

        [RubyMethod("insert")]
        public static IList/*!*/ Insert(RubyContext/*!*/ context, IList/*!*/ self, int index, [NotNull]params object[] args) {
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

        [RubyMethod("inspect")]
        public static MutableString Inspect(RubyContext/*!*/ context, IList/*!*/ self) {
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
                    str.Append(RubySites.Inspect(context, obj));
                }
                str.Append(']');
                return str;
            }
        }

        #region join, to_s

        public static void RecursiveJoin(RubyContext/*!*/ context, IList/*!*/ list, string separator, MutableString/*!*/ result, Dictionary<object, bool>/*!*/ seen) {
            Assert.NotNull(list, result, seen);
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
                    result.Append(RubySites.ToS(context, item));
                } else if (item == null) {
                    // append nothing
                } else {
                    IList listItem = item as IList;
                    if (listItem != null) {
                        RecursiveJoin(context, listItem, separator, result, seen);
                    } else {
                        result.Append(RubySites.ToS(context, item));
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
        public static MutableString/*!*/ Join(RubyContext/*!*/ context, [NotNull]IList/*!*/ self) {
            MutableString separator = context.ItemSeparator;
            return Join(context, self, (separator != null ? separator.ConvertToString() : String.Empty));
        }

        [RubyMethod("join")]
        public static MutableString Join(RubyContext/*!*/ context, IList/*!*/ self, string separator) {
            Assert.NotNull(context, self, separator);

            MutableString result = MutableString.CreateMutable();
            RecursiveJoin(context, self, separator, result, new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance));
            return result;
        }

        [RubyMethod("join")]
        public static MutableString Join(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]MutableString/*!*/ separator) {
            return Join(context, self, separator.ConvertToString());
        }

        #endregion

        [RubyMethod("last")]
        public static object Last(IList/*!*/ self) {
            return self.Count == 0 ? null : self[self.Count - 1];
        }

        [RubyMethod("last")]
        public static IList/*!*/ Last(RubyContext/*!*/ context, IList/*!*/ self, int count) {
            if (count < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
            }

            count = count > self.Count ? self.Count : count;
            return GetResultRange(context, self, self.Count - count, count);
        }

        [RubyMethod("last")]
        public static object Last(RubyContext/*!*/ context, IList/*!*/ self, object count) {
            return Last(context, self, Protocols.CastToFixnum(context, count));
        }

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int Length(IList/*!*/ self) {
            return self.Count;
        }

        [RubyMethod("nitems")]
        public static int NumberOfNonNilItems(IList/*!*/ self) {
            int count = 0;
            foreach (object obj in self)
                if (obj != null)
                    count++;
            return count;
        }

        [RubyMethod("pop")]
        public static object Pop(RubyContext/*!*/ context, IList/*!*/ self) {
            if (self.Count == 0)
                return null;

            RubyUtils.RequiresNotFrozen(context, self);
            object result = self[self.Count - 1];
            self.RemoveAt(self.Count - 1);
            return result;
        }

        [RubyMethod("push")]
        public static IList/*!*/ Push(RubyContext/*!*/ context, IList/*!*/ self, [NotNull] params object[] values) {
            if (values.Length > 0)
                RubyUtils.RequiresNotFrozen(context, self);
            AddRange(self, values);
            return self;
        }

        [RubyMethod("reverse")]
        public static RubyArray/*!*/ Reverse(IList/*!*/ self) {
            RubyArray reversedList = new RubyArray(self.Count);
            for (int i = 0; i < self.Count; ++i) {
                reversedList.Add(self[self.Count - i - 1]);
            }
            return reversedList;
        }

        [RubyMethod("reverse!")]
        public static IList InPlaceReverse(RubyContext/*!*/ context, IList/*!*/ self) {
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

        [RubyMethod("rassoc")]
        public static IList GetContainerOfSecondItem(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            return GetContainerOf(context, self, 1, item);
        }

        [RubyMethod("replace")]
        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static IList Replace(RubyContext/*!*/ context, IList/*!*/ self, [NotNull, DefaultProtocol]IList/*!*/ other) {
            RubyUtils.RequiresNotFrozen(context, self);

            self.Clear();
            AddRange(self, other);
            return self;
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(RubyContext/*!*/ context, IList/*!*/ self, object item) {
            for (int i = self.Count - 1; i >= 0; --i) {
                if (Protocols.IsEqual(context, self[i], item))
                    return i;
            }
            return null;
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
        public static object SliceInPlace(RubyContext/*!*/ context, IList/*!*/ self, [NotNull]Range/*!*/ range) {
            RubyUtils.RequiresNotFrozen(context, self);
            object result = GetElement(context, self, range);
            SetElement(context, self, range, null);
            return result;
        }

        [RubyMethod("slice!")]
        public static IList/*!*/ SliceInPlace(RubyContext/*!*/ context, IList/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {
            RubyUtils.RequiresNotFrozen(context, self);
            IList result = GetElements(context, self, start, length);
            SetElement(context, self, start, length, null);
            return result;
        }

        #region sort, sort!

        [RubyMethod("sort")]
        public static IList/*!*/ Sort(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            IList result = CreateResultArray(context, self);
            Replace(context, result, ArrayOps.SortInPlace(context, block, ToArray(self)));
            return result;
        }

        [RubyMethod("sort!")]
        public static IList/*!*/ SortInPlace(RubyContext/*!*/ context, BlockParam block, IList/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
            // this should always call ArrayOps.SortInPlace instead
            Debug.Assert(!(self is RubyArray));

            // TODO: this is not optimal because it makes an extra array copy
            // (only affects sorting of .NET types, do we need our own quicksort?)
            Replace(context, self, ArrayOps.SortInPlace(context, block, ToArray(self)));
            return self;
        }

        #endregion

        [RubyMethod("transpose")]
        public static RubyArray/*!*/ Transpose(RubyContext/*!*/ context, IList/*!*/ self) {
            // Get the arrays. Note we need to check length as we go, so we call to_ary on all the
            // arrays we encounter before the error (if any).
            RubyArray result = new RubyArray();
            for (int i = 0; i < self.Count; i++) {
                IList list = Protocols.CastToArray(context, self[i]);

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

        [RubyMethod("to_a")]
        [RubyMethod("to_ary")]
        public static RubyArray/*!*/ ToArray(IList/*!*/ self) {
            RubyArray list = new RubyArray(self.Count);
            foreach (object item in self) {
                list.Add(item);
            }
            return list;
        }

        [RubyMethod("uniq")]
        public static IList/*!*/ Unique(RubyContext/*!*/ context, IList/*!*/ self) {
            IList result = CreateResultArray(context, self);

            Dictionary<object, bool> seen = new Dictionary<object, bool>(context.EqualityComparer);
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
            Dictionary<object, bool> seen = new Dictionary<object, bool>(context.EqualityComparer);
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

        [RubyMethod("unshift")]
        public static IList Unshift(RubyContext/*!*/ context, IList/*!*/ self, [NotNull] params object[] args) {
            if (args.Length > 0) {
                RubyUtils.RequiresNotFrozen(context, self);
            }

            InsertRange(self, 0, args);
            return self;
        }

        [RubyMethod("values_at")]
        public static RubyArray/*!*/ ValuesAt(RubyContext/*!*/ context, IList/*!*/ self, [NotNull] params object[] values) {
            RubyArray result = new RubyArray();

            for (int i = 0; i < values.Length; ++i) {
                Range range = values[i] as Range;
                if (range != null) {
                    IList fragment = GetElement(context, self, range);
                    if (fragment != null) {
                        AddRange(result, fragment);
                    }
                } else
                    result.Add(GetElement(self, Protocols.CastToFixnum(context, values[i])));
            }

            return result;
        }
    }
}
