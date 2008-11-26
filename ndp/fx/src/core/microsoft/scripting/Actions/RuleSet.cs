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

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic.Binders {

    /// <summary>
    /// A RuleSet is a collection of rules to apply to the objects at a DynamicSite.  Each Rule also
    /// includes a target that is to be called if the rules' conditions are met.
    /// RuleSets are all immutable.
    /// </summary>
    internal abstract class RuleSet<T> where T : class {
        internal abstract RuleSet<T> AddRule(CallSiteRule<T> newRule);
        internal abstract CallSiteRule<T>[] GetRules();
        internal abstract T GetTarget();
    }
}
