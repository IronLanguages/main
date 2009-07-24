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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    public sealed class ModuleLoader {
        private readonly ScriptCode _sc;
        private readonly string _parentName, _name;


        internal ModuleLoader(ScriptCode sc, string parentName, string name) {
            _sc = sc;
            _parentName = parentName;
            _name = name;
        }

        public Scope load_module(CodeContext/*!*/ context, string fullName) {
            PythonContext pc = PythonContext.GetContext(context);

            Scope res = pc.CreateModule(_sc.SourceUnit.Path, _sc.CreateScope(), _sc, ModuleOptions.Initialize).Scope;

            if (_parentName != null) {
                // if we are a module in a package update the parent package w/ our scope.
                object parent;
                if (pc.SystemStateModules.TryGetValue(_parentName, out parent)) {
                    Scope s = parent as Scope;
                    if (s != null) {
                        s.SetVariable(SymbolTable.StringToId(_name), res);
                    }
                }
            }

            return res;
        }
    }

}
