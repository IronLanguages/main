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

using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;
using System;
using Microsoft.Scripting.Actions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Interpreter;
    
namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    
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

        private MSA.ParameterExpression[]/*!*/ DefineParameters(AstGenerator/*!*/ gen, ScopeBuilder/*!*/ scope) {
            
            // user defined locals/args:
            MSA.ParameterExpression[] parameters = DefinedScope.TransformParameters(_parameters, HiddenParameterCount);
            scope.AddVisibleParameters(parameters, HiddenParameterCount);

            parameters[0] = Ast.Parameter(typeof(object), "#self");

            if (_parameters.Block != null) {
                // map user defined proc parameter to the special param #1:
                parameters[1] = _parameters.Block.TransformBlockParameterDefinition();
            } else {
                parameters[1] = Ast.Parameter(typeof(Proc), "#block");
            }

            return parameters;
        }

        internal MSA.LambdaExpression/*!*/ TransformBody(AstGenerator/*!*/ gen, RubyScope/*!*/ declaringScope, RubyModule/*!*/ declaringModule) {
            string encodedName = RubyExceptionData.EncodeMethodName(_name, gen.SourcePath, Location);
            
            ScopeBuilder scope = new ScopeBuilder();
            
            MSA.ParameterExpression[] parameters = DefineParameters(gen, scope);
            var currentMethodVariable = scope.DefineHiddenVariable("#method", typeof(RubyMethodInfo));
            var rfcVariable = scope.DefineHiddenVariable("#rfc", typeof(RuntimeFlowControl));
            var scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyMethodScope));
            var selfParameter = parameters[0];
            var blockParameter = parameters[1];

            gen.EnterMethodDefinition(
                scope,
                selfParameter,
                scopeVariable,
                blockParameter,
                rfcVariable,
                _name,
                _parameters
            );

            DefinedScope.TransformLocals(scope);

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

            MSA.ParameterExpression unwinder = scope.DefineHiddenVariable("#unwinder", typeof(Exception));
            
            MSA.Expression body = AstUtils.Try(
                profileStart,

                // scope initialization:
                Ast.Assign(rfcVariable, Methods.CreateRfcForMethod.OpCall(AstUtils.Convert(blockParameter, typeof(Proc)))),
                Ast.Assign(scopeVariable, Methods.CreateMethodScope.OpCall(
                    scope.VisibleVariables(), 
                    Ast.Constant(declaringScope, typeof(RubyScope)),
                    Ast.Constant(declaringModule, typeof(RubyModule)), 
                    Ast.Constant(_name),
                    rfcVariable, selfParameter, blockParameter,
                    EnterInterpretedFrameExpression.Instance
                )),
            
                _parameters.TransformOptionalsInitialization(gen),
                traceCall,
                Body.TransformResult(gen, ResultOperation.Return)
            ).Filter(unwinder, Methods.IsMethodUnwinderTargetFrame.OpCall(scopeVariable, unwinder),
                Ast.Return(gen.ReturnLabel, Methods.GetMethodUnwinderReturnValue.OpCall(unwinder))
            ).Finally(  
                // leave frame:
                Methods.LeaveMethodFrame.OpCall(rfcVariable),
                LeaveInterpretedFrameExpression.Instance,
                profileEnd,
                traceReturn
            );

            body = gen.AddReturnTarget(scope.CreateScope(body));
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
                (_target != null) ? _target.TransformRead(gen) : gen.CurrentSelfVariable,  // target
                gen.CurrentScopeVariable,
                Ast.Constant(new RubyMethodBody(gen.Context, this, gen.Document, gen.Encoding))
            );
        }
    }
}
