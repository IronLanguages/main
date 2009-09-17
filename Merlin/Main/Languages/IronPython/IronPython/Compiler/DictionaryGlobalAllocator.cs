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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    /// <summary>
    /// Provides globals for when we need to lookup into a dictionary for each global access.
    /// 
    /// This is the slowest form of globals and is only used when we need to run against an
    /// arbitrary dictionary given to us by a user.
    /// </summary>
    class DictionaryGlobalAllocator : GlobalAllocator {
        public DictionaryGlobalAllocator() {
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast, Dictionary<int, bool> handlerLocations, Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations) {
            PythonCompilerOptions pco = ((PythonCompilerOptions)context.Options);
            PythonContext pc = (PythonContext)context.SourceUnit.LanguageContext;

            var lambda = Ast.Lambda<Func<CodeContext, FunctionCode, object>>(
                Utils.Convert(body, typeof(object)),
                pco.ModuleName ?? "<unnamed>",
                ArrayGlobalAllocator._arrayFuncParams
            );

            return new PythonScriptCode(context, lambda, context.SourceUnit, handlerLocations, loopAndFinallyLocations);
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get { return ArrayGlobalAllocator._globalContext; }
        }

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            return new LookupGlobalVariable(ag.LocalContext, name, isLocal);
        }
    }
}
