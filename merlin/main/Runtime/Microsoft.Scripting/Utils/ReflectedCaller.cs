/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Utils {
    public partial class ReflectedCaller {
        internal ReflectedCaller() { }
        private static readonly Dictionary<MethodInfo, ReflectedCaller> _cache = new Dictionary<MethodInfo,ReflectedCaller>();

        /// <summary>
        /// Creates a new ReflectedCaller which can be used to quickly invoke the provided MethodInfo.
        /// </summary>
        public static ReflectedCaller Create(MethodInfo info) {
            if ((!info.IsStatic && info.DeclaringType.IsValueType) || 
                info is System.Reflection.Emit.DynamicMethod) {
                return new SlowReflectedCaller(info);
            }            

            ParameterInfo[] pis = info.GetParameters();
            int argCnt = pis.Length;
            if (!info.IsStatic) argCnt++;
            if (argCnt >= MaxHelpers) {
                // no delegate for this size, fallback to reflection invoke
                return new SlowReflectedCaller(info);
            }

            foreach (ParameterInfo pi in pis) {
                if (pi.ParameterType.IsByRef) {
                    // we don't support ref args via generics.
                    return new SlowReflectedCaller(info);
                }
            }

            // see if we've created one w/ a delegate
            ReflectedCaller res;
            if (ShouldCache(info)) {
                lock (_cache) {
                    if (_cache.TryGetValue(info, out res)) {
                        return res;
                    }
                }
            }

            // create it 
            try {
                if (argCnt < MaxArgs) {
                    res = FastCreate(info, pis);
                } else {
                    res = SlowCreate(info, pis);
                }
            } catch (TargetInvocationException tie) {
                if (!(tie.InnerException is NotSupportedException)) {
                    throw;
                }

                res = new SlowReflectedCaller(info);
            } catch (NotSupportedException) {
                // if Delegate.CreateDelegate can't handle the method fallback to 
                // the slow reflection version.  For example this can happen w/ 
                // a generic method defined on an interface and implemented on a class.
                res = new SlowReflectedCaller(info);
            }

            // cache it for future users if it's a reasonable method to cache
            if (ShouldCache(info)) {
                lock (_cache) {
                    _cache[info] = res;
                }
            }

            return res;            
        }

        private static bool ShouldCache(MethodInfo info) {            
            return !(info is DynamicMethod);
        }
               
        /// <summary>
        /// Gets the next type or null if no more types are available.
        /// </summary>
        private static Type TryGetParameterOrReturnType(MethodInfo target, ParameterInfo[] pi, int index) {
            if (!target.IsStatic) {
                index--;
                if (index < 0) {
                    return target.DeclaringType;
                }
            }

            if (index < pi.Length) {
                // next in signature
                return pi[index].ParameterType;
            }

            if (target.ReturnType == typeof(void) || index > pi.Length) {
                // no more parameters
                return null;
            }

            // last parameter on Invoke is return type
            return target.ReturnType;
        }

        private static bool IndexIsNotReturnType(int index, MethodInfo target, ParameterInfo[] pi) {
            return pi.Length != index || (pi.Length == index && !target.IsStatic);
        }

        /// <summary>
        /// Uses reflection to create new instance of the appropriate ReflectedCaller
        /// </summary>
        private static ReflectedCaller SlowCreate(MethodInfo info, ParameterInfo[] pis) {
            List<Type> types = new List<Type>();
            if (!info.IsStatic) types.Add(info.DeclaringType);
            foreach (ParameterInfo pi in pis) {
                types.Add(pi.ParameterType);
            }
            if (info.ReturnType != typeof(void)) {
                types.Add(info.ReturnType);
            }
            Type[] arrTypes = types.ToArray();

            return (ReflectedCaller)Activator.CreateInstance(GetHelperType(info, arrTypes), info);
        }
    }

    sealed partial class SlowReflectedCaller : ReflectedCaller {
        private MethodInfo _target;

        public SlowReflectedCaller(MethodInfo target) {
            _target = target;
        }
        
        public override object Invoke(params object[] args) {
            return InvokeWorker(args);
        }        
       
        public override object InvokeInstance(object instance, params object[] args) {
            if (_target.IsStatic) {
                try {
                    return _target.Invoke(null, args);
                } catch (TargetInvocationException e) {
                    throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                }
            }

            try {
                return _target.Invoke(instance, args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private object InvokeWorker(params object[] args) {
            if (_target.IsStatic) {
                try {
                    return _target.Invoke(null, args);
                } catch (TargetInvocationException e) {
                    throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                }
            }

            try {
                return _target.Invoke(args[0], GetNonStaticArgs(args));
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object[] GetNonStaticArgs(object[] args) {
            object[] newArgs = new object[args.Length - 1];
            for (int i = 0; i < newArgs.Length; i++) {
                newArgs[i] = args[i + 1];
            }
            return newArgs;
        }
    }
}
