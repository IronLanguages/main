/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using ScriptCodeFunc = System.Func<Microsoft.Scripting.Runtime.Scope, Microsoft.Scripting.Runtime.LanguageContext, object>;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using System.Threading;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using System.Security;

namespace IronRuby.Runtime {
    internal sealed class RubyScriptCode : ScriptCode {
        private sealed class CustomGenerator : DebugInfoGenerator {
            public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression node) {
                RubyMethodDebugInfo.GetOrCreate(method.Name).AddMapping(ilOffset, node.StartLine);
            }
        }

        private readonly Expression<ScriptCodeFunc> _code;
        private ScriptCodeFunc _target;

        public RubyScriptCode(Expression<ScriptCodeFunc>/*!*/ code, SourceUnit/*!*/ sourceUnit)
            : base(sourceUnit) {
            Assert.NotNull(code);
            _code = code;
        }

        internal RubyScriptCode(ScriptCodeFunc/*!*/ target, SourceUnit/*!*/ sourceUnit)
            : base(sourceUnit) {
            Assert.NotNull(target);
            _target = target;
        }

        private ScriptCodeFunc/*!*/ Target {
            get {
                if (_target == null) {
                    var compiledMethod = CompileLambda<ScriptCodeFunc>(_code, SourceUnit.LanguageContext.DomainManager.Configuration.DebugMode);
                    Interlocked.CompareExchange(ref _target, compiledMethod, null);
                }
                return _target;
            }
        }

        public override object Run() {
            return Target(CreateScope(), SourceUnit.LanguageContext);
        }

        public override object Run(Scope/*!*/ scope) {
            return Target(scope, SourceUnit.LanguageContext);
        }

        private static bool _HasPdbPermissions = true;

        internal static T/*!*/ CompileLambda<T>(Expression<T>/*!*/ lambda, bool debugMode) {
            if (debugMode) {
#if !SILVERLIGHT
                // try to use PDBs and fallback to CustomGenerator if not allowed to:
                if (_HasPdbPermissions) {
                    try {
                        return CompilerHelpers.CompileToMethod(lambda, DebugInfoGenerator.CreatePdbGenerator(), true);
                    } catch (SecurityException) {
                        // do not attempt next time in this app-domain:
                        _HasPdbPermissions = false;
                    }
                }
#endif
                return CompilerHelpers.CompileToMethod(lambda, new CustomGenerator(), false);
            } else {
                return lambda.Compile();
            }
        }
    }
}
