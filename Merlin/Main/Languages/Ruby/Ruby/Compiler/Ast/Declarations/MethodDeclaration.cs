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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System;
using Microsoft.Scripting.Actions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Interpreter;
    
namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    
    public partial class MethodDeclaration : DeclarationExpression {
        // self, block
        internal const int HiddenParameterCount = 2;
            
        /// <summary>
        /// Non-null for singleton/class methods.
        /// </summary>
        private readonly Expression _target;

        private readonly string/*!*/ _name;
        private readonly Parameters/*!*/ _parameters;

        public Expression Target {
            get { return _target; }
        }

        public string/*!*/ Name { 
            get { return _name; }
        }

        public Parameters/*!*/ Parameters {
            get { return _parameters; }
        }

        public MethodDeclaration(LexicalScope/*!*/ definedScope, Expression target, string/*!*/ name, Parameters parameters, Body/*!*/ body, 
            SourceSpan location)
            : base(definedScope, body, location) {
            Assert.NotNull(name);

            _target = target;
            _name = name;
            _parameters = parameters ?? Parameters.Empty;
        }

        private ScopeBuilder/*!*/ DefineLocals(out MSA.ParameterExpression/*!*/[]/*!*/ parameters) {
            parameters = new MSA.ParameterExpression[
                HiddenParameterCount +
                (_parameters.Mandatory != null ? _parameters.Mandatory.Count : 0) +
                (_parameters.Optional != null ? _parameters.Optional.Count : 0) +
                (_parameters.Array != null ? 1 : 0)
            ];

            int paramIndex = 0;
            int closureIndex = 0;
            int firstClosureParam = 1;
            parameters[paramIndex++] = Ast.Parameter(typeof(object), "#self");

            if (_parameters.Block != null) {
                parameters[paramIndex++] = Ast.Parameter(typeof(Proc), _parameters.Block.Name);
                _parameters.Block.SetClosureIndex(closureIndex++);
            } else {
                parameters[paramIndex++] = Ast.Parameter(typeof(Proc), "#block");
                firstClosureParam++;
            }

            if (_parameters.Mandatory != null) {
                foreach (var param in _parameters.Mandatory) {
                    parameters[paramIndex++] = Ast.Parameter(typeof(object), param.Name);
                    param.SetClosureIndex(closureIndex++);
                }
            }

            if (_parameters.Optional != null) {
                foreach (var lvalue in _parameters.Optional) {
                    var param = (LocalVariable)lvalue.Left;
                    parameters[paramIndex++] = Ast.Parameter(typeof(object), param.Name);
                    param.SetClosureIndex(closureIndex++);
                }
            }

            if (_parameters.Array != null) {
                parameters[paramIndex++] = Ast.Parameter(typeof(object), _parameters.Array.Name);
                _parameters.Array.SetClosureIndex(closureIndex++);
            }

            Debug.Assert(paramIndex == parameters.Length);

            // allocate closure slots for locals:
            int localCount = DefinedScope.AllocateClosureSlotsForLocals(closureIndex);

            return new ScopeBuilder(parameters, firstClosureParam, localCount, null, DefinedScope);
        }

        internal MSA.LambdaExpression/*!*/ TransformBody(AstGenerator/*!*/ gen, RubyScope/*!*/ declaringScope, RubyModule/*!*/ declaringModule) {
            string encodedName = RubyExceptionData.EncodeMethodName(_name, gen.SourcePath, Location);

            MSA.ParameterExpression[] parameters;
            ScopeBuilder scope = DefineLocals(out parameters);

            var currentMethodVariable = scope.DefineHiddenVariable("#method", typeof(RubyMethodInfo));
            var scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyMethodScope));
            var selfParameter = parameters[0];
            var blockParameter = parameters[1];

            // exclude block parameter even if it is explicitly specified:
            int visiblePrameterCountAndSignatureFlags = (parameters.Length - 2) << 2;
            if (_parameters.Block != null) {
                visiblePrameterCountAndSignatureFlags |= RubyMethodScope.HasBlockFlag;
            }
            if (_parameters.Array != null) {
                visiblePrameterCountAndSignatureFlags |= RubyMethodScope.HasUnsplatFlag;
            }

            gen.EnterMethodDefinition(
                scope,
                selfParameter,
                scopeVariable,
                blockParameter,
                _name,
                _parameters
            );

            // profiling:
            MSA.Expression profileStart, profileEnd;
            if (gen.Profiler != null) {
                int profileTickIndex = gen.Profiler.GetTickIndex(encodedName);
                var stampVariable = scope.DefineHiddenVariable("#stamp", typeof(long));
                profileStart = Ast.Assign(stampVariable, Methods.Stopwatch_GetTimestamp.OpCall());
                profileEnd = Methods.UpdateProfileTicks.OpCall(AstUtils.Constant(profileTickIndex), stampVariable);
            } else {
                profileStart = profileEnd = AstUtils.Empty();
            }

            // tracing:
            MSA.Expression traceCall, traceReturn;
            if (gen.TraceEnabled) {
                traceCall = Methods.TraceMethodCall.OpCall(
                    scopeVariable, 
                    gen.SourcePathConstant, 
                    AstUtils.Constant(Location.Start.Line)
                );

                traceReturn = Methods.TraceMethodReturn.OpCall(
                    gen.CurrentScopeVariable,
                    gen.SourcePathConstant,
                    AstUtils.Constant(Location.End.Line)
                );
            } else {
                traceCall = traceReturn = AstUtils.Empty();
            }

            MSA.ParameterExpression unwinder;
            
            MSA.Expression body = AstUtils.Try(
                profileStart,
                _parameters.TransformOptionalsInitialization(gen),
                traceCall,
                Body.TransformResult(gen, ResultOperation.Return)
            ).Filter(unwinder = Ast.Parameter(typeof(Exception), "#u"), Methods.IsMethodUnwinderTargetFrame.OpCall(scopeVariable, unwinder),
                Ast.Return(gen.ReturnLabel, Methods.GetMethodUnwinderReturnValue.OpCall(unwinder))
            ).Finally(  
                // leave frame:
                Methods.LeaveMethodFrame.OpCall(scopeVariable),
                Ast.Empty(),
                profileEnd,
                traceReturn
            );

            body = gen.AddReturnTarget(
                scope.CreateScope(
                    scopeVariable,
                    Methods.CreateMethodScope.OpCall(
                        scope.MakeLocalsStorage(),
                        scope.GetVariableNamesExpression(),
                        Ast.Constant(visiblePrameterCountAndSignatureFlags),
                        Ast.Constant(declaringScope, typeof(RubyScope)),
                        Ast.Constant(declaringModule, typeof(RubyModule)), 
                        Ast.Constant(_name),
                        selfParameter, blockParameter,
                        EnterInterpretedFrameExpression.Instance
                    ),
                    body
                )
            );

            gen.LeaveMethodDefinition();

            return CreateLambda(encodedName, parameters, body);
        }

        private static MSA.LambdaExpression/*!*/ CreateLambda(string/*!*/ name, MSA.ParameterExpression/*!*/[]/*!*/ parameters, MSA.Expression/*!*/ body) {
            Type lambdaType = DynamicSiteHelpers.GetStandardDelegateType(AstFactory.GetSignature(parameters, typeof(object)));
            if (lambdaType == null) {
                // to many parameters for Func<> delegate -> use object[]:
                MSA.ParameterExpression array = Ast.Parameter(typeof(object[]), "#params");
                var actualParameters = new MSA.ParameterExpression[] { parameters[0], parameters[1], array };
                parameters = ArrayUtils.ShiftLeft(parameters, 2);

                var bodyWithParamInit = new MSA.Expression[parameters.Length + 1];
                for (int i = 0; i < parameters.Length; i++) {
                    bodyWithParamInit[i] = Ast.Assign(parameters[i], Ast.ArrayIndex(array, AstUtils.Constant(i)));
                }
                bodyWithParamInit[parameters.Length] = body;

                return Ast.Lambda<Func<object, Proc, object[], object>>(
                    Ast.Block(
                        new ReadOnlyCollection<MSA.ParameterExpression>(parameters), 
                        new ReadOnlyCollection<MSA.Expression>(bodyWithParamInit)
                    ),
                    name,
                    new ReadOnlyCollection<MSA.ParameterExpression>(actualParameters)
                );
            } else {
                return Ast.Lambda(
                    lambdaType,
                    body,
                    name,
                    new ReadOnlyCollection<MSA.ParameterExpression>(parameters)
                );
            }
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.DefineMethod.OpCall(
                (_target != null) ? _target.TransformRead(gen) : AstUtils.Constant(null),
                gen.CurrentScopeVariable,
                Ast.Constant(new RubyMethodBody(gen.Context, this, gen.Document, gen.Encoding))
            );
        }
    }
}
