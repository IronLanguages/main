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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;

namespace IronPython.Runtime {
    /// <summary>
    /// Abstract base class for all PythonDictionary storage.
    /// 
    /// Defined as a class instead of an interface for performance reasons.  Also not
    /// using IDictionary* for keeping a simple interface.
    /// 
    /// Full locking is defined as being on the DictionaryStorage object it's self,
    /// not an internal member.  This enables subclasses to provide their own locking
    /// aruond large operations and call lock free functions.
    /// </summary>
    [Serializable]
    internal abstract class DictionaryStorage  {
        public abstract void Add(object key, object value);

        public virtual void AddNoLock(object key, object value) {
            Add(key, value);
        }

        public virtual void Add(SymbolId key, object value) {
            Add(SymbolTable.IdToString(key), value);
        }

        public abstract bool Contains(object key);
        
        public virtual bool Contains(SymbolId key) {
            return Contains(SymbolTable.IdToString(key));
        }

        public abstract bool Remove(object key);
        public abstract bool TryGetValue(object key, out object value);

        public virtual bool TryGetValue(SymbolId key, out object value) {
            return TryGetValue(SymbolTable.IdToString(key), out value);
        }

        public virtual bool TryRemoveValue(object key, out object value) {
            if (TryGetValue(key, out value)) {
                return Remove(key);
                
            }

            return false;
        }

        public abstract int Count { get; }
        public abstract void Clear();
        public virtual bool HasNonStringAttributes() {
            foreach (KeyValuePair<object, object> o in GetItems()) {
                if (!(o.Key is string)) {
                    return true;
                }
            }
            return false;
        }

        public abstract List<KeyValuePair<object, object>> GetItems();

        public virtual IEnumerable<object>/*!*/ GetKeys() {
            foreach (var o in GetItems()) {
                yield return o.Key;
            }
        }

        public virtual DictionaryStorage Clone() {
            CommonDictionaryStorage storage = new CommonDictionaryStorage();
            foreach (KeyValuePair<object, object> kvp in GetItems()) {
                storage.Add(kvp.Key, kvp.Value);
            }
            return storage;
        }
        
        /// <summary>
        /// Adds items from this dictionary into the other dictionary
        /// </summary>
        public virtual void CopyTo(DictionaryStorage/*!*/ into) {
            Debug.Assert(into != null);

            foreach (KeyValuePair<object, object> kvp in GetItems()) {
                into.Add(kvp.Key, kvp.Value);
            }
        }
    }

}
