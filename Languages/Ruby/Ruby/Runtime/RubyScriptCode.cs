/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using System.Threading;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using System.Security;
using IronRuby.Compiler;
using System.Diagnostics;

namespace IronRuby.Runtime {
    using ScriptCodeFunc = Func<RubyScope, object, object>;

    internal class RubyScriptCode : ScriptCode {
        private sealed class CustomGenerator : DebugInfoGenerator {
            public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression node) {
                RubyMethodDebugInfo.GetOrCreate(method.Name).AddMapping(ilOffset, node.StartLine);
            }
        }

        private readonly Expression<ScriptCodeFunc> _code;
        private readonly TopScopeFactoryKind _kind;
        private ScriptCodeFunc _target;

        public RubyScriptCode(Expression<ScriptCodeFunc>/*!*/ code, SourceUnit/*!*/ sourceUnit, TopScopeFactoryKind kind)
            : base(sourceUnit) {
            Assert.NotNull(code);
            _code = code;
            _kind = kind;
        }

        internal RubyScriptCode(ScriptCodeFunc/*!*/ target, SourceUnit/*!*/ sourceUnit, TopScopeFactoryKind kind)
            : base(sourceUnit) {
            Assert.NotNull(target);
            _target = target;
            _kind = kind;
        }

        internal Expression<ScriptCodeFunc> Code {
            get { return _code; }
        }
        
        private ScriptCodeFunc/*!*/ Target {
            get {
                if (_target == null) {
                    var compiledMethod = (ScriptCodeFunc)CompileLambda(_code, SourceUnit.LanguageContext);
                    Interlocked.CompareExchange(ref _target, compiledMethod, null);
                }
                return _target;
            }
        }

        public override object Run() {
            return Run(CreateScope(), false);
        }

        public override object Run(Scope/*!*/ scope) {
            return Run(scope, true);
        }

        private object Run(Scope/*!*/ scope, bool bindGlobals) {
            RubyScope localScope;
            RubyContext context = (RubyContext)LanguageContext;

            switch (_kind) {
                case TopScopeFactoryKind.Hosted:
                    localScope = RubyTopLevelScope.CreateHostedTopLevelScope(scope, context, bindGlobals);
                    break;

                case TopScopeFactoryKind.Main:
                    localScope = RubyTopLevelScope.CreateTopLevelScope(scope, context, true);
                    break;

                case TopScopeFactoryKind.File:
                    localScope = RubyTopLevelScope.CreateTopLevelScope(scope, context, false);
                    break;

                case TopScopeFactoryKind.WrappedFile:
                    localScope = RubyTopLevelScope.CreateWrappedTopLevelScope(scope, context);
                    break;

                default:
                    throw Assert.Unreachable;                
            }

            return Target(localScope, localScope.SelfObject);
        }

        private static bool _HasPdbPermissions = true;

        internal static Delegate/*!*/ CompileLambda(LambdaExpression/*!*/ lambda, LanguageContext/*!*/ context) {
            return CompileLambda(lambda, context.DomainManager.Configuration.DebugMode, context.Options.NoAdaptiveCompilation, context.Options.CompilationThreshold);
        }

        internal static Delegate/*!*/ CompileLambda(LambdaExpression/*!*/ lambda, bool debugMode, bool noAdaptiveCompilation, 
            int compilationThreshold) {

            if (debugMode) {
                return CompileDebug(lambda);
            } else if (noAdaptiveCompilation) {
                Delegate result = lambda.Compile();
                // DLR closures should not be used:
                Debug.Assert(!(result.Target is Closure) || ((Closure)result.Target).Locals == null);
                return result;
            } else {
                return lambda.LightCompile(compilationThreshold);
            }
        }

        // Avoid loading Ref.Emit types (Compact Framework):
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Delegate/*!*/ CompileDebug(LambdaExpression/*!*/ lambda) {
            // try to use PDBs and fallback to CustomGenerator if not allowed to:
            if (_HasPdbPermissions) {
                try {
                    return CompilerHelpers.CompileToMethod(lambda, DebugInfoGenerator.CreatePdbGenerator(), true);
                } catch (SecurityException) {
                    // do not attempt next time in this app-domain:
                    _HasPdbPermissions = false;
                }
            }
            return CompilerHelpers.CompileToMethod(lambda, new CustomGenerator(), false);
        }
    }
}
