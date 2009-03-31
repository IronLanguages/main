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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

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
    public sealed class FinallyFlowControlExpression : Expression {
        private readonly Expression _body;
        private Expression _reduced;

        internal FinallyFlowControlExpression(Expression body) {
            _body = body;
        }

        public override bool CanReduce {
            get { return true; }
        }

        protected override Type TypeImpl() {
            return Body.Type;
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
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

        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            Expression b = visitor(_body);
            if (b == _body) {
                return this;
            }
            return new FinallyFlowControlExpression(b);
        }
    }

    public partial class Utils {
        public static Expression FinallyFlowControl(Expression body) {
            return new FinallyFlowControlExpression(body);
        }
    }
}
