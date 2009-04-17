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
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace Microsoft.Scripting.Interpretation {
    internal sealed class CallSiteInfo {
        public int Counter { get; set; }
        public CallSite CallSite { get; set; }
        public Interpreter.MatchCallerTarget CallerTarget { get; set; }
    }

    internal interface IInterpretedScriptCode {
        LambdaExpression Code { get; }
        Dictionary<Expression, CallSiteInfo> CallSites { get; }
        SourceUnit SourceUnit { get; } 
    }
}
