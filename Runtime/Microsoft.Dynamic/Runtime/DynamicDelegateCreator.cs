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
using System.Dynamic;
using System.Reflection;

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

        /// <summary> Table of dynamically generated delegates which are shared based upon method signature. </summary>
        private Publisher<DelegateSignatureInfo, DelegateInfo> _dynamicDelegateCache = new Publisher<DelegateSignatureInfo, DelegateInfo>();

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

                ParameterInfo[] parameters = invoke.GetParameters();
                DelegateSignatureInfo signatureInfo = new DelegateSignatureInfo(
                    invoke.ReturnType,
                    parameters
                );

                DelegateInfo delegateInfo = _dynamicDelegateCache.GetOrCreateValue(signatureInfo,
                    delegate() {
                        // creation code
                        return signatureInfo.GenerateDelegateStub(_languageContext);
                    });


                result = delegateInfo.CreateDelegate(delegateType, dynamicObject);
                if (result != null) {
                    return result;
                }
            }

            throw ScriptingRuntimeHelpers.SimpleTypeError("Object is not callable.");
        }

    }
}
