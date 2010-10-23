/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting;

namespace IronRuby.Builtins {
    /// <summary>
    /// TODO: Scope should be IDO so this shouldn't be needed.
    /// </summary>
    [RubyClass("Scope", Extends = typeof(Scope), DefineIn = typeof(IronRubyOps.Clr))]
    public static class ScopeOps {
        #region method_missing

        [RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
        public static object MethodMissing(RubyScope/*!*/ scope, BlockParam block, Scope/*!*/ self, [NotNull]RubySymbol/*!*/ symbol, params object[]/*!*/ args) {
            return RubyTopLevelScope.ScopeMethodMissing(scope.RubyContext, self, block, null, symbol, args);
        }

        #endregion
    }
}
