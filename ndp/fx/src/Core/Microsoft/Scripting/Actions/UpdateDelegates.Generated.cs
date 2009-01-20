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

using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Dynamic {
    internal static partial class UpdateDelegates {

        /// <summary>
        /// Caches a single Matchmaker and its delegate to avoid expensive delegate
        /// recreation.  We just Interlock.Exchange this out each time we need one
        /// and replace it when we're done.  If multiple threads are operating we'll
        /// sometimes end up creating multiple delegates which is as bad w/o the
        /// cache.
        /// </summary>
        private static class MatchmakerCache<T> {
            public static Matchmaker Info;
        }

        private partial class Matchmaker {
            internal bool Match;
            internal Delegate Delegate;
        }

        //
        // WARNING: do not edit these methods here. The real source code lives
        // in two places: generate_dynsites.py, which generates the methods in
        // this file, and UpdateDelegates.cs, which dynamically generates
        // methods like these at run time. If you want to make a change, edit
        // *both* of those files instead
        //

        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute0<TRet>(CallSite site) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, TRet>>)site;
            CallSiteRule<Func<CallSite, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, TRet>> rule;
            Func<CallSite, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback0<TRet>;
            } else {
                ruleTarget = (Func<CallSite, TRet>)mm.Delegate;
            }

            try {
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        result = ruleTarget(site);
                        if (mm.Match) {
                            return result;
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback0<TRet>(CallSite site) {
                Match = false;
                return default(TRet);
            }
        }



        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid0(CallSite site) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite>>)site;
            CallSiteRule<Action<CallSite>>[] applicable;
            CallSiteRule<Action<CallSite>> rule;
            Action<CallSite> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid0;
            } else {
                ruleTarget = (Action<CallSite>)mm.Delegate;
            }

            try {
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        ruleTarget(site);
                        if (mm.Match) {
                            return;
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            ruleTarget(site);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        ruleTarget(site);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid0(CallSite site) {
                Match = false;
                return;
            }
        }



        #region Generated UpdateAndExecute Methods

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_update_targets from: generate_dynsites.py


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute1<T0, TRet>(CallSite site, T0 arg0) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, TRet>> rule;
            Func<CallSite, T0, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback1<T0, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback1<T0, TRet>(CallSite site, T0 arg0) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute2<T0, T1, TRet>(CallSite site, T0 arg0, T1 arg1) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, TRet>> rule;
            Func<CallSite, T0, T1, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback2<T0, T1, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback2<T0, T1, TRet>(CallSite site, T0 arg0, T1 arg1) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute3<T0, T1, T2, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, TRet>> rule;
            Func<CallSite, T0, T1, T2, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback3<T0, T1, T2, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback3<T0, T1, T2, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute4<T0, T1, T2, T3, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback4<T0, T1, T2, T3, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback4<T0, T1, T2, T3, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute5<T0, T1, T2, T3, T4, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback5<T0, T1, T2, T3, T4, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback5<T0, T1, T2, T3, T4, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute6<T0, T1, T2, T3, T4, T5, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, T5, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback6<T0, T1, T2, T3, T4, T5, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback6<T0, T1, T2, T3, T4, T5, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute7<T0, T1, T2, T3, T4, T5, T6, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback7<T0, T1, T2, T3, T4, T5, T6, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback7<T0, T1, T2, T3, T4, T5, T6, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static TRet UpdateAndExecute10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>)site;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>[] applicable;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>> rule;
            Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> ruleTarget, startingTarget = @this.Target;
            TRet result;

            int count, index;
            CallSiteRule<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.Fallback10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>;
            } else {
                ruleTarget = (Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                            if (mm.Match) {
                                return result;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                            result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                            if (mm.Match) {
                                return result;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                        result = ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                        if (mm.Match) {
                            return result;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Func<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal TRet Fallback10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
                Match = false;
                return default(TRet);
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid1<T0>(CallSite site, T0 arg0) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0>>)site;
            CallSiteRule<Action<CallSite, T0>>[] applicable;
            CallSiteRule<Action<CallSite, T0>> rule;
            Action<CallSite, T0> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid1<T0>;
            } else {
                ruleTarget = (Action<CallSite, T0>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid1<T0>(CallSite site, T0 arg0) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid2<T0, T1>(CallSite site, T0 arg0, T1 arg1) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1>>)site;
            CallSiteRule<Action<CallSite, T0, T1>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1>> rule;
            Action<CallSite, T0, T1> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid2<T0, T1>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid2<T0, T1>(CallSite site, T0 arg0, T1 arg1) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid3<T0, T1, T2>(CallSite site, T0 arg0, T1 arg1, T2 arg2) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2>> rule;
            Action<CallSite, T0, T1, T2> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid3<T0, T1, T2>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid3<T0, T1, T2>(CallSite site, T0 arg0, T1 arg1, T2 arg2) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid4<T0, T1, T2, T3>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3>> rule;
            Action<CallSite, T0, T1, T2, T3> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid4<T0, T1, T2, T3>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid4<T0, T1, T2, T3>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid5<T0, T1, T2, T3, T4>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4>> rule;
            Action<CallSite, T0, T1, T2, T3, T4> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid5<T0, T1, T2, T3, T4>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid5<T0, T1, T2, T3, T4>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid6<T0, T1, T2, T3, T4, T5>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4, T5>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5>> rule;
            Action<CallSite, T0, T1, T2, T3, T4, T5> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid6<T0, T1, T2, T3, T4, T5>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4, T5>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid6<T0, T1, T2, T3, T4, T5>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid7<T0, T1, T2, T3, T4, T5, T6>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>> rule;
            Action<CallSite, T0, T1, T2, T3, T4, T5, T6> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid7<T0, T1, T2, T3, T4, T5, T6>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4, T5, T6>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid7<T0, T1, T2, T3, T4, T5, T6>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid8<T0, T1, T2, T3, T4, T5, T6, T7>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>> rule;
            Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid8<T0, T1, T2, T3, T4, T5, T6, T7>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid8<T0, T1, T2, T3, T4, T5, T6, T7>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>> rule;
            Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
                Match = false;
                return;
            }    
        }


        [Obsolete("pregenerated CallSite<T>.Update delegate", true)]
        internal static void UpdateAndExecuteVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            //
            // Declare the locals here upfront. It actually saves JIT stack space.
            //
            var @this = (CallSite<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>)site;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>[] applicable;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> rule;
            Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> ruleTarget, startingTarget = @this.Target;

            int count, index;
            CallSiteRule<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>> originalRule = null;

            // get the matchmaker & its delegate
            Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Info, null);
            if (mm == null) {
                mm = new Matchmaker();
                mm.Delegate = ruleTarget = mm.FallbackVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            } else {
                ruleTarget = (Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)mm.Delegate;
            }

            try {    
                //
                // Create matchmaker and its site. We'll need them regardless.
                //
                mm.Match = true;
                site = CallSiteOps.CreateMatchmaker(
                    @this,
                    ruleTarget
                );

                //
                // Level 1 cache lookup
                //
                if ((applicable = CallSiteOps.GetRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        if ((object)startingTarget == (object)ruleTarget) {
                            // if we produce another monomorphic
                            // rule we should try and share code between the two.
                            originalRule = rule;
                        } else {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                            if (mm.Match) {
                                return;
                            }        

                            // Rule didn't match, try the next one
                            mm.Match = true;            
                        }                
                    }
                }

                //
                // Level 2 cache lookup
                //

                //
                // Any applicable rules in level 2 cache?
                //
                if ((applicable = CallSiteOps.FindApplicableRules(@this)) != null) {
                    for (index = 0, count = applicable.Length; index < count; index++) {
                        rule = applicable[index];

                        //
                        // Execute the rule
                        //
                        ruleTarget = CallSiteOps.SetTarget(@this, rule);

                        try {
                             ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                            if (mm.Match) {
                                return;
                            }
                        } finally {
                            if (mm.Match) {
                                //
                                // Rule worked. Add it to level 1 cache
                                //
                                CallSiteOps.AddRule(@this, rule);
                                // and then move it to the front of the L2 cache
                                @this.RuleCache.MoveRule(rule);
                            }
                        }

                        if ((object)startingTarget == (object)ruleTarget) {
                            // If we've gone megamorphic we can still template off the L2 cache
                            originalRule = rule;
                        }

                        // Rule didn't match, try the next one
                        mm.Match = true;
                    }
                }


                //
                // Miss on Level 0, 1 and 2 caches. Create new rule
                //

                rule = null;
                var args = new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 };

                for (; ; ) {
                    rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);

                    //
                    // Execute the rule on the matchmaker site
                    //

                    ruleTarget = CallSiteOps.SetTarget(@this, rule);

                    try {
                         ruleTarget(site, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                        if (mm.Match) {
                            return;
                        }
                    } finally {
                        if (mm.Match) {
                            //
                            // The rule worked. Add it to level 1 cache.
                            //
                            CallSiteOps.AddRule(@this, rule);
                        }
                    }

                    // Rule we got back didn't work, try another one
                    mm.Match = true;
                }
            } finally {
                Interlocked.Exchange(ref MatchmakerCache<Action<CallSite, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>.Info, mm);
            }
        }

        private partial class Matchmaker {
            internal void FallbackVoid10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
                Match = false;
                return;
            }    
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
