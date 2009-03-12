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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Class responsible for runtime binding of the dynamic operations on the dynamic call site.
    /// </summary>
    public abstract class CallSiteBinder {

        private static readonly LabelTarget _updateLabel = Expression.Label("CallSiteBinder.UpdateLabel");

        /// <summary>
        /// Initializes a new instance of the <see cref="CallSiteBinder"/> class.
        /// </summary>
        protected CallSiteBinder() {
        }

        /// <summary>
        /// Gets a label that can be used to cause the binding to be updated. It
        /// indicates that the expression's binding is no longer valid.
        /// This is typically used when the "version" of a dynamic object has
        /// changed.
        /// </summary>
        public static LabelTarget UpdateLabel {
            get { return _updateLabel; }
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
        public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);

        /// <summary>
        /// Provides low-level runtime binding support.  Classes can override this and provide a direct
        /// delegate for the implementation of rule.  This can enable saving rules to disk, having
        /// specialized rules available at runtime, or providing a different caching policy.
        /// </summary>
        /// <typeparam name="T">The target type of the CallSite.</typeparam>
        /// <param name="site">The CallSite the bind is being performed for.</param>
        /// <param name="args">The arguments for the binder.</param>
        /// <returns>A new delegate which replaces the CallSite Target.</returns>
        public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T : class {
            //
            // Get the Expression for the binding
            //
            Expression binding = Bind(args, CallSiteRule<T>.Parameters, CallSiteRule<T>.ReturnLabel);

            //
            // Check the produced rule
            //
            if (binding == null) {
                throw Error.NoOrInvalidRuleProduced();
            }
            
            //
            // see if we have an old rule to template off
            //
            T oldTarget = site.Target;

            RuleCache<T> cache = GetRuleCache<T>();
            CallSiteRule<T> newRule = null;
            foreach (CallSiteRule<T> cachedRule in cache.GetRules()) {
                if ((object)cachedRule.Target == (object)oldTarget) {
                    newRule = AutoRuleTemplate.CopyOrCreateTemplatedRule(cachedRule, binding);
                    break;
                }
            }

            //
            // finally produce the new rule if we need to
            //
            if (newRule == null) {
#if !MICROSOFT_SCRIPTING_CORE
                // We cannot compile rules in the heterogeneous app domains since they
                // may come from less trusted sources
                if (!AppDomain.CurrentDomain.IsHomogenous) {
                    throw Error.HomogenousAppDomainRequired();
                }
#endif
                Expression<T> e = Stitch<T>(binding);
                newRule = new CallSiteRule<T>(binding, e.Compile());
            }

            cache.AddRule(newRule);

            return newRule.Target;
        }

        /// <summary>
        /// Adds a target to the cache of known targets.  The cached targets will
        /// be scanned before calling BindDelegate to produce the new rule.
        /// </summary>
        /// <typeparam name="T">The type of target being added.</typeparam>
        /// <param name="target">The target delegate to be added to the cache.</param>
        protected void CacheTarget<T>(T target) where T : class {
            GetRuleCache<T>().AddRule(new CallSiteRule<T>(null, target));
        }

        internal static Expression<T> Stitch<T>(Expression binding) where T : class {
            Type targetType = typeof(T);
            Type siteType = typeof(CallSite<T>);

            var body = new ReadOnlyCollectionBuilder<Expression>(3);
            body.Add(binding);

            var site = Expression.Parameter(typeof(CallSite), "$site");
            var @params = CallSiteRule<T>.Parameters.AddFirst(site);

            Expression updLabel = Expression.Label(CallSiteBinder.UpdateLabel);

#if DEBUG
            // put the AST into the constant pool for debugging purposes
            updLabel = Expression.Block(
                Expression.Constant(binding),
                updLabel
            );
#endif

            
            body.Add(updLabel);
            body.Add(
                Expression.Label(
                    CallSiteRule<T>.ReturnLabel,
                    Expression.Condition(
                        Expression.Call(
                            typeof(CallSiteOps).GetMethod("SetNotMatched"),
                            @params.First()
                        ),
                        Expression.Default(CallSiteRule<T>.ReturnLabel.Type),
                        Expression.Invoke(
                            Expression.Property(
                                Expression.Convert(site, siteType),
                                typeof(CallSite<T>).GetProperty("Update")
                            ),
                            new TrueReadOnlyCollection<Expression>(@params)
                        )
                    )
                )
            );

            return new Expression<T>(
                "CallSite.Target",
                Expression.Block(body),
                true, // always compile the rules with tail call optimization
                new TrueReadOnlyCollection<ParameterExpression>(@params)
            );
        }


        /// <summary>
        /// The Level 2 cache - all rules produced for the same binder.
        /// </summary>
        internal Dictionary<Type, object> Cache;
        
        // keep alive primary binder.
        private CallSiteBinder theBinder;


        internal RuleCache<T> GetRuleCache<T>() where T : class {
            // make sure we have cache.
            if (Cache == null) {
                // to improve rule sharing try to get the primary binder and share with it.
                theBinder = GetPrimaryBinderInstance();

                // primary binder must have cache.
                if (theBinder.Cache == null) {
                    System.Threading.Interlocked.CompareExchange(
                            ref theBinder.Cache,
                            new Dictionary<Type, object>(),
                            null);
                }

                Cache = theBinder.Cache;
            }

            object ruleCache;
            var cache = Cache;
            lock (cache) {
                if (!cache.TryGetValue(typeof(T), out ruleCache)) {
                    cache[typeof(T)] = ruleCache = new RuleCache<T>();
                }
            }

            RuleCache<T> result = ruleCache as RuleCache<T>;
            System.Diagnostics.Debug.Assert(result != null);

            return result;
        }


        /// <summary>
        /// Trivial binder atomizer.
        /// </summary>
        private static WeakUniqueSet<CallSiteBinder> _binders = new WeakUniqueSet<CallSiteBinder>();
        private CallSiteBinder GetPrimaryBinderInstance() {
            return _binders.GetUniqueFor(this);
        }
    }
}
