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
    
    public partial class RedoStatement : JumpStatement {
        public RedoStatement(SourceSpan location)
            : base(null, location) {
        }

        // see Ruby Language.doc/Runtime/Control Flow Implementation/Redo
        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {

            // eval:
            if (gen.CompilerOptions.IsEval) {
                return Methods.EvalRedo.OpCall(gen.CurrentScopeVariable);
            }

            // loop:
            if (gen.CurrentLoop != null) {
                return Ast.Block(
                    Ast.Assign(gen.CurrentLoop.RedoVariable, AstUtils.Constant(true)),
                    Ast.Continue(gen.CurrentLoop.ContinueLabel),
                    AstUtils.Empty()
                );
            }

            // block:
            if (gen.CurrentBlock != null) {
                return Ast.Continue(gen.CurrentBlock.RedoLabel);
            }

            // method:
            return Methods.MethodRedo.OpCall(gen.CurrentScopeVariable);
        }
    }
}
