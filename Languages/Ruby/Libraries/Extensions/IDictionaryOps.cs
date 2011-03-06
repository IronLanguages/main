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
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting;

namespace IronRuby.Builtins {

    // TODO: IDictionary<TKey, TValue> instead of IDictionary<object, object>?
    //       (need support for extension methods on generic interfaces first)
    //       (IDictionary isn't a good solution because it doesn't have TryGetValue)
    [RubyModule(Extends = typeof(IDictionary<object, object>), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(Enumerable))]
    public static class IDictionaryOps {

        #region Helper methods

        // Make a 2 element array
        internal static RubyArray/*!*/ MakeArray(KeyValuePair<object, object> pair) {
            RubyArray list = new RubyArray(2);
            list.Add(CustomStringDictionary.ObjToNull(pair.Key));
            list.Add(pair.Value);
            return list;
        }

        internal static RubyArray/*!*/ MakeArray(object key, object value) {
            RubyArray list = new RubyArray(2);
            list.Add(CustomStringDictionary.ObjToNull(key));
            list.Add(value);
            return list;
        }

        // Replaces the data in dest with the data from src
        internal static T ReplaceData<T>(T dest, IEnumerable<KeyValuePair<object, object>> src) where T : IDictionary<object, object> {
            dest.Clear();
            foreach (KeyValuePair<object, object> pair in src) {
                dest[pair.Key] = pair.Value;
            }
            return dest;
        }

        // Copies key,value pairs into an array. Needed for iteration over the hash while mutating it.
        private static IEnumerable<KeyValuePair<object, object>> CopyKeyValuePairs(IDictionary<object, object>/*!*/ dict) {
            KeyValuePair<object, object>[] pairs = new KeyValuePair<object, object>[dict.Count];
            dict.CopyTo(pairs, 0);
            return pairs;
        }

        #endregion

        [RubyMethod("==")]
        public static bool Equals(RespondToStorage/*!*/ respondTo, BinaryOpStorage/*!*/ equals, IDictionary<object, object>/*!*/ self, object other) {
            return Protocols.RespondTo(respondTo, other, "to_hash") && Protocols.IsEqual(equals, other, self);
        }

        [MultiRuntimeAware]
        private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

        [RubyMethod("==")]
        public static bool Equals(BinaryOpStorage/*!*/ equals, IDictionary<object, object>/*!*/ self, [NotNull]IDictionary<object, object>/*!*/ other) {
            Assert.NotNull(self, other);

            if (ReferenceEquals(self, other)) {
                return true;
            }

            if (self.Count != other.Count) {
                return false;
            }

            using (IDisposable handleSelf = _EqualsTracker.TrackObject(self), handleOther = _EqualsTracker.TrackObject(other)) {
                if (handleSelf == null && handleOther == null) {
                    // both dictionaries went recursive:
                    return true;
                }

                // Each key value pair must be the same
                var site = equals.GetCallSite("==");
                foreach (KeyValuePair<object, object> pair in self) {
                    object value;
                    if (!other.TryGetValue(pair.Key, out value) || !Protocols.IsEqual(site, pair.Value, value)) {
                        return false;
                    }
                }
            }

            return true;
        }

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, object key) {
            object result;
            if (!self.TryGetValue(CustomStringDictionary.NullToObj(key), out result)) {
                return null;
            }
            return result;
        }

        // TODO: remove, frozen check should be implemented in Hash indexer
        [RubyMethod("[]=")]
        [RubyMethod("store")]
        public static object SetElement(RubyContext/*!*/ context, Hash/*!*/ self, object key, object value) {
            self.RequireNotFrozen();
            return RubyUtils.SetHashElement(context, self, key, value);
        }

        [RubyMethod("[]=")]
        [RubyMethod("store")]
        public static object SetElement(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, object key, object value) {
            return RubyUtils.SetHashElement(context, self, key, value);
        }

        // TODO: remove, frozen check should be implemented in Hash.Clear
        [RubyMethod("clear")]
        public static IDictionary<object, object>/*!*/ Clear(Hash/*!*/ self) {
            self.RequireNotFrozen();
            self.Clear();
            return self;
        }

        [RubyMethod("clear")]
        public static IDictionary<object, object>/*!*/ Clear(IDictionary<object, object>/*!*/ self) {
            self.Clear();
            return self;
        }

        // We don't define "dup" here because "dup" shouldn't show up on builtin types like Hash
        // (Kernel#dup just works for these types)
        private static IDictionary<object, object>/*!*/ Duplicate(
            CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            IDictionary<object, object>/*!*/ self) {

            // Call Kernel#dup, then copy items
            var copy = (IDictionary<object, object>)KernelOps.Duplicate(initializeCopyStorage, allocateStorage, self);
            return ReplaceData(copy, self);
        }

        [RubyMethod("default")]
        public static object GetDefaultValue(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, [Optional]object key) {
            return null;
        }

        [RubyMethod("default_proc")]
        public static Proc GetDefaultProc(IDictionary<object, object>/*!*/ self) {
            return null;
        }

        #region delete, delete_if

        [RubyMethod("delete")]
        public static object Delete(BlockParam block, Hash/*!*/ self, object key) {
            self.RequireNotFrozen();
            return Delete(block, (IDictionary<object, object>)self, key);
        }

        [RubyMethod("delete")]
        public static object Delete(BlockParam block, IDictionary<object, object>/*!*/ self, object key) {
            object value;
            if (!self.TryGetValue(CustomStringDictionary.NullToObj(key), out value)) {
                // key not found, call the block if it was passed in
                if (block != null) {
                    object result;
                    block.Yield(key, out result);
                    return result;
                }
                return null;
            }
            self.Remove(CustomStringDictionary.NullToObj(key));
            return value;
        }

        [RubyMethod("delete_if")]
        public static object DeleteIf(BlockParam block, Hash/*!*/ self) {
            self.RequireNotFrozen();
            return DeleteIf(block, (IDictionary<object, object>)self);
        }

        [RubyMethod("delete_if")]
        public static object DeleteIf(BlockParam block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            // Make a copy of the keys to delete, so we don't modify the collection
            // while iterating over it
            RubyArray keysToDelete = new RubyArray();

            foreach (var pair in self) {
                object result;
                if (block.Yield(CustomStringDictionary.ObjToNull(pair.Key), pair.Value, out result)) {
                    return result;
                }

                // Delete the key, unless 'false' or 'nil' is returned
                if (RubyOps.IsTrue(result)) {
                    keysToDelete.Add(pair.Key);
                }
            }

            foreach (object key in keysToDelete) {
                self.Remove(key);
            }

            return self;
        }

        #endregion

        #region each, each_pair, each_key, each_value

        [RubyMethod("each")]
        [RubyMethod("each_pair")]
        public static Enumerator/*!*/ Each(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            return new Enumerator((_, block) => Each(context, block, self));
        }

        [RubyMethod("each")]
        [RubyMethod("each_pair")]
        public static object Each(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0) {
                // Must make a copy of the Keys array so that we can iterate over a static set of keys. Remember
                // that the block can modify the hash, hence the need for a copy of the keys
                object[] keys = new object[self.Count];
                self.Keys.CopyTo(keys, 0);

                // TODO: what are all the scenarios where the block can mutate the hash? can it remove keys? if so, what happens?
                for (int i = 0; i < keys.Length; i++) {
                    object result;
                    if (block.Yield(MakeArray(keys[i], self[keys[i]]), out result)) {
                        return result;
                    }
                }
            }

            return self;
        }
        
        [RubyMethod("each_key")]
        public static Enumerator/*!*/ EachKey(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            return new Enumerator((_, block) => EachKey(context, block, self));
        }

        [RubyMethod("each_key")]
        public static object EachKey(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0) {
                // Must make a copy of the Keys array so that we can iterate over a static set of keys. Remember
                // that the block can modify the hash, hence the need for a copy of the keys
                object[] keys = new object[self.Count];
                self.Keys.CopyTo(keys, 0);

                // TODO: what are all the scenarios where the block can mutate the hash? can it remove keys? if so, what happens?
                for (int i = 0; i < keys.Length; i++) {
                    object result;
                    if (block.Yield(CustomStringDictionary.ObjToNull(keys[i]), out result)) {
                        return result;
                    }
                }
            }

            return self;
        }

        [RubyMethod("each_value")]
        public static Enumerator/*!*/ EachValue(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            return new Enumerator((_, block) => EachValue(context, block, self));
        }

        [RubyMethod("each_value")]
        public static object EachValue(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0) {
                // Ruby allows modifications while iterating thru the dictionary:
                object[] values = new object[self.Count];
                self.Values.CopyTo(values, 0);

                for (int i = 0; i < values.Length; i++) {
                    object result;
                    if (block.Yield(values[i], out result)) {
                        return result;
                    }
                }
            }

            return self;
        }

        #endregion

        [RubyMethod("empty?")]
        public static bool Empty(IDictionary<object, object>/*!*/ self) {
            return self.Count == 0;
        }

        [RubyMethod("fetch")]
        public static object Fetch(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self, object key, [Optional]object defaultValue) {
            object result;
            if (self.TryGetValue(CustomStringDictionary.NullToObj(key), out result)) {
                return result;
            }

            if (block != null) {
                if (defaultValue != Missing.Value) {
                    context.ReportWarning("block supersedes default value argument");
                }

                block.Yield(key, out result);
                return result;
            }

            if (defaultValue == Missing.Value) {
                throw RubyExceptions.CreateIndexError("key not found");
            }

            return defaultValue;
        }

        [RubyMethod("has_key?")]
        [RubyMethod("include?")]
        [RubyMethod("key?")]
        [RubyMethod("member?")]
        public static bool HasKey(IDictionary<object, object>/*!*/ self, object key) {
            return self.ContainsKey(CustomStringDictionary.NullToObj(key));
        }

        [RubyMethod("has_value?")]
        [RubyMethod("value?")]
        public static bool HasValue(BinaryOpStorage/*!*/ equals, IDictionary<object, object>/*!*/ self, object value) {
            foreach (KeyValuePair<object, object> pair in self) {
                if (Protocols.IsEqual(equals, pair.Value, value)) {
                    return true;
                }
            }
            return false;
        }

        [RubyMethod("index")]
        public static object Index(BinaryOpStorage/*!*/ equals, IDictionary<object, object>/*!*/ self, object value) {
            foreach (KeyValuePair<object, object> pair in self) {
                if (Protocols.IsEqual(equals, pair.Value, value)) {
                    return CustomStringDictionary.ObjToNull(pair.Key);
                }
            }
            return null;
        }

        [RubyMethod("invert")]
        public static Hash/*!*/ Invert(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            // invert returns a Hash, even from subclasses
            Hash hash = new Hash(context.EqualityComparer, self.Count);
            foreach (KeyValuePair<object, object> pair in self) {
                hash[CustomStringDictionary.NullToObj(pair.Value)] = CustomStringDictionary.ObjToNull(pair.Key);
            }
            return hash;
        }

        [RubyMethod("keys")]
        public static RubyArray/*!*/ GetKeys(IDictionary<object, object>/*!*/ self) {
            RubyArray keys = new RubyArray(self.Count);
            foreach (object key in self.Keys) {
                keys.Add(CustomStringDictionary.ObjToNull(key));
            }
            return keys;
        }

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int Length(IDictionary<object, object>/*!*/ self) {
            return self.Count;
        }

        [RubyMethod("merge")]
        public static object Merge(
            CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            BlockParam block, IDictionary<object, object>/*!*/ self, 
            [DefaultProtocol, NotNull]IDictionary<object, object>/*!*/ hash) {

            return Update(block, Duplicate(initializeCopyStorage, allocateStorage, self), hash);
        }

        [RubyMethod("merge!")]
        [RubyMethod("update")]
        public static object Update(BlockParam block, Hash/*!*/ self, [DefaultProtocol, NotNull]IDictionary<object, object>/*!*/ hash) {
            self.RequireNotFrozen();
            return Update(block, (IDictionary<object, object>)self, hash);
        }

        [RubyMethod("merge!")]
        [RubyMethod("update")]
        public static object Update(BlockParam block, IDictionary<object, object>/*!*/ self, [DefaultProtocol, NotNull]IDictionary<object, object>/*!*/ hash) {
            if (block == null) {
                foreach (var pair in CopyKeyValuePairs(hash)) {
                    self[CustomStringDictionary.NullToObj(pair.Key)] = pair.Value;
                }
            } else {
                foreach (var pair in CopyKeyValuePairs(hash)) {
                    object key = pair.Key, newValue = pair.Value, oldValue;
                    if (self.TryGetValue(key, out oldValue)) {
                        if (block.Yield(CustomStringDictionary.ObjToNull(key), oldValue, pair.Value, out newValue)) {
                            return newValue;
                        }
                    }
                    self[key] = newValue;
                }
            }
            return self;
        }

        [RubyMethod("rehash")]
        public static IDictionary<object, object> Rehash(Hash/*!*/ self) {
            self.RequireNotFrozen();
            return Rehash((IDictionary<object, object>)self);
        }

        [RubyMethod("rehash")]
        public static IDictionary<object, object> Rehash(IDictionary<object, object>/*!*/ self) {
            return ReplaceData(self, CopyKeyValuePairs(self));
        }

        // This works like delete_if, not reject!
        // (because it needs to return the new collection)
        [RubyMethod("reject")]
        public static object Reject(
            CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            BlockParam block, IDictionary<object, object>/*!*/ self) {

            return DeleteIf(block, Duplicate(initializeCopyStorage, allocateStorage, self));
        }

        // This works like delete_if, but returns nil if no elements were removed
        [RubyMethod("reject!")]
        public static object RejectMutate(BlockParam block, Hash/*!*/ self) {
            self.RequireNotFrozen();
            return RejectMutate(block, (IDictionary<object, object>)self);
        }

        [RubyMethod("reject!")]
        public static object RejectMutate(BlockParam block, IDictionary<object, object>/*!*/ self) {

            // Make a copy of the keys to delete, so we don't modify the collection
            // while iterating over it
            RubyArray keysToDelete = new RubyArray();

            foreach (KeyValuePair<object,object> pair in self) {
                object result;
                if (block.Yield(CustomStringDictionary.ObjToNull(pair.Key), pair.Value, out result)) {
                    return result;
                }

                // Delete the key, unless 'false' or 'nil' is returned
                if (RubyOps.IsTrue(result)) {
                    keysToDelete.Add(pair.Key);
                }
            }

            foreach (object key in keysToDelete) {
                self.Remove(key);
            }

            return keysToDelete.Count == 0 ? null : self;
        }

        [RubyMethod("replace")]
        public static Hash/*!*/ Replace(RubyContext/*!*/ context, Hash/*!*/ self, [DefaultProtocol, NotNull]IDictionary<object, object>/*!*/ other) {
            self.RequireNotFrozen();
            return IDictionaryOps.ReplaceData(self, other);
        }

        [RubyMethod("select")]
        public static Enumerator/*!*/ Select(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            return new Enumerator((_, block) => Select(context, block, self));
        }

        [RubyMethod("select")]
        public static object Select(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, IDictionary<object, object>/*!*/ self) {
            Hash result = new Hash(context);

            foreach (var pair in CopyKeyValuePairs(self)) {
                object blockResult;
                if (block.Yield(CustomStringDictionary.ObjToNull(pair.Key), pair.Value, out blockResult)) {
                    return blockResult;
                }

                if (RubyOps.IsTrue(blockResult)) {
                    result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

        [RubyMethod("shift")]
        public static object Shift(Hash/*!*/ self) {
            self.RequireNotFrozen();
            return Shift((IDictionary<object, object>)self);
        }

        [RubyMethod("shift")]
        public static object Shift(IDictionary<object, object>/*!*/ self) {
            if (self.Count == 0) {
                return null;
            }

            IEnumerator<KeyValuePair<object, object>> e = self.GetEnumerator();
            e.MoveNext();
            KeyValuePair<object, object> pair = e.Current;
            self.Remove(pair.Key);

            return MakeArray(pair);
        }

        [RubyMethod("sort")]
        public static object Sort(ComparisonStorage/*!*/ comparisonStorage, BlockParam block, IDictionary<object, object>/*!*/ self) {
            return ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self));
        }

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(IDictionary<object, object>/*!*/ self) {
            RubyArray result = new RubyArray(self.Count);
            foreach (KeyValuePair<object, object> pair in self) {
                result.Add(MakeArray(pair));
            }
            return result;
        }

        [RubyMethod("flatten")]
        public static IList/*!*/ Flatten(ConversionStorage<IList>/*!*/ tryToAry, IDictionary<object, object>/*!*/ self,
            [DefaultProtocol, DefaultParameterValue(1)] int maxDepth) {

            if (maxDepth == 0) {
                return ToArray(self);
            } 
            
            if (maxDepth > 0) {
                maxDepth--;
            }

            RubyArray result = new RubyArray();
            IList list;
            foreach (KeyValuePair<object, object> pair in self) {
                if (maxDepth != 0 && (list = Protocols.TryCastToArray(tryToAry, pair.Key)) != null) {
                    IListOps.Flatten(tryToAry, list, maxDepth - 1, result);
                } else {
                    result.Add(pair.Key);
                }

                if (maxDepth != 0 && (list = Protocols.TryCastToArray(tryToAry, pair.Value)) != null) {
                    IListOps.Flatten(tryToAry, list, maxDepth - 1, result);
                } else {
                    result.Add(pair.Value);
                }
            }
            return result;
        }

        [RubyMethod("to_hash")]
        public static IDictionary<object, object> ToHash(IDictionary<object, object>/*!*/ self) {
            return self;
        }

        [RubyMethod("to_s")]
        [RubyMethod("inspect")]
        public static MutableString/*!*/ ToMutableString(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.CreateAscii("{...}");
                }

                MutableString str = MutableString.CreateMutable(RubyEncoding.Binary);
                str.Append('{');
                bool first = true;
                foreach (var entry in self) {
                    if (first) {
                        first = false;
                    } else {
                        str.Append(", ");
                    }
                    str.Append(context.Inspect(CustomStringDictionary.ObjToNull(entry.Key)));
                    str.Append("=>");
                    str.Append(context.Inspect(entry.Value));
                }
                str.Append('}');
                return str;
            }
        }
        
        [RubyMethod("values")]
        public static RubyArray/*!*/ GetValues(IDictionary<object, object>/*!*/ self) {
            return new RubyArray(self.Values);
        }

        [RubyMethod("values_at")]
        public static RubyArray/*!*/ ValuesAt(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, params object[]/*!*/ keys) {
            RubyArray values = new RubyArray();
            foreach (object key in keys) {
                values.Add(GetElement(context, self, key));
            }
            return values;
        }
    }

}
