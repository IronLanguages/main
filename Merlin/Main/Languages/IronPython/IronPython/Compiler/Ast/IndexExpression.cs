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
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class IndexExpression : Expression {
        private readonly Expression _target;
        private readonly Expression _index;

        public IndexExpression(Expression target, Expression index) {
            _target = target;
            _index = index;
        }

        public Expression Target {
            get { return _target; }
        }

        public Expression Index {
            get { return _index; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            return Binders.Operation(
                ag.BinderState,
                type,
                GetOperatorString,
                ArrayUtils.RemoveFirst(GetActionArgumentsForGetOrDelete(ag))
            );
        }

        private MSAst.Expression[] GetActionArgumentsForGetOrDelete(AstGenerator ag) {
            TupleExpression te = _index as TupleExpression;
            if (te != null && te.IsExpandable) {
                return ArrayUtils.Insert(AstUtils.CodeContext(), ag.Transform(_target), ag.Transform(te.Items));
            }

            SliceExpression se = _index as SliceExpression;
            if (se != null) {
                if (se.StepProvided) {
                    return new MSAst.Expression[] { 
                        AstUtils.CodeContext(),
                        ag.Transform(_target),
                        GetSliceValue(ag, se.SliceStart),
                        GetSliceValue(ag, se.SliceStop),
                        GetSliceValue(ag, se.SliceStep) 
                    };
                }

                return new MSAst.Expression[] { 
                    AstUtils.CodeContext(),
                    ag.Transform(_target),
                    GetSliceValue(ag, se.SliceStart),
                    GetSliceValue(ag, se.SliceStop)
                };
            }

            return new MSAst.Expression[] { AstUtils.CodeContext(), ag.Transform(_target), ag.Transform(_index) };
        }

        private static MSAst.Expression GetSliceValue(AstGenerator ag, Expression expr) {
            if (expr != null) {
                return ag.Transform(expr);
            }

            return Ast.Field(null, typeof(MissingParameter).GetField("Value"));
        }

        private MSAst.Expression[] GetActionArgumentsForSet(AstGenerator ag, MSAst.Expression right) {
            return ArrayUtils.Append(GetActionArgumentsForGetOrDelete(ag), right);
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            if (op != Operators.None) {
                right = Binders.Operation(
                    ag.BinderState,
                    typeof(object),
                    StandardOperators.FromOperator(op),
                    Binders.Operation(
                        ag.BinderState,
                        typeof(object),
                        GetOperatorString,
                        ArrayUtils.RemoveFirst(GetActionArgumentsForGetOrDelete(ag))
                    ),
                    right
                );                
            }
            
            return ag.AddDebugInfoAndVoid(
                Binders.Operation(
                    ag.BinderState,
                    typeof(object),
                    SetOperatorString,
                    ArrayUtils.RemoveFirst(GetActionArgumentsForSet(ag, right))
                ),
                Span
            );
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            return ag.AddDebugInfoAndVoid(
                Binders.Operation(
                    ag.BinderState,
                    typeof(object),
                    DeleteOperatorString,
                    ArrayUtils.RemoveFirst(GetActionArgumentsForGetOrDelete(ag))
                ),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_index != null) {
                    _index.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        private string GetOperatorString {
            get {
                if (_index is SliceExpression) {
                    return StandardOperators.GetSlice;
                }

                return StandardOperators.GetItem;
            }
        }

        private string SetOperatorString {
            get {
                if (_index is SliceExpression) {
                    return StandardOperators.SetSlice;
                }

                return StandardOperators.SetItem;
            }
        }

        private string DeleteOperatorString {
            get {
                if (_index is SliceExpression) {
                    return StandardOperators.DeleteSlice;
                }

                return StandardOperators.DeleteItem;
            }
        }
    }
}
