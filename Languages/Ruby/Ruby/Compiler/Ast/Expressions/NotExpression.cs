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
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class NotExpression : Expression {
        private readonly Expression _expression;

        public Expression/*!*/ Expression {
            get { return _expression; }
        }

        public NotExpression(Expression expression, SourceSpan location) 
            : base(location) {
            _expression = expression;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            var target = (_expression != null) ? _expression.Transform(gen) : Ast.Constant(null);
            return MethodCall.TransformRead(this, gen, false, Symbols.Bang, target, null, null, null, null);
        }

        internal override MSA.Expression/*!*/ TransformReadBoolean(AstGenerator/*!*/ gen, bool positive) {
            return (positive ? Methods.IsTrue : Methods.IsFalse).OpCall(AstUtils.Box(TransformRead(gen)));
        }

        internal override Expression/*!*/ ToCondition(LexicalScope/*!*/ currentScope) {
            if (_expression != null) {
                var newExpression = _expression.ToCondition(currentScope);
                if (newExpression != _expression) {
                    return new NotExpression(newExpression, Location);
                }
            }

            return this;
        }
    }
}
