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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

namespace System.Dynamic {
    /// <summary>
    /// This uses linear search to find a rule.  Clearly that doesn't scale super well.
    /// We will address this in the future.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RuleCache<T> where T : class {
        private readonly LinkedList<CallSiteRule<T>> _list = new LinkedList<CallSiteRule<T>>();
        private const int MaxRulesPerSignaturePerCallSiteBinder = 100;

        internal RuleCache() {
        }

        /// <summary>
        /// Looks through the rule list, and returns rules that apply
        /// </summary>
        internal CallSiteRule<T>[] FindApplicableRules() {
            lock (_list) {
                //
                // Clone the list for execution
                //
                int live = _list.Count;
                CallSiteRule<T>[] rules = null;

                if (live > 0) {
                    rules = new CallSiteRule<T>[live];
                    int index = 0;

                    LinkedListNode<CallSiteRule<T>> node = _list.First;
                    while (node != null) {
                        rules[index++] = node.Value;
                        node = node.Next;
                    }
                    Debug.Assert(index == live);
                }
                //
                // End of lock
                //

                return rules;
            }
        }

        internal void AddRule(CallSiteRule<T> rule) {
            lock (_list) {
                _list.AddFirst(rule);
                if (_list.Count > MaxRulesPerSignaturePerCallSiteBinder) {
                    _list.RemoveLast();
                }
            }
        }

        internal void RemoveRule(CallSiteRule<T> rule) {
            lock (_list) {
                LinkedListNode<CallSiteRule<T>> node = _list.First;
                while (node != null) {
                    if (node.Value == rule) {
                        _list.Remove(node);
                        break;
                    }
                    node = node.Next;
                }
            }
        }

        internal void MoveRule(CallSiteRule<T> rule) {
            lock (_list) {
                LinkedListNode<CallSiteRule<T>> node = _list.First;
                while (node != null) {
                    if (node.Value == rule) {
                        _list.Remove(node);
                        _list.AddFirst(node);
                        break;
                    }
                    node = node.Next;
                }
            }
        }
    }
}
