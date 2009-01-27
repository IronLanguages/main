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
            Type classType = theClass.GetType();
            bool isClass = (classType == typeof(RubyClass));
            if (!isClass && classType != typeof(RubyModule)) {
                throw new NotSupportedException("each_object only supported for objects of type Class or Module");
            }
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            Dictionary<RubyModule, object> visited = new Dictionary<RubyModule, object>();
            Stack<RubyModule> modules = new Stack<RubyModule>();
            modules.Push(theClass.Context.ObjectClass);
            while (modules.Count > 0) {
                RubyModule next = modules.Pop();
                RubyClass asClass = next as RubyClass;

                if (!isClass || asClass != null) {
                    object result;
                    if (block.Yield(next, out result)) {
                        return result;
                    }
                }

                using (theClass.Context.ClassHierarchyLocker()) {
                    next.EnumerateConstants(delegate(RubyModule module, string name, object value) {
                        RubyModule constAsModule = value as RubyModule;
                        if (constAsModule != null && !visited.ContainsKey(constAsModule)) {
                            modules.Push(constAsModule);
                            visited[module] = null;
                        }
                        return false;
                    });
                }
            }
            return visited.Count;
        }

        [RubyMethod("garbage_collect", RubyMethodAttributes.PublicSingleton)]
        public static void GarbageCollect(RubyModule/*!*/ self) {
            GC.Collect();
        }

        [RubyMethod("undefine_finalizer", RubyMethodAttributes.PublicSingleton)]
        public static object DefineFinalizer(RubyModule/*!*/ self, object obj) {
            return obj;
        }
    }
}
