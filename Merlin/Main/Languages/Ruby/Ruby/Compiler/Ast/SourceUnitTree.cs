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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using MSA = System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public partial class SourceUnitTree : Node {

        private readonly LexicalScope/*!*/ _definedScope;
        private readonly List<Initializer> _initializers;
        private readonly Statements/*!*/ _statements;
        private readonly Encoding/*!*/ _encoding;

        // An offset of the first byte after __END__ that can be read via DATA constant or -1 if __END__ is not present.
        private readonly int _dataOffset;

        public List<Initializer> Initializers {
            get { return _initializers; }
        }

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        public Encoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public SourceUnitTree(LexicalScope/*!*/ definedScope, Statements/*!*/ statements, List<Initializer> initializers, 
            Encoding/*!*/ encoding, int dataOffset)
            : base(SourceSpan.None) {
            Assert.NotNull(definedScope, statements, encoding);

            _definedScope = definedScope;
            _statements = statements;
            _initializers = initializers;
            _encoding = encoding;
            _dataOffset = dataOffset;
        }

        internal MSA.Expression<T>/*!*/ Transform<T>(AstGenerator/*!*/ gen) {
            Debug.Assert(gen != null);

            ScopeBuilder scope = new ScopeBuilder();

            MSA.ParameterExpression[] parameters;
            MSA.Expression selfVariable;
            MSA.Expression rfcVariable;
            MSA.Expression parentScope;
            MSA.Expression language;
            MSA.Expression runtimeScopeVariable;
            MSA.Expression moduleVariable;
            MSA.Expression blockParameter;
            MSA.Expression currentMethodVariable;

            if (gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.None ||
                gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.Module) {
                parameters = new MSA.ParameterExpression[6];

                parameters[0] = Ast.Parameter(typeof(RubyScope), "#scope");
                selfVariable = parameters[1] = Ast.Parameter(typeof(object), "#self");
                parameters[2] = Ast.Parameter(typeof(RubyModule), "#module");
                blockParameter = parameters[3] = Ast.Parameter(typeof(Proc), "#block");
                currentMethodVariable = parameters[4] = Ast.Parameter(typeof(RubyMethodInfo), "#method");
                rfcVariable = parameters[5] = Ast.Parameter(typeof(RuntimeFlowControl), "#rfc");

                if (gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.Module) {
                    runtimeScopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyScope));
                    parentScope = parameters[0];
                    moduleVariable = parameters[2];
                } else {
                    runtimeScopeVariable = parameters[0];
                    moduleVariable = null;
                    parentScope = null;
                }

                language = null;
            } else {
                parameters = new MSA.ParameterExpression[2];
                parentScope = parameters[0] = Ast.Parameter(typeof(Scope), "#globalScope");
                language = parameters[1] = Ast.Parameter(typeof(LanguageContext), "#language");

                selfVariable = scope.DefineHiddenVariable("#self", typeof(object));
                rfcVariable = scope.DefineHiddenVariable("#rfc", typeof(RuntimeFlowControl));
                runtimeScopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyScope));
                blockParameter = null;
                currentMethodVariable = null;
                moduleVariable = null;
            }

            gen.EnterSourceUnit(
                scope,
                selfVariable,
                runtimeScopeVariable,
                blockParameter,
                rfcVariable,
                currentMethodVariable,
                gen.CompilerOptions.TopLevelMethodName, // method name
                null                                    // parameters
            );

            _definedScope.TransformLocals(scope);

            MSA.Expression scopeFactoryCall;

            switch (gen.CompilerOptions.FactoryKind) {
                case TopScopeFactoryKind.Default:
                    scopeFactoryCall = Methods.CreateTopLevelScope.OpCall(
                        scope.VisibleVariables(), parentScope, language, selfVariable, rfcVariable
                    );
                    break;

                case TopScopeFactoryKind.GlobalScopeBound:
                    scopeFactoryCall = Methods.CreateTopLevelHostedScope.OpCall(
                        scope.VisibleVariables(), parentScope, language, selfVariable, rfcVariable
                    );
                    break;

                case TopScopeFactoryKind.Main:
                    scopeFactoryCall = Methods.CreateMainTopLevelScope.OpCall(
                        scope.VisibleVariables(), parentScope, language, selfVariable, rfcVariable,
                        AstUtils.Constant(gen.SourceUnit.Path, typeof(string)), AstUtils.Constant(_dataOffset)
                    );
                    break;

                case TopScopeFactoryKind.None:
                    scopeFactoryCall = null;
                    break;

                case TopScopeFactoryKind.Module:
                    scopeFactoryCall = Methods.CreateModuleEvalScope.OpCall(
                        scope.VisibleVariables(), parentScope, selfVariable, moduleVariable
                    );
                    break;

                case TopScopeFactoryKind.WrappedFile:
                    scopeFactoryCall = Methods.CreateWrappedTopLevelScope.OpCall(
                        scope.VisibleVariables(), parentScope, language, selfVariable, rfcVariable
                    );
                    break;

                default:
                    throw Assert.Unreachable;
            }

            MSA.Expression prologue, body;

            if (scopeFactoryCall != null) {
                prologue = Ast.Assign(runtimeScopeVariable, scopeFactoryCall);
            } else {
                prologue = null;
            }

            if (gen.SourceUnit.Kind == SourceCodeKind.InteractiveCode) {
                var resultVariable = scope.DefineHiddenVariable("#result", typeof(object));

                var epilogue = Methods.PrintInteractiveResult.OpCall(runtimeScopeVariable,
                    Ast.Dynamic(ConvertToSAction.Instance, typeof(MutableString), Methods.GetContextFromScope.OpCall(gen.CurrentScopeVariable), 
                        Ast.Dynamic(RubyCallAction.Make("inspect", RubyCallSignature.WithScope(0)), typeof(object), 
                            gen.CurrentScopeVariable, resultVariable
                        )
                    )
                );

                body = gen.TransformStatements(prologue, _statements, epilogue, ResultOperation.Store(resultVariable));
            } else {
                body = gen.TransformStatements(prologue, _statements, ResultOperation.Return);
            }

            body = GenerateCheckForAsyncException(scope, runtimeScopeVariable, body);
            body = gen.AddReturnTarget(scope.CreateScope(body));
            gen.LeaveSourceUnit();

            return Ast.Lambda<T>(
                body,
                RubyExceptionData.EncodeMethodName(gen.SourceUnit, RubyExceptionData.TopLevelMethodName, SourceSpan.None),
                parameters
            );
        }

        private static MSA.Expression GenerateCheckForAsyncException(ScopeBuilder scope, MSA.Expression runtimeScopeVariable, MSA.Expression body) {
            MSA.ParameterExpression exception = scope.DefineHiddenVariable("#exception", typeof(System.Threading.ThreadAbortException));
            MSA.CatchBlock handler = Ast.Catch(exception,
                Ast.Call(
                    Methods.CheckForAsyncRaiseViaThreadAbort,
                    runtimeScopeVariable,
                    exception));
            if (body.Type == typeof(void)) {
                body = Ast.TryCatch(body, handler);
            } else {
                MSA.ParameterExpression variable = scope.DefineHiddenVariable("#value", body.Type);
                body = Ast.Block(
                    Ast.TryCatch(
                        AstUtils.Void(Ast.Assign(variable, body)),
                        handler),
                    variable);
            }

            return body;
        }
    }
}
