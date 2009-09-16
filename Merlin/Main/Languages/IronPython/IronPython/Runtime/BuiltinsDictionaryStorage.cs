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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Modules;

namespace IronPython.Runtime {
    class BuiltinsDictionaryStorage : ModuleDictionaryStorage {
        private readonly EventHandler<ModuleChangeEventArgs/*!*/>/*!*/ _change;
        private object _import;

        public BuiltinsDictionaryStorage(EventHandler<ModuleChangeEventArgs/*!*/>/*!*/ change)
            : base(typeof(Builtin)) {
            _change = change;
        }

        public override void Add(object key, object value) {
            string strkey = key as string;
            if (strkey != null) {
                if (strkey == "__import__") {
                    _import = value;
                }
                _change(this, new ModuleChangeEventArgs(strkey, ModuleChangeType.Set, value));
            }
            base.Add(key, value);
        }
        
        protected override void LazyAdd(object name, object value) {
            base.Add(name, value);
        }

        public override bool Remove(object key) {
            string strkey = key as string;
            if (strkey != null) {
                if (strkey == "__import__") {
                    _import = null;
                }
                _change(this, new ModuleChangeEventArgs(strkey, ModuleChangeType.Delete));
            }
            return base.Remove(key);
        }

        public override void Clear() {
            _import = null;
            base.Clear();
        }

        public override bool TryGetImport(out object value) {
            if (_import == null) {
                if (base.TryGetImport(out value)) {
                    _import = value;
                    return true;
                }
                return false;
            }

            value = _import;
            return true;
        }

        public override void Reload() {
            _import = null;
            base.Reload();
        }
    }
}
