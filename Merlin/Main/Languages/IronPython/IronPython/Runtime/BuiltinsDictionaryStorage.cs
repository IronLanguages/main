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

        public BuiltinsDictionaryStorage(EventHandler<ModuleChangeEventArgs/*!*/>/*!*/ change)
            : base(typeof(Builtin)) {
            _change = change;
        }

        public override void Add(object key, object value) {
            if (key is string) { 
                _change(this, new ModuleChangeEventArgs(SymbolTable.StringToId((string)key), ModuleChangeType.Set, value));
            }
            base.Add(key, value);
        }

        protected override void LazyAdd(object name, object value) {
            base.Add(name, value);
        }

        public override void Add(SymbolId key, object value) {
            _change(this, new ModuleChangeEventArgs(key, ModuleChangeType.Set, value));
            base.Add(key, value);
        }

        public override bool Remove(object key) {
            if (key is string) {
                _change(this, new ModuleChangeEventArgs(SymbolTable.StringToId((string)key), ModuleChangeType.Delete));
            }
            return base.Remove(key);
        }        
    }
}
