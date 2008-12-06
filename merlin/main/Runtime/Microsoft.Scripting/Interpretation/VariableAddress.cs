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

using System.Linq.Expressions;

namespace Microsoft.Scripting.Interpretation {
    internal sealed class VariableAddress : EvaluationAddress {
        internal VariableAddress(Expression expr)
            : base(expr) {
        }

        internal override object GetValue(InterpreterState state, bool outParam) {
            if (outParam) {
                return null;
            }

            return base.GetValue(state, outParam);
        }
    }
}
