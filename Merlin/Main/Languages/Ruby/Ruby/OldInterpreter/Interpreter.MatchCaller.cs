/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Interpretation {
    internal static partial class Interpreter {
        internal delegate object MatchCallerTarget(CallSite site, object[] args);

        /// <summary>
        /// MatchCaller allows to call match maker delegate with the signature (object, CallSite, object[])
        /// It is used by the call site cache lookup logic when searching for applicable rule.
        /// </summary>
        internal static class MatchCaller {
            public static object Target0(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object>>)site).Target(site);
            }

            public static object Target1(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object>>)site).Target(site, args[0]);
            }

            public static object Target2(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object>>)site).Target(site, args[0], args[1]);
            }

            public static object Target3(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object>>)site).Target(site, args[0], args[1], args[2]);
            }

            public static object Target4(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3]);
            }

            public static object Target5(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3], args[4]);
            }

            public static object Target6(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3], args[4], args[5]);
            }

            public static object Target7(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            }

            public static object Target8(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            }

            public static object Target9(CallSite site, object[] args) {
                return ((CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>)site).Target(site, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            }

            private struct RefFixer {
                internal readonly LocalBuilder Temp;
                internal readonly int Index;

                internal RefFixer(LocalBuilder temp, int index) {
                    Temp = temp;
                    Index = index;
                }
            }

            // TODO: Should this really be Type -> WeakReference?
            // Issue #1, we'll end up growing the dictionary for each unique type
            // Issue #2, we'll lose the generated delegate in the first gen-0
            // collection.
            //
            // We probably need to replace this with an actual cache that holds
            // onto the delegates and ages them out.
            //
            private static readonly Dictionary<Type, MatchCallerTarget> _Callers = new Dictionary<Type, MatchCallerTarget>();
            private static readonly Type[] _CallerSignature = new Type[] { typeof(CallSite), typeof(object[]) };

            internal static MatchCallerTarget GetCaller(Type type) {
                MatchCallerTarget target;

                // LOCK to extract the weak reference with the updater DynamicMethod 
                lock (_Callers) {
                    if (!_Callers.TryGetValue(type, out target)) {
                        target = (MatchCallerTarget)CreateCaller(type);
                        _Callers[type] = target;
                    }
                }

                return target;
            }

            private static int _id;

            /// <summary>
            /// Uses LCG to create method such as this:
            /// 
            /// object MatchCaller(object target, CallSite site, object[] args) {
            ///      return ((ActualDelegateType)target)(site, args[0], args[1], args[2], ...);
            /// }
            /// 
            /// inserting appropriate casts and boxings as needed.
            /// </summary>
            /// <param name="type">Type of the delegate to call</param>
            /// <returns>A MatchCallerTarget delegate.</returns>
            private static object CreateCaller(Type type) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Interpreter.MatchCaller.CreateCaller");
                
                MethodInfo invoke = type.GetMethod("Invoke");
                ParameterInfo[] parameters = invoke.GetParameters();

                var il = Snippets.Shared.CreateDynamicMethod("_istub_" + Interlocked.Increment(ref _id), typeof(object), _CallerSignature, false);
                Type siteType = typeof(CallSite<>).MakeGenericType(type);

                List<RefFixer> fixers = null;

                // Emit the call site and cast it to the right type
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, siteType);
                il.Emit(OpCodes.Ldfld, siteType.GetField("Target"));

                // CallSite
                il.Emit(OpCodes.Ldarg_0);

                // Arguments
                for (int i = 1; i < parameters.Length; i++) {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldc_I4, i - 1);
                    il.Emit(OpCodes.Ldelem_Ref);
                    Type pt = parameters[i].ParameterType;
                    if (pt.IsByRef) {
                        RefFixer rf = new RefFixer(il.DeclareLocal(pt.GetElementType()), i - 1);
                        if (rf.Temp.LocalType.IsValueType) {
                            il.Emit(OpCodes.Unbox_Any, rf.Temp.LocalType);
                        } else if (rf.Temp.LocalType != typeof(object)) {
                            il.Emit(OpCodes.Castclass, rf.Temp.LocalType);
                        }
                        il.Emit(OpCodes.Stloc, rf.Temp);
                        il.Emit(OpCodes.Ldloca, rf.Temp);

                        if (fixers == null) {
                            fixers = new List<RefFixer>();
                        }
                        fixers.Add(rf);
                    } else if (pt.IsValueType) {
                        il.Emit(OpCodes.Unbox_Any, pt);
                    } else if (pt != typeof(object)) {
                        il.Emit(OpCodes.Castclass, pt);
                    }
                }

                // Call the delegate
                il.Emit(OpCodes.Callvirt, invoke);

                // Propagate the ref parameters back into the array.
                if (fixers != null) {
                    foreach (RefFixer rf in fixers) {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldc_I4, rf.Index);
                        il.Emit(OpCodes.Ldloc, rf.Temp);
                        if (rf.Temp.LocalType.IsValueType) {
                            il.Emit(OpCodes.Box, rf.Temp.LocalType);
                        }
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }

                // Return value
                if (invoke.ReturnType == typeof(void)) {
                    il.Emit(OpCodes.Ldnull);
                } else if (invoke.ReturnType.IsValueType) {
                    il.Emit(OpCodes.Box, invoke.ReturnType);
                }

                il.Emit(OpCodes.Ret);
                return il.CreateDelegate<MatchCallerTarget>();
            }
        }
    }
}
