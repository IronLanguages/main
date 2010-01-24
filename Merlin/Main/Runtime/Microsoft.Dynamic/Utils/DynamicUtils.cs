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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;

namespace Microsoft.Scripting.Utils {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public static class DynamicUtils {
        /// <summary>
        /// Returns the list of expressions represented by the <see cref="DynamicMetaObject"/> instances.
        /// </summary>
        /// <param name="objects">An array of <see cref="DynamicMetaObject"/> instances to extract expressions from.</param>
        /// <returns>The array of expressions.</returns>
        public static Expression[] GetExpressions(DynamicMetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                DynamicMetaObject mo = objects[i];
                res[i] = mo != null ? mo.Expression : null;
            }

            return res;
        }

        /// <summary>
        /// Creates an instance of <see cref="DynamicMetaObject"/> for a runtime value and the expression that represents it during the binding process.
        /// </summary>
        /// <param name="argValue">The runtime value to be represented by the <see cref="DynamicMetaObject"/>.</param>
        /// <param name="parameterExpression">An expression to represent this <see cref="DynamicMetaObject"/> during the binding process.</param>
        /// <returns>The new instance of <see cref="DynamicMetaObject"/>.</returns>
        public static DynamicMetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicMetaObjectProvider ido = argValue as IDynamicMetaObjectProvider;
            if (ido != null) {
                return ido.GetMetaObject(parameterExpression);
            } else {
                return new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty, argValue);
            }
        }

        public static T LightBind<T>(this DynamicMetaObjectBinder binder, CallSite<T> site, object[] args, int compilationThreshold) where T : class {
            Delegate d = Bind<T>(binder, args).LightCompile(compilationThreshold);
            T result = (T)(object)d;

            LightLambda lambda = d.Target as LightLambda;
            if (lambda != null) {
                lambda.Compile += (_, e) => {
                    // If the rule is still used in the site.Target we can replace it by the compiled delegate.
                    // site.Target can be updated by another thread before we write the compiled delegate. 
                    // In such case we run the compiled rule, detect that it is not applicable and replace it from rule cache.
                    // TODO: replace the interpreted delegates in L1 and L2 caches as well?
                    if (site.Target == result) {
                        site.Target = (T)(object)e.Compiled;
                    }
                };
            } else {
                PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "Rule not interpreted");
            }
            return result;
        }

        public static LambdaExpression/*!*/ Bind<T>(this DynamicMetaObjectBinder binder, object[] args) where T : class {
            var signature = LambdaSignature<T>.Instance;

            LabelTarget returnLabel;
            if (signature.ReturnLabel.Type == typeof(object) && binder.ReturnType != typeof(void) && binder.ReturnType != typeof(object)) {
                returnLabel = Expression.Label(binder.ReturnType);
            } else {
                returnLabel = signature.ReturnLabel;
            }

            Expression binding = binder.Bind(args, signature.Parameters, returnLabel);
            if (binding == null) {
                throw new InvalidOperationException("CallSiteBinder.Bind must return non-null meta-object");
            }

            return Stitch<T>(binding, signature, returnLabel);
        }

        // TODO: This should be merged into CallSiteBinder.
        private static LambdaExpression/*!*/ Stitch<T>(Expression binding, LambdaSignature<T> signature, LabelTarget returnLabel) where T : class {
            Expression updLabel = Expression.Label(CallSiteBinder.UpdateLabel);

            var site = Expression.Parameter(typeof(CallSite), "$site");
            var @params = ArrayUtils.Insert(site, signature.Parameters);
            
            Expression body;
            if (returnLabel != signature.ReturnLabel) {
                // TODO:
                // This allows the binder to produce a strongly typed binding expression that gets boxed 
                // if the call site's return value is of type object. 
                // The current implementation of CallSiteBinder is too strict as it requires the two types to be reference-assignable.
                
                var tmp = Expression.Parameter(typeof(object));
                body = Expression.Convert(
                    Expression.Block(new[] { tmp },
                        binding,
                        updLabel,
                        Expression.Label(
                            returnLabel,
                            Expression.Condition(
                                Expression.NotEqual(
                                    Expression.Assign(
                                        tmp, 
                                        Expression.Invoke(
                                            Expression.Property(
                                                Expression.Convert(site, typeof(CallSite<T>)),
                                                typeof(CallSite<T>).GetProperty("Update")
                                            ),
                                            @params
                                        )
                                    ),
                                    AstUtils.Constant(null)
                                ),
                                Expression.Convert(tmp, returnLabel.Type),
                                Expression.Default(returnLabel.Type)
                            )
                        )
                    ),
                    typeof(object)
                );
            } else {
                body = Expression.Block(
                    binding,
                    updLabel,
                    Expression.Label(
                        returnLabel, 
                        Expression.Invoke(
                            Expression.Property(
                                Expression.Convert(site, typeof(CallSite<T>)),
                                typeof(CallSite<T>).GetProperty("Update")
                            ),
                            @params
                        )
                    )
                );
            }

            return Expression.Lambda<T>(
                body,
                "CallSite.Target",
                true, // always compile the rules with tail call optimization
                @params
            );
        }

        // TODO: This should be merged into CallSiteBinder.
        private sealed class LambdaSignature<T> where T : class {
            internal static readonly LambdaSignature<T> Instance = new LambdaSignature<T>();

            internal readonly ReadOnlyCollection<ParameterExpression> Parameters;
            internal readonly LabelTarget ReturnLabel;

            private LambdaSignature() {
                Type target = typeof(T);
                if (!typeof(Delegate).IsAssignableFrom(target)) {
                    throw new InvalidOperationException();
                }

                MethodInfo invoke = target.GetMethod("Invoke");
                ParameterInfo[] pis = invoke.GetParameters();
                if (pis[0].ParameterType != typeof(CallSite)) {
                    throw new InvalidOperationException();
                }

                var @params = new ReadOnlyCollectionBuilder<ParameterExpression>(pis.Length - 1);
                for (int i = 0; i < pis.Length - 1; i++) {
                    @params.Add(Expression.Parameter(pis[i + 1].ParameterType, "$arg" + i));
                }

                Parameters = @params.ToReadOnlyCollection();
                ReturnLabel = Expression.Label(invoke.GetReturnType());
            }
        }
    }
}
