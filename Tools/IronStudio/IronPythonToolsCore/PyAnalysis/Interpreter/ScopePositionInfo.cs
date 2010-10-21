/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    /// <summary>
    /// Holds information which allows us to quickly discover the relevant 
    /// scopes for a given piece of code.  This includes the linear start & stop
    /// position, the associated scope with that position, and any children.
    /// </summary>
    internal class ScopePositionInfo {
        public readonly int Start;
        public readonly int Stop;
        public readonly InterpreterScope Scope;
        public readonly List<ScopePositionInfo> Children;

        public ScopePositionInfo(int start, int stop, InterpreterScope scope) {
            Start = start;
            Stop = stop;
            Scope = scope;
            Children = new List<ScopePositionInfo>();
        }
    }
}
