/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;    
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class ReturnStatement : Statement {
        private readonly Expression _expression;

        public ReturnStatement(Expression expression) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            if (ag.IsGenerator) {
                if (_expression != null) {
                    // Statements can't return null, so return a rethrow. 
                    // Callers should detecet the ag.AddError and avoid trying to execute the tree, 
                    // but if they accidentally do, use Throw instead of empty so that
                    // we'll get an exception.
                    return Ast.Throw(
                        Ast.New(
                            typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)
                        )
                    );
                }

                return ag.AddDebugInfo(AstUtils.YieldBreak(ag.GeneratorLabel), Span);
            }

            return ag.AddDebugInfo(
                Ast.Return(
                    ag.ReturnLabel,
                    ag.TransformOrConstantNull(_expression, typeof(object))
                ),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        internal override bool CanThrow {
            get {
                if (_expression == null) {
                    return false;
                }

                return _expression.CanThrow;
            }
        }
    }
}
