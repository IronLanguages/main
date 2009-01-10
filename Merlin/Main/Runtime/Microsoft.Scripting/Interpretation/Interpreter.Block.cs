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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpretation {
    /// <summary>
    /// Interpreter partial class. This part contains interpretation code for lambdas.
    /// </summary>
    public static partial class Interpreter {
        private static int _DelegateCounter;
        private static WeakDictionary<LambdaExpression, MethodInfo> _Delegates;

        private static object DoExecute(InterpreterState state, LambdaExpression lambda) {
            object ret = Interpreter.Interpret(state, lambda.Body);

            ControlFlow cf = ret as ControlFlow;
            if (cf != null) {
                return cf.Value;
            } else {
                return ret;
            }
        }

        /// <summary>
        /// Called by the code:LambdaInvoker.Invoke from the delegate generated below by
        /// code:GetDelegateForInterpreter.
        /// 
        /// This method must repackage arguments to match the lambdas signature, which
        /// may mean repackaging the parameter arrays.
        /// 
        /// Input are two arrays - regular arguments passed into the generated delegate,
        /// and (if the delegate had params array), the parameter array, separately.
        /// </summary>
        internal static object InterpretLambda(InterpreterState lexicalParentState, LambdaExpression lambda, object[] args) {
            Assert.NotNull(lexicalParentState, lambda, args);
            
            var state = InterpreterState.Current.Update(
                (caller) => lexicalParentState.CreateForLambda(lambda, caller, args)
            );

            try {
                object result = Interpret(state, lambda.Body);

                var cf = result as ControlFlow;
                if (cf != null) {
                    return (cf.Kind == ControlFlowKind.Yield) ? cf.Value : null;
                }

                return result;
            } finally {
                InterpreterState.Current.Value = state.Caller;
            }
        }

        /// <summary>
        /// Gets the delegate associated with the LambdaExpression.
        /// Either it uses cached MethodInfo and creates delegate from it, or it will generate
        /// completely new dynamic method, store it in a cache and use it to create the delegate.
        /// </summary>
        private static Delegate GetDelegateForInterpreter(InterpreterState state, LambdaExpression lambda) {
            MethodInfo method;
            if (!LambdaInvoker.TryGetGenericInvokeMethod(lambda.Parameters.Count, out method) || HasByRefParameter(lambda)) {
                return GenerateDelegateForInterpreter(state, lambda);
            }

            Type[] signature = GetSignature(lambda);
            method = method.MakeGenericMethod(signature);
            return ReflectionUtils.CreateDelegate(method, lambda.Type, new LambdaInvoker(lambda, state));
        }

        private static bool HasByRefParameter(LambdaExpression lambda) {
            for (int i = 0; i < lambda.Parameters.Count; i++) {
                if (lambda.Parameters[i].IsByRef) {
                    return true;
                }
            }
            return false;
        }

        private static Type[] GetSignature(LambdaExpression lambda) {
            Type[] result = new Type[1 + lambda.Parameters.Count];
            for (int i = 0; i < lambda.Parameters.Count; i++) {
                result[i] = lambda.Parameters[i].Type;
            }
            result[lambda.Parameters.Count] = lambda.ReturnType;
            return result;
        }

        private static Delegate GenerateDelegateForInterpreter(InterpreterState state, LambdaExpression lambda) {
            if (_Delegates == null) {
                Interlocked.CompareExchange<WeakDictionary<LambdaExpression, MethodInfo>>(
                    ref _Delegates,
                    new WeakDictionary<LambdaExpression, MethodInfo>(),
                    null
                );
            }

            bool found;
            MethodInfo method;

            //
            // LOCK to find the MethodInfo
            //

            lock (_Delegates) {
                found = _Delegates.TryGetValue(lambda, out method);
            }

            if (!found) {
                method = CreateDelegateForInterpreter(lambda.Type);

                //
                // LOCK to store the MethodInfo
                // (and maybe find one added while we were creating new delegate, in which case
                // throw away the new one and use the one from the cache.
                //

                lock (_Delegates) {
                    MethodInfo conflict;
                    if (!_Delegates.TryGetValue(lambda, out conflict)) {
                        _Delegates.Add(lambda, method);
                    } else {
                        method = conflict;
                    }
                }
            }

            return ReflectionUtils.CreateDelegate(method, lambda.Type, new LambdaInvoker(lambda, state));
        }

        /// <summary>
        /// The core of the interpreter, calling back onto itself via delegates.
        /// </summary>
        private static MethodInfo CreateDelegateForInterpreter(Type type) {
            Debug.Assert(type != typeof(Delegate) && typeof(Delegate).IsAssignableFrom(type));

            //
            // Get the desired signature
            //
            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo[] parameters = invoke.GetParameters();

            string name = "Interpreted_" + Interlocked.Increment(ref _DelegateCounter);

            Type[] signature = CreateInterpreterSignature(parameters);
            DynamicILGen il = Snippets.Shared.CreateDynamicMethod(name, invoke.ReturnType, signature, false);

            // Collect all arguments received by the delegate into an array
            // and pass them to the Interpreter along with the LambdaInvoker

            // LambdaInvoker
            il.EmitLoadArg(0);
            int count = parameters.Length;

            // Create the array
            il.EmitInt(count);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < count; i++) {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitLoadArg(i + 1);
                EmitExplicitCast(il, parameters[i].ParameterType, typeof(object));
                il.EmitStoreElement(typeof(object));
            }

            // Call back to interpreter
            il.EmitCall(LambdaInvoker.GetInvokeMethod());

            // Cast back to the delegate return type
            EmitExplicitCast(il, typeof(object), invoke.ReturnType);

            // And return whatever the result was.
            il.Emit(OpCodes.Ret);

            //
            // We are done (for now), finish the MethodInfo
            //
            return il.Finish();
        }

        private static void EmitExplicitCast(ILGen il, Type from, Type to) {
            if (!il.TryEmitExplicitCast(from, to)) {
                throw new ArgumentException(String.Format("Cannot cast from '{0}' to '{1}'", from, to));
            }
        }

        private static Type[] CreateInterpreterSignature(ParameterInfo[] parameters) {
            Type[] signature = new Type[parameters.Length + 1];

            // First one is always LambdaInvoker.
            signature[0] = typeof(LambdaInvoker);

            // The rest is copied from the parameter infos.
            for (int i = 0; i < parameters.Length; i++) {
                signature[i + 1] = parameters[i].ParameterType;
            }

            return signature;
        }
    }
}
