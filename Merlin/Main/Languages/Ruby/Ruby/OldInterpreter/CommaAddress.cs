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

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Interpretation {
    internal class CommaAddress : EvaluationAddress {
        private List<EvaluationAddress> _addrs;

        internal CommaAddress(BlockExpression address, List<EvaluationAddress> addresses)
            : base(address) {
            _addrs = addresses;
        }

        internal override object GetValue(InterpreterState state, bool outParam) {
            object result = null;
            for (int i = 0; i < _addrs.Count; i++) {
                EvaluationAddress current = _addrs[i];

                if (current != null) {
                    object val = current.GetValue(state, outParam);
                    if (i == Index) {
                        result = val;
                    }
                }
            }
            return result;
        }

        internal override object AssignValue(InterpreterState state, object value) {
            EvaluationAddress addr = _addrs[Index];
            if (addr != null) return addr.AssignValue(state, value);
            return null;
        }

        internal int Index {
            get {
                return ((BlockExpression)Expression).Expressions.Count - 1;
            }
        }
    }
}
