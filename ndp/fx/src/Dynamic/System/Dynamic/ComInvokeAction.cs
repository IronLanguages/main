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
    internal sealed class  ComInvokeAction : InvokeBinder {
        public override object CacheIdentity {
            get { return this; }
        }

        internal ComInvokeAction(params ArgumentInfo[] arguments)
            : base(arguments) {
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
        private SplatInvokeBinder() {}
        private static readonly SplatInvokeBinder _instance = new SplatInvokeBinder();

        internal static SplatInvokeBinder Instance{
            get {
                return _instance;
            }
        }

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

            DynamicMetaObject[] splattedArgs = new DynamicMetaObject[argLen];
            ArgumentInfo[] arginfos = new ArgumentInfo[argLen];

            for (int i = 0; i < argLen; i++) {
                Expression argExpr = Expression.ArrayAccess(
                    argAsArrayExpr,
                    Expression.Constant(i)
                );

                splattedArgs[i] = new DynamicMetaObject(
                    argExpr,
                    BindingRestrictions.Empty,
                    argValues[i]
                );
                arginfos[i] = Expression.ByRefPositionalArgument(i);
            }


            BindingRestrictions arrayLenRestriction = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(
                    Expression.ArrayLength(
                        argAsArrayExpr
                    ),
                    Expression.Constant(argLen)
                )
            );

            ComInvokeAction invokeBinder = new ComInvokeAction(arginfos);
            DynamicMetaObject innerAction = target.BindInvoke(invokeBinder, splattedArgs);

            BindingRestrictions restrictions =
                target.Restrictions.
                Merge(arg.Restrictions).
                Merge(arrayLenRestriction).
                Merge(innerAction.Restrictions);

            return new DynamicMetaObject(
                innerAction.Expression,
                restrictions
            );
        }

        public override object CacheIdentity {
            get { return this; }
        }
    }

}

#endif
