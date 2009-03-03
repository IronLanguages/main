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
using System.Collections.Generic;

using Microsoft.Scripting;

namespace IronPython.Runtime {
    abstract class CustomDictionaryStorage : DictionaryStorage {
        private readonly CommonDictionaryStorage/*!*/ _storage = new CommonDictionaryStorage();

        public override void Add(object key, object value) {
            if (key is string) {
                Add(SymbolTable.StringToId((string)key), value);
                return;
            }
            _storage.Add(key, value);
        }

        public override bool Contains(object key) {
            if (key is string) {
                return Contains(SymbolTable.StringToId((string)key));
            }

            return _storage.Contains(key);
        }

        public override bool Remove(object key) {
            if (key is string) {
                SymbolId id = SymbolTable.StringToId((string)key);

                return TryRemoveExtraValue(id) ?? _storage.Remove(key);

            }
            return _storage.Remove(key);
        }

        public override bool TryGetValue(object key, out object value) {
            if (key is string) {
                return TryGetValue(SymbolTable.StringToId((string)key), out value);
            }

            return _storage.TryGetValue(key, out value);
        }

        public override int Count {
            get { return GetItems().Count; }
        }

        public override void Clear() {
            _storage.Clear();
            foreach (var item in GetExtraItems()) {
                TryRemoveExtraValue(item.Key);
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = _storage.GetItems();

            foreach (var item in GetExtraItems()) {
                res.Add(new KeyValuePair<object, object>(SymbolTable.IdToString(item.Key), item.Value));
            }

            return res;
        }

        public override void Add(SymbolId key, object value) {
            if (TrySetExtraValue(key, value)) {
                return;
            }

            _storage.Add(key, value);
        }

        public override bool TryGetValue(SymbolId key, out object value) {
            if (TryGetExtraValue(key, out value)) {
                return true;
            }

            return _storage.TryGetValue(key, out value);
        }

        public override bool Contains(SymbolId key) {
            object dummy;
            if (TryGetExtraValue(key, out dummy)) {
                return true;
            }

            return _storage.Contains(key);
        }

        /// <summary>
        /// Gets all of the extra names and values stored in the dictionary.
        /// </summary>
        protected abstract IEnumerable<KeyValuePair<SymbolId, object>> GetExtraItems();

        /// <summary>
        /// Attemps to sets a value in the extra keys.  Returns true if the value is set, false if 
        /// the value is not an extra key.
        /// </summary>
        protected abstract bool TrySetExtraValue(SymbolId key, object value);

        /// <summary>
        /// Attempts to get a value from the extra keys.  Returns true if the value is an extra
        /// key and has a value.  False if it is not an extra key or doesn't have a value.
        /// </summary>
        protected abstract bool TryGetExtraValue(SymbolId key, out object value);

        /// <summary>
        /// Attempts to remove the key.  Returns true if the key is removed, false
        /// if the key was not removed, or null if the key is not an extra key.
        /// </summary>
        protected abstract bool? TryRemoveExtraValue(SymbolId key);
    }
}
