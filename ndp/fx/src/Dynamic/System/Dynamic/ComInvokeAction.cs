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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Dynamic {
    internal sealed class ComInvokeAction : InvokeBinder {
        internal ComInvokeAction(CallInfo callInfo)
            : base(callInfo) {
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj as ComInvokeAction);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? DynamicMetaObject.CreateThrow(target, args, typeof(NotSupportedException), Strings.CannotCall);
        }
    }


    internal sealed class SplatInvokeBinder : DynamicMetaObjectBinder {

        // SplatInvokeBinder is a singleton
        private SplatInvokeBinder() { }
        private static readonly SplatInvokeBinder _instance = new SplatInvokeBinder();

        internal static SplatInvokeBinder Instance {
            get {
                return _instance;
            }
        }

        private static Type ByRefObjectType = typeof(object).MakeByRefType();

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(target, "args");

            // there must be only one argument and it is always object[].
            // see SplatCallSite.Invoke
            Debug.Assert(args.Length == 1);
            Debug.Assert(args[0] != null);
            Debug.Assert(args[0].Expression.Type == typeof(object[]));


            // We know it is an array.
            DynamicMetaObject arg = args[0];

            Expression argAsArrayExpr = arg.Expression;

            object[] argValues = (object[])arg.Value;
            int argLen = argValues.Length;

            IndexExpression[] argElements = new IndexExpression[argLen];
            ParameterExpression[] temps = new ParameterExpression[argLen];

            DynamicMetaObject[] splattedArgs = new DynamicMetaObject[argLen];

            for (int i = 0; i < argLen; i++) {

                // temps have Object& type here
                // it is ok as they will be stored on a nested lambda scope which allows ref types
                ParameterExpression temp = Expression.Parameter(ByRefObjectType, "arg" + i);
                temps[i] = temp;

                // values that we will assign to temps via Invoke.
                argElements[i] = Expression.ArrayAccess(
                    argAsArrayExpr,
                    Expression.Constant(i)
                );

                // metaobjects for the inner bind.
                splattedArgs[i] = new DynamicMetaObject(
                    temp,
                    BindingRestrictions.Empty,
                    argValues[i]
                );
            }


            BindingRestrictions arrayLenRestriction = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(
                    Expression.ArrayLength(
                        argAsArrayExpr
                    ),
                    Expression.Constant(argLen)
                )
            );

            ComInvokeAction invokeBinder = new ComInvokeAction(Expression.CallInfo(argLen));
            DynamicMetaObject innerAction = target.BindInvoke(invokeBinder, splattedArgs);

            Expression exprRestrictions = Expression.Lambda(innerAction.Restrictions.ToExpression(), temps);

            // note that inner restrictions need to be added through Invoke to have correct scoping.
            BindingRestrictions restrictions =
                target.Restrictions.
                Merge(arg.Restrictions).
                Merge(arrayLenRestriction).
                Merge(BindingRestrictions.GetExpressionRestriction(Expression.Invoke(exprRestrictions, argElements)));

            // invoke inner expression in its own scope. 
            Expression innerExpression = Expression.Invoke(
                Expression.Lambda(innerAction.Expression, temps),
                argElements
            );

            return new DynamicMetaObject(
                innerExpression,
                restrictions
            );
        }
    }
}

#endif
