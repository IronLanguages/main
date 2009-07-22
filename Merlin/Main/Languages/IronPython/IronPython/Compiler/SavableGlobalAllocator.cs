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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    class SavableGlobalAllocator : ArrayGlobalAllocator {
        private readonly List<MSAst.Expression/*!*/>/*!*/ _constants;

        public SavableGlobalAllocator(PythonContext/*!*/ context)
            : base(context) {
            _constants = new List<MSAst.Expression>();
        }

        public override System.Linq.Expressions.Expression GetConstant(object value) {
            return Utils.Constant(value);
        }

        public override System.Linq.Expressions.Expression[] PrepareScope(AstGenerator gen) {
            gen.AddHiddenVariable(GlobalArray);
            return new MSAst.Expression[] {
                Ast.Assign(
                    GlobalArray, 
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"),
                        ArrayGlobalAllocator._globalContext
                    )
                )
            };
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {
            MSAst.ParameterExpression scope = Ast.Parameter(typeof(Scope), "$scope");
            MSAst.ParameterExpression language = Ast.Parameter(typeof(LanguageContext), "$language ");

            // finally build the funcion that's closed over the array and
            var func = Ast.Lambda<Func<Scope, LanguageContext, object>>(
                Ast.Block(
                    new[] { GlobalArray },
                    Ast.Assign(
                        GlobalArray, 
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("GetGlobalArray"),
                            scope
                        )
                    ),
                    body
                ),
                ((PythonCompilerOptions)context.Options).ModuleName,
                new MSAst.ParameterExpression[] { scope, language }
            );

            PythonCompilerOptions pco = context.Options as PythonCompilerOptions;

            return new SavableScriptCode(func, context.SourceUnit, GetNames(), pco.ModuleName);
        }
    }
}
