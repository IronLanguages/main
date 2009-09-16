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
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    class SavableGlobalAllocator : ArrayGlobalAllocator {
        public SavableGlobalAllocator(PythonContext/*!*/ context)
            : base(context) {
        }

        public override MSAst.Expression GetConstant(object value) {
            return Utils.Constant(value);
        }

        public override MSAst.Expression[] PrepareScope(AstGenerator gen) {
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

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast, Dictionary<int, bool> handlerLocations, Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations) {
            // finally build the funcion that's closed over the array
            var func = Ast.Lambda<Func<CodeContext, FunctionCode, object>>(
                Ast.Block(
                    new[] { GlobalArray },
                    Ast.Assign(
                        GlobalArray, 
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"),
                            IronPython.Compiler.Ast.ArrayGlobalAllocator._globalContext 
                        )
                    ),
                    Utils.Convert(body, typeof(object))
                ),
                ((PythonCompilerOptions)context.Options).ModuleName,
                ArrayGlobalAllocator._arrayFuncParams
            );

            PythonCompilerOptions pco = context.Options as PythonCompilerOptions;

            return new PythonSavableScriptCode(func, context.SourceUnit, GetNames(), pco.ModuleName);
        }
    }
}
