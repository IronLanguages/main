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

namespace Microsoft.Scripting.Interpretation {
    internal sealed class CallSiteInfo {
        public int Counter { get; set; }
        public CallSite CallSite { get; set; }
        public Interpreter.MatchCallerTarget CallerTarget { get; set; }
    }
    
    public class InterpretedScriptCode : ScriptCode {
        // call sites allocated for the tree:
        private Dictionary<Expression, CallSiteInfo> _callSites;
        private readonly LambdaExpression _code;

        internal Dictionary<Expression, CallSiteInfo> CallSites {
            get {
                if (_callSites == null) {
                    Interlocked.CompareExchange(ref _callSites, new Dictionary<Expression, CallSiteInfo>(), null);
                }

                return _callSites;
            }
        }

        public override object Run() {
            return InvokeTarget(_code, CreateScope());
        }

        public override object Run(Scope scope) {
            return InvokeTarget(_code, scope);
        }

        internal bool HasCallSites {
            get { return _callSites != null; }
        }

        public InterpretedScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : base(sourceUnit) {
            _code = code;
        }

        public LambdaExpression Code {
            get { return _code; }
        }

        protected object InvokeTarget(LambdaExpression code, Scope scope) {
            return Interpreter.TopLevelExecute(this, scope, LanguageContext);
        }
    }
}
