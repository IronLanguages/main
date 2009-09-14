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

using System.Diagnostics;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    // rescue type
    //   statements
    // rescue type => target
    //   statements
    // rescue types,*type-array
    //   statements
    // rescue types,*type-array => target
    //   statements
    public partial class RescueClause : Node {
        private readonly Expression[]/*!*/ _types;   // might be empty
        private readonly Expression _splatType;          // optional
        private readonly LeftValue _target;		         // optional
        private readonly Statements _statements;   // optional

        public Expression[] Types {
            get { return _types; }
        }

        public LeftValue Target {
            get { return _target; }
        }

        public Statements Statements {
            get { return _statements; }
        }

        public RescueClause(LeftValue target, Statements statements, SourceSpan location)
            : base(location) {
            _target = target;
            _types = Expression.EmptyArray;
            _statements = statements;
        }
        
        public RescueClause(CompoundRightValue type, LeftValue target, Statements statements, SourceSpan location)
            : base(location) {
            _types = type.RightValues;
            _splatType = type.SplattedValue;
            _target = target;
            _statements = statements;
        }

        public RescueClause(Expression/*!*/ type, LeftValue target, Statements statements, SourceSpan location)
            : base(location) {
            Assert.NotNull(type);
            _types = new Expression[] { type };
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
            if (_types.Length != 0 || _splatType != null) {
                var comparisonSiteStorage = Ast.Constant(new BinaryOpStorage(gen.Context));

                if (_types.Length == 0) {
                    // splat only:
                    condition = MakeCompareSplattedExceptions(gen, comparisonSiteStorage, TransformSplatType(gen));
                } else if (_types.Length == 1 && _splatType == null) {
                    condition = MakeCompareException(gen, comparisonSiteStorage, _types[0].TransformRead(gen));
                } else {

                    // forall{i}: <temps[i]> = evaluate type[i]
                    var temps = new MSA.Expression[_types.Length + (_splatType != null ? 1 : 0)];
                    var exprs = new MSA.Expression[temps.Length  + 1];
                    
                    int i = 0;
                    while (i < _types.Length) {
                        var tmp = gen.CurrentScope.DefineHiddenVariable("#type_" + i, typeof(object));
                        temps[i] = tmp;
                        exprs[i] = Ast.Assign(tmp, _types[i].TransformRead(gen));
                        i++;
                    }

                    if (_splatType != null) {
                        var tmp = gen.CurrentScope.DefineHiddenVariable("#type_" + i, typeof(object));
                        temps[i] = tmp;
                        exprs[i] = Ast.Assign(tmp, TransformSplatType(gen));

                        i++;
                    }

                    Debug.Assert(i == temps.Length);

                    // CompareException(<temps[0]>) || ... CompareException(<temps[n]>) || CompareSplattedExceptions(<splatTypes>)
                    i = 0;
                    condition = MakeCompareException(gen, comparisonSiteStorage, temps[i++]);
                    while (i < _types.Length) {
                        condition = Ast.OrElse(condition, MakeCompareException(gen, comparisonSiteStorage, temps[i++]));
                    }

                    if (_splatType != null) {
                        condition = Ast.OrElse(condition, MakeCompareSplattedExceptions(gen, comparisonSiteStorage, temps[i++]));
                    }

                    Debug.Assert(i == temps.Length);

                    // (temps[0] = type[0], ..., temps[n] == type[n], condition)
                    exprs[exprs.Length - 1] = condition;
                    condition = AstFactory.Block(exprs);
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

        private MSA.Expression/*!*/ TransformSplatType(AstGenerator/*!*/ gen) {
            return Ast.Dynamic(ConvertToArraySplatAction.Make(gen.Context), typeof(object), _splatType.TransformRead(gen));
        }

        private MSA.Expression/*!*/ MakeCompareException(AstGenerator/*!*/ gen, MSA.Expression/*!*/ comparisonSiteStorage, MSA.Expression/*!*/ expression) {
            return Methods.CompareException.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, AstFactory.Box(expression));
        }

        private MSA.Expression/*!*/ MakeCompareSplattedExceptions(AstGenerator/*!*/ gen, MSA.Expression/*!*/ comparisonSiteStorage, MSA.Expression/*!*/ expression) {
            return Methods.CompareSplattedExceptions.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, expression);
        }
    }
}
