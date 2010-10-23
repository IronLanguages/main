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
        //		when args; statements
        //      ...
        //      when args; statements
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
        //     when arg0, ..., *argn; statements
        //     ...
        // end

        private readonly Expression _value;
        private readonly WhenClause/*!*/[]/*!*/ _whenClauses;
        private readonly Statements _elseStatements;

        public Expression Value {
            get { return _value; } 
        }

        public WhenClause/*!*/[]/*!*/ WhenClauses {
            get { return _whenClauses; }
        }

        public Statements ElseStatements {
            get { return _elseStatements; }
        }

        internal CaseExpression(Expression value, WhenClause/*!*/[] whenClauses, ElseIfClause elseClause, SourceSpan location)
            : this(value, whenClauses, (elseClause != null) ? elseClause.Statements : null, location) {
        }

        public CaseExpression(Expression value, WhenClause/*!*/[] whenClauses, Statements elseStatements, SourceSpan location)
            : base(location) {
            _value = value;
            _whenClauses = whenClauses ?? WhenClause.EmptyArray;
            _elseStatements = elseStatements;
        }

        // when <expr>
        //   generates into:
        //   RubyOps.IsTrue(<expr>) if the case has no value, otherise:
        //   RubyOps.IsTrue(Call("===", <expr>, <value>))
        private static MSA.Expression/*!*/ MakeTest(AstGenerator/*!*/ gen, Expression/*!*/ expr, MSA.Expression value) {
            MSA.Expression transformedExpr = expr.TransformRead(gen);
            if (expr is SplattedArgument) {
                if (value != null) {
                    return Methods.ExistsUnsplatCompare.OpCall(
                        Ast.Constant(CallSite<Func<CallSite, object, object, object>>.Create(
                            RubyCallAction.Make(gen.Context, "===", RubyCallSignature.WithImplicitSelf(2))
                        )),
                        AstUtils.LightDynamic(ExplicitTrySplatAction.Make(gen.Context), transformedExpr),
                        AstUtils.Box(value)
                    );
                } else {
                    return Methods.ExistsUnsplat.OpCall(
                        AstUtils.LightDynamic(ExplicitTrySplatAction.Make(gen.Context), transformedExpr)
                    );
                }
            } else {
                if (value != null) {
                    return AstFactory.IsTrue(
                        CallSiteBuilder.InvokeMethod(gen.Context, "===", RubyCallSignature.WithScope(1),
                            gen.CurrentScopeVariable,
                            transformedExpr,
                            value
                        )
                    );
                } else {
                    return AstFactory.IsTrue(transformedExpr);
                }
            }
        }

        // when <expr0>, *<expr1>, ..., <exprN>
        // generates:
        // <MakeTest>(<expr0>) || <MakeArrayTest>(<expr1>) || ... [ || <MakeTest>(<exprN>) ]
        internal static MSA.Expression/*!*/ TransformWhenCondition(AstGenerator/*!*/ gen, Expression/*!*/[]/*!*/ comparisons, MSA.Expression value) {
            MSA.Expression result;
            if (comparisons.Length > 0) {
                result = MakeTest(gen, comparisons[comparisons.Length - 1], value);
                for (int i = comparisons.Length - 2; i >= 0; i--) {
                    result = Ast.OrElse(MakeTest(gen, comparisons[i], value), result);
                }
            } else {
                result = Ast.Constant(false);
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

            for (int i = _whenClauses.Length - 1; i >= 0; i--) {
                // emit: else (if (condition) body else result)
                result = AstFactory.Condition(
                    TransformWhenCondition(gen, _whenClauses[i].Comparisons, value),
                    gen.TransformStatementsToExpression(_whenClauses[i].Statements),
                    result
                );
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
