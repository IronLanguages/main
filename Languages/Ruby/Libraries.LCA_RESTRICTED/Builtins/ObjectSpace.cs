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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [RubyModule("ObjectSpace")]
    public static class ObjectSpace {
        [RubyMethod("define_finalizer", RubyMethodAttributes.PublicSingleton)]
        public static object DefineFinalizer(RubyModule/*!*/ self, object obj, Proc proc) {
            RubyArray result = new RubyArray(2);
            result.Add(0);
            result.Add(proc);
            return result;
        }

        [RubyMethod("each_object", RubyMethodAttributes.PublicSingleton)]
        public static object EachObject(BlockParam block, RubyModule/*!*/ self, [NotNull]RubyClass/*!*/ theClass) {
            if (!theClass.HasAncestor(self.Context.ModuleClass)) {
                throw RubyExceptions.CreateRuntimeError("each_object only supported for objects of type Class or Module");
            }

            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            int matches = 0;
            List<RubyModule> visitedModules = new List<RubyModule>();
            Stack<RubyModule> pendingModules = new Stack<RubyModule>();
            pendingModules.Push(theClass.Context.ObjectClass);

            while (pendingModules.Count > 0) {
                RubyModule next = pendingModules.Pop();
                visitedModules.Add(next);

                if (theClass.Context.IsKindOf(next, theClass)) {
                    matches++;

                    object result;
                    if (block.Yield(next, out result)) {
                        return result;
                    }
                }

                using (theClass.Context.ClassHierarchyLocker()) {
                    next.EnumerateConstants(delegate(RubyModule module, string name, object value) {
                        RubyModule constAsModule = value as RubyModule;
                        if (constAsModule != null && !visitedModules.Contains(constAsModule)) {
                            pendingModules.Push(constAsModule);
                        }
                        return false;
                    });
                }
            }
            return matches;
        }

        [RubyMethod("garbage_collect", RubyMethodAttributes.PublicSingleton)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        public static void GarbageCollect(RubyModule/*!*/ self) {
            GC.Collect();
        }

        [RubyMethod("undefine_finalizer", RubyMethodAttributes.PublicSingleton)]
        public static object DefineFinalizer(RubyModule/*!*/ self, object obj) {
            return obj;
        }
    }
}
