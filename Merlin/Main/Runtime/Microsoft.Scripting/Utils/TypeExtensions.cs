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
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {

    // Extensions on System.Type and friends
    internal static class TypeExtensions {

        internal static Type GetReturnType(this MethodBase mi) {
            return (mi.IsConstructor) ? mi.DeclaringType : ((MethodInfo)mi).ReturnType;
        }

        /// <summary>
        /// A helper routine to check if a type can be treated as sealed - i.e. there
        /// can never be a subtype of this given type.  This corresponds to a type
        /// that is either declared "Sealed" or is a ValueType and thus unable to be
        /// extended.
        /// 
        /// TODO: this should not be needed. Type.IsSealed does the right thing.
        /// </summary>
        internal static bool IsSealedOrValueType(this Type type) {
            return type.IsSealed || type.IsValueType;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified parameter is mandatory, i.e. is not optional and doesn't have a default value.
        /// </summary>
        internal static bool IsMandatoryParameter(this ParameterInfo pi) {
            return (pi.Attributes & (ParameterAttributes.Optional | ParameterAttributes.HasDefault)) == 0;
        }

        internal static bool HasDefaultValue(this ParameterInfo pi) {
            return (pi.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        internal static bool IsByRefParameter(this ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            if (pi.ParameterType.IsByRef) return true;

            return (pi.Attributes & (ParameterAttributes.Out)) == ParameterAttributes.Out;
        }
    }
}
