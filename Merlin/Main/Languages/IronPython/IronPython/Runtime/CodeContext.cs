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
                return _scope.ModuleScope;
            }
        }

        public PythonContext LanguageContext {
            get {
                return _languageContext;
            }
        }
    }
}
