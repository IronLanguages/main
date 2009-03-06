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

using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    /// <summary>
    /// This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.
    /// Represents a cache of runtime binding rules.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public class RuleCache<T> where T : class {
        private CallSiteRule<T>[] _rules = new CallSiteRule<T>[0];
        private readonly Object cacheLock = new Object();

        private const int MaxRules = 128;

        internal RuleCache() { }

        internal CallSiteRule<T>[] GetRules() {
            return _rules;
        }

        // move the rule +2 up.
        // this is called on every successful rule.
        internal void MoveRule(CallSiteRule<T> rule, int i) {
            // limit search to MaxSearch elements. 
            // Rule should not get too far unless it has been already moved up.
            const int MaxSearch = 8;
            int count = _rules.Length - i;
            if (count > MaxSearch) {
                count = MaxSearch;
            }

            // need a lock to make sure we are moving the right rule and not loosing any.
            lock (cacheLock) {
                i = Array.IndexOf(_rules, rule, i, count);
                if (i < 0) {
                    return;
                }
                _rules[i] = _rules[i - 1];
                _rules[i - 1] = _rules[i - 2];
                _rules[i - 2] = rule;
            }
        }

        private const int insPosition = MaxRules / 2;
        internal void AddRule(CallSiteRule<T> newRule) {
            // need a lock to make sure we are not loosing rules.
            lock (cacheLock) {
                if (_rules.Length < insPosition) {
                    _rules = _rules.AddLast(newRule);
                } else {
                    _rules = Insert(_rules, newRule);
                }
            }
        }
        
        internal void ReplaceRule(CallSiteRule<T> oldRule, CallSiteRule<T> newRule) {
            // need a lock to make sure we are replacing the right rule
            lock (cacheLock) {
                int i = Array.IndexOf(_rules, oldRule);
                if (i >= 0) {
                    _rules[i] = newRule;
                    return; // DONE
                }
            }
            // could not find it.
            AddRule(newRule);
        }
               
        
        //inserts items at insPosition
        private static CallSiteRule<T>[] Insert(CallSiteRule<T>[] rules, CallSiteRule<T> item) {
            CallSiteRule<T>[] newRules;

            int newLength = rules.Length + 1;
            if (newLength > MaxRules) {
                newLength = MaxRules;
                newRules = rules;
            } else {
                newRules = new CallSiteRule<T>[newLength];
            }

            Array.Copy(rules, 0, newRules, 0, insPosition);
            newRules[insPosition] = item;
            Array.Copy(rules, insPosition, newRules, insPosition + 1, newLength - insPosition - 1);
            return newRules;
        }
    }
}
