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

using System.Dynamic;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices {

    // Conceptually these are instance methods on CallSite<T> but
    // we don't want users to see them

    /// <summary>
    /// This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public static class CallSiteOps {

        /// <summary>
        /// Sets the target of the dynamic call site to the given call site rule.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <param name="rule">An instance of the call site rule.</param>
        /// <returns>A delegate representing the call site rule.</returns>
        [Obsolete("do not use this method", true)]
        public static T SetTarget<T>(CallSite<T> site, CallSiteRule<T> rule) where T : class {
            return site.Target = rule.RuleSet.GetTarget();
        }

        /// <summary>
        /// Creates an instance of a dynamic call site used for cache lookup.
        /// </summary>
        /// <returns>The new call site.</returns>
        [Obsolete("do not use this method", true)]
        public static CallSite CreateMatchmaker() {
            var mm = new CallSite(null);
            CallSiteOps.ClearMatch(mm);
            return mm;
        }

        /// <summary>
        /// Checks if a dynamic site requires an update.
        /// </summary>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <returns>true if rule needs updating, false otherwise.</returns>
        [Obsolete("do not use this method", true)]
        public static bool NeedsUpdate(CallSite site) {
            var res = site._match;
            site._match = false;  //avoid branch here to make sure the method is inlined
            return res;
        }

        /// <summary>
        /// Checks whether the executed rule matched
        /// </summary>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <returns>true if rule matched, false otherwise.</returns>
        [Obsolete("do not use this method", true)]
        public static bool GetMatch(CallSite site) {
            return site._match;
        }

        /// <summary>
        /// Clears the match flag on the matchmaker call site.
        /// </summary>
        /// <param name="site">An instance of the dynamic call site.</param>
        [Obsolete("do not use this method", true)]
        public static void ClearMatch(CallSite site) {
            site._match = true;
        }

        /// <summary>
        /// Adds a rule to the cache maintained on the dynamic call site.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <param name="rule">An instance of the call site rule.</param>
        [Obsolete("do not use this method", true)]
        public static void AddRule<T>(CallSite<T> site, CallSiteRule<T> rule) where T : class {
            if (site.Rules == null) {
                site.Rules = rule.RuleSet;
            } else {
                site.Rules = site.Rules.AddRule(rule);
            }
        }

        /// <summary>
        /// Updates rules in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="this">An instance of the dynamic call site.</param>
        /// <param name="matched">The matched rule index.</param>
        [Obsolete("do not use this method", true)]
        public static void UpdateRules<T>(CallSite<T> @this, int matched) where T : class {
            if (matched > 1) {
                @this.Rules.MoveRule(matched);
            }
        }
        
        /// <summary>
        /// Gets the dynamic binding rules from the call site.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <returns>An array of dynamic binding rules.</returns>
        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T>[] GetRules<T>(CallSite<T> site) where T : class {
            return (site.Rules == null) ? null : site.Rules.GetRules();
        }


        /// <summary>
        /// Retrieves binding rule cache.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <returns>The cache.</returns>
        [Obsolete("do not use this method", true)]
        public static RuleCache<T> GetRuleCache<T>(CallSite<T> site) where T : class {
            return site.Binder.GetRuleCache<T>();
        }

        /// <summary>
        /// Moves the binding rule within the cache.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="cache">Cache.</param>
        /// <param name="rule">An instance of the call site rule.</param>
        /// <param name="i">An index of the call site rule.</param>
        [Obsolete("do not use this method", true)]
        public static void MoveRule<T>(RuleCache<T> cache, CallSiteRule<T> rule, int i) where T : class {
            if (i > 1) {
                cache.MoveRule(rule, i);
            }
        }

        /// <summary>
        /// Searches the dynamic rule cache for rules applicable to the dynamic operation.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="cache">The cache.</param>
        /// <returns>The array of applicable rules.</returns>
        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T>[] FindApplicableRules<T>(RuleCache<T> cache) where T : class {
            return cache.GetRules();
        }

        /// <summary>
        /// Creates a new call site rule for the dynamic operation by calling the <see cref="CallSiteBinder.Bind"/> on the call site's binder.
        /// </summary>
        /// <typeparam name="T">The type of the delegate of the <see cref="CallSite"/>.</typeparam>
        /// <param name="cache">Cache.</param>
        /// <param name="site">An instance of the dynamic call site.</param>
        /// <param name="oldRule">A binding rule to be removed from the cache.</param>
        /// <param name="originalRule">A binding rule originally found on the dynamic call site.</param>
        /// <param name="args">An array of runtime values for the dynamic binding.</param>
        /// <returns>The new cal site rule.</returns>
        [Obsolete("do not use this method", true)]
        public static CallSiteRule<T> CreateNewRule<T>(RuleCache<T> cache, CallSite<T> site, CallSiteRule<T> oldRule, CallSiteRule<T> originalRule, object[] args) where T : class {
            Expression binding = site.Binder.Bind(args, CallSiteRule<T>.Parameters, CallSiteRule<T>.ReturnLabel);

            //
            // Check the produced rule
            //
            if (binding == null) {
                throw Error.NoOrInvalidRuleProduced();
            }

            var newRule = new CallSiteRule<T>(binding);

            if (originalRule != null) {
                // compare our new rule and our original monomorphic rule.  If they only differ from constants
                // then we'll want to re-use the code gen if possible.
                newRule = AutoRuleTemplate.CopyOrCreateTemplatedRule(originalRule, newRule);
            }

            //
            // Add the rule to the level 2 cache. This is an optimistic add so that cache miss
            // on another site can find this existing rule rather than building a new one.
            //
            if (oldRule != null) {
                //
                // The rule didn't work and since we optimistically added it into the
                // level 2 cache. Remove it now since the rule is no good.
                //
                cache.ReplaceRule(oldRule, newRule);
            } else {
                cache.AddRule(newRule);
            }

            return newRule;
        }
    }
}
