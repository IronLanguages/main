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
using Microsoft.Scripting;
using System.Text;

namespace IronPython.Compiler.Ast {
    public class DottedName : Node {
        // TODO: Make string[] 
        private readonly SymbolId[] _names;

        public DottedName(SymbolId[] names) {
            _names = names;
        }

        public IList<SymbolId> Names {
            get { return _names; }
        }

        public string MakeString() {
            if (_names.Length == 0) return String.Empty;

            StringBuilder ret = new StringBuilder(SymbolTable.IdToString(_names[0]));
            for (int i = 1; i < _names.Length; i++) {
                ret.Append('.');
                ret.Append(SymbolTable.IdToString(_names[i]));
            }
            return ret.ToString();
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }
}
