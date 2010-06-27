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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Contracts;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope. The code can be re-executed multiple times in different
    /// scopes. Hosting API counterpart for this class is <c>CompiledCode</c>.
    /// </summary>
    public abstract class ScriptCode {
        private readonly SourceUnit _sourceUnit;

        protected ScriptCode(SourceUnit sourceUnit) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            _sourceUnit = sourceUnit;
        }

        public LanguageContext LanguageContext {
            get { return _sourceUnit.LanguageContext; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public virtual Scope CreateScope() {
            return new Scope();
        }

        public virtual object Run() {
            return Run(CreateScope());
        }

        public abstract object Run(Scope scope);

        [Confined]
        public override string ToString() {
            return String.Format("ScriptCode '{0}' from {1}", SourceUnit.Path, LanguageContext.GetType().Name);
        }
    }
}
