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
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the key for the LanguageContext.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateSignatureInfo {
        private readonly Type _returnType;
        private readonly ParameterInfo[] _parameters;

        internal DelegateSignatureInfo(Type returnType, ParameterInfo[] parameters) {
            Assert.NotNull(returnType);
            Assert.NotNullItems(parameters);

            _parameters = parameters;
            _returnType = returnType;
        }

        [Confined]
        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;

            if (dsi == null || 
                dsi._parameters.Length != _parameters.Length ||
                dsi._returnType != _returnType) {
                return false;
            }

            for (int i = 0; i < _parameters.Length; i++) {
                if (dsi._parameters[i] != _parameters[i]) {
                    return false;
                }
            }

            return true;
        }

        [Confined]
        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < _parameters.Length; i++) {
                hashCode ^= _parameters[i].GetHashCode();
            }
            hashCode ^= _returnType.GetHashCode();
            return hashCode;
        }

        [Confined]
        public override string ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(_returnType.ToString());
            text.Append("(");
            for (int i = 0; i < _parameters.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(_parameters[i].ParameterType.Name);
            }
            text.Append(")");
            return text.ToString();
        }

        internal DelegateInfo GenerateDelegateStub(LanguageContext context) {
            return new DelegateInfo(context, _returnType, _parameters);
        }
    }
}
