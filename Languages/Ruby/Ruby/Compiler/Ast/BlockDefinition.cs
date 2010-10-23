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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using AstParameters = ReadOnlyCollectionBuilder<MSA.ParameterExpression>;

    public partial class BlockDefinition : Block {
        //	{ |args| body }

        // self, block flow control:
        internal const int HiddenParameterCount = BlockDispatcher.HiddenParameterCount;

        // if the block has more parameters object[] is used:
        internal const int MaxBlockArity = BlockDispatcher.MaxBlockArity;

        private readonly Parameters/*!*/ _parameters;
        private readonly Statements/*!*/ _body;
        private readonly LexicalScope/*!*/ _definedScope;

        public Parameters/*!*/ Parameters {
            get { return _parameters; }
        }

        public Statements/*!*/ Body {
            get { return _body; }
        }

        public sealed override bool IsDefinition { get { return true; } }

        // The number of parameters excluding splat and proc:
        private int ParameterCount {
            get {
                return _parameters.Mandatory.Length + _parameters.Optional.Length;
            }
        }

        private bool HasFormalParametersInArray {
            get {
                return ParameterCount > MaxBlockArity || HasUnsplatParameter || HasProcParameter; 
            }
        }

        private bool HasUnsplatParameter {
            get {
                return _parameters.Unsplat != null;
            }
        }

        private bool HasProcParameter {
            get {
                return _parameters.Block != null;
            }
        }

        public BlockDefinition(LexicalScope/*!*/ definedScope, Parameters parameters, Statements/*!*/ body, SourceSpan location)
            : base(location) {
            Assert.NotNull(definedScope, body);

            _definedScope = definedScope;
            _body = body;
            _parameters = parameters ?? Parameters.Empty; 
        }

        private AstParameters/*!*/ DefineParameters(out MSA.ParameterExpression/*!*/ selfVariable, out MSA.ParameterExpression/*!*/ blockParamVariable) {
            // Block signature:
            // <proc>, <self>, <leading-mandatory>, <trailing-mandatory>, <optional>, &<block>, *<unsplat>
            // or
            // <proc>, <self>, object[] { <leading-mandatory>, <trailing-mandatory>, <optional>, &<block> } *<unsplat>,
            var parameters = new AstParameters(
                HiddenParameterCount +
                (HasFormalParametersInArray ? 1 : ParameterCount) +
                (HasUnsplatParameter ? 1 : 0) +
                (HasProcParameter ? 1 : 0)
            );

            // hidden parameters:
            // #proc must be the first one - it is used as instance target for method invocation:
            parameters.Add(blockParamVariable = Ast.Parameter(typeof(BlockParam), "#bp"));
            parameters.Add(selfVariable = Ast.Parameter(typeof(object), "#self"));

            if (HasFormalParametersInArray) {
                parameters.Add(Ast.Parameter(typeof(object[]), "#parameters"));
            } else {
                for (int i = 0; i < ParameterCount; i++) {
                    parameters.Add(Ast.Parameter(typeof(object), "#" + i));
                }
            }

            if (HasUnsplatParameter) {
                parameters.Add(Ast.Parameter(typeof(RubyArray), "#array"));
            }

            if (HasProcParameter) {
                parameters.Add(Ast.Parameter(typeof(Proc), "#proc"));
            }

            return parameters;
        }

        private ScopeBuilder/*!*/ DefineLocals(ScopeBuilder/*!*/ parentBuilder) {
            return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), parentBuilder, _definedScope);
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return Transform(gen, false);
        }

        internal MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen, bool isLambda) {
            ScopeBuilder scope = DefineLocals(gen.CurrentScope);

            // define hidden parameters and RHS-placeholders (#1..#n will be used as RHS of a parallel assignment):
            MSA.ParameterExpression blockParameter, selfParameter;
            var parameters = DefineParameters(out selfParameter, out blockParameter);

            MSA.ParameterExpression scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyBlockScope));
            MSA.LabelTarget redoLabel = Ast.Label();

            gen.EnterBlockDefinition(
                scope,
                blockParameter,
                selfParameter,
                scopeVariable,
                redoLabel
            );

            MSA.Expression paramInit = MakeParametersInitialization(gen, parameters);
            MSA.ParameterExpression blockUnwinder, filterVariable;

            MSA.Expression traceCall, traceReturn;
			if (gen.TraceEnabled) {
                int firstStatementLine = _body.Count > 0 ? _body.First.Location.Start.Line : Location.End.Line;
                int lastStatementLine = _body.Count > 0 ? _body.Last.Location.End.Line : Location.End.Line;

                traceCall = Methods.TraceBlockCall.OpCall(scopeVariable, blockParameter, gen.SourcePathConstant, AstUtils.Constant(firstStatementLine));
                traceReturn = Methods.TraceBlockReturn.OpCall(scopeVariable, blockParameter, gen.SourcePathConstant, AstUtils.Constant(lastStatementLine));
            } else {
                traceCall = traceReturn = Ast.Empty();
            }

            MSA.Expression body = AstUtils.Try(
                paramInit,
                traceCall,
                Ast.Label(redoLabel),
                AstUtils.Try(
                    gen.TransformStatements(_body, ResultOperation.Return)
                ).Catch(blockUnwinder = Ast.Parameter(typeof(BlockUnwinder), "#u"),
                    // redo:
                    AstUtils.IfThen(Ast.Field(blockUnwinder, BlockUnwinder.IsRedoField), Ast.Goto(redoLabel)),

                    // next:
                    gen.Return(Ast.Field(blockUnwinder, BlockUnwinder.ReturnValueField))
                )
            ).Filter(filterVariable = Ast.Parameter(typeof(Exception), "#e"),
                Methods.FilterBlockException.OpCall(scopeVariable, filterVariable)
            ).Finally(
                traceReturn,
                Ast.Empty()
            );

            body = gen.AddReturnTarget(
                scope.CreateScope(
                    scopeVariable,
                    Methods.CreateBlockScope.OpCall(new AstExpressions {
                        scope.MakeLocalsStorage(),
                        scope.GetVariableNamesExpression(),
                        blockParameter, 
                        selfParameter,
                        EnterInterpretedFrameExpression.Instance
                    }),
                    body
                )
            );

            gen.LeaveBlockDefinition();

            int parameterCount = ParameterCount;
            var attributes = _parameters.GetBlockSignatureAttributes();

            var dispatcher = Ast.Constant(
                BlockDispatcher.Create(parameterCount, attributes, gen.SourcePath, Location.Start.Line), typeof(BlockDispatcher)
            );

            return Ast.Coalesce(
                (isLambda ? Methods.InstantiateLambda : Methods.InstantiateBlock).OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, dispatcher),
                (isLambda ? Methods.DefineLambda : Methods.DefineBlock).OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, dispatcher,
                    BlockDispatcher.CreateLambda(
                        body,
                        RubyStackTraceBuilder.EncodeMethodName(gen.CurrentMethod.MethodName, gen.SourcePath, Location, gen.DebugMode),
                        parameters,
                        parameterCount,
                        attributes
                    )
                )
            );
        }

        private MSA.Expression/*!*/ GetParameterAccess(AstParameters/*!*/ parameters, MSA.Expression paramsArray, int i) {
            return (paramsArray != null) ? (MSA.Expression)Ast.ArrayAccess(paramsArray, AstUtils.Constant(i)) : parameters[HiddenParameterCount + i];
        }

        private MSA.Expression/*!*/ MakeParametersInitialization(AstGenerator/*!*/ gen, AstParameters/*!*/ parameters) {
            var result = new AstExpressions(
                ParameterCount + 
                (_parameters.Optional.Length > 0 ? 1 : 0) + 
                (_parameters.Unsplat != null ? 1 : 0) +
                (_parameters.Block != null ? 1 : 0) +
                1
             );

            // TODO: we can skip parameters that are locals (need to be defined as parameters, not as #n):

            var paramsArray = HasFormalParametersInArray ? parameters[HiddenParameterCount] : null;

            int parameterIndex = 0;
            for (int i = 0; i < _parameters.Mandatory.Length; i++) {
                result.Add(_parameters.Mandatory[i].TransformWrite(gen, GetParameterAccess(parameters, paramsArray, parameterIndex)));
                parameterIndex++;
            }

            if (_parameters.Optional.Length > 0) {
                for (int i = 0; i < _parameters.Optional.Length; i++) {
                    result.Add(_parameters.Optional[i].Left.TransformWrite(gen, GetParameterAccess(parameters, paramsArray, parameterIndex)));
                    parameterIndex++;
                }
                result.Add(_parameters.TransformOptionalsInitialization(gen));
            }

            if (_parameters.Unsplat != null) {
                // the last parameter is unsplat:
                result.Add(_parameters.Unsplat.TransformWrite(gen, parameters[parameters.Count - (_parameters.Block != null ? 2 : 1)]));
            }

            if (_parameters.Block != null) {
                result.Add(_parameters.Block.TransformWrite(gen, parameters[parameters.Count - 1]));
                parameterIndex++;
            }

            result.Add(AstUtils.Empty());
            return Ast.Block(result);
        }
    }
}
