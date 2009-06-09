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

using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using System.Diagnostics;
using System;

namespace IronRuby.Builtins {
    [RubyModule("IronRuby", Extends = typeof(Ruby), Restrictions = ModuleRestrictions.None)]
    public static class IronRubyOps {

        [RubyMethod("dlr_config", RubyMethodAttributes.PublicSingleton)]
        public static DlrConfiguration/*!*/ GetCurrentRuntimeConfiguration(RubyContext/*!*/ context, object self) {
            return context.DomainManager.Configuration;
        }

        [RubyModule("Clr", Restrictions = ModuleRestrictions.None)]
        public static class ClrOps {
            [RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
            public static Hash/*!*/ GetProfile(RubyContext/*!*/ context, object self) {
                if (!((RubyOptions)context.Options).Profile) {
                    throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
                }

                Hash result = new Hash(context);
                foreach (var entry in Profiler.Instance.GetProfile()) {
                    result[entry.Key] = Protocols.Normalize(Utils.DateTimeTicksFromStopwatch(entry.Value));
                }
                return result;
            }

            [RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
            public static object GetProfile(RubyContext/*!*/ context, BlockParam/*!*/ block, object self) {
                if (!((RubyOptions)context.Options).Profile) {
                    throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
                }

                var start = Profiler.Instance.GetProfile();
                object blockResult;
                if (block.Yield(out blockResult)) {
                    return blockResult;
                }

                Hash result = new Hash(context);
                foreach (var entry in Profiler.Instance.GetProfile()) {
                    long startTime;
                    if (!start.TryGetValue(entry.Key, out startTime)) {
                        startTime = 0;
                    }
                    long elapsed = entry.Value - startTime;
                    if (elapsed > 0) {
                        result[entry.Key] = Protocols.Normalize(Utils.DateTimeTicksFromStopwatch(elapsed));
                    }
                }
                return result;
            }
        }
    }
}
