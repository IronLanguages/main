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
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Threading;

namespace System.Runtime.CompilerServices {

    //
    // A CallSite provides a fast mechanism for call-site caching of dynamic dispatch
    // behvaior. Each site will hold onto a delegate that provides a fast-path dispatch
    // based on previous types that have been seen at the call-site. This delegate will
    // call UpdateAndExecute if it is called with types that it hasn't seen before.
    // Updating the binding will typically create (or lookup) a new delegate
    // that supports fast-paths for both the new type and for any types that 
    // have been seen previously.
    // 
    // DynamicSites will generate the fast-paths specialized for sets of runtime argument
    // types. However, they will generate exactly the right amount of code for the types
    // that are seen in the program so that int addition will remain as fast as it would
    // be with custom implementation of the addition, and the user-defined types can be
    // as fast as ints because they will all have the same optimal dynamically generated
    // fast-paths.
    // 
    // DynamicSites don't encode any particular caching policy, but use their
    // CallSiteBinding to encode a caching policy.
    //


    /// <summary>
    /// A Dynamic Call Site base class. This type is used as a parameter type to the
    /// dynamic site targets. The first parameter of the delegate (T) below must be
    /// of this type.
    /// </summary>
    public class CallSite {

        // Cache of CallSite constructors for a given delegate type
        private static CacheDict<Type, Func<CallSiteBinder, CallSite>> _SiteCtors;

        /// <summary>
        /// The Binder responsible for binding operations at this call site.
        /// This binder is invoked by the UpdateAndExecute below if all Level 0,
        /// Level 1 and Level 2 caches experience cache miss.
        /// </summary>
        internal readonly CallSiteBinder _binder;

        // only CallSite<T> derives from this
        internal CallSite(CallSiteBinder binder) {
            _binder = binder;
        }

        /// <summary>
        /// used by Matchmaker sites to indicate rule match.
        /// </summary>
        internal bool _match;

        /// <summary>
        /// Class responsible for binding dynamic operations on the dynamic site.
        /// </summary>
        public CallSiteBinder Binder {
            get { return _binder; }
        }

        /// <summary>
        /// Creates a CallSite with the given delegate type and binder.
        /// </summary>
        /// <param name="delegateType">The CallSite delegate type.</param>
        /// <param name="binder">The CallSite binder.</param>
        /// <returns>The new CallSite.</returns>
        public static CallSite Create(Type delegateType, CallSiteBinder binder) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.Requires(delegateType.IsSubclassOf(typeof(Delegate)), "delegateType", Strings.TypeMustBeDerivedFromSystemDelegate);

            if (_SiteCtors == null) {
                // It's okay to just set this, worst case we're just throwing away some data
                _SiteCtors = new CacheDict<Type, Func<CallSiteBinder, CallSite>>(100);
            }
            Func<CallSiteBinder, CallSite> ctor;

            var ctors = _SiteCtors;
            lock (ctors) {
                if (!ctors.TryGetValue(delegateType, out ctor)) {
                    MethodInfo method = typeof(CallSite<>).MakeGenericType(delegateType).GetMethod("Create");
                    ctor = (Func<CallSiteBinder, CallSite>)Delegate.CreateDelegate(typeof(Func<CallSiteBinder, CallSite>), method);
                    ctors.Add(delegateType, ctor);
                }
            }
            return ctor(binder);
        }
    }

    /// <summary>
    /// Dynamic site type.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    public sealed partial class CallSite<T> : CallSite where T : class {
        /// <summary>
        /// The update delegate. Called when the dynamic site experiences cache miss.
        /// </summary>
        /// <returns>The update delegate.</returns>
        public T Update {
            get {
                Debug.Assert(_CachedUpdate != null, "all normal sites should have Update cached once there is an instance.");
                return _CachedUpdate;
            }
        }

        /// <summary>
        /// The Level 0 cache - a delegate specialized based on the site history.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public T Target;


        /// <summary>
        /// The Level 1 cache - a history of the dynamic site.
        /// </summary>
        internal SmallRuleSet<T> Rules;


        // Cached update delegate for all sites with a given T
        private static T _CachedUpdate;

        private CallSite(CallSiteBinder binder)
            : base(binder) {
            Target = GetUpdateDelegate();
        }

        /// <summary>
        /// Creates an instance of the dynamic call site, initialized with the binder responsible for the
        /// runtime binding of the dynamic operations at this call site.
        /// </summary>
        /// <param name="binder">The binder responsible for the runtime binding of the dynamic operations at this call site.</param>
        /// <returns>The new instance of dynamic call site.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static CallSite<T> Create(CallSiteBinder binder) {
            return new CallSite<T>(binder);
        }

        private T GetUpdateDelegate() {
            // This is intentionally non-static to speed up creation - in particular MakeUpdateDelegate
            // as static generic methods are more expensive than instance methods.  We call a ref helper
            // so we only access the generic static field once.
            return GetUpdateDelegate(ref _CachedUpdate);
        }

        private T GetUpdateDelegate(ref T addr) {
            if (addr == null) {
                // reduce creation cost by not using Interlocked.CompareExchange.  Calling I.CE causes
                // us to spend 25% of our creation time in JIT_GenericHandle.  Instead we'll rarely
                // create 2 delegates with no other harm caused.
                addr = MakeUpdateDelegate();
            }
            return addr;
        }

        /// <summary>
        /// Clears the rule cache ... used by the call site tests.
        /// </summary>
        private void ClearRuleCache() {
            // make sure it initialized/atomized etc...
            Binder.GetRuleCache<T>();

            var cache = Binder.Cache;

            if (cache != null) {
                lock (cache) {
                    cache.Clear();
                }
            }
        }

        internal T MakeUpdateDelegate() {
            Type target = typeof(T);
            Type[] args;
            MethodInfo invoke = target.GetMethod("Invoke");

            if (target.IsGenericType && IsSimpleSignature(invoke, out args)) {
                MethodInfo method = null;

                if (invoke.ReturnType == typeof(void)) {
                    if (target == DelegateHelpers.GetActionType(args.AddFirst(typeof(CallSite)))) {
                        method = typeof(UpdateDelegates).GetMethod("UpdateAndExecuteVoid" + args.Length, BindingFlags.NonPublic | BindingFlags.Static);
                    }
                } else {
                    if (target == DelegateHelpers.GetFuncType(args.AddFirst(typeof(CallSite)))) {
                        method = typeof(UpdateDelegates).GetMethod("UpdateAndExecute" + (args.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                    }
                }
                if (method != null) {
                    return (T)(object)method.MakeGenericMethod(args).CreateDelegate(target);
                }
            }

            return CreateCustomUpdateDelegate(invoke);
        }


        private static bool IsSimpleSignature(MethodInfo invoke, out Type[] sig) {
            ParameterInfo[] pis = invoke.GetParametersCached();
            ContractUtils.Requires(pis.Length > 0 && pis[0].ParameterType == typeof(CallSite), "T");

            Type[] args = new Type[invoke.ReturnType != typeof(void) ? pis.Length : pis.Length - 1];
            bool supported = true;

            for (int i = 1; i < pis.Length; i++) {
                ParameterInfo pi = pis[i];
                if (pi.IsByRefParameter()) {
                    supported = false;
                }
                args[i - 1] = pi.ParameterType;
            }
            if (invoke.ReturnType != typeof(void)) {
                args[args.Length - 1] = invoke.ReturnType;
            }
            sig = args;
            return supported;
        }

        //
        // WARNING: If you're changing this method, make sure you update the
        // pregenerated versions as well, which are generated by
        // generate_dynsites.py
        // The two implementations *must* be kept functionally equivalent!
        //
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private T CreateCustomUpdateDelegate(MethodInfo invoke) {
            var body = new List<Expression>();
            var vars = new List<ParameterExpression>();
            var @params = invoke.GetParametersCached().Map(p => Expression.Parameter(p.ParameterType, p.Name));
            var @return = Expression.Label(invoke.GetReturnType());
            var typeArgs = new[] { typeof(T) };

            var site = @params[0];
            var arguments = @params.RemoveFirst();

            //var @this = (CallSite<T>)site;
            var @this = Expression.Variable(typeof(CallSite<T>), "this");
            vars.Add(@this);
            body.Add(Expression.Assign(@this, Expression.Convert(site, @this.Type)));

            //CallSiteRule<T>[] applicable;
            var applicable = Expression.Variable(typeof(CallSiteRule<T>[]), "applicable");
            vars.Add(applicable);

            //CallSiteRule<T> rule;
            var rule = Expression.Variable(typeof(CallSiteRule<T>), "rule");
            vars.Add(rule);

            //T ruleTarget, startingTarget = @this.Target;
            var ruleTarget = Expression.Variable(typeof(T), "ruleTarget");
            vars.Add(ruleTarget);
            var startingTarget = Expression.Variable(typeof(T), "startingTarget");
            vars.Add(startingTarget);
            body.Add(Expression.Assign(startingTarget, Expression.Field(@this, "Target")));

            //TRet result;
            ParameterExpression result = null;
            if (@return.Type != typeof(void)) {
                vars.Add(result = Expression.Variable(@return.Type, "result"));
            }

            //int count, index;
            var count = Expression.Variable(typeof(int), "count");
            vars.Add(count);
            var index = Expression.Variable(typeof(int), "index");
            vars.Add(index);

            //CallSiteRule<T> originalRule = null;
            var originalRule = Expression.Variable(typeof(CallSiteRule<T>), "originalRule");
            vars.Add(originalRule);

            ////
            //// Create matchmaker site. We'll need it regardless.
            ////
            //site = CallSiteOps.CreateMatchmaker();
            body.Add(
                Expression.Assign(
                    site,
                    Expression.Call(
                        typeof(CallSiteOps).GetMethod("CreateMatchmaker")
                    )
                )
            );

            ////
            //// Level 1 cache lookup
            ////
            //if ((applicable = CallSiteOps.GetRules(@this)) != null) {
            //    for (index = 0, count = applicable.Length; index < count; index++) {
            //        rule = applicable[index];

            //        //
            //        // Execute the rule
            //        //
            //        ruleTarget = CallSiteOps.SetTarget(@this, rule);
            //
            //        if ((object)startingTarget == (object)ruleTarget) {
            //            // if we produce another monomorphic
            //            // rule we should try and share code between the two.
            //            originalRule = rule;
            //        } else {
            //            %(setResult)s ruleTarget(site, %(args)s);
            //            if (CallSiteOps.GetMatch(site)) {
            //                %(returnResult)s;
            //            }
            //
            //            // Rule didn't match, try the next one
            //            CallSiteOps.ClearMatch(site);
            //        }                
            //    }
            //}
            Expression invokeRule;

            Expression getMatch = Expression.Call(
                typeof(CallSiteOps).GetMethod("GetMatch"),
                site
            );

            Expression resetMatch = Expression.Call(
                typeof(CallSiteOps).GetMethod("ClearMatch"),
                site
            );

            if (@return.Type == typeof(void)) {
                invokeRule = Expression.Block(
                    Expression.Invoke(ruleTarget, new TrueReadOnlyCollection<Expression>(@params)),
                    Expression.IfThen(getMatch, Expression.Return(@return))
                );
            } else {
                invokeRule = Expression.Block(
                    Expression.Assign(result, Expression.Invoke(ruleTarget, new TrueReadOnlyCollection<Expression>(@params))),
                    Expression.IfThen(getMatch, Expression.Return(@return, result))
                );
            }

            var getRule = Expression.Assign(
                ruleTarget,
                Expression.Call(
                    typeof(CallSiteOps),
                    "SetTarget",
                    typeArgs,
                    @this,
                    Expression.Assign(rule, Expression.ArrayAccess(applicable, index))
                )
            );

            var checkOriginalRuleOrInvoke = Expression.IfThenElse(
                Expression.Equal(
                    Helpers.Convert(startingTarget, typeof(object)),
                    Helpers.Convert(ruleTarget, typeof(object))
                ),
                Expression.Assign(originalRule, rule),
                Expression.Block(invokeRule, resetMatch)
            );

            var @break = Expression.Label();

            var breakIfDone = Expression.IfThen(
                Expression.Equal(index, count),
                Expression.Break(@break)
            );

            var incrementIndex = Expression.PreIncrementAssign(index);

            body.Add(
                Expression.IfThen(
                    Expression.NotEqual(
                        Expression.Assign(applicable, Expression.Call(typeof(CallSiteOps), "GetRules", typeArgs, @this)),
                        Expression.Constant(null, applicable.Type)
                    ),
                    Expression.Block(
                        Expression.Assign(count, Expression.ArrayLength(applicable)),
                        Expression.Assign(index, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.Block(
                                breakIfDone,
                                getRule,
                                checkOriginalRuleOrInvoke,
                                incrementIndex
                            ),
                            @break,
                            null
                        )
                    )
                )
            );

            ////
            //// Level 2 cache lookup
            ////
            //var cache = @this.Binder.GetRuleCache<%(funcType)s>();

            ////
            //// Any applicable rules in level 2 cache?
            ////
            //if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
            //    count = applicable.Length;
            //    for (index = 0; index < count; index++) {
            //        rule = applicable[index];
            //
            //        //
            //        // Execute the rule
            //        //
            //        ruleTarget = CallSiteOps.SetTarget(@this, rule);
            //
            //        try {
            //            result = ruleTarget(site, arg0);
            //            if (match) {
            //                return result;
            //            }
            //        } finally {
            //            if (CallSiteOps.GetMatch(site)) {
            //                //
            //                // Rule worked. Add it to level 1 cache
            //                //
            //
            //                CallSiteOps.AddRule(@this, rule);
            //                // and then move it to the front of the L2 cache
            //                @this.RuleCache.MoveRule(rule);
            //            }
            //        }
            //
            //        if (startingTarget == ruleTarget) {
            //            // If we've gone megamorphic we can still template off the L2 cache
            //            originalRule = rule;
            //        }
            //
            //        // Rule didn't match, try the next one
            //        CallSiteOps.ClearMatch(site);
            //    }
            //}

            var cache = Expression.Variable(typeof(RuleCache<T>), "cache");
            vars.Add(cache);

            body.Add(
                Expression.Assign(
                    cache,
                    Expression.Call(typeof(CallSiteOps), "GetRuleCache", typeArgs, @this)
                )
            );

            var tryRule = Expression.TryFinally(
                invokeRule,
                Expression.IfThen(
                    getMatch,
                    Expression.Block(
                        Expression.Call(typeof(CallSiteOps), "AddRule", typeArgs, @this, rule),
                        Expression.Call(typeof(CallSiteOps), "MoveRule", typeArgs, cache, rule)
                    )
                )
            );

            var checkOriginalRule = Expression.IfThen(
                Expression.Equal(
                    Helpers.Convert(startingTarget, typeof(object)),
                    Helpers.Convert(ruleTarget, typeof(object))
                ),
                Expression.Assign(originalRule, rule)
            );


            body.Add(
                Expression.IfThen(
                    Expression.NotEqual(
                        Expression.Assign(
                            applicable,
                            Expression.Call(typeof(CallSiteOps), "FindApplicableRules", typeArgs, cache)
                        ),
                        Expression.Constant(null, applicable.Type)
                    ),
                    Expression.Block(
                        Expression.Assign(count, Expression.ArrayLength(applicable)),
                        Expression.Assign(index, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.Block(
                                breakIfDone,
                                getRule,
                                tryRule,
                                checkOriginalRule,
                                resetMatch,
                                incrementIndex
                            ),
                            @break,
                            null
                        )
                    )
                )
            );

            ////
            //// Miss on Level 0, 1 and 2 caches. Create new rule
            ////

            //rule = null;
            body.Add(Expression.Assign(rule, Expression.Constant(null, rule.Type)));

            //var args = new object[] { arg0, arg1, ... };
            var args = Expression.Variable(typeof(object[]), "args");
            vars.Add(args);
            body.Add(
                Expression.Assign(
                    args,
                    Expression.NewArrayInit(typeof(object), arguments.Map(p => Convert(p, typeof(object))))
                )
            );

            //for (; ; ) {
            //    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

            //    //
            //    // Execute the rule on the matchmaker site
            //    //
            //    ruleTarget = CallSiteOps.SetTarget(@this, rule);

            //    try {
            //        %(setResult)s ruleTarget(site, %(args)s);
            //        if (match) {
            //            %(returnResult)s;
            //        }
            //    } finally {
            //        if (match) {
            //            //
            //            // The rule worked. Add it to level 1 cache.
            //            //
            //            CallSiteOps.AddRule(@this, rule);
            //        }
            //    }

            //    // Rule we got back didn't work, try another one
            //    match = true;
            //}

            getRule = Expression.Assign(
                ruleTarget,
                Expression.Call(
                    typeof(CallSiteOps),
                    "SetTarget",
                    typeArgs,
                    @this,
                    Expression.Assign(
                        rule,
                        Expression.Call(typeof(CallSiteOps), "CreateNewRule", typeArgs, cache, @this, rule, originalRule, args)
                    )
                )
            );

            tryRule = Expression.TryFinally(
                invokeRule,
                Expression.IfThen(
                    getMatch,
                    Expression.Call(typeof(CallSiteOps), "AddRule", typeArgs, @this, rule)
                )
            );

            body.Add(
                Expression.Loop(
                    Expression.Block(getRule, tryRule, resetMatch),
                    null, null
                )
            );

            body.Add(Expression.Default(@return.Type));

            var lambda = Expression.Lambda<T>(
                Expression.Label(
                    @return,
                    Expression.Block(
                        new ReadOnlyCollection<ParameterExpression>(vars),
                        new ReadOnlyCollection<Expression>(body)
                    )
                ),
                "CallSite.Target",
                new ReadOnlyCollection<ParameterExpression>(@params)
            );

            // Need to compile with forceDynamic because T could be invisible,
            // or one of the argument types could be invisible
            return lambda.Compile();
        }

        private static Expression Convert(Expression arg, Type type) {
            if (TypeUtils.AreReferenceAssignable(type, arg.Type)) {
                return arg;
            }
            return Expression.Convert(arg, type);
        }
    }
}
