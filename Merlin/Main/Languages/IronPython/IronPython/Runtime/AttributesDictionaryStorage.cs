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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Threading;

namespace IronPython.Runtime {
    [Serializable]
    internal class AttributesDictionaryStorage : DictionaryStorage {
        private IAttributesCollection _data;
        private readonly CommonDictionaryStorage _hidden;

        public AttributesDictionaryStorage(IAttributesCollection data) {
            Debug.Assert(data != null);

            _hidden = new CommonDictionaryStorage();
            foreach (var key in data.Keys) {
                string strKey = key as string;
                if (strKey != null && strKey.Length > 0 && strKey[0] == '$') {
                    _hidden.Add(strKey, null);
                }
            }

            _data = data;
        }

        public override void Add(object key, object value) {
            AddNoLock(key, value);
        }

        public override void AddNoLock(object key, object value) {
            _hidden.Remove(key);

            string strKey = key as string;
            if (strKey != null) {
                _data[SymbolTable.StringToId(strKey)] = value;
            } else {
                _data.AddObjectKey(key, value);
            }
        }

        public override bool Contains(object key) {
            if (_hidden.Contains(key)) {
                return false;
            }

            string strKey = key as string;
            if (strKey != null) {
                return _data.ContainsKey(SymbolTable.StringToId(strKey));
            } else {
                return _data.ContainsObjectKey(key);
            }
        }

        public override bool Remove(object key) {
            if (_hidden.Contains(key)) {
                return false;
            }
            string strKey = key as string;
            if (strKey != null) {
                return _data.Remove(SymbolTable.StringToId(strKey));
            } else {
                return _data.RemoveObjectKey(key);
            }
        }

        public override bool TryGetValue(object key, out object value) {
            if (_hidden.Contains(key)) {
                value = null;
                return false;
            }

            string strKey = key as string;
            if (strKey != null) {
                return _data.TryGetValue(SymbolTable.StringToId(strKey), out value);
            }

            return _data.TryGetObjectValue(key, out value);
        }

        public override int Count {
            get {
                return _data.Count - _hidden.Count;
            }
        }

        public override void Clear() {
            _data = new SymbolDictionary();
            _hidden.Clear();
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>(Count);
            foreach (var kvp in _data) {
                if (!_hidden.Contains(kvp.Key)) {
                    res.Add(kvp);
                }
            }
            return res;
        }

        public override bool HasNonStringAttributes() {
            return true;
        }
    }
}
