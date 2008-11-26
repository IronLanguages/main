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

using System.Diagnostics;
using System.Dynamic.Binders;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Dynamic.ComInterop {
    internal static class ComBinderHelpers {

        internal static bool PreferPut(Type type) {
            Debug.Assert(type != null);

            if (type.IsValueType || type.IsArray) return true;

            if (type == typeof(String) ||
                type == typeof(DBNull) ||
                type == typeof(System.Reflection.Missing) ||
                type == typeof(CurrencyWrapper)) {

                return true;
            } else {
                return false;
            }
        }

        internal static bool IsStrongBoxArg(MetaObject o) {
            if (o.IsByRef) return false;

            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        internal static MetaObject RewriteStrongBoxAsRef(CallSiteBinder action, MetaObject target, MetaObject[] args) {
            Debug.Assert(action != null && target != null && args != null);

            var restrictions = target.Restrictions.Merge(Restrictions.Combine(args));

            Expression[] argExpressions = new Expression[args.Length + 1];
            Type[] signatureTypes = new Type[args.Length + 3]; // args + CallSite, target, returnType

            signatureTypes[0] = typeof(CallSite);

            //TODO: we are not restricting on target type here, but in theory we could. 
            //It is a tradeoff between rule reuse and higher polymorphism of the site. 
            argExpressions[0] = target.Expression;
            signatureTypes[1] = target.Expression.Type;

            for (int i = 0; i < args.Length; i++) {
                MetaObject currArgument = args[i];
                if (IsStrongBoxArg(currArgument)) {
                    restrictions = restrictions.Merge(Restrictions.GetTypeRestriction(currArgument.Expression, currArgument.LimitType));

                    // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                    Expression boxedValueAccessor = Expression.Field(
                        Helpers.Convert(
                            currArgument.Expression,
                            currArgument.LimitType
                        ),
                        currArgument.LimitType.GetField("Value")
                    );

                    argExpressions[i + 1] = boxedValueAccessor;
                    signatureTypes[i + 2] = boxedValueAccessor.Type.MakeByRefType();
                } else {
                    argExpressions[i + 1] = currArgument.Expression;
                    signatureTypes[i + 2] = currArgument.Expression.Type;
                }
            }

            // Last signatureType is the return value
            signatureTypes[signatureTypes.Length - 1] = typeof(object);

            return new MetaObject(
                Expression.MakeDynamic(
                    Expression.GetDelegateType(signatureTypes),
                    action,
                    argExpressions
                ),
                restrictions
            );
        }
    }
}

#endif
