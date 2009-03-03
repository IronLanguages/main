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

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Compiler;

namespace IronPython.Runtime {
    class GlobalDictionaryStorage : CustomDictionaryStorage {
        private readonly Dictionary<SymbolId, PythonGlobal/*!*/>/*!*/ _globals;
        private readonly PythonGlobal/*!*/[] _data;

        public GlobalDictionaryStorage(Dictionary<SymbolId, PythonGlobal/*!*/>/*!*/ globals) {
            Assert.NotNull(globals);
            
            _globals = globals;
        }

        public GlobalDictionaryStorage(Dictionary<SymbolId, PythonGlobal/*!*/>/*!*/ globals, PythonGlobal/*!*/[]/*!*/ data) {
            Assert.NotNull(globals, data);

            _globals = globals;
            _data = data;
        }

        protected override IEnumerable<KeyValuePair<SymbolId, object>> GetExtraItems() {
            foreach (KeyValuePair<SymbolId, PythonGlobal> global in _globals) {
                if (global.Value.RawValue != Uninitialized.Instance) {
                    yield return new KeyValuePair<SymbolId, object>(global.Key, global.Value.RawValue);
                }
            }
        }

        protected override bool? TryRemoveExtraValue(SymbolId key) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global)) {
                if (global.RawValue != Uninitialized.Instance) {
                    global.RawValue = Uninitialized.Instance;
                    return true;
                } else {
                    return false;
                }
            }
            return null;
        }

        protected override bool TrySetExtraValue(Microsoft.Scripting.SymbolId key, object value) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global)) {
                global.CurrentValue = value;
                return true;
            }
            return false;
        }

        protected override bool TryGetExtraValue(Microsoft.Scripting.SymbolId key, out object value) {
            PythonGlobal global;
            if (_globals.TryGetValue(key, out global) && global.RawValue != Uninitialized.Instance) {
                value = global.RawValue;
                return true;
            }

            value = null;
            return false;
        }

        public PythonGlobal/*!*/[] Data {
            get {
                return _data;
            }
        }
    }
}
