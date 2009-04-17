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

using ScriptCodeFunc = System.Func<
    IronRuby.Runtime.RubyScope, 
    IronRuby.Runtime.RuntimeFlowControl,
    object, 
    object
>;

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
using IronRuby.Compiler;
using Microsoft.Scripting.Interpretation;

namespace IronRuby.Runtime {
    internal class RubyScriptCode : ScriptCode, IInterpretedScriptCode {
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
                    var compiledMethod = CompileLambda(_code, SourceUnit.LanguageContext.DomainManager.Configuration.DebugMode);
                    Interlocked.CompareExchange(ref _target, compiledMethod, null);
                }
                return _target;
            }
        }

        public override object Run() {
            return Run(CreateScope());
        }

        public override object Run(Scope/*!*/ scope) {
            RubyScope localScope;
            RubyContext context = (RubyContext)LanguageContext;

            switch (_kind) {
                case TopScopeFactoryKind.Default:
                    localScope = RubyTopLevelScope.CreateTopLevelScope(scope, context);
                    break;

                case TopScopeFactoryKind.GlobalScopeBound:
                    localScope = RubyTopLevelScope.CreateTopLevelHostedScope(scope, context);
                    break;

                case TopScopeFactoryKind.Main:
                    localScope = RubyTopLevelScope.CreateMainTopLevelScope(scope, context);
                    break;

                case TopScopeFactoryKind.WrappedFile:
                    localScope = RubyTopLevelScope.CreateWrappedTopLevelScope(scope, context);
                    break;

                default:
                    throw Assert.Unreachable;                
            }

            if (context.RubyOptions.InterpretedMode) {
                return Interpreter.TopLevelExecute(this, localScope, localScope.RuntimeFlowControl, localScope.SelfObject);
            } else {
                return Target(localScope, localScope.RuntimeFlowControl, localScope.SelfObject);
            }
        }

        #region // TODO: remove (old interpreter)

        internal class Evaled : IInterpretedScriptCode {
            // call sites allocated for the tree:
            private Dictionary<Expression, CallSiteInfo> _callSites;

            SourceUnit IInterpretedScriptCode.SourceUnit {
                get { return _source; }
            }

            LambdaExpression IInterpretedScriptCode.Code {
                get { return _code; }
            }

            Dictionary<Expression, CallSiteInfo> IInterpretedScriptCode.CallSites {
                get {
                    if (_callSites == null) {
                        Interlocked.CompareExchange(ref _callSites, new Dictionary<Expression, CallSiteInfo>(), null);
                    }

                    return _callSites;
                }
            }

            private readonly LambdaExpression _code;
            private readonly SourceUnit _source;

            public Evaled(LambdaExpression code, SourceUnit source) {
                _code = code;
                _source = source;
            }
        }
        
        // call sites allocated for the tree:
        private Dictionary<Expression, CallSiteInfo> _callSites;

        SourceUnit IInterpretedScriptCode.SourceUnit {
            get { return SourceUnit; }
        }

        LambdaExpression IInterpretedScriptCode.Code {
            get { return _code; }
        }

        Dictionary<Expression, CallSiteInfo> IInterpretedScriptCode.CallSites {
            get {
                if (_callSites == null) {
                    Interlocked.CompareExchange(ref _callSites, new Dictionary<Expression, CallSiteInfo>(), null);
                }

                return _callSites;
            }
        }

        internal bool HasCallSites {
            get { return _callSites != null; }
        }

        #endregion

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
