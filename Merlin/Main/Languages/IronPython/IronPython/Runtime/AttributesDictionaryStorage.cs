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
using System.Threading;

namespace IronPython.Runtime {
    [Serializable]
    internal class AttributesDictionaryStorage : DictionaryStorage {
        private IAttributesCollection _data;

        public AttributesDictionaryStorage(IAttributesCollection data) {
            _data = data;
        }

        public override void Add(object key, object value) {
            AddNoLock(key, value);
        }

        public override void AddNoLock(object key, object value) {
            string strKey = key as string;
            if (strKey != null) {
                _data[SymbolTable.StringToId(strKey)] = value;
            } else {
                _data.AddObjectKey(key, value);
            }
        }

        public override bool Contains(object key) {
            if (_data == null) return false;

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
            }

            return _data.TryGetObjectValue(key, out value);
        }

        public override int Count {
            get {
                return _data.Count;
            }
        }

        public override void Clear() {
            _data = null;
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            return new List<KeyValuePair<object, object>>(_data);
        }

        public override bool HasNonStringAttributes() {
            return true;
        }
    }
}
