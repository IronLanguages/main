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
    internal class EvaluationAddress {
        private readonly Expression _expr;

        internal EvaluationAddress(Expression expression) {
            _expr = expression;
        }

        internal virtual object GetValue(InterpreterState state, bool outParam) {
            return Interpreter.Evaluate(state, _expr);
        }

        internal virtual object AssignValue(InterpreterState state, object value) {
            return Interpreter.EvaluateAssign(state, _expr, value);
        }

        internal Expression Expression {
            get {
                return _expr;
            }
        }
    }
}
