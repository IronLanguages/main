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

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Interpretation {

    /// <summary>
    /// An ILocalVariables implementation for the interpreter
    /// 
    /// TODO: This isn't quite correct, because it doesn't implement the
    /// LocalScopeExpression.IsClosure feature that only exposes variables that
    /// would otherwise be lifted. To implement it correctly would require a
    /// full variable binding pass, something the interpreter doesn't need
    /// today. The only thing that this breaks is Python's func_closure
    /// </summary>
    internal sealed class InterpreterVariables : IRuntimeVariables {
        private readonly InterpreterState _state;
        private readonly ReadOnlyCollection<ParameterExpression> _vars;

        internal InterpreterVariables(InterpreterState state, RuntimeVariablesExpression node) {
            _state = state;
            _vars = node.Variables;
        }

        int IRuntimeVariables.Count {
            get { return _vars.Count; }
        }

        object IRuntimeVariables.this[int index] {
            get {
                return _state.GetValue(_vars[index]);
            }
            set {
                _state.SetValue(_vars[index], value);
            }
        }
    }
}
