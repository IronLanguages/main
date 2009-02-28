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

        private MSA.Expression/*!*/ TransformBody(AstGenerator/*!*/ gen, MSA.Expression/*!*/ methodDefinitionVariable) {
            string encodedName = RubyExceptionData.EncodeMethodName(gen.SourceUnit, _name, Location);
            
            ScopeBuilder scope = new ScopeBuilder();
            MSA.Expression parentScope = gen.CurrentScopeVariable;

            MSA.ParameterExpression[] parameters = DefineParameters(gen, scope);
            MSA.Expression currentMethodVariable = scope.DefineHiddenVariable("#method", typeof(RubyMethodInfo));
            MSA.Expression rfcVariable = scope.DefineHiddenVariable("#rfc", typeof(RuntimeFlowControl));
            MSA.Expression scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyMethodScope));
            MSA.Expression selfParameter = parameters[0];
            MSA.Expression blockParameter = parameters[1];

            gen.EnterMethodDefinition(
                scope,
                selfParameter,
                scopeVariable,
                blockParameter,
                rfcVariable,
                currentMethodVariable,
                _name,
                _parameters
            );

            DefinedScope.TransformLocals(scope);

            MSA.ParameterExpression unwinder = scope.DefineHiddenVariable("#unwinder", typeof(MethodUnwinder));

            MSA.Expression body = AstFactory.MakeUserMethodBody(
                gen, Location.End.Line,
                blockParameter,
                rfcVariable,
                unwinder,
                Ast.Block(
                    Ast.Assign(currentMethodVariable, methodDefinitionVariable),

                    Ast.Assign(scopeVariable, Methods.CreateMethodScope.OpCall(
                        scope.VisibleVariables(), parentScope, currentMethodVariable, rfcVariable, selfParameter, blockParameter)
                    ),
                
                    _parameters.TransformOptionalsInitialization(gen),
                    gen.TraceEnabled ? Methods.TraceMethodCall.OpCall(scopeVariable, Ast.Convert(AstUtils.Constant(gen.SourceUnit.Path), typeof(string)), AstUtils.Constant(Location.Start.Line)) : AstUtils.Empty(),
                    Body.TransformResult(gen, ResultOperation.Return),
                    AstUtils.Empty()
                ),
                ResultOperation.Return,
                (gen.Profiler != null) ? gen.Profiler.GetTickIndex(encodedName) : -1,
                (gen.Profiler != null) ? scope.DefineHiddenVariable("#stamp", typeof(long)) : null,
                gen.ReturnLabel
            );

            body = gen.AddReturnTarget(scope.CreateScope(body));
            gen.LeaveMethodDefinition();

            return CreateLambda(
                encodedName, 
                parameters, 
                body
            );
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

                return Ast.Lambda(
                    RubyMethodInfo.ParamsArrayDelegateType,
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
            MSA.Expression methodDefinitionVariable = gen.CurrentScope.DefineHiddenVariable(
                "#method_" + _name, typeof(RubyMethodInfo)
            );

            return Methods.MethodDefined.OpCall(
                Ast.Assign(methodDefinitionVariable,
                    Methods.DefineMethod.OpCall(
                        (_target != null) ? _target.TransformRead(gen) : gen.CurrentSelfVariable,  // target
                        AstUtils.Constant((gen.SavingToDisk) ? (object)new Serializable(this) : this), 
                        gen.CurrentScopeVariable,
                        AstUtils.Constant(_target != null),                                // isSingleton?
                        AstUtils.Constant(_name),
                        TransformBody(gen, methodDefinitionVariable),
                        AstUtils.Constant(_parameters.MandatoryCount),
                        AstUtils.Constant(_parameters.OptionalCount),
                        AstUtils.Constant(_parameters.Array != null)                       // hasUnsplatParameter
                    )
                )
            );
        }

        internal sealed class Serializable : IExpressionSerializable {
            private readonly MethodDeclaration/*!*/ _method;

            public MethodDeclaration/*!*/ Method {
                get { return _method; }
            }

            public Serializable(MethodDeclaration/*!*/ method) {
                _method = method;
            }

            public MSA.Expression/*!*/ CreateExpression() {
                // TODO: serialize source code:
                return AstUtils.Constant(null);       
            }
        }
    }
}
