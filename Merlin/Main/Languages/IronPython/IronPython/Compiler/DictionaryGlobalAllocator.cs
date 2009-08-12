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
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Provides globals for when we need to lookup into a dictionary for each global access.
    /// 
    /// This is the slowest form of globals and is only used when we need to run against an
    /// arbitrary dictionary given to us by a user.
    /// </summary>
    class DictionaryGlobalAllocator : GlobalAllocator {
        public DictionaryGlobalAllocator() {
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {
            PythonCompilerOptions pco = ((PythonCompilerOptions)context.Options);
            PythonContext pc = (PythonContext)context.SourceUnit.LanguageContext;

            if (body is MSAst.ConstantExpression) {
                object value = ((MSAst.ConstantExpression)body).Value;
                return new PythonScriptCode(codeCtx => value, context.SourceUnit);
            }

            var lambda = Ast.Lambda<Func<CodeContext, object>>(
                Utils.Convert(body, typeof(object)),
                pco.ModuleName ?? "<unnamed>",
                ArrayGlobalAllocator._globalContextList
            );

            Func<CodeContext, object> func;

            if (pc.ShouldInterpret(pco, context.SourceUnit)) {
                func = CompilerHelpers.LightCompile(lambda);
            } else {
                func = lambda.Compile(context.SourceUnit.EmitDebugSymbols);
            }

            return new PythonScriptCode(func, context.SourceUnit);
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get { return ArrayGlobalAllocator._globalContext; }
        }

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            return new LookupGlobalVariable(ag.LocalContext, name, isLocal);
        }
    }
}
