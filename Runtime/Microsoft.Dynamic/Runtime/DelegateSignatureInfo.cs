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
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the key for the LanguageContext.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateSignatureInfo {
        private readonly Type _returnType;
        private readonly Type[] _parameterTypes;

        internal DelegateSignatureInfo(MethodInfo invoke) {
            Assert.NotNull(invoke);

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            _parameterTypes = parameterTypes;
            _returnType = invoke.ReturnType;
        }

        internal Type ReturnType { get { return _returnType; } }
        internal Type[] ParameterTypes { get { return _parameterTypes; } }

        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;

            if (dsi == null ||
                dsi._parameterTypes.Length != _parameterTypes.Length ||
                dsi._returnType != _returnType) {
                return false;
            }

            for (int i = 0; i < _parameterTypes.Length; i++) {
                if (dsi._parameterTypes[i] != _parameterTypes[i]) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < _parameterTypes.Length; i++) {
                hashCode ^= _parameterTypes[i].GetHashCode();
            }
            hashCode ^= _returnType.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(_returnType.ToString());
            text.Append("(");
            for (int i = 0; i < _parameterTypes.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(_parameterTypes[i].Name);
            }
            text.Append(")");
            return text.ToString();
        }
    }
}