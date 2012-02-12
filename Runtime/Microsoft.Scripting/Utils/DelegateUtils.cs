/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Microsoft.Scripting.Utils {
    internal static class DelegateUtils {
#if FEATURE_REFEMIT
        private static AssemblyBuilder _assembly;
        private static ModuleBuilder _modBuilder;
        private static int _typeCount;
        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        public const char GenericArityDelimiter = '`';

        private static TypeBuilder DefineDelegateType(string name) {
            if (_assembly == null) {
                AssemblyBuilder newAssembly;
#if WIN8
                newAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicDelegates"), AssemblyBuilderAccess.Run);
#else
                newAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicDelegates"), AssemblyBuilderAccess.Run);
#endif
                Interlocked.CompareExchange(ref _assembly, newAssembly, null);

                lock (_assembly) {
                    if (_modBuilder == null) {
                        _modBuilder = _assembly.DefineDynamicModule("DynamicDelegates");
                    }
                }
            }

            return _modBuilder.DefineType(
                name + Interlocked.Increment(ref _typeCount),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,                
                typeof(MulticastDelegate)
            );
        }
#endif
        /// <summary>
        /// Gets a Func of CallSite, object * paramCnt, object delegate type
        /// that's suitable for use in a non-strongly typed call site.
        /// </summary>
        public static Type GetObjectCallSiteDelegateType(int paramCnt) {
            switch (paramCnt) {
                case 0: return typeof(Func<CallSite, object, object>);
                case 1: return typeof(Func<CallSite, object, object, object>);
                case 2: return typeof(Func<CallSite, object, object, object, object>);
                case 3: return typeof(Func<CallSite, object, object, object, object, object>);
                case 4: return typeof(Func<CallSite, object, object, object, object, object, object>);
                case 5: return typeof(Func<CallSite, object, object, object, object, object, object, object>);
                case 6: return typeof(Func<CallSite, object, object, object, object, object, object, object, object>);
                case 7: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object>);
                case 8: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object>);
                case 9: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>);
                case 10: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 11: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 12: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 13: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 14: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                default:
#if FEATURE_REFEMIT
                    Type[] paramTypes = new Type[paramCnt + 2];
                    
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[1] = typeof(object);
                    for (int i = 0; i < paramCnt; i++) {
                        paramTypes[i + 2] = typeof(object);
                    }
                    TypeBuilder tb = DefineDelegateType("Delegate");
                    tb.DefineConstructor(
                        MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                        CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
                    tb.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, typeof(object), paramTypes).SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
                    return tb.CreateType();
#else
                    throw new NotSupportedException();
#endif
            }
        }
    }
}
