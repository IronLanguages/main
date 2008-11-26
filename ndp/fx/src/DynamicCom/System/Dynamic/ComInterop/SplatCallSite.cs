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

using System.Diagnostics;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic.Binders;
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Dynamic.ComInterop {

    // TODO: this entire class should go away.
    // Instead we should be using a dynamic convert action to get a delegate
    internal sealed partial class SplatCallSite {
        internal delegate object SplatCaller(object[] args);

        // TODO: Should free these eventually
        private readonly SynchronizedDictionary<int, SplatCaller> _callers = new SynchronizedDictionary<int, SplatCaller>();
        private readonly CallSiteBinder _binder;

        public SplatCallSite(CallSiteBinder binder) {
            _binder = binder;
        }

        public object Invoke(object[] args) {
            Debug.Assert(args != null);

            SplatCaller caller;
            if (!_callers.TryGetValue(args.Length, out caller)) {
                _callers[args.Length] = caller = MakeCaller(args.Length);
            }

            return caller(args);
        }
        
        private SplatCaller MakeCaller(int args) {
            MethodInfo mi = GetType().GetMethod("CallHelper" + args);
            if (mi != null) {
                Type delegateType = mi.GetParameters()[0].ParameterType.GetGenericArguments()[0];
                CallSite site = CallSite.Create(delegateType, _binder);
                return (SplatCaller)Delegate.CreateDelegate(typeof(SplatCaller), site, mi);
            }
            return MakeBigCaller(args);
        }

        /// <summary>
        /// Uses LCG to create method such as this:
        /// 
        /// object SplatCaller(CallSite{T} site, object[] args) {
        ///      return site.Target(site, args[0], args[1], args[2], ...);
        /// }
        /// 
        /// where the CallSite is bound to the delegate
        /// </summary>
        /// <param name="args">the number of arguments</param>
        /// <returns>a SplatCaller delegate.</returns>
        private SplatCaller MakeBigCaller(int args) {
            // Get the dynamic site type
            var siteDelegateTypeArgs = new Type[args + 2];
            siteDelegateTypeArgs[0] = typeof(CallSite);
            for (int i = 1, n = siteDelegateTypeArgs.Length; i < n;  i++) {
                siteDelegateTypeArgs[i] = typeof(object);
            }
            Type siteDelegateType = Expression.GetDelegateType(siteDelegateTypeArgs);

            // Create the callsite and get its type
            CallSite callSite = CallSite.Create(siteDelegateType, _binder);
            Type siteType = callSite.GetType();

            var method = new DynamicMethod("_stub_SplatCaller", typeof(object), new Type[] { siteType, typeof(object[]) }, true);
            var gen = method.GetILGenerator();
            
            // Emit the site's target
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, siteType.GetField("Target"));

            // Emit the site
            gen.Emit(OpCodes.Ldarg_0);

            // Emit the arguments
            for (int i = 0; i < args; i++) {
                gen.Emit(OpCodes.Ldarg_1);
                gen.EmitInt(i);
                gen.Emit(OpCodes.Ldelem_Ref);
            }
            
            // Invoke the target
            gen.Emit(OpCodes.Callvirt, siteDelegateType.GetMethod("Invoke"));
            gen.Emit(OpCodes.Ret);

            // Create the delegate
            return method.CreateDelegate<SplatCaller>(callSite);
        }
    }
}
