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
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

using IronPython.Compiler.Ast;
using IronPython.Runtime;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler {
    /// <summary>
    /// Represents a script code which can be consumed at runtime as-is.  This code has
    /// no external dependencies and is closed over it's scope.  
    /// </summary>
    class RuntimeScriptCode : ScriptCode {
        private readonly CompilerContext/*!*/ _context;
        private readonly PythonAst/*!*/ _ast;
        private readonly CodeContext/*!*/ _optimizedContext;
        private readonly MSAst.Expression<Func<object>> _code;
        private Func<object> _optimizedTarget;

        private ScriptCode _unoptimizedCode;

        public RuntimeScriptCode(CompilerContext/*!*/ context, MSAst.Expression<Func<object>>/*!*/ expression, PythonAst/*!*/ ast, CodeContext/*!*/ codeContext)
            : base(context.SourceUnit) {
            _code = expression;
            _ast = ast;
            _context = context;
            _optimizedContext = codeContext;
        }

        public override object Run() {
            return InvokeTarget(_code, CreateScope());
        }

        public override object Run(Scope scope) {
            return InvokeTarget(_code, scope);
        }

        private object InvokeTarget(MSAst.LambdaExpression code, Scope scope) {
            if (scope == _optimizedContext.Scope) {
                EnsureCompiled();

                return _optimizedTarget();
            }

            // if we're running different code then re-compile the code under a new scope
            if (_unoptimizedCode == null) {
                // TODO: Copy instead of mutate
                ((PythonCompilerOptions)_context.Options).Optimized = false;
                Interlocked.CompareExchange(
                    ref _unoptimizedCode,
                    _ast.TransformToAst(CompilationMode.Loookup, _context),
                    null
                );
            }

            return _unoptimizedCode.Run(scope);
        }

        public override Scope/*!*/ CreateScope() {
            return _optimizedContext.Scope;
        }

        private void EnsureCompiled() {
            if (_optimizedTarget == null) {
                Interlocked.CompareExchange(ref _optimizedTarget, Compile(), null);
            }
        }

        private Func<object>/*!*/ Compile() {
            var pco = (PythonCompilerOptions)_context.Options;
            var pc = (PythonContext)SourceUnit.LanguageContext;
            
            if (pc.ShouldInterpret(pco, SourceUnit)) {
                return CompilerHelpers.LightCompile(_code);
            } else {
                return _code.Compile(SourceUnit.EmitDebugSymbols);
            }
        }
    }
}
