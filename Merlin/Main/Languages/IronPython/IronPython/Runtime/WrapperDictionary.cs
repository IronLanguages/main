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
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime {
    [Serializable]
    internal class WrapperDictionaryStorage : DictionaryStorage {
        private TopNamespaceTracker/*!*/ _data;

        public WrapperDictionaryStorage(TopNamespaceTracker/*!*/ data) {
            _data = data;
        }
      
        public override void Add(object key, object value) {
            string strKey = key as string;
            if (strKey != null) {
                _data[SymbolTable.StringToId(strKey)] = value;
            } else {
                _data.AddObjectKey(key, value);
            }
        }

        public override bool Contains(object key) {
            string strKey = key as string;
            if (strKey != null) {
                return _data.ContainsKey(SymbolTable.StringToId(strKey));
            } else {
                return _data.ContainsObjectKey(key);
            }
        }

        public override bool Remove(object key) {
            string strKey = key as string;
            if (strKey != null) {
                return _data.Remove(SymbolTable.StringToId(strKey));
            } else {
                return _data.RemoveObjectKey(key);
            }
        }

        public override bool TryGetValue(object key, out object value) {
            string strKey = key as string;
            if (strKey != null) {
                return _data.TryGetValue(SymbolTable.StringToId(strKey), out value);
            } else {
                return _data.TryGetObjectValue(key, out value);
            }
        }

        public override int Count {
            get {
                return _data.Count;
            }
        }

        public override void Clear() {
            ICollection<object> keys = _data.Keys;
            foreach (object key in keys) {
                _data.RemoveObjectKey(key);
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            return new List<KeyValuePair<object, object>>(_data);
        }
    }
}
