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

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using System;

namespace Microsoft.Scripting.Interpretation {
    /// <summary>
    /// Helper class used by the interpreter to package lambda as a delegate,
    /// allow it being called, and then resume interpretation.
    /// </summary>
    public class LambdaInvoker {
        private readonly LambdaExpression _lambda;
        private readonly InterpreterState _state;

        internal LambdaInvoker(LambdaExpression lambda, InterpreterState state) {
            Assert.NotNull(lambda, state);

            _lambda = lambda;
            _state = state;
        }

        private static MethodInfo _invoke;
        private static MethodInfo _invoke0;
        private static MethodInfo _invoke1;
        private static MethodInfo _invoke2;
        private static MethodInfo _invoke3;
        private static MethodInfo _invoke4;
        private static MethodInfo _invoke5;
        private static MethodInfo _invoke6;
        private static MethodInfo _invoke7;

        internal static MethodInfo GetInvokeMethod() {
            return GetMethod(ref _invoke, "Invoke");
        }

        /// <summary>
        /// Selects an Invoke method declared in this class given the number of parameters.
        /// Note that the selected overload could only be used if no by-ref parameters are needed in the signature.
        /// Returns false if there is no overload with that many parameters.
        /// </summary>
        internal static bool TryGetGenericInvokeMethod(int paramCount, out MethodInfo genericMethod) {
            switch (paramCount) {
                case 0: genericMethod = GetMethod(ref _invoke0, "Invoke0"); return true;
                case 1: genericMethod = GetMethod(ref _invoke1, "Invoke1"); return true;
                case 2: genericMethod = GetMethod(ref _invoke2, "Invoke2"); return true;
                case 3: genericMethod = GetMethod(ref _invoke3, "Invoke3"); return true;
                case 4: genericMethod = GetMethod(ref _invoke4, "Invoke4"); return true;
                case 5: genericMethod = GetMethod(ref _invoke5, "Invoke5"); return true;
                case 6: genericMethod = GetMethod(ref _invoke6, "Invoke6"); return true;
                case 7: genericMethod = GetMethod(ref _invoke7, "Invoke7"); return true;

                default:
                    genericMethod = null;
                    return false;
            }
        }

        private static MethodInfo GetMethod(ref MethodInfo method, string name) {
            if (method == null) {
                Interlocked.CompareExchange(ref method, typeof(LambdaInvoker).GetMethod(name), null);
            }
            return method;
        }

        public object Invoke(object[] args) {
            return Interpreter.InterpretLambda(_state, _lambda, args);
        }

        public TResult Invoke0<TResult>() {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, ArrayUtils.EmptyObjects);
        }

        public TResult Invoke1<T1, TResult>(T1 arg1) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1 });
        }

        public TResult Invoke2<T1, T2, TResult>(T1 arg1, T2 arg2) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2 });
        }

        public TResult Invoke3<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2, arg3 });
        }

        public TResult Invoke4<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2, arg3, arg4 });
        }

        public TResult Invoke5<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }

        public TResult Invoke6<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }

        public TResult Invoke7<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            return (TResult)Interpreter.InterpretLambda(_state, _lambda, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }
    }

    internal sealed class GeneratorInvoker {
        private readonly GeneratorExpression _generator;
        private readonly InterpreterState _state;

#if OBSOLETE
        // Arbitrary constant, chosen to be different from "Finished"
        private const int InterpretingGenerator = GeneratorRewriter.Finished + 1;
#endif
        internal GeneratorInvoker(GeneratorExpression generator, InterpreterState state) {
            _generator = generator;
            _state = state;
        }

        /// <summary>
        /// Triggers interpretation of the Lambda
        /// </summary>
        public void Invoke<T>(ref int state, ref T current) {
            throw new NotImplementedException();
#if OBSOLETE
            object res = Interpreter.ExecuteGenerator(_state, _generator.Body);
            ControlFlow cf = res as ControlFlow;
            if (cf != null && cf.Kind == ControlFlowKind.Yield && _state.CurrentYield != null) {
                current = (T)cf.Value;
                state = InterpretingGenerator;
                return;
            }

            //current = default(T);
            state = GeneratorRewriter.Finished;
#endif
        }
    }
}
