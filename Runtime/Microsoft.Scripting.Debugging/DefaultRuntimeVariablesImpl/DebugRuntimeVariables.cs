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

using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Debugging {
    /// <summary>
    /// Implementation of IDebugRuntimeVariables, which wraps IRuntimeVariables + FunctionInfo/DebugMarker
    /// </summary>
    internal class DebugRuntimeVariables : IDebugRuntimeVariables {
        private readonly IRuntimeVariables _runtimeVariables;

        internal DebugRuntimeVariables(IRuntimeVariables runtimeVariables) {
            _runtimeVariables = runtimeVariables;
        }

        #region IRuntimeVariables

        public int Count {
            get { return _runtimeVariables.Count - 2; }
        }

        public object this[int index] {
            get { return _runtimeVariables[2 + index]; }
            set { _runtimeVariables[2 + index] = value; }
        }

        #endregion

        #region IDebugRuntimeVariables

        public FunctionInfo FunctionInfo {
            get { return (FunctionInfo)_runtimeVariables[0]; }
        }

        public int DebugMarker {
            get { return (int)_runtimeVariables[1]; }
        }

        #endregion
    }
}
