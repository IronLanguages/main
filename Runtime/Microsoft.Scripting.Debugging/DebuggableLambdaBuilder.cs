/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Diagnostics;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Debugging {
    using Ast = MSAst.Expression;

    /// <summary>
    /// DebuggableLambdaBuilder is used to transform a DLR expression tree into a debuggable lambda expression.
    /// </summary>
    internal class DebuggableLambdaBuilder {
        private readonly DebugContext _debugContext;                                    // DebugContext
        private Dictionary<DebugSourceFile, MSAst.ParameterExpression> _sourceFilesMap; // Map of source file instances to their variable nodes
        private readonly DebugLambdaInfo _lambdaInfo;                                   // Labmda info that's passed to us by the compiler
        private string _alias;                                                          // Lambda alias
        private readonly MSAst.Expression _debugContextExpression;                      // DebugContext Expression
        private readonly MSAst.Expression _globalDebugMode;                             // Global DebugMode expression
        private MSAst.LabelTarget _generatorLabelTarget;                                // Generator label target
        private DebugSourceSpan[] _debugMarkerLocationMap;                              // Map of debug markers to source locations
        private IList<VariableInfo>[] _variableScopeMap;                                // Map of debug markers to source locations
        private List<MSAst.ParameterExpression> _lambdaVars;                            // List of locals for transformed lambda
        private List<MSAst.ParameterExpression> _lambdaParams;                          // List of params for transformed lambda
        private List<MSAst.ParameterExpression> _generatorVars;                         // List of locals for generator lambda
        private List<MSAst.ParameterExpression> _generatorParams;                       // List of params for generator lambda
        private MSAst.ParameterExpression _retVal;                                      // Temp variable used to store the labmda's return value.
        private MSAst.Expression _pushFrame;                                            // Push-frame expression
        private MSAst.Expression _conditionalPushFrame;                                 // Conditional Push-frame expression
        private bool _noPushFrameOptimization;                                          // Flag that specifies whether we're performing the push-frame optimization for this lambda
        private readonly List<MSAst.ParameterExpression> _pendingLocals;                // List of original locals
        private readonly List<MSAst.ParameterExpression> _verifiedLocals;               // List of post-transform locals
        private readonly Dictionary<string, object> _verifiedLocalNames;                // Unique names for locals
        private readonly List<VariableInfo> _variableInfos;                             // List of variables that'll be available at runtime
        private readonly Dictionary<MSAst.ParameterExpression, VariableInfo> _pendingToVariableInfosMap;               // Map of original locals to their VariableInfo
        private readonly Dictionary<MSAst.ParameterExpression, MSAst.ParameterExpression> _pendingToVerifiedLocalsMap; // Map of original locals to post-transform locals
        private MSAst.Expression _functionInfo;                                         // Expression that's used to initialize $funcInfo local
        private int _lambdaId;

        // Static variable/parameter expressions
        private static readonly MSAst.ParameterExpression _frame = Ast.Variable(typeof(DebugFrame), "$frame");
        private static readonly MSAst.ParameterExpression _thread = Ast.Variable(typeof(DebugThread), "$thread");
        private static readonly MSAst.ParameterExpression _debugMarker = Ast.Variable(typeof(int), "$debugMarker");
        private static readonly MSAst.ParameterExpression _framePushed = Ast.Variable(typeof(bool), "$framePushed");
        private static readonly MSAst.ParameterExpression _funcInfo = Ast.Parameter(typeof(FunctionInfo), "$funcInfo");
        private static readonly MSAst.ParameterExpression _traceLocations = Ast.Parameter(typeof(bool[]), "$traceLocations");
        private static readonly MSAst.ParameterExpression _retValAsObject = Ast.Variable(typeof(object), "$retVal");
        private static readonly MSAst.ParameterExpression _retValFromGeneratorLoop = Ast.Variable(typeof(object), "$retValFromGen");
        private static readonly MSAst.ParameterExpression _frameExitException = Ast.Parameter(typeof(bool), "$frameExitException");

        internal DebuggableLambdaBuilder(DebugContext debugContext, DebugLambdaInfo lambdaInfo) {
            _debugContext = debugContext;
            _lambdaInfo = lambdaInfo;

            _alias = _lambdaInfo.LambdaAlias;
            _debugContextExpression = AstUtils.Constant(debugContext);

            // Variables
            _verifiedLocals = new List<MSAst.ParameterExpression>();
            _verifiedLocalNames = new Dictionary<string, object>();
            _pendingLocals = new List<MSAst.ParameterExpression>();
            _variableInfos = new List<VariableInfo>();
            _pendingToVariableInfosMap = new Dictionary<MSAst.ParameterExpression, VariableInfo>();
            _pendingToVerifiedLocalsMap = new Dictionary<MSAst.ParameterExpression, MSAst.ParameterExpression>();

            // DebugMode expression that's used by the transformed code to see what the current debug mode is
            _globalDebugMode = Ast.Property(_debugContextExpression, "Mode");
        }

        internal MSAst.LambdaExpression Transform(MSAst.LambdaExpression lambda) {
            if (_alias == null) {
                _alias = lambda.Name;

                if (_alias == null) {
                    _alias = "$lambda" + ++_lambdaId;
                }
            }

            // Create lambda builders
            _lambdaVars = new List<MSAst.ParameterExpression>();
            _lambdaParams = new List<MSAst.ParameterExpression>();
            _generatorVars = new List<MSAst.ParameterExpression>();
            _generatorParams = new List<MSAst.ParameterExpression>();

            if (lambda.Body is GeneratorExpression) {
                return TransformGenerator(lambda);
            } else {
                return TransformLambda(lambda);
            }
        }

        private MSAst.LambdaExpression TransformLambda(MSAst.LambdaExpression lambda) {
            MSAst.Expression body = lambda.Body;

            _lambdaVars.AddRange(new[] { _thread, _framePushed, _funcInfo, _traceLocations, _debugMarker, _frameExitException });

            _generatorParams.Add(_frame);

            Type returnType = lambda.Type.GetMethod("Invoke").ReturnType;

            // Create $retVal variable only if the return type isn't void
            if (returnType == typeof(object))
                _retVal = _retValAsObject;
            else if (returnType != typeof(void))
                _retVal = Ast.Variable(returnType, "$retVal");

            if (_retVal != null) {
                _lambdaVars.Add(_retVal);
                _generatorVars.Add(_retVal);
            }

            _lambdaVars.Add(_retValFromGeneratorLoop);

            Dictionary<MSAst.ParameterExpression, object> parameters = new Dictionary<MSAst.ParameterExpression, object>();
            foreach (MSAst.ParameterExpression parameter in lambda.Parameters) {
                parameters.Add(parameter, null);
            }

            // Add parameters to the pending list
            _pendingLocals.AddRange(lambda.Parameters);

            // Run 1st tree walk to identify all locals
            LambdaWalker lambdaWalker = new LambdaWalker();
            body = lambdaWalker.Visit(body);

            // Add all locals to pending list
            _pendingLocals.AddRange(lambdaWalker.Locals);

            // Process the variables
            LayOutVariables(lambdaWalker.StrongBoxedLocals, parameters);

            // Rewrite for generator
            MSAst.Expression generatorBody = TransformToGeneratorBody(body);

            // Add source file variables
            _lambdaVars.AddRange(_sourceFilesMap.Values);

            // Create the expression for pushing the frame
            CreatePushFrameExpression();

            // Rewrite for debuggable body
            MSAst.Expression debuggableBody = TransformToDebuggableBody(body);

            // Get the generator factory lambda
            MSAst.LambdaExpression generatorFactoryLambda = CreateGeneratorFactoryLambda(generatorBody);

            // Create FunctionInfo object
            CreateFunctionInfo(generatorFactoryLambda);


            // Create the outer lambda
            return CreateOuterLambda(lambda.Type, debuggableBody);
        }

        private MSAst.LambdaExpression TransformGenerator(MSAst.LambdaExpression lambda) {
            GeneratorExpression generator = (GeneratorExpression)lambda.Body;
            MSAst.Expression body = generator.Body;

            _generatorLabelTarget = generator.Target;

            // $TODO: Detect if the label's type is not typeof(object), and create a new label
            Debug.Assert(_generatorLabelTarget.Type == typeof(object));

            _generatorParams.Add(_frame);

            Dictionary<MSAst.ParameterExpression, object> parameters = new Dictionary<MSAst.ParameterExpression, object>();
            foreach (MSAst.ParameterExpression parameter in lambda.Parameters) {
                parameters.Add(parameter, null);
            }

            // Add parameters to the pending list
            _pendingLocals.AddRange(lambda.Parameters);

            // Run 1st tree walk to identify all locals
            LambdaWalker lambdaWalker = new LambdaWalker();
            lambdaWalker.Visit(body);

            // Add all locals to pending list
            _pendingLocals.AddRange(lambdaWalker.Locals);

            // Prepare variables
            LayoutVariablesForGenerator(parameters);

            // Rewrite for generator
            MSAst.Expression generatorBody = TransformToGeneratorBody(body);

            // Get the generator factory lambda
            MSAst.LambdaExpression generatorFactoryLambda = CreateGeneratorFactoryLambda(generatorBody);

            // Create FunctionInfo object
            CreateFunctionInfo(generatorFactoryLambda);

            // Create our own outer generator lambda
            return CreateOuterGeneratorFactory(lambda.Type);
        }

        // Lays out variables exactly in the order they'll be lifted.  Any variables with conflicting names
        // are replaced with new variables.  Strongbox'ed variables are not lifted here.
        private void LayOutVariables(Dictionary<MSAst.ParameterExpression, object> strongBoxedLocals, Dictionary<MSAst.ParameterExpression, object> parameters) {

            IList<MSAst.ParameterExpression> hiddenVariables = _lambdaInfo.HiddenVariables;

            int byrefIndex = 0;
            int strongBoxIndex = 0;

            for (int i = 0; i < _pendingLocals.Count; i++) {
                MSAst.ParameterExpression pendingLocal = _pendingLocals[i];
                MSAst.ParameterExpression verifiedLocal = pendingLocal;

                string alias;

                // See if there's an alias for the local
                if (_lambdaInfo.VariableAliases == null || !_lambdaInfo.VariableAliases.TryGetValue(pendingLocal, out alias)) {
                    alias = pendingLocal.Name;
                }

                bool isParameter = parameters.ContainsKey(pendingLocal);
                bool isHidden = hiddenVariables != null && hiddenVariables.Contains(pendingLocal);
                bool isStrongBoxed = strongBoxedLocals.ContainsKey(pendingLocal);

                if (alias == null) {
                    alias = "local";
                    isHidden = true;
                }

                bool isDuplicate = _verifiedLocalNames.ContainsKey(alias);

                // Check if we need to replace the local because of name collisions
                if (isDuplicate) {
                    // Get a unique name
                    int count = 1;
                    while (isDuplicate) {
                        alias = alias + count++;
                        isDuplicate = _verifiedLocalNames.ContainsKey(alias);
                    }

                    verifiedLocal = Ast.Parameter(verifiedLocal.Type, alias);
                }

                _verifiedLocals.Add(verifiedLocal);
                _verifiedLocalNames.Add(alias, null);

                if (pendingLocal != verifiedLocal) {
                    _pendingToVerifiedLocalsMap.Add(pendingLocal, verifiedLocal);
                }

                int localIndex = isStrongBoxed ? strongBoxIndex++ : byrefIndex++;
                VariableInfo varInfo = new VariableInfo(alias, pendingLocal.Type, isParameter, isHidden, isStrongBoxed, localIndex, _variableInfos.Count);

                _variableInfos.Add(varInfo);
                _pendingToVariableInfosMap.Add(pendingLocal, varInfo);

                // Add the variable to builders
                if (isParameter) {
                    _lambdaParams.Add(pendingLocal);
                    _generatorParams.Add(pendingLocal);
                } else {
                    _lambdaVars.Add(verifiedLocal);
                    _generatorVars.Add(pendingLocal);
                }
            }
        }

        private void LayoutVariablesForGenerator(Dictionary<MSAst.ParameterExpression, object> parameters) {
            IList<MSAst.ParameterExpression> hiddenVariables = _lambdaInfo.HiddenVariables;
            int strongBoxIndex = 0;

            for (int i = 0; i < _pendingLocals.Count; i++) {
                MSAst.ParameterExpression pendingLocal = _pendingLocals[i];
                MSAst.ParameterExpression verifiedLocal = pendingLocal;

                string alias;

                // See if there's an alias for the local
                if (_lambdaInfo.VariableAliases == null || !_lambdaInfo.VariableAliases.TryGetValue(pendingLocal, out alias)) {
                    alias = pendingLocal.Name;
                }

                bool isParameter = parameters.ContainsKey(pendingLocal);
                bool isHidden = hiddenVariables != null && hiddenVariables.Contains(pendingLocal);

                if (alias == null) {
                    alias = "local";
                    isHidden = true;
                }

                bool isDuplicate = _verifiedLocalNames.ContainsKey(alias);

                // Check if we need to replace the local because of name collisions
                if (isDuplicate) {
                    // Get a unique name
                    int count = 1;
                    while (isDuplicate) {
                        alias = alias + count++;
                        isDuplicate = _verifiedLocalNames.ContainsKey(alias);
                    }

                    verifiedLocal = Ast.Parameter(verifiedLocal.Type, alias);
                }

                _verifiedLocals.Add(verifiedLocal);
                _verifiedLocalNames.Add(alias, null);

                if (pendingLocal != verifiedLocal) {
                    _pendingToVerifiedLocalsMap.Add(pendingLocal, verifiedLocal);
                }

                VariableInfo varInfo = new VariableInfo(alias, pendingLocal.Type, isParameter, isHidden, true, strongBoxIndex++, _variableInfos.Count);

                _variableInfos.Add(varInfo);
                _pendingToVariableInfosMap.Add(pendingLocal, varInfo);

                // Add the variable to builders
                if (isParameter) {
                    _lambdaParams.Add(pendingLocal);
                    _generatorParams.Add(pendingLocal);
                } else {
                    _generatorVars.Add(pendingLocal);
                }
            }
        }

        private void CreatePushFrameExpression() {
            _pushFrame = Ast.Block(
                Ast.Assign(_framePushed, Ast.Constant(true)),

                // Get thread
                Ast.Assign(
                    _thread,
                    AstUtils.SimpleCallHelper(
                        typeof(RuntimeOps).GetMethod("GetCurrentThread"),
                        _debugContextExpression
                    )
                ),

                _debugContext.ThreadFactory.CreatePushFrameExpression(_funcInfo, _debugMarker, _verifiedLocals, _variableInfos, _thread)
            );

            _conditionalPushFrame = AstUtils.If(
                Ast.Equal(_framePushed, Ast.Constant(false)),
                _pushFrame
            );
        }

        private void CreateFunctionInfo(MSAst.LambdaExpression generatorFactoryLambda) {
            if (_lambdaInfo.CompilerSupport != null && _lambdaInfo.CompilerSupport.DoesExpressionNeedReduction(generatorFactoryLambda)) {
                _functionInfo = _lambdaInfo.CompilerSupport.QueueExpressionForReduction(
                    Ast.Call(
                        typeof(RuntimeOps).GetMethod("CreateFunctionInfo"),
                        generatorFactoryLambda,
                        AstUtils.Constant(_alias),
                        AstUtils.Constant(_debugMarkerLocationMap, typeof(object)),
                        AstUtils.Constant(_variableScopeMap, typeof(object)),
                        AstUtils.Constant(_variableInfos, typeof(object)),
                        Ast.Constant(_lambdaInfo.CustomPayload, typeof(object))
                    )
                );
            } else {
                _functionInfo = Ast.Constant(
                    DebugContext.CreateFunctionInfo(
                        generatorFactoryLambda.Compile(),
                        _alias,
                        _debugMarkerLocationMap,
                        _variableScopeMap,
                        _variableInfos,
                        _lambdaInfo.CustomPayload), 
                    typeof(FunctionInfo));
            }
        }

        private MSAst.Expression TransformToDebuggableBody(MSAst.Expression body) {
            return new DebugInfoRewriter(
                _debugContext,
                false,
                _traceLocations,
                _thread,
                _frame,
                _noPushFrameOptimization ? null : _conditionalPushFrame,
                _debugMarker,
                _globalDebugMode,
                _sourceFilesMap,
                null,
                _pendingToVerifiedLocalsMap,
                null,
                _lambdaInfo).Visit(body);
        }

        private MSAst.Expression TransformToGeneratorBody(MSAst.Expression body) {
            if (_generatorLabelTarget == null)
                _generatorLabelTarget = Ast.Label(typeof(object));

            DebugInfoRewriter debugInfoToYieldRewriter = new DebugInfoRewriter(
                _debugContext,
                true,
                _traceLocations,
                _thread,
                _frame,
                null,
                null,
                _globalDebugMode,
                null,
                _generatorLabelTarget,
                null,
                _pendingToVariableInfosMap,
                _lambdaInfo);

            MSAst.Expression transformedBody = debugInfoToYieldRewriter.Visit(body);
            _debugMarkerLocationMap = debugInfoToYieldRewriter.DebugMarkerLocationMap;
            _variableScopeMap = debugInfoToYieldRewriter.VariableScopeMap;

            // Populate sourceFileMap-to-variables map
            _sourceFilesMap = new Dictionary<DebugSourceFile, MSAst.ParameterExpression>();
            foreach (DebugSourceSpan sourceSpan in _debugMarkerLocationMap) {
                if (!_sourceFilesMap.ContainsKey(sourceSpan.SourceFile)) {
                    _sourceFilesMap.Add(sourceSpan.SourceFile, Ast.Parameter(typeof(DebugSourceFile)));
                }
            }

            // Don't perform leaf-frame optimization if the compiler didn't ask for it or 
            // if we found unconditional calls to other debuggable labmdas.
            _noPushFrameOptimization = !_lambdaInfo.OptimizeForLeafFrames || debugInfoToYieldRewriter.HasUnconditionalFunctionCalls;
            return transformedBody;
        }

        private MSAst.LambdaExpression CreateOuterLambda(Type lambdaType, MSAst.Expression debuggableBody) {
            List<MSAst.Expression> bodyExpressions = new List<MSAst.Expression>();
            List<MSAst.Expression> tryExpressions = new List<MSAst.Expression>();
            List<MSAst.Expression> finallyExpressions = new List<MSAst.Expression>();

            Type returnType = lambdaType.GetMethod("Invoke").ReturnType;
            MSAst.LabelTarget returnLabelTarget = Ast.Label(returnType);

            // Init $funcInfo
            tryExpressions.Add(
                Ast.Assign(
                    _funcInfo,
                    Ast.Convert(_functionInfo, typeof(FunctionInfo))
                )
            );

            // Init $traceLocations
            // $TODO: only do this if we're in TracePoints mode
            tryExpressions.Add(
                Ast.Assign(
                    _traceLocations,
                    Ast.Call(typeof(RuntimeOps).GetMethod("GetTraceLocations"), _funcInfo)
                )
            );

            // Init sourceFile locals
            foreach (var entry in _sourceFilesMap) {
                tryExpressions.Add(
                    Ast.Assign(
                        entry.Value,
                        Ast.Constant(entry.Key, typeof(DebugSourceFile))
                    )
                );
            }

            if (_noPushFrameOptimization) {
                tryExpressions.Add(_pushFrame);
            }
            
            tryExpressions.Add(Ast.Call(
                typeof(RuntimeOps).GetMethod("OnFrameEnterTraceEvent"),
                _thread
            ));
            
            var frameExit = AstUtils.If(
                Ast.Equal(
                    _debugMarkerLocationMap.Length > 0 ?
                        Ast.Property(_sourceFilesMap[_debugMarkerLocationMap[0].SourceFile], "Mode") :
                        _globalDebugMode,
                    AstUtils.Constant((int)DebugMode.FullyEnabled)
                ),
                Ast.Call(
                    typeof(RuntimeOps).GetMethod("OnFrameExitTraceEvent"),
                    _thread,
                    _debugMarker,
                    _retVal != null ? (MSAst.Expression)Ast.Convert(_retVal, typeof(object)) : Ast.Constant(null)
                )
            );

            // normal exit
            tryExpressions.Add(
                Ast.Block(
                    _retVal != null ? Ast.Assign(_retVal, debuggableBody) : debuggableBody, 
                    Ast.Assign(_frameExitException, Ast.Constant(true)),
                    frameExit) 
            );

            tryExpressions.Add(
                _retVal != null ? (MSAst.Expression)Ast.Return(returnLabelTarget, _retVal) : Ast.Empty()
            );

            MSAst.Expression[] popFrame = new MSAst.Expression[] {
                AstUtils.If(
                    // Fire thead-exit event if PopFrame returns true
                    Ast.AndAlso(
                        Ast.Equal(Ast.Call(typeof(RuntimeOps).GetMethod("PopFrame"), _thread), Ast.Constant(true)),
                        Ast.Equal(_globalDebugMode, AstUtils.Constant((int)DebugMode.FullyEnabled))
                    ),
                    Ast.Call(
                        typeof(RuntimeOps).GetMethod("OnThreadExitEvent"),
                        _thread
                    )
                )
            };

            if (_noPushFrameOptimization) {
                finallyExpressions.AddRange(popFrame);
            } else {
                finallyExpressions.Add(
                     AstUtils.If(
                         Ast.Equal(_framePushed, Ast.Constant(true)),
                         popFrame
                    )
                );
            }

            MSAst.ParameterExpression caughtException;

            // Run the function body
            bodyExpressions.Add(Ast.TryCatchFinally(
                Ast.TryCatch(
                    Ast.Block(
                        ArrayUtils.Append(tryExpressions.ToArray(), Ast.Default(returnType))
                    ),
                    Ast.Catch(
                        caughtException = Ast.Variable(typeof(Exception), "$caughtException"), 
                        Ast.Block(
                            // The expressions below will always throw.
                            // If the exception needs to be cancelled then OnTraceEvent will throw ForceToGeneratorLoopException.
                            // If the exception is not being cancelled then we'll just rethrow at the end of the catch block.
                            AstUtils.If(
                                Ast.Not(
                                    Ast.TypeIs(
                                        caughtException,
                                        typeof(ForceToGeneratorLoopException)
                                    )
                                ),
                                AstUtils.If(
                                    Ast.NotEqual(_globalDebugMode, AstUtils.Constant((int)DebugMode.Disabled)),
                                    _noPushFrameOptimization ? Ast.Empty() : _conditionalPushFrame,
                                    Ast.Call(
                                        typeof(RuntimeOps).GetMethod("OnTraceEventUnwind"),
                                        _thread,
                                        _debugMarker,
                                        caughtException
                                    )
                                ),
                                // exception exit
                                AstUtils.If(
                                    Ast.Not(_frameExitException),
                                    frameExit
                                )                                
                            ),

                            Ast.Rethrow(),

                            // Ensuring that the catch block is of the same type as the try block
                            Ast.Default(returnType)
                        )
                    )
                ),
                Ast.Block(finallyExpressions),
                Ast.Catch(
                    typeof(ForceToGeneratorLoopException),
                    Ast.TryFinally(
                        // Handle ForceToGeneratorLoopException
                        Ast.Block(
                            returnType != typeof(void) ? Ast.Block(
                                Ast.Assign(
                                    _retValFromGeneratorLoop, 
                                    Ast.Call(
                                        typeof(RuntimeOps).GetMethod("GeneratorLoopProc"),
                                        _thread
                                    )
                                ),
                                AstUtils.If(
                                    Ast.NotEqual(
                                        _retValFromGeneratorLoop,
                                        Ast.Constant(null)
                                    ),
                                    Ast.Assign(_retVal, Ast.Convert(_retValFromGeneratorLoop, returnType)),
                                    Ast.Return(
                                        returnLabelTarget,
                                        Ast.Convert(_retValFromGeneratorLoop, returnType)
                                    )
                                ).Else(
                                    Ast.Assign(_retVal, Ast.Default(returnType)),
                                    Ast.Return(
                                        returnLabelTarget,
                                        Ast.Default(returnType)
                                    )
                                )
                            ) :
                            Ast.Block(
                                Ast.Call(
                                    typeof(RuntimeOps).GetMethod("GeneratorLoopProc"),
                                    _thread
                                ),
                                Ast.Return(returnLabelTarget)
                            )
                            ,
                            // Ensuring that the catch block is of the same type as the try block
                            Ast.Default(returnType)
                        ),
                        // Make sure that the debugMarker is up-to-date after the generator loop
                        Ast.Assign(
                            _debugMarker, 
                            Ast.Call(
                                typeof(RuntimeOps).GetMethod("GetCurrentSequencePointForLeafGeneratorFrame"),
                                _thread
                            )
                        )
                    )
                )
            ));

            MSAst.Expression body = Ast.Block(bodyExpressions);

            if (body.Type == typeof(void) && returnType != typeof(void)) {
                body = Ast.Block(body, Ast.Default(returnType));
            }

            return Ast.Lambda(
                lambdaType,
                Ast.Block(
                    _lambdaVars,
                    Ast.Label(returnLabelTarget, body)
                ),
                _alias,
                _lambdaParams);
        }

        private MSAst.LambdaExpression CreateGeneratorFactoryLambda(MSAst.Expression generatorBody) {
            MSAst.Expression body = Ast.Block(
                Ast.Call(
                    typeof(RuntimeOps).GetMethod("ReplaceLiftedLocals"),
                    _frame,
                    Ast.RuntimeVariables(_pendingLocals)
                ),
                generatorBody
            );

            if (_retVal != null) {
                body = Ast.Block(
                    Ast.Assign(_retVal, body),
                    AstUtils.YieldReturn(
                        _generatorLabelTarget, Ast.Convert(_retVal, typeof(object))
                    )
                );
            }

            List<Type> argTypes = new List<Type>();
            for (int i = 0; i < _variableInfos.Count; i++) {
                VariableInfo varInfo = _variableInfos[i];
                if (varInfo.IsParameter)
                    argTypes.Add(varInfo.VariableType);
            }

            if (body.Type != typeof(void)) {
                body = AstUtils.Void(body);
            }

            return AstUtils.GeneratorLambda(
                InvokeTargets.GetGeneratorFactoryTarget(argTypes.ToArray()),
                _generatorLabelTarget,
                Ast.Block(
                    _generatorVars,
                    body
                ),
                _alias,
                _generatorParams);
        }

        private MSAst.LambdaExpression CreateOuterGeneratorFactory(Type lambdaType) {
            MSAst.LabelTarget returnLabelTarget = Ast.Label(lambdaType.GetMethod("Invoke").ReturnType);

            MSAst.Expression body = Ast.Return(
                returnLabelTarget,
                Ast.Call(
                    typeof(RuntimeOps),
                    "CreateDebugGenerator",
                    new[] { _generatorLabelTarget.Type },
                     Ast.Call(
                        typeof(RuntimeOps).GetMethod("CreateFrameForGenerator"),
                        _debugContextExpression,
                        _functionInfo
                    )
                )
            );

            MSAst.LabelExpression returnLabel = null;
            if (returnLabelTarget.Type == typeof(void)) {
                returnLabel = Ast.Label(returnLabelTarget, AstUtils.Void(body));
            } else {
                returnLabel = Ast.Label(returnLabelTarget, AstUtils.Convert(body, returnLabelTarget.Type));
            }

            return Ast.Lambda(
                lambdaType,
                Ast.Block(
                    _lambdaVars,
                    returnLabel
                ),
                _alias,
                _lambdaParams);
        }
    }
}
