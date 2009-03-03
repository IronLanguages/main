using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

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
