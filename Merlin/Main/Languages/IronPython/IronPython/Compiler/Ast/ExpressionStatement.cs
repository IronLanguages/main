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

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class ExpressionStatement : Statement {
        private readonly Expression _expression;

        public ExpressionStatement(Expression expression) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression expression = ag.Transform(_expression);

            if (ag.PrintExpressions) {
                expression = Ast.Call(
                    AstGenerator.GetHelperMethod("PrintExpressionValue"),
                    ag.LocalContext,
                    AstGenerator.ConvertIfNeeded(expression, typeof(object))
                );
            }

            return ag.AddDebugInfoAndVoid(expression, _expression.Span);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        public override string Documentation {
            get {
                ConstantExpression ce = _expression as ConstantExpression;
                if (ce != null) {
                    return ce.Value as string;
                }
                return null;
            }
        }

        internal override bool CanThrow {
            get {
                return _expression.CanThrow;
            }
        }
    }
}
