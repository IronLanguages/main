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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class DictionaryExpression : Expression {
        private readonly SliceExpression[] _items;

        public DictionaryExpression(params SliceExpression[] items) {
            _items = items;
        }

        public IList<SliceExpression> Items {
            get { return _items; }
        }

        public override MSAst.Expression Reduce() {
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


                    parts[index * 2] = TransformOrConstantNull(slice.SliceStop, typeof(object));
                    MSAst.Expression key = parts[index * 2 + 1] = TransformOrConstantNull(slice.SliceStart, typeof(object));

                    Type newType;
                    if (key.NodeType == MSAst.ExpressionType.Convert) {
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
                    heterogeneous ? AstMethods.MakeDictFromItems : AstMethods.MakeHomogeneousDictFromItems,
                    Ast.NewArrayInit(
                        typeof(object),
                        parts
                    )
                );
            }

            // empty dictionary
            return Ast.Call(
                AstMethods.MakeDict,
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
