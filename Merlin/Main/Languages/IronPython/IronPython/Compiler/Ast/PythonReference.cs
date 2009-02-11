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

using Microsoft.Scripting;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    class PythonReference {
        private SymbolId _name;
        private PythonVariable _variable;

        public PythonReference(SymbolId name) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        internal PythonVariable PythonVariable {
            get { return _variable; }
            set { _variable = value; }
        }

        internal MSAst.Expression Variable {
            get { return _variable != null ? _variable.Variable : null; }
        }
    }
}
