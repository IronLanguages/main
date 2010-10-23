/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

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
                return Methods.EvalRetry.OpCall(gen.CurrentScopeVariable);
            }

            // rescue clause:
            if (gen.CurrentRescue != null) {
                return Ast.Block(
                    Ast.Assign(gen.CurrentRescue.RetryingVariable, AstUtils.Constant(true)),
                    Ast.Goto(gen.CurrentRescue.RetryLabel, typeof(void))
                );
            }

            // block:
            if (gen.CurrentBlock != null) {
                return gen.Return(Methods.BlockRetry.OpCall(gen.CurrentBlock.BfcVariable));
            }

            // primary frame:
            return gen.Return(Methods.MethodRetry.OpCall(gen.CurrentScopeVariable, gen.MakeMethodBlockParameterRead()));
        }
    }
}