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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class IfExpression : Expression {
        private Expression/*!*/ _condition;
        private Statements _body;
        private List<ElseIfClause> _elseIfClauses;

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public Statements Body {
            get { return _body; }
        }

        public List<ElseIfClause> ElseIfClauses {
            get { return _elseIfClauses; }
        }

        public IfExpression(Expression/*!*/ condition, Statements/*!*/ body, List<ElseIfClause>/*!*/ elseIfClauses, SourceSpan location)
            : base(location) {
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(elseIfClauses, "elseIfClauses");

            // all but the last clause should have non-null conditions:
            for (int i = 0; i < elseIfClauses.Count - 1; i++) {
                if (elseIfClauses[i].Condition == null) {
                    throw ExceptionUtils.MakeArgumentItemNullException(i, "elseIfClauses");
                }
            }

            _condition = condition;
            _body = body;
            _elseIfClauses = elseIfClauses;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {

            MSA.Expression result;

            int i = _elseIfClauses.Count - 1;

            if (i >= 0 && _elseIfClauses[i].Condition == null) {
                // ... else body end
                result = gen.TransformStatementsToExpression(_elseIfClauses[i].Statements);
                i--;
            } else {
                // no else clause => the result of the if-expression is nil:
                result = AstUtils.Constant(null);
            }

            while (i >= 0) {
                // emit: else (if (condition) body else result)
                result = AstFactory.Condition(
                    _elseIfClauses[i].Condition.TransformCondition(gen, true),
                    gen.TransformStatementsToExpression(_elseIfClauses[i].Statements),
                    result
                );
                i--;
            }

            // if (condition) body else result
            return AstFactory.Condition(
                _condition.TransformCondition(gen, true),
                gen.TransformStatementsToExpression(_body),
                result
            );
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            // do not mark a sequence point wrapping the entire condition:
            return TransformRead(gen);
        }
    }
}
