/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    // TODO: remove
    public static class RubySites {
        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> InspectSharedSite =
            CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(InstanceCallAction("inspect"));

        public static MutableString Inspect(RubyContext/*!*/ context, object obj) {
            return InspectSharedSite.Target(InspectSharedSite, context, obj);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> RespondToSharedSite =
            CallSite<Func<CallSite, RubyContext, object, SymbolId, object>>.Create(InstanceCallAction("respond_to?", 1));

        public static bool RespondTo(RubyContext/*!*/ context, object obj, string name) {
            return RubyOps.IsTrue(RespondToSharedSite.Target(RespondToSharedSite, context, obj, SymbolTable.StringToId(name)));
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> ToSSharedSite = CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(
            InstanceCallAction("to_s"));

        public static MutableString ToS(RubyContext/*!*/ context, object value) {
            return ToSSharedSite.Target(ToSSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> ModuleConstMissingSharedSite = CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>>.Create(
            InstanceCallAction("const_missing", 1));

        public static object ModuleConstMissing(RubyContext/*!*/ context, RubyModule self, string/*!*/ name) {
            return ModuleConstMissingSharedSite.Target(ModuleConstMissingSharedSite, context, self, SymbolTable.StringToId(name));
        }

        #region Helpers

        public static RubyCallAction InstanceCallAction(string/*!*/ name) {
            return RubyCallAction.Make(name, 0);
        }
        
        public static RubyCallAction InstanceCallAction(string/*!*/ name, int argumentCount) {
            return RubyCallAction.Make(name, argumentCount);
        }

        public static RubyCallAction InstanceCallAction(string/*!*/ name, RubyCallSignature callSignature) {
            return RubyCallAction.Make(name, callSignature);
        }

        #endregion        
    }
}
