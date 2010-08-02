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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using RuntimeHelpers = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The builder for creating the LambdaExpression node.
    /// 
    /// Since the nodes require that parameters and variables are created
    /// before hand and then passed to the factories creating LambdaExpression
    /// this builder keeps track of the different pieces and at the end creates
    /// the LambdaExpression.
    /// 
    /// TODO: This has some functionality related to CodeContext that should be
    /// removed, in favor of languages handling their own local scopes
    /// </summary>
    public class LambdaBuilder {
        private readonly List<ParameterExpression> _locals = new List<ParameterExpression>();
        private List<ParameterExpression> _params = new List<ParameterExpression>();
        private readonly List<KeyValuePair<ParameterExpression, bool>> _visibleVars = new List<KeyValuePair<ParameterExpression, bool>>();
        private string _name;
        private Type _returnType;
        private ParameterExpression _paramsArray;
        private Expression _body;
        private bool _dictionary;
        private bool _visible = true;
        private bool _completed;

        private static int _lambdaId; //for generating unique lambda name

        internal LambdaBuilder(string name, Type returnType) {
            _name = name;
            _returnType = returnType;
        }

        /// <summary>
        /// The name of the lambda.
        /// Currently anonymous/unnamed lambdas are not allowed.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _name = value;
            }
        }

        /// <summary>
        /// Return type of the lambda being created.
        /// </summary>
        public Type ReturnType {
            get {
                return _returnType;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _returnType = value;
            }
        }

        /// <summary>
        /// List of lambda's local variables for direct manipulation.
        /// </summary>
        public List<ParameterExpression> Locals {
            get {
                return _locals;
            }
        }

        /// <summary>
        /// List of lambda's parameters for direct manipulation
        /// </summary>
        public List<ParameterExpression> Parameters {
            get {
                return _params;
            }
        }

        /// <summary>
        /// The params array argument, if any.
        /// </summary>
        public ParameterExpression ParamsArray {
            get {
                return _paramsArray;
            }
        }

        /// <summary>
        /// The body of the lambda. This must be non-null.
        /// </summary>
        public Expression Body {
            get {
                return _body;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _body = value;
            }
        }

        /// <summary>
        /// The generated lambda should have dictionary of locals
        /// instead of allocating them directly on the CLR stack.
        /// </summary>
        public bool Dictionary {
            get {
                return _dictionary;
            }
            set {
                _dictionary = value;
            }
        }

        /// <summary>
        /// The scope is visible (default). Invisible if false.
        /// </summary>
        public bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        }
        
        public List<ParameterExpression> GetVisibleVariables() {
            var vars = new List<ParameterExpression>(_visibleVars.Count);
            foreach (var v in _visibleVars) {
                if (EmitDictionary || v.Value) {
                    vars.Add(v.Key);
                }
            }
            return vars;
        }

        /// <summary>
        /// Creates a parameter on the lambda with a given name and type.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public ParameterExpression Parameter(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");            
            ParameterExpression result = Expression.Parameter(type, name);
            _params.Add(result);
            _visibleVars.Add(new KeyValuePair<ParameterExpression, bool>(result, false));
            return result;
        }

        /// <summary>
        /// Creates a parameter on the lambda with a given name and type.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public ParameterExpression ClosedOverParameter(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ParameterExpression result = Expression.Parameter(type, name);
            _params.Add(result);
            _visibleVars.Add(new KeyValuePair<ParameterExpression, bool>(result, true));
            return result;
        }

        /// <summary>
        /// adds existing parameter to the lambda.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public void AddParameters(params ParameterExpression[] parameters) {
            _params.AddRange(parameters);
        }

        /// <summary>
        /// Creates a hidden parameter on the lambda with a given name and type.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public ParameterExpression CreateHiddenParameter(string name, Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            ParameterExpression result = Expression.Parameter(type, name);
            _params.Add(result);
            return result;
        }

        /// <summary>
        /// Creates a params array argument on the labmda.
        /// 
        /// The params array argument is added to the signature immediately. Before the lambda is
        /// created, the builder validates that it is still the last (since the caller can modify
        /// the order of parameters explicitly by maniuplating the parameter list)
        /// </summary>
        public ParameterExpression CreateParamsArray(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type.IsArray, "type");
            ContractUtils.Requires(type.GetArrayRank() == 1, "type");
            ContractUtils.Requires(_paramsArray == null, "type", "Already have parameter array");

            return _paramsArray = Parameter(type, name);
        }

        /// <summary>
        /// Creates a local variable with specified name and type.
        /// TODO: simplify by pushing logic into callers
        /// </summary>
        public Expression ClosedOverVariable(Type type, string name) {
            ParameterExpression result = Expression.Variable(type, name);
            _locals.Add(result);
            _visibleVars.Add(new KeyValuePair<ParameterExpression, bool>(result, true));
            return result;
        }

        /// <summary>
        /// Creates a local variable with specified name and type.
        /// TODO: simplify by pushing logic into callers
        /// </summary>
        public Expression Variable(Type type, string name) {
            ParameterExpression result = Expression.Variable(type, name);
            _locals.Add(result);
            _visibleVars.Add(new KeyValuePair<ParameterExpression, bool>(result, false));
            return result;
        }

        /// <summary>
        /// Creates a temporary variable with specified name and type.
        /// </summary>
        public ParameterExpression HiddenVariable(Type type, string name) {
            ParameterExpression result = Expression.Variable(type, name);
            _locals.Add(result);
            return result;
        }

        /// <summary>
        /// Adds the temporary variable to the list of variables maintained
        /// by the builder. This is useful in cases where the variable is
        /// created outside of the builder.
        /// </summary>
        public void AddHiddenVariable(ParameterExpression temp) {
            ContractUtils.RequiresNotNull(temp, "temp");
            _locals.Add(temp);
        }

        /// <summary>
        /// Creates the LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <param name="lambdaType">Desired type of the lambda. </param>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeLambda(Type lambdaType) {
            Validate();
            EnsureSignature(lambdaType);

            LambdaExpression lambda = Expression.Lambda(
                lambdaType,
                AddDefaultReturn(MakeBody()),
                _name + "$" + Interlocked.Increment(ref _lambdaId),
                new ReadOnlyCollectionBuilder<ParameterExpression>(_params)
            );

            // The builder is now completed
            _completed = true;

            return lambda;
        }

        /// <summary>
        /// Creates the LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeLambda() {
            ContractUtils.Requires(_paramsArray == null, "Paramarray lambdas require explicit delegate type");
            Validate();

            LambdaExpression lambda = Expression.Lambda(
                GetLambdaType(_returnType, _params),
                AddDefaultReturn(MakeBody()), 
                _name + "$" + Interlocked.Increment(ref _lambdaId),
                _params
            );

            // The builder is now completed
            _completed = true;

            return lambda;
        }


        /// <summary>
        /// Creates the generator LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeGenerator(LabelTarget label, Type lambdaType) {
            Validate();
            EnsureSignature(lambdaType);

            LambdaExpression lambda = Utils.GeneratorLambda(
                lambdaType,
                label,
                MakeBody(),
                _name + "$" + Interlocked.Increment(ref _lambdaId), 
                _params
            );

            // The builder is now completed
            _completed = true;

            return lambda;
        }

        /// <summary>
        /// Fixes up lambda body and parameters to match the signature of the given delegate if needed.
        /// </summary>
        /// <param name="delegateType"></param>
        private void EnsureSignature(Type delegateType) {
            System.Diagnostics.Debug.Assert(_params != null, "must have parameter list here");

            //paramMapping is the dictionary where we record how we want to map parameters
            //the key is the parameter, the value is the expression it should be redirected to
            //so far the parameter can only be redirected to itself (means no change needed) or to
            //a synthetic variable that is added to the lambda when the original parameter has no direct
            //parameter backing in the delegate signature
            // Example:
            //      delegate siganture      del(x, params y[])
            //      lambda signature        lambda(a, b, param n[])
            //          
            //  for the situation above the mapping will be  <a, x>, <b, V1>, <n, V2>
            //  where V1 and V2 are synthetic variables and initialized as follows -  V1 = y[0] , V2 = {y[1], y[2],... y[n]}
            ParameterInfo[] delegateParams = delegateType.GetMethod("Invoke").GetParameters();

            bool delegateHasParamarray = delegateParams.Length > 0 && delegateParams[delegateParams.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
            bool lambdaHasParamarray = ParamsArray != null;

            if (lambdaHasParamarray && !delegateHasParamarray) {
                throw new ArgumentException("paramarray lambdas must have paramarray delegate type");
            }

            int copy = delegateHasParamarray ? delegateParams.Length - 1 : delegateParams.Length;
            int unwrap = _params.Count - copy;
            if (lambdaHasParamarray) unwrap--;

            // Lambda must have at least as many parameters as the delegate, not counting the paramarray
            if (unwrap < 0) {
                throw new ArgumentException("lambda does not have enough parameters");
            }

            // shortcircuit if no rewrite is needed.
            if (!delegateHasParamarray) {
                bool needRewrite = false;
                for (int i = 0; i < copy; i++) {
                    if (_params[i].Type != delegateParams[i].ParameterType) {
                        needRewrite = true;
                    }
                }

                if (!needRewrite) {
                    return;
                }
            }

            List<ParameterExpression> newParams = new List<ParameterExpression>(delegateParams.Length);
            List<ParameterExpression> backingVars = new List<ParameterExpression>();
            List<Expression> preambuleExpressions = new List<Expression>();
            Dictionary<ParameterExpression, ParameterExpression> paramMapping = new Dictionary<ParameterExpression, ParameterExpression>();

            for (int i = 0; i < copy; i++) {
                // map to a converted variable
                if (_params[i].Type != delegateParams[i].ParameterType) {
                    ParameterExpression newParameter = Expression.Parameter(delegateParams[i].ParameterType, delegateParams[i].Name);
                    ParameterExpression mappedParameter = _params[i];
                    ParameterExpression backingVariable = Expression.Variable(mappedParameter.Type, mappedParameter.Name);

                    newParams.Add(newParameter);
                    backingVars.Add(backingVariable);
                    paramMapping.Add(mappedParameter, backingVariable);
                    preambuleExpressions.Add(
                        Expression.Assign(
                            backingVariable,
                            Expression.Convert(
                                newParameter,
                                mappedParameter.Type
                            )
                        )
                    );
                } else {
                    //use the same parameter expression
                    newParams.Add(_params[i]);
                    paramMapping.Add(_params[i], _params[i]);
                }
            }

            if (delegateHasParamarray) {
                ParameterInfo delegateParamarrayPi = delegateParams[delegateParams.Length - 1];
                ParameterExpression delegateParamarray = Expression.Parameter(delegateParamarrayPi.ParameterType, delegateParamarrayPi.Name);

                newParams.Add(delegateParamarray);

                //unwarap delegate paramarray into variables and map parameters to the variables
                for (int i = 0; i < unwrap; i++) {
                    ParameterExpression mappedParameter = _params[copy + i];
                    ParameterExpression backingVariable = Expression.Variable(mappedParameter.Type, mappedParameter.Name);

                    backingVars.Add(backingVariable);
                    paramMapping.Add(mappedParameter, backingVariable);
                    preambuleExpressions.Add(
                        Expression.Assign(
                            backingVariable,
                            AstUtils.Convert(
                                Expression.ArrayAccess(
                                    delegateParamarray,
                                    AstUtils.Constant(i)
                                ),
                                mappedParameter.Type
                             )
                        )
                    );
                }

                //lambda's paramarray should get elements from the delegate paramarray after skipping those that we unwrapped.
                if (lambdaHasParamarray) {
                    ParameterExpression mappedParameter = _paramsArray;
                    ParameterExpression backingVariable = Expression.Variable(mappedParameter.Type, mappedParameter.Name);

                    backingVars.Add(backingVariable);
                    paramMapping.Add(mappedParameter, backingVariable);

                    // Call the helper
                    MethodInfo shifter = typeof(RuntimeHelpers).GetMethod("ShiftParamsArray");
                    shifter = shifter.MakeGenericMethod(delegateParamarrayPi.ParameterType.GetElementType());

                    preambuleExpressions.Add(
                        Expression.Assign(
                            backingVariable,
                            AstUtils.Convert(
                                Expression.Call(
                                    shifter,
                                    delegateParamarray,
                                    AstUtils.Constant(unwrap)
                                ),
                                mappedParameter.Type
                            )
                        )
                    );
                }
            }


            Expression newBody = new LambdaParameterRewriter(paramMapping).Visit(_body);

            preambuleExpressions.Add(newBody);
            _body = Expression.Block(preambuleExpressions);

            _paramsArray = null;
            _locals.AddRange(backingVars);
            _params = newParams;

            for (int i = 0; i < _visibleVars.Count; i++) {
                ParameterExpression p = _visibleVars[i].Key as ParameterExpression;
                ParameterExpression v;
                if (p != null && paramMapping.TryGetValue(p, out v)) {
                    _visibleVars[i] = new KeyValuePair<ParameterExpression, bool>(v, _visibleVars[i].Value);
                }
            }
        }


        /// <summary>
        /// Validates that the builder has enough information to create the lambda.
        /// </summary>
        private void Validate() {
            if (_completed) {
                throw new InvalidOperationException("The builder is closed");
            }
            if (_returnType == null) {
                throw new InvalidOperationException("Return type is missing");
            }
            if (_name == null) {
                throw new InvalidOperationException("Name is missing");
            }
            if (_body == null) {
                throw new InvalidOperationException("Body is missing");
            }

            if (_paramsArray != null &&
                (_params.Count == 0 || _params[_params.Count -1] != _paramsArray)) {
                throw new InvalidOperationException("The params array parameter is not last in the parameter list");
            }
        }

        private bool EmitDictionary {
            get { return _dictionary; }
        }

        private Expression MakeBody() {
            Expression body = _body;
            
            // wrap a scope if needed
            if (_locals != null && _locals.Count > 0) {
                body = Expression.Block(new ReadOnlyCollection<ParameterExpression>(_locals.ToArray()), body);
            }

            return body;
        }

        // Add a default return value if needed
        private Expression AddDefaultReturn(Expression body) {
            if (body.Type == typeof(void) && _returnType != typeof(void)) {
                body = Expression.Block(body, Utils.Default(_returnType));
            }
            return body;
        }

        private static Type GetLambdaType(Type returnType, IList<ParameterExpression> parameterList) {
            ContractUtils.RequiresNotNull(returnType, "returnType");

            bool action = returnType == typeof(void);
            int paramCount = parameterList == null ? 0 : parameterList.Count;

            Type[] typeArgs = new Type[paramCount + (action ? 0 : 1)];
            for (int i = 0; i < paramCount; i++) {
                ContractUtils.RequiresNotNull(parameterList[i], "parameter");
                typeArgs[i] = parameterList[i].Type;
            }

            Type delegateType;
            if (action)
                delegateType = Expression.GetActionType(typeArgs);
            else {
                typeArgs[paramCount] = returnType;
                delegateType = Expression.GetFuncType(typeArgs);
            }
            return delegateType;
        }
    }

    public static partial class Utils {
        /// <summary>
        /// Creates new instance of the LambdaBuilder with the specified name and return type.
        /// </summary>
        /// <param name="returnType">Return type of the lambda being built.</param>
        /// <param name="name">Name for the lambda being built.</param>
        /// <returns>new LambdaBuilder instance</returns>
        public static LambdaBuilder Lambda(Type returnType, string name) {
            return new LambdaBuilder(name, returnType);
        }
    }
}

namespace Microsoft.Scripting.Runtime {
    public static partial class ScriptingRuntimeHelpers {
        /// <summary>
        /// Used by prologue code that is injected in lambdas to ensure that delegate signature matches what 
        /// lambda body expects. Such code typically unwraps subset of the params array manually, 
        /// but then passes the rest in bulk if lambda body also expects params array.
        /// 
        /// This calls ArrayUtils.ShiftLeft, but performs additional checks that
        /// ArrayUtils.ShiftLeft assumes.
        /// </summary>
        public static T[] ShiftParamsArray<T>(T[] array, int count) {
            if (array != null && array.Length > count) {
                return ArrayUtils.ShiftLeft(array, count);
            } else {
                return new T[0];
            }
        }
    }
}
