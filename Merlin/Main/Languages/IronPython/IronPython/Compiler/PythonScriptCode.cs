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
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    /// <summary>
    /// Represents a script code which can be dynamically bound to execute against
    /// arbitrary Scope objects.  This is used for code when the user runs against
    /// a particular scope as well as for exec and eval code as well.  It is also
    /// used when tracing is enabled.
    /// </summary>
    class PythonScriptCode : RunnableScriptCode {
        private readonly CompilerContext/*!*/ _context;
        private CodeContext _defaultContext;
        private readonly Expression<Func<CodeContext/*!*/, FunctionCode/*!*/, object>> _lambda;
        private readonly Dictionary<int, bool> _handlerLocations;                         // list of exception handler locations for debugging
        private readonly Dictionary<int, Dictionary<int, bool>> _loopAndFinallyLocations; // list of loop and finally locations for debugging

        private Func<CodeContext/*!*/, FunctionCode/*!*/, object>/*!*/ _target, _tracingTarget; // lazily compiled targets

        public PythonScriptCode(CompilerContext/*!*/ context, Expression<Func<CodeContext/*!*/, FunctionCode/*!*/, object>>/*!*/ lambda, SourceUnit/*!*/ sourceUnit, Dictionary<int, bool> handlerLocations, Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations)
            : base(sourceUnit) {
            Assert.NotNull(lambda, context);

            _context = context;
            _lambda = lambda;
            _handlerLocations = handlerLocations;
            _loopAndFinallyLocations = loopAndFinallyLocations;
        }

        public override object Run() {
            if (SourceUnit.Kind == SourceCodeKind.Expression) {
                return EvalWrapper(DefaultContext);
            }

            return RunWorker(DefaultContext);
        }

        public override object Run(Scope scope) {
            CodeContext ctx = GetContextForScope(scope, SourceUnit);
            
            if (SourceUnit.Kind == SourceCodeKind.Expression) {
                return EvalWrapper(ctx);
            }

            return RunWorker(ctx);
        }

        private object RunWorker(CodeContext ctx) {
            Func<CodeContext/*!*/, FunctionCode/*!*/, object> target = GetTarget();

            Exception e = PythonOps.SaveCurrentException();
            PushFrame(ctx, target);
            try {
                return target(ctx, EnsureFunctionCode(target));
            } finally {
                PythonOps.RestoreCurrentException(e);
                PopFrame();
            }
        }

        private Func<CodeContext/*!*/, FunctionCode/*!*/, object> GetTarget() {
            Func<CodeContext/*!*/, FunctionCode/*!*/, object> target;
            PythonContext pc = (PythonContext)_context.SourceUnit.LanguageContext;
            if (!pc.EnableTracing) {
                EnsureTarget();
                target = _target;
            } else {
                EnsureTracingTarget();
                target = _tracingTarget;
            }
            return target;
        }

        public override FunctionCode GetFunctionCode() {
            return EnsureFunctionCode(GetTarget());
        }

        public override Scope/*!*/ CreateScope() {
            return new Scope();
        }

        protected override FunctionAttributes GetCodeAttributes() {
            return GetCodeAttributes(_context);
        }

        // wrapper so we can do minimal code gen for eval code
        private object EvalWrapper(CodeContext ctx) {
            try {
                return RunWorker(ctx);
            } catch (Exception) {
                PythonOps.UpdateStackTrace(ctx, Code, _target.Method, "<module>", "<string>", 0);
                throw;
            }
        }

        private Func<CodeContext, FunctionCode, object> CompileBody(Expression<Func<CodeContext/*!*/, FunctionCode/*!*/, object>> lambda) {
            Func<CodeContext, FunctionCode, object> func;
            PythonContext pc = (PythonContext)_context.SourceUnit.LanguageContext;

            if (lambda.Body is ConstantExpression) {
                // skip compiling for really simple code
                object value = ((ConstantExpression)lambda.Body).Value;
                return (codeCtx, functionCode) => value;
            }

            if (pc.ShouldInterpret((PythonCompilerOptions)_context.Options, _context.SourceUnit)) {
                func = CompilerHelpers.LightCompile(lambda, false);
            } else {
                func = lambda.Compile(_context.SourceUnit.EmitDebugSymbols);
            }

            return func;
        }

        private void EnsureTarget() {
            if (_target == null) {
                _target = CompileBody(_lambda);
            }
        }

        private CodeContext DefaultContext {
            get {
                if (_defaultContext == null) {
                    _defaultContext = CreateTopLevelCodeContext(new PythonDictionary(),  _context.SourceUnit.LanguageContext);
                }

                return _defaultContext;
            }
        }

        private void EnsureTracingTarget() {
            if (_tracingTarget == null) {
                PythonContext pc = (PythonContext)_context.SourceUnit.LanguageContext;

                var debugProperties = new PythonDebuggingPayload(null, _loopAndFinallyLocations, _handlerLocations);

                var debugInfo = new Microsoft.Scripting.Debugging.CompilerServices.DebugLambdaInfo(
                    null,           // IDebugCompilerSupport
                    null,           // lambda alias
                    false,          // optimize for leaf frames
                    null,           // hidden variables
                    null,           // variable aliases
                    debugProperties // custom payload
                );

                _tracingTarget = CompileBody((Expression<Func<CodeContext/*!*/, FunctionCode/*!*/, object>>)pc.DebugContext.TransformLambda(_lambda, debugInfo));
                debugProperties.Code = EnsureFunctionCode(_tracingTarget);
            }
        }
    }
}
