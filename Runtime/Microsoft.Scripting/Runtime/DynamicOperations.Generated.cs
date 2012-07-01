/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

//
// To regenerate code in this file run:
// ir Languages/Ruby/Scripts/CodeGenerator.rb DynamicOperations.Generated.cs
//

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    public sealed partial class DynamicOperations {
        private const int /*$$*/PregeneratedInvokerCount = 14;

        private Func<DynamicOperations, CallSiteBinder, object, object[], object> GetInvoker(int paramCount) {
            Func<DynamicOperations, CallSiteBinder, object, object[], object> invoker;
            lock (_invokers) {
                if (!_invokers.TryGetValue(paramCount, out invoker)) {
                    _invokers[paramCount] = invoker = GetPregeneratedInvoker(paramCount) ?? EmitInvoker(paramCount);
                }
            }
            return invoker;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Func<DynamicOperations, CallSiteBinder, object, object[], object> EmitInvoker(int paramCount) {
#if !FEATURE_REFEMIT
            throw new NotSupportedException();
#else
            ParameterExpression dynOps = Expression.Parameter(typeof(DynamicOperations));
            ParameterExpression callInfo = Expression.Parameter(typeof(CallSiteBinder));
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression args = Expression.Parameter(typeof(object[]));
            Type funcType = DelegateUtils.EmitCallSiteDelegateType(paramCount);
            ParameterExpression site = Expression.Parameter(typeof(CallSite<>).MakeGenericType(funcType));
            Expression[] siteArgs = new Expression[paramCount + 2];
            siteArgs[0] = site;
            siteArgs[1] = target;
            for (int i = 0; i < paramCount; i++) {
                siteArgs[i + 2] = Expression.ArrayIndex(args, Expression.Constant(i));
            }

            var getOrCreateSiteFunc = new Func<CallSiteBinder, CallSite<Func<object>>>(GetOrCreateSite<Func<object>>).GetMethodInfo().GetGenericMethodDefinition();
            return Expression.Lambda<Func<DynamicOperations, CallSiteBinder, object, object[], object>>(
                Expression.Block(
                    new[] { site },
                    Expression.Assign(
                        site,
                        Expression.Call(dynOps, getOrCreateSiteFunc.MakeGenericMethod(funcType), callInfo)
                    ),
                    Expression.Invoke(
                        Expression.Field(
                            site,
                            site.Type.GetField("Target")
                        ),
                        siteArgs
                    )
                ),
                new[] { dynOps, callInfo, target, args }
            ).Compile();
#endif
        }

        private static Func<DynamicOperations, CallSiteBinder, object, object[], object> GetPregeneratedInvoker(int paramCount) {
            switch (paramCount) {
#if GENERATOR 
                def generate; $PregeneratedInvokerCount.times { |n| @n = n + 1; super }; end
                def n; @n; end
                def objects; "object, " * @n; end
                def args; (0..@n-1).map { |i| ", args[#{i}]" }.join; end
#else
                case /*$n{*/0/*}*/:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, /*$objects*/object>>(binder);
                        return site.Target(site, target/*$args*/);
                    };
#endif
                #region Generated
                case 1:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object>>(binder);
                        return site.Target(site, target, args[0]);
                    };
                case 2:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1]);
                    };
                case 3:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2]);
                    };
                case 4:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3]);
                    };
                case 5:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4]);
                    };
                case 6:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5]);
                    };
                case 7:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    };
                case 8:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    };
                case 9:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                    };
                case 10:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                    };
                case 11:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                    };
                case 12:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
                    };
                case 13:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
                    };
                case 14:
                    return (ops, binder, target, args) => {
                        var site = ops.GetOrCreateSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binder);
                        return site.Target(site, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
                    };
                #endregion
            }
            return null;
        }
    }
}
