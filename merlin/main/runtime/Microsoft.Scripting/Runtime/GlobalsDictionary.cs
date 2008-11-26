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
using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Intended for internal use to initialization optimized module dictionaries.  Exposed publicly because 
    /// generated types implement this interface.
    /// </summary>
    public interface IModuleDictionaryInitialization {
        void InitializeModuleDictionary(CodeContext context);
    }

    /// <summary>
    /// Dictionary backed by an array used for collectable globals. See also
    /// GlobalArrayRewriter
    /// </summary>
    public sealed class GlobalsDictionary : CustomSymbolDictionary, IModuleDictionaryInitialization {
        private readonly ModuleGlobalWrapper[] _data;
        private readonly SymbolId[] _names;

        // lazily created mapping
        private Dictionary<SymbolId, int> _indexes;

        public GlobalsDictionary(SymbolId[] names) {
            Debug.Assert(names != null);

            _data = new ModuleGlobalWrapper[names.Length];
            _names = names;
        }
        
        internal ModuleGlobalWrapper[] Data {
            get { return _data; }
        }

        public override SymbolId[] GetExtraKeys() {
            return _names;
        }

        protected internal override bool TryGetExtraValue(SymbolId key, out object value) {
            EnsureIndexes();

            int index;
            if (_indexes.TryGetValue(key, out index)) {
                object raw = _data[index].RawValue;
                if (raw != Uninitialized.Instance) {
                    value = raw;
                    return true;
                }
            }

            value = null;
            return false;
        }

        protected internal override bool TrySetExtraValue(SymbolId key, object value) {
            EnsureIndexes();

            int index;
            if (_indexes.TryGetValue(key, out index)) {
                _data[index].CurrentValue = value;
                return true;
            }
            return false;
        }

        private void EnsureIndexes() {
            if (_indexes == null) {
                int count = _names.Length;
                Dictionary<SymbolId, int> indexes = new Dictionary<SymbolId, int>(count);
                for (int index = 0; index < count; index++) {
                    indexes[_names[index]] = index;
                }
                _indexes = indexes;
            }
        }

        void IModuleDictionaryInitialization.InitializeModuleDictionary(CodeContext context) {
            if (_names.Length == 0) {
                return;
            }

            if (_data[0] != null) {
                throw Error.AlreadyInitialized();
            }

            for (int i = 0; i < _names.Length; i++) {
                _data[i] = new ModuleGlobalWrapper(context, _names[i]);
            }
        }
    }
}
