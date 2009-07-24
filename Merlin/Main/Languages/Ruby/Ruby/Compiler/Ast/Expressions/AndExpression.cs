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
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// TODO: unify with OrExpression (?)
    /// </summary>
    public partial class AndExpression : Expression {
        private readonly Expression/*!*/ _left;
        private readonly Expression/*!*/ _right;

        public Expression/*!*/ Left {
            get { return _left; }
        }

        public Expression/*!*/ Right {
            get { return _right; }
        }

        public AndExpression(Expression/*!*/ left, Expression/*!*/ right, SourceSpan location) 
            : base(location) {
            Assert.NotNull(left, right);

            _left = left;
            _right = right;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return TransformRead(gen, _left.TransformRead(gen), _right.TransformRead(gen));
        }

        internal static MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression/*!*/ left, MSA.Expression/*!*/ right) {
            MSA.ParameterExpression temp;
            MSA.Expression result = AstUtils.CoalesceTrue(
                AstFactory.Box(left),
                AstFactory.Box(right),
                Methods.IsTrue,
                out temp
            );

            gen.CurrentScope.AddHidden(temp);
            return result;
        }

        internal override Expression/*!*/ ToCondition(LexicalScope/*!*/ currentScope) {
            var newLeft = _left.ToCondition(currentScope);
            var newRight = _right.ToCondition(currentScope);

            if (newLeft != _left || newRight != _right) {
                return new AndExpression(newLeft, newRight, Location);
            }

            return this;
        }
    }
}
