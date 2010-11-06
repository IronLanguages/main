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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [RubyModule("GC")]
    public static class RubyGC {
        [RubyMethod("enable", RubyMethodAttributes.PublicSingleton)]
        public static bool Enable(object self) {
            return false;
        }

        [RubyMethod("disable", RubyMethodAttributes.PublicSingleton)]
        public static bool Disable(object self) {
            return false;
        }

        [RubyMethod("start", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("garbage_collect", RubyMethodAttributes.PublicInstance)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        public static void GarbageCollect(object self) {
            GC.Collect();
        }
    }
}
