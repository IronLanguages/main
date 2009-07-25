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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    internal abstract class ScopeDictionaryStorage : DictionaryStorage {
        private Scope/*!*/ _scope;

        public ScopeDictionaryStorage(Scope/*!*/ scope) {
            _scope = scope;
        }

        public override void Add(object key, object value) {
            string strKey = key as string;
            if (strKey != null) {
                Scope.SetVariable(SymbolTable.StringToId(strKey), value);
            } else {
                Scope.SetObjectName(key, value);
            }
        }

        public override void Add(SymbolId key, object value) {
            Scope.SetVariable(key, value);
        }

        public override bool Contains(object key) {
            foreach (Scope scope in GetVisibleScopes()) {
                if (ScopeContains(key, scope)) {
                    return true;
                }
            }
            return false;
        }

        public override bool Contains(SymbolId key) {
            foreach (Scope scope in GetVisibleScopes()) {
                if (scope.ContainsVariable(key)) {
                    return true;
                }
            }
            return false;
        }
        
        public override bool Remove(object key) {
            foreach (Scope scope in GetVisibleScopes()) {
                string strKey = key as string;
                if (strKey != null) {
                    if (scope.TryRemoveVariable(SymbolTable.StringToId(strKey))) {
                        return true;
                    }
                } else if (scope.TryRemoveObjectName(key)) {
                    return true;
                }
            }

            return false;
        }

        public override bool TryGetValue(object key, out object value) {
            foreach (Scope scope in GetVisibleScopes()) {
                string strKey = key as string;
                if (strKey != null) {
                    if (scope.TryGetVariable(SymbolTable.StringToId(strKey), out value)) {
                        return true;
                    }
                } else if (scope.TryGetObjectName(key, out value)) {
                    return true;
                }
            }

            value = null;
            return false;
        }

        public override bool TryGetValue(SymbolId key, out object value) {
            foreach (Scope scope in GetVisibleScopes()) {
                if (scope.TryGetVariable(key, out value)) {
                    return true;
                }
            }

            value = null;
            return false;
        }

        public override int Count {
            get {
                int count = 0;
                foreach (Scope scope in GetVisibleScopes()) {
                    count += scope.Dict.Count;
                }
                return count;
            }
        }

        public override void Clear() {
            foreach (Scope scope in GetVisibleScopes()) {
                scope.Clear();
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            return new List<KeyValuePair<object, object>>(Scope.GetAllItems());
        }

        protected Scope/*!*/ Scope {
            get {
                return _scope;
            }
        }

        protected abstract IEnumerable<Scope>/*!*/ GetVisibleScopes();

        private static bool ScopeContains(object key, Scope scope) {
            string strKey = key as string;
            if (strKey != null) {
                object dummy;
                if (scope.TryGetVariable(SymbolTable.StringToId(strKey), out dummy)) {
                    return true;
                }
            } else {
                object dummy;
                if (scope.TryGetObjectName(key, out dummy)) {
                    return true;
                }
            }
            return false;
        }
    }
}
