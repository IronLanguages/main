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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace IronPython.Runtime.Operations {
    public static class ScriptScopeOps {

        // dynamic behavior of the scope:
        // TODO: remove? Nessie uses this, but it should be possible to fix Nessie

        [SpecialName]
        public static IList<object>/*!*/ GetMemberNames(CodeContext/*!*/ context, ScriptScope/*!*/ scope) {
            return ScopeOps.GetMemberNames(context, HostingHelpers.GetScope(scope));
        }

        [SpecialName]
        public static object GetCustomMember(CodeContext/*!*/ context, ScriptScope/*!*/ scope, string name) {
            return ScopeOps.GetCustomMember(context, HostingHelpers.GetScope(scope), name);
        }

        [SpecialName]
        public static void SetMember(CodeContext/*!*/ context, ScriptScope/*!*/ scope, string name, object value) {
            ScopeOps.SetMember(context, HostingHelpers.GetScope(scope), name, value);
        }

        [SpecialName]
        public static bool DeleteMember(CodeContext/*!*/ context, ScriptScope/*!*/ scope, string name) {
            return ScopeOps.DeleteMember(context, HostingHelpers.GetScope(scope), name);
        }
    }
}
