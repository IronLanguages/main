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

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Debugging {
    /// <summary>
    /// Implements IRuntimeVariables in a way that preserves scoping within the lambda.
    /// </summary>
    internal class ScopedRuntimeVariables : IRuntimeVariables {
        private readonly IList<VariableInfo> _variableInfos;
        private readonly IRuntimeVariables _variables;

        internal ScopedRuntimeVariables(IList<VariableInfo> variableInfos, IRuntimeVariables variables) {
            _variableInfos = variableInfos;
            _variables = variables;
        }

        #region IRuntimeVariables

        public int Count {
            get { return _variableInfos.Count; }
        }

        public object this[int index] {
            get {
                Debug.Assert(index < _variableInfos.Count);
                return _variables[_variableInfos[index].GlobalIndex];
            }
            set {
                Debug.Assert(index < _variableInfos.Count);
                _variables[_variableInfos[index].GlobalIndex] = value;
            }
        }

        #endregion
    }
}
