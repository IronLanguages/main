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
        private ScriptCode _sc;

        public ModuleLoader(ScriptCode sc) {
            _sc = sc;
        }

        public Scope load_module(CodeContext/*!*/ context, string fullName) {
            PythonContext pc = PythonContext.GetContext(context);

            return pc.CreateModule(_sc.SourceUnit.Path, _sc.CreateScope(), _sc, ModuleOptions.Initialize).Scope;
        }
    }

}
