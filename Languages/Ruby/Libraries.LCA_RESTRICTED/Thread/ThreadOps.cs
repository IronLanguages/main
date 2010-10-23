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
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Threading {
    /// <summary>
    /// These methods are loaded only after doing "require 'thread'"
    /// </summary>
    [RubyClass(Extends = typeof(Thread), Inherits = typeof(object))]
    public static class ThreadOps {
        [RubyMethod("exclusive", RubyMethodAttributes.PublicSingleton)]
        public static object Exclusive(RubyContext/*!*/ context, [NotNull]BlockParam /*!*/block, object self) {
            IronRuby.Builtins.ThreadOps.Critical(context, self, true);
            try {
                object result;
                block.Yield(out result);
                return result;
            } finally {
                // Note that we assume that Thread.critical was false when this method was called.
                // IronRuby does not support mismatched calls to Thread.critical= since it uses
                // native threads, not green threads
                IronRuby.Builtins.ThreadOps.Critical(context, self, false);
            }
        }
    }
}
