/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using MSA = System.Linq.Expressions;
    
    public partial class SymbolLiteral : Expression {
        private readonly string/*!*/ _value;

        public string/*!*/ Value {
            get { return _value; }
        }

        internal SymbolLiteral(string/*!*/ value, SourceSpan location)
            : base(location) {
            _value = value;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.CreateSymbolB.OpCall(Ast.Constant(_value));
        }
    }
}
