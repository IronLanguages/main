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
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    class ObjectAttributesAdapter  : DictionaryStorage {
        private readonly object _backing;
        private readonly CodeContext/*!*/ _context;

        public ObjectAttributesAdapter(CodeContext/*!*/ context, object backing) {
            _backing = backing;
            _context = context;
        }

        internal object Backing {
            get {
                return _backing;
            }
        }
#if FALSE
        #region IAttributesCollection Members

        public object this[SymbolId name] {
            get {
                object res;
                if (TryGetValue(name, out res)) return res;

                throw PythonOps.NameError(name);
            }
            set {
                Add(name, value);
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void AddObjectKey(object name, object value) {
            PythonOps.SetIndex(_backing, name, value);
        }

        public bool TryGetObjectValue(object name, out object value) {
            try {
                value = PythonOps.GetIndex(_backing, name);
                return true;
            } catch (KeyNotFoundException) {
                // return false
            }
            value = null;
            return false;
        }

        public bool RemoveObjectKey(object name) {
            try {
                PythonOps.DelIndex(_backing, name);
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        public bool ContainsObjectKey(object name) {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count {
            get { return PythonOps.Length(_backing); }
        }

        public ICollection<object> Keys {
            get { return (ICollection<object>)Converter.Convert(PythonOps.Invoke(_backing, Symbols.Keys), typeof(ICollection<object>)); }
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
#endif

        public override void Add(object key, object value) {
            PythonContext.GetContext(_context).SetIndex(_backing, key, value);
        }

        public override bool Contains(object key) {
            object dummy;
            return TryGetValue(key, out dummy);
        }

        public override bool Remove(object key) {
            try {
                PythonContext.GetContext(_context).DelIndex(_backing, key);
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        public override bool TryGetValue(object key, out object value) {
            try {
                value = PythonOps.GetIndex(_context, _backing, key);
                return true;
            } catch (KeyNotFoundException) {
                // return false
            }
            value = null;
            return false;
        }

        public override int Count {
            get { return PythonOps.Length(_backing);  }
        }

        public override void Clear() {
            PythonOps.Invoke(_context, _backing, SymbolTable.StringToId("clear"));
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>();
            foreach (object o in Keys) {
                object val;
                TryGetValue(o, out val);

                res.Add(new KeyValuePair<object, object>(o, val));            
            }
            return res;
        }

        private ICollection<object> Keys {
            get { return (ICollection<object>)Converter.Convert(PythonOps.Invoke(_context, _backing, Symbols.Keys), typeof(ICollection<object>)); }
        }
    }
}
