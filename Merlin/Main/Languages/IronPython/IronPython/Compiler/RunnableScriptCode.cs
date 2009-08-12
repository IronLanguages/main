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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using IronPython.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {    
    abstract class RunnableScriptCode : ScriptCode {
        private FunctionCode _code;

        public RunnableScriptCode(SourceUnit sourceUnit)
            : base(sourceUnit) {
        }

        public override object Run() {
            return base.Run();
        }

        public override object Run(Scope scope) {
            throw new NotImplementedException();
        }

        protected static CodeContext/*!*/ CreateTopLevelCodeContext(Scope/*!*/ scope, LanguageContext/*!*/ context) {
            context.EnsureScopeExtension(CodeContext.GetModuleScope(scope));
            return new CodeContext(scope, (PythonContext)context);
        }

        protected void EnsureFunctionCode(Delegate dlg) {
            if (_code == null) {
                _code = new FunctionCode(
                    (PythonContext)SourceUnit.LanguageContext,
                    dlg,
                    null,
                    "<module>",
                    "",
                    ArrayUtils.EmptyStrings,
                    FunctionAttributes.None,
                    SourceSpan.None,
                    SourceUnit.Path,
                    false,
                    true,
                    new SymbolId[0],
                    null,
                    null
                );
            }
        }

        protected void PushFrame(CodeContext context, Delegate code) {
            if (((PythonContext)SourceUnit.LanguageContext).PythonOptions.Frames) {
                EnsureFunctionCode(code);
                PythonOps.PushFrame(context, _code);
            }
        }

        protected void PopFrame() {
            if (((PythonContext)SourceUnit.LanguageContext).PythonOptions.Frames) {
                List<FunctionStack> stack = PythonOps.GetFunctionStack();
                stack.RemoveAt(stack.Count - 1);
            }
        }
    }
}
