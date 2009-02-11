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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding {

    static class Binders {
        public static Expression/*!*/ Invoke(BinderState/*!*/ binder, Type/*!*/ resultType, CallSignature signature, params Expression/*!*/[]/*!*/ args) {
            PythonInvokeBinder invoke = new PythonInvokeBinder(binder, signature);
            switch(args.Length) {
                case 0: return Ast.Dynamic(invoke, resultType, AstUtils.CodeContext());
                case 1: return Ast.Dynamic(invoke, resultType, AstUtils.CodeContext(), args[0]);
                case 2: return Ast.Dynamic(invoke, resultType, AstUtils.CodeContext(), args[0], args[1]);
                case 3: return Ast.Dynamic(invoke, resultType, AstUtils.CodeContext(), args[0], args[1], args[2]);
                default:
                    return Ast.Dynamic(
                        invoke,
                        resultType,
                        new ReadOnlyCollection<Expression>(ArrayUtils.Insert(AstUtils.CodeContext(), args))
                    );
            }

        }

        public static Expression/*!*/ Convert(BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            return Ast.Dynamic(
                new ConversionBinder(
                    binder,
                    type,
                    resultKind
                ),
                type,
                target
            );
        }

        /// <summary>
        /// Backwards compatible Convert for the old sites that need to flow CodeContext
        /// </summary>
        public static Expression/*!*/ Convert(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            return Ast.Dynamic(
                new ConversionBinder(
                    binder,
                    type,
                    resultKind
                ),
                type,
                target
            );
        }

        public static Expression/*!*/ Operation(BinderState/*!*/ binder, Type/*!*/ resultType, PythonOperationKind operation, Expression arg0) {
            return Ast.Dynamic(
                UnaryOperationBinder(
                    binder,
                    operation
                ),
                resultType,
                arg0
            );
        }

        public static Expression/*!*/ Operation(BinderState/*!*/ binder, Type/*!*/ resultType, PythonOperationKind operation, Expression arg0, Expression arg1) {
            return Ast.Dynamic(
                BinaryOperationBinder(
                    binder,
                    operation
                ),
                resultType,
                arg0,
                arg1
            );
        }

        public static Expression/*!*/ Operation(BinderState/*!*/ binder, Type/*!*/ resultType, PythonOperationKind operation, params Expression[] args) {
            if (args.Length == 1) {
                return Operation(binder, resultType, operation, args[0]);
            } else if (args.Length == 2) {
                return Operation(binder, resultType, operation, args[0], args[1]);
            }
            return Ast.Dynamic(
                new PythonOperationBinder(
                    binder,
                    operation
                ),
                resultType,
                args
            );
        }

        public static Expression/*!*/ Set(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target, Expression/*!*/ value) {
            return Ast.Dynamic(
                new PythonSetMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target,
                value
            );
        }

        public static Expression/*!*/ Get(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Get(AstUtils.CodeContext(), binder, resultType, name, target);
        }

        public static Expression/*!*/ Get(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.Dynamic(
                new PythonGetMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target,
                codeContext
            );
        }

        public static Expression/*!*/ TryGet(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.Dynamic(
                new PythonGetMemberBinder(
                    binder,
                    name,
                    true
                ),
                resultType,
                target,
                codeContext
            );
        }

        public static Expression/*!*/ TryGet(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return TryGet(AstUtils.CodeContext(), binder, resultType, name, target);
        }

        public static Expression/*!*/ Delete(BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {        
            return Ast.Dynamic(
                new PythonDeleteMemberBinder(
                    binder,
                    name
                ),
                resultType,
                target
            );
        }

        public static DynamicMetaObjectBinder/*!*/ BinaryOperationRetBool(BinderState/*!*/ state, PythonOperationKind operatorName) {
            return BinaryOperationRetType(state, operatorName, typeof(bool));
        }

        public static DynamicMetaObjectBinder/*!*/ BinaryOperationRetType(BinderState/*!*/ state, PythonOperationKind operatorName, Type retType) {
            return new ComboBinder(
                new BinderMappingInfo(
                    BinaryOperationBinder(state, operatorName),
                    ParameterMappingInfo.Parameter(0),
                    ParameterMappingInfo.Parameter(1)
                ),
                new BinderMappingInfo(
                    new ConversionBinder(state, retType, ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        public static DynamicMetaObjectBinder UnaryOperationBinder(BinderState state, PythonOperationKind operatorName) {
            ExpressionType? et = GetExpressionTypeFromUnaryOperator(operatorName);
            
            if (et == null) {
                return new PythonOperationBinder(
                    state,
                    operatorName
                );
            }

            return new PythonUnaryOperationBinder(state, et.Value);
        }

        private static ExpressionType? GetExpressionTypeFromUnaryOperator(PythonOperationKind operatorName) {
            switch (operatorName) {
                case PythonOperationKind.Positive: return ExpressionType.UnaryPlus;
                case PythonOperationKind.Negate: return ExpressionType.Negate;
                case PythonOperationKind.OnesComplement: return ExpressionType.OnesComplement;
                case PythonOperationKind.Not: return ExpressionType.IsFalse;
            }
            return null;
        }

        public static DynamicMetaObjectBinder BinaryOperationBinder(BinderState state, PythonOperationKind operatorName) {
            ExpressionType? et = GetExpressionTypeFromBinaryOperator(operatorName);

            if (et == null) {
                return new PythonOperationBinder(
                    state,
                    operatorName
                );
            }

            return new PythonBinaryOperationBinder(state, et.Value);
        }

        private static ExpressionType? GetExpressionTypeFromBinaryOperator(PythonOperationKind operatorName) {
            switch (operatorName) {
                case PythonOperationKind.Add: return ExpressionType.Add;
                case PythonOperationKind.BitwiseAnd: return ExpressionType.And;
                case PythonOperationKind.Divide: return ExpressionType.Divide;
                case PythonOperationKind.ExclusiveOr: return ExpressionType.ExclusiveOr;
                case PythonOperationKind.Mod: return ExpressionType.Modulo;
                case PythonOperationKind.Multiply: return ExpressionType.Multiply;
                case PythonOperationKind.BitwiseOr: return ExpressionType.Or;
                case PythonOperationKind.Power: return ExpressionType.Power;
                case PythonOperationKind.RightShift: return ExpressionType.RightShift;
                case PythonOperationKind.LeftShift: return ExpressionType.LeftShift;
                case PythonOperationKind.Subtract: return ExpressionType.Subtract;

                case PythonOperationKind.InPlaceAdd: return ExpressionType.AddAssign;
                case PythonOperationKind.InPlaceBitwiseAnd: return ExpressionType.AndAssign;
                case PythonOperationKind.InPlaceDivide: return ExpressionType.DivideAssign;
                case PythonOperationKind.InPlaceExclusiveOr: return ExpressionType.ExclusiveOrAssign;
                case PythonOperationKind.InPlaceMod: return ExpressionType.ModuloAssign;
                case PythonOperationKind.InPlaceMultiply: return ExpressionType.MultiplyAssign;
                case PythonOperationKind.InPlaceBitwiseOr: return ExpressionType.OrAssign;
                case PythonOperationKind.InPlacePower: return ExpressionType.PowerAssign;
                case PythonOperationKind.InPlaceRightShift: return ExpressionType.RightShiftAssign;
                case PythonOperationKind.InPlaceLeftShift: return ExpressionType.LeftShiftAssign;
                case PythonOperationKind.InPlaceSubtract: return ExpressionType.SubtractAssign;

                case PythonOperationKind.Equal: return ExpressionType.Equal;
                case PythonOperationKind.GreaterThan: return ExpressionType.GreaterThan;
                case PythonOperationKind.GreaterThanOrEqual: return ExpressionType.GreaterThanOrEqual;
                case PythonOperationKind.LessThan: return ExpressionType.LessThan;
                case PythonOperationKind.LessThanOrEqual: return ExpressionType.LessThanOrEqual;
                case PythonOperationKind.NotEqual: return ExpressionType.NotEqual;
            }
            return null;
        }

        public static DynamicMetaObjectBinder/*!*/ InvokeAndConvert(BinderState/*!*/ state, int argCount, Type retType) {
            // +2 for the target object and CodeContext which InvokeBinder recevies
            ParameterMappingInfo[] args = new ParameterMappingInfo[argCount + 2];   
            for (int i = 0; i < argCount + 2; i++) {
                args[i] = ParameterMappingInfo.Parameter(i);
            }

            return new ComboBinder(
                new BinderMappingInfo(
                    new PythonInvokeBinder(
                        state,
                        new CallSignature(argCount)
                    ),
                    args
                ),
                new BinderMappingInfo(
                    new ConversionBinder(state, retType, ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional splatting.
        /// 
        /// The signature of the target site should be object(function), object[], retType
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static PythonInvokeBinder/*!*/ InvokeSplat(BinderState/*!*/ state) {
            return new PythonInvokeBinder(
                state,
                new CallSignature(new Argument(ArgumentType.List))
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional and keyword splatting.
        /// 
        /// The signature of the target site should be object(function), object[], dictionary, retType
        /// </summary>
        public static PythonInvokeBinder/*!*/ InvokeKeywords(BinderState/*!*/ state) {
            return new PythonInvokeBinder(
                state,
                new CallSignature(new Argument(ArgumentType.List), new Argument(ArgumentType.Dictionary))
            );
        }

        internal static Expression GetIndex(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonGetIndexBinder(
                    binderState,
                    expression.Length
                ),
                type,
                expression
            );
        }

        internal static Expression GetSlice(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonGetSliceBinder(binderState),
                type,
                expression
            );
        }

        internal static Expression SetIndex(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonSetIndexBinder(
                    binderState,
                    expression.Length - 1
                ),
                type,
                expression
            );
        }

        internal static Expression SetSlice(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonSetSliceBinder(binderState),
                type,
                expression
            );
        }

        internal static Expression DeleteIndex(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonDeleteIndexBinder(
                    binderState,
                    expression.Length
                ),
                type,
                expression
            );
        }

        internal static Expression DeleteSlice(BinderState binderState, Type type, Expression[] expression) {
            return Ast.Dynamic(
                new PythonDeleteSliceBinder(binderState),
                type,
                expression
            );
        }

    }
}
