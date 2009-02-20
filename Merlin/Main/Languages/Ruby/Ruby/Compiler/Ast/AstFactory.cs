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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using System.Threading;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    
    public static class AstFactory {

        public static readonly MSA.Expression[] EmptyExpressions = new MSA.Expression[0];
        public static readonly MSA.ParameterExpression[] EmptyParameters = new MSA.ParameterExpression[0];
        public static readonly MSA.Expression NullOfMutableString = Ast.Constant(null, typeof(MutableString));
        public static readonly MSA.Expression NullOfProc = Ast.Constant(null, typeof(Proc));
        internal static readonly MSA.Expression BlockReturnReasonBreak = Ast.Constant(BlockReturnReason.Break);

        #region Control Flow

        internal static MSA.Expression/*!*/ MakeUserMethodBody(AstGenerator gen, int lastLine,
            MSA.Expression/*!*/ blockParameter, MSA.Expression/*!*/ rfcVariable,
            MSA.ParameterExpression/*!*/ methodUnwinder, MSA.Expression/*!*/ bodyStatement, ResultOperation resultOperation, 
            int profileTickIndex, MSA.ParameterExpression stampVariable, MSA.LabelTarget returnLabel) {

            Assert.NotNull(blockParameter, rfcVariable, bodyStatement, methodUnwinder);
            Debug.Assert(!resultOperation.IsIgnore, "return value should not be ignored");
            Debug.Assert(returnLabel != null || resultOperation.Variable != null, "return label needed");

            MSA.Expression resultExpression = Ast.Field(methodUnwinder, MethodUnwinder.ReturnValueField);
            if (resultOperation.Variable != null) {
                resultExpression = Ast.Assign(resultOperation.Variable, resultExpression);
            } else {
                resultExpression = Ast.Return(returnLabel, resultExpression);
            }

            // TODO: move this to the caller:
            MSA.Expression profileStart, profileEnd;
            if (stampVariable != null) {
                profileStart = Ast.Assign(stampVariable, Methods.Stopwatch_GetTimestamp.OpCall());
                profileEnd = Methods.UpdateProfileTicks.OpCall(Ast.Constant(profileTickIndex), stampVariable);
            } else {
                profileStart = profileEnd = Ast.Empty();
            }

            return AstUtils.Try(
                // initialize frame (RFC):
                profileStart,
                Ast.Assign(rfcVariable, Methods.CreateRfcForMethod.OpCall(AstUtils.Convert(blockParameter, typeof(Proc)))),
                bodyStatement
            ).Filter(methodUnwinder, Ast.Equal(Ast.Field(methodUnwinder, MethodUnwinder.TargetFrameField), rfcVariable),

                // return unwinder.ReturnValue;
                resultExpression

            ).Finally(
                Ast.Assign(Ast.Field(rfcVariable, RuntimeFlowControl.IsActiveMethodField), Ast.Constant(false)),
                profileEnd,
                gen != null && gen.TraceEnabled ? Methods.TraceMethodReturn.OpCall(
                    gen.CurrentScopeVariable, 
                    Ast.Convert(Ast.Constant(gen.SourceUnit.Path), typeof(string)),
                    Ast.Constant(lastLine)
                ) : Ast.Empty()
            );
        }

        #endregion

        public static MSA.Expression/*!*/ Infinite(MSA.LabelTarget @break, MSA.LabelTarget @continue, params MSA.Expression[]/*!*/ body) {
            return AstUtils.Infinite(Ast.Block(body), @break, @continue);
        }

        public static MSA.Expression[]/*!*/ CreateExpressionArray(int count) {
            return (count > 0) ? new MSA.Expression[count] : EmptyExpressions;
        }

        public static MSA.Expression/*!*/ Block(params MSA.Expression/*!*/[]/*!*/ expressions) {
            switch (expressions.Length) {
                case 0: return Ast.Empty();
                case 1: return expressions[0];
                default: return Ast.Block(new ReadOnlyCollection<MSA.Expression>(expressions));
            }
        }

        public static MSA.Expression/*!*/ Block(List<MSA.Expression/*!*/>/*!*/ expressions) {
            switch (expressions.Count) {
                case 0: return Ast.Empty();
                case 1: return expressions[0];
                default: return Ast.Block(new ReadOnlyCollection<MSA.Expression>(expressions.ToArray()));
            }
        }

        public static MSA.Expression/*!*/ Box(MSA.Expression/*!*/ expression) {
            return AstUtils.Convert(expression, typeof(object));
        }

        public static MSA.Expression/*!*/ IsTrue(MSA.Expression/*!*/ expression) {
            if (expression.Type == typeof(bool)) {
                return expression;
            } else {
                return Methods.IsTrue.OpCall(Box(expression));
            }
        }

        public static MSA.Expression/*!*/ IsFalse(MSA.Expression/*!*/ expression) {
            if (expression.Type == typeof(bool)) {
                return Ast.Not(expression);
            } else {
                return Methods.IsFalse.OpCall(Box(expression));
            }
        }

        internal static TryStatementBuilder/*!*/ FinallyIf(this TryStatementBuilder/*!*/ builder, bool ifdef, params MSA.Expression[]/*!*/ body) {
            return ifdef ? builder.Finally(body) : builder;
        }

        internal static TryStatementBuilder/*!*/ FilterIf(this TryStatementBuilder/*!*/ builder, bool ifdef,
            MSA.ParameterExpression/*!*/ holder, MSA.Expression/*!*/ condition, params MSA.Expression[]/*!*/ body) {
            return ifdef ? builder.Filter(holder, condition, body) : builder;
        }

        public static MSA.Expression/*!*/ Condition(MSA.Expression/*!*/ test, MSA.Expression/*!*/ ifTrue, MSA.Expression/*!*/ ifFalse) {
            Assert.NotNull(test, ifTrue, ifFalse);
            Debug.Assert(test.Type == typeof(bool));

            if (ifTrue.Type != ifFalse.Type) {
                if (ifTrue.Type.IsAssignableFrom(ifFalse.Type)) {
                    ifFalse = Ast.Convert(ifFalse, ifTrue.Type);
                } else if (ifFalse.Type.IsAssignableFrom(ifTrue.Type)) {
                    ifTrue = Ast.Convert(ifTrue, ifFalse.Type);
                } else {
                    ifTrue = Box(ifTrue);
                    ifFalse = Box(ifFalse);
                }
            }

            return Ast.Condition(test, ifTrue, ifFalse);
        }

        internal static MSA.Expression/*!*/ CallDelegate(Delegate/*!*/ method, MSA.Expression[]/*!*/ arguments) {
            // We prefer to peek inside the delegate and call the target method directly. However, we need to
            // exclude DynamicMethods since Delegate.Method returns a dummy MethodInfo, and we cannot emit a call to it.
            if (method.Method.DeclaringType == null || !method.Method.DeclaringType.IsPublic || !method.Method.IsPublic) {
                // do not inline:
                return Ast.Call(Ast.Constant(method), method.GetType().GetMethod("Invoke"), arguments);
            } else if (method.Target != null) {
                if (method.Method.IsStatic) {
                    // inline a closed static delegate:
                    return Ast.Call(null, method.Method, ArrayUtils.Insert(Ast.Constant(method.Target), arguments));
                } else {
                    // inline a closed instance delegate:
                    return Ast.Call(Ast.Constant(method.Target), method.Method, arguments);
                }
            } else if (method.Method.IsStatic) {
                // inline an open static delegate:
                return Ast.Call(null, method.Method, arguments);
            } else {
                // inline an open instance delegate:
                return Ast.Call(arguments[0], method.Method, ArrayUtils.RemoveFirst(arguments));
            }
        }

        internal static Type/*!*/[]/*!*/ GetSignature(MSA.ParameterExpression/*!*/[]/*!*/ parameters, Type/*!*/ returnType) {
            Type[] result = new Type[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++) {
                result[i] = parameters[i].Type;
            }

            // return type:
            result[result.Length - 1] = returnType;
            return result;
        }

        internal static MSA.Expression/*!*/ YieldExpression(
            IList<MSA.Expression>/*!*/ arguments, 
            MSA.Expression splattedArgument,
            MSA.Expression rhsArgument,
            MSA.Expression/*!*/ bfcVariable,
            MSA.Expression/*!*/ selfArgument) {

            Assert.NotNull(arguments, bfcVariable, selfArgument);

            bool hasArgumentArray;
            var opMethod = Methods.Yield(arguments.Count, splattedArgument != null, rhsArgument != null, out hasArgumentArray);

            var args = new List<MSA.Expression>();

            foreach (var arg in arguments) {
                args.Add(AstFactory.Box(arg));
            }

            if (hasArgumentArray) {
                args = CollectionUtils.MakeList<MSA.Expression>(Ast.NewArrayInit(typeof(object), args));
            }

            if (splattedArgument != null) {
                args.Add(AstFactory.Box(splattedArgument));
            }

            if (rhsArgument != null) {
                args.Add(AstFactory.Box(rhsArgument));
            }

            args.Add(AstFactory.Box(selfArgument));
            args.Add(bfcVariable);

            return Ast.Call(opMethod, args.ToArray());
        }
    }
}
