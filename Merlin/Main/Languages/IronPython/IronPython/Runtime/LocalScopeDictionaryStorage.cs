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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    internal class LocalScopeDictionaryStorage : ScopeDictionaryStorage {
        public LocalScopeDictionaryStorage(Scope scope)
            : base(scope) {

        }

        private static bool ScopeVisible(Scope curScope, Scope myScope) {
            if (myScope.Parent != null) {
                // non-leaf classes and globals (top most)
                return (curScope == myScope || curScope.IsVisible) && curScope.Parent != null;
            }
            // top-level locals, we'll just iterate once
            return curScope != null;
        }

        protected override IEnumerable<Scope> GetVisibleScopes() {
            Scope curScope = Scope;
            while (ScopeVisible(curScope, Scope)) {
                yield return curScope;
                curScope = curScope.Parent;
            }
        }

        /// <summary>
        /// if the locals scope is composed of only a single dictionary, returns 
        /// it.  Otherwise returns the virtualized LocalsDictionary 
        /// </summary>
        internal static IAttributesCollection GetDictionaryFromScope(Scope scope) {
            Scope curScope = scope;
            int count = 0;
            while (ScopeVisible(curScope, scope)) {
                curScope = curScope.Parent;
                count++;
            }

            if (count == 1) {
                PythonDictionary pd = scope.Dict as PythonDictionary;
                if (pd != null) {
                    return pd;
                }
            }

            return new PythonDictionary(new LocalScopeDictionaryStorage(scope));
        }
    }    
}
