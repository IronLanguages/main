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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Provides support for converting objects to delegates using the DLR binders
    /// available by the provided language context.
    /// 
    /// Primarily this supports converting objects implementing IDynamicMetaObjectProvider
    /// to the appropriate delegate type.  
    /// 
    /// If the provided object is already a delegate of the appropriate type then the 
    /// delegate will simply be returned.
    /// </summary>
    public class DynamicDelegateCreator {
        private readonly LanguageContext _languageContext;

        public DynamicDelegateCreator(LanguageContext languageContext) {
            ContractUtils.RequiresNotNull(languageContext, "languageContext");

            _languageContext = languageContext;
        }

        /// <summary>
        /// Creates a delegate with a given signature that could be used to invoke this object from non-dynamic code (w/o code context).
        /// A stub is created that makes appropriate conversions/boxing and calls the object.
        /// The stub should be executed within a context of this object's language.
        /// </summary>
        /// <returns>The converted delegate.</returns>
        /// <exception cref="T:Microsoft.Scripting.ArgumentTypeException">The object is either a subclass of Delegate but not the requested type or does not implement IDynamicMetaObjectProvider.</exception>
        public Delegate GetDelegate(object callableObject, Type delegateType) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            Delegate result = callableObject as Delegate;
            if (result != null) {
                if (!delegateType.IsAssignableFrom(result.GetType())) {
                    throw ScriptingRuntimeHelpers.SimpleTypeError(String.Format("Cannot cast {0} to {1}.", result.GetType(), delegateType));
                }

                return result;
            }

            IDynamicMetaObjectProvider dynamicObject = callableObject as IDynamicMetaObjectProvider;
            if (dynamicObject != null) {

                MethodInfo invoke;

                if (!typeof(Delegate).IsAssignableFrom(delegateType) || (invoke = delegateType.GetMethod("Invoke")) == null) {
                    throw ScriptingRuntimeHelpers.SimpleTypeError("A specific delegate type is required.");
                }

                result = GetOrCreateDelegateForDynamicObject(callableObject, delegateType, invoke);
                if (result != null) {
                    return result;
                }
            }

            throw ScriptingRuntimeHelpers.SimpleTypeError("Object is not callable.");
        }

#if FEATURE_LCG
        // Table of dynamically generated delegates which are shared based upon method signature. 
        //
        // We generate a dynamic method stub and object[] closure template for each signature.
        // The stub does only depend on the signature, it doesn't depend on the dynamic object.
        // So we can reuse these stubs among multiple dynamic object for which a delegate was created with the same signature.
        // 
        private Publisher<DelegateSignatureInfo, DelegateInfo> _dynamicDelegateCache = new Publisher<DelegateSignatureInfo, DelegateInfo>();

        public Delegate GetOrCreateDelegateForDynamicObject(object dynamicObject, Type delegateType, MethodInfo invoke) {
            var signatureInfo = new DelegateSignatureInfo(invoke);
            DelegateInfo delegateInfo = _dynamicDelegateCache.GetOrCreateValue(
                signatureInfo, 
                () => new DelegateInfo(_languageContext, signatureInfo.ReturnType, signatureInfo.ParameterTypes)
            );

            return delegateInfo.CreateDelegate(delegateType, dynamicObject);
        }
#else
        //
        // Using Expression Trees we create a new stub for every dynamic object and every delegate type.
        // This is less efficient than with LCG since we can't reuse generated code for multiple dynamic objects and signatures.
        //
        private static ConditionalWeakTable<object, Dictionary<Type, Delegate>> _dynamicDelegateCache =
            new ConditionalWeakTable<object, Dictionary<Type, Delegate>>();

        private Delegate GetOrCreateDelegateForDynamicObject(object dynamicObject, Type delegateType, MethodInfo invoke) {
            var signatures = _dynamicDelegateCache.GetOrCreateValue(dynamicObject);
            lock (signatures) {
                Delegate result;
                if (!signatures.TryGetValue(delegateType, out result)) {
                    result = DelegateInfo.CreateDelegateForDynamicObject(_languageContext, dynamicObject, delegateType, invoke);
                    signatures.Add(delegateType, result);
                }

                return result;
            }
        }
#endif
    }
}