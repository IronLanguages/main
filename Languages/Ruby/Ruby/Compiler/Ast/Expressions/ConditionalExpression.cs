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

namespace IronRuby.Compiler.Ast {

    /// <summary>
    /// Lightweight form of if-then-else-end expression. 
    /// Used for ternary ?: operator.
    /// </summary>
    public partial class ConditionalExpression : Expression {
        private readonly Expression/*!*/ _condition;
        private readonly Expression/*!*/ _trueExpression;
        private readonly Expression/*!*/ _falseExpression;

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public Expression/*!*/ TrueExpression {
            get { return _trueExpression; }
        }

        public Expression/*!*/ FalseExpression {
            get { return _falseExpression; }
        }
        
        public ConditionalExpression(Expression/*!*/ condition, Expression/*!*/ trueExpression, Expression/*!*/ falseExpression, SourceSpan location) 
            : base(location) {
            Assert.NotNull(condition, trueExpression, falseExpression);
            _condition = condition;
            _trueExpression = trueExpression;
            _falseExpression = falseExpression;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return AstFactory.Condition(
                _condition.TransformReadBoolean(gen, true),
                _trueExpression.TransformRead(gen),
                _falseExpression.TransformRead(gen)
            );
        }
    }
}
