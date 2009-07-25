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
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;

namespace IronPython.Runtime {
    /// <summary>
    /// Captures the state of a piece of code.
    /// </summary>    
    public sealed class CodeContext {
        private readonly Scope _scope;
        private readonly PythonContext _languageContext;
        private readonly CodeContext _parent;

        public CodeContext(Scope scope, PythonContext languageContext)
            : this(scope, languageContext, null) {
        }

        public CodeContext(Scope scope, PythonContext languageContext, CodeContext parent) {
            Assert.NotNull(languageContext);

            _languageContext = languageContext;
            _scope = scope;
            _parent = parent;
        }

        public CodeContext Parent {
            get { return _parent; }
        }

        public Scope Scope {
            get {
                return _scope;
            }
        }

        public Scope GlobalScope {
            get {
                Debug.Assert(_scope != null, "Global scope not available");
                return GetModuleScope(_scope);
            }
        }

        public PythonContext LanguageContext {
            get {
                return _languageContext;
            }
        }

        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.
        /// </summary>
        internal bool TryLookupName(SymbolId name, out object value) {
            Scope curScope = _scope;
            do {
                if (curScope == _scope || curScope.IsVisible) {
                    if (curScope.TryGetVariable(name, out value)) {
                        return true;
                    }
                }

                curScope = curScope.Parent;
            } while (curScope != null);

            value = null;
            return false;
        }

        internal bool TryLookupGlobal(SymbolId name, out object value) {
            object builtins;
            if (!GlobalScope.TryGetVariable(Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            Scope builtinsScope = builtins as Scope;
            if (builtinsScope != null && builtinsScope.TryGetVariable(name, out value)) return true;

            IAttributesCollection dict = builtins as IAttributesCollection;
            if (dict != null && dict.TryGetValue(name, out value)) return true;

            value = null;
            return false;
        }

        internal static Scope GetModuleScope(Scope scope) {
            Scope cur = scope;
            while (cur.Parent != null) {
                cur = cur.Parent;
            }

            return cur;
        }
    }
}
