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

using System.Diagnostics;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Open3 {

    [RubyModule("Open3")]
    public static class Open3 {
#if !SILVERLIGHT
        [RubyMethod("popen3", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static RubyArray/*!*/ OpenPipe(
            RubyContext/*!*/ context, 
            object self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ command) {

            Process process = RubyProcess.CreateProcess(context, command, true, true, true);
            RubyArray result = new RubyArray();
            result.Add(new RubyIO(context, null, process.StandardInput, IOMode.WriteOnly));
            result.Add(new RubyIO(context, process.StandardOutput, null, IOMode.ReadOnly));
            result.Add(new RubyIO(context, process.StandardError, null, IOMode.ReadOnly));

            if (context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19) {
                result.Add(ThreadOps.RubyThreadInfo.FromThread(System.Threading.Thread.CurrentThread));
            }

            return result;
        }
#endif
    }
}
