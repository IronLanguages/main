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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;

    public partial class UnlessExpression : Expression {

        private readonly Expression/*!*/ _condition;
        private readonly List<Expression> _statements;
        private readonly ElseIfClause _elseClause;

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public List<Expression> Statements {
            get { return _statements; }
        }

        public ElseIfClause ElseClause {
            get { return _elseClause; }
        }

        public UnlessExpression(Expression/*!*/ condition, List<Expression>/*!*/ statements, ElseIfClause elseClause, SourceSpan location)
            : base(location) {
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(statements, "statements");
            ContractUtils.Requires(elseClause == null || elseClause.Condition == null, "elseClause", "No condition allowed.");

            _statements = statements;
            _condition = condition;
            _elseClause = elseClause;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return AstFactory.Condition(
                Methods.IsFalse.OpCall(AstFactory.Box(_condition.TransformRead(gen))),
                gen.TransformStatementsToExpression(_statements),
                gen.TransformStatementsToExpression(_elseClause != null ? _elseClause.Statements : null)
            );
        }
    }
}