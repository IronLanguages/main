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

namespace System.Dynamic.Binders {
    internal sealed class EmptyRuleSet<T> : RuleSet<T> where T : class {
        internal static readonly RuleSet<T> FixedInstance = new EmptyRuleSet<T>();

        private EmptyRuleSet() {
        }

        internal override RuleSet<T> AddRule(CallSiteRule<T> newRule) {
            return this;
        }

        internal override CallSiteRule<T>[] GetRules() {
            // TODO: could return an empty array, which would save the null
            // check in UpdateAndExecute
            return null;
        }

        internal override T GetTarget() {
            // Return null so CallSiteOps.SetPolymorphicTarget won't update the target
            return null;
        }
    }
}
