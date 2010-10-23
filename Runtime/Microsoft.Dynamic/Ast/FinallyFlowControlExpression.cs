/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Wrapping a tree in this node enables jumps from finally blocks
    /// It does this by generating control-flow logic in the tree
    /// 
    /// Reducing this node requires a full tree walk of its body
    /// (but not nested lambdas)
    /// 
    /// WARNING: this node cannot contain jumps across blocks, because it
    /// assumes any unknown jumps are jumps to an outer scope.
    /// </summary>
    public sealed class FinallyFlowControlExpression : Expression, IInstructionProvider {
        private readonly Expression _body;
        private Expression _reduced;

        internal FinallyFlowControlExpression(Expression body) {
            _body = body;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public sealed override Type Type {
            get { return Body.Type; }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public Expression Body {
            get { return _body; }
        }

        public override Expression Reduce() {
            if (_reduced == null) {
                _reduced = new FlowControlRewriter().Reduce(_body);
            }
            return _reduced;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            Expression b = visitor.Visit(_body);
            if (b == _body) {
                return this;
            }
            return new FinallyFlowControlExpression(b);
        }

        void IInstructionProvider.AddInstructions(LightCompiler compiler) {
            // the interpreter deals with jumps out of finally blocks just fine:
            compiler.Compile(_body);
        }
    }

    public partial class Utils {
        public static Expression FinallyFlowControl(Expression body) {
            return new FinallyFlowControlExpression(body);
        }
    }
}
