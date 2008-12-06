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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;    

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Rewrites dynamic sites so that the lambda can be emitted as a non-dynamic method.
    /// </summary>
    public abstract class StaticLambdaRewriter : ExpressionVisitor {
        protected override Expression VisitDynamic(DynamicExpression node) {
            Type siteType = typeof(CallSite<>).MakeGenericType(node.DelegateType);

            // Rewite call site as constant
            var siteExpr = VisitConstant(Expression.Constant(DynamicSiteHelpers.MakeSite(node.Binder, siteType)));

            // Rewrite all of the arguments
            var args = Visit(node.Arguments);

            var siteVar = Expression.Variable(siteExpr.Type, "$site");

            // ($site = siteExpr).Target.Invoke($site, *args)
            return Expression.Block(
                new [] { siteVar },
                Expression.Call(
                    Expression.Field(
                        Expression.Assign(siteVar, siteExpr),
                        siteType.GetField("Target")
                    ),
                    node.DelegateType.GetMethod("Invoke"),
                    ArrayUtils.Insert(siteVar, args)
                )
            );
        }
    }
}
