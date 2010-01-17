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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Represents a symbol literal encoded by the containing source file encoding.
    /// </summary>
    public partial class SymbolLiteral : StringLiteral {
        internal SymbolLiteral(object/*!*/ value, SourceSpan location) 
            : base(value, location) {
        }

        public SymbolLiteral(string/*!*/ value, SourceSpan location)
            : base(value, location) {
        }

        public SymbolLiteral(byte[]/*!*/ value, SourceSpan location)
            : base(value, location) {
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.CreateSymbolL.OpCall(StringConstructor.MakeConstant(Value), AstUtils.Constant(gen.Encoding));
        }
    }
}
