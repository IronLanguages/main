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

using System.Reflection;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {

    public static class BinderHelpers {

        public static bool IsParamsMethod(MethodBase method) {
            return IsParamsMethod(method.GetParameters());
        }

        public static bool IsParamsMethod(ParameterInfo[] pis) {
            foreach (ParameterInfo pi in pis) {
                if (CompilerHelpers.IsParamArray(pi) || IsParamDictionary(pi)) return true;
            }
            return false;
        }

        public static bool IsParamDictionary(ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamDictionaryAttribute), false);
        }

    }
}
