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

using IronPython.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// A ScriptCode which has been loaded from an assembly which is saved on disk.
    /// </summary>
    class OnDiskScriptCode : ScriptCode {
        private readonly Func<Scope, LanguageContext, object> _code;
        private Scope _optimizedScope;
        private readonly string _moduleName;

        public OnDiskScriptCode(Func<Scope, LanguageContext, object> code, SourceUnit sourceUnit, string moduleName) :
            base(sourceUnit) {
            _code = code;
            _moduleName = moduleName;
        }

        public override object Run() {
            return _code(CreateScope(), SourceUnit.LanguageContext);
        }

        public override object Run(Scope scope) {
            if (scope == CreateScope()) {
                return Run();
            }

            throw new NotSupportedException();
        }

        public string ModuleName {
            get {
                return _moduleName;
            }
        }

        public override Scope CreateScope() {
            if (_optimizedScope == null) {
                CachedOptimizedCodeAttribute[] attrs = (CachedOptimizedCodeAttribute[])_code.Method.GetCustomAttributes(typeof(CachedOptimizedCodeAttribute), false);

                // create the CompilerContext for the ScriptCode
                CachedOptimizedCodeAttribute optimizedCode = attrs[0];

                // create the storage for the global scope
                Dictionary<SymbolId, PythonGlobal> globals = new Dictionary<SymbolId, PythonGlobal>();
                PythonGlobal[] globalArray = new PythonGlobal[optimizedCode.Names.Length];
                Scope scope = new Scope(new PythonDictionary(new GlobalDictionaryStorage(globals, globalArray)));

                CodeContext res = new CodeContext(scope, (PythonContext)SourceUnit.LanguageContext);

                for (int i = 0; i < optimizedCode.Names.Length; i++) {
                    SymbolId name = SymbolTable.StringToId(optimizedCode.Names[i]);
                    globalArray[i] = globals[name] = new PythonGlobal(res, name);                    
                }

                _optimizedScope = scope;
            }
            return _optimizedScope;
        }
    }
}
