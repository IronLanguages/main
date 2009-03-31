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
        private MSAst.ParameterExpression/*!*/ _globalScope, _language, _globalCtx;

        public DictionaryGlobalAllocator() {            
            _globalScope = Ast.Parameter(typeof(Scope), "$scope");
            _language = Ast.Parameter(typeof(LanguageContext), "$language");
            _globalCtx = ArrayGlobalAllocator._globalContext;
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {            
            MSAst.Expression finalBody = Ast.Block(
                new[] { _globalCtx },
                Ast.Assign(
                    _globalCtx,
                    Ast.Call(typeof(PythonOps).GetMethod("CreateTopLevelCodeContext"),
                        _globalScope,
                        _language
                    )
                ),
                body
            );

            PythonCompilerOptions pco = ((PythonCompilerOptions)context.Options);
            string name = pco.ModuleName ?? "<unnamed>";
            var lambda = Ast.Lambda<Func<Scope, LanguageContext, object>>(
                finalBody, 
                name,
                new[] { _globalScope, _language } 
            );


            Func<Scope, LanguageContext, object> func;
            // TODO: adaptive compilation should be eanbled
            /*PythonContext pc = (PythonContext)context.SourceUnit.LanguageContext;
            if (pc.ShouldInterpret(pco, context.SourceUnit)) {
                func = CompilerHelpers.LightCompile(lambda);
            } else*/ {
                func = lambda.Compile(context.SourceUnit.EmitDebugSymbols);
            }

            return new PythonScriptCode(func, context.SourceUnit);
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get { return _globalCtx; }
        }

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            return new LookupGlobalVariable(Ast.Property(ag.LocalContext, "Scope"), name, isLocal);
        }
    }
}
