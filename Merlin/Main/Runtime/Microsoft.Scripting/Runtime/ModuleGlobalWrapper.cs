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

using System.Diagnostics;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Runtime {
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
    public sealed class ModuleGlobalWrapper {
        private object _value;
        private ModuleGlobalCache _global;
        private SymbolId _name;
        private CodeContext _context;

        public ModuleGlobalWrapper(CodeContext context, SymbolId name) {
            _value = Uninitialized.Instance;
            _context = context;
            _global = context.LanguageContext.GetModuleCache(name);
            _name = name;
        }

        public object CurrentValue {
            get {
                if (_value != Uninitialized.Instance) return _value;

                return GetCachedValue();
            }
            set {
                if (value == Uninitialized.Instance && _value == Uninitialized.Instance) {
                    throw _context.LanguageContext.MissingName(_name);

                }
                _value = value;
            }
        }

        public SymbolId Name { get { return _name; } }

        private object GetCachedValue() {
            if (_global.IsCaching) {
                if (_global.HasValue) return _global.Value;
            } else {
                object value;
                // HACK: Shouldn't look in the GlobalScope here, but need to until JSGlobalObject
                // unifies w/ module dictionary.
                if (_context.GlobalScope.TryGetName(_context.LanguageContext, _name, out value)) {
                    return value;
                }

                if (_context.LanguageContext.TryLookupGlobal(_context.Scope, _name, out value)) {
                    return value;
                }
            }

            // TODO: support returning undefined
            throw _context.LanguageContext.MissingName(_name);
        }

        public object RawValue {
            get {
                return _value;
            }
        }

        public string Display {
            get {
                if (_value != Uninitialized.Instance) return GetStringDisplay(_value);

                if (_global.IsCaching && _global.HasValue) return GetStringDisplay(_global.Value);
                object value;
                if (_context.LanguageContext.TryLookupGlobal(_context.Scope, _name, out value))
                    return GetStringDisplay(value);

                return GetStringDisplay(Uninitialized.Instance);
            }
        }

        private static string GetStringDisplay(object val) {
            return val == null ? "(null)" : val.ToString();
        }

        [Confined]
        public override string ToString() {
            return string.Format("ModuleGlobal: {0} Value: {1} ({2})",
                _name,
                _value,
                RawValue == Uninitialized.Instance ? "Module Local" : "Global");
        }
    }
}
