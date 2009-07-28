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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    /// <summary>
    /// Provides cached global variable for modules to enable optimized access to
    /// module globals.  Both the module global value and the cached value can be held
    /// onto and the cached value can be invalidated by the providing LanguageContext.
    /// 
    /// The cached value is provided by the LanguageContext.GetModuleCache API.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerDisplay("{Display}")]
#endif
    public sealed class PythonGlobal {
        private object _value;
        private ModuleGlobalCache _global;
        private SymbolId _name;
        private CodeContext/*!*/ _context;

        internal static PropertyInfo/*!*/ CurrentValueProperty = typeof(PythonGlobal).GetProperty("CurrentValue");
        internal static PropertyInfo/*!*/ RawValueProperty = typeof(PythonGlobal).GetProperty("RawValue");

        public PythonGlobal(CodeContext/*!*/ context, SymbolId name) {
            Assert.NotNull(context);

            _value = Uninitialized.Instance;
            _context = context;
            _name = name;
        }

        public object CurrentValue {
            get {
                if (_value != Uninitialized.Instance) {
                    return _value;
                }

                return GetCachedValue();
            }
            set {
                if (value == Uninitialized.Instance && _value == Uninitialized.Instance) {
                    throw PythonOps.GlobalNameError(_name);

                }
                _value = value;
            }
        }

        public SymbolId Name { get { return _name; } }

        private object GetCachedValue() {
            if (_global == null) {                
                _global = ((PythonContext)_context.LanguageContext).GetModuleGlobalCache(_name);
            }

            if (_global.IsCaching) {
                if (_global.HasValue) {
                    return _global.Value;
                }
            } else {
                object value;

                if (_context.TryLookupGlobal(_name, out value)) {
                    return value;
                }
            }

            throw PythonOps.GlobalNameError(_name);
        }

        public object RawValue {
            get {
                return _value;
            }
            internal set {
                _value = value;
            }
        }

        public string Display {
            get {
                try {
                    return GetStringDisplay(CurrentValue);
                } catch (MissingMemberException) {
                    return "<uninitialized>";
                }
            }
        }

        private static string GetStringDisplay(object val) {
            return val == null ? "(null)" : val.ToString();
        }

        public override string ToString() {
            return string.Format("ModuleGlobal: {0} Value: {1} ({2})",
                _name,
                _value,
                RawValue == Uninitialized.Instance ? "Module Local" : "Global");
        }


    }
}
