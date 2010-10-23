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
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using Ast = MSA.Expression;

    /// <summary>
    /// Represents if- and unless- statements.
    /// Also used for (condition) ? jump-statement : jump-statement.
    /// </summary>
    public partial class ConditionalStatement : Expression {
        private readonly Expression/*!*/ _condition;
        private readonly Expression/*!*/ _body;
        private readonly Expression _elseStatement;
        private readonly bool _negateCondition;

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public Expression/*!*/ Body {
            get { return _body; }
        }

        public Expression ElseStatement {
            get { return _elseStatement; }
        }

        public bool IsUnless {
            get { return _negateCondition; }
        }
        
        public ConditionalStatement(Expression/*!*/ condition, bool negateCondition, Expression/*!*/ body, Expression elseStatement, SourceSpan location) 
            : base(location) {
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(body, "body");

            _condition = condition;
            _body = body;
            _negateCondition = negateCondition;
            _elseStatement = elseStatement;
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return AstUtils.IfThenElse(
                _condition.TransformCondition(gen, !_negateCondition), 
                _body.Transform(gen),
                _elseStatement != null ? _elseStatement.Transform(gen) : AstUtils.Empty()
            );
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Ast.Condition(
                _condition.TransformReadBoolean(gen, !_negateCondition),
                AstUtils.Box(_body.TransformRead(gen)),
                (_elseStatement != null) ? AstUtils.Box(_elseStatement.TransformRead(gen)) : (MSA.Expression)AstUtils.Constant(null)
            );
        }
    }
}
