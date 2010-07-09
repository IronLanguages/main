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
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    
    public partial class SourceUnitTree : Node {

        private readonly LexicalScope/*!*/ _definedScope;
        private readonly List<FileInitializerStatement> _initializers;
        private readonly Statements/*!*/ _statements;
        private readonly RubyEncoding/*!*/ _encoding;

        // An offset of the first byte after __END__ that can be read via DATA constant or -1 if __END__ is not present.
        private readonly int _dataOffset;

        public List<FileInitializerStatement> Initializers {
            get { return _initializers; }
        }

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public SourceUnitTree(LexicalScope/*!*/ definedScope, Statements/*!*/ statements, List<FileInitializerStatement> initializers, 
            RubyEncoding/*!*/ encoding, int dataOffset)
            : base(SourceSpan.None) {
            Assert.NotNull(definedScope, statements, encoding);

            _definedScope = definedScope;
            _statements = statements;
            _initializers = initializers;
            _encoding = encoding;
            _dataOffset = dataOffset;
        }

        private ScopeBuilder/*!*/ DefineLocals() {
            return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), null, _definedScope);
        }

        internal MSA.Expression<T>/*!*/ Transform<T>(AstGenerator/*!*/ gen) {
            Debug.Assert(gen != null);

            ScopeBuilder scope = DefineLocals();

            MSA.ParameterExpression[] parameters;
            MSA.ParameterExpression selfVariable;
            MSA.ParameterExpression runtimeScopeVariable;
            MSA.ParameterExpression blockParameter;

            if (gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.None ||
                gen.CompilerOptions.FactoryKind == TopScopeFactoryKind.ModuleEval) {
                parameters = new MSA.ParameterExpression[4];

                runtimeScopeVariable = parameters[0] = Ast.Parameter(typeof(RubyScope), "#scope");
                selfVariable = parameters[1] = Ast.Parameter(typeof(object), "#self");
                parameters[2] = Ast.Parameter(typeof(RubyModule), "#module");
                blockParameter = parameters[3] = Ast.Parameter(typeof(Proc), "#block");
            } else {
                parameters = new MSA.ParameterExpression[2];

                runtimeScopeVariable = parameters[0] = Ast.Parameter(typeof(RubyScope), "#scope");
                selfVariable = parameters[1] = Ast.Parameter(typeof(object), "#self");

                blockParameter = null;
            }

            gen.EnterSourceUnit(
                scope,
                selfVariable,
                runtimeScopeVariable,
                blockParameter,
                gen.CompilerOptions.TopLevelMethodName, // method name for blocks
                null                                    // parameters for super calls
            );

            MSA.Expression body;

            if (_statements.Count > 0) {
                if (gen.PrintInteractiveResult) {
                    var resultVariable = scope.DefineHiddenVariable("#result", typeof(object));

                    var epilogue = Methods.PrintInteractiveResult.OpCall(runtimeScopeVariable,
                        AstUtils.LightDynamic(ConvertToSAction.Make(gen.Context), typeof(MutableString),
                            CallSiteBuilder.InvokeMethod(gen.Context, "inspect", RubyCallSignature.WithScope(0),
                                gen.CurrentScopeVariable, resultVariable
                            )
                        )
                    );

                    body = gen.TransformStatements(null, _statements, epilogue, ResultOperation.Store(resultVariable));
                } else {
                    body = gen.TransformStatements(_statements, ResultOperation.Return);
                }

                // TODO:
                var exceptionVariable = Ast.Parameter(typeof(Exception), "#exception");
                body = AstUtils.Try(
                    body
                ).Filter(exceptionVariable, Methods.TraceTopLevelCodeFrame.OpCall(runtimeScopeVariable, exceptionVariable),
                    Ast.Empty()
                );
            } else {
                body = AstUtils.Constant(null);
            }

            // scope initialization:
            MSA.Expression prologue;
            switch (gen.CompilerOptions.FactoryKind) {
                case TopScopeFactoryKind.None:
                case TopScopeFactoryKind.ModuleEval:
                    prologue = Methods.InitializeScopeNoLocals.OpCall(runtimeScopeVariable, EnterInterpretedFrameExpression.Instance);
                    break;

                case TopScopeFactoryKind.Hosted:
                case TopScopeFactoryKind.File:
                case TopScopeFactoryKind.WrappedFile:
                    prologue = Methods.InitializeScope.OpCall(
                        runtimeScopeVariable, scope.MakeLocalsStorage(), scope.GetVariableNamesExpression(),
                        EnterInterpretedFrameExpression.Instance
                    );
                    break;

                case TopScopeFactoryKind.Main:
                    prologue = Methods.InitializeScope.OpCall(
                        runtimeScopeVariable, scope.MakeLocalsStorage(), scope.GetVariableNamesExpression(),
                        EnterInterpretedFrameExpression.Instance
                    );
                    if (_dataOffset >= 0) {
                        prologue = Ast.Block(
                            prologue,
                            Methods.SetDataConstant.OpCall(
                                runtimeScopeVariable,
                                gen.SourcePathConstant,
                                AstUtils.Constant(_dataOffset)
                            )
                        );
                    }
                    break;

                default:
                    throw Assert.Unreachable;
            }

            // BEGIN blocks:
            if (gen.FileInitializers != null) {
                var b = new AstBlock();
                b.Add(prologue);
                b.Add(gen.FileInitializers);
                b.Add(body);
                body = b;
            }

            body = gen.AddReturnTarget(scope.CreateScope(body));

            gen.LeaveSourceUnit();

            return Ast.Lambda<T>(body, GetEncodedName(gen), parameters);
        }

        private static string/*!*/ GetEncodedName(AstGenerator/*!*/ gen) {
            return RubyStackTraceBuilder.EncodeMethodName(RubyStackTraceBuilder.TopLevelMethodName, gen.SourcePath, SourceSpan.None, gen.DebugMode);
        }
    }
}
