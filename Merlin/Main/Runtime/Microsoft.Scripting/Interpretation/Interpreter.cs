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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting.Interpretation")]

namespace Microsoft.Scripting.Interpretation {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class Interpreter {
        #region Entry points

        public static object TopLevelExecute(InterpretedScriptCode scriptCode, params object[] args) {
            ContractUtils.RequiresNotNull(scriptCode, "scriptCode");

            var state = InterpreterState.Current.Update(
                (caller) => InterpreterState.CreateForTopLambda(scriptCode, scriptCode.Code, caller, args)
            );

            try {
                return DoExecute(state, scriptCode.Code);
            } finally {
                InterpreterState.Current.Value = state.Caller;
            }
        }

        internal static object Evaluate(InterpreterState state, Expression expression) {
            object result = Interpret(state, expression);

            if (result is ControlFlow) {
                throw new InvalidOperationException("Invalid expression");
            }

            return result;
        }

        internal static object ExecuteGenerator(InterpreterState state, Expression expression) {
            return Interpret(state, expression);
        }

        #endregion

        /// <summary>
        /// Evaluates expression and checks it for ControlFlow. If it is control flow, returns true,
        /// otherwise returns false.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="node"></param>
        /// <param name="result">Result of the evaluation</param>
        /// <returns>true if control flow, false if not</returns>
        private static bool InterpretAndCheckFlow(InterpreterState state, Expression node, out object result) {
            result = Interpret(state, node);

            return result != ControlFlow.NextForYield && result is ControlFlow;
        }

        /// <summary>
        /// Evaluates an expression and checks to see if the ControlFlow is NextForYield.  If it is then we are currently
        /// searching for the next yield and we need to execute any additional nodes in a larger compound node.
        /// </summary>
        private static bool InterpretAndCheckYield(InterpreterState state, Expression target, out object res) {
            res = Interpret(state, target);
            if (res != ControlFlow.NextForYield) {
                return true;
            }
            return false;
        }

        // Individual expressions and statements

        private static object InterpretConstantExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            ConstantExpression node = (ConstantExpression)expr;
            return node.Value;
        }

        private static object InterpretConditionalExpression(InterpreterState state, Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            object test;

            if (InterpretAndCheckFlow(state, node.Test, out test)) {
                return test;
            }

            if (test == ControlFlow.NextForYield || (bool)test) {
                if (InterpretAndCheckYield(state, node.IfTrue, out test)) {
                    return test;
                }
            }

            return Interpret(state, node.IfFalse);
        }

        private static bool IsInputParameter(ParameterInfo pi) {
            return !pi.IsOut || (pi.Attributes & ParameterAttributes.In) != 0;
        }

        private static object InvokeMethod(InterpreterState state, MethodInfo method, object instance, params object[] parameters) {
            // TODO: Cache !!!
            ReflectedCaller _caller = null;

            if (_caller == null) {
                _caller = ReflectedCaller.Create(method);
            }

            try {
                if (instance == null) {
                    return _caller.Invoke(parameters);
                } else {
                    return _caller.InvokeInstance(instance, parameters);
                }
            } catch (Exception e) {
                // Give the language a chance to associate the interpreter stack trace with the exception.
                //
                // Note that this should be called for any exception caused by any Expression node
                // (for example, integer division by zero). For now, doing it for method calls
                // catches a large portion of the interesting cases (including calls into the language's library assembly).
                state.ScriptCode.LanguageContext.InterpretExceptionThrow(state, e, false);
                throw;
            }
        }

        private static object InterpretInvocationExpression(InterpreterState state, Expression expr) {
            InvocationExpression node = (InvocationExpression)expr;

            // TODO: this should have the same semantics of the compiler
            // in particular, it doesn't handle the case where the left hand
            // side returns a lambda that we need to interpret
            return InterpretMethodCallExpression(state, Expression.Call(node.Expression, node.Expression.Type.GetMethod("Invoke"), ArrayUtils.ToArray(node.Arguments)));
        }

        private static object InterpretIndexExpression(InterpreterState state, Expression expr) {
            var node = (IndexExpression)expr;

            if (node.Indexer != null) {
                return InterpretMethodCallExpression(
                    state,
                    Expression.Call(node.Object, node.Indexer.GetGetMethod(true), node.Arguments)
                );
            }

            if (node.Arguments.Count != 1) {
                var get = node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                return InterpretMethodCallExpression(
                    state,
                    Expression.Call(node.Object, get, node.Arguments)
                );
            }

            object array, index;

            if (InterpretAndCheckFlow(state, node.Object, out array)) {
                return array;
            }
            if (InterpretAndCheckFlow(state, node.Arguments[0], out index)) {
                return index;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return ((Array)array).GetValue((int)index);
        }

        private static object InterpretMethodCallExpression(InterpreterState state, Expression expr) {
            MethodCallExpression methodCall = (MethodCallExpression)expr;
            return InterpretMethodCallExpression(state, expr, methodCall.Method, methodCall.Object, methodCall.Arguments);
        }

        private static object InterpretMethodCallExpression(InterpreterState state, Expression expr,
            MethodInfo method, Expression target, IList<Expression> arguments) {

            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!method.IsStatic) {
                if (InterpretAndCheckFlow(state, target, out instance)) {
                    return instance;
                }
            }

            var parameterInfos = method.GetParameters();

            object[] parameters;
            if (!state.TryGetStackState(expr, out parameters)) {
                parameters = new object[parameterInfos.Length];
            }

            Debug.Assert(parameters.Length == parameterInfos.Length);

            int lastByRefParamIndex = -1;
            var paramAddrs = new EvaluationAddress[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) {
                ParameterInfo info = parameterInfos[i];

                if (info.ParameterType.IsByRef) {
                    lastByRefParamIndex = i;
                    paramAddrs[i] = EvaluateAddress(state, arguments[i]);

                    object value = paramAddrs[i].GetValue(state, !IsInputParameter(info));
                    if (IsInputParameter(info)) {
                        if (value != ControlFlow.NextForYield) {
                            // implict cast?
                            parameters[i] = Cast.Explicit(value, info.ParameterType.GetElementType());
                        }
                    }
                } else if (IsInputParameter(info)) {
                    Expression arg = arguments[i];
                    object argValue = null;
                    if (arg != null) {
                        if (InterpretAndCheckFlow(state, arg, out argValue)) {
                            if (state.CurrentYield != null) {
                                state.SaveStackState(expr, parameters);
                            }

                            return argValue;
                        }
                    }

                    if (argValue != ControlFlow.NextForYield) {
                        parameters[i] = argValue;
                    }
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            try {
                object res;
                try {
                    // Call the method                    
                    res = InvokeMethod(state, method, instance, parameters);
                } finally {
                    // expose by-ref args
                    for (int i = 0; i <= lastByRefParamIndex; i++) {
                        if (parameterInfos[i].ParameterType.IsByRef) {
                            paramAddrs[i].AssignValue(state, parameters[i]);
                        }
                    }
                }

                // back propagate instance on value types if the instance supports it.
                if (method.DeclaringType != null && method.DeclaringType.IsValueType && !method.IsStatic) {
                    EvaluateAssign(state, target, instance);
                }

                return res;
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object InterpretAndAlsoBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || (bool)ret) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        private static object InterpretOrElseBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || !(bool)ret) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        // TODO: support conversion lambda
        private static object InterpretCoalesceBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;

            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || ret == null) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        private static object InterpretReducibleExpression(InterpreterState state, Expression expr) {
            Debug.Assert(expr.CanReduce);

            //expr is an OpAssignement expression.
            //Reduce it before interpreting.
            return Interpret(state, expr.Reduce());
        }

        private static object InterpretBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;

            object left, right;

            if (InterpretAndCheckFlow(state, node.Left, out left)) {
                return left;
            }
            if (InterpretAndCheckFlow(state, node.Right, out right)) {
                return right;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (node.Method != null) {
                return node.Method.Invoke(null, new object[] { left, right });
            } else {
                return EvaluateBinaryOperator(node.NodeType, left, right);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateBinaryOperator(ExpressionType nodeType, object l, object r) {
            switch (nodeType) {
                case ExpressionType.ArrayIndex:
                    Array array = (Array)l;
                    int index = (int)r;
                    return array.GetValue(index);

                case ExpressionType.GreaterThan:
                    return ScriptingRuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) > 0);
                case ExpressionType.LessThan:
                    return ScriptingRuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) < 0);
                case ExpressionType.GreaterThanOrEqual:
                    return ScriptingRuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) >= 0);
                case ExpressionType.LessThanOrEqual:
                    return ScriptingRuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) <= 0);
                case ExpressionType.Equal:
                    return ScriptingRuntimeHelpers.BooleanToObject(TestEquals(l, r));

                case ExpressionType.NotEqual:
                    return ScriptingRuntimeHelpers.BooleanToObject(!TestEquals(l, r));

                case ExpressionType.Multiply:
                    return EvalMultiply(l, r);
                case ExpressionType.Add:
                    return EvalAdd(l, r);
                case ExpressionType.Subtract:
                    return EvalSub(l, r);
                case ExpressionType.Divide:
                    return EvalDiv(l, r);
                case ExpressionType.Modulo:
                    return EvalMod(l, r);
                case ExpressionType.And:
                    return EvalAnd(l, r);
                case ExpressionType.Or:
                    return EvalOr(l, r);
                case ExpressionType.ExclusiveOr:
                    return EvalXor(l, r);
                case ExpressionType.AddChecked:
                    return EvalAddChecked(l, r);
                case ExpressionType.MultiplyChecked:
                    return EvalMultiplyChecked(l, r);
                case ExpressionType.SubtractChecked:
                    return EvalSubChecked(l, r);
                case ExpressionType.Power:
                    return EvalPower(l, r);

                default:
                    throw new NotImplementedException(nodeType.ToString());
            }
        }

        private static object EvalMultiply(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l * (int)r);
            if (l is uint) return (uint)l * (uint)r;
            if (l is short) return (short)((short)l * (short)r);
            if (l is ushort) return (ushort)((ushort)l * (ushort)r);
            if (l is long) return (long)l * (long)r;
            if (l is ulong) return (ulong)l * (ulong)r;
            if (l is float) return (float)l * (float)r;
            if (l is double) return (double)l * (double)r;
            throw new InvalidOperationException("multiply: {0} " + CompilerHelpers.GetType(l).Name);
        }
        private static object EvalMultiplyChecked(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject(checked((int)l * (int)r));
            if (l is uint) return checked((uint)l * (uint)r);
            if (l is short) return checked((short)((short)l * (short)r));
            if (l is ushort) return checked((ushort)((ushort)l * (ushort)r));
            if (l is long) return checked((long)l * (long)r);
            if (l is ulong) return checked((ulong)l * (ulong)r);
            if (l is float) return checked((float)l * (float)r);
            if (l is double) return checked((double)l * (double)r);
            throw new InvalidOperationException("multiply: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAdd(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l + (int)r);
            if (l is uint) return (uint)l + (uint)r;
            if (l is short) return (short)((short)l + (short)r);
            if (l is ushort) return (ushort)((ushort)l + (ushort)r);
            if (l is long) return (long)l + (long)r;
            if (l is ulong) return (ulong)l + (ulong)r;
            if (l is float) return (float)l + (float)r;
            if (l is double) return (double)l + (double)r;
            throw new InvalidOperationException("add: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAddChecked(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject(checked((int)l + (int)r));
            if (l is uint) return checked((uint)l + (uint)r);
            if (l is short) return checked((short)((short)l + (short)r));
            if (l is ushort) return checked((ushort)((ushort)l + (ushort)r));
            if (l is long) return checked((long)l + (long)r);
            if (l is ulong) return checked((ulong)l + (ulong)r);
            if (l is float) return checked((float)l + (float)r);
            if (l is double) return checked((double)l + (double)r);
            throw new InvalidOperationException("add: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalSub(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l - (int)r);
            if (l is uint) return (uint)l - (uint)r;
            if (l is short) return (short)((short)l - (short)r);
            if (l is ushort) return (ushort)((ushort)l - (ushort)r);
            if (l is long) return (long)l - (long)r;
            if (l is ulong) return (ulong)l - (ulong)r;
            if (l is float) return (float)l - (float)r;
            if (l is double) return (double)l - (double)r;
            throw new InvalidOperationException("sub: {0} " + CompilerHelpers.GetType(l).Name);
        }
        private static object EvalSubChecked(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject(checked((int)l - (int)r));
            if (l is uint) return checked((uint)l - (uint)r);
            if (l is short) return checked((short)((short)l - (short)r));
            if (l is ushort) return checked((ushort)((ushort)l - (ushort)r));
            if (l is long) return checked((long)l - (long)r);
            if (l is ulong) return checked((ulong)l - (ulong)r);
            if (l is float) return checked((float)l - (float)r);
            if (l is double) return checked((double)l - (double)r);
            throw new InvalidOperationException("sub: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalMod(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l % (int)r);
            if (l is uint) return (uint)l % (uint)r;
            if (l is short) return (short)((short)l % (short)r);
            if (l is ushort) return (ushort)((ushort)l % (ushort)r);
            if (l is long) return (long)l % (long)r;
            if (l is ulong) return (ulong)l % (ulong)r;
            if (l is float) return (float)l % (float)r;
            if (l is double) return (double)l % (double)r;
            throw new InvalidOperationException("mod: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalDiv(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l / (int)r);
            if (l is uint) return (uint)l / (uint)r;
            if (l is short) return (short)((short)l / (short)r);
            if (l is ushort) return (ushort)((ushort)l / (ushort)r);
            if (l is long) return (long)l / (long)r;
            if (l is ulong) return (ulong)l / (ulong)r;
            if (l is float) return (float)l / (float)r;
            if (l is double) return (double)l / (double)r;
            throw new InvalidOperationException("div: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAnd(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l & (int)r);
            if (l is uint) return (uint)l & (uint)r;
            if (l is short) return (short)((short)l & (short)r);
            if (l is ushort) return (ushort)((ushort)l & (ushort)r);
            if (l is long) return (long)l & (long)r;
            if (l is ulong) return (ulong)l & (ulong)r;
            throw new InvalidOperationException("and: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalOr(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l | (int)r);
            if (l is uint) return (uint)l | (uint)r;
            if (l is short) return (short)((short)l | (short)r);
            if (l is ushort) return (ushort)((ushort)l | (ushort)r);
            if (l is long) return (long)l | (long)r;
            if (l is ulong) return (ulong)l | (ulong)r;
            throw new InvalidOperationException("or: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalXor(object l, object r) {
            if (l is int) return ScriptingRuntimeHelpers.Int32ToObject((int)l ^ (int)r);
            if (l is uint) return (uint)l ^ (uint)r;
            if (l is short) return (short)((short)l ^ (short)r);
            if (l is ushort) return (ushort)((ushort)l ^ (ushort)r);
            if (l is long) return (long)l ^ (long)r;
            if (l is ulong) return (ulong)l ^ (ulong)r;
            throw new InvalidOperationException("xor: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalPower(object l, object r) {
            return System.Math.Pow((double)l, (double)r);
        }

        private static object EvalCoalesce(object l, object r) {
            return l ?? r;
        }


        private static bool TestEquals(object l, object r) {
            // We don't need to go through the same type checks as the emit case,
            // since we know we're always dealing with boxed objects.

            return Object.Equals(l, r);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretQuoteUnaryExpression(InterpreterState state, Expression expr) {
            // TODO: should we do all the fancy tree rewrite stuff here?
            return ((UnaryExpression)expr).Operand;
        }

        private static object InterpretUnboxUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (value != null && node.Type == value.GetType()) {
                return value;
            }

            throw new InvalidCastException(string.Format("cannot unbox value to type '{0}'", node.Type));
        }

        private static object InterpretConvertUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            if (node.Method != null) {
                return InterpretMethodCallExpression(state, expr, node.Method, null, new[] { node.Operand });
            }

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (node.Type == typeof(void)) {
                return null;
            }

            // TODO: distinguish between Convert and ConvertChecked
            // TODO: semantics should match compiler
            return Cast.Explicit(value, node.Type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object InterpretUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (node.NodeType) {
                case ExpressionType.TypeAs:
                    if (value != null && expr.Type.IsAssignableFrom(value.GetType())) {
                        return value;
                    } else {
                        return null;
                    }

                case ExpressionType.Not:
                    if (value is bool) return (bool)value ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True;
                    if (value is int) return ScriptingRuntimeHelpers.Int32ToObject((int)~(int)value);
                    if (value is long) return (long)~(long)value;
                    if (value is short) return (short)~(short)value;
                    if (value is uint) return (uint)~(uint)value;
                    if (value is ulong) return (ulong)~(ulong)value;
                    if (value is ushort) return (ushort)~(ushort)value;
                    if (value is byte) return (byte)~(byte)value;
                    if (value is sbyte) return (sbyte)~(sbyte)value;
                    throw new InvalidOperationException("can't perform unary not on type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.Negate:
                    if (value is int) return ScriptingRuntimeHelpers.Int32ToObject((int)(-(int)value));
                    if (value is long) return (long)(-(long)value);
                    if (value is short) return (short)(-(short)value);
                    if (value is float) return -(float)value;
                    if (value is double) return -(double)value;
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.UnaryPlus:
                    if (value is int) return ScriptingRuntimeHelpers.Int32ToObject((int)+(int)value);
                    if (value is long) return (long)+(long)value;
                    if (value is short) return (short)+(short)value;
                    if (value is uint) return (uint)+(uint)value;
                    if (value is ulong) return (ulong)+(ulong)value;
                    if (value is ushort) return (ushort)+(ushort)value;
                    if (value is byte) return (byte)+(byte)value;
                    if (value is sbyte) return (sbyte)+(sbyte)value;
                    throw new InvalidOperationException("can't perform unary plus on type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.NegateChecked:
                    if (value is int) return ScriptingRuntimeHelpers.Int32ToObject(checked((int)(-(int)value)));
                    if (value is long) return checked((long)(-(long)value));
                    if (value is short) return checked((short)(-(short)value));
                    if (value is float) return checked(-(float)value);
                    if (value is double) return checked(-(double)value);
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.ArrayLength:
                    System.Array arr = (System.Array)value;
                    return arr.Length;

                default:
                    throw new NotImplementedException();
            }
        }

        private static object InterpretRuntimeVariablesExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            RuntimeVariablesExpression node = (RuntimeVariablesExpression)expr;
            return new InterpreterVariables(state, node);
        }

        private static object InterpretNewExpression(InterpreterState state, Expression expr) {
            NewExpression node = (NewExpression)expr;

            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (InterpretAndCheckFlow(state, node.Arguments[i], out argValue)) {
                    return argValue;
                }
                args[i] = argValue;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            try {
                return node.Constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretListInitExpression(InterpreterState state, Expression expr) {
            throw new NotImplementedException("InterpretListInitExpression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretMemberInitExpression(InterpreterState state, Expression expr) {
            throw new NotImplementedException("InterpretMemberInitExpression");
        }

        private static object InterpretTypeBinaryExpression(InterpreterState state, Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Expression, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            bool result;
            if (node.NodeType == ExpressionType.TypeEqual) {
                result = value != null && value.GetType() == node.TypeOperand;
            } else {
                result = node.TypeOperand.IsInstanceOfType(value);
            }
            return ScriptingRuntimeHelpers.BooleanToObject(result);
        }

        private static object InterpretDynamicExpression(InterpreterState state, Expression expr) {
            DynamicExpression node = (DynamicExpression)expr;
            var arguments = node.Arguments;

            object[] args;
            if (!state.TryGetStackState(node, out args)) {
                args = new object[arguments.Count];
            }

            for (int i = 0, n = arguments.Count; i < n; i++) {
                object argValue;
                if (InterpretAndCheckFlow(state, arguments[i], out argValue)) {
                    if (state.CurrentYield != null) {
                        state.SaveStackState(node, args);
                    }

                    return argValue;
                }
                if (argValue != ControlFlow.NextForYield) {
                    args[i] = argValue;
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            var metaAction = node.Binder as DynamicMetaObjectBinder;
            if (metaAction != null) {
                return InterpretMetaAction(state, metaAction, node, args);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Interpreter.Site: Compiling non-meta-action");
            var callSiteInfo = GetCallSite(state, node);
            return callSiteInfo.CallerTarget(callSiteInfo.CallSite, args);
        }

        private const int SiteCompileThreshold = 2;

        private static object InterpretMetaAction(InterpreterState state, DynamicMetaObjectBinder action, DynamicExpression node, object[] argValues) {
            var callSites = state.LambdaState.ScriptCode.CallSites;
            CallSiteInfo callSiteInfo;

            // TODO: better locking
            lock (callSites) {
                if (!callSites.TryGetValue(node, out callSiteInfo)) {
                    callSiteInfo = new CallSiteInfo();
                    callSites.Add(node, callSiteInfo);
                }
            }

            callSiteInfo.Counter++;
            if (callSiteInfo.Counter > SiteCompileThreshold) {
                if (callSiteInfo.CallSite == null) {
                    SetCallSite(callSiteInfo, node);
                }

                try {
                    return callSiteInfo.CallerTarget(callSiteInfo.CallSite, argValues);
                } catch(Exception e) {
                    state.ScriptCode.LanguageContext.InterpretExceptionThrow(state, e, false);
                    throw;
                }
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Interpreter: Interpreting meta-action");

            if (argValues.Length == 0) {
                throw new InvalidOperationException();
            }

            DynamicMetaObject[] args = DynamicMetaObject.EmptyMetaObjects;
            if (argValues.Length != 1) {
                args = new DynamicMetaObject[argValues.Length - 1];
                for (int i = 0; i < args.Length; i++) {
                    args[i] = DynamicUtils.ObjectToMetaObject(
                        argValues[i + 1],
                        Expression.Constant(argValues[i + 1])
                    );
                }
            }

            DynamicMetaObject binding = action.Bind(
                DynamicUtils.ObjectToMetaObject(
                    argValues[0],
                    Expression.Constant(argValues[0])
                ),
                args
            );

            if (binding == null) {
                throw new InvalidOperationException("Bind cannot return null.");
            }

            // restrictions ignored, they should be valid:
            AssertTrueRestrictions(state, binding);

            var result = Interpret(state, binding.Expression);
            return result;
        }

        [Conditional("DEBUG")]
        private static void AssertTrueRestrictions(InterpreterState state, DynamicMetaObject binding) {
            var test = binding.Restrictions.ToExpression();
            var result = Interpret(state, test);
            Debug.Assert(result is bool && (bool)result);
        }

        private static CallSiteInfo GetCallSite(InterpreterState state, DynamicExpression node) {
            CallSiteInfo callSiteInfo;
            var callSites = state.LambdaState.ScriptCode.CallSites;

            // TODO: better locking
            lock (callSites) {
                if (!callSites.TryGetValue(node, out callSiteInfo)) {
                    callSiteInfo = new CallSiteInfo();
                    SetCallSite(callSiteInfo, node);
                    callSites.Add(node, callSiteInfo);
                }
            }

            return callSiteInfo;
        }
        
        // The ReflectiveCaller cache
        private static readonly Dictionary<ValueArray<Type>, ReflectedCaller> _executeSites = new Dictionary<ValueArray<Type>, ReflectedCaller>();

        private static void SetCallSite(CallSiteInfo info, DynamicExpression node) {
            var arguments = node.Arguments;

            // TODO: remove CodeContext special case
            if (arguments.Count > 0 && arguments[0].Type != typeof(CodeContext)) {
                switch (arguments.Count) {
                    case 0:
                        info.CallSite = CallSite<Func<CallSite, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target0);
                        return;

                    case 1:
                        info.CallSite = CallSite<Func<CallSite, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target1);
                        return;

                    case 2:
                        info.CallSite = CallSite<Func<CallSite, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target2);
                        return;

                    case 3:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target3);
                        return;

                    case 4:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target4);
                        return;

                    case 5:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target5);
                        return;

                    case 6:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target6);
                        return;

                    case 7:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target7);
                        return;

                    case 8:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target8);
                        return;

                    case 9:
                        info.CallSite = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(node.Binder);
                        info.CallerTarget = new MatchCallerTarget(MatchCaller.Target9);
                        return;
                }
            }

            var callSite = CreateCallSite(node);
            info.CallSite = callSite;
            info.CallerTarget = MatchCaller.GetCaller((callSite.GetType().GetGenericArguments()[0]));
        }

        private static CallSite CreateCallSite(DynamicExpression node) {
            var arguments = node.Arguments;

            // non-optimized signatures:
            Type[] types = CompilerHelpers.GetSiteTypes(arguments, node.Type);

            int i = (arguments.Count > 0 && arguments[0].Type != typeof(CodeContext)) ? 1 : 0;

            for (; i < arguments.Count; i++) {
                if (!arguments[i].Type.IsByRef) {
                    types[i] = typeof(object);
                }
            }

            ReflectedCaller rc;
            lock (_executeSites) {
                ValueArray<Type> array = new ValueArray<Type>(types);
                if (!_executeSites.TryGetValue(array, out rc)) {
                    Type delegateType = DynamicSiteHelpers.MakeCallSiteDelegate(types);
                    MethodInfo target = typeof(InterpreterHelpers).GetMethod("CreateSite").MakeGenericMethod(delegateType);
                    _executeSites[array] = rc = ReflectedCaller.Create(target);
                }
            }

            return (CallSite)rc.Invoke(node.Binder);
        }

        private static object InterpretIndexAssignment(InterpreterState state, BinaryExpression node) {
            var index = (IndexExpression)node.Left;

            object instance, value;
            var args = new object[index.Arguments.Count];

            if (InterpretAndCheckFlow(state, index.Object, out instance)) {
                return instance;
            }

            for (int i = 0; i < index.Arguments.Count; i++) {
                object arg;
                if (InterpretAndCheckFlow(state, index.Arguments[i], out arg)) {
                    return arg;
                }
                args[i] = arg;
            }

            if (InterpretAndCheckFlow(state, node.Right, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (index.Indexer != null) {
                // For indexed properties, just call the setter
                InvokeMethod(state, index.Indexer.GetSetMethod(true), instance, args);
            } else if (index.Arguments.Count != 1) {
                // Multidimensional arrays, call set
                var set = index.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);
                InvokeMethod(state, set, instance, args);
            } else {
                ((Array)instance).SetValue(value, (int)args[0]);
            }

            return value;
        }

        private static object InterpretVariableAssignment(InterpreterState state, Expression expr) {
            var node = (BinaryExpression)expr;
            object value;
            if (InterpretAndCheckFlow(state, node.Right, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            EvaluateAssignVariable(state, node.Left, value);
            return value;
        }

        private static object InterpretAssignBinaryExpression(InterpreterState state, Expression expr) {
            var node = (BinaryExpression)expr;
            switch (node.Left.NodeType) {
                case ExpressionType.Index:
                    return InterpretIndexAssignment(state, node);
                case ExpressionType.MemberAccess:
                    return InterpretMemberAssignment(state, node);
                case ExpressionType.Parameter:
                case ExpressionType.Extension:
                    return InterpretVariableAssignment(state, node);
                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Left.NodeType);
            }
        }

        private static object InterpretParameterExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return state.GetValue(expr);
        }

        private static object InterpretLambdaExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            LambdaExpression node = (LambdaExpression)expr;
            return GetDelegateForInterpreter(state, node);
        }

        private static object InterpretMemberAssignment(InterpreterState state, BinaryExpression node) {
            var left = (MemberExpression)node.Left;

            object target = null, value;
            if (left.Expression != null) {
                if (InterpretAndCheckFlow(state, left.Expression, out target)) {
                    return target;
                }
            }
            if (InterpretAndCheckFlow(state, node.Right, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (left.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)left.Member;
                    field.SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)left.Member;
                    property.SetValue(target, value, null);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
            return value;
        }

        private static object InterpretMemberExpression(InterpreterState state, Expression expr) {
            MemberExpression node = (MemberExpression)expr;

            object self = null;
            if (node.Expression != null) {
                if (InterpretAndCheckFlow(state, node.Expression, out self)) {
                    return self;
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    return field.GetValue(self);
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    return property.GetValue(self, ArrayUtils.EmptyObjects);
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }

        private static object InterpretNewArrayExpression(InterpreterState state, Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;
            ConstructorInfo constructor;

            if (node.NodeType == ExpressionType.NewArrayBounds) {
                int rank = node.Type.GetArrayRank();
                Type[] types = new Type[rank];
                object[] bounds = new object[rank];
                for (int i = 0; i < rank; i++) {
                    types[i] = typeof(int);
                    object value;
                    if (InterpretAndCheckFlow(state, node.Expressions[i], out value)) {
                        return value;
                    }
                    bounds[i] = value;
                }

                if (state.CurrentYield != null) {
                    return ControlFlow.NextForYield;
                }

                constructor = expr.Type.GetConstructor(types);
                return constructor.Invoke(bounds);
            } else {
                // this must be ExpressionType.NewArrayInit
                object[] values;
                if (!state.TryGetStackState(node, out values)) {
                    values = new object[node.Expressions.Count];
                }

                for (int i = 0; i < node.Expressions.Count; i++) {
                    object value;
                    if (InterpretAndCheckFlow(state, node.Expressions[i], out value)) {
                        if (state.CurrentYield != null) {
                            // yield w/ expressions on the stack, we need to save the currently 
                            // evaluated nodes for when we come back.
                            state.SaveStackState(node, values);
                        }

                        return value;
                    }

                    if (value != ControlFlow.NextForYield) {
                        values[i] = value;
                    }
                }

                if (state.CurrentYield != null) {
                    // we were just walking looking for yields, this has no result.
                    return ControlFlow.NextForYield;
                }

                if (node.Type != typeof(object[])) {
                    constructor = expr.Type.GetConstructor(new Type[] { typeof(int) });
                    Array contents = (Array)constructor.Invoke(new object[] { node.Expressions.Count });
                    // value arrays cannot be cast to object arrays

                    for (int i = 0; i < node.Expressions.Count; i++) {
                        contents.SetValue(values[i], i);
                    }
                    return contents;
                }

                return values;
            }
        }

        private static object InterpretGotoExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            var node = (GotoExpression)expr;

            object value = null;
            if (node.Value != null) {
                value = Interpret(state, node.Value);
                ControlFlow cf = value as ControlFlow;
                if (cf != null) {
                    // propagate
                    return cf;
                }
            }

            return ControlFlow.Goto(node.Target, value);
        }

        private static object InterpretDefaultExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            Type type = expr.Type;
            if (type == typeof(void)) {
                return ControlFlow.NextStatement;
            } else if (type.IsValueType) {
                return Activator.CreateInstance(type);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Labeled statement makes break/continue go to the end of the contained expression.
        /// </summary>
        private static object InterpretLabelExpression(InterpreterState state, Expression expr) {
            LabelExpression node = (LabelExpression)expr;

            object res = ControlFlow.NextStatement;
            if (node.DefaultValue != null) {
                res = Interpret(state, node.DefaultValue);
                var cf = res as ControlFlow;
                if (cf != null && cf.Kind == ControlFlowKind.Goto && cf.Label == node.Target) {
                    res = cf.Value;
                }
            }

            return res;
        }

        private static object InterpretLoopExpression(InterpreterState state, Expression expr) {
            LoopExpression node = (LoopExpression)expr;

            for (; ; ) {
                ControlFlow cf;

                object body = Interpret(state, node.Body);
                if ((cf = body as ControlFlow) != null) {
                    if (cf.Kind == ControlFlowKind.Goto) {
                        if (cf.Label == node.BreakLabel) {
                            // Break out of the loop and execute next statement outside
                            return ControlFlow.NextStatement;
                        } else if (cf.Label != node.ContinueLabel) {
                            return cf;
                        }
                    } else if (cf.Kind == ControlFlowKind.Yield) {
                        return body;
                    }
                }
            }
        }

        private static object InterpretDebugInfoExpression(InterpreterState state, Expression expr) {
            var node = (DebugInfoExpression)expr;

            if (state.CurrentYield == null) {
                // Note: setting index to 0 because we don't have one available
                // Index should be removed from SourceLocation
                state.CurrentLocation = new SourceLocation(0, node.StartLine, node.StartColumn);
            }

            return Interpret(state, node.Expression);
        }

        private static object InterpretBlockExpression(InterpreterState state, Expression expr) {
            BlockExpression node = (BlockExpression)expr;

            InterpreterState child = state;
            if (node.Variables.Count > 0) {
                // restore scope if we yielded
                if (!state.TryGetStackState(node, out child)) {
                    // otherwise, create a new nested scope
                    child = state.CreateForScope(node);
                }
            }

            try {
                var expressions = node.Expressions;
                int count = expressions.Count;

                if (count > 0) {
                    int current = 0;

                    for (; ; ) {
                        object val = null;
                        Expression ce = expressions[current];

                        if (InterpretAndCheckFlow(child, ce, out val)) {
                            // Control flow
                            if (val != ControlFlow.NextStatement) {
                                ControlFlow cf = (ControlFlow)val;
                                if (cf.Kind == ControlFlowKind.Goto) {
                                    // Is the goto within the block?
                                    for (int target = 0; target < count; target++) {
                                        LabelExpression le = expressions[target] as LabelExpression;
                                        if (le != null && le.Target == cf.Label) {
                                            // Reset to execute the code from after the label
                                            // We are going to the label and since label is at the end of the
                                            // expression, set to target and we'll advance below.
                                            current = target;
                                            val = null;
                                            goto Next;
                                        }
                                    }
                                }

                                return cf;
                            }
                        }

                    Next:
                        // Next expression
                        current++;

                        // Last expression?
                        if (current >= count) {
                            return node.Type != typeof(void) ? val : ControlFlow.NextStatement;
                        }
                    }
                }
            } finally {
                if (node.Variables.Count > 0) {
                    if (state.CurrentYield != null) {
                        // save scope if yielding so we can restore it
                        state.SaveStackState(node, child);
                    }
                }
            }

            return ControlFlow.NextStatement;
        }

        private static object InterpretSwitchExpression(InterpreterState state, Expression expr) {
            // TODO: yield aware switch
            SwitchExpression node = (SwitchExpression)expr;

            object testValue;
            if (InterpretAndCheckFlow(state, node.Test, out testValue)) {
                return testValue;
            }

            int test = (int)testValue;
            ReadOnlyCollection<SwitchCase> cases = node.SwitchCases;
            int target = 0;
            while (target < cases.Count) {
                SwitchCase sc = cases[target];
                if (sc.IsDefault || sc.Value == test) {
                    break;
                }

                target++;
            }

            while (target < cases.Count) {
                SwitchCase sc = cases[target];
                object result = Interpret(state, sc.Body);

                ControlFlow cf = result as ControlFlow;
                if (cf != null) {
                    if (cf.Label == node.BreakLabel) {
                        return ControlFlow.NextStatement;
                    } else if (cf.Kind == ControlFlowKind.Yield || cf.Kind == ControlFlowKind.Goto) {
                        return cf;
                    }
                }
                target++;
            }

            return ControlFlow.NextStatement;
        }

        #region Exceptions

        [ThreadStatic]
        private static List<Exception> _evalExceptions;

        private static void PopEvalException() {
            _evalExceptions.RemoveAt(_evalExceptions.Count - 1);
            if (_evalExceptions.Count == 0) _evalExceptions = null;
        }

        private static void PushEvalException(Exception exc) {
            if (_evalExceptions == null) _evalExceptions = new List<Exception>();
            _evalExceptions.Add(exc);
        }

        private static Exception LastEvalException {
            get {
                if (_evalExceptions == null || _evalExceptions.Count == 0) {
                    throw new InvalidOperationException("rethrow outside of catch block");
                }

                return _evalExceptions[_evalExceptions.Count - 1];
            }
        }

        private static object InterpretThrowUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;
            Exception ex;

            if (node.Operand == null) {
                ex = LastEvalException;
            } else {
                object exception;
                if (InterpretAndCheckFlow(state, node.Operand, out exception)) {
                    return exception;
                }

                ex = (Exception)exception;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            state.LambdaState.ScriptCode.LanguageContext.InterpretExceptionThrow(state, ex, true);
            throw ex;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        private static object InterpretTryExpression(InterpreterState state, Expression expr) {
            // TODO: Yield aware
            TryExpression node = (TryExpression)expr;
            bool rethrow = false, catchFaulted = false;
            Exception savedExc = null;
            object ret = ControlFlow.NextStatement;

            try {
                if (!InterpretAndCheckFlow(state, node.Body, out ret)) {
                    ret = ControlFlow.NextStatement;
                }
            } catch (Exception exc) {
                rethrow = true;
                savedExc = exc;
                if (node.Handlers != null) {
                    PushEvalException(exc);
                    try {
                        ret = ControlFlowKind.NextStatement;
                        foreach (CatchBlock handler in node.Handlers) {
                            if (handler.Test.IsInstanceOfType(exc)) {
                                if (handler.Variable != null) {
                                    EvaluateAssignVariable(state, handler.Variable, exc);
                                }

                                if (handler.Filter != null) {
                                    object filterResult;
                                    if (InterpretAndCheckFlow(state, handler.Filter, out filterResult)) {
                                        ret = filterResult;
                                        break;
                                    } else if (!((bool)filterResult)) {
                                        // handler doesn't apply, check next handler.
                                        continue;
                                    }
                                }

                                rethrow = false;
                                catchFaulted = true;
                                object body;
                                if (InterpretAndCheckFlow(state, handler.Body, out body)) {
                                    ret = body;
                                }
                                catchFaulted = false;
                                break;
                            }
                        }
                    } finally {
                        PopEvalException();
                    }
                }
            } finally {
                if (node.Finally != null || ((rethrow || catchFaulted) && node.Fault != null)) {
                    Expression faultOrFinally = node.Finally ?? node.Fault;

                    object result;
                    if (InterpretAndCheckFlow(state, faultOrFinally, out result) &&
                        result != ControlFlow.NextStatement) {
                        ret = result;
                        rethrow = false;
                    }
                }
                if (rethrow) {
                    throw ExceptionHelpers.UpdateForRethrow(savedExc);
                }
            }

            return ret;
        }

        private static object InterpretYieldExpression(InterpreterState state, YieldExpression node) {
            // Yield break
            if (node.Value == null) {
                return ControlFlow.YieldBreak;
            }

            if (state.CurrentYield == node) {
                // we've just advanced past the current yield, start executing code again.
                state.CurrentYield = null;
                return ControlFlow.NextStatement;
            }

            object res;
            if (InterpretAndCheckFlow(state, node.Value, out res) && res != ControlFlow.NextStatement) {
                // yield contains flow control.
                return res;
            }

            if (state.CurrentYield == null) {
                // we are the yield, we just ran our code, and now we
                // need to return the result.
                state.CurrentYield = node;

                return ControlFlow.YieldReturn(res);
            }

            return ControlFlow.NextForYield;
        }

        private static object InterpretGeneratorExpression(InterpreterState state, GeneratorExpression generator) {
            // Fast path for object
            if (generator.Target.Type == typeof(object)) {
                return InterpretGenerator<object>(state, generator);
            }

            // TODO: slow path
            return ReflectedCaller.Create(
                typeof(Interpreter).GetMethod(
                    "InterpretGenerator", BindingFlags.NonPublic | BindingFlags.Static
                ).MakeGenericMethod(generator.Target.Type)
            ).Invoke(state, generator);
        }

        private static object InterpretGenerator<T>(InterpreterState state, GeneratorExpression generator) {
            var caller = InterpreterState.Current.Value;
            if (generator.IsEnumerable) {
                return new GeneratorEnumerable<T>(
                    () => new GeneratorInvoker(generator, state.CreateForGenerator(caller)).Invoke
                );
            } else {
                return new GeneratorEnumerator<T>(
                    new GeneratorInvoker(generator, state.CreateForGenerator(caller)).Invoke
                );
            }
        }

        private static object InterpretExtensionExpression(InterpreterState state, Expression expr) {
            var ffc = expr as FinallyFlowControlExpression;
            if (ffc != null) {
                return Interpret(state, ffc.Body);
            }

            var yield = expr as YieldExpression;
            if (yield != null) {
                return InterpretYieldExpression(state, yield);
            }

            var generator = expr as GeneratorExpression;
            if (generator != null) {
                return InterpretGeneratorExpression(state, generator);
            }

            return Interpret(state, expr.ReduceExtensions());
        }

        #endregion

        internal static object EvaluateAssign(InterpreterState state, Expression node, object value) {
            switch (node.NodeType) {
                case ExpressionType.Parameter:
                case ExpressionType.Extension:
                    return EvaluateAssignVariable(state, node, value);
                // TODO: this is wierd, why are we supporting assign to assignment?
                case ExpressionType.Assign:
                    return EvaluateAssignVariable(state, ((BinaryExpression)node).Left, value);
                case ExpressionType.MemberAccess:
                    return EvaluateAssign(state, (MemberExpression)node, value);
                default:
                    return value;
            }
        }

        private static object EvaluateAssignVariable(InterpreterState state, Expression var, object value) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            state.SetValue(var, value);
            return value;
        }

        private static object EvaluateAssign(InterpreterState state, MemberExpression node, object value) {
            object self = null;
            if (InterpretAndCheckFlow(state, node.Expression, out self)) {
                return self;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    field.SetValue(self, value);
                    return value;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    property.SetValue(self, value, ArrayUtils.EmptyObjects);
                    return value;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }


        private static EvaluationAddress EvaluateAddress(InterpreterState state, Expression node) {
            switch (node.NodeType) {
                case ExpressionType.Parameter:
                    return new VariableAddress(node);
                case ExpressionType.Block:
                    return EvaluateAddress(state, (BlockExpression)node);
                case ExpressionType.Conditional:
                    return EvaluateAddress(state, (ConditionalExpression)node);
                default:
                    return new EvaluationAddress(node);
            }
        }


        private static EvaluationAddress EvaluateAddress(InterpreterState state, BlockExpression node) {
            if (node.Type == typeof(void)) {
                throw new NotSupportedException("Address of block without value");
            }

            List<EvaluationAddress> addresses = new List<EvaluationAddress>();
            foreach (Expression current in node.Expressions) {
                addresses.Add(EvaluateAddress(state, current));
            }
            return new CommaAddress(node, addresses);
        }

        private static EvaluationAddress EvaluateAddress(InterpreterState state, ConditionalExpression node) {
            object test = (bool)Interpret(state, node.Test);

            if ((bool)test) {
                return EvaluateAddress(state, node.IfTrue);
            } else {
                return EvaluateAddress(state, node.IfFalse);
            }
        }
    }
}
