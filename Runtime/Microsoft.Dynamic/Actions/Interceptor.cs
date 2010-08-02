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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Interceptor prototype. The interceptor is a call site binder that wraps
    /// a real call site binder and can perform arbitrary operations on the expression
    /// trees that the wrapped binder produces:
    ///   * Dumping the trees
    ///   * Additional rewriting
    ///   * Static compilation
    ///   * ...
    /// </summary>
    public static class Interceptor {
        public static Expression Intercept(Expression expression) {
            InterceptorWalker iw = new InterceptorWalker();
            return iw.Visit(expression);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static LambdaExpression Intercept(LambdaExpression lambda) {
            InterceptorWalker iw = new InterceptorWalker();
            return iw.Visit(lambda) as LambdaExpression;
        }

        internal class InterceptorSiteBinder : CallSiteBinder {
            private readonly CallSiteBinder _binder;

            internal InterceptorSiteBinder(CallSiteBinder binder) {
                _binder = binder;
            }

            public override int GetHashCode() {
                return _binder.GetHashCode();
            }

            public override bool Equals(object obj) {
                return obj != null && obj.Equals(_binder);
            }

            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                Expression binding = _binder.Bind(args, parameters, returnLabel);

                //
                // TODO: Implement interceptor action here
                //

                //
                // Call interceptor recursively to continue intercepting on rules
                //
                return Interceptor.Intercept(binding);
            }
        }

        internal class InterceptorWalker : ExpressionVisitor {
            protected override Expression VisitDynamic(DynamicExpression node) {
                CallSiteBinder binder = node.Binder;
                if (!(binder is InterceptorSiteBinder)) {
                    binder = new InterceptorSiteBinder(binder);
                    return Expression.MakeDynamic(node.DelegateType, binder, node.Arguments);
                } else {
                    return node;
                }
            }
        }
    }
}
