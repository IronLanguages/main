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
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;

namespace IronPython.Compiler {
    class OnDiskScriptCode : ScriptCode {
        private readonly DlrMainCallTarget _code;
        private Scope _optimizedScope;
        
        public OnDiskScriptCode(DlrMainCallTarget code, SourceUnit sourceUnit) :
            base(null, code, sourceUnit) {
            _code = code;
        }

        public override Scope CreateScope() {
            if (_optimizedScope == null) {
                CachedOptimizedCodeAttribute[] attrs = (CachedOptimizedCodeAttribute[])_code.Method.GetCustomAttributes(typeof(CachedOptimizedCodeAttribute), false);

                // create the CompilerContext for the ScriptCode
                CachedOptimizedCodeAttribute optimizedCode = attrs[0];

                // create the storage for the global scope
                GlobalsDictionary dict = new GlobalsDictionary(SymbolTable.StringsToIds(optimizedCode.Names));

                // create the CodeContext for the code from the storage
                Scope scope = new Scope(dict);
                CodeContext context = new CodeContext(scope, SourceUnit.LanguageContext);

                // initialize the tuple
                IModuleDictionaryInitialization ici = dict as IModuleDictionaryInitialization;
                if (ici != null) {
                    ici.InitializeModuleDictionary(context);
                }

                _optimizedScope = scope;
            }
            return _optimizedScope;
        }

        public override void EnsureCompiled() {
            CreateScope();
        }
    }
}
