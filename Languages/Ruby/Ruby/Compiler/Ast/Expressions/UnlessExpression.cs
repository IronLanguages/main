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

#if FEATURE_CORE_DLR
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    public partial class UnlessExpression : Expression {

        private readonly Expression/*!*/ _condition;
        private readonly Statements _statements;
        private readonly ElseIfClause _elseClause;

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public Statements Statements {
            get { return _statements; }
        }

        public ElseIfClause ElseClause {
            get { return _elseClause; }
        }

        public UnlessExpression(Expression/*!*/ condition, Statements/*!*/ statements, ElseIfClause elseClause, SourceSpan location)
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
                _condition.TransformCondition(gen, false),            
                gen.TransformStatementsToExpression(_statements),
                gen.TransformStatementsToExpression(_elseClause != null ? _elseClause.Statements : null)
            );
        }
    }
}