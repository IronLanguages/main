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

using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace IronRuby.Builtins {
    [RubyModule("IronRuby", Extends = typeof(Ruby), Restrictions = ModuleRestrictions.NotPublished)]
    public static class IronRubyOps {

        [RubyMethod("configuration", RubyMethodAttributes.PublicSingleton)]
        public static DlrConfiguration/*!*/ GetConfiguration(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return context.DomainManager.Configuration;
        }

        [RubyMethod("globals", RubyMethodAttributes.PublicSingleton)]
        public static Scope/*!*/ GetGlobalScope(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return context.DomainManager.Globals;
        }

        [RubyMethod("loaded_assemblies", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetLoadedAssemblies(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return new RubyArray(context.DomainManager.GetLoadedAssemblyList());
        }

        /// <summary>
        /// Gets a live read-only and thread-safe dictionary that maps full paths of the loaded scripts to their scopes.
        /// </summary>
        [RubyMethod("loaded_scripts", RubyMethodAttributes.PublicSingleton)]
        public static IDictionary<string, Scope>/*!*/ GetLoadedScripts(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return new ReadOnlyDictionary<string, Scope>(context.Loader.LoadedScripts);
        }

        /// <summary>
        /// The same as Kernel#require except for that it returns the loaded Assembly or Scope (even if already loaded).
        /// </summary>
        [RubyMethod("require", RubyMethodAttributes.PublicSingleton)]
        public static object/*!*/ Require(RubyScope/*!*/ scope, RubyModule/*!*/ self, MutableString/*!*/ libraryName) {
            object loaded;
            
            scope.RubyContext.Loader.LoadFile(
                null, self, libraryName, LoadFlags.LoadOnce | LoadFlags.AppendExtensions | LoadFlags.ResolveLoaded | LoadFlags.AnyLanguage, out loaded
            );

            Debug.Assert(loaded != null);
            return loaded;
        }

        /// <summary>
        /// The same as Kernel#require except for that it returns the loaded Assembly or Scope.
        /// </summary>
        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        public static object/*!*/ Load(RubyScope/*!*/ scope, RubyModule/*!*/ self, MutableString/*!*/ libraryName) {
            object loaded;
            scope.RubyContext.Loader.LoadFile(null, self, libraryName, LoadFlags.ResolveLoaded | LoadFlags.AnyLanguage, out loaded);
            Debug.Assert(loaded != null);
            return loaded;
        }

        [RubyModule("Clr", Restrictions = ModuleRestrictions.NoUnderlyingType)]
        public static class Clr {
            [RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
            public static Hash/*!*/ GetProfile(RubyContext/*!*/ context, object self) {
                if (!context.RubyOptions.Profile) {
                    throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
                }

                Hash result = new Hash(context);
                foreach (var entry in Profiler.Instance.GetProfile()) {
                    result[entry.Id] = Utils.DateTimeTicksFromStopwatch(entry.Ticks);
                }
                return result;
            }

            [RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
            public static object GetProfile(RubyContext/*!*/ context, BlockParam/*!*/ block, object self) {
                if (!context.RubyOptions.Profile) {
                    throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
                }

                var start = Profiler.Instance.GetProfile();
                object blockResult;
                if (block.Yield(out blockResult)) {
                    return blockResult;
                }

                var startDict = new Dictionary<string, long>();
                foreach (var counter in start) {
                    startDict[counter.Id] = counter.Ticks;
                }

                Hash result = new Hash(context);
                foreach (var entry in Profiler.Instance.GetProfile()) {
                    long startTime;
                    if (!startDict.TryGetValue(entry.Id, out startTime)) {
                        startTime = 0;
                    }
                    long elapsed = entry.Ticks - startTime;
                    if (elapsed > 0) {
                        result[entry.Id] = Utils.DateTimeTicksFromStopwatch(elapsed);
                    }
                }
                return result;
            }
        }
    }
}
