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

using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    /// <summary>
    /// Represents a script code which can be dynamically bound to execute against
    /// arbitrary Scope objects.  This is used for code when the user runs against
    /// a particular scope as well as for exec and eval code as well.
    /// </summary>
    class PythonScriptCode : ScriptCode {
        private readonly Func<CodeContext/*!*/, object>/*!*/ _target;

        public PythonScriptCode(Func<CodeContext/*!*/, object>/*!*/ target, SourceUnit/*!*/ sourceUnit)
            : base(sourceUnit) {
            Assert.NotNull(target);

            _target = target;
        }

        public override object Run() {
            if (SourceUnit.Kind == SourceCodeKind.Expression) {
                return EvalWrapper(new Scope());
            }
            return _target(PythonOps.CreateTopLevelCodeContext(new Scope(), SourceUnit.LanguageContext));
        }

        public override object Run(Scope scope) {
            if (SourceUnit.Kind == SourceCodeKind.Expression) {
                return EvalWrapper(scope);
            }

            return _target(PythonOps.CreateTopLevelCodeContext(scope, SourceUnit.LanguageContext));
        }

        public override Scope/*!*/ CreateScope() {
            return new Scope();
        }

        // wrapper so we can do minimal code gen for eval code
        private object EvalWrapper(Scope scope) {
            try {
                return _target(PythonOps.CreateTopLevelCodeContext(scope, SourceUnit.LanguageContext));
            } catch (Exception) {
                PythonOps.UpdateStackTrace(new CodeContext(scope, (PythonContext)SourceUnit.LanguageContext), _target.Method, "<module>", "<string>", 0);
                throw;
            }
        }
    }
}
