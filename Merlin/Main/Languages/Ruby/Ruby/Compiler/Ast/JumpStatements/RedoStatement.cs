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

using Microsoft.Scripting;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    
    public partial class RedoStatement : JumpStatement {
        public RedoStatement(SourceSpan location)
            : base(null, location) {
        }

        // see Ruby Language.doc/Runtime/Control Flow Implementation/Redo
        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {

            // eval:
            if (gen.CompilerOptions.IsEval) {
                return Methods.EvalRedo.OpCall(gen.CurrentRfcVariable);
            }

            // loop:
            if (gen.CurrentLoop != null) {
                return Ast.Block(
                    Ast.Assign(gen.CurrentLoop.RedoVariable, Ast.Constant(true)),
                    Ast.Continue(gen.CurrentLoop.ContinueLabel),
                    Ast.Empty()
                );
            }

            // block:
            if (gen.CurrentBlock != null) {
                return Ast.Continue(gen.CurrentBlock.RedoLabel);
            }

            // method:
            return Methods.MethodRedo.OpCall(gen.CurrentRfcVariable);
        }
    }
}
