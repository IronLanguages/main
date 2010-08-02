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
    public partial class IsDefinedExpression : Expression {
        private readonly Expression/*!*/ _expression;

        public Expression/*!*/ Expression {
            get { return _expression; }
        }

        public IsDefinedExpression(Expression/*!*/ expression, SourceSpan location)
            : base(location) {
            Assert.NotNull(expression);

            _expression = expression;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return _expression.TransformIsDefined(gen);
        }

        internal override MSA.Expression/*!*/ TransformReadBoolean(AstGenerator/*!*/ gen, bool positive) {
            return _expression.TransformBooleanIsDefined(gen, positive);
        }
    }
}
