/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Interpreter {
    internal partial class LightLambda {
        internal static StrongBox<object>[] EmptyClosure = new StrongBox<object>[0];

        private readonly Interpreter _interpreter;
        private readonly StrongBox<object>[] _closure;

        // Adaptive compilation support
        private readonly LightDelegateCreator _delegateCreator;
        private Delegate _compiled;

        internal LightLambda(Interpreter interpreter) {
            this._interpreter = interpreter;
            this._closure = EmptyClosure;
        }

        internal LightLambda(Interpreter interpreter, StrongBox<object>[] closure, LightDelegateCreator delegateCreator) {
            this._interpreter = interpreter;
            this._closure = closure;
            this._delegateCreator = delegateCreator;
        }

        /// <summary>
        /// Set by LightDelegateCreator once the delegate is compiled.
        /// </summary>
        internal Delegate Compiled {
            get { return _compiled; }
            set { _compiled = value; }
        }

        /// <summary>
        /// Used by LightDelegateCreator to set the delegate.
        /// </summary>
        internal StrongBox<object>[] Closure {
            get { return _closure; }
        }

        private InterpretedFrame PrepareToRun() {
            if (_delegateCreator != null) {
                _delegateCreator.UpdateExecutionCount();
            }

            return new InterpretedFrame(_interpreter, _closure);
        }

        private static MethodInfo GetRunMethod(Type delegateType) {
            // insert a cache here?
            var method = delegateType.GetMethod("Invoke");
            var paramInfos = method.GetParameters();
            Type[] paramTypes;
            string name = "Run";
            if (paramInfos.Length > MaxParameters) return null;

            if (method.ReturnType == typeof(void)) {
                name += "Void";
                paramTypes = new Type[paramInfos.Length];
            } else {
                paramTypes = new Type[paramInfos.Length + 1];
                paramTypes[paramTypes.Length - 1] = method.ReturnType;
            }

            MethodInfo runMethod;

            if (method.ReturnType == typeof(void) && paramTypes.Length == 2 && 
                paramInfos[0].ParameterType.IsByRef && paramInfos[1].ParameterType.IsByRef)
            {
                runMethod = typeof(LightLambda).GetMethod("RunVoidRef2", BindingFlags.NonPublic | BindingFlags.Instance);
                paramTypes[0] = paramInfos[0].ParameterType.GetElementType();
                paramTypes[1] = paramInfos[1].ParameterType.GetElementType();
            } else if(method.ReturnType == typeof(void) && paramTypes.Length == 0) {
                return typeof(LightLambda).GetMethod("RunVoid0", BindingFlags.NonPublic | BindingFlags.Instance);
            } else if (paramInfos.Length < LightLambda.MaxParameters) {
                for (int i = 0; i < paramInfos.Length; i++) {
                    paramTypes[i] = paramInfos[i].ParameterType;
                    if (paramTypes[i].IsByRef) return null;
                }

                runMethod = typeof(LightLambda).GetMethod(name + paramInfos.Length, BindingFlags.NonPublic | BindingFlags.Instance);
            } else {
                return null;
            }
            return runMethod.MakeGenericMethod(paramTypes);
        }

        //TODO enable sharing of these custom delegates
        private Delegate CreateCustomDelegate(Type delegateType) {
            var method = delegateType.GetMethod("Invoke");
            var paramInfos = method.GetParameters();
            var parameters = new ParameterExpression[paramInfos.Length];
            for (int i = 0; i < paramInfos.Length; i++) {
                parameters[i] = Expression.Parameter(paramInfos[i].ParameterType, paramInfos[i].Name);
            }

            var data = Expression.NewArrayInit(typeof(object), parameters);
            var self = AstUtils.Constant(this);
            var runMethod = typeof(LightLambda).GetMethod("Run");
            var body = Expression.Convert(Expression.Call(self, runMethod, data), method.ReturnType);
            var lambda = Expression.Lambda(delegateType, body, parameters);
            return lambda.Compile();
        }


        internal Delegate MakeDelegate(Type delegateType) {
            var method = GetRunMethod(delegateType);
            if (method == null) {
                return CreateCustomDelegate(delegateType);
            }
            return Delegate.CreateDelegate(delegateType, this, method);
        }

        internal void RunVoidRef2<T0, T1>(ref T0 arg0, ref T1 arg1) {
            if (_compiled != null) {
                ((ActionRef<T0, T1>)_compiled)(ref arg0, ref arg1);
                return;
            }

            var frame = PrepareToRun();
            // copy in and copy out for today...
            frame.Data[0] = arg0;
            frame.Data[1] = arg1;
            frame.BoxLocals();
            var ret = _interpreter.Run(frame);
            arg0 = (T0)frame.Data[0];
            arg1 = (T1)frame.Data[1];
        }

        
        public object Run(params object[] arguments) {
            if (_compiled != null) {
                return _compiled.DynamicInvoke(arguments);
            }

            var frame = PrepareToRun();
            for (int i = 0; i < arguments.Length; i++) {
                frame.Data[i] = arguments[i];
            }
            frame.BoxLocals();
            object ret = _interpreter.Run(frame);
            return ret;
        }
    }
}
