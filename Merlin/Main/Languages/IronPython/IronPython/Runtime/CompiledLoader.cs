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
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    public class CompiledLoader {
        private Dictionary<string, ScriptCode> _codes = new Dictionary<string, ScriptCode>();

        internal void AddScriptCode(ScriptCode code) {
            string name = code.SourceUnit.Path;
            name = name.Replace(Path.DirectorySeparatorChar, '.');
            if (name.EndsWith("__init__.py")) {
                name = name.Substring(0, name.Length - ".__init__.py".Length);
            }
            _codes[name] = code;
        }

        public ModuleLoader find_module(CodeContext/*!*/ context, string fullname, List path) {
            ScriptCode sc;
            if (_codes.TryGetValue(fullname, out sc)) {
                return new ModuleLoader(sc);
            }

            return null;
        }
    }
}
