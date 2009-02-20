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
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;

namespace System.Dynamic {
    /// <summary>
    /// The dynamic call site binder that participates in the <see cref="DynamicMetaObject"/> binding protocol.
    /// </summary>
    /// <remarks>
    /// The <see cref="CallSiteBinder"/> performs the binding of the dynamic operation using the runtime values
    /// as input. On the other hand, the <see cref="DynamicMetaObjectBinder"/> participates in the <see cref="DynamicMetaObject"/>
    /// binding protocol.
    /// </remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMetaObjectBinder"/> class.
        /// </summary>
        protected DynamicMetaObjectBinder() {
        }

        /// <summary>
        /// Performs the runtime binding of the dynamic operation on a set of arguments.
        /// </summary>
        /// <param name="args">An array of arguments to the dynamic operation.</param>
        /// <param name="parameters">The array of <see cref="ParameterExpression"/> instances that represent the parameters of the call site in the binding process.</param>
        /// <param name="returnLabel">A LabelTarget used to return the result of the dynamic binding.</param>
        /// <returns>
        /// An Expression that performs tests on the dynamic operation arguments, and
        /// performs the dynamic operation if hte tests are valid. If the tests fail on
        /// subsequent occurrences of the dynamic operation, Bind will be called again
        /// to produce a new <see cref="Expression"/> for the new argument types.
        /// </returns>
        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            if (args.Length == 0) {
                throw new InvalidOperationException();
            }

            DynamicMetaObject target = ObjectToMetaObject(args[0], parameters[0]);
            DynamicMetaObject[] metaArgs = CreateArgumentMetaObjects(args, parameters);

            DynamicMetaObject binding = Bind(target, metaArgs);

            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            Expression body = binding.Expression;
            BindingRestrictions restrictions = binding.Restrictions;

            // if target is an IDO we may have a target-specific binding. 
            // so it makes sense to restrict on the target's type.
            // ideally IDO's should do this, but they often miss this.
            if (args[0] as IDynamicMetaObjectProvider != null) {
                restrictions = BindingRestrictions.GetTypeRestriction(target).Merge(restrictions);
            }

            restrictions = AddRemoteObjectRestrictions(restrictions, args, parameters);

            // Add the return label
            body = AddReturn(body, returnLabel, body.Type);

            // Finally, add restrictions
            if (restrictions != BindingRestrictions.Empty) {
                body = Expression.IfThen(restrictions.ToExpression(), body);
            }

            return body;
        }

        private static DynamicMetaObject[] CreateArgumentMetaObjects(object[] args, ReadOnlyCollection<ParameterExpression> parameters) {
            DynamicMetaObject[] mos;
            if (args.Length != 1) {
                mos = new DynamicMetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    mos[i - 1] = ObjectToMetaObject(args[i], parameters[i]);
                }
            } else {
                mos = DynamicMetaObject.EmptyMetaObjects;
            }
            return mos;
        }

        private static BindingRestrictions AddRemoteObjectRestrictions(BindingRestrictions restrictions, object[] args, ReadOnlyCollection<ParameterExpression> parameters) {
#if !SILVERLIGHT

            for (int i = 0; i < parameters.Count; i++) {
                var expr = parameters[i];
                var value = args[i] as MarshalByRefObject;

                // special case for MBR objects.
                // when MBR objects are remoted they can have different conversion behavior
                // so bindings created for local and remote objects should not be mixed.
                if (value != null && !IsComObject(value)) {
                    BindingRestrictions remotedRestriction;
                    if (RemotingServices.IsObjectOutOfAppDomain(value)) {
                        remotedRestriction = BindingRestrictions.GetExpressionRestriction(
                            Expression.AndAlso(
                                Expression.NotEqual(expr, Expression.Constant(null)),
                                Expression.Call(
                                    typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"),
                                    expr
                                )
                            )
                        );
                    } else {
                        remotedRestriction = BindingRestrictions.GetExpressionRestriction(
                            Expression.AndAlso(
                                Expression.NotEqual(expr, Expression.Constant(null)),
                                Expression.Not(
                                    Expression.Call(
                                        typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"),
                                        expr
                                    )
                                )
                            )
                        );
                    }
                    restrictions = restrictions.Merge(remotedRestriction);
                }
            }

#endif
            return restrictions;
        }

        private static DynamicMetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicMetaObjectProvider ido = argValue as IDynamicMetaObjectProvider;
#if !SILVERLIGHT
            if (ido != null && !RemotingServices.IsObjectOutOfAppDomain(argValue)) {
#else
            if (ido != null) {
#endif
                return ido.GetMetaObject(parameterExpression);
            } else {
                return new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty, argValue);
            }
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic operation.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);

        /// <summary>
        /// Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
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

        /// <summary>
        /// Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.
        /// </summary>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
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
                    new TrueReadOnlyCollection<Expression>(exprs)
                ),
                rs
            );
        }

        #endregion

        private Expression AddReturn(Expression body, LabelTarget @return, Type toType) {
            switch (body.NodeType) {
                case ExpressionType.Conditional:
                    var conditional = (ConditionalExpression)body;

                    // We can only look for defer expression as one of the
                    // branches of a conditional. Otherwise we confuse actual
                    // "defers" with "my dynamic test failed".
                    if (IsDeferExpression(conditional.IfTrue)) {
                        return Expression.Condition(
                            Expression.Not(conditional.Test),
                            ConvertAndReturn(conditional.IfFalse, @return, toType),
                            Expression.Default(conditional.Type),
                            conditional.Type
                        );
                    }
                    if (IsDeferExpression(conditional.IfFalse)) {
                        return Expression.Condition(
                            conditional.Test,
                            ConvertAndReturn(conditional.IfTrue, @return, toType),
                            Expression.Default(conditional.Type),
                            conditional.Type
                        );
                    }
                    return Expression.Condition(
                        conditional.Test,
                        AddReturn(conditional.IfTrue, @return, toType),
                        AddReturn(conditional.IfFalse, @return, toType),
                        conditional.Type
                    );
                case ExpressionType.Block:
                    var block = (BlockExpression)body;

                    int count = block.ExpressionCount;
                    Expression[] nodes = new Expression[count];

                    for (int i = 0; i < nodes.Length - 1; i++) {
                        nodes[i] = block.GetExpression(i);
                    }
                    nodes[nodes.Length - 1] = AddReturn(block.GetExpression(count - 1), @return, toType);

                    return block.Rewrite(block.Variables, nodes);
                default:
                    return ConvertAndReturn(body, @return, toType);
            }
        }

        // We need this instead of folding it into AddReturn because Python
        // generates trees that won't work unless we stop at the first "defer"
        // that we see.
        private static Expression ConvertAndReturn(Expression body, LabelTarget @return, Type toType) {
            // Coerce the body to the rule's result type.
            Expression result = body;
            if (toType != result.Type) {
                result = Expression.Block(toType, result);
            }

            // Then add a convert to the lambda return type.
            if (@return.Type != typeof(void) && result.Type != @return.Type) {
                result = Helpers.Convert(result, @return.Type);
            }

            return Expression.Return(@return, result, body.Type);
        }

        // The presence of a call to our own binder actually means:
        // "this rule failed, please try to bind again". To make it work
        // we need to remove the body so we'll fall through and report that
        // it failed to match, otherwise we stack overflow.
        private bool IsDeferExpression(Expression node) {
            var dynamic = node as DynamicExpression;
            if (dynamic != null) {
                if (dynamic.Binder != this) {
                    return false;
                }

                // We should be making sure that all of the arguments match,
                // but we can't yet, because Python sometimes drops one of the
                // arguments but they don't really mean to.
                return true;
            }

            if (node.NodeType == ExpressionType.Convert) {
                return IsDeferExpression(((UnaryExpression)node).Operand);
            }

            // Simple reference conversions or void conversion might look like
            // a block with one expression.
            var block = node as BlockExpression;
            if (block != null && block.ExpressionCount == 1) {
                return IsDeferExpression(block.GetExpression(0));
            }

            return false;
        }

#if !SILVERLIGHT
        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");
        private static bool IsComObject(object obj) {
            // we can't use System.Runtime.InteropServices.Marshal.IsComObject(obj) since it doesn't work in partial trust
            return obj != null && ComObjectType.IsAssignableFrom(obj.GetType());
        }
#endif

    }
}
