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

    public class ListExpression : SequenceExpression {
        

        public ListExpression(params Expression[] items)
            : base(items) {
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            if (Items.Count == 0) {
                return Ast.Call(
                    AstGenerator.GetHelperMethod("MakeEmptyListFromCode"),
                    AstGenerator.EmptyExpression
                );            
            }

            return Ast.Call(
                AstGenerator.GetHelperMethod("MakeListNoCopy", new Type[] { typeof(object[]) }),  // method
                Ast.NewArrayInit(                                                               // parameters
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
    }
}
