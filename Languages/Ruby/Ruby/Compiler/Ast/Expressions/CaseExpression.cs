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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    
    public partial class CaseExpression : Expression {
        
        //	case value 
        //		when args: statements
        //      ...
        //      when args: statements
        //  else
        //      statements
        //	end

        // equivalent to
        // value == null:
        //   if <expr> then <stmts> elseif <expr> then <stmts> else <stmts> end
        // value != null:
        //   if <expr> === <value> then <stmts> elseif <expr> === <value> then <stmts> else <stmts> end

        // the only tricky part is that the when clause can contain a splatted array:
        // case ...
        //     when arg0, ..., *argn: statements
        //     ...
        // end

        private readonly Expression _value;
        private readonly List<WhenClause> _whenClauses;
        private readonly Statements _elseStatements;

        public Expression Value {
            get { return _value; } 
        }

        public List<WhenClause> WhenClauses {
            get { return _whenClauses; }
        }

        public Statements ElseStatements {
            get { return _elseStatements; }
        }

        internal CaseExpression(Expression value, List<WhenClause> whenClauses, ElseIfClause elseClause, SourceSpan location)
            : this(value, whenClauses, (elseClause != null) ? elseClause.Statements : null, location) {
        }

        public CaseExpression(Expression value, List<WhenClause> whenClauses, Statements elseStatements, SourceSpan location)
            : base(location) {
            _value = value;
            _whenClauses = whenClauses;
            _elseStatements = elseStatements;
        }

        // when <expr>
        //   generates into:
        //   RubyOps.IsTrue(<expr>) if the case has no value, otherise:
        //   RubyOps.IsTrue(Call("===", <expr>, <value>))
        private static MSA.Expression/*!*/ MakeTest(AstGenerator/*!*/ gen, MSA.Expression/*!*/ expr, MSA.Expression value) {
            if (value != null) {
                expr = CallSiteBuilder.InvokeMethod(gen.Context, "===", RubyCallSignature.WithScope(1),
                    gen.CurrentScopeVariable,
                    expr,
                    value
                );
            }
            return AstFactory.IsTrue(expr);
        }

        // when [<expr>, ...] *<array>
        private static MSA.Expression/*!*/ MakeArrayTest(AstGenerator/*!*/ gen, MSA.Expression/*!*/ array, MSA.Expression value) {
            return Methods.ExistsUnsplat.OpCall(
                Ast.Constant(CallSite<Func<CallSite, object, object, object>>.Create(
                    RubyCallAction.Make(gen.Context, "===", RubyCallSignature.WithImplicitSelf(2))
                )),
                AstUtils.LightDynamic(ConvertToArraySplatAction.Make(gen.Context), array), 
                AstUtils.Box(value)
            );
        }

        // when <expr0>, ... [*<array>]
        // generates:
        // <MakeTest>(<expr0>) || <MakeTest>(<expr1>) || ... [ || <MakeArrayTest>(<array>) ]
        internal static MSA.Expression/*!*/ TransformWhenCondition(AstGenerator/*!*/ gen, Expression[] comparisons, 
            Expression comparisonArray, MSA.Expression value) {

            MSA.Expression result;
            if (comparisonArray != null) {
                result = MakeArrayTest(gen, comparisonArray.TransformRead(gen), value);
            } else {
                result = AstUtils.Constant(false);
            }

            if (comparisons != null) {
                for (int i = comparisons.Length - 1; i >= 0; i--) {
                    result = Ast.OrElse(
                        MakeTest(gen, comparisons[i].TransformRead(gen), value),
                        result
                    );
                }
            }

            return result;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            MSA.Expression result;
            if (_elseStatements != null) {
                // ... else body end
                result = gen.TransformStatementsToExpression(_elseStatements);
            } else {
                // no else clause => the result of the if-expression is nil:
                result = AstUtils.Constant(null);
            }

            MSA.Expression value;
            if (_value != null) {
                value = gen.CurrentScope.DefineHiddenVariable("#case-compare-value", typeof(object));
            } else {
                value = null;
            }

            if (_whenClauses != null) {
                for (int i = _whenClauses.Count - 1; i >= 0; i--) {
                    // emit: else (if (condition) body else result)
                    result = AstFactory.Condition(
                        TransformWhenCondition(gen, _whenClauses[i].Comparisons, _whenClauses[i].ComparisonArray, value),
                        gen.TransformStatementsToExpression(_whenClauses[i].Statements),
                        result
                    );
                }
            }

            if (_value != null) {
                result = Ast.Block(
                    Ast.Assign(value, Ast.Convert(_value.TransformRead(gen), typeof(object))),
                    result
                );
            }

            return result;
        }
    }
}
