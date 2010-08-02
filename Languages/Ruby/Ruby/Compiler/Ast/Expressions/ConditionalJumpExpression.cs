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

    /// <summary>
    /// Represents {condition} {and/or/&&/||} {jump-statement}, 
    /// or {condition} ? {jump-statement} : {value}.
    /// </summary>
    public partial class ConditionalJumpExpression : Expression {
        private readonly bool _negateCondition;
        private readonly Expression/*!*/ _condition;
        private readonly Expression _value;
        private readonly JumpStatement/*!*/ _jumpStatement;

        public bool NegateCondition {
            get { return _negateCondition; }
        }

        public bool IsBooleanExpression {
            get { return _value == null; }
        }

        public Expression/*!*/ Condition {
            get { return _condition; }
        }

        public Expression Value {
            get { return _value; }
        }

        public JumpStatement/*!*/ JumpStatement {
            get { return _jumpStatement; }
        }

        public ConditionalJumpExpression(Expression/*!*/ condition, JumpStatement/*!*/ jumpStatement, bool negateCondition, Expression value, SourceSpan location)
            : base(location) {
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(jumpStatement, "jumpStatement");

            _condition = condition;
            _jumpStatement = jumpStatement;
            _negateCondition = negateCondition;
            _value = value;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            if (_value != null) {
                return Ast.Block(
                    AstUtils.IfThen(
                        _condition.TransformReadBoolean(gen, !_negateCondition),
                        _jumpStatement.Transform(gen)
                    ),
                    _value.TransformRead(gen)
                );
            } else {
                MSA.Expression tmpVariable = gen.CurrentScope.DefineHiddenVariable("#tmp_cond", typeof(object));
                return Ast.Block(
                    Ast.Assign(tmpVariable, AstUtils.Box(_condition.TransformRead(gen))),
                    AstUtils.IfThen(
                        (_negateCondition ? Methods.IsFalse : Methods.IsTrue).OpCall(tmpVariable),
                        _jumpStatement.Transform(gen)
                    ),
                    tmpVariable
                );
            }
        }
    }
}
