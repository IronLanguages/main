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
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Runtime.CompilerServices;

namespace IronRuby.Builtins {
    [RubyModule("ObjectSpace")]
    public static class ObjectSpace {
        #region define_finalizer, undefine_finalizer

        private sealed class FinalizerInvoker {
            // TODO: make this variable invisible to users
            public const string InstanceVariableName = "<FINALIZER>";

            private CallSite<Func<CallSite, object, object, object>> _callSite;
            private object _finalizer;

            public FinalizerInvoker(CallSite<Func<CallSite, object, object, object>>/*!*/ callSite, object finalizer) {
                Assert.NotNull(callSite);
                _callSite = callSite;
                _finalizer = finalizer;
            }

            ~FinalizerInvoker() {
                if (_callSite != null) {
                    try {
                        _callSite.Target(_callSite, _finalizer, 0);
                    } catch (Exception e) {
                        // nop
                        Utils.Log("An exception has been thrown from finalizer: " + e, "OS:FINALIZER");
                    }
                }
            }
        }

        [RubyMethod("define_finalizer", RubyMethodAttributes.PublicSingleton)]
        public static object DefineFinalizer(RespondToStorage/*!*/ respondTo, BinaryOpStorage/*!*/ call, RubyModule/*!*/ self, object obj, object finalizer) {
            if (!Protocols.RespondTo(respondTo, finalizer, "call")) {
                throw RubyExceptions.CreateArgumentError("finalizer should be callable (respond to :call)");
            }

            respondTo.Context.SetInstanceVariable(obj, FinalizerInvoker.InstanceVariableName, new FinalizerInvoker(call.GetCallSite("call"), finalizer));
            RubyArray result = new RubyArray(2);
            result.Add(0);
            result.Add(finalizer);
            return result;
        }

        [RubyMethod("undefine_finalizer", RubyMethodAttributes.PublicSingleton)]
        public static object UndefineFinalizer(RubyContext/*!*/ context, RubyModule/*!*/ self, object obj) {
            object invokerObj;
            if (context.TryRemoveInstanceVariable(obj, FinalizerInvoker.InstanceVariableName, out invokerObj)) {
                GC.SuppressFinalize(invokerObj);
            }
            return obj;
        }

        #endregion

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
    }
}
