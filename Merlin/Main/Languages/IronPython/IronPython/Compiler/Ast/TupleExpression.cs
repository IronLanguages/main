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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class TupleExpression : SequenceExpression {
        private bool _expandable;

        public TupleExpression(bool expandable, params Expression[] items)
            : base(items) {
            _expandable = expandable;
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (Items.Length == 0) {
                ag.AddError("can't assign to ()", Span);
                return null;
            }

            return base.TransformSet(ag, span, right, op);
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            if (_expandable) {
                return Ast.NewArrayInit(
                    typeof(object),
                    ag.TransformAndConvert(Items, typeof(object))
                );
            }

            return Ast.Call(
                AstGenerator.GetHelperMethod("MakeTuple"),
                Ast.NewArrayInit(
                    typeof(object),
                    ag.TransformAndConvert(Items, typeof(object))
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (Items != null) {
                    foreach (Expression e in Items) {
                        e.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        public bool IsExpandable {
            get {
                return _expandable;
            }
        }
    }
}
