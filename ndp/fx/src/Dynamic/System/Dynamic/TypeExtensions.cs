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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace System.Dynamic {

    // Extensions on System.Type and friends
    internal static class TypeExtensions {

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target) {
            Debug.Assert(methodInfo != null && delegateType != null);

            var dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        // Warning: This can be slower than you might expect due to the generic type argument & static method
        internal static T CreateDelegate<T>(this MethodInfo methodInfo, object target) {
            return (T)(object)methodInfo.CreateDelegate(typeof(T), target);
        }
    }
}
