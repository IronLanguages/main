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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

namespace System.Dynamic {
    public abstract class DynamicMetaObjectBinder : CallSiteBinder {

        #region Standard Binder Kinds

        internal const int OperationBinderHash = 0x4000000;
        internal const int UnaryOperationBinderHash = 0x8000000;
        internal const int BinaryOperationBinderHash = 0xc000000;
        internal const int GetMemberBinderHash = 0x10000000;
        internal const int SetMemberBinderHash = 0x14000000;
        internal const int DeleteMemberBinderHash = 0x18000000;
        internal const int GetIndexBinderHash = 0x1c000000;
        internal const int SetIndexBinderHash = 0x20000000;
        internal const int DeleteIndexBinderHash = 0x24000000;
        internal const int InvokeMemberBinderHash = 0x28000000;
        internal const int ConvertBinderHash = 0x2c000000;
        internal const int CreateInstanceBinderHash = 0x30000000;
        internal const int InvokeBinderHash = 0x34000000;
        internal const int BinaryOperationOnMemberBinderHash = 0x38000000;
        internal const int BinaryOperationOnIndexBinderHash = 0x3c000000;
        internal const int UnaryOperationOnMemberBinderHash = 0x40000000;
        internal const int UnaryOperationOnIndexBinderHash = 0x44000000;

        #endregion

        #region Public APIs

        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            if (args.Length == 0) {
                throw new InvalidOperationException();
            }

            DynamicMetaObject[] mos;
            if (args.Length != 1) {
                mos = new DynamicMetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    mos[i - 1] = DynamicMetaObject.ObjectToMetaObject(args[i], parameters[i]);
                }
            } else {
                mos = DynamicMetaObject.EmptyMetaObjects;
            }

            DynamicMetaObject binding = Bind(
                DynamicMetaObject.ObjectToMetaObject(args[0], parameters[0]),
                mos
            );

            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            return GetMetaObjectRule(binding, returnLabel);
        }

        public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);

        public DynamicMetaObject Defer(DynamicMetaObject target, params DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");

            if (args == null) {
                return MakeDeferred(
                        target.Restrictions,
                        target
                );
            } else {
                return MakeDeferred(
                        target.Restrictions.Merge(BindingRestrictions.Combine(args)),
                        args.AddFirst(target)
                );
            }
        }

        public DynamicMetaObject Defer(params DynamicMetaObject[] args) {
            return MakeDeferred(
                BindingRestrictions.Combine(args),
                args
            );
        }

        private DynamicMetaObject MakeDeferred(BindingRestrictions rs, params DynamicMetaObject[] args) {
            var exprs = DynamicMetaObject.GetExpressions(args);

            Type delegateType = DelegateHelpers.MakeDeferredSiteDelegate(args, typeof(object));

            // Because we know the arguments match the delegate type (we just created the argument types)
            // we go directly to DynamicExpression.Make to avoid a bunch of unnecessary argument validation
            return new DynamicMetaObject(
                DynamicExpression.Make(
                    typeof(object),
                    delegateType,
                    this,
                    new ReadOnlyCollection<Expression>(exprs)
                ),
                rs
            );
        }

        #endregion

        private Expression AddReturn(Expression body, LabelTarget @return) {
            switch (body.NodeType) {
                case ExpressionType.Conditional:
                    ConditionalExpression conditional = (ConditionalExpression)body;
                    if (IsDeferExpression(conditional.IfTrue)) {
                        return Expression.Condition(
                            Expression.Not(conditional.Test),
                            Expression.Return(@return, Helpers.Convert(conditional.IfFalse, @return.Type)),
                            Expression.Empty()
                        );
                    } else if (IsDeferExpression(conditional.IfFalse)) {
                        return Expression.Condition(
                            conditional.Test,
                            Expression.Return(@return, Helpers.Convert(conditional.IfTrue, @return.Type)),
                            Expression.Empty()
                        );
                    }
                    return Expression.Condition(
                        conditional.Test,
                        AddReturn(conditional.IfTrue, @return),
                        AddReturn(conditional.IfFalse, @return)
                    );
                case ExpressionType.Throw:
                    return body;
                case ExpressionType.Block:
                    // block could have a throw which we need to run through to avoid 
                    // trying to convert it
                    BlockExpression block = (BlockExpression)body;

                    int count = block.ExpressionCount;
                    Expression[] nodes = new Expression[count];

                    for (int i = 0; i < nodes.Length - 1; i++) {
                        nodes[i] = block.GetExpression(i);
                    }
                    nodes[nodes.Length - 1] = AddReturn(block.GetExpression(count - 1), @return);

                    return Expression.Block(block.Variables, nodes);
                default:
                    return Expression.Return(@return, Helpers.Convert(body, @return.Type));
            }
        }

        private bool IsDeferExpression(Expression e) {
            if (e.NodeType == ExpressionType.Dynamic) {
                return ((DynamicExpression)e).Binder == this;
            }

            if (e.NodeType == ExpressionType.Convert) {
                return IsDeferExpression(((UnaryExpression)e).Operand);
            }

            return false;
        }

        private Expression GetMetaObjectRule(DynamicMetaObject binding, LabelTarget @return) {
            Debug.Assert(binding != null);

            Expression body = AddReturn(binding.Expression, @return);

            if (binding.Restrictions != BindingRestrictions.Empty) {
                // add the test only if we have one
                body = Expression.Condition(
                    binding.Restrictions.ToExpression(),
                    body,
                    Expression.Empty()
                );
            }

            return body;
        }
    }
}
