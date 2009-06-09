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
using IronPython.Runtime;
using MSAst = System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using IronPython.Runtime.Operations;

    public class DictionaryExpression : Expression {
        private readonly SliceExpression[] _items;

        public DictionaryExpression(params SliceExpression[] items) {
            _items = items;
        }

        public SliceExpression[] Items {
            get { return _items; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            // create keys & values into array and then call helper function
            // which creates the dictionary
            if (_items.Length != 0) {
                MSAst.Expression[] parts = new MSAst.Expression[_items.Length * 2];
                Type t = null;
                bool heterogeneous = false;
                for (int index = 0; index < _items.Length; index++) {
                    SliceExpression slice = _items[index];
                    // Eval order should be:
                    //   { 2 : 1, 4 : 3, 6 :5 }
                    // This is backwards from parameter list eval, so create temporaries to swap ordering.

                    
                    parts[index * 2] = ag.TransformOrConstantNull(slice.SliceStop, typeof(object));
                    MSAst.Expression key = parts[index * 2 + 1] = ag.TransformOrConstantNull(slice.SliceStart, typeof(object));

                    Type newType;
                    if (key.NodeType == System.Linq.Expressions.ExpressionType.Convert) {
                        newType = ((MSAst.UnaryExpression)key).Operand.Type;
                    } else {
                        newType = key.Type;
                    }

                    if (t == null) {
                        t = newType;
                    } else if (newType == typeof(object)) {
                        heterogeneous = true;
                    } else if (newType != t) {
                        heterogeneous = true;
                    }
                }

                return Ast.Call(
                    typeof(PythonOps).GetMethod(heterogeneous ? "MakeDictFromItems" : "MakeHomogeneousDictFromItems"),
                    Ast.NewArrayInit(
                        typeof(object),
                        parts
                    )
                );
            }

            // empty dictionary
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeDict"),
                AstUtils.Constant(0)
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_items != null) {
                    foreach (SliceExpression s in _items) {
                        s.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
