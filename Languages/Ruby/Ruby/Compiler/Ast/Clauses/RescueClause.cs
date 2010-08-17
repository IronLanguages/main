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

using System.Diagnostics;
using System.Collections;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using AstParameters = ReadOnlyCollectionBuilder<MSA.ParameterExpression>;
    using System;

    // rescue types, *type-array, types, *type-array, ... 
    //   statements
    // rescue types, *type-array, types, *type-array, ... => target
    //   statements
    public partial class RescueClause : Node {
        private readonly Expression/*!*/[]/*!*/ _types;  // might be empty, might include SplattedArgument expressions
        private readonly LeftValue _target;		         // optional
        private readonly Statements _statements;         // optional

        public Expression/*!*/[]/*!*/ Types {
            get { return _types; }
        }

        public LeftValue Target {
            get { return _target; }
        }

        public Statements Statements {
            get { return _statements; }
        }
        
        public RescueClause(Expression/*!*/[]/*!*/ types, LeftValue target, Statements statements, SourceSpan location)
            : base(location) {
            Assert.NotNullItems(types);
            _types = types;
            _target = target;
            _statements = statements;
        }

        //
        // rescue stmts                     ... if (StandardError === $!) { stmts; } 
        // rescue <types> stmts             ... temp1 = type1; ...; if (<temp1> === $! || ...) { stmts; }
        // rescue <types> => <lvalue> stmts ... temp1 = type1; ...; if (<temp1> === $! || ...) { <lvalue> = $!; stmts; }
        // 
        internal IfStatementTest/*!*/ Transform(AstGenerator/*!*/ gen, ResultOperation resultOperation) {
            Assert.NotNull(gen);
            
            MSA.Expression condition;
            if (_types.Length != 0) {
                var comparisonSiteStorage = Ast.Constant(new BinaryOpStorage(gen.Context));

                if (_types.Length == 1) {
                    condition = MakeCompareException(gen, comparisonSiteStorage, _types[0].TransformRead(gen), _types[0] is SplattedArgument);
                } else {
                    // forall{i}: <temps[i]> = evaluate type[i]
                    var temps = new MSA.Expression[_types.Length];
                    var exprs = new BlockBuilder();

                    for (int i = 0; i < _types.Length; i++) {
                        var t = _types[i].TransformRead(gen);
                        var tmp = gen.CurrentScope.DefineHiddenVariable("#type_" + i, t.Type);
                        temps[i] = tmp;
                        exprs.Add(Ast.Assign(tmp, t));
                    }

                    // CompareException(<temps[0]>) || ... CompareException(<temps[n]>) || CompareSplattedExceptions(<splatTypes>)
                    condition = MakeCompareException(gen, comparisonSiteStorage, temps[0], _types[0] is SplattedArgument);
                    for (int i = 1; i < _types.Length; i++) {
                        condition = Ast.OrElse(condition, MakeCompareException(gen, comparisonSiteStorage, temps[i], _types[i] is SplattedArgument));
                    }

                    // (temps[0] = type[0], ..., temps[n] == type[n], condition)
                    exprs.Add(condition);
                    condition = exprs;
                }
            } else {
                condition = Methods.CompareDefaultException.OpCall(gen.CurrentScopeVariable);
            }

            return AstUtils.IfCondition(condition,
                gen.TransformStatements(
                    // <lvalue> = e;
                    (_target != null) ? _target.TransformWrite(gen, Methods.GetCurrentException.OpCall(gen.CurrentScopeVariable)) : null,

                    // body:
                    _statements,

                    resultOperation
                )
            );
        }

        private MSA.Expression/*!*/ MakeCompareException(AstGenerator/*!*/ gen, MSA.Expression/*!*/ comparisonSiteStorage, MSA.Expression/*!*/ expression, bool isSplatted) {
            if (isSplatted) {
                return Methods.CompareSplattedExceptions.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, expression);
            } else {
                return Methods.CompareException.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, AstUtils.Box(expression));
            }
        }
    }
}

