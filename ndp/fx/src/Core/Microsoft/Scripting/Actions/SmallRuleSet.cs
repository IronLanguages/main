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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic {

    /// <summary>
    /// This holds a set of rules for a particular DynamicSite.  Any given
    /// SmallRuleSet instance is immutable and therefore they may be cached
    /// and shared.  At the moment, the only ones that are shared are
    /// SmallRuleSets with a single rule.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class SmallRuleSet<T> where T : class {
        private const int MaxRules = 10;
        private readonly T[] _rules;

        internal SmallRuleSet(T[] rules) {
            _rules = rules;
        }

        internal SmallRuleSet<T> AddRule(T newRule) {
            var temp = _rules.AddFirst(newRule);
            if (_rules.Length < MaxRules) {
                return new SmallRuleSet<T>(temp);
            } else {
                Array.Copy(temp, _rules, MaxRules);
                return this;
            }
        }

        // moves rule +2 up.
        internal void MoveRule(int i) {
            var rule = _rules[i];

            _rules[i] = _rules[i - 1];
            _rules[i - 1] = _rules[i - 2];
            _rules[i - 2] = rule;
        }

        internal T[] GetRules() {
            return _rules;
        }
    }
}
