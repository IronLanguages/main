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
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    public partial class OrExpression : Expression {
        private readonly Expression/*!*/ _left;
        private readonly Expression/*!*/ _right;

        public Expression/*!*/ Left {
            get { return _left; }
        }

        public Expression/*!*/ Right {
            get { return _right; }
        }

        public OrExpression(Expression/*!*/ left, Expression/*!*/ right, SourceSpan location)
            : base(location) {
            Assert.NotNull(left, right);

            _left = left;
            _right = right;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return TransformRead(gen, _left.TransformRead(gen), _right.TransformRead(gen));
        }

        internal override MSA.Expression/*!*/ TransformReadBoolean(AstGenerator/*!*/ gen, bool positive) {
            return AstFactory.Logical(_left.TransformReadBoolean(gen, positive), _right.TransformReadBoolean(gen, positive), !positive);
        }

        internal static MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression/*!*/ left, MSA.Expression/*!*/ right) {
            MSA.ParameterExpression temp;

            MSA.Expression result = AstUtils.CoalesceFalse(
                AstUtils.Box(left),
                AstUtils.Box(right),
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
                return new OrExpression(newLeft, newRight, Location);
            }

            return this;
        }
    }
}
