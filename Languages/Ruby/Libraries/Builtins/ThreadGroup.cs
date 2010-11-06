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

using System.Threading;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [RubyClass("ThreadGroup", Inherits = typeof(object))]
    public class ThreadGroup {
        [RubyMethod("add")]
        public static ThreadGroup/*!*/ Add([NotNull]ThreadGroup/*!*/ self, [NotNull]Thread/*!*/ thread) {
            ThreadOps.RubyThreadInfo.FromThread(thread).Group = self;
            return self;
        }

        // enclose
        // enclosed?

        [RubyMethod("list")]
        public static RubyArray/*!*/ List([NotNull]ThreadGroup/*!*/ self) {
            ThreadOps.RubyThreadInfo[] threads = ThreadOps.RubyThreadInfo.Threads;
            RubyArray result = new RubyArray(threads.Length);
            foreach (ThreadOps.RubyThreadInfo threadInfo in threads) {
                Thread thread = threadInfo.Thread;
                if (thread != null && threadInfo.Group == self) {
                    result.Add(thread);
                }
            }

            return result;
        }

        [RubyConstant]
        public readonly static ThreadGroup Default = new ThreadGroup();
    }
}
