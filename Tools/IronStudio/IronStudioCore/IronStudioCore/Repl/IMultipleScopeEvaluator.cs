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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.IronStudio.Repl {
    /// <summary>
    /// Supports a REPL evaluator which enables the user to switch between
    /// multiple scopes of execution.
    /// </summary>
    public interface IMultipleScopeEvaluator : IReplEvaluator {
        IEnumerable<string> GetAvailableScopes();
        event EventHandler<EventArgs> AvailableScopesChanged;
        event EventHandler<EventArgs> CurrentScopeChanged;
        void SetScope(string scopeName);
        string CurrentScopeName {
            get;
        }
        bool EnableMultipleScopes {
            get;
        }
    }
}
