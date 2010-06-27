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
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;

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
            list.Add(BaseSymbolDictionary.ObjToNull(pair.Key));
            list.Add(pair.Value);
            return list;
        }

        internal static RubyArray/*!*/ MakeArray(object key, object value) {
            RubyArray list = new RubyArray(2);
            list.Add(BaseSymbolDictionary.ObjToNull(key));
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

        #region Instance Methods

        [RubyMethod("==")]
        public static bool Equals(BinaryOpStorage/*!*/ equals, IDictionary<object, object>/*!*/ self, object otherHash) {
            IDictionary<object, object> other = otherHash as IDictionary<object, object>;
            if (other == null || self.Count != other.Count) {
                return false;
            }

            // Each key value pair must be the same
            foreach (KeyValuePair<object, object> pair in self) {
                object value;
                if (!other.TryGetValue(pair.Key, out value) || !Protocols.IsEqual(equals, pair.Value, value)) {
                    return false;
                }
            }
            return true;
        }

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, object key) {
            object result;
            if (!self.TryGetValue(BaseSymbolDictionary.NullToObj(key), out result)) {
                return null;
            }
            return result;
        }

        [RubyMethod("[]=")]
        [RubyMethod("store")]
        public static object SetElement(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, object key, object value) {
            RubyUtils.RequiresNotFrozen(context, self);
            return RubyUtils.SetHashElement(context, self, key, value);
        }

        [RubyMethod("clear")]
        public static IDictionary<object, object> Clear(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
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

        [RubyMethod("delete")]
        public static object Delete(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self, object key) {
            RubyUtils.RequiresNotFrozen(context, self);

            object value;
            if (!self.TryGetValue(BaseSymbolDictionary.NullToObj(key), out value)) {
                // key not found, call the block if it was passed in
                if (block != null) {
                    object result;
                    block.Yield(key, out result);
                    return result;
                }
                return null;
            }
            self.Remove(BaseSymbolDictionary.NullToObj(key));
            return value;
        }

        [RubyMethod("delete_if")]
        public static object DeleteIf(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

            if (self.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            // Make a copy of the keys to delete, so we don't modify the collection
            // while iterating over it
            RubyArray keysToDelete = new RubyArray();

            foreach (var pair in self) {
                object result;
                if (block.Yield(BaseSymbolDictionary.ObjToNull(pair.Key), pair.Value, out result)) {
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

        [RubyMethod("each")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
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

        [RubyMethod("each_pair")]
        public static object EachPair(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0) {
                // Must make a copy of the Keys array so that we can iterate over a static set of keys. Remember
                // that the block can modify the hash, hence the need for a copy of the keys
                object[] keys = new object[self.Count];
                self.Keys.CopyTo(keys, 0);

                // TODO: what are all the scenarios where the block can mutate the hash? can it remove keys? if so, what happens?
                for (int i = 0; i < keys.Length; i++) {
                    object result;
                    if (block.Yield(BaseSymbolDictionary.ObjToNull(keys[i]), self[keys[i]], out result)) {
                        return result;
                    }
                }
            }

            return self;
        }

        [RubyMethod("each_key")]
        public static object EachKey(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
            if (self.Count > 0) {
                // Must make a copy of the Keys array so that we can iterate over a static set of keys. Remember
                // that the block can modify the hash, hence the need for a copy of the keys
                object[] keys = new object[self.Count];
                self.Keys.CopyTo(keys, 0);

                // TODO: what are all the scenarios where the block can mutate the hash? can it remove keys? if so, what happens?
                for (int i = 0; i < keys.Length; i++) {
                    object result;
                    if (block.Yield(BaseSymbolDictionary.ObjToNull(keys[i]), out result)) {
                        return result;
                    }
                }
            }

            return self;
        }

        [RubyMethod("each_value")]
        public static object EachValue(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
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

        [RubyMethod("empty?")]
        public static bool Empty(IDictionary<object, object>/*!*/ self) {
            return self.Count == 0;
        }

        [RubyMethod("fetch")]
        public static object Fetch(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self, object key, [Optional]object defaultValue) {
            object result;
            if (self.TryGetValue(BaseSymbolDictionary.NullToObj(key), out result)) {
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
            return self.ContainsKey(BaseSymbolDictionary.NullToObj(key));
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
                    return BaseSymbolDictionary.ObjToNull(pair.Key);
                }
            }
            return null;
        }

        [RubyMethod("indexes")]
        [RubyMethod("indices")]
        public static RubyArray/*!*/ Indexes(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self, params object[]/*!*/ keys) {
            context.ReportWarning("Hash#indices is deprecated; use Hash#values_at");
            return ValuesAt(context, self, keys);
        }

        [RubyMethod("inspect")]
        public static MutableString Inspect(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.CreateAscii("{...}");
                }
                MutableString str = MutableString.CreateMutable(RubyEncoding.Binary);
                str.Append('{');
                foreach (KeyValuePair<object, object> pair in self) {
                    if (str.Length != 1) {
                        str.Append(", ");
                    }
                    str.Append(context.Inspect(BaseSymbolDictionary.ObjToNull(pair.Key)));
                    str.Append("=>");
                    str.Append(context.Inspect(pair.Value));
                }
                str.Append('}');
                return str;
            }
        }

        [RubyMethod("invert")]
        public static Hash/*!*/ Invert(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            // invert returns a Hash, even from subclasses
            Hash hash = new Hash(context.EqualityComparer, self.Count);
            foreach (KeyValuePair<object, object> pair in self) {
                hash[BaseSymbolDictionary.NullToObj(pair.Value)] = BaseSymbolDictionary.ObjToNull(pair.Key);
            }
            return hash;
        }

        [RubyMethod("keys")]
        public static RubyArray/*!*/ GetKeys(IDictionary<object, object>/*!*/ self) {
            RubyArray keys = new RubyArray(self.Count);
            foreach (object key in self.Keys) {
                keys.Add(BaseSymbolDictionary.ObjToNull(key));
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

            return Update(allocateStorage.Context, block, Duplicate(initializeCopyStorage, allocateStorage, self), hash);
        }

        [RubyMethod("merge!")]
        [RubyMethod("update")]
        public static object Update(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self, 
            [DefaultProtocol, NotNull]IDictionary<object, object>/*!*/ hash) {

            if (hash.Count > 0) {
                RubyUtils.RequiresNotFrozen(context, self);
            }

            if (block == null) {
                foreach (var pair in CopyKeyValuePairs(hash)) {
                    self[BaseSymbolDictionary.NullToObj(pair.Key)] = pair.Value;
                }
            } else {
                foreach (var pair in CopyKeyValuePairs(hash)) {
                    object key = pair.Key, newValue = pair.Value, oldValue;
                    if (self.TryGetValue(key, out oldValue)) {
                        if (block.Yield(BaseSymbolDictionary.ObjToNull(key), oldValue, pair.Value, out newValue)) {
                            return newValue;
                        }
                    }
                    self[key] = newValue;
                }
            }
            return self;
        }

        [RubyMethod("rehash")]
        public static IDictionary<object, object> Rehash(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);
            return ReplaceData(self, CopyKeyValuePairs(self));
        }

        // This works like delete_if, not reject!
        // (because it needs to return the new collection)
        [RubyMethod("reject")]
        public static object Reject(
            CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage,
            BlockParam block, IDictionary<object, object>/*!*/ self) {

            return DeleteIf(allocateStorage.Context, block, Duplicate(initializeCopyStorage, allocateStorage, self));
        }

        // This works like delete_if, but returns nil if no elements were removed
        [RubyMethod("reject!")]
        public static object RejectMutate(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

            // Make a copy of the keys to delete, so we don't modify the collection
            // while iterating over it
            RubyArray keysToDelete = new RubyArray();

            foreach (KeyValuePair<object,object> pair in self) {
                object result;
                if (block.Yield(BaseSymbolDictionary.ObjToNull(pair.Key), pair.Value, out result)) {
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
            self.Mutate();
            return IDictionaryOps.ReplaceData(self, other);
        }

        [RubyMethod("select")]
        public static object Select(RubyContext/*!*/ context, BlockParam block, IDictionary<object, object>/*!*/ self) {
            RubyArray list = new RubyArray();

            foreach (var pair in CopyKeyValuePairs(self)) {
                object result;
                if (block.Yield(BaseSymbolDictionary.ObjToNull(pair.Key), pair.Value, out result)) {
                    return result;
                }

                // Select the key, unless 'false' or 'nil' is returned
                if (RubyOps.IsTrue(result)) {
                    list.Add(MakeArray(pair));
                }
            }

            return list;
        }

        [RubyMethod("shift")]
        public static object Shift(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

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
        public static object Sort(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,            
            BlockParam block, IDictionary<object, object>/*!*/ self) {
            return ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, block, ToArray(self));
        }

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(IDictionary<object, object>/*!*/ self) {
            RubyArray result = new RubyArray(self.Count);
            foreach (KeyValuePair<object, object> pair in self) {
                result.Add(MakeArray(pair));
            }
            return result;
        }

        [RubyMethod("to_hash")]
        public static IDictionary<object, object> ToHash(IDictionary<object, object>/*!*/ self) {
            return self;
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToMutableString(ConversionStorage<MutableString>/*!*/ tosConversion, 
            IDictionary<object, object>/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteToSTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.CreateAscii("{...}");
                } else {
                    return IListOps.Join(tosConversion, ToArray(self));
                }
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

        #endregion
    }

}
