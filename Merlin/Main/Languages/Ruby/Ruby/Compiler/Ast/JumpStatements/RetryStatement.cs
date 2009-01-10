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


    public partial class RetryStatement : JumpStatement {
        public RetryStatement(SourceSpan location)
            : base(null, location) {
        }

        // see Ruby Language.doc/Runtime/Control Flow Implementation/Retry
        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return TransformRetry(gen);
        }

        internal static MSA.Expression/*!*/ TransformRetry(AstGenerator/*!*/ gen) {
            // eval:
            if (gen.CompilerOptions.IsEval) {
                return Methods.EvalRetry.OpCall(gen.CurrentRfcVariable);
            }

            // rescue clause:
            if (gen.CurrentRescue != null) {
                return Ast.Block(
                    Ast.Assign(gen.CurrentRescue.RetryingVariable, Ast.Constant(true)),
                    Ast.Continue(gen.CurrentRescue.ContinueLabel),
                    Ast.Empty()
                );
            }

            // block:
            if (gen.CurrentBlock != null) {
                return gen.Return(Methods.BlockRetry.OpCall(gen.CurrentBlock.BfcVariable));
            }

            // primary frame:
            return gen.Return(Methods.MethodRetry.OpCall(gen.CurrentRfcVariable, gen.MakeMethodBlockParameterRead()));
        }
    }
}