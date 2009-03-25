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
using System.Diagnostics;
using System;

namespace IronRuby.Builtins {
    [RubyModule("IronRuby", Extends = typeof(Ruby))]
    public static class IronRubyOps {

        [RubyModule("Clr")]
        public static class ClrOps {
            
            //[RubyConstant("String")]
            //public static RubyModule/*!*/ GetClrStringModule(RubyModule/*!*/ module) {
            //    return GetModule(module, typeof(ClrString));
            //}

            //[RubyConstant("Float")]
            //public static RubyModule/*!*/ GetClrFloatModule(RubyModule/*!*/ module) {
            //    return GetModule(module, typeof(ClrFloat));
            //}

            //[RubyConstant("Integer")]
            //public static RubyModule/*!*/ GetClrIntegerModule(RubyModule/*!*/ module) {
            //    return GetModule(module, typeof(ClrInteger));
            //}

            //[RubyConstant("BigInteger")]
            //public static RubyModule/*!*/ GetClrBigIntegerModule(RubyModule/*!*/ module) {
            //    return GetModule(module, typeof(ClrBigInteger));
            //}

            //[RubyConstant("MultiDimensionalArray")]
            //public static RubyModule/*!*/ GetMultiDimensionalArrayModule(RubyModule/*!*/ module) {
            //    return GetModule(module, typeof(MultiDimensionalArray));
            //}

            //private static RubyModule/*!*/ GetModule(RubyModule/*!*/ declaringModule, Type/*!*/ type) {
            //    RubyModule result;
            //    declaringModule.Context.TryGetModule(type, out result);
            //    Debug.Assert(result != null);
            //    return result;
            //}

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
